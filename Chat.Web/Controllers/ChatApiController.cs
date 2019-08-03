﻿using Chat.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private ChatterersDbContext chatterersDb;
        public ChatApiController(ChatterersDbContext chaDb)
        {
            chatterersDb = chaDb;
        }
        [HttpGet]
        [Produces("application/xml")]
        public ContentResult ApiList()
        {
            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<api>" +
                "<option>" +
                "<name usage='/api/reg'>reg</name>" +
                "<description>registration request</description>" +
                "<accepts type='json' method='post'>" +
                "<request>" +
                "<name type='string' min='3' max='256'/>" +
                "<email type='string' min='6' max='256'/>" +
                "<password type='string' min='8' max='32'/>" +
                "</request>" +
                "</accepts>" +
                "<returns>" +
                "<success type='text/plain'>\"pending_{email}\"</success>" +
                "<failure type='text/plain'>\"server_failed\", \"email_is_taken\", \"name_is_taken\", \"{exception}\"</failure>" +
                "</returns>" +
                "</option>" +
                "<option>" +
                "<name usage='/api/val'>val</name>" +
                "<description>email address validation and registration completion</description>" +
                "<accepts type='json' method='post'>" +
                "<code type='Int64'/>" +
                "</accepts>" +
                "<returns>" +
                "<success type='text/plain'>\"added_{email}\"</success>" +
                "<failure type='text/plain'>\"server_failed\", \"email_is_taken\", \"name_is_taken\", \"invalid_confirmation_id\", \"register_request_required\", \"{exception}\"</failure>" +
                "</returns>" +
                "</option>" +
                "</api>", "application/xml", Encoding.UTF8);
        }
        [HttpPost("reg")]
        [Produces("text/plain")]
        public async Task<ContentResult> RequestRegister(RegRequest request)
        {
            string ret = "server_failed";
            try
            {
                if (chatterersDb.Chatterers.Any(c => c.Email == request.Email))
                    ret = "email_is_taken";
                else if (chatterersDb.Chatterers.Any(c => c.Name == request.Name))
                    ret = "name_is_taken";
                else
                {
                    Random rand = new Random();
                    string path = Path.Join(Startup.RootPath, "RegFiles");
                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(path);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Directory.CreateDirectory(path);
                        files = Directory.GetFiles(path);
                    }
                    string fileName = rand.Next(1234, 9876).ToString();
                    if (files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            string iName = Path.GetFileNameWithoutExtension(files[i]);
                            if (iName.Substring(iName.Length - 4) == fileName)
                            {
                                fileName = rand.Next(1234, 9876).ToString();
                                i = -1;
                            }
                        }
                    }
                    using (StreamWriter writer = new StreamWriter($"{path}{Path.DirectorySeparatorChar}{DateTime.UtcNow.Ticks}_{fileName}.json"))
                        writer.Write(JsonConvert.SerializeObject(request));
                    await SendMail(request.Email, request.Name, fileName);
                    ret = "pending_" + request.Email;
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("val")]
        [Produces("text/plain")]
        public async Task<ContentResult> Validate(object _id)
        {
            string ret = "server_failed";
            try
            {
                long id = (long)_id;
                if (id < 1234 || id > 9876)
                    throw new InvalidDataException("invalid_confirmation_id");
                string fileId = id.ToString();
                RegRequest request = null;
                string path = Path.Join(Startup.RootPath, "RegFiles");
                string[] files;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch
                {
                    throw new InvalidOperationException("register_request_required");
                }
                if(files.Length <= 0)
                {
                    ret = "register_request_required";
                }
                else
                {
                    foreach(var file in files)
                    {
                        string[] fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                        if (DateTime.UtcNow.Ticks - long.Parse(fileName[0]) > TimeSpan.FromHours(1).Ticks)
                            System.IO.File.Delete(file);
                        else
                        {
                            if(fileName[1] == fileId)
                            {
                                using (StreamReader reader = new StreamReader(file))
                                {
                                    request = JsonConvert.DeserializeObject<RegRequest>(reader.ReadToEnd());
                                }
                                System.IO.File.Delete(file);
                            }
                        }
                    }
                    if (request == null)
                        ret = "register_request_required";
                    else
                    {
                        if (chatterersDb.Chatterers.Any(c => c.Email == request.Email))
                            ret = "email_is_taken";
                        else if (chatterersDb.Chatterers.Any(c => c.Name == request.Name))
                            ret = "name_is_taken";
                        else
                        {
                            await chatterersDb.Chatterers.AddAsync(new ChatterersDbContext.Chatterer
                            {
                                Name = request.Name,
                                Email = request.Email,
                                Password = request.Password,
                                LastActive = DateTime.UtcNow.Ticks
                            });
                            await chatterersDb.SaveChangesAsync();
                            ret = "added_" + request.Email;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpGet("sign")] // NO GET (not encrypted)
        //[Produces("text/plain")]
        public string Html()
        {
            //implement signing in
            return "simple";
        }
        private async Task<string> SendMail(string to, string name, string code)
        {
            string mailContent, ret;
            using(StreamReader reader = new StreamReader(Startup.RootPath + "/Assets/mailconfirm.html"))
            {
                mailContent = await reader.ReadToEndAsync();
            }
            mailContent = mailContent.Insert(mailContent.IndexOf("</strong>"), name);
            mailContent = mailContent.Insert(mailContent.IndexOf("</span>"), code);
            try
            {
                using (SmtpClient client = new SmtpClient("smtp.live.com"))
                {
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential("splendiferouslife@outlook.com", "adm3.1415");
                    MailMessage message = new MailMessage("splendiferouslife@outlook.com", to, "CHAT Registration Confirmation", mailContent);
                    message.IsBodyHtml = true;
                    message.BodyEncoding = Encoding.UTF8;
                    await client.SendMailAsync(message);
                }
                ret = "ok";
            }
            catch (Exception e)
            {
                ret = e.Message;
            }
            return ret;
        }
    }
}

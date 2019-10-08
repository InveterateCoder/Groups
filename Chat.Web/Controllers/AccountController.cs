using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private User _user;
        public AccountController(User user)
            => _user = user;
        [HttpPost("reg")]
        public async Task<ContentResult> RequestRegister([FromBody]RegRequest request)
        {
            string ret;
            if (_user.Chatterer != null)
            {
                Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                ret = "signed_out";
            }
            else
            {
                try
                {
                    request.Email = request.Email.ToLower();
                    if (_user.Chatterers.Any(c => c.Email == request.Email))
                        ret = "email_is_taken";
                    else if (!StaticData.IsNameValid(request.Name))
                        ret = "invalid_name";
                    else if (_user.Chatterers.Any(c => c.Name == request.Name))
                        ret = "name_is_taken";
                    else
                    {
                        Random rand = new Random();
                        string path = Path.Join(StaticData.RootPath, "RegFiles");
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
                        await Task.WhenAll(CleanInactiveUsers(), SendMail(request.Email, request.Name, fileName));
                        ret = "pending_" + request.Email;
                    }
                }
                catch (Exception e)
                {
                    ret = e.Message;
                }
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("val")]
        public async Task<ContentResult> Validate([Required, FromBody]object _id)
        {
            string ret;
            if (_user.Chatterer != null)
            {
                Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                ret = "signed_out";
            }
            else
            {
                try
                {
                    long id;
                    string ids = _id as string;
                    if (ids != null)
                        id = long.Parse(ids);
                    else
                        id = (long)_id;
                    if (id < 1234 || id > 9876)
                        throw new InvalidDataException("invalid_confirmation_id");
                    string fileId = id.ToString();
                    RegRequest request = null;
                    string path = Path.Join(StaticData.RootPath, "RegFiles");
                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(path);
                    }
                    catch
                    {
                        throw new InvalidOperationException("reg_request_required");
                    }
                    if (files.Length <= 0)
                    {
                        ret = "reg_request_required";
                    }
                    else
                    {
                        foreach (var file in files)
                        {
                            string[] fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                            if (DateTime.UtcNow.Ticks - long.Parse(fileName[0]) > TimeSpan.FromHours(1).Ticks)
                                System.IO.File.Delete(file);
                            else
                            {
                                if (fileName[1] == fileId)
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
                            ret = "reg_request_required";
                        else
                        {
                            if (_user.Chatterers.Any(c => c.Email == request.Email))
                                ret = "email_is_taken";
                            else if (_user.Chatterers.Any(c => c.Name == request.Name))
                                ret = "name_is_taken";
                            else
                            {
                                await _user.Chatterers.AddAsync(new ChatterersDb.Chatterer
                                {
                                    Name = request.Name,
                                    Email = request.Email,
                                    Password = request.Password,
                                });
                                await _user.SaveAsync();
                                ret = "added_" + request.Email;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ret = e.Message;
                }
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("sign")]
        public async Task<ContentResult> SignIn([FromBody]SignRequest request)
        {
            string ret;
            try
            {
                if (_user.Chatterer != null)
                {
                    Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    ret = "signed_out";
                }
                else
                {
                    var user = _user.Chatterers.Where(c => c.Email == request.Email.ToLower()).SingleOrDefault();
                    if (user == null)
                        ret = "user_not_found";
                    else
                    {
                        if (user.Password != request.Password)
                            ret = "password_incorrect";
                        else
                        {
                            if (user.ConnectionId != null)
                                ret = "multiple_signins_forbidden";
                            else
                            {
                                SHA256 sha256 = SHA256.Create();
                                byte[] bytes;
                                string token;
                                do
                                {
                                    bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(
                                    $"{DateTime.UtcNow.ToString()}-{user.Name}-{user.Email}-{user.Password}-{Guid.NewGuid().ToString()}"));
                                    token = Convert.ToBase64String(bytes);
                                }
                                while (_user.Chatterers.Where(c => c.Token == token).FirstOrDefault() != null);
                                user.Token = token;
                                sha256.Dispose();
                                bytes = null;
                                await _user.SaveAsync();
                                HttpContext.Response.Cookies.Append(StaticData.AuthenticationCookieName, user.Token,
                                    new CookieOptions { Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.Now.AddDays(30) });
                                ret = "OK";
                            }
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
        private async Task CleanInactiveUsers()
        {
            var limit = DateTime.UtcNow.Subtract(TimeSpan.FromDays(300)).Ticks;
            var inactiveUsrs = _user.Chatterers.Where(c => c.LastActive < limit);
            var groupIds = inactiveUsrs.Where(c => c.Group != null).Select(u => u.Id);
            foreach (var id in groupIds)
            {
                var inGroupUsrs = _user.Chatterers.Where(c => c.InGroupId == id);
                foreach(var usr in inGroupUsrs)
                {
                    usr.InGroupId = 0;
                    usr.InGroupPassword = null;
                }
                _user.Database.Messages.RemoveRange(_user.Database.Messages.Where(m => m.GroupId == id));
            }
            _user.Chatterers.RemoveRange(inactiveUsrs);
            await _user.SaveAsync();
        }
        private async Task<string> SendMail(string to, string name, string code)
        {
            string mailContent, ret;
            using(StreamReader reader = new StreamReader($"{StaticData.RootPath}{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}mailconfirm.html"))
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
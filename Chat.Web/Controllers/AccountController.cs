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
        private ChatterersDb _dbContext;
        private User _user;
        public AccountController(ChatterersDb chaDb, User user)
        {
            _dbContext = chaDb;
            _user = user;
        }
        [HttpPost("reg")]
        public async Task<ContentResult> RequestRegister([FromBody]RegRequest request)
        {
            string ret;
            if (_user.Name != null)
            {
                Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                ret = "signed_out";
            }
            else
            {
                try
                {
                    request.Email = request.Email.ToLower();
                    if (_dbContext.Chatterers.Any(c => c.Email == request.Email))
                        ret = "email_is_taken";
                    else if (_dbContext.Chatterers.Any(c => c.Name == request.Name))
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
                        await SendMail(request.Email, request.Name, fileName);
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
            if (_user.Name != null)
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
                            if (_dbContext.Chatterers.Any(c => c.Email == request.Email))
                                ret = "email_is_taken";
                            else if (_dbContext.Chatterers.Any(c => c.Name == request.Name))
                                ret = "name_is_taken";
                            else
                            {
                                await _dbContext.Chatterers.AddAsync(new ChatterersDb.Chatterer
                                {
                                    Name = request.Name,
                                    Email = request.Email,
                                    Password = request.Password,
                                    LastActive = DateTime.UtcNow.Ticks
                                });
                                await _dbContext.SaveChangesAsync();
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
        public async Task<ContentResult> SignIn([FromBody]SignRequest request, [FromServices]ActiveUsers activeUsers)
        {
            string ret;
            try
            {
                if (_user.Name != null)
                {
                    Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    ret = "signed_out";
                }
                else
                {
                    var user = _dbContext.Chatterers.Where(c => c.Email == request.Email.ToLower()).SingleOrDefault();
                    if (user == null)
                        ret = "user_not_found";
                    else
                    {
                        if (user.Password != request.Password)
                            ret = "password_incorrect";
                        else
                        {
                            if (activeUsers.Users.Any(u => u.Name == user.Name))
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
                                while (_dbContext.Chatterers.Where(c => c.Token == token).FirstOrDefault() != null);
                                user.Token = token;
                                user.LastActive = DateTime.UtcNow.Ticks;
                                sha256.Dispose();
                                bytes = null;
                                await _dbContext.SaveChangesAsync();
                                HttpContext.Response.Cookies.Append(StaticData.AuthenticationCookieName, user.Token,
                                    new CookieOptions { Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.Now.AddDays(30) });
                                ret = $"OK_{user.Name.Length}_{user.Name}{user.Group ?? string.Empty}";
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
        [HttpPost("user/signout")]
        public async Task<ContentResult> SignOut()
        {
            string ret;
            try
            {
                var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                chatterer.Token = null;
                chatterer.InGroup = null;
                chatterer.InGroupPassword = null;
                await _dbContext.SaveChangesAsync();
                HttpContext.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                ret = "OK";
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("user/del")]
        public async Task<ContentResult> Delete([FromBody]SignRequest request)
        {
            string ret;
            try
            {
                var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                if (request.Email.ToLower() != chatterer.Email)
                    ret = "wrong_email";
                else if (request.Password != chatterer.Password)
                    ret = "wrong_password";
                else
                {
                    Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    _dbContext.Chatterers.Remove(chatterer);
                    await _dbContext.SaveChangesAsync();
                    ret = "deleted";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("user/change")]
        public async Task<ContentResult>Change([FromBody]AccountChangeRequest request)
        {
            string ret;
            try
            {
                var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                if (request.Password != chatterer.Password)
                    ret = "wrong_password";
                else if (request.NewName == null && request.NewPassword == null)
                    ret = "no_change_requested";
                else
                {

                    if (request.NewName != null && request.NewName != chatterer.Name
                        && request.NewPassword != null && request.NewPassword != chatterer.Password)
                    {
                        chatterer.Name = request.NewName;
                        chatterer.Password = request.NewPassword;
                        await _dbContext.SaveChangesAsync();
                        ret = "name&pass_changed";
                    }
                    else if (request.NewName != null && request.NewName != chatterer.Name)
                    {
                        chatterer.Name = request.NewName;
                        await _dbContext.SaveChangesAsync();
                        ret = "name_changed";
                    }
                    else if (request.NewPassword != null && request.NewPassword != chatterer.Password)
                    {
                        chatterer.Password = request.NewPassword;
                        await _dbContext.SaveChangesAsync();
                        ret = "pass_changed";
                    }
                    else
                        ret = "same_credentials";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
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
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
        private string email = "<!DOCTYPE html><html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta charset=\"utf-8\"> <meta name=\"viewport\" content=\"width=device-width\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta name=\"format-detection\" content=\"telephone=no,address=no,email=no,date=no,url=no\"> <title>CHAT email confirmation</title> <style> html, body { margin: 0 !important; padding: 0 !important; height: 100% !important; width: 100% !important; } * { -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; } table, td { mso-table-lspace: 0pt !important; mso-table-rspace: 0pt !important; } table { border-spacing: 0 !important; border-collapse: collapse !important; table-layout: fixed !important; margin: 0 auto !important; } @media only screen and (min-device-width: 320px) and (max-device-width: 374px) { u ~ div .email-container { min-width: 320px !important; } } @media only screen and (min-device-width: 375px) and (max-device-width: 413px) { u ~ div .email-container { min-width: 375px !important; } } @media only screen and (min-device-width: 414px) { u ~ div .email-container { min-width: 414px !important; } } @media screen and (max-width: 600px) { .email-container p { font-size: 17px !important; } } </style></head><body width=\"100%\" style=\"margin: 0; padding: 0 !important; mso-line-height-rule: exactly;\"><center style=\"width: 100%;\"> <div style=\"max-width: 600px; margin: 0 auto;\" class=\"email-container\"> <table align=\"center\" role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" width=\"100%\" style=\"margin: auto;\"> <tr> <td> <table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" width=\"100%\"> <tr> <td style=\"padding: 20px; font-family: sans-serif; font-size: 15px; line-height: 20px; color: #555555;\"> <h1 style=\"margin: 0 0 10px 0; font-family: sans-serif; font-size: 25px; line-height: 30px; color: #333333; font-weight: normal;\">Please confirm this email address.</h1> <p style=\"margin: 0;\">Hello there, <strong></strong>. This is an automatically generated message. The following is the confirmation number:</p> </td> </tr> <tr> <td style=\"padding: 0 20px;\"> <table align=\"center\" role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"margin: auto;\"> <tr> <td class=\"button-td button-td-primary\" style=\"border-radius: 4px; background: #222222;\"> <span style=\"letter-spacing: 3px; background: #222222; border: 1px solid #000000; font-family: sans-serif; font-size: 15px; line-height: 15px; padding: 13px 17px; color: #ffffff; display: block; border-radius: 4px;\"></span></td> </tr> </table> </td> </tr> <tr> <td style=\"padding: 20px; font-family: sans-serif; font-size: 15px; line-height: 20px; color: #555555;\"> <h2 style=\"margin: 0 0 10px 0; font-family: sans-serif; font-size: 18px; line-height: 22px; color: #333333; font-weight: bold;\">The confirmation number will expire in one hour.</h2> <hr><h1 style=\"margin: 15px 0 0 0; text-align: center;\">Groups</h1> </td> </tr> </table> </td> </tr> </table> </div> </center></body></html>";
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
                        int code;
                        do code = rand.Next(1234, 9876);
                        while (_user.Database.RegRequests.FirstOrDefault(r => r.Code == code) != null);
                        _user.Database.RegRequests.Add(new ChatterersDb.RegData
                        {
                            Code = code,
                            RequestTime = DateTime.UtcNow.Ticks,
                            Name = request.Name,
                            Email = request.Email,
                            Password = request.Password
                        });
                        await _user.SaveAsync();
                        await CleanInactiveUsers();
                        var mret = await SendMail(request.Email, request.Name, code.ToString());
                        if (mret != "ok") ret = mret;
                        else ret = "pending_" + request.Email;
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
                    long idl;
                    string ids = _id as string;
                    if (ids != null)
                        idl = long.Parse(ids);
                    else
                        idl = (long)_id;
                    if (idl < 1234 || idl > 9876)
                        throw new InvalidDataException("invalid_confirmation_id");
                    int id = (int)idl;
                    var overdueReq = _user.Database.RegRequests.Where(r => r.RequestTime < DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks);
                    var mustSave = false;
                    if (overdueReq.Count() > 0)
                    {
                        _user.Database.RegRequests.RemoveRange(overdueReq);
                        mustSave = true;
                    }
                    var reqReq = _user.Database.RegRequests.FirstOrDefault(r => r.Code == id);
                    if(reqReq == null)
                    {
                        ret = "reg_request_required";
                        if (mustSave)
                            await _user.SaveAsync();
                    }
                    else
                    {
                        if (_user.Chatterers.Any(c => c.Email == reqReq.Email))
                        {
                            ret = "email_is_taken";
                            if (mustSave)
                                await _user.SaveAsync();
                        }
                        else if(_user.Chatterers.Any(c => c.Name == reqReq.Name))
                        {
                            ret = "name_is_taken";
                            if (mustSave)
                                await _user.SaveAsync();
                        }
                        else
                        {
                            await _user.Chatterers.AddAsync(new ChatterersDb.Chatterer
                            {
                                Name = reqReq.Name,
                                Email = reqReq.Email,
                                Password = reqReq.Password,
                            });
                            ret = "added_" + reqReq.Email;
                            _user.Database.RegRequests.Remove(reqReq);
                            await _user.SaveAsync();
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
            if (inactiveUsrs.Count() > 0)
            {
                var groupIds = inactiveUsrs.Where(c => c.Group != null).Select(u => u.Id);
                if (groupIds.Count() > 0)
                {
                    foreach (var id in groupIds)
                    {
                        var inGroupUsrs = _user.Chatterers.Where(c => c.InGroupId == id);
                        foreach (var usr in inGroupUsrs)
                        {
                            usr.InGroupId = 0;
                            usr.InGroupPassword = null;
                        }
                        _user.Database.Messages.RemoveRange(_user.Database.Messages.Where(m => m.GroupId == id));
                    }
                }
                _user.Chatterers.RemoveRange(inactiveUsrs);
                await _user.SaveAsync();
            }
        }
        private async Task<string> SendMail(string to, string name, string code)
        {
            string mailContent = email, ret;
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
                    MailMessage message = new MailMessage("splendiferouslife@outlook.com", to, "Groups Registration Confirmation", mailContent);
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
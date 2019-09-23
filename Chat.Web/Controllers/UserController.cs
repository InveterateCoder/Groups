using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chat.Web.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private User _user;
        public UserController(User user)
            => _user = user;
        [HttpGet("info")]
        public async Task<JsonResult> Info() => new JsonResult(new
        {
            name = _user.Name,
            group = _user.Group,
            ingroup = (await _user.Chatterers.FindAsync(_user.InGroupId))?.Group,
            pub_key = "BFnbEjZPGFowzLKbDeFjlJ-o5juCQWiaFUzDH6jb_H3Rid3EO8f59N8PSe5AAMp5KhLMV31u1V79RxBiAmeofH0"
        });
        [HttpPost("signout")]
        public async Task<ContentResult> SignOut()
        {
            string ret;
            try
            {
                _user.Token = null;
                _user.InGroupId = 0;
                _user.InGroupPassword = null;
                await _user.SaveAsync();
                HttpContext.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                ret = "OK";
            }
            catch (Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("del")]
        public async Task<ContentResult> Delete([FromBody]SignRequest request)
        {
            string ret;
            try
            {
                if (request.Email.ToLower() != _user.Email)
                    ret = "wrong_email";
                else if (request.Password != _user.Password)
                    ret = "wrong_password";
                else
                {
                    Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    _user.Chatterers.Remove(_user.Chatterer);
                    await _user.SaveAsync();
                    ret = "deleted";
                }
            }
            catch (Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("change")]
        public async Task<ContentResult> Change([FromBody]AccountChangeRequest request)
        {
            string ret;
            try
            {
                if (request.Password != _user.Password)
                    ret = "wrong_password";
                else if (request.NewName == null && request.NewPassword == null)
                    ret = "no_change_requested";
                else
                {
                    if (request.NewName != null && request.NewName != _user.Name
                        && request.NewPassword != null && request.NewPassword != _user.Password)
                    {
                        if (_user.Chatterers.Any(c => c.Name == request.NewName))
                            ret = "name_exists";
                        else
                        {
                            _user.Name = request.NewName;
                            _user.Password = request.NewPassword;
                            await _user.SaveAsync();
                            ret = "name&pass_changed";
                        }
                    }
                    else if (request.NewName != null && request.NewName != _user.Name)
                    {
                        if (_user.Chatterers.Any(c => c.Name == request.NewName))
                            ret = "name_exists";
                        else
                        {
                            _user.Name = request.NewName;
                            await _user.SaveAsync();
                            ret = "name_changed";
                        }
                    }
                    else if (request.NewPassword != null && request.NewPassword != _user.Password)
                    {
                        _user.Password = request.NewPassword;
                        await _user.SaveAsync();
                        ret = "pass_changed";
                    }
                    else
                        ret = "same_credentials";
                }
            }
            catch (Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
    }
}

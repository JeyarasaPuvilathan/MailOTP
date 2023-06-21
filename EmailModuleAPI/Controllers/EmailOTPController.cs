using EmailModuleAPI.DBContext;
using EmailModuleAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailModuleAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailOTPController : ControllerBase
    {
        private readonly IEmailOTP _emailOTP;

        public EmailOTPController(IEmailOTP emailOTP)
        {
            _emailOTP = emailOTP;
        }

        [HttpPost]
        [Route("check-OTP")]
        public async Task<IActionResult> PostAsyncCheckOTPl(string otp, string email)
        {
            var result = await _emailOTP.CheckOTP(otp, email);
            if(result.IsOTPValid)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost]
        [Route("check-mail-valid and send OTP to mail")]
        public async Task<IActionResult> PostAsyncCheckEmailValid(string email)
        {
            var result = await _emailOTP.checkEmailValid(email);
            if (result.IsEmailValid)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}

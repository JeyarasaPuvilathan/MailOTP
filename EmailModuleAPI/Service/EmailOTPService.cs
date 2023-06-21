using EmailModuleAPI.Controllers;
using EmailModuleAPI.DBContext;
using EmailModuleAPI.Entities;
using EmailModuleAPI.Enum;
using EmailModuleAPI.Interfaces;
using EmailModuleAPI.Model;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EmailModuleAPI.Service
{
    public class EmailOTPService : IEmailOTP
    {
        private readonly ILogger<EmailOTPService> _logger;
        private readonly EmailDbContext _emailDbContext;
        private readonly IConfiguration _configuration;

        public EmailOTPService(ILogger<EmailOTPService> logger, EmailDbContext emailDbContext, IConfiguration configuration)
        {
            _logger = logger;
            _emailDbContext = emailDbContext; // test
            _configuration = configuration;
        }

        // service for check OTP
        public async Task<OtpValid> CheckOTP(string otp, string email)
        {
            OtpValid otpValid = new OtpValid();
            otpValid.OTP = otp;
            try
            {
                
                // get OTP information from using mail
                UserOtp otpInfo = await GetOTPInfoByMail(email);

                // check weather otpInfo is null or not
                if (otpInfo != null)
                {
                    // check input value with OTP
                    if (otpInfo.Otp_pass == otp) // time
                    {
                        // validate OTP if enter  1 min
                        if ((DateTime.Now - otpInfo.Otp_timestamp).TotalSeconds <= 60.00) // try
                        {
                            otpInfo.Otp_tries += 1;
                            _emailDbContext.userOtp.Update(otpInfo);
                            await _emailDbContext.SaveChangesAsync();

                            // validate the OTP after 10 try
                            if (otpInfo.Otp_tries < 10)
                            {
                                otpValid.IsOTPValid = true;
                                otpValid.statusCode = StatusCode.STATUS_OTP_OK;
                            }
                            else
                            {
                                otpValid.IsOTPValid = false;
                                otpValid.statusCode = StatusCode.STATUS_OTP_FAIL;
                            }
                        }
                        else
                        {
                            otpValid.IsOTPValid = false;
                            otpValid.statusCode = StatusCode.STATUS_OTP_TIMEOUT;
                        }

                    }
                    else
                    {
                        otpValid.IsOTPValid = false;
                        otpValid.statusCode = StatusCode.STATUS_EMAIL_INVALID;
                    }

                }
                else
                {
                    otpValid.IsOTPValid = false;
                    otpValid.statusCode = StatusCode.STATUS_OTP_FAIL;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went to wrong: {ex}");
                otpValid.IsOTPValid = false;
                otpValid.statusCode = StatusCode.STATUS_OTP_FAIL;

            }

            return otpValid;
        }

       
        // email validation function
        public async Task<EmailValid> checkEmailValid(string email)
        {
            EmailValid emailValid = new EmailValid();
            emailValid.Email = email;
            try
            {
                try
                {
                    // email domain validation part
                    emailValid.IsEmailValid = ValidateEmail(email);
                    if(!emailValid.IsEmailValid ) {
                        emailValid.IsEmailValid = false;
                        emailValid.statusCode = StatusCode.STATUS_EMAIL_INVALID;

                        return emailValid;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Something went to wrong: {ex}");
                    emailValid.IsEmailValid = false;
                    emailValid.statusCode = StatusCode.STATUS_EMAIL_INVALID;

                    return emailValid;

                }

                // getting the user infomation
                var user = _emailDbContext.User.Where(e => e.Email == email).FirstOrDefault();

                // validate ther user is exsit or not and email is validate
                if (user != null && emailValid.IsEmailValid)
                {
                    
                  if( await generateOTPEmail(email))
                    {
                        emailValid.IsEmailValid = true;
                        emailValid.statusCode = StatusCode.STATUS_EMAIL_OK;
                    }
                  else
                    {
                        emailValid.IsEmailValid = false;
                        emailValid.statusCode = StatusCode.STATUS_EMAIL_FAIL;
                    }
                }
                else
                {
                    emailValid.IsEmailValid = false;
                    emailValid.statusCode = StatusCode.STATUS_EMAIL_FAIL;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went to wrong: {ex}");
                emailValid.IsEmailValid = false;
                emailValid.statusCode = StatusCode.STATUS_EMAIL_INVALID;
            }

            return emailValid;
        }

        private bool ValidateEmail(string email)
        {
            // email domain validation part
            var mailAddress = new MailAddress(email);
            if (!(mailAddress.Host.ToLower() == "dso.org.sg"))
            {
                return false;
            }

            // Disallowed special characters
            string disallowedCharsPattern = @"[!#$%^&*()+=\[\]{}|\\<>\/]";
            if (Regex.IsMatch(email, disallowedCharsPattern))
            {
                return false;
            }

            // Coding issues such as string comparison using .contains()
            string codingIssuesPattern = @"(?i)\b(?:contains|indexOf)\b";
            if (Regex.IsMatch(email, codingIssuesPattern))
            {
                return false;
            }

            // Max length of email address
            int maxLength = 255; // Maximum length for email addresses
            if (email.Length > maxLength)
            {
                return false;
            }

            // Regular expression pattern for email validation
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Validate the email address against the pattern
            return Regex.IsMatch(email, emailPattern);
        }

        // function for generate the OTP
        private async Task<bool> generateOTPEmail(string email)
        {
            try
            {
                string otpPass;

                // generate the 6 digit random number
                // Define a string containing all valid digits
                string validDigits = "0123456789";

                // Create a new instance of the Random class
                Random random = new Random();

                // Generate a random 6-digit number with leading zeros
                otpPass = new string(Enumerable.Range(0, 6)
                                              .Select(_ => validDigits[random.Next(validDigits.Length)])
                                              .ToArray());

                // getting OTP information using email if already exsit
                UserOtp userOtpinfo = await GetOTPInfoByMail(email);
                string emailBody = $"Your OTP Code is {otpPass}. The code is valid for 1 minute.";

                // call the function for send the OTP to Mail
                bool isSendOTP = SendEmail(email, emailBody);

                // check weather OTP information already exist or not and check OTP send successfully
                // update the UserOTP table if data already exsit
                if (userOtpinfo != null && isSendOTP)
                {
                    userOtpinfo.Otp_pass = otpPass;
                    userOtpinfo.Otp_timestamp = DateTime.Now;
                    userOtpinfo.Otp_tries = 0;
                    _emailDbContext.userOtp.Update(userOtpinfo);
                    await _emailDbContext.SaveChangesAsync();
                }
                else
                {
                    UserOtp userOtpNew = new UserOtp()
                    {
                        Otp_pass = otpPass,
                        Otp_tries = 0
                    };

                    // insert the data to userOTP table for first time
                    _emailDbContext.userOtp.Add(userOtpNew);
                    await _emailDbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");

                return false;

            }

            return true;
        }

        private bool SendEmail(string userEmail, string emailBody)
        {
            try
            {
                // Replace with your SMTP server and email
                string smtpServer = _configuration.GetValue<string>("SmtpConfig:smtpServer");
                int smtpPort = _configuration.GetValue<int>("SmtpConfig:smtpPort");

                // Create a new instance of the SmtpClient
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    // Set the mailMessage, Subject and Body
                    MailMessage mailMessage = new MailMessage("noreply@example.com", userEmail);
                    mailMessage.Subject = "OTP Code";
                    mailMessage.Body = emailBody;

                    // send the mail
                    smtpClient.Send(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in send the mail: {ex}");

                return false;
            }
        }

        private async Task<UserOtp> GetOTPInfoByMail(string email)
        {
            try
            {


                var userOtpinfo =  (from user in _emailDbContext.User
                                                join userOtp in _emailDbContext.userOtp
                                                    on user.UserId equals userOtp.UserId
                                                select new
                                                {
                                                    UserId = userOtp.UserId,
                                                    email = user.Email,
                                                    OtpPass = userOtp.Otp_pass,
                                                    OtpTimestamp = userOtp.Otp_timestamp,
                                                    OtpTries = userOtp.Otp_tries

                                                }).Where(x => x.email == email).Select(x => new UserOtp()
                                                {
                                                    UserId = x.UserId,
                                                    Otp_pass = x.OtpPass,
                                                    Otp_timestamp = x.OtpTimestamp,
                                                    Otp_tries = x.OtpTries
                                                }).FirstOrDefault();

                if (userOtpinfo != null) {
                    UserOtp userOtpResult = new UserOtp()
                    {
                        UserId = userOtpinfo.UserId,
                        Otp_pass = userOtpinfo.Otp_pass,
                        Otp_timestamp = userOtpinfo.Otp_timestamp,
                        Otp_tries = userOtpinfo.Otp_tries
                    };
                    return userOtpResult;
                }
               

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went to  wrong: {ex}");           
            }

            return null;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WM.Register.DAO;
using WM.Register.MD5;
using WM.Register.Models;

namespace WM.Register.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        //[Authorize]
        public ActionResult Login(srv_VNOGateWay_Merchant srv_VNOGateWay_Merchant, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    var logined = new RegisterDbContext().Database.SqlQuery<srv_VNOGateWay_Merchant>("exec [dbo].[Login] @email, @Password", new SqlParameter("email", srv_VNOGateWay_Merchant.merchant_email), new SqlParameter("Password", Common.MD5Hash(srv_VNOGateWay_Merchant.password))).FirstOrDefault();
                    if (logined != null)
                    {
                        FormsAuthentication.SetAuthCookie(srv_VNOGateWay_Merchant.merchant_email, false);

                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, srv_VNOGateWay_Merchant.merchant_email, DateTime.Now, DateTime.Now.AddMinutes(30), false, JsonConvert.SerializeObject(logined));
                        string encryptForm = FormsAuthentication.Encrypt(ticket);
                        HttpCookie httpCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptForm);
                        Response.Cookies.Add(httpCookie);
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                //return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
            }
            return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return View();
            }
        }
        public ActionResult ChangePass()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult ChangePass(string newPass, string oldPass/*, srv_VNOGateWay_Merchant srv_VNOGateWay_Merchant*/)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Common.MD5Hash(oldPass) == ((User as WebIPrincipal).User.password))
                {
                    var change = new RegisterDbContext().Database.SqlQuery<srv_VNOGateWay_Merchant>("exec [dbo].[ChangePasswordMerchant] @id, @NewPassword, @OldPass", new SqlParameter("id", (User as WebIPrincipal).User.id), new SqlParameter("NewPassword", Common.MD5Hash(newPass)), new SqlParameter("OldPass", Common.MD5Hash(oldPass))).FirstOrDefault();
                    FormsAuthentication.SignOut();
                    return Json(new { result = 1 });
                }
                else
                {
                    return Json("Wrong Password", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return RedirectToAction("Login", "Login");
            }

        }
        public ActionResult ForgotPassword()
        {


            return View();
        }

        [HttpPost]
        public JsonResult SendForgotPassword(string username, string email)
        {
            string resetTitle = "Password Reset";
            string resetTemplate = "Dear {fullName} <br><br><br>Your password has been successfully reset. <br> The new password is: {password} <br>Please change your password after the first log in.<br><br><br>Sincerely,<br>Administrator<br><i>Note: This is an auto-generated email, please do not reply.</i>";
            IQueryable<srv_VNOGateWay_Merchant> list = StoreProceduce.ListAllMerchant().AsQueryable();
            srv_VNOGateWay_Merchant account = list.Where(x => x.merchant_email.Equals(email)).FirstOrDefault();

            if (account != null)
            {
                try
                {
                    var password = account.password;// chỗ này giải mã md5 nhé chỗ này t chưa làm m làm nhé!
                    SendMail(resetTitle, resetTemplate.Replace("{password}", password).Replace("{fullName}", account.merchant_name), email, null);

                    return Json(new { message = "chúng tôi đã gửi password về mail của quý khách, vui lòng kiểm tra mail.", status = true });
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message, status = false });
                }
            }
            else
            {
                return Json(new { message = "mail người nhận không tồn tại!", status = false });
            }
        }

        private void SendMail(string title, string content, string email, string fileUlr)
        {
            try
            {
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string sender = smtpSection.From;
                string password = smtpSection.Network.Password;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(smtpSection.Network.Host);

                mail.From = new MailAddress(sender);
                mail.To.Add(email);
                mail.Subject = title;
                mail.Body = content;
                mail.IsBodyHtml = true;
                SmtpServer.UseDefaultCredentials = false;

                if (fileUlr != null)
                {
                    Attachment attachment;
                    attachment = new Attachment(fileUlr);
                    mail.Attachments.Add(attachment);
                }
                SmtpServer.Port = smtpSection.Network.Port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(sender, password);
                SmtpServer.EnableSsl = smtpSection.Network.EnableSsl;

                SmtpServer.Send(mail);
            }
            catch (SmtpException e)
            {
                throw new SmtpException(SmtpStatusCode.UserNotLocalTryAlternatePath, "mail người nhận không tồn tại!: " + e);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(srv_VNOGateWay_Merchant model)
        {
            Guid.NewGuid();
            return View(model);
        }
    }
}
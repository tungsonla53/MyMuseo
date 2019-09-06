using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MyMuseo.Models;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.Mime;
using System.Text;
using MyMuseo.DataService;
using System.Collections.Generic;

namespace MyMuseo.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            // var user = UserManager.Find(model.Email, model.Password);
            //
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    //MigrateShoppingCart(model.Email);
                    if (String.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("CollectorDetail", "Home", new { id = 0 });
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.TypeId = new SelectList(
               new List<SelectListItem>
               {
                    new SelectListItem {Text = "A Collector", Value = "0"},
                    new SelectListItem {Text = "An Artist or Craftsperson", Value = "1"},
                    new SelectListItem {Text = "An Artist/Craftsperson and collector", Value = "2"},
               }, "Value", "Text", "0");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);

                //var confirmationToken = Guid.NewGuid().ToString();
                //var user = new User { UserName = model.Email.TrimEnd(), Nickname = model.Nickname.TrimEnd(), IsConfirmed = true, ConfirmationToken = confirmationToken, CreatedDate = DateTime.UtcNow };
                //var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    CollectorRespository collectorRepository = new CollectorRespository();
                    Collector collector = new Collector();
                    collector.FirstName = model.FirstName;
                    collector.LastName = model.LastName;
                    collector.UserId = user.Id;
                    collector.ProfileImage = @"/Content/images/place-holder.png";
                    collector.PageImage = @"/Content/images/place-holder.png";
                    collector.CreatedDate = DateTime.Now;
                    collector.TypeId = model.TypeId;
                    collectorRepository.InsertCollector(collector);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("no-reply@mymuseo.com");
                    mailMessage.To.Add(new MailAddress(model.Email));
                    mailMessage.To.Add(new MailAddress("tsonla@yahoo.com"));
                    mailMessage.To.Add(new MailAddress("gjamroski@mac.com"));
                    mailMessage.Subject = "Welcome";
                    mailMessage.IsBodyHtml = true;
                    //
                    var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
                    String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
                    var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
                    String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));

                    TemplateModel content = _collectorRespository.GetTemplateByName("Welcome Email");
                    string mailTemplate = content.TemplateText;
                    string theMessage = String.Format(mailTemplate, image0, image1, callbackUrl);
                    //string theMessage = "<html><body><table width='600'><tr><td align='center'><img src='data:image/gif;base64," + image0 + "' /><br/><br/></td></tr><tr><td><img src='data:image/gif;base64," + image1 + "' /></td></tr><tr><td style='font-size: 18px;'>Thank you for registering with myMuseo!. To activate your account please click to link below. Once you account is activated you can create your profile, add collectibles and so much more. We are happy to have you as part of our community!<br/><br/></td></tr><tr><td align='right'><a href='" + callbackUrl + "'  style='background: #222222; border: 15px solid #222222; padding: 0 10px;color: #ffffff; font-family: sans-serif; font-size: 13px; line-height: 1.1; text-align: center; text-decoration: none; display: block; border-radius: 3px; font-weight: bold; width: 200px;'>Activate Your Account Now!</a></td></tr></table></body></html>";
                    mailMessage.Body = theMessage;
                    this.SendMail(mailMessage);
                    return RedirectToAction("Confirm", "Account", new { Email = user.Email });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Confirm(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var collector = _collectorRespository.GetCollectorByEmail(model.Email);
                if (collector == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                string userName = String.Format("{0} {1}", collector.FirstName, collector.LastName);

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("no-reply@mymuseo.com");
                mailMessage.To.Add(new MailAddress(model.Email));
                mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
                mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));

                mailMessage.Subject = "Reset Your Password";
                mailMessage.IsBodyHtml = true;
                var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
                String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
                var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
                String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
                TemplateModel content = _collectorRespository.GetTemplateByName("Password Reset Request");
                string mailTemplate = content.TemplateText;
                string theMessage = String.Format(mailTemplate, image0, image1, userName, callbackUrl);
                mailMessage.Body = theMessage;
                this.SendMail(mailMessage);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var collector = _collectorRespository.GetCollectorByEmail(model.Email);
            if (collector == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            string userName = String.Format("{0} {1}", collector.FirstName, collector.LastName);
            //
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("no-reply@mymuseo.com");
                mailMessage.To.Add(new MailAddress(model.Email));
                mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
                mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
                mailMessage.Subject = "Password Reset";
                mailMessage.IsBodyHtml = true;
                var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
                String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
                var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
                String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
                TemplateModel content = _collectorRespository.GetTemplateByName("Password Reset Confirmation");
                string mailTemplate = content.TemplateText;
                string theMessage = String.Format(mailTemplate, image0, image1, userName);
                mailMessage.Body = theMessage;
                this.SendMail(mailMessage);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private void MigrateShoppingCart(string UserName)
        {
            // Associate shopping cart items with logged-in user
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
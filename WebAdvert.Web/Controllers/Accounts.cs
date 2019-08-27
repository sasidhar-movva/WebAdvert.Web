using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class Accounts: Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _cognitoUserPool;

        public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager,
            CognitoUserPool cognitoUserPool)
        {
            this._cognitoUserPool = cognitoUserPool;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if(ModelState.IsValid)
            {
                var user = _cognitoUserPool.GetUser(signupModel.Email);

                if(user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User With this email already exists.");
                    return View(signupModel);
                }

                var createdUser = await _userManager.CreateAsync(user, signupModel.Password);

                if(createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            var model = new ConfirmEmailModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmEmailModel confirmEmailModel)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmEmailModel.Email);
                if(user == null)
                {
                    ModelState.AddModelError("NotFound", "User with the given email address is not found");
                    return View(confirmEmailModel);
                }

                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, confirmEmailModel.EmailVerificationCode, true);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return View(confirmEmailModel);
                }
            }

            return View(confirmEmailModel);
        }

        [HttpGet]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            return View(loginModel);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel loginModel)
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email,
                    loginModel.Password, loginModel.RememberMe, false);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Email and Password dont match");
                }
            }

            return View("Login", loginModel);
        }

    }
}

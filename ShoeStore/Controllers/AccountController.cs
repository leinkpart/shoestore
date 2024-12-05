using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShoeStore.Models;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.Security;

namespace ShoeStore.Controllers
{
    public class AccountController : Controller
    {
        ShoeStoreEntities data = new ShoeStoreEntities();
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user, FormCollection fc)
        {
            var userName = fc["userName"];
            var password = fc["password"];
            var email = fc["email"];
            var confirmPassword = fc["confirmPassword"];          

            // Kiểm tra email có tồn tại
            if (data.Users.Any(u => u.Email == email))
            {
                TempData["error2"] = "Email đã tồn tại!";
                return View();
            }

            // Kiểm tra mật khẩu có hợp lệ không
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            if (!Regex.IsMatch(password, passwordPattern))
            {
                TempData["error3"] = "Mật khẩu không hợp lệ";
                return View();
            }

            // Kiểm tra mật khẩu và xác nhận mật khẩu có khớp không
            if (password != confirmPassword)
            {
                TempData["error4"] = "Mật khẩu không khớp.";
                return View();
            }

            // Băm mật khẩu
            var hashedPassword = HashPassword(password);

            user.Name = userName;
            user.Email = email;
            user.PasswordHash = hashedPassword;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            data.Users.Add(user);
            data.SaveChanges();

            return RedirectToAction("Login");
        }

        // Đăng nhập
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var hashedPassword = HashPassword(password);

            User user = data.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                Session["Account"] = user;
                Session["UserName"] = user.Name;

                // Tạo cookie lưu thông tin đăng nhập
                HttpCookie cookie = new HttpCookie("DataLogin")
                {
                    Expires = DateTime.Now.AddDays(7)
                };

                cookie["Account"] = user.Email;
                cookie["name"] = HttpUtility.UrlEncode(user.Name);

                FormsAuthentication.SetAuthCookie(user.Email, true);

                var prePage = Session["PrePage"] != null ? Session["PrePage"].ToString() : Url.Action("Index", "Home");

                // Add cookie to the response
                Response.Cookies.Add(cookie);

                return Redirect(prePage);             
            }
            else
            {
                TempData["error"] = "Sai thông tin đăng nhập.";              
            }
            return View();
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            HttpCookie cookie = Request.Cookies["DataLogin"];
            if (cookie != null)
            {
                string mail = cookie["Account"];
                string fullName = cookie["name"];

                cookie.Expires = DateTime.Now.AddDays(-1);

                Response.Cookies.Add(cookie);
                // Use the cookie values as needed
            }
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        // Hàm băm mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
using _23dh111584_MyStore.Models.ViewModels;
using _23dh111584_MyStore.Models;
using System;
using System.Security.Cryptography;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;

namespace _23dh111584_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên người dùng đã tồn tại chưa
                if (db.User.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên người dùng đã tồn tại.");
                    return View(model);
                }

                // Kiểm tra mật khẩu và xác nhận mật khẩu có khớp nhau không
                if (model.Password != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "Mật khẩu và xác nhận mật khẩu không khớp.");
                    return View(model);
                }

                // Tạo bản ghi User mà không mã hóa mật khẩu
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu mật khẩu dưới dạng văn bản thuần
                    UserRole = "C"  // Bạn có thể thay đổi role nếu cần
                };
                db.User.Add(user);

                // Tạo bản ghi Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customer.Add(customer);

                // Lưu thông tin vào CSDL
                db.SaveChanges();

                // Sau khi đăng ký thành công, chuyển đến trang đăng nhập
                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
        // GET: Account/Login

        public ActionResult Login()

        {
            return View();

        }

        // POST: Account/Login

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // Tìm người dùng theo tên đăng nhập
            var user = db.User.SingleOrDefault(u => u.Username == username);

            if (user != null)
            {
                // Kiểm tra mật khẩu không mã hóa
                if (user.Password == password) // So sánh mật khẩu trực tiếp
                {
                    // Đăng nhập thành công, lưu thông tin vào Session
                    Session["Username"] = user.Username;
                    Session["Role"] = user.UserRole;

                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Mật khẩu không chính xác.";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Tên đăng nhập không tồn tại.";
                return View();
            }
        }
        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // Trang thông tin người dùng (cần đăng nhập)
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var user = db.User.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customer.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterVM
            {
                Username = user.Username,
                CustomerName = customer.CustomerName,
                CustomerEmail = customer.CustomerEmail,
                CustomerPhone = customer.CustomerPhone,
                CustomerAddress = customer.CustomerAddress
            };

            return View(model);
        }

        // Thay đổi mật khẩu (cần đăng nhập)
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                var user = db.User.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    // Kiểm tra mật khẩu hiện tại không mã hóa
                    if (user.Password != currentPassword)
                    {
                        ModelState.AddModelError("currentPassword", "Mật khẩu hiện tại không đúng.");
                        return View();
                    }

                    // Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
                    if (newPassword != confirmPassword)
                    {
                        ModelState.AddModelError("confirmPassword", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                        return View();
                    }

                    // Cập nhật mật khẩu mới
                    user.Password = newPassword; // Lưu mật khẩu mới dưới dạng văn bản thuần
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
                    return RedirectToAction("ProfileInfo");
                }
            }
           return View();
        }
    }
}
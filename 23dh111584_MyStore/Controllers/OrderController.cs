using _23dh111584_MyStore.Models.ViewModels;
using _23dh111584_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _23dh111584_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            // Kiểm tra giỏ hàng trong session, nếu giỏ hàng trống hoặc không có sản phẩm thì chuyển hướng về trang chủ
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // Xác thực người dùng đã đăng nhập chưa, nếu chưa thì chuyển hướng tới trang Đăng nhập
            var user = db.User.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu người dùng chưa đăng nhập
            }

            // Lấy thông tin khách hàng từ CSDL
            var customer = db.Customer.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu không tìm thấy khách hàng
            }

            // Tạo dữ liệu hiển thị cho CheckoutVM
            var model = new CheckoutVM
            {
                CartItems = cart,
                TotalAmount = cart.Sum(item => item.TotalPrice), // Tính tổng giá trị giỏ hàng
                OrderDate = DateTime.Now, // Mặc định lấy thời gian hiện tại
                ShippingAddress = customer.CustomerAddress, // Địa chỉ giao hàng
                CustomerID = customer.CustomerID, // Mã khách hàng
                Username = customer.Username // Tên đăng nhập của khách hàng
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            // Kiểm tra tính hợp lệ của ModelState
            if (ModelState.IsValid)
            {
                var cart = Session["Cart"] as List<CartItem>;

                // Nếu giỏ hàng trống, chuyển hướng về trang Home
                if (cart == null || !cart.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra nếu người dùng chưa đăng nhập thì chuyển hướng tới trang Login
                var user = db.User.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin khách hàng
                var customer = db.Customer.SingleOrDefault(c => c.Username == user.Username);
                if (customer == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Xử lý phương thức thanh toán
                string paymentStatus = "Chưa thanh toán";

                if (model.PaymentMethod == "Tiền mặt")
                {
                    paymentStatus = "Thanh toán tiền mặt";
                }
                else if (model.PaymentMethod == "Paypal")
                {
                    paymentStatus = "Thanh toán bằng Paypal";
                    return RedirectToAction("PaymentWithPaypal", "PayPal", model); // Điều hướng đến trang thanh toán Paypal
                }
                else if (model.PaymentMethod == "Mua trước trả sau")
                {
                    paymentStatus = "Trả góp";
                }

                // Tạo đơn hàng và chi tiết đơn hàng
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = model.OrderDate,
                    TotalAmount = model.TotalAmount,
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    DeliveryMethod = model.DeliveryMethod,
                    ShippingAddress = model.ShippingAddress,
                    OrderDetail = cart.Select(item => new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                };

                // Lưu đơn hàng vào CSDL
                db.Order.Add(order);
                db.SaveChanges();

                // Xóa giỏ hàng trong session sau khi đặt hàng thành công
                Session["Cart"] = null;

                // Chuyển hướng đến trang xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            // Nếu model không hợp lệ, trả lại view với model chứa thông tin lỗi
            return View(model);
        }


        // GET: Order/OrderSuccess
        public ActionResult OrderSuccess(int id)
        {
            // Lấy thông tin đơn hàng theo ID
            var order = db.Order.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return RedirectToAction("Index", "Home"); // Nếu không tìm thấy đơn hàng, chuyển hướng về trang chủ
            }

            return View(order); // Trả về view xác nhận đơn hàng
        }
    }
}
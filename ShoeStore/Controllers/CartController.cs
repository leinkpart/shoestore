using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ShoeStore.Controllers
{
    public class CartController : Controller
    {
        ShoeStoreEntities data = new ShoeStoreEntities();

        // Địa chỉ email người gửi và mật khẩu
        private string senderEmail = "linhpeter04@gmail.com";
        private string passwordEmail = "swcx zvjl dsbd jjbc";

        // GET: Cart
        public List<GioHang> GetCart()
        {
            List<GioHang> listCart = Session["cart"] as List<GioHang>;
            if (listCart == null)
            {
                listCart = new List<GioHang>();
                Session["cart"] = listCart;
            }

            return listCart;
        }

        // Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int id, string url)
        {
            // Tìm kiếm trong cơ sở dữ liệu để chắc chắn tồn tại
            var sp = data.Products.Find(id);
            if (sp == null)
            {
                // Xử lý nếu không tìm thấy sách
                return HttpNotFound("Sản phẩm không tồn tại.");
            }

            var cart = Session["Cart"] as List<GioHang> ?? new List<GioHang>();

            var existingItem = cart.FirstOrDefault(c => c.ProductID == id);
            if (existingItem != null)
            {
                existingItem.Quantity++; // Tăng số lượng
            }
            else
            {
                cart.Add(new GioHang
                {
                    ProductID = sp.ProductID,
                    Name = sp.Name,
                    Price = sp.Price,
                    Quantity = 1,
                    ImageURL = sp.ImageURL
                });
            }
            Session["Cart"] = cart;
            return Redirect(url);
        }

        private int TongSoLuong()
        {
            int qty = 0;
            List<GioHang> listCart = Session["Cart"] as List<GioHang>;
            if (listCart != null)
            {
                qty = listCart.Sum(c => c.Quantity);
            }
            return qty;
        }

        private decimal TongTien()
        {
            decimal dMoney = 0;
            List<GioHang> listCart = Session["Cart"] as List<GioHang>;
            if (listCart != null)
            {
                dMoney = listCart.Sum(c => c.Total);
            }
            return dMoney;
        }

        public ActionResult GioHang()
        {
            List<GioHang> listCart = GetCart();
            ViewBag.Qty = TongSoLuong();
            ViewBag.TotalAmount = TongTien();
            return View(listCart);
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public ActionResult RemoveFromCart(int id)
        {
            var cart = Session["Cart"] as List<GioHang> ?? new List<GioHang>();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
            }

            return RedirectToAction("GioHang");
        }

        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public ActionResult UpdateCart(int id, int quantity)
        {
            var cart = Session["Cart"] as List<GioHang> ?? new List<GioHang>();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                Session["Cart"] = cart;
            }

            return RedirectToAction("GioHang");
        }

        public ActionResult _QtyItem()
        {
            ViewBag.Qty = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }

        //Kiem tra thanh toan
        [HttpGet]
        public ActionResult Checkout()
        {
            if (Session["Account"] == null || Session["Account"].ToString() == "")
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng từ session hoặc database
            var cart = Session["Cart"] as List<GioHang>;
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Product");
            }

            //// Tạo đối tượng để chứa thông tin thanh toán (người đặt)
            //CheckoutViewModel model = new CheckoutViewModel
            //{
            //    CartItems = cart,
            //    TotalAmount = cart.Sum(i => i.Total)
            //};

            //decimal totalAmount = cart.Sum(item => item.Total);

            List<GioHang> lstCart = GetCart();

            var selectedAddressJson = TempData["SelectedAddress"] as string;
            DeliveryAddress selectedAddress = null;

            if (!string.IsNullOrEmpty(selectedAddressJson))
            {
                selectedAddress = Newtonsoft.Json.JsonConvert.DeserializeObject<DeliveryAddress>(selectedAddressJson);
            }

            ViewBag.SelectedAddress = selectedAddress;

            ViewBag.Qty = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstCart);
        }

        [HttpPost]
        public ActionResult Checkout(FormCollection fc)
        {
            List<GioHang> lst = new List<GioHang>();
            ViewBag.Qty = TongSoLuong();
            ViewBag.TongTien = TongTien();

            string strPay = fc["thanhtoan"];
            switch (strPay)
            {
                case "vivnpay":
                    // Xử lý thanh toán qua VNPay
                    return RedirectToAction("PaymentVNPay", "GioHang", new { ngayGiao = fc["NgayGiao"] });
                case "vimomo":
                    // Xử lý thanh toán qua MoMo
                    return RedirectToAction("PaymentMomo", "GioHang");
                case "cashondelivery":
                    // Xử lý thanh toán khi nhận hàng
                    SaveOrderBill(fc["NgayGiao"], false);
                    return RedirectToAction("ThankYou");
                default:
                    // Xử lý cho trường hợp không hợp lệ
                    break;
            }
            return View(lst);
        }

        public void SaveOrderBill (string NgayGiao, bool isPaied)
        {
            User user = (User)Session["Account"];
            List<GioHang> lst = GetCart();

            Order dh = new Order
            {
                UserID = user.UserID,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3),
                Status = "Delivering"
            };

            data.Orders.Add(dh);
            data.SaveChanges();


            foreach (var item in lst)
            {
                OrderDetail ctd = new OrderDetail
                {
                    OrderItemID = dh.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    TotalPrice = item.Total
                };
                data.OrderDetails.Add(ctd);
            }

            Payment pm = new Payment
            {
                OrderId = dh.OrderID,
                PaymentStatus = isPaied
            };
            data.Payments.Add(pm);

            data.SaveChanges();
            SendMail(dh, user, lst, pm);
            Session["Cart"] = null;
        }

        public ActionResult AddressRecieve(int selectedAddressId)
        {
            // Lấy địa chỉ từ cơ sở dữ liệu
            var address = data.DeliveryAddresses.Find(selectedAddressId);

            if (address == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy địa chỉ được chọn!";
                return RedirectToAction("Checkout");
            }

            // Lưu địa chỉ được chọn vào TempData
            TempData["SelectedAddress"] = address;
            TempData.Keep("SelectedAddress"); // Giữ TempData cho request tiếp theo

            return RedirectToAction("Checkout");
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        public void SendMail(Order dh, User user, List<GioHang> cart, Payment pm)
        {
            // Declare sender email and password
            string senderEmail = "linhpeter04@gmail.com";
            string passwordEmail = "swcx zvjl dsbd jjbc";

            // Cấu hình SMTP
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, passwordEmail),
                EnableSsl = true
            };

            // Tiêu đề email
            string subject = $"Xác nhận đơn hàng #{dh.OrderID}";

            // Nội dung email (HTML)
            StringBuilder body = new StringBuilder();
            body.AppendLine("<!DOCTYPE html>");
            body.AppendLine("<html lang='en'>");
            body.AppendLine("<head><meta charset='UTF-8'><style>");
            body.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; }");
            body.AppendLine("table { width: 100%; border-collapse: collapse; }");
            body.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            body.AppendLine("th { background-color: #f4f4f4; }");
            body.AppendLine("h1 { color: #333; }");
            body.AppendLine("</style></head>");
            body.AppendLine("<body>");
            body.AppendLine($"<h3>Xin chào {user.Name},</h3>");
            body.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại <strong>ShoeStore</strong>. <br/>Dưới đây là thông tin đơn hàng của bạn:</p>");

            // Thông tin đặt hàng
            body.AppendLine("<h2>Thông tin đơn hàng</h2>");
            body.AppendLine($"<p><strong>Ngày đặt hàng:</strong> {(dh.OrderDate.HasValue ? dh.OrderDate.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật")}</p>");
            body.AppendLine($"<p><strong>Ngày giao hàng dự kiến:</strong> {(dh.DeliveryDate.HasValue ? dh.DeliveryDate.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật")}</p>");
            body.AppendLine($"<p><strong>Hình thức thanh toán:</strong> {(pm.PaymentMethod ?? "Chưa cập nhật")}</p>"); // Hình thức thanh toán

            // Chi tiết sản phẩm
            body.AppendLine("<h2>Chi tiết sản phẩm</h2>");
            body.AppendLine("<table><thead><tr><th>Tên sách</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead><tbody>");

            decimal tongTien = 0;
            foreach (var item in cart)
            {
                decimal thanhTien = (decimal)(item.Quantity * item.Price);
                body.AppendLine($"<tr><td>{item.Name}</td><td>x{item.Quantity}</td><td>{item.Price:C}</td><td>{thanhTien:C}</td></tr>");
                tongTien += thanhTien;
            }

            body.AppendLine("</tbody></table>");
            body.AppendLine($"<p><strong>Tổng thanh toán:</strong> {tongTien:C}</p>");

            // Lời cảm ơn và liên hệ
            body.AppendLine("<p>Cảm ơn bạn đã tin tưởng và ủng hộ chúng tôi.</p>");
            body.AppendLine("<p>Trân trọng,<br/><strong>ShoeStore</strong></p>");
            body.AppendLine("<footer style='margin-top: 20px; font-size: 12px; color: #777;'>");
            body.AppendLine("<p>Liên hệ chúng tôi: <a href='mailto:support@shoestore.com'>support@shoestore.com</a> | Hotline: 123-456-789</p>");
            body.AppendLine("<p>Địa chỉ: 06 đ.Trần Văn Ơn, p.Phú Hòa, tp.Thủ Dầu Một, Bình Dương</p>");
            body.AppendLine("</footer>");
            body.AppendLine("</body>");
            body.AppendLine("</html>");

            // Gửi email
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, "ShoeStore");
                message.To.Add(new MailAddress(user.Email));
                message.Subject = subject;
                message.Body = body.ToString();
                message.IsBodyHtml = true;

                // Gửi email
                mail.Send(message);
            }
        }
    }
}
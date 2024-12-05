using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeStore.Controllers
{
    public class LoveProductController : Controller
    {
        private ShoeStoreEntities data = new ShoeStoreEntities();

        // GET: LoveProduct
        public ActionResult WishList()
        {
            // Lấy UserID từ Session
            User user = (User)Session["Account"];

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách sản phẩm yêu thích
            var loveProducts = data.LoveProducts
                .Where(lp => lp.UserID == user.UserID)
                .Select(lp => lp.Product)
                .ToList();

            return View(loveProducts);
        }

        [HttpPost]
        public JsonResult ToggleLoveProduct(int productId)
        {
            // Giả sử bạn đã lưu thông tin User trong Session
            User user = (User)Session["Account"];

            if (user == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để sử dụng tính năng này." });
            }

            // Kiểm tra xem sản phẩm đã nằm trong danh sách yêu thích chưa
            var existingLove = data.LoveProducts.FirstOrDefault(lp => lp.UserID == user.UserID && lp.ProductID == productId);

            if (existingLove != null)
            {
                // Nếu đã yêu thích thì xóa
                data.LoveProducts.Remove(existingLove);
                data.SaveChanges();
                return Json(new { success = true, isLoved = false });
            }
            else
            {
                // Nếu chưa yêu thích thì thêm vào
                var loveProduct = new LoveProduct
                {
                    UserID = user.UserID,
                    ProductID = productId,
                    CreatedAt = DateTime.Now
                };
                data.LoveProducts.Add(loveProduct);
                data.SaveChanges();
                return Json(new { success = true, isLoved = true });
            }
        }
    }
}
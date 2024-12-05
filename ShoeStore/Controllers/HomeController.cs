using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace ShoeStore.Controllers
{
    public class HomeController : Controller
    {
        ShoeStoreEntities data = new ShoeStoreEntities();

        private List<Product> GetSPMoi (int pageNumber,  int pageSize)
        {
            return data.Products.OrderByDescending(sp => sp.UpdatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        // GET: Home
        public ActionResult Index(int? page)
        {
            // Kiểm tra Session trước
            var userName = Session["UserName"] != null ? Session["UserName"].ToString() : null;
            ViewBag.UserName = userName;

            // Lấy danh sách ID sản phẩm yêu thích
            User user = (User)Session["Account"];
            var lovedProductIds = new List<int>();
            if (user != null)
            {
                lovedProductIds = data.LoveProducts
                                      .Where(lp => lp.UserID == user.UserID)
                                      .Select(lp => lp.ProductID)
                                      .ToList();
            }
            ViewBag.LovedProductIds = lovedProductIds; // Truyền danh sách yêu thích vào View


            int pageNumber = page ?? 1;
            int pageSize = 8;
            int totalGoods = data.Products.Count();
            int totalPage = (int)Math.Ceiling((double)totalGoods / pageSize);
            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = pageNumber;
            var lstNewProduct = GetSPMoi(pageNumber, pageSize);

            return View(lstNewProduct);
        }

        // Action khác, ví dụ: About
        public ActionResult About()
        {
            return View();
        }
    }
}
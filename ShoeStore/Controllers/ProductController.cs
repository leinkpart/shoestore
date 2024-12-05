using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeStore.Controllers
{
    public class ProductController : Controller
    {
        ShoeStoreEntities data = new ShoeStoreEntities();
        // GET: Product
        public ActionResult Index(string[] selectedBrands, string priceRange)
        {
            var products = data.Products.AsQueryable();
            var brands = data.Brands.ToList();

            // Lọc theo thương hiệu
            if (selectedBrands != null && selectedBrands.Length > 0)
            {
                products = products.Where(p => selectedBrands.Contains(p.Brand.Name));
            }

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                string[] ranges = priceRange.Split('-');
                if (ranges.Length == 2 && decimal.TryParse(ranges[0], out decimal minPrice) && decimal.TryParse(ranges[1], out decimal maxPrice))
                {
                    products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
            }

            // Đếm số sản phẩm tìm thấy
            int productCount = products.Count();

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

            // Đưa dữ liệu đến View
            ViewBag.Brands = brands;
            ViewBag.SelectedBrands = selectedBrands ?? new string[0];
            ViewBag.PriceRange = priceRange;
            ViewBag.ProductCount = productCount;

            return View(products.ToList());
        }

        public ActionResult ProductDetail (int id)
        {
            var product = data.Products.SingleOrDefault(p => p.ProductID == id);
            var comment = data.Reviews.Where(c => c.ProductID == id).ToList();

            ViewBag.Conmment = comment;
            ViewBag.Product = product;

            return View(product);
        }
    }
    
}
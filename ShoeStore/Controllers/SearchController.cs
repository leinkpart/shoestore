using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeStore.Controllers
{
    public class SearchController : Controller
    {
        ShoeStoreEntities data = new ShoeStoreEntities();

        // GET: Search
        public ActionResult Index(string query)
        {
            List<Product> products;

            if (string.IsNullOrEmpty(query))
            {
                // Nếu không có từ khóa tìm kiếm
                products = new List<Product>();
            }
            else
            {
                // Lọc các sản phẩm theo từ khóa tìm kiếm trong tên sản phẩm
                products = data.Products
                                    .Where(p => p.Name.Contains(query))
                                    .ToList();
            }

            // Trả về view với danh sách sản phẩm và số lượng sản phẩm
            ViewBag.ProductCount = products.Count;
            return View(products);
        }
    }
}
using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeStore.Controllers
{
    public class DeliveryAddressController : Controller
    {
        private ShoeStoreEntities data = new ShoeStoreEntities();

        // GET: DeliveryAddress
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllAddresses()
        {
            var addresses = data.DeliveryAddresses.ToList();
            return Json(addresses, JsonRequestBehavior.AllowGet);
        }

        // Thêm địa chỉ mới
        [HttpPost]
        public JsonResult AddAddress(string HoTen, string DienThoai, string DiaChi, string Xa, string Huyen, string Tinh)
        {
            if (ModelState.IsValid)
            {
                var address = new DeliveryAddress();
                address.HoTen = HoTen;
                address.DienThoai = DienThoai;
                address.DiaChi = DiaChi;
                address.Xa = Xa;
                address.Huyen = Huyen;
                address.Tinh = Tinh;
                data.DeliveryAddresses.Add(address);
                data.SaveChanges();
                return Json(new { success = true, message = "Thêm địa chỉ thành công!" });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }

        // Sửa địa chỉ
        [HttpPost]
        public JsonResult EditAddress(DeliveryAddress address)
        {
            var existingAddress = data.DeliveryAddresses.Find(address.AddressId);
            if (existingAddress != null)
            {
                existingAddress.HoTen = address.HoTen;
                existingAddress.DienThoai = address.DienThoai;
                existingAddress.DiaChi = address.DiaChi;
                existingAddress.Xa = address.Xa;
                existingAddress.Huyen = address.Huyen;
                existingAddress.Tinh = address.Tinh;

                data.SaveChanges();
                return Json(new { success = true, message = "Cập nhật địa chỉ thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy địa chỉ!" });
        }

        // Xóa địa chỉ
        [HttpPost]
        public JsonResult DeleteAddress(int id)
        {
            var address = data.DeliveryAddresses.Find(id);
            if (address != null)
            {
                data.DeliveryAddresses.Remove(address);
                data.SaveChanges();
                return Json(new { success = true, message = "Xóa địa chỉ thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy địa chỉ!" });
        }

        [HttpPost]
        public ActionResult SelectAddress(int id)
        {
            var address = data.DeliveryAddresses.FirstOrDefault(a => a.AddressId == id);
            if (address != null)
            {
                // Lưu địa chỉ vào TempData
                TempData["SelectedAddress"] = Newtonsoft.Json.JsonConvert.SerializeObject(address);
                return Json(new { success = true, address });
            }

            return Json(new { success = false, message = "Không tìm thấy địa chỉ." });
        }
    }
}
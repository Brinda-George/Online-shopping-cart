﻿using Online_SHopping_Cart.Models;
using Online_SHopping_Cart.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Web;
using System.Web.Mvc;

namespace Online_SHopping_Cart.Controllers
{
    public class ServiceController : Controller
    {
        ShoppingCartDbEntities db = new ShoppingCartDbEntities();
        // GET: Service
        #region Homepage
        /// <summary>
        /// 
        /// </summary>
        /// <returns>a home page</returns>
        public ActionResult Service_Home()
        {   
            Notification_Count();   //For getting notification count in navigation bar

            string userName = Session["user"].ToString();
            int orderCount = 0;
            int serviceProviderId = db.User_Table.Where(x => x.UserName == userName).Select(x => x.UserId).FirstOrDefault();
           
            var orders = (from a in db.Order_Table where a.OrderStatus == 1 && a.OrderIsDeleted == false select a).ToList();
            foreach (var items in orders)
            {
                var serviceIdList = (from b in db.OrderDetail_Table where items.OrderId == b.Orderid select b.Serviceid).ToList();
                foreach (var item in serviceIdList)
                {
                    var serviceId = (from c in db.Service_Table where c.ServiceId == item select c.ServiceProviderid).FirstOrDefault();
                    if (serviceId == serviceProviderId)
                    {
                        if (items.OrderDeliveryDate > System.DateTime.Now)
                        {
                            orderCount++;
                        }
                    }

                }
                Session["OrderCount"] = orderCount;//For printing number of currently running orders in home page
            }          
            return View();
        }
        /// <summary>
        /// For getting notification number in navigation bar 
        /// </summary>
        public void Notification_Count()
        {
            string userName = Session["user"].ToString();
            int serviceProviderId = db.User_Table.Where(x => x.UserName == userName).Select(x => x.UserId).FirstOrDefault();
            List<Order_Table> orderList = new List<Order_Table>();

            var orders = (from a in db.Order_Table where a.OrderStatus == 1 && a.OrderIsDeleted == false && a.OrderNotification == "00" || a.OrderNotification == "10" select a);

            foreach (var items in orders)
            {
                var serviceIdList = (from b in db.OrderDetail_Table where items.OrderId == b.Orderid select b.Serviceid).ToList();
                foreach (var item in serviceIdList)
                {
                    var serviceId = (from a in db.Service_Table where a.ServiceId == item select a.ServiceProviderid).FirstOrDefault();
                    if (serviceId == serviceProviderId)
                    {
                        orderList.Add(items);
                    }
                }
            }

            ViewBag.Order = orderList.DistinctBy(x => x.OrderId).ToList();

            Session["NotificationCount"] = orderList.DistinctBy(x => x.OrderId).Count();
        }

        /// <summary>
        /// For disabling notification number in navigation bar after seen by the user
        /// </summary>
        public void DisableNotification()
        {
            Order_Table obj = new Order_Table();
            var order = (from a in db.Order_Table select a).ToList();
            foreach (var item in order)
            {
                if (item.OrderNotification == "00")         //notification satus=00,when new order is placed
                {
                    item.OrderNotification = "01";          //changing status to 01, indicating service provider has seen the notification
                }
                else if (item.OrderNotification == "10")    //notification satus = 10, when new order is seen by seller but not service provider
                {
                    item.OrderNotification = "11";          //changing status to 11, indicating both seller and service provider has seen the notification
                }

            }

            db.SaveChanges();

            Notification_Count();
        }
        #endregion

        #region Add Service
        /// <summary>
        /// Get method for adding service by the courier service provider
        /// </summary>
        /// <returns>a page for adding service</returns>
        [HttpGet]
        public ActionResult Add_Service()
        {
            Notification_Count();
            
            Service_ViewModel svm_obj = new Service_ViewModel();
            List<BaseCategory_Table> baseCategoryList = new List<BaseCategory_Table>();
            var productListItem = new List<SelectListItem>();

            baseCategoryList = db.BaseCategory_Table.Where(x => x.BaseCatIsDeleted==false).ToList();
            
            foreach (var item in baseCategoryList)
            {
                productListItem.Add(new SelectListItem
                {
                    Text = item.BaseCatName.ToString(),
                    Value = item.BaseCatId.ToString(),

                });

                ViewBag.BaseCategory = productListItem;
            }
          
            svm_obj.locationList = db.Location_Table.Where(x=>x.LocationIsDeleted==false).ToList();
            svm_obj.selectedLocation = 0;

            ViewBag.message = TempData["Message"];
            return View(svm_obj);

        }
        /// <summary>
        /// For displaying product category based on base category
        /// </summary>
        /// <param name="baseCategoryId"></param>
        /// <returns>dropdownlist of product categories</returns>
        public ActionResult GetProductCategory(string baseCategoryId)
        {
            int baseCatId;
            List<SelectListItem> productCategoryList = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(baseCategoryId))
            {
                baseCatId = Convert.ToInt32(baseCategoryId);
                List<ProductCategory_Table> productCategory = db.ProductCategory_Table.Where(x => x.BaseCatid == baseCatId && x.ProductCatIsDeleted==false).ToList();
                foreach (var item in productCategory)
                {
                    productCategoryList.Add(new SelectListItem
                    {
                        Text = item.ProductCatName.ToString(),
                        Value = item.ProductCatId.ToString(),

                    });
                }
            }

            return Json(productCategoryList, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// For displaying partial view of products based on the type of product choosen
        /// </summary>
        /// <param name="productCatId"></param>
        /// <returns>partial view of products</returns>
        [HttpGet]
        public ActionResult Products_PartialView(int productCatId)
        {
            List<Buyer_Product> productList = new List<Buyer_Product>();

            var products = (from p in db.Product_Table
                            where p.ProductCatid == productCatId && p.ProductIsDeleted == false
                            select p).ToList();
            foreach (var item in products)
            {
                Buyer_Product product = new Buyer_Product();
                product.ProductName = item.ProductName;
                product.ProductId = item.ProductId;
                product.ProductPrice = item.ProductPrice;
                product.ProductDesc = item.ProductDesc;
                Image_Table image = db.Image_Table.Where(x => x.Productid == item.ProductId && x.ImageIsDeleted==false).FirstOrDefault();
                if(image != null)
                {
                    product.BinaryImage = image.BinaryImage;
                    productList.Add(product);
                }
            }

            ViewBag.ProductList = productList.DistinctBy(x => x.ProductId).ToList();
            return PartialView("_productsPartialView");
        }
        /// <summary>
        /// For displaying different images of same product when clicked
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns>list of images</returns>
        [HttpPost]
        public ActionResult ImageDisplay(int imageId)
        {
            Product_Table product = db.Product_Table.Find(imageId);

            var imageList = (from a in db.Image_Table
                             where a.Productid == product.ProductId && a.ImageIsDeleted == false
                             select a).ToList();
            ViewBag.ImageList = imageList.ToList();
            return PartialView("_imageDisplay");
        }
        /// <summary>
        /// post method for adding service.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="productIds"></param>
        /// <returns>Add service page</returns>
        [HttpPost]
        public ActionResult Add_Service(Service_ViewModel model, string[] productIds)
        {
            try
            {
                Notification_Count();

                string name = Session["user"].ToString();
                int serviceProviderId = db.User_Table.Where(x => x.UserName == name).Select(x => x.UserId).FirstOrDefault();
                List<Order_Table> orderList = new List<Order_Table>();

                for (int i = 0; i < productIds.Count(); i++)
                {
                    int currentProduct = Convert.ToInt32(productIds[i]);
                    var product = (from s in db.Service_Table where s.Productid == currentProduct && s.Locationid == model.selectedLocation && s.ServiceProviderid == serviceProviderId && s.ServiceIsDeleted == false select s).FirstOrDefault();

                    if (product == null)
                    {
                        Service_Table service = new Service_Table();
                        service.Locationid = model.selectedLocation;
                        service.DeliveryCharge = model.deliveryCharge;
                        service.ServiceDesc = model.serviceDescription;
                        service.Productid = Convert.ToInt32(productIds[i]);
                        service.ServiceName = model.serviceName;
                        service.ServiceProviderid = serviceProviderId;
                        service.ServiceCreatedDate = System.DateTime.Now;
                        service.ServiceUpdatedDate = System.DateTime.Now;
                        service.ServiceCreatedBy = Session["user"].ToString();
                        service.ServiceUpdatedBy = Session["user"].ToString();
                        service.ServiceIsDeleted = false;
                        db.Service_Table.Add(service);
                        db.SaveChanges();


                    }
                    else
                    {
                        var productName = (from p in db.Product_Table where p.ProductId == currentProduct select p.ProductName).FirstOrDefault();
                        string message = String.Format("Same location already added for the product " + productName);
                        TempData["Message"] = message;
                        return RedirectToAction("Add_Service");
                    }
                }
                TempData["Message"] = "Service Added";
                return RedirectToAction("Add_Service");
            }
            catch (Exception e)
            {
                TempData["Message"] = "fill all fileds";
                return RedirectToAction("Add_Service");
            }

        }
        #endregion

        #region Manage service
        /// <summary>
        /// For editing the existing services
        /// </summary>
        /// <returns>grid view of added services</returns>
        public ActionResult Manage_Service()
        {
            Notification_Count();
            string userName = Session["user"].ToString();
            int serviceProviderId = db.User_Table.Where(x => x.UserName == userName).Select(x => x.UserId).FirstOrDefault();

            var serviceDetails = (from s in db.Service_Table
                                  join p in db.Product_Table on s.Productid equals p.ProductId
                                  join l in db.Location_Table on s.Locationid equals l.LocationId
                                  where s.ServiceIsDeleted != true && s.ServiceProviderid == serviceProviderId
                                  select new
                                  {
                                      s.ServiceId,
                                      s.ServiceName,
                                      s.DeliveryCharge,
                                      s.ServiceDesc,
                                      l.LocationName,
                                      p.ProductName
                                  });

            ViewBag.Services = serviceDetails.ToList();



            return View();
        }
        /// <summary>
        /// For editing the details
        /// </summary>
        /// <param name="ServiceId"></param>
        /// <param name="ServiceName"></param>
        /// <param name="DeliveryCharge"></param>
        /// <param name="ServiceDesc"></param>
        /// <param name="LocationName"></param>
        /// <param name="ProductName"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ServiceEdit(int ServiceId, string ServiceName, decimal DeliveryCharge, string ServiceDesc, string LocationName, string ProductName)
        {

            Product_Table product = new Product_Table();
            Location_Table location = new Location_Table();
            Service_Table service = db.Service_Table.Find(ServiceId);
            service.ServiceName = ServiceName;
            service.DeliveryCharge = DeliveryCharge;
            service.ServiceDesc = ServiceDesc;
            int locationId = (from l in db.Location_Table where l.LocationName == LocationName select l.LocationId).FirstOrDefault();
            service.Locationid = locationId;
            int productId = (from p in db.Product_Table where p.ProductName == ProductName select p.ProductId).FirstOrDefault();
            service.Productid = productId;
            service.ServiceUpdatedBy = Session["user"].ToString();
            service.ServiceUpdatedDate = System.DateTime.Now;
            db.SaveChanges();
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("Manage_Service", "Service");
            return Json(new { ServiceId = ServiceId, ServiceName = ServiceName, DeliveryCharge = DeliveryCharge, ServiceDesc = ServiceDesc, LocationName = LocationName, ProductName = ProductName, Url = redirectUrl }, JsonRequestBehavior.AllowGet);



        }
        /// <summary>
        /// For deleting the service.
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ServiceDelete(int serviceId)
        {
            Service_Table service = db.Service_Table.Find(serviceId);
            service.ServiceIsDeleted = true;
            service.ServiceUpdatedBy= Session["user"].ToString();
            service.ServiceUpdatedDate = System.DateTime.Now;
            db.SaveChanges();
            bool result = true;
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("Manage_Service", "Service");
            return Json(new { Url = redirectUrl, result });
        }
        #endregion

        #region View service
        /// <summary>
        /// For displaying the details of customers who have opted his service.grid view will show a link to diplay details of a purticular order. 
        /// </summary>
        /// <returns>grid view of order details</returns>
        public ActionResult View_Service()
        {
            DisableNotification();
            Notification_Count();

            string name = Session["user"].ToString();
            List<Order_Table> orderList = new List<Order_Table>();
            int serviceProviderId = db.User_Table.Where(x => x.UserName == name).Select(x => x.UserId).FirstOrDefault();
            
            var orders = (from o in db.Order_Table where o.OrderStatus == 1 && o.OrderIsDeleted == false select o);
            foreach (var items in orders)
            {
                var serviceIdList = (from or in db.OrderDetail_Table where items.OrderId == or.Orderid select or.Serviceid).ToList();
                foreach (var item in serviceIdList)
                {

                    var serviceId = (from s in db.Service_Table where s.ServiceId == item select s.ServiceProviderid).FirstOrDefault();
                    if (serviceId == serviceProviderId)
                    {
                        orderList.Add(items);
                    }

                }
            }

            ViewBag.OrderList = orderList.DistinctBy(x => x.OrderId).ToList();

            return View();

        }
        /// <summary>
        /// For displaying the order details inside an order using order id and user id.
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="UserId"></param>
        /// <returns>partial view popup of order details</returns>
        [HttpPost]
        
        public ActionResult OrderDetails(int OrderId, int UserId)
        {

            List<OrderHistory_ViewModel> ohvmlist = new List<OrderHistory_ViewModel>();
            string userName = db.User_Table.Where(x => x.UserId == UserId).Select(x => x.UserName).FirstOrDefault();
            string userPhone = db.User_Table.Where(x => x.UserId == UserId).Select(x => x.UserPhno).FirstOrDefault();

            var orderList = db.OrderDetail_Table.Where(x => x.Orderid == OrderId).ToList();
            List<string> list = new List<string>();
            foreach (var item in orderList)
            {
                var service = db.OrderDetail_Table.Where(x => x.Orderid == OrderId).Select(x => x.Serviceid).FirstOrDefault();
                var product_desc = db.Product_Table.Where(x => x.ProductId == item.Productid).Select(x => x.ProductDesc).FirstOrDefault();
                var product = db.Product_Table.Where(x => x.ProductId == item.Productid).Select(x => x.ProductName).FirstOrDefault();
                var deliveryadd = db.Order_Table.Where(x => x.OrderId == OrderId).Select(x => x.OrderDeliveryAddress).FirstOrDefault();
                var deliverydate = db.Order_Table.Where(x => x.OrderId == OrderId).Select(x => x.OrderDeliveryDate).FirstOrDefault();
                var image = db.Image_Table.Where(x => x.Productid == item.Productid).Select(x => x.BinaryImage).FirstOrDefault();
                OrderHistory_ViewModel vm_obj = new OrderHistory_ViewModel();
                vm_obj.ProductName = product;
                vm_obj.ProductDesc = product_desc;
                vm_obj.OrderDelivryAddress = deliveryadd;
                vm_obj.Amount = (decimal)item.Amount;
                vm_obj.OrderDeliveryDate = (DateTime)deliverydate;
                vm_obj.CustomerName = userName;
                vm_obj.BinaryImage = image;
                ohvmlist.Add(vm_obj);
                list.Add(userName);
                list.Add(deliveryadd);
                list.Add(userPhone);

            }
            ViewBag.list = list.Distinct();
            return PartialView("_orderDetails", ohvmlist);
        }

        #endregion
        [HttpGet]
        public ActionResult profile()
        {
            Notification_Count();
            ViewBag.fill_msg = TempData["fill_msg"];
            
            string name = Session["user"].ToString();
            User_Table obj = db.User_Table.Where(x => x.UserName == name).FirstOrDefault();
            return View(obj);
        }

        [HttpPost]
        public ActionResult profile(User_Table obj)
        {
            Notification_Count();
            if (obj.FirstName != null && obj.LastName != null && obj.UserEmail != null && obj.UserAddress != null && obj.UserPhno != null)
            {
                string name = Session["user"].ToString();
                User_Table user = db.User_Table.Where(x => x.UserName == name).FirstOrDefault();
                user.FirstName = obj.FirstName;
                user.LastName = obj.LastName;
                user.UserEmail = obj.UserEmail;
                user.UserAddress = obj.UserAddress;
                user.UserPhno = obj.UserPhno;
                user.UserUpdateBy = name;
                user.UserUpdatedDate = System.DateTime.Now;
                db.SaveChanges();
            }
            else
            {
                TempData["fill_msg"] = "Please Enter The Details";
                return RedirectToAction("profile");
            }
            return View();
        }

        public ActionResult ChangePassword()
        {
            Notification_Count();
            ViewBag.message = TempData["message"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            Notification_Count();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User_Table obj = new User_Table();
            string name = Session["user"].ToString();
            User_Table details = (from a in db.User_Table where a.UserName == name select a).FirstOrDefault();
            if (details.Password == model.OldPassword)
            {
                if (details.Password == model.NewPassword)
                {
                    TempData["message"] = "your old password and new password are same!!!";
                }
                else if (model.NewPassword == model.ConfirmPassword)
                {
                    details.Password = model.NewPassword;
                    db.SaveChanges();
                    TempData["message"] = "password changes successfully!!";
                }
                else
                {
                    TempData["message"] = "confirm password an new password does not match";
                }
            }
            else
            {
                TempData["message"] = "your old password is incorrect ";
            }
            return RedirectToAction("ChangePassword");
        }

        public void logout()
        {
            Session["user"] = null;
            Session["count"] = null;
            Session.Abandon();
            Response.Redirect("/User/login");
        }
    }
}
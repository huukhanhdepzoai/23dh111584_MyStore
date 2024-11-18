﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _23dh111584_MyStore.Models.ViewModels;
using _23dh111584_MyStore.Models;
using PagedList;

namespace _23dh111584_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();
        public ActionResult Index(string searchTerm, int? Page)
        {
            var model = new HomeProductVM();
            var products = db.Product.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p =>
                        p.ProductName.Contains(searchTerm) ||
                        p.ProductDescription.Contains(searchTerm) ||
                        p.Category.CategoryName.Contains(searchTerm));
            }
            int pageNumber = Page ?? 1;
            int pageSize = 6;
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetail.Count()).Take(10).ToList();
            model.NewProducts = products.OrderBy(p => p.OrderDetail.Count()).Take(20).ToPagedList(pageNumber, pageSize);
            return View(model);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Product pro = db.Product.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }
            var product = db.Product.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();
            ProductDetailVM model = new ProductDetailVM();

            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;
            model.Product = pro;
            model.RelatedProducts = product.OrderBy(p => p.ProductID).Take(8).ToPagedList(pageNumber, pageSize);
            model.TopProducts = product.OrderByDescending(p => p.OrderDetail.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }
            return View(model);
        }
    }
}

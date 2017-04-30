using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ParseSQL2.Models;
using ParseSQL2.DAL;


namespace ParseSQL2.Controllers
{
    public class DefaultController : Controller
    {
        QueryContext context = new QueryContext();
        // GET: Default
        public ActionResult Index()
        {
            //   context.Database.Initialize(true);

            //   QueryMaster query = new QueryMaster();
            var query = context.Query.ToList();
    
             
   
            return View(query);
        }

        // GET: Default/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Default/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Default/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                QueryMaster query = new QueryMaster();
                query.QueryText = collection[1];
                // TODO: Add insert logic here
                query.customerid = Convert.ToInt32(collection[2]);
                context.Query.Add(query);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Default/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Default/Edit/5
        [HttpPost]
        public ActionResult Edit(QueryMaster query)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    context.Entry(query).State = System.Data.Entity.EntityState.Modified;
              //      context.Query.Add(query);
                    context.SaveChanges();
                }
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Default/Delete/5
        public ActionResult Delete(int id)
        {
            QueryMaster query = context.Query.Find(id);
            return View(query);
        }
        

        // POST: Default/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                QueryMaster query = context.Query.Find(id);
                context.Query.Remove(query);
                context.SaveChanges();
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

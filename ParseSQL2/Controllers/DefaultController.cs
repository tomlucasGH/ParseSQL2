using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ParseSQL2.Models;
using ParseSQL2.DAL;
using Helpers;


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

                Parser parser = new Parser();

                List<Parser.outputstruct> tablelist = Parser.GetTableNamesFromQueryString(query.QueryText);
                foreach(Parser.outputstruct n in tablelist)
                {
                    TableList table = new TableList();
                    table.TableName = n.Table;
                    table.owner = n.Owner;
                    table.AliasName = n.Alias;
                    table.queryID = query.ID;
                    context.Tables.Add(table);
                }
                context.SaveChanges();
                List<Parser.columnstruct> columnlist = Parser.FindColumns(query.QueryText);
                foreach(Parser.columnstruct n in columnlist)
                {
                    SelectColumns columns = new SelectColumns();
                    var tbl =
             from c in context.Tables
              where c.queryID == query.ID && 
              (c.AliasName.Replace("[","").Replace("]","") == n.Alias || c.TableName.Replace("[","").Replace("]","") == n.Alias)
              select  c.TableName;

                    if (tbl.Count() > 0)
                    {
                        columns.QueryID = query.ID;
                        columns.TableName = tbl.First().ToString();
                        columns.ColumnName = n.Column;
                        context.columnlist.Add(columns);
                    }
                    }
                context.SaveChanges();

                List<Parser.wherestruct> whereclauseList = Parser.GetFilterCriteriaFromQueryString(query.QueryText, tablelist);
                foreach(Parser.wherestruct n in whereclauseList)
                {
                    WhereClause whereclauseClass = new WhereClause();
                    whereclauseClass.ColumnName = n.Column;
                    whereclauseClass.comparison_operator = n.comparison_operator;
                    whereclauseClass.comparison_value = n.comparison_value;
                    whereclauseClass.TableName = n.Table;
                    whereclauseClass.QueryID = query.ID;
                    whereclauseClass.function_string = n.function_string;
                    whereclauseClass.function_name = n.function_name;


                    context.whereclause.Add(whereclauseClass);
                }
                context.SaveChanges();
                return RedirectToAction("Index");

            }
            catch (Exception ex)
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
        // GET: Default/Execute
        public ActionResult Add_source()
        {
            var source = context.datasources.ToList();
            return View(source);
        }
        [HttpGet]
        public ActionResult Create_source()
        {
                 return View();
        }
        [HttpPost]
        public ActionResult Create_source(FormCollection collection)
        {
            try
            {
                datasources sources = new datasources();
                sources.type = collection[1].ToString();
                sources.connection = collection[2].ToString();
                sources.name = collection[3].ToString();
                context.datasources.Add(sources);
                context.SaveChanges();
                // TODO: Add execute logic here
                //datasources sources = context.datasources.Find(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

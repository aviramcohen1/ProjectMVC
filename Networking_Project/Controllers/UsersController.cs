using Networking_Project.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using System.Windows.Forms;

namespace Networking_Project.Controllers
{
    public class UsersController : Controller
    {
        private DATABASEEntities db = new DATABASEEntities();
        // GET: Users
        public ViewResult Index(string sortOrder, string currentFilter, string cF1, string cF2,
           string searchCategory, string searchPriceR1, string searchPriceR2, string searchDateR1,
           string searchDateR2, string cF3, string cF4, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CategorySortParm = String.IsNullOrEmpty(sortOrder) ? "category_desc" : "";
            ViewBag.TicketPriceSortParm = sortOrder == "TicketPrice" ? "ticketprice_desc" : "TicketPrice";
            ViewBag.RankSortParm = sortOrder == "Rank" ? "rank_desc" : "Rank";
            if (searchCategory != null)
            {
                page = 1;
            }
            else
            {
                if (searchCategory == null)
                { searchCategory = currentFilter; }
                else if (searchPriceR1 == null && searchPriceR2 == null)
                {
                    searchPriceR1 = cF1;
                    searchPriceR2 = cF2;
                }
                else if (searchDateR1 == null && searchDateR2 == null)
                {
                    searchDateR1 = cF3;
                    searchDateR2 = cF4;
                }
                else { page = 1; }
            }

            ViewBag.CurrentFilter = searchCategory;
            ViewBag.cF1 = searchPriceR1;
            ViewBag.cF2 = searchPriceR2;
            ViewBag.cF3 = searchDateR1;
            ViewBag.cF4 = searchDateR2;
            var movies = from s in db.MoviesTbl
                           select s;
            if (!String.IsNullOrEmpty(searchCategory) || !String.IsNullOrEmpty(searchPriceR1) &&
                !String.IsNullOrEmpty(searchPriceR2) ||
                !String.IsNullOrEmpty(searchDateR1) && !String.IsNullOrEmpty(searchDateR2))
            {
                if (!String.IsNullOrEmpty(searchCategory))
                {
                    movies = movies.Where(s => s.Category.Contains(searchCategory)
                                         || s.Category.Contains(searchCategory));
                }
                else if (!String.IsNullOrEmpty(searchDateR1) && !String.IsNullOrEmpty(searchDateR2))
                {
                    var dt1 = DateTime.Parse(searchDateR1);
                    var dt2 = DateTime.Parse(searchDateR2);
                    movies = movies.Where(s => s.StartTime >= dt1 && s.StartTime <= dt2);
                }
                else
                {

                    var pricerange1 = Int32.Parse(searchPriceR1);
                    var pricetrange2 = Int32.Parse(searchPriceR2);
                    movies = movies.Where(s => s.TicketPrice >= pricerange1 && s.TicketPrice <= pricetrange2);
                }
            }
            switch (sortOrder)
            {
                case "category_desc":
                    movies = movies.OrderByDescending(s => s.Category);
                    break;
                case "TicketPrice":
                    movies = movies.OrderBy(s => s.TicketPrice);
                    break;
                case "ticketprice_desc":
                    movies = movies.OrderByDescending(s => s.TicketPrice);
                    break;
                case "Rank":
                    movies = movies.OrderBy(s => s.Rank);
                    break;
                case "rank_desc":
                    movies = movies.OrderByDescending(s => s.Rank);
                    break;
                default:
                    movies = movies.OrderByDescending(s => s.Category);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(movies.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Order()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Order(int? id, int iduser, OrdersTbl ord)
        {
            DateTime localDate = DateTime.Now;
            string text = "This film has already been screened!!!";
            MoviesTbl movie = db.MoviesTbl.Find(id);
            if (movie.StartTime <= localDate)
            {
                MessageBox.Show(text);
                return RedirectToAction("Index");
            }
            if (id == null)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if (movie == null)
            {

                return HttpNotFound();
            }
           
            int id_ = id.GetValueOrDefault();
            ord.idScreen = id_;
            ord.idUser = iduser;
            ord.Pay = false;
            ord.Status = "Still in stock";
            ord.NumberCard = 00000000;
            ord.Validity = localDate;
            ord.validation = 000;
            if (Order_(ord))
            {
                var check_occupied = db.OrdersTbl.FirstOrDefault(s => s.idScreen == ord.idScreen &&
                s.Chair == ord.Chair&&s.Pay==true);

                if (check_occupied == null && ord.Chair >= 1 && ord.Chair <= movie.NumberSeats
                   )
                {
                    db.OrdersTbl.Add(ord);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    if (check_occupied != null)

                    {
                        ViewBag.ErrorMessage = ("its seat already occupied!!! choose another seat");


                    }
                    if (ord.Chair < 1 || ord.Chair > movie.NumberSeats)
                    {
                        ViewBag.ErrorMessage = ("Invalid seat!!! choose seat between 1-" + movie.NumberSeats);

                    }
                    return View(ord);
                }
            }
            return View(ord);

        }
        public ActionResult editOrder()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editOrder(int? idorder, int idscreen, OrdersTbl ord)
        {
            DateTime localDate = DateTime.Now;
            string text = "This film has already been screened!!!";
            MoviesTbl movie = db.MoviesTbl.Find(idscreen);
            OrdersTbl order = db.OrdersTbl.Find(idorder);
            if (movie.StartTime < localDate||order.Pay==true)
            {
                if (movie.StartTime < localDate)
                { MessageBox.Show(text); }
                else
                {
                    MessageBox.Show("credit this still out of stock!!!");
                }
                return RedirectToAction("Index");
            }
            if (idorder == null)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (movie == null)
            {

                return HttpNotFound();
            }

            int id_ = idorder.GetValueOrDefault();
            ord.idScreen = id_;
            ord.idUser = order.idUser;
            ord.Pay = false;
            ord.Status = "Still in stock";
            ord.NumberCard = 00000000;
            ord.Validity = localDate;
            ord.validation = 000;
            if (Order_(ord))
            {
                var check_occupied = db.OrdersTbl.FirstOrDefault(s => s.idScreen == ord.idScreen &&
                s.Chair == ord.Chair && ord.Pay == true);

                if (check_occupied == null && ord.Chair >= 1 && ord.Chair <= movie.NumberSeats
                   )
                {
                    db.OrdersTbl.Remove(order);
                    db.OrdersTbl.Add(ord);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    if (check_occupied != null)

                    {
                        MessageBox.Show("its seat already occupied!!! choose another seat");
                        
                    }
                    if (ord.Chair < 1 || ord.Chair > movie.NumberSeats)
                    {
                        MessageBox.Show("Invalid seat!!! choose seat between 1-" + movie.NumberSeats);
                    

                    }
                    return View(ord);
                }
            }
            return View(ord);
        }
        public bool Order_(OrdersTbl ord)
        {
            if (ModelState.IsValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        [HttpGet]
        public ActionResult MyOrders(int id)
        {

            DataTable myorders = new DataTable();
            var id_ = id.ToString();  
            string query = "select ot.idOrder, mt.idScreen,mt.Name,mt.StartTime,mt.EndTime,mt.Hall,mt.NumberSeats, " +
            "ot.Chair,mt.TicketPrice,ot.Status "+
           "from MoviesTbl mt,OrdersTbl ot where" +
            " mt.idScreen = ot.idScreen and ot.idUser =" +
            id_;
            string constring = @"data source = (localdb)\ProjectsV13; initial catalog = DATABASE; integrated security = True;MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection sqlCon = new SqlConnection(constring))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(myorders);
            }

            return View(myorders);

        }
        public ActionResult Payment()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int id/*, OrdersTbl ord*/)
        {
            DateTime localDate = DateTime.Now;
            OrdersTbl ord_old = db.OrdersTbl.Find(id);
            OrdersTbl ord_ = new OrdersTbl
            {
                //idOrder = ord_old.idOrder,
                idUser = ord_old.idUser,
                idScreen = ord_old.idScreen,
                Chair = ord_old.Chair,
                Pay = true,
                Status = "A ticket was purchased",
                NumberCard = 000000,
                Validity = localDate,
                validation = 0
            };
            if (ModelState.IsValid)
            {
                var cheak_occ = db.OrdersTbl.FirstOrDefault(s => s.idScreen == ord_old.idScreen && s.Pay == true);
                if(cheak_occ!=null)
                {
                    MessageBox.Show("Not invalid!! the ticket out of stock!!");
                    return RedirectToAction("Index");
                }
                //OrdersTbl ordd = db.OrdersTbl.Find(id);
                db.OrdersTbl.Remove(ord_old);   
                db.OrdersTbl.Add(ord_);
                db.SaveChanges();
                //CHECK IF THE ISCREEN WITH CHAIR=CHAIR FIND IN DB
                var order_check = db.OrdersTbl.Where(x => x.idScreen==ord_.idScreen&&x.Chair==ord_.Chair&&x.Pay==false).FirstOrDefault();
                if(order_check!=null)
                {
                   
                    OrdersTbl ord_new = new OrdersTbl
                    {
                        //idOrder = order_check.idOrder,
                        idUser = order_check.idUser,
                        idScreen = order_check.idScreen,
                        Chair = order_check.Chair,
                        Pay = false,
                        Status = "out of stock",
                        NumberCard = 000000,
                        Validity = localDate,
                        validation = 0
                    };
                    int id_ = order_check.idOrder;
                    OrdersTbl ordd_ = db.OrdersTbl.Find(id_);
                    db.OrdersTbl.Remove(ordd_);
                    db.OrdersTbl.Add(ord_new);
                    db.SaveChanges();

                }
              
                string text = "Payment made successfully!!!";
                MessageBox.Show(text);
                return RedirectToAction("Index");
            }
            else
            {
                return View(ord_old);
            }
           
        }
       
    }
}
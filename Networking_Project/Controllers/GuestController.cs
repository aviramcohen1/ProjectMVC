using Networking_Project.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Networking_Project.Controllers
{
    public class GuestController : Controller
    {
        // GET: Guest

        private DATABASEEntities db = new DATABASEEntities();
       
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

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(movies.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult PayMent()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayMent(int id,int iduser,OrdersTbl ord)
        {
            DateTime localDate = DateTime.Now;
            string text = "This film has already been screened!!!";
            MoviesTbl movie = db.MoviesTbl.Find(id);
            if (movie.StartTime <= localDate)
            {
                MessageBox.Show(text);
                return RedirectToAction("Index");
            }
            ord.idScreen = id;
            ord.idUser = iduser;
            ord.Pay = true;
            ord.Status = "A ticket was purchased";
            if (Order_(ord))
            {
                var check_occupied = db.OrdersTbl.FirstOrDefault(s => s.idScreen == ord.idScreen &&
                s.Chair == ord.Chair && ord.Pay == true);
                var check_pay = db.OrdersTbl.FirstOrDefault(s => s.idScreen == ord.idScreen &&
              s.Chair == ord.Chair && ord.Pay == false);

                if (check_occupied == null && ord.Chair >= 1 && ord.Chair <= movie.NumberSeats
                   )
                {
                    db.OrdersTbl.Add(ord);
                    if(check_pay!=null)
                    {
                        OrdersTbl ord_new = new OrdersTbl
                        {
                            //idOrder = order_check.idOrder,
                            idUser = check_pay.idUser,
                            idScreen = check_pay.idScreen,
                            Chair = check_pay.Chair,
                            Pay = false,
                            Status = "out of stock",
                            NumberCard = 000000,
                            Validity = localDate,
                            validation = 0
                        };
                        int id_ = check_pay.idOrder;
                        OrdersTbl ordd_ = db.OrdersTbl.Find(id_);
                        db.OrdersTbl.Remove(ordd_);
                        db.OrdersTbl.Add(ord_new);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    if (check_occupied != null)

                    {
                        MessageBox.Show("its seat already occupied!!! choose another seat");
                        //ViewBag.ErrorMessage = ("its seat already occupied!!! choose another seat");


                    }
                    if (ord.Chair <= 1 || ord.Chair >= movie.NumberSeats)
                    {
                        MessageBox.Show("Invalid seat!!! choose seat between 1 - " + movie.NumberSeats);
                        //ViewBag.error = "Invalid seat!!! choose seat between 1-" + movie.NumberSeats;

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
    }
}
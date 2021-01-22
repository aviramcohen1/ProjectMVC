using Networking_Project.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Networking_Project.Controllers
{
    public class AdminController : Controller
    {
        private DATABASEEntities db = new DATABASEEntities();
        // GET: Admin
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
        // GET: Movie/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Create([Bind(Include = "idScreen,Name,StartTime,EndTime,Hall,NumberSeats,TicketPrice,AgeMin,Category,Rank,Poster")] MoviesTbl movie)
        {

            DateTime localDate = DateTime.Now;
            HttpPostedFileBase file = Request.Files["ImageData"];
            var hall_check = db.MoviesTbl.Where(x => x.Hall == movie.Hall).FirstOrDefault();
            var time_cheak = db.MoviesTbl.Where(x => x.StartTime >= movie.StartTime && x.StartTime <= movie.EndTime ||
            x.EndTime <= movie.EndTime && x.EndTime >= movie.StartTime).FirstOrDefault();
            if (ModelState.IsValid)
            {

                if (movie.StartTime < localDate || movie.EndTime < localDate ||
                    hall_check != null && time_cheak != null
                   || movie.StartTime >= movie.EndTime)
                {
                    if (movie.StartTime > movie.EndTime)
                    { ModelState.AddModelError("EndTime", "End time can't be before start time !!! "); }
                    if (hall_check != null && time_cheak != null)
                    { ModelState.AddModelError("Hall", "This hall is occupied at this time, please choose another hall!!!"); }
                    if (movie.StartTime < localDate || movie.EndTime < localDate)
                    {
                        ModelState.AddModelError("StartTime", "This date has passed, please select a future date!!!");
                    }


                    return View(movie);
                }
                movie.Poster = ConvertToBytes(file);
                db.MoviesTbl.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(movie);


        }
        // GET: Movie/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoviesTbl movie = db.MoviesTbl.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            db.MoviesTbl.Remove(movie);
            db.SaveChanges();
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idScreen,Name,StartTime,EndTime,Hall,NumberSeats,TicketPrice,AgeMin,Category,Rank,Poster")] MoviesTbl movie)
        {
            DateTime localDate = DateTime.Now;
            HttpPostedFileBase file = Request.Files["ImageData"];
            var hall_check = db.MoviesTbl.Where(x => x.Hall == movie.Hall).FirstOrDefault();
            var time_cheak = db.MoviesTbl.Where(x => x.StartTime >= movie.StartTime && x.StartTime <= movie.EndTime ||
            x.EndTime <= movie.EndTime && x.EndTime >= movie.StartTime).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (movie.StartTime < localDate || movie.EndTime < localDate ||
                   hall_check != null && time_cheak != null
                  || movie.StartTime >= movie.EndTime)
                {
                    if (movie.StartTime > movie.EndTime)
                    { ModelState.AddModelError("EndTime", "End time can't be before start time !!!"); }
                    if (hall_check != null && time_cheak != null)
                    { ModelState.AddModelError("Hall", "This hall is occupied at this time, please choose another hall!!!"); }
                    if (movie.StartTime < localDate || movie.EndTime < localDate)
                    {
                        ModelState.AddModelError("StartTime", "This date has passed, please select a future date!!!");
                    }

                    return View(movie);
                }
                //db.Entry(movie).State = EntityState.Modified;
                movie.Poster = ConvertToBytes(file);
                db.MoviesTbl.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }
        // GET: Movie/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoviesTbl movie = db.MoviesTbl.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            
            db.MoviesTbl.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
            
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }
        public ActionResult RetrieveImage(int id)
        {
            byte[] cover = GetImageFromDataBase(id);
            if (cover != null)
            {
                return File(cover, "image/jpg");
            }
            else
            {
                return null;
            }
        }
        public byte[] GetImageFromDataBase(int Id)
        {
            var q = from temp in db.MoviesTbl where temp.idScreen == Id select temp.Poster;
            byte[] cover = q.First();
            return cover;
        }
    }
}
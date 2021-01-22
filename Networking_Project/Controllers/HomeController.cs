using Networking_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Networking_Project.Controllers
{
    public class HomeController : Controller
    {
        private DATABASEEntities _db = new DATABASEEntities();
        //GET: Register

        public ActionResult Register()
        {
            return View();
        }

        //POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UsersTbl _user)
        {
            if (ModelState.IsValid)
            {
                var check = _db.UsersTbl.FirstOrDefault(s => s.Email == _user.Email);
                if (check == null)
                {
                    _db.UsersTbl.Add(_user);
                    _db.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    MessageBox.Show("Email already exists");
                    return View();
                }


            }
            return View();


        }

        public ActionResult Login()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var data = _db.UsersTbl.Where(s => s.Email.Equals(email) && s.Password.Equals(password)).ToList();
                if (data.Count() > 0)
                {
                    //add session
                    Session["FullName"] = data.FirstOrDefault().FirstName + " " + data.FirstOrDefault().LastName;
                    Session["Email"] = data.FirstOrDefault().Email;
                    Session["idUser"] = data.FirstOrDefault().idUser;
                    string id = Session["idUser"].ToString();
                    if (Session["idUser"] != null)
                    {
                        if (id == "1")
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Users");
                        }
                    }
                   
                }
                else
                {
                    MessageBox.Show("Login failed");
                    return RedirectToAction("Login");
                }
            }
            return View();
        }


        //Logout
        public ActionResult Logout()
        {
            Session.Clear();//remove session
            return RedirectToAction("Login");
        }



       

    }
}
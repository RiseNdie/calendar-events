using MVC_2b.Models;
using MVC_2b.Repositories;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PagedList.Mvc;

namespace MVC_2b.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(); 
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string username, string password)
        {
            UsersRepository userRep = new UsersRepository();
            List<User> userList = userRep.GetAll();
            foreach(User user in userList)
            {
                if(user.Username==username)
                {
                    if(user.Password==password)
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, true);
                        return RedirectToAction("ListUsers");
                    }
                }
            }
            return View();
        }

        public ActionResult ListUsers(string sortOn, string orderBy, string pSortOn, int? page)
        {
            int recordsPerPage = 5;
            if (!page.HasValue)
            {
                page = 1;
                if (string.IsNullOrWhiteSpace(orderBy) || orderBy.Equals("asc"))
                {
                    orderBy = "desc";
                }
                else
                {
                    orderBy = "asc";
                }
            }
            if (!string.IsNullOrWhiteSpace(sortOn) && !sortOn.Equals(pSortOn, StringComparison.CurrentCultureIgnoreCase))
            {
                orderBy = "asc";
            }

            ViewBag.OrderBy = orderBy;
            ViewBag.SortOn = sortOn;

            UsersRepository usersRepository = new UsersRepository();
            List<User> userList = usersRepository.GetAll();

            var list = userList.AsQueryable();

            switch (sortOn)
            {
                case "FirstName":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.FirstName);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.FirstName);
                    }
                    break;
                case "LastName":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.LastName);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.LastName);
                    }
                    break;
                case "Username":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.Username);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.Username);
                    }
                    break;
                default:
                    list = list.OrderBy(p => p.Id);
                    break;
            }

            var finalList = list.ToPagedList(page.Value, recordsPerPage);
            return View(finalList);
        }

        public ActionResult EditUser(int? id)
        {
            if(id>0)
            {
                UsersRepository usersRepository = new UsersRepository();
                User user = usersRepository.GetById(id.Value);
                ViewBag.User = user;
            }
            else
            {
                User newuser = new User();
                ViewBag.User = newuser;
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditUser(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.User = user;
                return View();
            }
            else
            {
                UsersRepository usersRepository = new UsersRepository();
                usersRepository.Save(user);
                return RedirectToAction("ListUsers");
            }
        }

        public ActionResult DeleteUser(int? id)
        {
            if (id > 0)
            {
                UsersRepository usersRepository = new UsersRepository();
                User user = usersRepository.GetById(id.Value);
                usersRepository.Delete(user);
                return RedirectToAction("ListUsers");
            }
            else
            {
                return RedirectToAction("ListUsers");
            }

        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_2b.Repositories;
using MVC_2b.Models;
using PagedList;

namespace MVC_2b.Controllers
{
    [Authorize]
    public class UserGroupsController : Controller
    {

        public ActionResult ListUserGroups(string sortOn, string orderBy, string pSortOn, int? page)
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

            UserGroupsRepository ugr = new UserGroupsRepository();
            List<UserGroup> userGroups = ugr.GetAll();

            var list = userGroups.AsQueryable(); ;

            switch (sortOn)
            {
                case "Name":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.Name);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.Name);
                    }
                    break;
                default:
                    list = list.OrderBy(p => p.Id);
                    break;
            }

            var finalList = list.ToPagedList(page.Value, recordsPerPage);
            return View(finalList);
        }

        public ActionResult EditUserGroup(int? id)
        {
            if (id > 0)
            {
                UserGroupsRepository ugr = new UserGroupsRepository();
                UserGroup userGroup = ugr.GetById(id.Value);
                ViewBag.UserGroup = userGroup;
            }
            else
            {
                UserGroup newuserGroup = new UserGroup();
                ViewBag.UserGroup = newuserGroup;
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditUserGroup(UserGroup userGroup)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserGroup = userGroup;
                return View();
            }
            else
            {
                UserGroupsRepository ugr = new UserGroupsRepository();
                ugr.Save(userGroup);
                return RedirectToAction("ListUserGroups");
            }
        }

        public ActionResult DeleteUserGroup(int? id)
        {
            if (id > 0)
            {
                UserGroupsRepository ugr = new UserGroupsRepository();
                UserGroup userGroup = ugr.GetById(id.Value);
                ugr.Delete(userGroup);
                return RedirectToAction("ListUserGroups");
            }
            else
            {
                return RedirectToAction("ListUserGroups");
            }

        }
    }
}
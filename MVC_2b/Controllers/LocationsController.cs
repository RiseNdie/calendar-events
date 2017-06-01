using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_2b.Models;
using MVC_2b.Repositories;
using PagedList;

namespace MVC_2b.Controllers
{
    [Authorize]
    public class LocationsController : Controller
    {
        //
        // GET: /Location/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult EditLocation(int? id)
        {
            if (id > 0)
            {
                LocationsRepository locationsRepository = new LocationsRepository();
                Location location = locationsRepository.GetById(id.Value);
                ViewBag.Location = location;
            }
            else
            {
                Location newLocation = new Location();
                ViewBag.Location = newLocation;
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditLocation(Location location)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Location = location;
                return View();
            }
            else
            {
                LocationsRepository locationsRepository = new LocationsRepository();
                locationsRepository.Save(location);
                return RedirectToAction("ListLocations");
            }
        }

        public ActionResult DeleteLocation(int? id)
        {
            if (id > 0)
            {
                LocationsRepository locationRepository = new LocationsRepository();
                Location location = locationRepository.GetById(id.Value);
                locationRepository.Delete(location);
                return RedirectToAction("ListLocations");
            }
            else
            {
                return RedirectToAction("ListLocations");
            }

        }

        public ActionResult ListLocations(string sortOn, string orderBy, string pSortOn, int? page)
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

            LocationsRepository locationsRepository = new LocationsRepository();
            List<Location> listLocations = locationsRepository.GetAll();

            var list = listLocations.AsQueryable();

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
                case "Adress":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.Adress);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.Adress);
                    }
                    break;
                case "Floor":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.Floor);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.Floor);
                    }
                    break;
                case "RoomNumber":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.RoomNumber);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.RoomNumber);
                    }
                    break;
                default:
                    list = list.OrderBy(p => p.Id);
                    break;
            }

            var finalList = list.ToPagedList(page.Value, recordsPerPage);
            return View(finalList);
        }
    }
}
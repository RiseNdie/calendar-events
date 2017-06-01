using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MVC_2b.Models;
using MVC_2b.Repositories;
using PagedList;

namespace MVC_2b.Controllers
{
    [Authorize]
    public class ConfRoomsController : Controller
    {
        private static string[] Scopes = {CalendarService.Scope.Calendar};
        private static string ApplicationName = "MVC_Project";
        //
        // GET: /ConfRooms/
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult EditConfRoom(int? id)
        {
            if (id > 0)
            {
                ConfRoomsRepository confRoomsRepository = new ConfRoomsRepository();
                ConfRoom confRoom = confRoomsRepository.GetById(id.Value);
                ViewBag.ConfRoom = confRoom;
                ViewBag.OldName = confRoom.Name;
            }
            else
            {
                ConfRoom newConfRoom = new ConfRoom();
                ViewBag.ConfRoom = newConfRoom;
            }
            return View();
        }

        private UserCredential GoogleAuth()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(@"C:\Users\Marto\Desktop\project\MVC_2b\App_Data\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "marsata95@gmail.com",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            return credential;

        }

        [HttpPost]
        public ActionResult EditConfRoom(ConfRoom confRoom)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ConfRoom = confRoom;
                TempData["OldConfRoomName"] = null;
                return View();
            }
            else
            {
                if (TempData["OldConfRoomName"] == null)
                {
                    GoogleAuth();
                    var NewCalendar = new Calendar();

                    var service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleAuth(),
                        ApplicationName = ApplicationName,
                        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                    });
                    NewCalendar.Summary = confRoom.Name; 
                    var request = service.Calendars.Insert(NewCalendar);
                    var myCalendar = request.Execute();
                }
                else
                {
                    string tmpOldName = TempData["OldConfRoomName"].ToString();
                    GoogleAuth();
                    var service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleAuth(),
                        ApplicationName = ApplicationName,
                        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                    });
                    var getAllCalendarsList = service.CalendarList.List().Execute();
                    var id = getAllCalendarsList.Items.Where(y => y.Summary == tmpOldName).Select(z => z.Id).ToList();
                    Calendar tmpCalendar = service.Calendars.Get(id[0]).Execute();
                    tmpCalendar.Summary = confRoom.Name;
                    var updateCalendar = service.Calendars.Update(tmpCalendar, id[0]).Execute();
                }
                ConfRoomsRepository confRoomsRepository = new ConfRoomsRepository();
                confRoomsRepository.Save(confRoom);
                return RedirectToAction("ListConfRooms");
            }
        }

        public ActionResult DeleteConfRoom(int? id)
        {
            if (id > 0)
            {
                ConfRoomsRepository confRoomsRepository = new ConfRoomsRepository();
                ConfRoom confRoom = confRoomsRepository.GetById(id.Value);
                confRoomsRepository.Delete(confRoom);

                GoogleAuth();

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleAuth(),
                    ApplicationName = ApplicationName,
                    ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                });

                var getAllCalendarsList = service.CalendarList.List().Execute();
                var idDelete = getAllCalendarsList.Items.Where(y => y.Summary == confRoom.Name).Select(z => z.Id).ToList();

                service.Calendars.Delete(idDelete[0]).Execute();

                EventsRepository eventsRepository = new EventsRepository();
                List<MVC_2b.Models.Event> listEvents = eventsRepository.GetAll();

                foreach (var events in listEvents)
                {
                    if (confRoom.Name == events.ConferenceRoomName)
                    {
                        eventsRepository.Delete(events);
                    }
                }


                return RedirectToAction("ListConfRooms");
            }
            else
            {
                return RedirectToAction("ListConfRooms");
            }

        }

        public ActionResult ListConfRooms(string sortOn, string orderBy, string pSortOn, int? page)
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

            ConfRoomsRepository confRoomsRepository = new ConfRoomsRepository();
            List<ConfRoom> listConfRooms = confRoomsRepository.GetAll();

            var list = listConfRooms.AsQueryable();

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
                case "IsFree":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.IsFree);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.IsFree);
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
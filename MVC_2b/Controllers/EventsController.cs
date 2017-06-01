using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using Event = MVC_2b.Models.Event;

namespace MVC_2b.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private static string[] Scopes = { CalendarService.Scope.Calendar };
        private static string ApplicationName = "MVC_Project";
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditEvent(int? id)
        {
            if (id > 0)
            {
                EventsRepository eventsRepository = new EventsRepository();
                Event events = eventsRepository.GetById(id.Value);
                ViewBag.Events = events;
            }
            else
            {
                Event events = new Event();
                ViewBag.Events = events;
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditEvent(Event events)
        {
            ViewBag.WasSomethingWrong = false;
            if (!ModelState.IsValid)
            {
                if (events.Id == 0)
                {
                    ViewBag.Events = new MVC_2b.Models.Event();
                }
                else
                {
                    ViewBag.WasSomethingWrong = true;
                    events.ConferenceRoomName = Request.Form["confRoomsList"];
                    ViewBag.Events = events;
                }
                return View();
            }
            else
            {
                GoogleAuth();
                string selectedConfRoom = Request.Form["confRoomsList"];
                events.ConferenceRoomName = selectedConfRoom;
                if (TempData["oldEventName"] == null && TempData["oldEventRoom"] == null)
                {
                    if (Request.Form["usersList"] != null)
                    {
                        IEnumerable<string> selectedUsers = Request.Form["usersList"].Split(',');
                        events.UsersAttending = null;
                        foreach (var user in selectedUsers)
                        {
                            events.UsersAttending += user + ",";
                        }
                        events.UsersAttending = events.UsersAttending.Remove(events.UsersAttending.Length - 1);
                    }

                    var service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleAuth(),
                        ApplicationName = ApplicationName,
                        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                    });

                    var getAllCalendarsList = service.CalendarList.List().Execute();
                    var id =
                        getAllCalendarsList.Items.Where(y => y.Summary == events.ConferenceRoomName)
                            .Select(z => z.Id)
                            .ToList();

                    string[] stringDate = events.EventDate.ToString().Split('/').ToArray();
                    int startingHour = int.Parse(String.Concat(events.StartHour[0], events.StartHour[1]));
                    int startingMinutes = int.Parse(String.Concat(events.StartHour[3], events.StartHour[4]));
                    int endingHour = int.Parse(String.Concat(events.EndHour[0], events.EndHour[1]));
                    int endingMinutes = int.Parse(String.Concat(events.EndHour[3], events.EndHour[4]));

                    UsersRepository users = new UsersRepository();
                    List<User> allUsersList = users.GetAll();
                    List<EventAttendee> usersAttending = new List<EventAttendee>();
                    EventAttendee attendee;

                    foreach (var user in allUsersList)
                    {
                        string tmpName = user.FirstName + " " + user.LastName;
                        if (events.UsersAttending.Contains(tmpName))
                        {
                            attendee = new EventAttendee()
                            {
                                DisplayName = tmpName,
                                Email = user.Email,
                            };
                            usersAttending.Add(attendee);
                        }
                    }

                    Google.Apis.Calendar.v3.Data.Event newEvent = new Google.Apis.Calendar.v3.Data.Event
                    {
                        Summary = events.Name,
                        Start = new EventDateTime()
                        {
                            DateTime =
                                new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                    int.Parse(stringDate[1]), startingHour, startingMinutes, 0),
                            TimeZone = "Europe/Sofia"
                        },
                        End = new EventDateTime()
                        {
                            DateTime =
                                new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                    int.Parse(stringDate[1]), endingHour, endingMinutes, 0),
                            TimeZone = "Europe/Sofia"
                        },
                        Recurrence = new String[] {"RRULE:FREQ=DAILY;COUNT=1"},
                        Attendees = usersAttending,
                        Reminders = new Google.Apis.Calendar.v3.Data.Event.RemindersData()
                        {
                            UseDefault = false,
                            Overrides = new EventReminder[]
                            {
                                new EventReminder() {Method = "email", Minutes = 60},
                            }
                        }, 
                    };
                    EventsResource.InsertRequest insertEvent = new EventsResource.InsertRequest(service, newEvent,id[0]);
                    insertEvent.SendNotifications = true;
                    insertEvent.Execute();

                }
                else if (TempData["oldEventName"].ToString() != events.Name && TempData["oldEventRoom"].ToString() == events.ConferenceRoomName)

                {
                    if (Request.Form["usersList"] != null)
                    {
                        IEnumerable<string> selectedUsers = Request.Form["usersList"].Split(',');
                        events.UsersAttending = null;
                        foreach (var user in selectedUsers)
                        {
                            events.UsersAttending += user + ",";
                        }
                        events.UsersAttending = events.UsersAttending.Remove(events.UsersAttending.Length - 1);
                    }
                    var service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleAuth(),
                        ApplicationName = ApplicationName,
                        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                    });

                    var getAllCalendarsList = service.CalendarList.List().Execute();
                    var calendarId =
                        getAllCalendarsList.Items.Where(y => y.Summary == events.ConferenceRoomName)
                            .Select(z => z.Id)
                            .ToList();

                    var eventsList = service.Events.List(calendarId[0]).Execute();
                    var eventId =
                        eventsList.Items.Where(y => y.Summary == TempData["oldEventName"].ToString())
                            .Select(z => z.Id)
                            .ToList();

                    string[] stringDate = events.EventDate.ToString().Split('/').ToArray();
                    int startingHour = int.Parse(String.Concat(events.StartHour[0], events.StartHour[1]));
                    int startingMinutes = int.Parse(String.Concat(events.StartHour[3], events.StartHour[4]));
                    int endingHour = int.Parse(String.Concat(events.EndHour[0], events.EndHour[1]));
                    int endingMinutes = int.Parse(String.Concat(events.EndHour[3], events.EndHour[4]));
                    Google.Apis.Calendar.v3.Data.Event tmpEvent =
                        service.Events.Get(calendarId[0], eventId[0]).Execute();
                    tmpEvent.Summary = events.Name;
                    tmpEvent.Start = new EventDateTime()
                    {
                        DateTime =
                            new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                int.Parse(stringDate[1]), startingHour, startingMinutes, 0),
                                TimeZone = "Europe/Sofia"
                    };
                    tmpEvent.End = new EventDateTime()
                    {
                        DateTime =
                            new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                int.Parse(stringDate[1]), endingHour, endingMinutes, 0),
                        TimeZone = "Europe/Sofia"
                    };
                    tmpEvent.Description = "Users Attending: " + events.UsersAttending;


                    var request = service.Events.Update(tmpEvent, calendarId[0], eventId[0]);
                    var updateEvent = request.Execute();

                }
                else
                {
                    if (Request.Form["usersList"] != null)
                    {
                        IEnumerable<string> selectedUsers = Request.Form["usersList"].Split(',');
                        events.UsersAttending = null;
                        foreach (var user in selectedUsers)
                        {
                            events.UsersAttending += user + ",";
                        }
                        events.UsersAttending = events.UsersAttending.Remove(events.UsersAttending.Length - 1);
                    }
                    var service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleAuth(),
                        ApplicationName = ApplicationName,
                        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                    });

                    var getAllCalendarsList = service.CalendarList.List().Execute();
                    var oldCalendarId =
                        getAllCalendarsList.Items.Where(y => y.Summary == TempData["oldEventRoom"].ToString())
                            .Select(z => z.Id)
                            .ToList();
                    var eventsList = service.Events.List(oldCalendarId[0]).Execute();
                    var eventId = eventsList.Items.Where(y => y.Summary == TempData["oldEventName"].ToString())
                        .Select(z => z.Id)
                        .ToList();
                    var newCalendarId =
                        getAllCalendarsList.Items.Where(y => y.Summary == events.ConferenceRoomName)
                            .Select(z => z.Id)
                            .ToList();
                    service.Events.Move(oldCalendarId[0], eventId[0], newCalendarId[0]).Execute();
                    var tmpEvent = service.Events.Get(newCalendarId[0], eventId[0]).Execute();
                    tmpEvent.Summary = events.Name;


                    string[] stringDate = events.EventDate.ToString().Split('/').ToArray();
                    int startingHour = int.Parse(String.Concat(events.StartHour[0], events.StartHour[1]));
                    int startingMinutes = int.Parse(String.Concat(events.StartHour[3], events.StartHour[4]));
                    int endingHour = int.Parse(String.Concat(events.EndHour[0], events.EndHour[1]));
                    int endingMinutes = int.Parse(String.Concat(events.EndHour[3], events.EndHour[4]));
                    tmpEvent.Start = new EventDateTime()
                    {
                        DateTime =
                            new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                int.Parse(stringDate[1]), startingHour, startingMinutes, 0),
                        TimeZone = "Europe/Sofia"
                    };
                    tmpEvent.End = new EventDateTime()
                    {
                        DateTime =
                            new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                                int.Parse(stringDate[1]), endingHour, endingMinutes, 0),
                        TimeZone = "Europe/Sofia"
                    };
                    tmpEvent.Description = "Users Attending: " + events.UsersAttending;
                    var request = service.Events.Update(tmpEvent, newCalendarId[0], eventId[0]).Execute();
                }
                EventsRepository eventsRepository = new EventsRepository();
                eventsRepository.Save(events);
                return RedirectToAction("ListEvents");
            }
        }

        public ActionResult DeleteEvent(int? id)
        {
            if (id > 0)
            {
                EventsRepository eventsRepository = new EventsRepository();
                Event events = eventsRepository.GetById(id.Value);

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleAuth(),
                    ApplicationName = ApplicationName,
                    ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
                });

                var getAllCalendarsList = service.CalendarList.List().Execute();
                var calendarId =
                    getAllCalendarsList.Items.Where(y => y.Summary == events.ConferenceRoomName.ToString())
                        .Select(z => z.Id)
                        .ToList();
                var eventsList = service.Events.List(calendarId[0]).Execute();
                var eventId = eventsList.Items.Where(y => y.Summary == events.Name.ToString())
                    .Select(z => z.Id)
                    .ToList();

                service.Events.Delete(calendarId[0], eventId[0]).Execute();

                eventsRepository.Delete(events);
                return RedirectToAction("ListEvents");
            }
            else
            {
                return RedirectToAction("ListEvents");
            }

        }

        public ActionResult ListEvents(string sortOn, string orderBy, string pSortOn, int? page)
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

            EventsRepository eventsRepository = new EventsRepository();
            List<Event> listEvents = eventsRepository.GetAll();

            var list = listEvents.AsQueryable();

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
                case "ConfRoom":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.ConferenceRoomName);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.ConferenceRoomName);
                    }
                    break;
                case "Date":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.EventDate);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.EventDate);
                    }
                    break;
                case "StartHour":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.StartHour);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.StartHour);
                    }
                    break;
                case "EndHour":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(p => p.EndHour);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.EndHour);
                    }
                    break;
                default:
                    list = list.OrderBy(p => p.Id);
                    break;
            }

            var finalList = list.ToPagedList(page.Value, recordsPerPage);
            return View(finalList);
        }

        private UserCredential GoogleAuth()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(@"C:\Users\Marto\Desktop\Uni\2.1 Сибтийно Програмиране\Проект\project\MVC_2b\App_Data\client_secret.json", FileMode.Open, FileAccess.Read))
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

        public ActionResult CalendarView(int? id)
        {
            EventsRepository events= new EventsRepository();
            Event calEvent = events.GetById(id.Value);

            ViewBag.CurrentEventName = calEvent.Name;

            string[] stringDate = calEvent.EventDate.ToString().Split('/').ToArray();

            DateTime eventDate = new DateTime(int.Parse(stringDate[2].Remove(4)), int.Parse(stringDate[0]),
                int.Parse(stringDate[1]), 0, 0, 0);

            int daysInMonth = DateTime.DaysInMonth(eventDate.Year, eventDate.Month);
            string startingWeekDay = new DateTime(eventDate.Year, eventDate.Month, 1).DayOfWeek.ToString();

            ViewBag.DaysInMonth = daysInMonth;
            ViewBag.Month = eventDate.Month;
            ViewBag.EventDate = eventDate.Day;


            int startingDayInt = 0;

            switch (startingWeekDay)
            {
                case "Monday":
                    startingDayInt = 1;
                    break;
                case"Tuesday":
                    startingDayInt = 2;
                    break;
                case "Wednesday":
                    startingDayInt = 3;
                    break;
                case "Thursday":
                    startingDayInt = 4;
                    break;
                case "Friday":
                    startingDayInt = 5;
                    break;
                case "Saturday":
                    startingDayInt = 6;
                    break;
                case "Sunday":
                    startingDayInt = 7;
                    break;
                default:
                    break;
            }

            ViewBag.StartingDayOfWeek = startingDayInt;
            return View();
        }
    }
}
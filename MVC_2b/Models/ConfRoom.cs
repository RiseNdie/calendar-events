using System.ComponentModel.DataAnnotations;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace MVC_2b.Models
{
    public class ConfRoom : BaseEntityWithId
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        public bool IsFree { get; set; }
        public CalendarService Calendar = new CalendarService(new BaseClientService.Initializer()
    {
        ApiKey = "AIzaSyCs83fQ9iQPWclA4-BsGz0a0ZMmXOn4xnI",
        ApplicationName = "MVC_Project",

    });

        public override string ToString()
        {
            return this.Name;
        }
    }
}
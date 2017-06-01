using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_2b.Models
{
    public class Event:BaseEntityWithId
    {
        [Required (ErrorMessage = "First Name is required!")]
        public string Name { get; set; }
        public string ConferenceRoomName { get; set; }
        [Required(ErrorMessage = "Data Format is Invalid! (Format: MM-dd-yyyy")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime? EventDate { get; set; }

        [RegularExpression(@"^([0-9]|0[0-9]|1[0-9]|2[0-4]):[0-5][0-9]$")]
        [Required(ErrorMessage = "Start hour is invalid! (Format: HH:MM)")]
        public string StartHour { get; set; }
        [RegularExpression(@"^([0-9]|0[0-9]|1[0-9]|2[0-4]):[0-5][0-9]$")]
        [Required (ErrorMessage = "End hour is invalid! (Format: HH:MM)" )]
        public string EndHour { get; set; }
        public string UsersAttending { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_2b.Models
{
    public class Location : BaseEntityWithId
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Adress is required!")]
        public string Adress { get; set; }
        [Required(ErrorMessage = "Floor is required!")]
        public string Floor { get; set; }
        [Required(ErrorMessage = "RoomNumber is required!")]
        public string RoomNumber { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
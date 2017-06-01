using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_2b.Models
{
    public class UserGroup : BaseEntityWithId
    {
        [Required (ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
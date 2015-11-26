using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC5_full_version.Models
{
    public class EmployeeDetails
    {

        public string user_id { get; set; }
        public string UserFullName { get; set; }
        public string UserAddress { get; set; }
        public string user_area { get; set; }
        public string user_dist { get; set; }
        public string UserJobTitle { get; set; }
        public string ExpFrom { get; set; }
        public string ExpTo { get; set; }
        public string CompanyName { get; set; }
        public string ExpWorkDetails { get; set; }
    }
}
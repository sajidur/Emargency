using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC5_full_version.Models
{
    public class Address
    {
     public int user_address_id {get;set;}
    
      public string user_address1 {get;set;}
     
      public string user_zipcode {get;set;}
         public string user_dist {get;set;}
         public string user_area {get;set;}

         public string user_country { get; set; }
    }
}
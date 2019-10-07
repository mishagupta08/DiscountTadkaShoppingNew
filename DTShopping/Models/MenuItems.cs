using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class Category
    {
        public int id { get; set; }
        public int? parent_id { get; set; }
        public string slug { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string ordar { get; set; }
        public bool flag { get; set; }
        public bool status { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public string parentCategoryName { get; set; }
        public List<Category> Childern { get; set; }
    }

    public class Banners
    {
        public int id { get; set; }
        public int banner_id { get; set; }
        public string banner_image { get; set; }
        public string url { get; set; }
        public bool flag { get; set; }        
        public DateTime created { get; set; }
        public DateTime modified { get; set; }        
    }
}
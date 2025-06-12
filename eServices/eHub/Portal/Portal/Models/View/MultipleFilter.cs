using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models.View
{
    public class MultipleFilter
    {
        public string groupOp { get; set; }
        public List<FilterRule> rules { get; set; }
    }

    public class FilterRule
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }

}
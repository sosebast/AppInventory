using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppInventory.Models
{
    public class ChartData
    {
        public string Label { get; set; }

        public string[] Labels { get; set; }

        public int[] Values { get; set; }

        public string[] Filters { get; set; }

        public int SumTotal { get; set; }
    }
}
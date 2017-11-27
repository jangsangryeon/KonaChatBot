using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KonaChatBot.Models
{
    public class PriceTextDlgList
    {
        public int textDlgId { get; set; }
        public int priceDlgId { get; set; }
        public string cardTitle { get; set; }
        public string cardText { get; set; }
        public string cardTextDesc { get; set; }

    }
}
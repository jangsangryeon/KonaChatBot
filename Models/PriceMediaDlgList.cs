using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KonaChatBot.Models
{
    public class PriceMediaDlgList
    {
        public int mediaDlgId { get; set; }
        public int priceDlgId { get; set; }
        public string trim { get; set; }
        public string engine { get; set; }
        public string cardTitle { get; set; }
        public string cardSubTitle { get; set; }
        public string cardText { get; set; }
        public string mediaUrl { get; set; }
        public string btn1Type { get; set; }
        public string btn1Title { get; set; }
        public string btn1Context { get; set; }
        public string btn2Type { get; set; }
        public string btn2Title { get; set; }
        public string btn2Context { get; set; }
        public string btn3Type { get; set; }
        public string btn3Title { get; set; }
        public string btn3Context { get; set; }
        public string btn4Type { get; set; }
        public string btn4Title { get; set; }
        public string btn4Context { get; set; }
        public string cardDivision { get; set; }
        public string cardValue { get; set; }
        public string dlgDesc { get; set; }

    }
}
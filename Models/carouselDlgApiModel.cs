using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecCsChatBotDemo.Models
{
    public class CarouselDlgApiModel
    {

        public int conversationId { get; set; }
        public int dialogCount { get; set; }
        public int status { get; set; }

        public List<CarouselDialog> dialogs { get; set; }

    }

    public class CarouselDialog
    {
        public string type { get; set; }
        public List<Herocard> herocards { get; set; }
        
    }

    public class Herocard
    {
        public string type { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string text { get; set; }
        public string media_url { get; set; }
        public string card_division { get; set; }
        public string card_value { get; set; }
        public List<CarouselDialogButton> buttons { get; set; }
    }

    public class CarouselDialogButton
    {
        public string type { get; set; }
        public string title { get; set; }
        public string values { get; set; }
        
    }
}
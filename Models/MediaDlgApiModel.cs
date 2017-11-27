using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecCsChatBotDemo.Models
{
    public class MediaDlgApiModel
    {

        public int conversationId { get; set; }
        public int dialogCount { get; set; }
        public int status { get; set; }

        public List<MediaDialog> dialogs { get; set; }

    }

    public class MediaDialog
    {
        public string type { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string media_url { get; set; }
        public string card_division { get; set; }
        public string card_value { get; set; }
        public List<MediaDialogButton> buttons { get; set; }
    }

    public class MediaDialogButton
    {
        public string type { get; set; }
        public string title { get; set; }
        public string values { get; set; }
        
    }
}
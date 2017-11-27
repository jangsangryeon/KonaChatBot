using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecCsChatBotDemo.Models
{
    public class TextDlgApiModel
    {

        public int conversationId { get; set; }
        public int dialogCount { get; set; }
        public int status { get; set; }

        public List<TextDialog> dialogs { get; set; }

    }

    public class TextDialog
    {
        public string type { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }
}
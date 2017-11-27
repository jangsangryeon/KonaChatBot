using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecCsChatBotDemo.Models
{
    public class JsonParseModel
    {

        public int conversationId { get; set; }
        public int dialogCount { get; set; }
        public int status { get; set; }

        public List<Dialog> dialogs { get; set; }

    }

    public class Dialog
    {
        public string type { get; set; }
    }
}
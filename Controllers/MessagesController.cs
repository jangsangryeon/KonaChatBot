using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using KonaChatBot.DB;
using KonaChatBot.Models;
using Newtonsoft.Json.Linq;

using System.Configuration;
using System.Web.Configuration;
using KonaChatBot.Dialogs;
using System.IO;

namespace KonaChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public readonly string TEXTDLG = "2";
        public readonly string CARDDLG = "3";
        public readonly string MEDIADLG = "4";
		
		public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/KonaChatBot");
        const string chatBotAppID = "appID";
        public static int appID = Convert.ToInt32(rootWebConfig.ConnectionStrings.ConnectionStrings[chatBotAppID].ToString());

        //config 변수 선언
        static public string[] LUIS_NM = new string[10];        //루이스 이름
        static public string[] LUIS_APP_ID = new string[10];    //루이스 app_id
        static public string LUIS_SUBSCRIPTION = "";            //루이스 구독키
        static public int LUIS_TIME_LIMIT;                      //루이스 타임 체크
        static public string QUOTE = "";                        //견적 url
        static public string TESTDRIVE = "";                    //시승 url
        static public string BOT_ID = "";                       //bot id
        static public string MicrosoftAppId = "";               //app id
        static public string MicrosoftAppPassword = "";         //app password
        static public string LUIS_SCORE_LIMIT = "";             //루이스 점수 체크

        public static JObject Luis = new JObject();             //루이스 json 선언

		public static int sorryMessageCnt = 0;
		public static int chatBotID = 0;

        public static int pagePerCardCnt = 10;
        public static int pageRotationCnt = 0;
        public static int fbLeftCardCnt = 0;
        public static string FB_BEFORE_MENT = "";

        public static List<RelationList> relationList = new List<RelationList>();
        public static string luisId = "";
        public static string luisIntent = "";
        public static string luisEntities = "";
        public static string queryStr = "";
        public static DateTime startTime;

        public static CacheList cacheList = new CacheList();

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            string cashOrgMent = "";

            DbConnect db = new DbConnect();

 			var response = Request.CreateResponse(HttpStatusCode.OK);

			if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                startTime = DateTime.Now;

                //파라메터 호출
                if (LUIS_NM.Count(s => s != null) > 0)
                {
                    //string[] LUIS_NM = new string[10];
                    Array.Clear(LUIS_NM, 0, LUIS_NM.Length);
                }

                if (LUIS_APP_ID.Count(s => s != null) > 0)
                {
                    //string[] LUIS_APP_ID = new string[10];
                    Array.Clear(LUIS_APP_ID, 0, LUIS_APP_ID.Length);
                }
                //Array.Clear(LUIS_APP_ID, 0, 10);
                List<ConfList> confList = db.SelectConfig();

                for (int i = 0; i < confList.Count; i++)
                {
                    switch (confList[i].cnfType)
                    {
                        case "LUIS_APP_ID":
                            LUIS_APP_ID[LUIS_APP_ID.Count(s => s != null)] = confList[i].cnfValue;
                            LUIS_NM[LUIS_NM.Count(s => s != null)] = confList[i].cnfNm;
                            break;
                        case "LUIS_SUBSCRIPTION":
                            LUIS_SUBSCRIPTION = confList[i].cnfValue;
                            break;
                        case "BOT_ID":
                            BOT_ID = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppId":
                            MicrosoftAppId = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppPassword":
                            MicrosoftAppPassword = confList[i].cnfValue;
                            break;
                        case "QUOTE":
                            QUOTE = confList[i].cnfValue;
                            break;
                        case "TESTDRIVE":
                            TESTDRIVE = confList[i].cnfValue;
                            break;
                        case "LUIS_SCORE_LIMIT":
                            LUIS_SCORE_LIMIT = confList[i].cnfValue;
                            break;
                        case "LUIS_TIME_LIMIT":
                            LUIS_TIME_LIMIT = Convert.ToInt32(confList[i].cnfValue);
                            break;
                        default: //미 정의 레코드
                            Debug.WriteLine("*conf type : " + confList[i].cnfType + "* conf value : " + confList[i].cnfValue);
                            break;
                    }
                }

                Debug.WriteLine("* DB conn : " + activity.Type);

                //초기 다이얼로그 호출
                List<DialogList> dlg = db.SelectInitDialog(activity.ChannelId);

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                foreach (DialogList dialogs in dlg)
                {
                    Activity initReply = activity.CreateReply();
                    initReply.Recipient = activity.From;
                    initReply.Type = "message";
                    initReply.Attachments = new List<Attachment>();
                    initReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    Attachment tempAttachment;

                    if (dialogs.dlgType.Equals(CARDDLG))
                    {
                        foreach (CardList tempcard in dialogs.dialogCard)
                        {
                            tempAttachment = getAttachmentFromDialog(tempcard);
                            initReply.Attachments.Add(tempAttachment);
                        }
                    } else
                    {
                        tempAttachment = getAttachmentFromDialog(dialogs);
                        initReply.Attachments.Add(tempAttachment);
                    }
                    await connector.Conversations.SendToConversationAsync(initReply);
                }

                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                Debug.WriteLine("* activity.Type == ActivityTypes.Message ");
                string orgMent = activity.Text;
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                //대화 시작 시간
                startTime = DateTime.Now;
                long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();

                //await Conversation.SendAsync(activity, () => new TestDriveApi(orgMent));
                //await Conversation.SendAsync(activity, () => new RecommendApiDialog());
                //response = Request.CreateResponse(HttpStatusCode.OK);
                //return response;

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////페이스북 위치 값 저장
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //var facebooklocation = activity.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
                //if (facebooklocation != null)
                //{
                //    try
                //    {
                //        var geo = (facebooklocation.Geo as JObject)?.ToObject<GeoCoordinates>();
                //        if (geo != null)
                //        {
                //            HistoryLog("[activity.Text]2 ==>> activity.Text :: location [" + activity.Text + "]");
                //            HistoryLog("[logic start] ==>> userID :: location [" + geo.Longitude + " " + geo.Latitude + "]");
                //            orgMent = "current location:" + geo.Longitude + ":" + geo.Latitude;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Debug.WriteLine("ex : " + ex.ToString());
                //        HistoryLog("[logic start] ==>> userID :: location error [" + activity.Conversation.Id + "]");
                //    }
                //}

                //금칙어 체크
                CardList bannedMsg = db.BannedChk(orgMent);
                Debug.WriteLine("* bannedMsg : " + bannedMsg.cardText);//해당금칙어에 대한 답변

                if (bannedMsg.cardText != null)
                {
                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = bannedMsg.cardText;

                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                    return response;
                }

                queryStr = orgMent;
                //인텐트 엔티티 검출
                //캐시 체크
                cashOrgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);
                cacheList = db.CacheChk(cashOrgMent.Replace(" ",""));                     // 캐시 체크
                

                //캐시에 없을 경우
                if (cacheList.luisIntent == null || cacheList.luisEntities == null)
                {
                    //루이스 호출전 TBL_WORD_CHG_DICT 테이블에서 전처리
                    //for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                    //{
                    //    string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                    //    if (!string.IsNullOrEmpty(chgMsg))
                    //    {
                    //        orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                    //    }
                    //}

                    //루이스 체크
                    cacheList.luisId = GetMultiLUIS(orgMent);

                    float luisScore = (float)Luis["intents"][0]["score"];
                    int luisEntityCount = (int)Luis["entities"].Count();

                    string luisEntities = "";
                    string luisType = "";
                    if (luisScore > Convert.ToDouble(LUIS_SCORE_LIMIT) && luisEntityCount > 0)
                    {
                        for (int i = 0; i < luisEntityCount; i++)
                        {
                            //luisEntities = luisEntities + Luis["entities"][i]["entity"] + ",";
                            luisType = (string)Luis["entities"][i]["type"];
                            luisType = Regex.Split(luisType, "::")[1];
                            luisEntities = luisEntities + luisType + ",";
                        }
                        Debug.WriteLine("luisEntities - 1 : " + luisEntities);
                    }

                    if(luisEntities.Length > 0)
                    {
                        luisEntities = luisEntities.Substring(0, luisEntities.LastIndexOf(","));
                        luisEntities = Regex.Replace(luisEntities, " ", "");

                        //string[] luisEntities_array = new string[10];
                        //luisEntities_array = Regex.Split(luisEntities, ",");
                        //Array.Resize(ref luisEntities_array, luisEntities_array.Count(s => s != null));
                        ////Array.Sort(luisEntities_array);
                        ////Array.Reverse(luisEntities_array);
                        //luisEntities_array = luisEntities_array.OrderByDescending(c => c).ToArray();
                        //foreach (var str in luisEntities_array)
                        //{
                        //    Debug.WriteLine(str.ToString());
                        //}
                        //Debug.WriteLine("luisEntities_array = " + luisEntities_array);

                        luisEntities = db.SelectArray(luisEntities);

                        Debug.WriteLine("luisEntities - 2 : " + luisEntities);

                        cacheList.luisIntent = (string)Luis["intents"][0]["intent"];
                        cacheList.luisEntities = luisEntities;
                    }
                    
                }
                luisId = cacheList.luisId;
                luisIntent = cacheList.luisIntent;
                luisEntities = cacheList.luisEntities;

                
                //다이얼로그 호출
                await Conversation.SendAsync(activity, () => new RootDialog(cacheList.luisId, cacheList.luisIntent, cacheList.luisEntities));
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return response;
            
        }

        private Attachment getAttachmentFromDialog(DialogList dlg)
        {
            Attachment returnAttachment = new Attachment();

            if (dlg.dlgType.Equals(TEXTDLG))
            {
                HeroCard plCard = new HeroCard()
                {
                    Title = dlg.cardTitle,
                    Text = dlg.cardText
                };
                returnAttachment = plCard.ToAttachment();
            }
            else if (dlg.dlgType.Equals(MEDIADLG))
            {
                List<CardImage> cardImages = new List<CardImage>();
                List<CardAction> cardButtons = new List<CardAction>();

                if (dlg.mediaUrl != null)
                {
                    cardImages.Add(new CardImage(url: dlg.mediaUrl));
                }

                if (dlg.btn1Type != null)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = dlg.btn1Context,
                        Type = dlg.btn1Type,
                        Title = dlg.btn1Title
                    };

                    cardButtons.Add(plButton);
                }

                if (dlg.btn2Type != null)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = dlg.btn2Context,
                        Type = dlg.btn2Type,
                        Title = dlg.btn2Title
                    };

                    cardButtons.Add(plButton);
                }

                if (dlg.btn3Type != null)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = dlg.btn3Context,
                        Type = dlg.btn3Type,
                        Title = dlg.btn3Title
                    };

                    cardButtons.Add(plButton);
                }

                if (dlg.btn4Type != null)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = dlg.btn4Context,
                        Type = dlg.btn4Type,
                        Title = dlg.btn4Title
                    };

                    cardButtons.Add(plButton);
                }

                HeroCard plCard = new HeroCard()
                {
                    Title = dlg.cardTitle,
                    Text = dlg.cardTitle,
                    Images = cardImages,
                    Buttons = cardButtons
                };
                returnAttachment = plCard.ToAttachment();
            }
            else
            {
                Debug.WriteLine("Dialog Type Error : " + dlg.dlgType);
            }
            return returnAttachment;
        }

        private Attachment getAttachmentFromDialog(CardList card)
        {
            Attachment returnAttachment = new Attachment();

            List<CardImage> cardImages = new List<CardImage>();
            List<CardAction> cardButtons = new List<CardAction>();

            if (card.imgUrl != null)
            {
                cardImages.Add(new CardImage(url: card.imgUrl));
            }

            if (card.btn1Type != null)
            {
                CardAction plButton = new CardAction()
                {
                    Value = card.btn1Context,
                    Type = card.btn1Type,
                    Title = card.btn1Title
                };

                cardButtons.Add(plButton);
            }

            if (card.btn2Type != null)
            {
                CardAction plButton = new CardAction()
                {
                    Value = card.btn2Context,
                    Type = card.btn2Type,
                    Title = card.btn2Title
                };

                cardButtons.Add(plButton);
            }

            if (card.btn3Type != null)
            {
                CardAction plButton = new CardAction()
                {
                    Value = card.btn3Context,
                    Type = card.btn3Type,
                    Title = card.btn3Title
                };

                cardButtons.Add(plButton);
            }

            if (card.btn4Type != null)
            {
                CardAction plButton = new CardAction()
                {
                    Value = card.btn4Context,
                    Type = card.btn4Type,
                    Title = card.btn4Title
                };

                cardButtons.Add(plButton);
            }

            HeroCard plCard = new HeroCard()
            {
                Title = card.cardTitle,
                Text = card.cardTitle,
                Images = cardImages,
                Buttons = cardButtons
            };
            returnAttachment = plCard.ToAttachment();

            return returnAttachment;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private static async Task<JObject> GetIntentFromBotLUIS(string luis_app_id, string luis_subscription, string query)
        {

            JObject jsonObj = new JObject();

            query = Uri.EscapeDataString(query);

            string url = string.Format("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);
            Debug.WriteLine("url = " + url);
            using (HttpClient client = new HttpClient())
            {

                HttpResponseMessage msg = await client.GetAsync(url);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();

                if (jsonObj["entities"].Count() != 0 && (float)jsonObj["intents"][0]["score"] > 0.3)
                {
                    //break;
                }

            }
            return jsonObj;
        }

        public static void HistoryLog(String strMsg)
        {
            try
            {
                string m_strLogPrefix = AppDomain.CurrentDomain.BaseDirectory + @"LOG\";
                string m_strLogExt = @".LOG";
                DateTime dtNow = DateTime.Now;
                string strDate = dtNow.ToString("yyyy-MM-dd");
                string strPath = String.Format("{0}{1}{2}", m_strLogPrefix, strDate, m_strLogExt);
                string strDir = Path.GetDirectoryName(strPath);
                DirectoryInfo diDir = new DirectoryInfo(strDir);

                if (!diDir.Exists)
                {
                    diDir.Create();
                    diDir = new DirectoryInfo(strDir);
                }

                if (diDir.Exists)
                {
                    System.IO.StreamWriter swStream = File.AppendText(strPath);
                    string strLog = String.Format("{0}: {1}", dtNow.ToString("MM/dd/yyyy hh:mm:ss.fff"), strMsg);
                    swStream.WriteLine(strLog);
                    swStream.Close(); ;
                }
            }
            catch (System.Exception e)
            {
                HistoryLog(e.Message);
                Debug.WriteLine(e.Message);
            }
        }

        private String GetMultiLUIS(string query)
        {
            


            Debug.WriteLine("LUIS NM : "+ LUIS_NM.Count());
            //for (int i = 0; i < LUIS_NM.Count(); i++)
            //{
            //    Debug.WriteLine("LUIS_NM [ " + i + " ] ====>>>> " + LUIS_NM[i].ToString());
            //}

            int MAX = LUIS_APP_ID.Count(s => s != null);
            Array.Resize(ref LUIS_APP_ID, MAX);
            Array.Resize(ref LUIS_NM, MAX);

            String[] returnLuisName = new string[MAX];
            JObject[] Luis_before = new JObject[MAX];
            string LuisName = "";
            List<string[]> textList = new List<string[]>(MAX);


            for (int i = 0; i < MAX; i++)
            {
                //textList.Add(LUIS_APP_ID[i] +"|"+ LUIS_SUBSCRIPTION + "|" + query);
                textList.Add(new string[] { LUIS_NM[i], LUIS_APP_ID[i], LUIS_SUBSCRIPTION, query });

            }

            //병렬처리 시간 체크
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Parallel.For(0, MAX, new ParallelOptions { MaxDegreeOfParallelism = MAX }, async async =>
            {

                //var task_luis = Task<JObject>.Run(async () => await GetIntentFromBotLUIS(textList[async][1], textList[async][2], textList[async][3]));
                var task_luis = Task<JObject>.Run(() => GetIntentFromBotLUIS(textList[async][1], textList[async][2], textList[async][3]));

                try
                {
                    //task_luis.GetAwaiter();
                    //task_luis.Wait();
                    Task.WaitAll(task_luis);
                    Debug.WriteLine(async + " -- task1 이름 = " + textList[async][0]);
                    Debug.WriteLine(async + " -- task1 결과 = " + task_luis.Result["topScoringIntent"]["score"]);

                    //최저 스코어 체크
                    //if ((float)task_luis.Result["topScoringIntent"]["score"] > Convert.ToDouble(LUIS_SCORE_LIMIT))
                    //{
                    //    //이전 json 확인
                    //    if (Luis_before.Count != 0)
                    //    {
                    //        //이전 topScoringIntent 비교
                    //        if ((float)task_luis.Result["topScoringIntent"]["score"] > (float)Luis_before["topScoringIntent"]["score"])
                    //        {
                    //            Debug.WriteLine("task_luis = " + (float)task_luis.Result["topScoringIntent"]["score"]);
                    //            Debug.WriteLine("Luis_before = " + (float)Luis_before["topScoringIntent"]["score"]);
                    //            Luis = Luis_before;
                    //            returnLuisName = textList[async][0];
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Luis_before = task_luis.Result;
                    //        returnLuisName = textList[async][0];
                    //    }
                    //}

                    Luis_before[async] = task_luis.Result;
                    returnLuisName[async] = textList[async][0];

                }
                catch (AggregateException e)
                {
                    Debug.WriteLine("error = " + e.Message);
                }

            });
            
            watch.Stop();
            //Luis = Luis_before;

            try
            {
                for (int i = 0; i < MAX; i++)
                {
                    Debug.WriteLine("i = " + Luis_before[i]["topScoringIntent"]["score"]);
                    Debug.WriteLine("ij = " + returnLuisName[i]);

                    //entities 가 없을때 score 0으로 설정
                    if ((int)Luis_before[i]["entities"].Count() < 1)
                    {
                        Luis_before[i]["topScoringIntent"]["score"] = 0;
                    }
                    
                    if (i == 0)
                    {
                        Luis = Luis_before[0];
                        LuisName = returnLuisName[0];
                    } else
                    {

                        if (((float)Luis["topScoringIntent"]["score"] < (float)Luis_before[i]["topScoringIntent"]["score"]) && Luis_before[i]["topScoringIntent"]["intent"].ToString() != "None")
                        {
                            if (Luis_before[i]["entities"].Count() > 0)
                            {
                                Luis = Luis_before[i];
                                LuisName = returnLuisName[i];
                            }
                        }                      
                    }

                }
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.WriteLine("error = " + e.Message);
            }

            //Debug.WriteLine(watch.Elapsed.ToString());
            //Debug.WriteLine("Luis_SCORE = " + (float)Luis["topScoringIntent"]["score"]);
            //Debug.WriteLine("Luis_nm = " + LuisName);
            //Debug.WriteLine("Luis = " + Luis);
            return LuisName;
        }
    }
}
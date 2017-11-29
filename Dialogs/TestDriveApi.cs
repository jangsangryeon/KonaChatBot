using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using KonaChatBot.DB;
using KonaChatBot.Models;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.ConnectorEx;


namespace KonaChatBot.Dialogs
{
    [Serializable]
    public class TestDriveApi : IDialog<object>
    {

        private string queryStr;

        public TestDriveApi(string queryStr)
        {
            this.queryStr = queryStr;
        }

        //public Task StartAsync(IDialogContext context)
        //{
        //    context.Wait(MessageReceivedAsync);

        //    return Task.CompletedTask;
        //}

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            //지역변수 인텐트 엔티티로 결과값 조회
            // Db
            DbConnect db = new DbConnect();

            // 변수 선언
            string replaceStr = "";
            //현재위치사용승인
            var activity = (Activity)await result;
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            queryStr = MessagesController.queryStr;

            var facebooklocation = activity.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
            if (facebooklocation != null)
            {
                try
                {
                    var geo = (facebooklocation.Geo as JObject)?.ToObject<GeoCoordinates>();
                    if (geo != null)
                    {
                        //HistoryLog("[activity.Text]2 ==>> activity.Text :: location [" + activity.Text + "]");
                        //HistoryLog("[logic start] ==>> userID :: location [" + geo.Longitude + " " + geo.Latitude + "]");
                        queryStr = "current location:" + geo.Longitude + ":" + geo.Latitude;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ex : " + ex.ToString());
                    //HistoryLog("[logic start] ==>> userID :: location error [" + activity.Conversation.Id + "]");
                }
            }

            if (queryStr.Contains("current location"))
            {
                if (activity.ChannelId == "facebook" && facebooklocation == null)
                {
                    //testDriveWhereStr = "test drive center region=seoul,current location=current location,query=Approve your current location";
                    /////////////////////////////////////////////////////////////////////////////////
                    //facebook location start
                    /////////////////////////////////////////////////////////////////////////////////
                    //HistoryLog("[location test] conversation id :: [" + activity.Conversation.Id + "] start");

                    Activity reply_option = activity.CreateReply();

                    reply_option.ChannelData = new FacebookMessage
                    (
                        text: "나와 함께 당신의 위치를 공유하십시오.",
                        quickReplies: new List<FacebookQuickReply>
                        {
                                            // if content_type is location, title and payload are not used
                                            // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
                                            // for more information.
                                            new FacebookQuickReply(
                                                contentType: FacebookQuickReply.ContentTypes.Location,
                                                title: default(string),
                                                payload: default(string)
                                            )
                        }
                    );
                    var reply_facebook = await connector.Conversations.SendToConversationAsync(reply_option);
                    //response = Request.CreateResponse(HttpStatusCode.OK);
                    //return response;
                    /////////////////////////////////////////////////////////////////////////////////
                    //facebook location end
                    /////////////////////////////////////////////////////////////////////////////////
                }
                else
                {
                    if (!queryStr.Contains(':'))
                    {
                        //첫번쨰 메세지 출력 x
                        //response = Request.CreateResponse(HttpStatusCode.OK);
                        //return response;
                    }
                    else
                    {
                        //위도경도에 따른 값 출력
                        try
                        {
                            string regionStr = "";
                            string location = queryStr;
                            //테스트용
                            //string location = "129.0929788:35.2686635";
                            string[] location_result = location.Split(':');
                            regionStr = db.LocationValue(location_result[1], location_result[2]);

                            queryStr = regionStr + " 시승센터";
                        }
                        catch
                        {
                            queryStr = "서울 시승센터";
                        }
                    }
                }
            }

            //엔티티 type, 엔티티 value 추출
            List<TestDriveList_API> SelectTestDriveList_API = db.SelectTestDriveList_API(queryStr);
            //다이얼로그 ID 추출
            List<TestDriveList_API_DLG> SelectTestDriveList_API_DLG = db.SelectTestDriveList_API_DLG(SelectTestDriveList_API[0].entityType);

            //paging start
            //String beforeMent = "";
            //int facebookpagecount = 1;
            //int fbLeftCardCnt = 0;

            //if (context.ConversationData.TryGetValue("commonBeforeQustion", out beforeMent))
            //{
            //    if (beforeMent.Equals(queryStr) && activity.ChannelId.Equals("facebook"))
            //    {
            //        if (context.ConversationData.TryGetValue("facebookPageCount", out facebookpagecount))
            //        {
            //            facebookpagecount++;
            //        }
            //        context.ConversationData.SetValue("facebookPageCount", facebookpagecount);
            //        fbLeftCardCnt++;
            //    }
            //}
            //paging end

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            for (int td = 0; td < SelectTestDriveList_API_DLG.Count; td++)
            {
                if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(1))
                {
                    Debug.WriteLine("GUBUN 1");
                }
                else if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(2))
                {
                    Debug.WriteLine("GUBUN 2");
                    //텍스트 값 가져오기
                    List<TestDriveList_API_DLG_TEXT> SelectTestDriveList_API_DLG_TEXT = db.SelectTestDriveList_API_DLG_TEXT(SelectTestDriveList_API_DLG[td].testdrive_dlg_id);
                    //예외처리
                    if (SelectTestDriveList_API_DLG_TEXT.Count != 0)
                    {
                        replaceStr = SelectTestDriveList_API_DLG_TEXT[0].testdrive_card_text;
                    }
                    else
                    {
                        replaceStr = "";
                    }

                }
                else if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(3))
                {
                    Debug.WriteLine("GUBUN 3");
                }
                else
                {
                    Debug.WriteLine("GUBUN 4");
                    //미디어 값 가져오기
                    List<TestDriveList_API_DLG_MEDIA> SelectTestDriveList_API_DLG_MEDIA = db.SelectTestDriveList_API_DLG_MEDIA(SelectTestDriveList_API_DLG[td].testdrive_dlg_id, SelectTestDriveList_API[0].entityValue);

                    for (int i = 0; i < SelectTestDriveList_API_DLG_MEDIA.Count; i++)
                    {
                        //CardImage 입력
                        CardImage cardImage = new CardImage()
                        {
                            Url = SelectTestDriveList_API_DLG_MEDIA[i].media_url
                        };

                        //CardAction 입력
                        List<CardAction> cardButtons = new List<CardAction>();

                        if (SelectTestDriveList_API_DLG_MEDIA[i].btn_1_context.Length != 0)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_context,
                                Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_type,
                                Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_title
                            };
                            cardButtons.Add(plButton);
                        }

                        if (SelectTestDriveList_API_DLG_MEDIA[i].btn_2_context.Length != 0)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_context,
                                Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_type,
                                Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_title
                            };
                            cardButtons.Add(plButton);
                        }

                        if (SelectTestDriveList_API_DLG_MEDIA[i].btn_3_context.Length != 0)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_context,
                                Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_type,
                                Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_title
                            };
                            cardButtons.Add(plButton);
                        }

                        //맵에서 text로 출력되는 주소값 치환
                        if (!string.IsNullOrEmpty(replaceStr))
                        {
                            reply.Text = replaceStr.Replace("CALL(지점 주소)", SelectTestDriveList_API_DLG_MEDIA[i].address);
                            await context.PostAsync(reply);
                            replaceStr = "";
                            reply.Text = "";
                        }


                        reply.Attachments.Add(GetHeroCard(SelectTestDriveList_API_DLG_MEDIA[i].card_title, SelectTestDriveList_API_DLG_MEDIA[i].card_subtitle, SelectTestDriveList_API_DLG_MEDIA[i].card_text, cardImage, cardButtons));
                    }

                    await context.PostAsync(reply);
                    //reply 초기화
                    reply.Attachments.Clear();
                }

            }
            //context.Wait(this.MessageReceivedAsync);
            context.Done("");
        }


        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, /*CardAction cardAction*/ List<CardAction> buttons)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = buttons,
            };

            return heroCard.ToAttachment();
        }

    }
}
namespace KonaChatBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using DB;
    using System.Diagnostics;
    using Models;
    using System.Collections.Generic;

    [Serializable]
    public class RecommendApiDialog : IDialog<object>
    {
        DbConnect db = new DbConnect();
        CardImage cardImage = new CardImage();
        List<CardAction> cardButtons = new List<CardAction>();
        string use = "", important = "", age = "", gender = "";

        public async Task StartAsync(IDialogContext context)
        {
            //string use = "", important = "", age = "", gender = "";

            var activity = context.Activity;
            var reply = context.MakeMessage();

            List<RecommendConfirm> rc = db.SelectedRecommendConfirm;            

            foreach (RecommendConfirm temprc in rc)
            {
                if (!temprc.KEYWORDGROUP.Equals(""))
                {
                    
                    if (temprc.KEYWORDGROUP.Equals(RecommendContextConstants.Use) && temprc.KEYWORD.Equals("기타 용도"))
                    {
                        //if (context.ConversationData.TryGetValue(RecommendContextConstants.Use, out use))
                        //{
                        //    if (context.ConversationData.TryGetValue(RecommendContextConstants.Important, out important))
                        //    {
                        //        temprc.KEYWORDGROUP = RecommendContextConstants.Gender;
                        //    } else
                        //    {
                        //        temprc.KEYWORDGROUP = RecommendContextConstants.Important;
                        //    }
                        //}

                        context.ConversationData.SetValue(temprc.KEYWORDGROUP, temprc.KEYWORD);
                        break;
                        
                    }
                    else
                    {
                        Debug.WriteLine("temprc.KEYWORDGROUP.Equals(RecommendContextConstants.Important) = " + temprc.KEYWORDGROUP.Equals(RecommendContextConstants.Important));
                        if (temprc.KEYWORDGROUP.Equals(RecommendContextConstants.Important) && temprc.KEYWORD.Equals("기타"))
                        {
                            temprc.KEYWORDGROUP = RecommendContextConstants.Gender;
                        }

                        context.ConversationData.SetValue(temprc.KEYWORDGROUP, temprc.KEYWORD);
                    }                    
                }
            }

            if (!context.ConversationData.TryGetValue(RecommendContextConstants.Use, out use))
            {
                //await context.PostAsync($"use 요청");
                reply.Attachments.Add(getRecommendDialog(1));
                await context.PostAsync(reply);
            }
            else if (!context.ConversationData.TryGetValue(RecommendContextConstants.Important, out important))
            {
                //await context.PostAsync($"important 요청");
                reply.Attachments.Add(getRecommendDialog(2));
                await context.PostAsync(reply);
            }
            else if ((!context.ConversationData.TryGetValue(RecommendContextConstants.Age, out age)) && (!context.ConversationData.TryGetValue(RecommendContextConstants.Gender, out gender)))
            {
                //await context.PostAsync($"age gender 요청");
                reply.Attachments.Add(getRecommendDialog(3));
                await context.PostAsync(reply);
            }
            else
            {
                context.ConversationData.TryGetValue(RecommendContextConstants.Use, out use);
                context.ConversationData.TryGetValue(RecommendContextConstants.Important, out important);
                context.ConversationData.TryGetValue(RecommendContextConstants.Age, out age);
                context.ConversationData.TryGetValue(RecommendContextConstants.Gender, out gender);
                reply.Attachments.Add(getRecommendDialog(4));

                //초기화
                context.ConversationData.Clear();
                await context.PostAsync(reply);
            }
            DateTime endTime = DateTime.Now;
            Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - MessagesController.startTime).Milliseconds));
            Debug.WriteLine("* activity.Type : " + activity.Type);
            Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
            Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);

            int dbResult = db.insertUserQuery(MessagesController.queryStr, MessagesController.luisIntent, MessagesController.luisEntities, "0", MessagesController.luisId, 'H', 0);
            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

            if (db.insertHistory(activity.Conversation.Id, MessagesController.queryStr, MessagesController.relationList[0].dlgId.ToString(), activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds), 0) > 0)
            {
                Debug.WriteLine("HISTORY RESULT SUCCESS");
                //HistoryLog("HISTORY RESULT SUCCESS");
            }
            else
            {
                Debug.WriteLine("HISTORY RESULT SUCCESS");
                //HistoryLog("HISTORY RESULT FAIL");
            }
            context.Done<IMessageActivity>(null);
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

        private Attachment getRecommendDialog(int rcmdDlgId)
        {
            //MEDIA 데이터 추출
            Attachment returnAttachment = new Attachment();

            List<Recommend_DLG_MEDIA> SelectRecommend_DLG_MEDIA = db.SelectRecommend_DLG_MEDIA(rcmdDlgId);
            if (rcmdDlgId != 4)
            {
                for (int i = 0; i < SelectRecommend_DLG_MEDIA.Count; i++)
                {
                    //CardImage 입력
                    cardImage = new CardImage()
                    {
                        Url = SelectRecommend_DLG_MEDIA[i].media_url
                    };

                    if (SelectRecommend_DLG_MEDIA[i].btn_1_context.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[i].btn_1_context,
                            Type = SelectRecommend_DLG_MEDIA[i].btn_1_type,
                            Title = SelectRecommend_DLG_MEDIA[i].btn_1_title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (SelectRecommend_DLG_MEDIA[i].btn_2_context.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[i].btn_2_context,
                            Type = SelectRecommend_DLG_MEDIA[i].btn_2_type,
                            Title = SelectRecommend_DLG_MEDIA[i].btn_2_title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (SelectRecommend_DLG_MEDIA[i].btn_3_context.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[i].btn_3_context,
                            Type = SelectRecommend_DLG_MEDIA[i].btn_3_type,
                            Title = SelectRecommend_DLG_MEDIA[i].btn_3_title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (SelectRecommend_DLG_MEDIA[i].btn_4_context.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[i].btn_4_context,
                            Type = SelectRecommend_DLG_MEDIA[i].btn_4_type,
                            Title = SelectRecommend_DLG_MEDIA[i].btn_4_title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (SelectRecommend_DLG_MEDIA[i].btn_5_context.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[i].btn_5_context,
                            Type = SelectRecommend_DLG_MEDIA[i].btn_5_type,
                            Title = SelectRecommend_DLG_MEDIA[i].btn_5_title
                        };
                        cardButtons.Add(plButton);
                    }

                    //message.Attachments.Add(GetHeroCard(SelectRecommend_DLG_MEDIA[i].card_title, "", SelectRecommend_DLG_MEDIA[i].card_text, cardImage, cardButtons));
                    returnAttachment = GetHeroCard(SelectRecommend_DLG_MEDIA[i].card_title, "", SelectRecommend_DLG_MEDIA[i].card_text, cardImage, cardButtons);
                }
            }else
            {
                string domainURL = "https://bottest.hyundai.com";

                List<RecommendList> RecommendList = db.SelectedRecommendList(use, important, gender, age);
                RecommendList recommend = new RecommendList();

                for (var i = 0; i < RecommendList.Count; i++)
                {
                    string main_color_view = "";
                    string main_color_view_nm = "";

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_1))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM1 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_2))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_2 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM2 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_3))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_3 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM3 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_4))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_4 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM4 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_5))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_5 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM5 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_6))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_6 + "/00001.jpg" + "@";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM6 + "@";
                    };

                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_7))
                    {
                        main_color_view += domainURL + "/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_7 + "/00001.jpg";
                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM7 + "@";
                    };

                    main_color_view = main_color_view.TrimEnd('@');
                    main_color_view_nm = main_color_view_nm.TrimEnd('@');

                    var subtitle = RecommendList[i].TRIM_DETAIL + "|" + "가격: " + RecommendList[i].TRIM_DETAIL_PRICE + "|" +
                                    main_color_view + "|" +
                                    RecommendList[i].OPTION_1_IMG_URL + "|" +
                                    RecommendList[i].OPTION_1 + "|" +
                                    RecommendList[i].OPTION_2_IMG_URL + "|" +
                                    RecommendList[i].OPTION_2 + "|" +
                                    RecommendList[i].OPTION_3_IMG_URL + "|" +
                                    RecommendList[i].OPTION_3 + "|" +
                                    RecommendList[i].OPTION_4_IMG_URL + "|" +
                                    RecommendList[i].OPTION_4 + "|" +
                                    RecommendList[i].OPTION_5_IMG_URL + "|" +
                                    RecommendList[i].OPTION_5 + "|" +
                                    main_color_view_nm;

                    if (SelectRecommend_DLG_MEDIA[0].btn_1_title.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[0].btn_1_context,
                            Type = SelectRecommend_DLG_MEDIA[0].btn_1_type,
                            Title = SelectRecommend_DLG_MEDIA[0].btn_1_title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (SelectRecommend_DLG_MEDIA[0].btn_2_title.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = SelectRecommend_DLG_MEDIA[0].btn_2_context,
                            Type = SelectRecommend_DLG_MEDIA[0].btn_2_type,
                            Title = SelectRecommend_DLG_MEDIA[0].btn_2_title
                        };
                        cardButtons.Add(plButton);
                    }                    
                    returnAttachment = GetHeroCard("trim", subtitle, "", cardImage, cardButtons);
                }
            }
            return returnAttachment;
        }

    }
}
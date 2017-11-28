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
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string use="", important="", age="", gender="";

            DbConnect db = new DbConnect();

            //String keywordGroup = db.SelectedRecommendConfirm(message.Text);
            RecommendConfirm rc = db.SelectedRecommendConfirm(message.Text);

            var reply = context.MakeMessage();
            string domainURL = "https://bottest.hyundai.com";

            //테스트용
            //if (message.Text.Equals("20~30대 여성"))
            //{
            //    message.Text = "여성";
            //}

            //message 분류에 맞게 context.ConversationData 에 SetValue
            if (rc.KEYWORDGROUP != "")
            {
                //2depth 기타와 3depth 기타 구분
                if (rc.KEYWORD.Equals("기타"))
                {
                    context.ConversationData.TryGetValue("Important", out important);
                    if (!string.IsNullOrEmpty(important))
                    {
                        context.ConversationData.SetValue("Gender", "기타");
                        context.ConversationData.SetValue("Age", "기타");
                        context.ConversationData.TryGetValue("Gender", out gender);
                        context.ConversationData.TryGetValue("Age", out age);

                    }
                    else
                    {
                        context.ConversationData.SetValue(rc.KEYWORDGROUP, rc.KEYWORD);
                    }                    
                }
                else
                {
                    context.ConversationData.SetValue(rc.KEYWORDGROUP, rc.KEYWORD);
                }
            }

            //다이얼로그 아이디 추출
            int rcmdDlgId;
            context.ConversationData.TryGetValue("Use", out use);
            context.ConversationData.TryGetValue("Important", out important);
            context.ConversationData.TryGetValue("Age", out age);
            context.ConversationData.TryGetValue("Gender", out gender);
            if (!string.IsNullOrEmpty(gender) || !string.IsNullOrEmpty(age))
            //if(context.ConversationData.TryGetValue("Age", out age) || context.ConversationData.TryGetValue("Gender", out gender))
            {
                //마지막 다이얼로그 아이디 입력
                rcmdDlgId = 4;
            } else
            {
                rcmdDlgId = db.SelectedRecommendDlgId(rc.KEYWORD);
            }
            
            //MEDIA 데이터 추출
            List<Recommend_DLG_MEDIA> SelectRecommend_DLG_MEDIA = db.SelectRecommend_DLG_MEDIA(rcmdDlgId);

            CardImage cardImage = new CardImage();
            List<CardAction> cardButtons = new List<CardAction>();

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

                reply.Attachments.Add(GetHeroCard(SelectRecommend_DLG_MEDIA[i].card_title, "", SelectRecommend_DLG_MEDIA[i].card_text, cardImage, cardButtons));
            }

            //추천 다이얼로그 출력
            if (!string.IsNullOrEmpty(use) && !string.IsNullOrEmpty(important) && (!string.IsNullOrEmpty(age)) || (!string.IsNullOrEmpty(gender)))
            {
                Debug.WriteLine("use, important, age, gender = " + use + "|||" + important + "|||" + age + "|||" + gender);
                //이전 reply 초기화
                reply.Attachments.Clear();
                cardButtons.Clear();

                List<RecommendList> RecommendList = db.SelectedRecommendList(use, important, gender, "");
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
               
                    reply.Attachments.Add(GetHeroCard("trim", subtitle, "", cardImage, cardButtons));
                    context.ConversationData.SetValue("Use", "");
                    context.ConversationData.SetValue("Important", "");
                    context.ConversationData.SetValue("Gender", "");
                    context.ConversationData.SetValue("Age", "");
                    context.ConversationData.TryGetValue("Use", out use);
                    context.ConversationData.TryGetValue("Important", out important);
                    context.ConversationData.TryGetValue("Gender", out gender);
                    context.ConversationData.TryGetValue("Age", out age);
                }
                
            }
            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {

            }
            catch (TooManyAttemptsException)
            {
            }

            context.Wait(this.MessageReceivedAsync);
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
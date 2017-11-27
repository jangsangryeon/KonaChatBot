using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using KonaChatBot.DB;
using KonaChatBot.Models;
using System.Diagnostics;

namespace KonaChatBot.Dialogs
{
    [Serializable]
    public class CommonDialog : IDialog<object>
    {
        private List<RelationList> relationList;
        private String channel;
        private String orgMent;
        private readonly string TEXTDLG = "2";
        private readonly string CARDDLG = "3";
        private readonly string MEDIADLG = "4";
        private readonly int MAXFACEBOOKCARDS = 10;

        public CommonDialog(List<RelationList> relationList, String channel, String orgMent)
        {
            this.relationList = relationList;
            this.orgMent = orgMent;
            this.channel = channel;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            String beforeMent = "";
            int facebookpagecount = 1;
            int fbLeftCardCnt = 0;

            if (context.ConversationData.TryGetValue("commonBeforeQustion", out beforeMent))
            {
                if (beforeMent.Equals(orgMent) && channel.Equals("facebook"))
                {
                    if (context.ConversationData.TryGetValue("facebookPageCount", out facebookpagecount))
                    {
                        facebookpagecount++;
                    }
                    context.ConversationData.SetValue("facebookPageCount", facebookpagecount);
                    fbLeftCardCnt++;
                }
            }

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            DbConnect db = new DbConnect();

            for (int m = 0; m < relationList.Count; m++)
            {
                DialogList dlg = db.SelectDialog(relationList[m].dlgId);

                Attachment tempAttachment = new Attachment();

                if (dlg.dlgType.Equals(CARDDLG))
                {
                    foreach (CardList tempcard in dlg.dialogCard)
                    {
                        if (context.ConversationData.TryGetValue("facebookPageCount", out facebookpagecount))
                        {
                            if (Int32.Parse(tempcard.card_order_no) > (MAXFACEBOOKCARDS * facebookpagecount) && Int32.Parse(tempcard.card_order_no) <= (MAXFACEBOOKCARDS * (facebookpagecount + 1)))
                            {
                                tempAttachment = getAttachmentFromDialog(tempcard);
                            } else if (Int32.Parse(tempcard.card_order_no) > (MAXFACEBOOKCARDS * (facebookpagecount + 1)))
                            {
                                fbLeftCardCnt++;
                            }
                        } else if (channel.Equals("facebook"))
                        {
                            if (Int32.Parse(tempcard.card_order_no) <= MAXFACEBOOKCARDS)
                            {
                                tempAttachment = getAttachmentFromDialog(tempcard);
                            } else
                            {
                                fbLeftCardCnt++;
                            }
                        } else
                        {
                            tempAttachment = getAttachmentFromDialog(tempcard);
                        }
                        reply.Attachments.Add(tempAttachment);
                    }
                }
                else
                {
                    tempAttachment = getAttachmentFromDialog(dlg);
                    reply.Attachments.Add(tempAttachment);
                }
                await context.PostAsync(reply);
                reply.Attachments.Clear();

                //페이스북에서 남은 카드가 있는경우
                if (beforeMent.Equals(orgMent) && channel.Equals("facebook") && fbLeftCardCnt > 0)
                {
                    reply.Attachments.Add(
                        GetHeroCard(
                        "", "",
                        fbLeftCardCnt + "개의 컨테츠가 더 있습니다.",
                        //new CardAction(ActionTypes.ImBack, "더 보기", value: userData.GetProperty<string>("FB_BEFORE_MENT")))
                        new CardAction(ActionTypes.ImBack, "더 보기", value: beforeMent))
                    );
                    await context.PostAsync(reply);
                    reply.Attachments.Clear();
                }
            }
            context.ConversationData.SetValue("commonBeforeQustion", orgMent);
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
        private static Attachment GetHeroCard(string title, string subtitle, string text, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }
    }
}
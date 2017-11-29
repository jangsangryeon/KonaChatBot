using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using KonaChatBot.DB;
using KonaChatBot.Models;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using SecCsChatBotDemo.Models;

namespace KonaChatBot.Dialogs
{
    [Serializable]
    public class PriceApi : IDialog<object>
    {
        private string luis_intent;
        private string[] entities;
        private string queryStr;

        public PriceApi(string luis_intent, string entitiesStr, string queryStr)
        {
            this.luis_intent = luis_intent;

            //엔티티 comma split
            if (entitiesStr != null && !entitiesStr.Equals(""))
            {
                entities = entitiesStr.Split(',');
            }
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
            DbConnect db = new DbConnect();

            //키워드 그룹 추출
            List<KeywordGroup> keywordgrouplist = db.SelectKeywordGroupList(entities);
            List<Price_API_DLG> priceApiDlgList = new List<Price_API_DLG>();
            List<PriceMediaDlgList> priceMediaDlgList = new List<PriceMediaDlgList>();

            String entitlekeywordgroup = "", entitlekeyworddetail = "";

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            foreach (KeywordGroup keyword in keywordgrouplist)
            {
                entitlekeywordgroup += keyword.keywordgroup + ",";
                entitlekeyworddetail += keyword.keyworddetail + "=" + keyword.keyword + ",";
            }
            entitlekeyworddetail = entitlekeyworddetail.Substring(0, entitlekeyworddetail.Length - 1);
            if (entitlekeywordgroup.Contains("TRIMWORD"))
            {

                if (entitlekeywordgroup.Contains("EXTERIORWORD"))
                {
                    //entities "가솔린", "2WD", "가격", "트림","외장색상"
                    Debug.WriteLine("TRIMWORD,EXTERIORWORD entitlekeyworddetail : " + entitlekeyworddetail);
                    priceApiDlgList = db.SelectPriceList_API_DLG("TRIMWORD,EXTERIORWORD");

                    for (int i = 0; i < priceApiDlgList.Count; i++)
                    {

                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                        if (priceApiDlgList[i].dlgType == "TEXT")
                        {
                            string carModelNm = db.SelectPriceModelValue("TRIMWORD,INTERIORWORD", entitlekeyworddetail);
                            await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, carModelNm)[0].cardText);
                        }
                        else if (priceApiDlgList[i].dlgType == "MEDIA")
                        {
                            Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                            Debug.WriteLine("entitlekeyworddetail = " + entitlekeyworddetail);
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "TRIMWORD,EXTERIORWORD", entitlekeyworddetail);
                        }

                    }
                }
                else if (entitlekeywordgroup.Contains("INTERIORWORD"))
                {
                    //entities "가솔린", "2WD", "가격", "트림","내장색상"
                    Debug.WriteLine("TRIMWORD,INTERIORWORD entitlekeyworddetail : " + entitlekeyworddetail);
                    priceApiDlgList = db.SelectPriceList_API_DLG("TRIMWORD,INTERIORWORD");

                    for (int i = 0; i < priceApiDlgList.Count; i++)
                    {

                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                        if (priceApiDlgList[i].dlgType == "TEXT")
                        {
                            string carModelNm = db.SelectPriceModelValue("TRIMWORD,INTERIORWORD", entitlekeyworddetail);
                            //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                            await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, carModelNm)[0].cardText);
                        }
                        else if (priceApiDlgList[i].dlgType == "MEDIA")
                        {
                            Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "TRIMWORD,INTERIORWORD", entitlekeyworddetail);
                        }

                    }
                }
                else if (entitlekeywordgroup.Contains("OPTIONWORD"))
                {
                    //entities "옵션"
                    Debug.WriteLine("TRIMWORD,OPTIONWORD entitlekeyworddetail : " + entitlekeyworddetail);
                    priceApiDlgList = db.SelectPriceList_API_DLG("TRIMWORD,OPTIONWORD");

                    for (int i = 0; i < priceApiDlgList.Count; i++)
                    {

                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                        if (priceApiDlgList[i].dlgType == "TEXT")
                        {
                            string carModelNm = db.SelectPriceModelValue("TRIMWORD,INTERIORWORD", entitlekeyworddetail);
                            await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, carModelNm)[0].cardText);
                        }
                        else if (priceApiDlgList[i].dlgType == "MEDIA")
                        {
                            Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "TRIMWORD,OPTIONWORD", entitlekeyworddetail);
                        }

                    }
                }
                else
                {
                    //entities "가솔린", "2WD", "가격", "트림"
                    Debug.WriteLine("TRIMWORD entitlekeyworddetail : " + entitlekeyworddetail);
                    priceApiDlgList = db.SelectPriceList_API_DLG("TRIMWORD");

                    for (int i = 0; i < priceApiDlgList.Count; i++)
                    {

                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                        Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                        if (priceApiDlgList[i].dlgType == "TEXT")
                        {
                            string carModelNm = db.SelectPriceModelValue("TRIMWORD,INTERIORWORD", entitlekeyworddetail);
                            await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, carModelNm)[0].cardText);
                        }
                        else if (priceApiDlgList[i].dlgType == "MEDIA")
                        {
                            Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "TRIMWORD", entitlekeyworddetail);
                        }

                    }
                }
            }
            else if (entitlekeywordgroup.Contains("OPTIONWORD"))
            {
                //entities "옵션"
                Debug.WriteLine("OPTIONWORD entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("OPTIONWORD");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "OPTIONWORD", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("OPTION"))
            {
                //entities "네비게이션"
                Debug.WriteLine("OPTION entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("OPTION");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "OPTION", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("INTERIORWORD"))
            {
                //entities "가솔린", "2WD", "내장색상"
                Debug.WriteLine("INTERIORWORD entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("INTERIORWORD");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "INTERIORWORD", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("EXTERIORWORD"))
            {
                //entities "가솔린", "2WD", "외장색상"
                Debug.WriteLine("EXTERIORWORD entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("EXTERIORWORD");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "EXTERIORWORD", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("INTERIOR"))
            {
                //entities "가솔린", "2WD", "오랜지"
                Debug.WriteLine("INTERIOR entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("INTERIOR");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "INTERIOR", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("EXTERIOR"))
            {
                //entities "가솔린", "2WD", "다크나이트"
                Debug.WriteLine("EXTERIOR entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("EXTERIOR");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "EXTERIOR", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("TRIM"))
            {
                //entities "가솔린", "2WD"
                Debug.WriteLine("TRIM entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("TRIM");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "TRIM", entitlekeyworddetail);
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("SHORTCUT"))
            {
                //entities "견적", "바로가기"
                Debug.WriteLine("SHORTCUT entitlekeyworddetail : " + entitlekeyworddetail);
                priceApiDlgList = db.SelectPriceList_API_DLG("SHORTCUT");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "SHORTCUT", "");
                    }

                }
            }
            else if (entitlekeywordgroup.Contains("PRICEWORD"))
            {
                //entities "가격"
                Debug.WriteLine("PRICEWORD entitlekeyworddetail : " + entitlekeyworddetail);


                List<PriceModelList> priceModelList = db.SelectPriceModelList();

                priceApiDlgList = db.SelectPriceList_API_DLG("PRICEWORD");

                for (int i = 0; i < priceApiDlgList.Count; i++)
                {

                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].keywordGrp);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].priceDlgId);
                    Debug.WriteLine("[ " + i + " ] : " + priceApiDlgList[i].dlgType);


                    if (priceApiDlgList[i].dlgType == "TEXT")
                    {
                        //db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId)[0].cardText;
                        await context.PostAsync(db.SelectPriceTextDlgList(priceApiDlgList[i].priceDlgId, "")[0].cardText);
                    }
                    else if (priceApiDlgList[i].dlgType == "MEDIA")
                    {
                        Debug.WriteLine("card dlg id = " + priceApiDlgList[i].priceDlgId);
                        Debug.WriteLine("card dlg id = " + entitlekeyworddetail);
                        Debug.WriteLine("card dlg id = " + entitlekeywordgroup);
                        priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "PRICEWORD", "");
                    }

                }
                //returndialog = "가격이 궁금하시군요";
            }
            else
            {
                Debug.WriteLine("keyword group error : " + entitlekeywordgroup);
            }

            Translator engineTranslateInfo;
            Translator trimTranslateInfo;
            Translator titleTranslateInfo;



            if (priceMediaDlgList.Count > 0)
            {
                string cardDiv = "";
                string cardVal = "";

                for (int i = 0; i < priceMediaDlgList.Count; i++)
                {
                    engineTranslateInfo = await getTranslate(priceMediaDlgList[i].engine);
                    trimTranslateInfo = await getTranslate(priceMediaDlgList[i].trim);
                    titleTranslateInfo = await getTranslate(priceMediaDlgList[i].cardTitle);
                     
                    //translateInfo.data.translations[0].translatedText.Replace("&#39;", "'")
                    //Debug.WriteLine("priceMediaDlgList[i].cardTitle color : " + engineTranslateInfo.data.translations[0].translatedText);
                    //Debug.WriteLine("priceMediaDlgList[i].cardTitle color : " + trimTranslateInfo.data.translations[0].translatedText);
                    //Debug.WriteLine("priceMediaDlgList[i].cardTitle color : " + titleTranslateInfo.data.translations[0].translatedText);
                    //Debug.WriteLine("AA : " + priceMediaDlgList[i].mediaUrl);
                    //Debug.WriteLine("AA : " + priceMediaDlgList[i].mediaUrl.Replace("CALL(ENGINEIMAGE_URL)", engineTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(TRIMIMAGE_URL)", trimTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(IMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText));
                    //Debug.WriteLine("AA : " + priceMediaDlgList[i].mediaUrl.Replace("CALL(OPTIMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText.Replace(" ", "_")).Replace("CALL(ENGINEIMAGE_URL)", engineTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(TRIMIMAGE_URL)", trimTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(IMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText));
                    //CardImage 입력
                    CardImage cardImage = new CardImage()     
                    {
                        Url = priceMediaDlgList[i].mediaUrl.Replace("CALL(OPTIMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText.Replace(" ", "_")).Replace("CALL(ENGINEIMAGE_URL)", engineTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(TRIMIMAGE_URL)", trimTranslateInfo.data.translations[0].translatedText.Replace(" ", "")).Replace("CALL(IMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText.Replace(" ", "").Replace("DarkKnight", "DarkNight"))
                    };

                    //CardAction 입력
                    List<CardAction> cardButtons = new List<CardAction>();

                    if (priceMediaDlgList[i].btn1Type.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = priceMediaDlgList[i].btn1Context,
                            Type = priceMediaDlgList[i].btn1Type,
                            Title = priceMediaDlgList[i].btn1Title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (priceMediaDlgList[i].btn2Type.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = priceMediaDlgList[i].btn2Context,
                            Type = priceMediaDlgList[i].btn2Type,
                            Title = priceMediaDlgList[i].btn2Title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (priceMediaDlgList[i].btn3Type.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = priceMediaDlgList[i].btn3Context,
                            Type = priceMediaDlgList[i].btn3Type,
                            Title = priceMediaDlgList[i].btn3Title
                        };
                        cardButtons.Add(plButton);
                    }
                    if (priceMediaDlgList[i].btn4Type.Length != 0)
                    {
                        CardAction plButton = new CardAction()
                        {
                            Value = priceMediaDlgList[i].btn4Context,
                            Type = priceMediaDlgList[i].btn4Type,
                            Title = priceMediaDlgList[i].btn4Title
                        };
                        cardButtons.Add(plButton);
                    }

                    if (priceMediaDlgList[i].cardDivision.Length != 0)
                    {
                        cardDiv = priceMediaDlgList[i].cardDivision;
                    }

                    if (priceMediaDlgList[i].cardValue.Length != 0)
                    {
                        //cardVal = priceMediaDlgList[i].cardValue.Replace();
                        cardVal = priceMediaDlgList[i].cardValue.Replace("CALL(OPTIMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText.Replace(" ", "_")).Replace("CALL(IMAGE_URL)", titleTranslateInfo.data.translations[0].translatedText.Replace(" ", "").Replace("DarkKnight", "DarkNight"));
                    }


                    ////맵에서 text로 출력되는 주소값 치환
                    //if (!string.IsNullOrEmpty(replaceStr))
                    //{
                    //    reply.Text = replaceStr.Replace("CALL(지점 주소)", SelectTestDriveList_API_DLG_MEDIA[i].address);
                    //    await context.PostAsync(reply);
                    //    replaceStr = "";
                    //    reply.Text = "";
                    //}


                    reply.Attachments.Add(GetHeroCard(priceMediaDlgList[i].cardTitle, priceMediaDlgList[i].cardSubTitle, priceMediaDlgList[i].cardText, cardImage, cardButtons, cardDiv, cardVal));
                }
                await context.PostAsync(reply);
            }

            context.Done("");
            //await context.PostAsync(returndialog);

            // 변수 선언
            //string replaceStr = "";
            //엔티티 type, 엔티티 value 추출
            //List<TestDriveList_API> SelectTestDriveList_API = db.SelectTestDriveList_API(luis_intent, entitiesStr, queryStr);
            ////다이얼로그 ID 추출
            //List<TestDriveList_API_DLG> SelectTestDriveList_API_DLG = db.SelectTestDriveList_API_DLG(SelectTestDriveList_API[0].entityType);


            //var reply = context.MakeMessage();
            //reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            //for (int td = 0; td < SelectTestDriveList_API_DLG.Count; td++)
            //{
            //    if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(1))
            //    {
            //        Debug.WriteLine("GUBUN 1");
            //    }
            //    else if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(2))
            //    {
            //        Debug.WriteLine("GUBUN 2");
            //        //텍스트 값 가져오기
            //        List<TestDriveList_API_DLG_TEXT> SelectTestDriveList_API_DLG_TEXT = db.SelectTestDriveList_API_DLG_TEXT(SelectTestDriveList_API_DLG[td].testdrive_dlg_id);
            //        //예외처리
            //        if (SelectTestDriveList_API_DLG_TEXT.Count != 0)
            //        {
            //            replaceStr = SelectTestDriveList_API_DLG_TEXT[0].testdrive_card_text;
            //        }
            //        else
            //        {
            //            replaceStr = "";
            //        }

            //    }
            //    else if (SelectTestDriveList_API_DLG[td].dlg_type.Equals(3))
            //    {
            //        Debug.WriteLine("GUBUN 3");
            //    }
            //    else
            //    {
            //        Debug.WriteLine("GUBUN 4");
            //        //미디어 값 가져오기
            //        List<TestDriveList_API_DLG_MEDIA> SelectTestDriveList_API_DLG_MEDIA = db.SelectTestDriveList_API_DLG_MEDIA(SelectTestDriveList_API_DLG[td].testdrive_dlg_id, SelectTestDriveList_API[0].entityValue);

            //        for (int i = 0; i < SelectTestDriveList_API_DLG_MEDIA.Count; i++)
            //        {
            //            //CardImage 입력
            //            CardImage cardImage = new CardImage()
            //            {
            //                Url = SelectTestDriveList_API_DLG_MEDIA[i].media_url
            //            };

            //            //CardAction 입력
            //            List<CardAction> cardButtons = new List<CardAction>();

            //            if (SelectTestDriveList_API_DLG_MEDIA[i].btn_1_context.Length != 0)
            //            {
            //                CardAction plButton = new CardAction()
            //                {
            //                    Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_context,
            //                    Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_type,
            //                    Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_1_title
            //                };
            //                cardButtons.Add(plButton);
            //            }

            //            if (SelectTestDriveList_API_DLG_MEDIA[i].btn_2_context.Length != 0)
            //            {
            //                CardAction plButton = new CardAction()
            //                {
            //                    Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_context,
            //                    Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_type,
            //                    Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_2_title
            //                };
            //                cardButtons.Add(plButton);
            //            }

            //            if (SelectTestDriveList_API_DLG_MEDIA[i].btn_3_context.Length != 0)
            //            {
            //                CardAction plButton = new CardAction()
            //                {
            //                    Value = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_context,
            //                    Type = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_type,
            //                    Title = SelectTestDriveList_API_DLG_MEDIA[i].btn_3_title
            //                };
            //                cardButtons.Add(plButton);
            //            }

            //            //맵에서 text로 출력되는 주소값 치환
            //            if (!string.IsNullOrEmpty(replaceStr))
            //            {
            //                reply.Text = replaceStr.Replace("CALL(지점 주소)", SelectTestDriveList_API_DLG_MEDIA[i].address);
            //                await context.PostAsync(reply);
            //                replaceStr = "";
            //                reply.Text = "";
            //            }


            //            reply.Attachments.Add(GetHeroCard(SelectTestDriveList_API_DLG_MEDIA[i].card_title, SelectTestDriveList_API_DLG_MEDIA[i].card_subtitle, SelectTestDriveList_API_DLG_MEDIA[i].card_text, cardImage, cardButtons));
            //        }

            //        await context.PostAsync(reply);
            //        //reply 초기화
            //        reply.Attachments.Clear();
            //    }
            //
            //}
        }
        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, /*CardAction cardAction*/ List<CardAction> buttons, string cardDivision, string cardValue)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = buttons,
                Card_division = cardDivision,
                Card_value = cardValue,

            };

            return heroCard.ToAttachment();
        }

        private static async Task<Translator> getTranslate(string input)
        {
            Translator trans = new Translator();

            using (HttpClient client = new HttpClient())
            {
                string appId = "AIzaSyDr4CH9BVfENdM9uoSK0fANFVWD0gGXlJM";

                string url = string.Format("https://translation.googleapis.com/language/translate/v2/?key={0}&q={1}&source=ko&target=en&model=nmt", appId, input);

                HttpResponseMessage msg = await client.GetAsync(url);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    trans = JsonConvert.DeserializeObject<Translator>(JsonDataResponse);
                }
                return trans;
            }

        }



    }
}
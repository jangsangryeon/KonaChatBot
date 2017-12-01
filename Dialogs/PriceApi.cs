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
using System.Net;
using System.Web.Http;
using System.Text.RegularExpressions;

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

        public async Task StartAsync(IDialogContext context)
        {
            DbConnect db = new DbConnect();
            //MessagesController.startTime = DateTime.Now;
            HttpResponseMessage response;

            //키워드 그룹 추출
            //List<KeywordGroup> keywordgrouplist = db.SelectKeywordGroupList(entities);
            List<KeywordGroup> keywordgrouplist = db.SelectKeywordGroupList(MessagesController.cacheList.luisEntities.Split(','));
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
                        if (entitlekeyworddetail != "INTERIORCOLOR=내장색상")
                        {
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "INTERIORWORD", entitlekeyworddetail);
                        }
                        else
                        {
                            priceMediaDlgList = db.SelectPriceMediaDlgList(16, "INTERIORWORD", entitlekeyworddetail);
                        }

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
                        if (entitlekeyworddetail != "EXTERIORCOLOR=외장색상")
                        {
                            priceMediaDlgList = db.SelectPriceMediaDlgList(priceApiDlgList[i].priceDlgId, "EXTERIORWORD", entitlekeyworddetail);
                        }
                        else
                        {
                            priceMediaDlgList = db.SelectPriceMediaDlgList(11, "EXTERIORWORD", entitlekeyworddetail);
                        }

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

                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - MessagesController.startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + context.Activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + context.Activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + context.Activity.ServiceUrl);
                //var message = await result;
                int dbResult = db.insertUserQuery(Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline), "", "", "0", "", 'D', 0);
                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                if (db.insertHistory(context.Activity.Conversation.Id, MessagesController.queryStr, "ERROR", context.Activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds), 0) > 0)
                {
                    Debug.WriteLine("HISTORY RESULT SUCCESS");
                    //HistoryLog("HISTORY RESULT SUCCESS");
                }
                else
                {
                    Debug.WriteLine("HISTORY RESULT SUCCESS");
                    //HistoryLog("HISTORY RESULT FAIL");
                }

            }

            if (priceMediaDlgList.Count > 0)
            {
                string cardDiv = "";
                string cardVal = "";
                CardImage cardImage;
                for (int i = 0; i < priceMediaDlgList.Count; i++)
                {
                    Translator imgNmTranslateInfo = new Translator();
                    if (priceMediaDlgList[0].groupNm == "TRIMWORD,EXTERIORWORD" || priceMediaDlgList[0].groupNm == "TRIMWORD,INTERIORWORD" || priceMediaDlgList[0].groupNm == "TRIMWORD,OPTIONWORD")//|| priceMediaDlgList[0].groupNm == "OPTION" || priceMediaDlgList[0].groupNm == "EXTERIOR" || priceMediaDlgList[0].groupNm == "INTERIOR")
                    {
                        imgNmTranslateInfo = await getTranslate(priceMediaDlgList[i].cardTitle);
                    }
                    else if (priceMediaDlgList[0].groupNm == "OPTIONWORD" || priceMediaDlgList[0].groupNm == "OPTION" || priceMediaDlgList[0].groupNm == "EXTERIOR" || priceMediaDlgList[0].groupNm == "INTERIOR")
                    {
                        imgNmTranslateInfo = await getTranslate(priceMediaDlgList[i].cardTitle);
                    }
                    else if (priceMediaDlgList[0].groupNm == "PRICEWORD")
                    {
                        imgNmTranslateInfo = await getTranslate(priceMediaDlgList[i].engine);
                    }
                    else if ((priceMediaDlgList[0].groupNm == "EXTERIORWORD" && priceMediaDlgList[0].entityDetail == "EXTERIORCOLOR=외장색상") || (priceMediaDlgList[0].groupNm == "INTERIORWORD" && priceMediaDlgList[0].entityDetail == "INTERIORCOLOR=내장색상"))
                    {
                        imgNmTranslateInfo = await getTranslate(priceMediaDlgList[i].cardTitle);
                    }
                    else
                    {
                        string trimNm = "";
                        if (!string.IsNullOrEmpty(priceMediaDlgList[i].trim))
                        {

                            if (priceMediaDlgList[i].trim.Contains(";"))
                            {
                                trimNm = priceMediaDlgList[i].trim.Split(';')[0].ToString();
                            }
                            else
                            {
                                trimNm = priceMediaDlgList[i].trim;
                            }
                        }
                        else
                        {
                            trimNm = "";
                        }
                        imgNmTranslateInfo = await getTranslate(trimNm);
                    }


                    if (priceMediaDlgList[0].groupNm == "TRIMWORD,OPTIONWORD" || priceMediaDlgList[0].groupNm == "OPTION" || priceMediaDlgList[0].groupNm == "OPTIONWORD")
                    {
                        cardImage = new CardImage()
                        {
                            Url = priceMediaDlgList[i].mediaUrl.Replace("CALL(IMAGE_URL)", imgNmTranslateInfo.data.translations[0].translatedText.Replace(" ", "_"))
                        };
                    }
                    else if (priceMediaDlgList[0].groupNm == "EXTERIORWORD" && priceMediaDlgList[0].entityDetail == "EXTERIORCOLOR=외장색상")
                    {
                        cardImage = new CardImage()
                        {
                            Url = priceMediaDlgList[i].mediaUrl.Replace("CALL(IMAGE_URL)", imgNmTranslateInfo.data.translations[0].translatedText.Replace(" ", "").Replace("DarkKnight", "DarkNight"))
                        };
                    }
                    else if (priceMediaDlgList[0].groupNm == "INTERIORWORD" && priceMediaDlgList[0].entityDetail == "INTERIORCOLOR=내장색상")
                    {
                        cardImage = new CardImage()
                        {
                            Url = priceMediaDlgList[i].mediaUrl.Replace("CALL(IMAGE_URL)", imgNmTranslateInfo.data.translations[0].translatedText.Replace(" ", "").Replace("AcidYellow", "Acid Yellow"))
                        };
                    }
                    else
                    {
                        cardImage = new CardImage()
                        {
                            Url = priceMediaDlgList[i].mediaUrl.Replace("CALL(IMAGE_URL)", imgNmTranslateInfo.data.translations[0].translatedText.Replace(" ", "").Replace("DarkKnight", "DarkNight").Replace("AcidYellow", "Acid Yellow"))
                        };
                    }


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
                        cardVal = priceMediaDlgList[i].cardValue.Replace("CALL(IMAGE_URL)", imgNmTranslateInfo.data.translations[0].translatedText.Replace(" ", " ").Replace("DarkKnight", "DarkNight")).ToLower();
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
                //response = Request.CreateResponse(HttpStatusCode.OK);
                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - MessagesController.startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + context.Activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + context.Activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + context.Activity.ServiceUrl);
                //var message = await result;
                int dbResult = db.insertUserQuery(Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline), MessagesController.cacheList.luisIntent, MessagesController.cacheList.luisEntities, "0", MessagesController.cacheList.luisId, 'H', 0);
                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                if (db.insertHistory(context.Activity.Conversation.Id, MessagesController.queryStr, MessagesController.cacheList.dlgId.ToString(), context.Activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds), 0) > 0)
                {
                    Debug.WriteLine("HISTORY RESULT SUCCESS");
                    //HistoryLog("HISTORY RESULT SUCCESS");
                }
                else
                {
                    Debug.WriteLine("HISTORY RESULT FAIL");
                    //HistoryLog("HISTORY RESULT FAIL");
                }


            }
            context.Done<IMessageActivity>(null);
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
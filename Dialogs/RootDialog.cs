using System;
using System.Collections.Generic;
using System.Threading;
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
    public class RootDialog : IDialog<object>
    {

        private string luisId;
        private string luisIntent;
        private string luisEntities;

        public RootDialog(string luisId, string luisIntent, string luisEntities)
        {
            this.luisId = luisId;
            this.luisIntent = luisIntent;
            this.luisEntities = luisEntities;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var activity = context.Activity;
            DbConnect db = new DbConnect();

            //context.Wait(this.MessageReceivedAsync);
            MessagesController.relationList = db.DefineTypeChk(MessagesController.luisId, MessagesController.luisIntent, MessagesController.luisEntities);
            
            if (MessagesController.relationList.Count > 0)
            {
                //답변이 시승 rest api 호출인 경우
                if (MessagesController.relationList[0].dlgApiDefine.Equals("api testdrive"))
                {
                    context.Call(new TestDriveApi(MessagesController.queryStr), this.ResumeAfterOptionDialog);
                }
                //답변이 가격 rest api 호출인 경우
                else if (MessagesController.relationList[0].dlgApiDefine.Equals("api quot"))
                {
                    context.Call(new PriceApi(MessagesController.luisIntent, MessagesController.luisEntities, MessagesController.queryStr), this.ResumeAfterOptionDialog);
                }
                //답변이 추천 rest api 호출인 경우
                else if (MessagesController.relationList[0].dlgApiDefine.Equals("api recommend"))
                {
                    context.Call(new RecommendApiDialog(), this.ResumeAfterOptionDialog);
                }
                //답변이 일반 답변인 경우
                else if (MessagesController.relationList[0].dlgApiDefine.Equals("D"))
                {
                    context.Call(new CommonDialog("", MessagesController.queryStr), this.ResumeAfterOptionDialog);
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
            }
            //relation 값이 없을 경우 -> 네이버 기사 검색
            else
            {
                //인텐트 파악은 추천으로 했으나 relation 테이블에 등록이 안되어 있는 경우
                if (MessagesController.luisIntent.Contains("recommend "))
                {
                    context.Call(new RecommendApiDialog(), this.ResumeAfterOptionDialog);
                } else
                {
                    context.Call(new IntentNoneDialog("", "", "", ""), this.ResumeAfterOptionDialog);
                }
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result) => context.Done<IMessageActivity>(null);
    }
}
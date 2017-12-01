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
            //MessagesController.relationList = db.DefineTypeChk(MessagesController.luisId, MessagesController.luisIntent, MessagesController.luisEntities);
            String fullentity = db.SearchCommonEntities;

            //Debug.WriteLine("fullentity = " + fullentity + "====" + fullentity.Length);
            //Debug.WriteLine("MessagesController.luisEntities = " + MessagesController.luisEntities + "====" + MessagesController.luisEntities.Length);

            string compareLuisEntity = "";
            if (!String.IsNullOrEmpty(fullentity))
            {
                //entity 길이 비교
                if (fullentity.Length > MessagesController.luisEntities.Length)
                {
                    compareLuisEntity = fullentity;
                }
                else
                {
                    compareLuisEntity = MessagesController.luisEntities;
                }
            }

            //DefineTypeChkSpare에서는 인텐트나 루이스아이디조건 없이 엔티티만 일치하면 다이얼로그 리턴
            MessagesController.relationList = db.DefineTypeChkSpare(compareLuisEntity);
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
            }
            //relation 값이 없을 경우 -> 네이버 기사 검색
            else
            {
                //인텐트 파악은 추천으로 했으나 relation 테이블에 등록이 안되어 있는 경우
                if (MessagesController.luisIntent.Contains("recommend "))
                {
                    context.Call(new RecommendApiDialog(), this.ResumeAfterOptionDialog);
                }else if (MessagesController.luisIntent.Contains("TESTDRIVE") || MessagesController.luisIntent.Contains("BRANCH"))
                {

                    context.Call(new TestDriveApi(MessagesController.queryStr), this.ResumeAfterOptionDialog);
                }
                else if (MessagesController.luisIntent.Contains("quot"))
                {
                    //context.Call(new RecommendApiDialog(), this.ResumeAfterOptionDialog);
                    context.Call(new PriceApi(MessagesController.luisIntent, MessagesController.luisEntities, MessagesController.queryStr), this.ResumeAfterOptionDialog);
                }
                else
                {
                    context.Call(new IntentNoneDialog("", "", "", ""), this.ResumeAfterOptionDialog);
                }
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result) => context.Done<IMessageActivity>(null);
    }
}
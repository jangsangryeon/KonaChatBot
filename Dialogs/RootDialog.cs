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
            DbConnect db = new DbConnect();

            //context.Wait(this.MessageReceivedAsync);
            MessagesController.relationList = db.DefineTypeChk(MessagesController.luisId, MessagesController.luisIntent, MessagesController.luisEntities);
            //relationList[0].dlgApiDefine.Equals("api testdrive")
            switch (MessagesController.relationList[0].dlgApiDefine)
            {
                case "api testdrive":
                    context.Call(new TestDriveApi(MessagesController.queryStr), this.ResumeAfterOptionDialog);
                    break;
                case "api quot":
                    context.Call(new PriceApi(MessagesController.luisIntent, MessagesController.luisEntities, MessagesController.queryStr), this.ResumeAfterOptionDialog);
                    break;
                case "api recommend":
                    context.Call(new RecommendApiDialog(), this.ResumeAfterOptionDialog);
                    break;
                default:
                    context.Call(new CommonDialog("", MessagesController.queryStr), this.ResumeAfterOptionDialog);
                    break;
            }
            //context.Wait(ResumeAfterOptionDialog);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //DbConnect db = new DbConnect();

            //try
            //{                
            //    MessagesController.relationList = db.DefineTypeChk(luisId, luisIntent, luisEntities);
            //    //relationList[0].dlgApiDefine.Equals("api testdrive")
            //    switch (MessagesController.relationList[0].dlgApiDefine)
            //    {
            //        case "api testdrive":
            //            context.Call(new TestDriveApi("시승"), this.ResumeAfterOptionDialog);
            //            break;

            //        case "api quot":
            //            context.Call(new PriceApi("견적", "견적","견적"), this.ResumeAfterOptionDialog);
            //            break;
            //    }
            //}
            //catch (TooManyAttemptsException ex)
            //{
            //    await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

            //    context.Wait(this.MessageReceivedAsync);
            //}
        }


        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                //context.Wait(this.MessageReceivedAsync);
                context.Done("");
            }
        }
    }
}
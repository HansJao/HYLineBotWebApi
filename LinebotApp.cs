using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HYLineBotWebApi.Adapter;
using HYLineBotWebApi.DataClass;
using Line.Messaging;
using Line.Messaging.Webhooks;
using static HYLineBotWebApi.DataClass.Enumeration.PageEnum;

namespace HYLineWebApi.Controllers
{
    public class LinebotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        public LinebotApp(LineMessagingClient messagingClient)
        {
            this.messagingClient = messagingClient;

        }

        #region Handlers
        public async Task SendMessage(IEnumerable<WebhookEvent> ev)
        {
            var firstEvent = (MessageEvent)ev.ToList().First();
            await HandleTextAsync(firstEvent.ReplyToken, ((TextEventMessage)firstEvent.Message).Text, firstEvent.Source.UserId);
        }

        public async Task HandleMessage(IEnumerable<WebhookEvent> eventList)
        {
            var mev = (MessageEvent)eventList.ToList().First();
            var message = ((TextEventMessage)mev.Message).Text;
            var messageSpilt = message.Split(' ');
            switch (ConvertToNarrow(messageSpilt.FirstOrDefault()).ToLower())
            {
                case "!":
                    await HandleTextAsync(mev.ReplyToken, "修改", mev.Source.UserId);
                    break;
                case "!數量":
                    await UpdateQuantityById(mev, messageSpilt);
                    break;
                case "?倉庫":
                    await SearchStore(mev, messageSpilt);
                    break;
                case "?名稱":
                    await SearchName(mev, messageSpilt);
                    break;
                case "+":
                    await Insert(mev, messageSpilt);
                    break;
                case "-":
                    await DeleteById(mev, messageSpilt);
                    break;
                case "help":
                    await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage>
                    {
                        new TextMessage("======查詢指令======\n ?倉庫 [倉庫名稱] \n ?名稱 [布種名稱] \n ======新增指令======\n + [倉庫名稱] [布種名稱] [顏色] [儲位] [數量] [備註] \n ======修改指令======\n !數量 [編號] [數量]\n ======刪除指令======\n - [編號]")
                    });
                    break;
                default:
                    return;
            }
            //await HandleTextAsync(mev.ReplyToken, ((TextEventMessage)mev.Message).Text, mev.Source.UserId);
        }
        private async Task UpdateQuantityById(MessageEvent mev, string[] messageSpilt)
        {
            int id;
            int quantity;
            if (messageSpilt.Count() != 3)
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("更新指令錯誤!!\n !數量 [編號] [數量]") });
                return;
            }
            if (!int.TryParse(messageSpilt[1], out id))
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("參數錯誤!\n輸入的編號不是數字!") });
                return;
            }
            if (!int.TryParse(messageSpilt[2], out quantity))
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("參數錯誤!\n輸入的數量不是數字!") });
                return;
            }

            var userName = GetUserName(mev.Source.UserId);
            DataAdapter da = new DataAdapter();
            var result = da.UpdateQuantityByID(id, quantity, userName);
            var replyMessage = "更新失敗";
            if (result == 1)
                replyMessage = "更新成功";
            await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage(replyMessage) });
        }

        private async Task DeleteById(MessageEvent mev, string[] messageSpilt)
        {
            int id;
            if (messageSpilt.Count() != 2)
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("更新指令錯誤!!\n - [編號]") });
                return;
            }
            if (!int.TryParse(messageSpilt[1], out id))
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("參數錯誤!\n輸入的編號不是數字!") });
                return;
            }
            DataAdapter da = new DataAdapter();
            var result = da.DelectByID(id);
            var replyMessage = "刪除失敗";
            if (result == 1)
                replyMessage = "刪除成功";
            await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage(replyMessage) });
        }

        private async Task Insert(MessageEvent mev, string[] messageSpilt)
        {
            var area = messageSpilt[1];
            var name = messageSpilt[2];
            var color = messageSpilt[3];
            var position = messageSpilt[4];
            var quantity = Convert.ToInt32(messageSpilt[5]);
            var memo = messageSpilt[6];
            var userName = GetUserName(mev.Source.UserId);
            DataAdapter da = new DataAdapter();
            var result = da.Insert(area, name, color, position, quantity, userName, memo);
            var replyMessage = "新增失敗";
            if (result == 1)
                replyMessage = "新增成功";
            await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage(replyMessage) });
        }

        private string GetUserName(string userID)
        {
            if (ConfigProvider.UserIDList.Count() == 0 || ConfigProvider.UserIDList.Where(w => w.Value == userID).Count() == 0)
            {
                return userID;
            }
            var user = ConfigProvider.UserIDList.Where(w => w.Value == userID).FirstOrDefault();
            return user.Key;
        }

        private async Task SearchName(MessageEvent mev, string[] messageSpilt)
        {
            if (messageSpilt.Count() != 2)
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("查詢名稱指令錯誤!!\n ?名稱 [布名]") });
                return;
            }
            DataAdapter da = new DataAdapter();
            var result = da.SearchName(messageSpilt[1]);
            var replyMessage = GetReplyMessage(result);
            await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage(replyMessage) });
        }
        private async Task SearchStore(MessageEvent mev, string[] messageSpilt)
        {
            if (messageSpilt.Count() < 2 || messageSpilt.Count() > 3)
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("查詢倉庫指令錯誤!!\n ?倉庫 [倉庫名稱]") });
                return;
            }
            var adapter = new DataAdapter();
            var areaResult = adapter.SearchArea(messageSpilt[1]);
            var groupByName = areaResult.GroupBy(g => g.Name);
            List<ITemplateAction> actions = new List<ITemplateAction>();
            List<CarouselColumn> column = new List<CarouselColumn>();

            foreach (var textile in groupByName)
            {
                actions.Add(new MessageTemplateAction(textile.Key, string.Concat("?名稱 ", textile.Key)));
            }
            int defaultPage = messageSpilt.Count() == 3 ? Convert.ToInt32(messageSpilt[2]) : 0;
            var perPartActions = actions.Skip((int)SearchPageEnum.TotalCount * defaultPage).Take((int)SearchPageEnum.TotalCount).ToList();
            if (perPartActions.Count() == 0)
            {
                await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { new TextMessage("已無資料") });
                return;
            }
            for (var i = 0; i < Convert.ToInt32(Math.Ceiling(Convert.ToDouble(perPartActions.Count()) / (int)SearchPageEnum.PerColumn)); i++)
            {
                var eachPage = perPartActions.Skip((int)SearchPageEnum.PerColumn * i).Take((int)SearchPageEnum.PerColumn).ToList();
                column.Add(new CarouselColumn(string.Concat("搜尋的倉庫:", messageSpilt[1]), actions: eachPage));
            }

            if (column.Last().Actions.Count() != (int)SearchPageEnum.PerColumn)
            {
                //如果數量還有才要顯示下一頁
                if (actions.Count() > (int)SearchPageEnum.TotalCount * (defaultPage + 1))
                {
                    column.Last().Actions.Add(new MessageTemplateAction("下一頁", string.Concat("?倉庫 ", messageSpilt[1], " ", defaultPage++)));
                    if (column.Last().Actions.Count() == 2)
                    {
                        column.Last().Actions.Add(new MessageTemplateAction("下一頁", string.Concat("?倉庫 ", messageSpilt[1], " ", defaultPage + 1)));
                    }
                }
                //數量已經沒了
                else
                {
                    column.Last().Actions.Add(new MessageTemplateAction("沒了", "就沒了阿"));
                    if (column.Last().Actions.Count() == 2)
                    {
                        column.Last().Actions.Add(new MessageTemplateAction("沒了", "就沒了咩"));
                    }
                }
            }
            //如果數量還有才要顯示下一頁
            else if (actions.Count() > (int)SearchPageEnum.TotalCount * (defaultPage + 1))
            {
                column.Add(new CarouselColumn("欲搜尋下一頁請點選", actions: new List<ITemplateAction>()
                {
                   new MessageTemplateAction("下一頁", string.Concat("?倉庫 ", messageSpilt[1]," ", defaultPage + 1)),
                   new MessageTemplateAction("下一頁", string.Concat("?倉庫 ", messageSpilt[1]," ", defaultPage + 1)),
                   new MessageTemplateAction("下一頁", string.Concat("?倉庫 ", messageSpilt[1]," ", defaultPage + 1))
                }));
            }

            var replyMessage = new TemplateMessage("Button Template",
                new CarouselTemplate(column));

            await messagingClient.ReplyMessageAsync(mev.ReplyToken, new List<ISendMessage> { replyMessage });
        }

        private string GetReplyMessage(IEnumerable<TextileStore> textileStoreList)
        {
            var replyMessage = string.Empty;
            foreach (var textile in textileStoreList)
            {
                replyMessage += string.Concat("編號:", textile.ID, "\n",
                                              "地點:", textile.Area, "\n",
                                              "名稱:", textile.Name, "\n",
                                              "顏色:", textile.Color, "\n",
                                              "儲位:", textile.Position, "\n",
                                              "數量:", textile.Quantity, "\n",
                                              "備註:", textile.Memo, "\n",
                                              "更新時間:", textile.ModifyDate.ToString("yyyy/MM/dd HH:mm:ss"), "\n",
                                              "修改人員:", textile.ModifyUser, "\n", "---------------------", "\n");
            }
            return replyMessage;
        }

        private string ConvertToNarrow(string text)
        {
            var narrow = string.Empty;
            narrow = text.Replace('？', '?').Replace('＋', '+').Replace('－', '-').Replace('！', '!');
            return narrow;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
                    // case EventMessageType.Image:
                    // case EventMessageType.Audio:
                    // case EventMessageType.Video:
                    // case EventMessageType.File:
                    //     // Prepare blob directory name for binary object.
                    //     var blobDirectoryName = ev.Source.Type + "_" + ev.Source.Id;
                    //     await HandleMediaAsync(ev.ReplyToken, ev.Message.Id, blobDirectoryName, ev.Message.Id);
                    //     break;
                    // case EventMessageType.Location:
                    //     var location = ((LocationEventMessage)ev.Message);
                    //     await HandleLocationAsync(ev.ReplyToken, location);
                    //     break;
                    // case EventMessageType.Sticker:
                    //     await HandleStickerAsync(ev.ReplyToken);
                    //     break;
            }
        }

        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            switch (ev.Postback.Data)
            {
                case "Date":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the date: " + ev.Postback.Params.Date);
                    break;
                case "Time":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the time: " + ev.Postback.Params.Time);
                    break;
                case "DateTime":
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "You chose the date-time: " + ev.Postback.Params.DateTime);
                    break;
                default:
                    await messagingClient.ReplyMessageAsync(ev.ReplyToken,
                        "Your postback is " + ev.Postback.Data);
                    break;
            }
        }

        protected override async Task OnFollowAsync(FollowEvent ev)
        {

            var userName = "";
            if (!string.IsNullOrEmpty(ev.Source.Id))
            {
                var userProfile = await messagingClient.GetUserProfileAsync(ev.Source.Id);
                userName = userProfile?.DisplayName ?? "";
            }

            await messagingClient.ReplyMessageAsync(ev.ReplyToken, $"Hello {userName}! Thank you for following !");
        }


        protected override async Task OnJoinAsync(JoinEvent ev)
        {
            await messagingClient.ReplyMessageAsync(ev.ReplyToken, $"Thank you for letting me join your {ev.Source.Type.ToString().ToLower()}!");
        }


        protected override async Task OnBeaconAsync(BeaconEvent ev)
        {
            var message = "";
            switch (ev.Beacon.Type)
            {
                case BeaconType.Enter:
                    message = "You entered the beacon area!";
                    break;
                case BeaconType.Leave:
                    message = "You leaved the beacon area!";
                    break;
                case BeaconType.Banner:
                    message = "You tapped the beacon banner!";
                    break;
            }

            await messagingClient.ReplyMessageAsync(ev.ReplyToken, $"{message}(Dm:{ev.Beacon.Dm}, Hwid:{ev.Beacon.Hwid})");
        }

        #endregion
        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            userMessage = userMessage.ToLower().Replace(" ", "");
            ISendMessage replyMessage = null;
            if (userMessage == "buttons")
            {
                replyMessage = new TemplateMessage("Button Template",
                    new ButtonsTemplate(text: "ButtonsTemplate", title: "Click Buttons.",
                    actions: new List<ITemplateAction> {
                        new MessageTemplateAction("Message Label", "sample data"),
                        new PostbackTemplateAction("Postback Label", "sample data", "sample data"),
                    new UriTemplateAction("Uri Label", "https://github.com/kenakamu")
                    }));
            }
            else if (userMessage == "confirm")
            {
                replyMessage = new TemplateMessage("Confirm Template",
                    new ConfirmTemplate("ConfirmTemplate", new List<ITemplateAction> {
                        new MessageTemplateAction("Yes", "Yes"),
                        new MessageTemplateAction("No", "No")
                    }));
            }
            else if (userMessage == "carousel")
            {
                List<ITemplateAction> actions1 = new List<ITemplateAction>();
                List<ITemplateAction> actions2 = new List<ITemplateAction>();

                // Add actions.
                actions1.Add(new MessageTemplateAction("Message Label", "sample data"));
                actions1.Add(new PostbackTemplateAction("Postback Label", "sample data", "sample data"));
                actions1.Add(new UriTemplateAction("Uri Label", "https://github.com/kenakamu"));

                // Add datetime picker actions
                actions2.Add(new DateTimePickerTemplateAction("DateTime Picker", "DateTime",
                    DateTimePickerMode.Datetime, "2017-07-21T13:00", null, null));
                actions2.Add(new DateTimePickerTemplateAction("Date Picker", "Date",
                    DateTimePickerMode.Date, "2017-07-21", null, null));
                actions2.Add(new DateTimePickerTemplateAction("Time Picker", "Time",
                    DateTimePickerMode.Time, "13:00", null, null));

                replyMessage = new TemplateMessage("Button Template",
                    new CarouselTemplate(new List<CarouselColumn> {
                        new CarouselColumn("Casousel 1 Text", "https://github.com/apple-touch-icon.png",
                        "Casousel 1 Title", actions1),
                        new CarouselColumn("Casousel 1 Text", "https://github.com/apple-touch-icon.png",
                        "Casousel 1 Title", actions2)
                    }));
            }
            // else if (userMessage == "imagecarousel")
            // {
            //     UriTemplateAction action = new UriTemplateAction("Uri Label", "https://github.com/kenakamu");

            //     replyMessage = new TemplateMessage("ImageCarouselTemplate",
            //         new ImageCarouselTemplate(new List<ImageCarouselColumn> {
            //             new ImageCarouselColumn("https://github.com/apple-touch-icon.png", action),
            //             new ImageCarouselColumn("https://github.com/apple-touch-icon.png", action),
            //             new ImageCarouselColumn("https://github.com/apple-touch-icon.png", action),
            //             new ImageCarouselColumn("https://github.com/apple-touch-icon.png", action),
            //             new ImageCarouselColumn("https://github.com/apple-touch-icon.png", action)
            //         }));
            // }
            // else if (userMessage == "imagemap")
            // {
            //     var url = HttpContext.Current.Request.Url;
            //     var imageUrl = $"{url.Scheme}://{url.Host}:{url.Port}/images/githubicon";
            //     replyMessage = new ImagemapMessage(
            //         imageUrl,
            //         "GitHub",
            //         new ImagemapSize(1040, 1040), new List<IImagemapAction>
            //         {
            //             new UriImagemapAction(new ImagemapArea(0, 0, 520, 1040), "http://github.com"),
            //             new MessageImagemapAction(new ImagemapArea(520, 0, 520, 1040), "I love LINE!")
            //         });
            // }
            // else if (userMessage == "addrichmenu")
            // {
            //     // Create Rich Menu
            //     RichMenu richMenu = new RichMenu()
            //     {
            //         Size = ImagemapSize.RichMenuLong,
            //         Selected = false,
            //         Name = "nice richmenu",
            //         ChatBarText = "touch me",
            //         Areas = new List<ActionArea>()
            //         {
            //             new ActionArea()
            //             {
            //                 Bounds = new ImagemapArea(0,0 ,ImagemapSize.RichMenuLong.Width,ImagemapSize.RichMenuLong.Height),
            //                 Action = new PostbackTemplateAction("ButtonA", "Menu A", "Menu A")
            //             }
            //         }
            //     };

            //     var richMenuId = await messagingClient.CreateRichMenuAsync(richMenu);
            //     var image = new MemoryStream(File.ReadAllBytes(HttpContext.Current.Server.MapPath(@"~\Images\richmenu.PNG")));
            //     // Upload Image
            //     await messagingClient.UploadRichMenuPngImageAsync(image, richMenuId);
            //     // Link to user
            //     await messagingClient.LinkRichMenuToUserAsync(userId, richMenuId);

            //     replyMessage = new TextMessage("Rich menu added");
            // }
            // else if (userMessage == "deleterichmenu")
            // {
            //     // Get Rich Menu for the user
            //     var richMenuId = await messagingClient.GetRichMenuIdOfUserAsync(userId);
            //     await messagingClient.UnLinkRichMenuFromUserAsync(userId);
            //     await messagingClient.DeleteRichMenuAsync(richMenuId);
            //     replyMessage = new TextMessage("Rich menu deleted");
            // }
            // else if (userMessage == "deleteallrichmenu")
            // {
            //     // Get Rich Menu for the user
            //     var richMenuList = await messagingClient.GetRichMenuListAsync();
            //     foreach (var richMenu in richMenuList)
            //     {
            //         await messagingClient.DeleteRichMenuAsync(richMenu.RichMenuId);
            //     }
            //     replyMessage = new TextMessage("All rich menu added");
            // }
            else
            {
                replyMessage = new TextMessage(userMessage);
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
        }

        // /// <summary>
        // /// Upload the received data to blob and returns the address
        // /// </summary>
        // private async Task HandleMediaAsync(string replyToken, string messageId, string blobDirectoryName, string blobName)
        // {
        //     var stream = await messagingClient.GetContentStreamAsync(messageId);
        //     var ext = GetFileExtension(stream.ContentHeaders.ContentType.MediaType);
        //     var uri = await blobStorage.UploadFromStreamAsync(stream, blobDirectoryName, blobName + ext);
        //     await messagingClient.ReplyMessageAsync(replyToken, uri.ToString());
        // }

        /// <summary>
        /// Reply the location user send.
        /// </summary>
        private async Task HandleLocationAsync(string replyToken, LocationEventMessage location)
        {
            await messagingClient.ReplyMessageAsync(replyToken, new[] {
                        new LocationMessage("Location", location.Address, location.Latitude, location.Longitude)
                    });
        }

        /// <summary>
        /// Replies random sticker
        /// Sticker ID of bssic stickers (packge ID =1)
        /// see https://devdocs.line.me/files/sticker_list.pdf
        /// </summary>
        private async Task HandleStickerAsync(string replyToken)
        {
            var stickerids = Enumerable.Range(1, 17)
                .Concat(Enumerable.Range(21, 1))
                .Concat(Enumerable.Range(100, 139 - 100 + 1))
                .Concat(Enumerable.Range(401, 430 - 400 + 1)).ToArray();

            var rand = new Random(Guid.NewGuid().GetHashCode());
            var stickerId = stickerids[rand.Next(stickerids.Length - 1)].ToString();
            await messagingClient.ReplyMessageAsync(replyToken, new[] {
                        new StickerMessage("1", stickerId)
                    });
        }

        private string GetFileExtension(string mediaType)
        {
            switch (mediaType)
            {
                case "image/jpeg":
                    return ".jpeg";
                case "audio/x-m4a":
                    return ".m4a";
                case "video/mp4":
                    return ".mp4";
                default:
                    return "";
            }
        }
    }
}
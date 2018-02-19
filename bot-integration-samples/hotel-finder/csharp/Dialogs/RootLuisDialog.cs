namespace LuisBot.Dialogs
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using LuisBot.Model;


    [LuisModel("7613da72-8006-4d27-bc0b-4a96e723d5bf", "e9d4cd55c7224d57b0c93cce45713594")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityGeographyCity = "builtin.geography.city";

        private const string EntityHotelName = "Hotel";

        private const string EntityAirportCode = "AirportCode";

        private IList<string> titleOptions = new List<string> { "“Very stylish, great stay, great staff”",
            "“good hotel awful meals”", "“Need more attention to little things”", "“Lovely small hotel ideally situated to explore the area.”",
            "“Positive surprise”", "“Beautiful suite and resort”" };



        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            if (result.Query == "bookflight")
            {

            }
            else if (result.Query == "saveforlater")
            {

            }



            string message = $"{AppConstant.UnableToUnderstandCommandMessage} '{result.Query}'. {AppConstant.TypeHelpMessage}";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("search")]
        public async Task SearchV2(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            
            // post message to bot console //
            //await context.PostAsync($"{AppConstant.SearchingTextMessage}: '{message.Text}' {AppConstant.DotDotDotMessage}");


            if (result != null)
            {
                context.Call(new BookingDialog(result), this.OnResumeBookingDialog);
            }
          

            //var hotelsQuery = new HotelsQuery();

            //EntityRecommendation cityEntityRecommendation;

            //if (result.TryFindEntity(EntityGeographyCity, out cityEntityRecommendation))
            //{
            //    cityEntityRecommendation.Type = "Destination";
            //}

           // var hotelsFormDialog = new FormDialog<HotelsQuery>(hotelsQuery, this.BuildHotelsForm, FormOptions.PromptInStart, result.Entities);

            //context.Call(hotelsFormDialog, this.ResumeAfterHotelsFormDialog);
        }

        private async Task OnResumeBookingDialog(IDialogContext context, IAwaitable<BookingInfo> result)
        {

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ShowHotelsReviews")]
        public async Task Reviews(IDialogContext context, LuisResult result)
        {
            EntityRecommendation hotelEntityRecommendation;

            if (result.TryFindEntity(EntityHotelName, out hotelEntityRecommendation))
            {
                await context.PostAsync($"Looking for reviews of '{hotelEntityRecommendation.Entity}'...");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 5; i++)
                {
                    var random = new Random(i);
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = this.titleOptions[random.Next(0, this.titleOptions.Count - 1)],
                        Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris odio magna, sodales vel ligula sit amet, vulputate vehicula velit. Nulla quis consectetur neque, sed commodo metus.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = "https://upload.wikimedia.org/wikipedia/en/e/ee/Unknown-person.gif" }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(AppConstant.ChatHelpMessage);

            context.Wait(this.MessageReceived);
        }

        private IForm<HotelsQuery> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<HotelsQuery> processHotelsSearch = async (context, state) =>
            {
                var message = "Searching for hotels";
                if (!string.IsNullOrEmpty(state.Destination))
                {
                    message += $" in {state.Destination}...";
                }
                else if (!string.IsNullOrEmpty(state.AirportCode))
                {
                    message += $" near {state.AirportCode.ToUpperInvariant()} airport...";
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<HotelsQuery>()
                .Field(nameof(HotelsQuery.Destination), (state) => string.IsNullOrEmpty(state.AirportCode))
                .Field(nameof(HotelsQuery.AirportCode), (state) => string.IsNullOrEmpty(state.Destination))
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterHotelsFormDialog(IDialogContext context, IAwaitable<HotelsQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var hotels = await this.GetHotelsAsync(searchQuery);

                await context.PostAsync($"{AppConstant.FoundHotelMessage} {hotels.Count()} {AppConstant.HotelTextMessageNotification}");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"Ratings:{hotel.Rating}",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = AppConstant.CancelOperationMessage;
                }
                else
                {
                    reply = $"{AppConstant.OpsssTextMessage} {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {
            var url = string.Empty;

            var apiKey = "AIzaSyCZtwsY0V0YLyRy0XhIZZ5bjJuZ7qpgK-c";

            if (!string.IsNullOrEmpty(searchQuery.Destination))
            {
                url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query=hotel%20{searchQuery.Destination}&l&key={apiKey}";

            }

            var hotels = new List<Hotel>();

            using (var client = new HttpClient())
            {
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    string result = await r.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<HotelResult>(result);

                    for (int i = 0; i < 5; i++)
                    {
                        Hotel hotel = new Hotel()
                        {
                            Name = data.Results[i].Name,
                            Location = data.Results[0].Formatted_address,
                            Rating = data.Results[i].Rating,

                        };
                        hotels.Add(hotel);
                    }
                }
            }

            return hotels;
        }
    }
}

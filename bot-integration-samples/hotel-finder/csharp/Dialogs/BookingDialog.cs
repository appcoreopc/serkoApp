using LuisBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LuisBot.Dialogs
{
    // NameDialog
    [Serializable]
    public class BookingDialog : IDialog<BookingInfo>
    {

        private LuisResult _result;

        public BookingDialog(LuisResult result)
        {
            _result = result;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Please wait while we get your booking ready");

            // Fake API Calls //

            FakeApiCalls(context, this._result);


            context.Wait(this.MessageReceivedAsync);
        }

        private void FakeApiCalls(IDialogContext context, LuisResult result)
        {
            var bookingsForUser = context.MakeMessage();
            bookingsForUser.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            bookingsForUser.Attachments = new List<Attachment>();

            (string FromLocation, string ToLocation) = ResolvePlace(result.Entities);
            (DateTime? start, DateTime? endDate) = ResolveTravellingDates(result.Entities);
            var titleMessage = $"Travelling from {FromLocation} to {ToLocation} on {start?.ToShortDateString()} till {endDate?.ToShortDateString()}";



            var bookItem = new HeroCard()
            {
                Title = titleMessage,
                Subtitle = "User Action",
                Buttons = new List<CardAction>()
                 {
                        new CardAction()
                        {
                             Title = "Book flight",
                             Type = ActionTypes.OpenUrl
                        },
                          new CardAction()
                        {
                             Title = "Book hotel",
                             Type = ActionTypes.OpenUrl
                        },
                           new CardAction()
                        {
                             Title = "Book cars",
                             Type = ActionTypes.OpenUrl
                        },

                             new CardAction()
                        {
                             Title = "Book all (flight, hotel and car)",
                             Type = ActionTypes.OpenUrl
                        },
                           new CardAction()
                        {
                             Title = "Save for later",
                             Type = ActionTypes.OpenUrl
                        }
                   }
            };

            bookingsForUser.Attachments.Add(bookItem.ToAttachment());
            context.PostAsync(bookingsForUser);

        }

        private (string FromLocation, string ToLocation) ResolvePlace(IList<EntityRecommendation> entities)
        {
            var result = GetAllItemByType("place", entities);

            if (result != null)
            {
                var orderedDestination = result.OrderBy(x => x.StartIndex);

                var startLocation = orderedDestination.First().Entity;
                var destination = orderedDestination.Last().Entity;

                return (startLocation, destination);

            }
            return (null, null);
        }

        private (DateTime? start, DateTime? endDate) ResolveTravellingDates(IList<EntityRecommendation> entities)
        {
            var result = GetItemByType("builtin.datetimeV2.daterange", entities);

            if (result != null)
            {
                if (result.Entity != null)
                {
                    try
                    {
                        var valueIsFound = result.Resolution.TryGetValue("values", out object appData);

                        var ap = appData as JArray;
                        var ap2 = ap.First().ToString();

                        var arrayData = JsonConvert.DeserializeObject<TravellingDates>(ap2);
                        return (arrayData.Start, arrayData.End);
                    }
                    catch (Exception ex)
                    {
                        var x = ex.Message;
                    }

                }
            }
            return (null, null);
        }

        private EntityRecommendation GetItemByType(string typename, IList<EntityRecommendation> sourceRecommendationType)
        {
            foreach (var item in sourceRecommendationType)
            {
                if (string.Equals(item.Type, typename, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        private IEnumerable<EntityRecommendation> GetAllItemByType(string typename, IList<EntityRecommendation> sourceRecommendationType)
        {
            foreach (var item in sourceRecommendationType)
            {
                if (string.Equals(item.Type, typename, StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;

        }
    }


    class TravellingDates
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

    }
}
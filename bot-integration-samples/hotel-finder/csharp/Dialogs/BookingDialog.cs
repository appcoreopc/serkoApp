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
using LuisBot.Util;

namespace LuisBot.Dialogs
{
    // NameDialog

    //new TextInput()
    //{
    //    Id = "Destination",
    //            Speak = "<s>Please enter your destination</s>",
    //            Placeholder = "Miami, Florida",
    //            Style = TextInputStyle.Text
    //        },
    //        new TextBlock() { Text = "When do you want to check in?" },
    //        new DateInput()
    //{
    //    Id = "Checkin",
    //            Speak = "<s>When do you want to check in?</s>"
    //        },
    //        new TextBlock() { Text = "How many nights do you want to stay?" },
    //        new NumberInput()
    //{
    //    Id = "Nights",
    //            Min = 1,
    //            Max = 60,
    //            Speak = "<s>How many nights do you want to stay?</s>"
    //  }

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

            var layout = new CardMessageUtil(context).MakeMessage(new HotelCardLayout().CreateHotelLayoutFromResult(titleMessage, null));
            context.PostAsync(layout);

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
using AdaptiveCards;
using LuisBot.Model;
using System.Collections.Generic;

namespace LuisBot.Util
{
    public class HotelCardLayout
    {
        public AdaptiveCard CreateHotelLayoutFromResult(string title, IEnumerable<HotelElement> data)
        {
            var intro = new List<CardElement>()
            {
                    new TextBlock()
                    {
                        Text = title,
                        Size = TextSize.ExtraLarge,
                        Speak = $"<s>{title}</s>",
                        Wrap = true
                    }
            };

            var adaptiveCard = new AdaptiveCard()
            {
                Body = intro,
                Actions = new List<ActionBase>() {

                    new SubmitAction()
                    {
                        Title = "Book Flight",
                        Speak = "<s>Book Flight</s>",
                        DataJson = "{ \"Type\": \"BookFlight\" }"
                    },                    

                    new SubmitAction()
                    {
                        Title = "Book Car",  
                        Speak = "<s>Book Car</s>",
                        DataJson = "{ \"Type\": \"BookCar\" }"
                    },

                    new SubmitAction()
                    {
                        Title = "Book Hotel",
                        Speak = "<s>Book Hotel</s>", 
                        DataJson = "{ \"Type\": \"BookHotel\" }"
                    },
                    new SubmitAction()
                    {
                        Title = "Book All",
                        Speak = "<s>Book All</s>", 
                        DataJson = "{ \"Type\": \"BookAll\" }"
                    },
                    new SubmitAction()
                    {
                        Title = "Save later",
                        Speak = "<s>Save later</s>",
                        DataJson = "{ \"Type\": \"SaveLater\" }"
                    },
                }
            };

            return adaptiveCard;
        }
    }

}
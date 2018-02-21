using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace LuisBot.Util
{
    public class CardMessageUtil
    {
        private IMessageActivity _messager;
        IDialogContext _context;

        public CardMessageUtil(IDialogContext context) =>
           _context = context; 
                                       
        public IMessageActivity MakeMessage(AdaptiveCard card)
        {
            _messager = _context.MakeMessage(); 

            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            _messager.Attachments.Add(attachment);

            return _messager;
        }

    }
}
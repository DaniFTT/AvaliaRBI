using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AvaliaRBI._2___Application.Shared;

public class WhatsappService
{
    private readonly string accountSid = "";
    private readonly string authToken = "";
    private readonly string fromWhatsAppNumber = "";

    public WhatsappService()
    {
        TwilioClient.Init(accountSid, authToken);
    }

    public void SendMessage(string messageBody, string toWhatsAppNumber = null)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{toWhatsAppNumber ?? ""}"))
        {
            From = new PhoneNumber($"whatsapp:{fromWhatsAppNumber}"),
            Body = messageBody,
        };

        var message = MessageResource.Create(messageOptions);
        Console.WriteLine($"Message SID: {message.Sid}");
    }
}


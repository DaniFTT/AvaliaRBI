using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AvaliaRBI._2___Application.Shared;

public class WhatsappService
{
    private readonly string accountSid = "ACb3203a5d2869e29d059242e7124a974c";
    private readonly string authToken = "f8495751d592b0c76a6c914634ede52a";
    private readonly string fromWhatsAppNumber = "+14155238886";

    public WhatsappService()
    {
        TwilioClient.Init(accountSid, authToken);
    }

    public void SendMessage(string messageBody, string toWhatsAppNumber = null)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{toWhatsAppNumber ?? "+5511973005415"}"))
        {
            From = new PhoneNumber($"whatsapp:{fromWhatsAppNumber}"),
            Body = messageBody,
            MediaUrl = new List<Uri>() { new Uri("https://drive.google.com/uc?export=download&id=1VYfkcNq3xFQI91WwIhj6Ol1GqPMG3-oc") }
        };

        var message = MessageResource.Create(messageOptions);
        Console.WriteLine($"Message SID: {message.Sid}");
    }
}


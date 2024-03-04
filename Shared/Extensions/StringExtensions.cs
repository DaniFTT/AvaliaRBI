using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account.Usage.Record;

namespace AvaliaRBI.Shared.Extensions;

public static class StringExtensions
{
    public static string NormalizeString(this string text)
    {
        text = text.Trim();
        return Regex.Replace(text, @"\s{2,}", " ");
    }
}


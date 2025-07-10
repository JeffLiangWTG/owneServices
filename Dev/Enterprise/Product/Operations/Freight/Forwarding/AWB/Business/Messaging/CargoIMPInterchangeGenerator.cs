using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public static class CargoIMPInterchangeGenerator
	{
		public static ZString CreateHubHeader(EDIMessage message, ZString priority, ZString carrier, ZString hawb, ZString mawb)
		{
			var sb = new ZStringBuilder();
			sb.AppendLine((NoResString)@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">"); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, @"<MessageType>{0}</MessageType>", message.EM_MessageType)); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, (NoResString)@"<Priority>{0}</Priority>", priority)); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, (NoResString)@"<Carrier>{0}</Carrier>", carrier)); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, @"<CreationDateTime>{0}</CreationDateTime>", ZDateTime.UtcNow.ToString("o", Culture.Invariant))); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, @"<HAWB>{0}</HAWB>", hawb)); // inline XML headr text
			sb.AppendLine(string.Format(Culture.Invariant, @"<MAWB>{0}</MAWB>", mawb)); // inline XML headr text
			var issuingCode = DataRegistry.Instance.GetIssuingCarrierAgentIATACode(message.EM_GB.ToGuid());
			var sanitizedissuingCode = SanitizeXml(issuingCode);
			sb.AppendLine(string.Format(Culture.Invariant, @"<IssuingCarrierAgentIATACode>{0}</IssuingCarrierAgentIATACode>", sanitizedissuingCode)); // inline XML headr text
			sb.AppendLine((NoResString)@"<Body>"); // inline XML headr text
			sb.Append(@"<![CDATA["); // inline XML headr text
			return sb.ToString();
		}

		static string SanitizeXml(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
			input = input.Trim();
			input = input.Replace("&", "")
						 .Replace("<", "")
						 .Replace(">", "")
						 .Replace("\"", "")
						 .Replace("'", "")
						 .Replace(" ", "");

			return input;
		}

		public static ZString CreateHubFooter()
		{
			return (NoResString)@"]]>
</Body>
</CargoIMP>"; // inline XML headr text
		}
	}
}

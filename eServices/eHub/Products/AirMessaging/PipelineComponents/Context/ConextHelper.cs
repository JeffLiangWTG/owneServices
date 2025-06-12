using System;
using System.Text;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	public static class ConextHelper
	{
		public static string ConvertMessageTypeToStandard(string messageType)
		{
			switch (messageType)
			{
				case "CIMFSU": return "FSU";
				case "CIMFMA": return "FMA";
				case "CIMFNA": return "FNA";
				case "CIMFSA": return "FSA";
			}
			return messageType;
		}

		public static string AddOriginalMessage(string InternalMessage, string reference)
		{
			if (!ValidateMAWB(reference)) return InternalMessage;

			string MAWB;
			string HAWB;
			SplitReference(reference, out MAWB, out HAWB);

			if (!ValidateMAWB(MAWB)) return InternalMessage;
			if (!ValidateHAWB(HAWB))
			{
				var message = new StringBuilder(InternalMessage);
				message.AppendLine("");
				message.AppendLine("FWB");
				message.AppendLine(String.Format("{0}-{1}", MAWB.Substring(0, 3), MAWB.Substring(3)));
				return message.ToString();
			}
			else
			{
				var message = new StringBuilder(InternalMessage);
				message.AppendLine("");
				message.AppendLine("FHL");
				message.AppendLine(String.Format("MBI/{0}-{1}/", MAWB.Substring(0, 3), MAWB.Substring(3)));
				message.AppendLine(String.Format("HBS/{0}/////", HAWB));
				return message.ToString();
			}
		}

		public static bool ValidateReference(string reference)
		{
			return reference.Length >= 11;
		}

		public static void SplitReference(string reference, out string MAWB, out string HAWB)
		{
			MAWB = reference.Substring(0, 11);
			HAWB = reference.Substring(11);
		}

		public static bool ValidateMAWB(string MAWB)
		{
			return MAWB.Length >= 11;
		}

		public static bool ValidateHAWB(string HAWB)
		{
			return HAWB.Length > 0;
		}

		public static string BuildAWBFromMessage(string message)
		{
			if (String.IsNullOrEmpty(message))
				return message;
			var rex = System.Text.RegularExpressions.Regex.Match(message, @"(\d{3}-\d{8}[A-Z]{6})");

			if (!rex.Success)
				return null;
			return rex.Groups[0].Value.Substring(0, 12).Replace("-", string.Empty);
		}
	}
}

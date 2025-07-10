using System.Globalization;
using System.Text;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class PortAuthorityMessageBuilderExtentions
	{
		public static string GetInterchangeText(this IPortAuthorityMessageBuilder builder, IPortAuthorityMessagingData data, ZDateTime preperationTime, string senderID, string recipientID)
		{
			string referenceNumber = preperationTime.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);

			UNBSegment unb = new UNBSegment();
			unb.DateTimeOfPreparation.Date = preperationTime.ToString("yyMMdd", CultureInfo.InvariantCulture);
			unb.DateTimeOfPreparation.Time = preperationTime.ToString("HHmm", CultureInfo.InvariantCulture);
			unb.InterchangeControlReference = referenceNumber;
			unb.InterchangeRecipient.RecipientIdentification = recipientID;
			unb.InterchangeSender.SenderIdentification = senderID;
			unb.SyntaxIdentifier.SyntaxIdentifier = "UNOA";
			unb.SyntaxIdentifier.SyntaxVersionNumber = "1";

			UNZSegment unz = new UNZSegment();
			unz.InterchangeControlCount = "1";
			unz.InterchangeControlReference = referenceNumber;

			UNCharacterSet characterSet = new UNOACharacterSet();

			StringBuilder result = new StringBuilder();
			result.Append(EDIInterchange.UNOAUNAString);
			result.Append(unb.ToString(characterSet));
			result.Append(builder.GenerateMessageText(data).Replace(EDIMessage.MessageNumberPlaceHolder, referenceNumber));
			result.Append(unz.ToString(characterSet));

			return result.ToString();
		}
	}
}

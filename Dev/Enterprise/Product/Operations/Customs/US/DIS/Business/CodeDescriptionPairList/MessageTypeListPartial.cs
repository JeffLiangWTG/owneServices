using CargoWise.Types;
using MessageTypes = Enterprise.Customs.US.DIS.Messaging.DataFileSchema.Constants.enumMessageType;

namespace Enterprise.Customs.US.DIS.Business
{
	partial class MessageTypeList
	{
		public static string GetCodeFrom(ZString messageType)
		{
			if (messageType.EqualsIgnoringCase(MessageTypes.DocumentSubmission))
			{
				return Codes.Submission;
			}
			else if (messageType.EqualsIgnoringCase(MessageTypes.DocumentReviewResponse))
			{
				return Codes.DocumentReviewResponse;
			}
			else if (messageType.EqualsIgnoringCase(MessageTypes.DocumentValidationResponse))
			{
				return Codes.DocumentValidationResponse;
			}
			else if (messageType.EqualsIgnoringCase(MessageTypes.RequestedData))
			{
				return Codes.RequestedData;
			}
			else if (messageType.EqualsIgnoringCase(MessageTypes.RequestForData))
			{
				return Codes.Request;
			}
			return Codes.Other;
		}
	}
}

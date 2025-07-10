using MessageTypes = Enterprise.Customs.US.DIS.Messaging.DataFileSchema.Constants.enumMessageType;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class MessageTypeListTest : NUnit.Framework.TestCase
	{
		public void TestGetCodeFrom()
		{
			AssertEquals(MessageTypeList.Codes.Submission, MessageTypeList.GetCodeFrom(MessageTypes.DocumentSubmission));
			AssertEquals(MessageTypeList.Codes.DocumentReviewResponse, MessageTypeList.GetCodeFrom(MessageTypes.DocumentReviewResponse));
			AssertEquals(MessageTypeList.Codes.DocumentValidationResponse, MessageTypeList.GetCodeFrom(MessageTypes.DocumentValidationResponse));
			AssertEquals(MessageTypeList.Codes.RequestedData, MessageTypeList.GetCodeFrom(MessageTypes.RequestedData));
			AssertEquals(MessageTypeList.Codes.Request, MessageTypeList.GetCodeFrom(MessageTypes.RequestForData));
			AssertEquals(MessageTypeList.Codes.Other, MessageTypeList.GetCodeFrom(MessageTypes.Other));
		}
	}
}

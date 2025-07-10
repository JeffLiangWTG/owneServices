using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_AXMessageSendingObjectValidation))]
	sealed class NX301_AXMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckAction()
		{
			var messageSendingObject = new NX301_AXMessageSendingObject(Factory.NewWithValidTestData<CusTWControllingMessageHeader>());
			var targetInfo = messageSendingObject.ActionInfo;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "1", "9");
		}
	}
}

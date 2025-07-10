using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageErrorTest : TestCaseWithFactory
	{
		public void TestMessageErrorProperty()
		{
			MessageError messageError = new MessageError(
				errorCode: "test1",
				shortDesc: "This is test MessageError property",
				narrative: "test property"
			);

			AssertNotNull(messageError);
			AssertEquals("test1", messageError.ErrorCode);
			AssertEquals("This is test MessageError property", messageError.ShortDesc);
			AssertEquals("test property", messageError.Narrative);

			MessageError messageError2 = new MessageError(
				errorCode: "test1    ",
				shortDesc: "This is test MessageError property				",
				narrative: "test property			"
			);

			AssertNotNull(messageError2);
			AssertEquals("test1", messageError2.ErrorCode);
			AssertEquals("This is test MessageError property", messageError2.ShortDesc);
			AssertEquals("test property", messageError2.Narrative);
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class InBondCommonErrorsTest : TestCaseWithFactory
	{
		public void TestGetMessageTextFromDescription()
		{
			var commonErrors = new InBondCommonErrors();
			var shortDescription = commonErrors.GetShortDescription("081");
			AssertEquals("MANIFEST TRANSMITTAL", shortDescription);

			var longDescription = commonErrors.GetLongDescription("081");
			AssertEquals("This message is not currently being used.", longDescription);

			var messageTextFromDescription = commonErrors.GetMessageTextFromDescription("MANIFEST TRANSMITTAL");
			AssertEquals("MANIFEST TRANSMITTAL<p />This message is not currently being used.", messageTextFromDescription);
		}
	}
}

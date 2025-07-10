using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	abstract class BaseEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestHasStatusOrErrors()
		{
			var message = GetEDIMessage();
			Assert("Default to true.", message.HasStatusOrErrors);
		}

		public void TestStatusesAndErrors()
		{
			var message = GetEDIMessage();
			AssertEquals(typeof(StatusErrorsDataViewCollection), message.StatusesAndErrors.GetType());
			AssertEquals(message.IsTransmitMessage, message.StatusesAndErrorsVisible);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals("No Statuses/Errors available on outgoing messages", message.StatusesErrorsExist);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals(string.Empty, message.StatusesErrorsExist);
			AssertEquals(ExpectedCountOfReconOriginalEntries, message.CountOfReconOriginalEntries);
		}

		public abstract void TestIsInterpretationInHtmlFormat();

		protected abstract BaseEDIMessage GetEDIMessage();

		protected virtual ZString ExpectedCountOfReconOriginalEntries => ZString.Empty;
	}
}

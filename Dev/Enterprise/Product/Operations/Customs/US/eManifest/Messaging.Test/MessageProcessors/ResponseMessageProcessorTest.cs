using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	abstract class ResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestHVLVRegistrySettings()
		{
			var processor = GetProcessorForTest();
			CombineAssertions("when trip is not from HVLV, email mode all from normal registry setting", () =>
			{
				AssertEquals("ESG", processor.AcknowledgementEmailMode);
				AssertEquals("ESG", processor.ImpedimentEmailMode);
				AssertEquals("ESG", processor.ErrorEmailMode);
			});
			var trip = processor.originalMessage.EM_LinkedObject as Trip;
			trip.Logs.AddNew(Events.Transferred, "|TYP=HVL");
			CombineAssertions("when trip is from HVLV, email mode all from HVLV registry setting", () =>
			{
				AssertEquals("NOE", processor.AcknowledgementEmailMode);
				AssertEquals("NOE", processor.ImpedimentEmailMode);
				AssertEquals("NOE", processor.ErrorEmailMode);
			});
		}

		protected abstract IProcessorForTest GetProcessorForTest();

		protected interface IProcessorForTest
		{
			string AcknowledgementEmailMode { get; }

			string ImpedimentEmailMode { get; }

			string ErrorEmailMode { get; }

			EDIMessage originalMessage { get; }
		}
	}
}

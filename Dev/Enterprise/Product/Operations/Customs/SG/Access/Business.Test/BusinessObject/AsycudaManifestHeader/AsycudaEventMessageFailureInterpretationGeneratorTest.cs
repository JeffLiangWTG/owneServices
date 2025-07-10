using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaEventMessageFailureInterpretationGeneratorTest : TestCaseWithFactory
	{
		public void TestHouseBillImportConsignmentContaining2SpecificErrors()
		{
			AssertFormattedHTMLMatchesForUniversalEvent("SGAccessEventConsignmentHasTwoDifferentErrors.xml", "FormattedEventConsignmentWithTwoDifferentErrors.html");
		}

		public void TestHouseBillImportConsignmentContaining1SpecificErrorAndTheGenericZ0()
		{
			AssertFormattedHTMLMatchesForUniversalEvent("SGAccessEventConsignmentWithGenericErrorCodeZ0.xml", "FormattedEventConsignmentWithGenericErrorCodeZ0.html");
		}

		public void TestMulitpleImportHouseBillsWithMultipleConsignments()
		{
			AssertFormattedHTMLMatchesForUniversalEvent("SGAccessEventConsignmentHasTwoHouseBills.xml", "FormattedEventConsignmentHasTwoHouseBills.html");
		}

		public void TestExportEventStillGroupsByHouseBill()
		{
			AssertFormattedHTMLMatchesForUniversalEvent("SGAccessEventExportShipment.xml", "FormattedEventExportShipment.html");
		}

		public void TestGenerateWithNullableContexts()
		{
			AssertFormattedHTMLMatchesForUniversalEvent("SGAccessEventWithNullableContexts.xml", "FormattedEventWithNullableContexts.html");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ASYCUDA.Business.Testing.ZZDataTestHelper(Factory);
			Factory.Save();
			message = Factory.New<AsycudaEDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var mrrLog = header.Logs.AddNew(Events.MessageReceived);
			message.AddUniversalDataLink(mrrLog);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		AsycudaEDIMessage message;
		EmbeddedResourceRetriever embeddedResourceRetriever;
		void AssertFormattedHTMLMatchesForUniversalEvent(string universalEventFileName, string formattedHTMLFileName)
		{
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath(universalEventFileName));
			var formattedHtml = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath(formattedHTMLFileName)).Replace(System.Environment.NewLine, ZString.Empty);
			AssertMultilineASCIIEquals("", formattedHtml, message.EM_MessageInterpretation);
		}

		string GetEmbeddedResourcePath(string filename) => "Enterprise.Customs.SG.Access.Business.Testing.BusinessObject.AsycudaManifestHeader.TestFiles." + filename;
	}
}

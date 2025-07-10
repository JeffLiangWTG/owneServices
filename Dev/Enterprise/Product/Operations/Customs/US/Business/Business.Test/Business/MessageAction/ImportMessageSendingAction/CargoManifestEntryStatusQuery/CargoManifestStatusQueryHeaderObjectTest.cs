using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestStatusQueryHeaderObject))]
	sealed class CargoManifestStatusQueryHeaderObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendDataForDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryFilerCode = "CJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = 9000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var sendingObject = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingObject.SendingObjects[0].ShouldSendMessage = true;
			AssertEquals("1 message was created.", 1, sendingObject.SendQueryMessage());
			AssertEquals(1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0].EM_MessageType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CargoManifestStatusQueryHeaderObject(declaration);
		}
	}
}

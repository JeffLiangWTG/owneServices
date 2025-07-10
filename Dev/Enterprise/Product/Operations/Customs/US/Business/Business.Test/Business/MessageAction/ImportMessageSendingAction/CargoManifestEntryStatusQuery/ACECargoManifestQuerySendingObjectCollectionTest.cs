using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestQuerySendingObjectCollection))]
	sealed class ACECargoManifestQuerySendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CargoManifestQuerySendingObjectCollection>
	{
		public void TestPopulateForDeclaration()
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
			declaration.JE_MasterBill = "Master1";
			declaration.JE_HouseBill = "House11";
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
			var collection = new CargoManifestQuerySendingObjectCollection(new CargoManifestStatusQueryHeaderObject(declaration));
			AssertEquals("Should be Entry, Master Bill, House Bill", 3, collection.Count);
			collection.PopulateObjects(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill);
			AssertEquals("Should be Master Bill, House Bill", 2, collection.Count);
			collection.PopulateObjects(CargoManifestStatusQueryActionList.Codes.Entry);
			AssertEquals("Should be Entry only", 1, collection.Count);
		}

		protected override Type GetExpectedCollectionType() => typeof(CargoManifestQuerySendingObjectCollection);

		protected override CargoManifestQuerySendingObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var result = new CargoManifestQuerySendingObjectCollection(new CargoManifestStatusQueryHeaderObject(declaration));
			result.HasChanges = false;
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "TestBill";
			var header = new CargoManifestStatusQueryHeaderObject(declaration);
			return new CargoManifestQuerySendingObject(header, declaration.PrimaryMasterBill);
		}
	}
}

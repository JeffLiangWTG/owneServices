using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEFDACollection))]
	public class ACEFDACollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			declaration.CopyLastFDADetailsToNewLine = true;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 218m;
			var fdaLineToBeDeleted = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(218m, fdaLineToBeDeleted.US_InvCurrValue);
			fdaLineToBeDeleted.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(218m, fdaLine.US_InvCurrValue);
			fdaLine.US_InvCurrValue = 200m;

			var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(18m, fdaLine2.US_InvCurrValue);
		}

		public void TestCopyACEFDALineSetValueToZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLineShipper = Factory.New<OrgHeader>();
			invoiceLineShipper.OH_Code = "INVLINE";
			invoiceLineShipper.OH_FullName = "INVLINE FULL NAME";
			invoiceLineShipper.MainAddress.OA_Address1 = "INVLINE ADDRESS";
			invoiceLine.JI_OA_FDAShipperAddress = invoiceLineShipper.MainAddress.PK;

			var invoiceLineManufacturer = Factory.New<OrgHeader>();
			invoiceLineManufacturer.OH_Code = "MANU";
			invoiceLineManufacturer.OH_FullName = "MANUFACTURER NAME";
			invoiceLineManufacturer.MainAddress.OA_Address1 = "MANU ADDRESS";
			invoiceLine.JI_LinePrice = 218m;
			invoiceLine.JI_OA_ManufacturerAddress = invoiceLineManufacturer.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHIP";
			shipper.OH_FullName = "SHIPPER NAME";
			shipper.MainAddress.OA_Address1 = "ABC ADDRESS 1";

			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_OA_ShipperAddress = shipper.MainAddress.PK;
			fdaLine1.US_DeliverToPartyAddress = shipper.MainAddress.PK;
			fdaLine1.US_FDAImporterAddress = shipper.MainAddress.PK;
			fdaLine1.US_OwnerAddress = shipper.MainAddress.PK;
			fdaLine1.US_ProducerType = "T";
			fdaLine1.US_LocationOfGoodsAddress = shipper.MainAddress.PK;
			declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			AssertEquals(fdaLine1.US_OA_ShipperAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine1.US_OA_ShipperAddress, shipper.MainAddress.PK);

			var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(fdaLine2.US_InvCurrValue, decimal.Zero);
		}

		public void TestCopyPreviousACEFDALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLineShipper = Factory.New<OrgHeader>();
			invoiceLineShipper.OH_Code = "INVLINE";
			invoiceLineShipper.OH_FullName = "INVLINE FULL NAME";
			invoiceLineShipper.MainAddress.OA_Address1 = "INVLINE ADDRESS";
			invoiceLine.JI_OA_FDAShipperAddress = invoiceLineShipper.MainAddress.PK;

			var invoiceLineManufacturer = Factory.New<OrgHeader>();
			invoiceLineManufacturer.OH_Code = "MANU";
			invoiceLineManufacturer.OH_FullName = "MANUFACTURER NAME";
			invoiceLineManufacturer.MainAddress.OA_Address1 = "MANU ADDRESS";
			invoiceLine.JI_LinePrice = 218m;
			invoiceLine.JI_OA_ManufacturerAddress = invoiceLineManufacturer.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHIP";
			shipper.OH_FullName = "SHIPPER NAME";
			shipper.MainAddress.OA_Address1 = "ABC ADDRESS 1";

			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_OA_ShipperAddress = shipper.MainAddress.PK;
			fdaLine1.US_DeliverToPartyAddress = shipper.MainAddress.PK;
			fdaLine1.US_FDAImporterAddress = shipper.MainAddress.PK;
			fdaLine1.US_OwnerAddress = shipper.MainAddress.PK;
			fdaLine1.US_ProducerType = "T";
			fdaLine1.US_LocationOfGoodsAddress = shipper.MainAddress.PK;
			declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			AssertEquals(fdaLine1.US_OA_ShipperAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine1.US_OA_ShipperAddress, shipper.MainAddress.PK);

			var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(fdaLine2.US_OA_ShipperAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine2.US_DeliverToPartyAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine2.US_FDAImporterAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine2.US_OwnerAddress, shipper.MainAddress.PK);
			AssertEquals(fdaLine2.US_ProducerType, fdaLine1.US_ProducerType);
			AssertEquals(fdaLine2.US_LocationOfGoodsAddress, shipper.MainAddress.PK);
		}

		public void TestDoNotDefaultInvValuesForStandAlonePriorNotice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals("Should not default line value for standalone prior notice", 0m, fda2.US_InvCurrValue);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.ACE_FDALines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.ACE_FDALines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			return new ACEFDACollection(invoiceLine);
		}
	}
}

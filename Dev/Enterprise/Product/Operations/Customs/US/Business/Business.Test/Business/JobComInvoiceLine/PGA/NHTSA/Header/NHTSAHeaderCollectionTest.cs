using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAHeaderCollection))]
	public class NHTSAHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValuesForOrg()
		{
			var header = InvoiceLine.NHTSALines.AddNew();
			AssertEquals(ZGuid.Empty, header.US_NHTFabricatingMFRAddress);
			AssertEquals(ZGuid.Empty, header.US_OA_NHTRetailer);

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTMFR";
			InvoiceLine.JI_OA_ManufacturerAddress = orgHeader1.MainAddress.PK;

			header = InvoiceLine.NHTSALines.AddNew();
			AssertEquals(orgHeader1.MainAddress.PK, header.US_NHTFabricatingMFRAddress);
			AssertEquals(ZGuid.Empty, header.US_OA_NHTRetailer);

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTRET";
			InvoiceLine.JI_OA_SoldToPartyAddress = orgHeader2.MainAddress.PK;

			header = InvoiceLine.NHTSALines.AddNew();
			AssertEquals(orgHeader1.MainAddress.PK, header.US_NHTFabricatingMFRAddress);
			AssertEquals(orgHeader2.MainAddress.PK, header.US_OA_NHTRetailer);
		}

		public void TestCopyPreviousNHTSALine()
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
			invoiceLineManufacturer.OH_Code = "MAN2";
			invoiceLineManufacturer.OH_FullName = "MANUFACTURER NAME2";
			invoiceLineManufacturer.MainAddress.OA_Address1 = "MANU2 ADDRESS";
			invoiceLine.JI_OA_ManufacturerAddress = invoiceLineManufacturer.MainAddress.PK;

			var invoiceLineSoldParty = Factory.New<OrgHeader>();
			invoiceLineSoldParty.OH_Code = "RETAI";
			invoiceLine.JI_OA_SoldToPartyAddress = invoiceLineSoldParty.MainAddress.PK;
			invoiceLine.JI_LinePrice = 218m;

			var nhtsaLineRetailer = Factory.New<OrgHeader>();
			nhtsaLineRetailer.OH_Code = "LINRET";
			nhtsaLineRetailer.MainAddress.OA_Address1 = "LINE RETAILER ADDRESS";

			var fabricatingMFRAddress = Factory.New<OrgHeader>();
			fabricatingMFRAddress.OH_Code = "MFR";
			fabricatingMFRAddress.OH_FullName = "MFR NAME";
			fabricatingMFRAddress.MainAddress.OA_Address1 = "MFR ADDRESS 1";

			var nhtsaLine1 = invoiceLine.NHTSALines.AddNew();
			nhtsaLine1.US_OA_NHTRetailer = nhtsaLineRetailer.PK;
			nhtsaLine1.US_NHTFabricatingMFRAddress = fabricatingMFRAddress.MainAddress.PK;
			AssertEquals(nhtsaLine1.US_NHTFabricatingMFRAddress, fabricatingMFRAddress.MainAddress.PK);
			AssertEquals(nhtsaLine1.US_OA_NHTRetailer, nhtsaLineRetailer.PK);
			AssertEquals(1, nhtsaLine1.NHTSADocuments.Count);
			var document1 = nhtsaLine1.NHTSADocuments.AddNew();
			document1.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Owner;
			document1.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			var document2 = nhtsaLine1.NHTSADocuments.AddNew();
			document2.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.OriginalVehicleManufacturer;
			document2.US_NHTDocumentType = NHTSADocumentTypeList.Codes._958;
			AssertEquals(3, nhtsaLine1.NHTSADocuments.Count);
			Factory.Save();

			declaration.CopyLastPGADetailsToNewLine = true;
			var nhtsaLine2 = invoiceLine.NHTSALines.AddNew();
			AssertEquals(nhtsaLine2.US_NHTFabricatingMFRAddress, fabricatingMFRAddress.MainAddress.PK);
			AssertEquals(nhtsaLine2.US_OA_NHTRetailer, nhtsaLineRetailer.PK);
			AssertEquals(3, nhtsaLine2.NHTSADocuments.Count);
			AssertEquals(NHTSADocumentTypeList.Codes._875, nhtsaLine2.NHTSADocuments[1].US_NHTDocumentType);
			AssertEquals(NHTSAOrganizationTypeList.Codes.Owner, nhtsaLine2.NHTSADocuments[1].US_NHTDocumentOwner);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.NHTSALines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.NHTSALines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return InvoiceLine.NHTSALines;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					Factory.Save();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}

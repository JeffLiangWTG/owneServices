using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSLineCollection))]
	public class NMFSLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.NMFSLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.NMFSLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		public void TestDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			AssertEquals(ZString.Empty, nmfsLine.US_ProgramType);
			AssertEquals(ZDecimal.Zero, nmfsLine.US_NetWeight);
			AssertEquals(ZString.Empty, nmfsLine.US_NetWeightUQ);

			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			nmfsLine = invoiceLine.NMFSLines.AddNew();
			AssertEquals(NMFSProgramCodeList.Codes.SIM, nmfsLine.US_ProgramType);
			AssertEquals(100m, nmfsLine.US_NetWeight);
			AssertEquals("KG", nmfsLine.US_NetWeightUQ);

			nmfsLine.US_NetWeight = 75m;
			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			AssertEquals(NMFSProgramCodeList.Codes.SIM, nmfsLine2.US_ProgramType);
			AssertEquals(25m, nmfsLine2.US_NetWeight);
			AssertEquals("KG", nmfsLine2.US_NetWeightUQ);

			using (invoiceLine.NMFSLines.SuspendAdditionallyForImport())
			{
				nmfsLine = invoiceLine.NMFSLines.AddNew();
				AssertEquals(ZDecimal.Zero, nmfsLine.US_NetWeight);
				AssertEquals(ZString.Empty, nmfsLine.US_NetWeightUQ);
			}
		}

		public void TestHarvestingDetailDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.CopyLastPGADetailsToNewLine = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;

			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			var detail1 = nmfsLine1.HarvestingDetails.AddNew();
			detail1.US_HarvestedCountry = Core.Constants.CountryCodes.HongKong;
			detail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			detail1.US_GearType = GearTypeList.Codes.Longline;
			detail1.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;

			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var detail2 = nmfsLine2.HarvestingDetails[0];

			AssertEquals("Expected:the default value of harvested country is correct", detail1.US_HarvestedCountry, detail2.US_HarvestedCountry);
			AssertEquals("Expected:the default value of ocean area of catch is correct", detail1.US_OceanAreaOfCatch, detail2.US_OceanAreaOfCatch);
			AssertEquals("Expected:the default value of vessel country is correct", detail1.US_GearType, detail2.US_GearType);
			AssertEquals("Expected:the default value of harvesting gear type is correct", detail1.US_VesselCountry, detail2.US_VesselCountry);

			detail1.US_VesselCountry = "";
			detail2.US_VesselCountry = Core.Constants.CountryCodes.Japan;

			detail1.US_GearType = "";
			detail2.US_GearType = GearTypeList.Codes.SportHandline;

			var nmfsLine3 = invoiceLine.NMFSLines.AddNew();
			nmfsLine3.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var detail3 = nmfsLine2.HarvestingDetails[0];

			AssertEquals("Expected:the default value of harvested country is correct", detail3.US_HarvestedCountry, detail1.US_HarvestedCountry);
			AssertEquals("Expected:the default value of ocean area of catch is correct", detail3.US_OceanAreaOfCatch, detail1.US_OceanAreaOfCatch);
			AssertEquals("Expected:the default value of vessel country is correct", detail3.US_GearType, detail2.US_GearType);
			AssertEquals("Expected:the default value of harvesting gear type is correct", detail3.US_VesselCountry, detail2.US_VesselCountry);

			using (invoiceLine.NMFSLines.SuspendAdditionallyForImport())
			{
				var nmfsLine4 = invoiceLine.NMFSLines.AddNew();
				nmfsLine4.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
				AssertEquals(0, nmfsLine4.HarvestingDetails.Count);
			}
		}

		public void TestHarvestingDetailsAreNotClonedDuringImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.CopyLastPGADetailsToNewLine = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			var detail1 = nmfsLine1.HarvestingDetails.AddNew();
			detail1.US_HarvestedCountry = Core.Constants.CountryCodes.HongKong;

			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			AssertEquals("(pre-condition) harvesting details are copied from previous line when import is not in progress", 1, nmfsLine2.HarvestingDetails.Count);
			AssertEquals("(pre-condition) harvesting details are copied from previous line when import is not in progress", Core.Constants.CountryCodes.HongKong, nmfsLine2.HarvestingDetails[0].US_HarvestedCountry);

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				var nmfsLine3 = invoiceLine.NMFSLines.AddNew();
				nmfsLine3.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
				AssertEquals("harvesting details should not be copied when import is in progress", 0, nmfsLine3.HarvestingDetails.Count);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NMFSLineCollection(InvoiceLine);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}

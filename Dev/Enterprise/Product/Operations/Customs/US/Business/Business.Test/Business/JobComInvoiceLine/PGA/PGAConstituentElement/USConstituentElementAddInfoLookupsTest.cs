using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USConstituentElementAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLists()
		{
			AssertEquals(typeof(YesNoDefaultList), Lookups.US_YesNoList.GetType());
			Assert("Should contain No", Lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", Lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !Lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));

			AssertEquals(typeof(OrganisationsFindBoxCollection), Lookups.Organizations.GetType());
			AssertEquals(typeof(RefCountryCollection), Lookups.USCountryList.GetType());
		}

		public void TestUnitsOfMeasureList()
		{
			AssertEquals(typeof(LaceyActUnitsOfMeasureList), Lookups.UnitsOfMeasureList.GetType());
			Assert(!Lookups.UnitsOfMeasureList.ContainsCode("GM"));
			Assert(!Lookups.UnitsOfMeasureList.ContainsCode("LT"));
			Assert(Lookups.UnitsOfMeasureList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Dozen));

			var invoiceLine = ConstituentElement.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			var fdaConst = fda.ProductConstituentElements.AddNew();

			var fdaLookups = fdaConst.AddInfoLookups;
			Assert(fdaLookups.UnitsOfMeasureList.ContainsCode("TOZ"));
			Assert(fdaLookups.UnitsOfMeasureList.ContainsCode("VY"));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			Assert(fdaLookups.UnitsOfMeasureList.ContainsCode("TOZ"));
			Assert(!fdaLookups.UnitsOfMeasureList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Number));

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var aceLacey = invoiceLine.LaceyActLines.AddNew();
			var aceConst = aceLacey.PG04ConstituentElements.AddNew();
			Assert(!aceConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Number));
			Assert(!aceConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Dozen));
			Assert(aceConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.CubicMeter));
			Assert(aceConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.CubicCentimeter));

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var pga1 = pivot.PGAs.AddNew();
			var productConst = pga1.PG04ConstituentElements.AddNew();
			AssertEquals(typeof(ACELaceyUnitsOfMeasureList), productConst.AddInfoLookups.UnitsOfMeasureList.GetType());
			Assert(productConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode("ML"));
			Assert(productConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode("CTL"));
			Assert(productConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode("L"));
			Assert(productConst.AddInfoLookups.UnitsOfMeasureList.ContainsCode("KL"));
		}

		USConstituentElementAddInfoLookups Lookups
		{
			get { return ConstituentElement.AddInfoLookups; }
		}

		ConstituentElement ConstituentElement
		{
			get
			{
				if (constituentElement == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pga = invoiceLine.LaceyActLines.AddNew();
					constituentElement = pga.PG04ConstituentElements.AddNew();
				}
				return constituentElement;
			}
		}
		ConstituentElement constituentElement;
	}
}

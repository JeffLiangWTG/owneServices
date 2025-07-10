using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USPGAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCertifyingIndividualList()
		{
			AssertNotNull(Lookups.CertifyingIndividualList);
			AssertEquals(2, Lookups.CertifyingIndividualList.Count);
			Assert(Lookups.CertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(Lookups.CertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
		}

		public void TestUnitOfMeasureList()
		{
			AssertNotNull(Lookups.UnitOfMeasureList);
			AssertEquals(16, Lookups.UnitOfMeasureList.Count);
			var codeList = Lookups.UnitOfMeasureList;
			Assert(codeList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.Grams));
			Assert(codeList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.Millimeters));
			Assert(codeList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.Centimeter));
			Assert(codeList.ContainsCode(ACELaceyUnitsOfMeasureList.Codes.Kilogram));
			Assert(!codeList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Pieces));
			Assert(!codeList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.Number));
			Assert(!codeList.ContainsCode(LaceyActUnitsOfMeasureList.Codes.BoardFeet));

			Assert(codeList.ContainsCode("ML"));
			Assert(codeList.ContainsCode("CTL"));
			Assert(codeList.ContainsCode("L"));
			Assert(codeList.ContainsCode("KL"));
		}

		#region Implementation

		USPGAAddInfoLookups Lookups
		{
			get { return PGA.AddInfoLookups; }
		}

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}
				return pga;
			}
		}
		PGA pga;

		#endregion
	}
}

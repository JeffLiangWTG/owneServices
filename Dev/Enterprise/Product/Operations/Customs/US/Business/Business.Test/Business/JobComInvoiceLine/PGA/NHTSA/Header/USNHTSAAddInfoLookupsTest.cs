using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAgencyProgramCodes()
		{
			AssertNotNull(Lookups.AgencyProgramCodes);
		}

		public void TestUSCountries()
		{
			AssertNotNull(Lookups.USCountries);
		}

		public void TestBoxNumbers()
		{
			AssertNotNull(Lookups.BoxNumbers);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			Assert(Lookups.BoxNumbers.ContainsCode(DepartmentOfTransportBoxNumberList.Codes._2A));

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			Assert(Lookups.BoxNumbers.ContainsCode(DepartmentOfTransportBoxNumberList.Codes._2A));

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OEI;
			Assert(!Lookups.BoxNumbers.ContainsCode(DepartmentOfTransportBoxNumberList.Codes._2A));

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OFF;
			Assert(!Lookups.BoxNumbers.ContainsCode(DepartmentOfTransportBoxNumberList.Codes._2A));

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.TPE;
			Assert(!Lookups.BoxNumbers.ContainsCode(DepartmentOfTransportBoxNumberList.Codes._2A));
		}

		public void TestOrganizations()
		{
			AssertNotNull(Lookups.Organizations);
		}

		public void TestTravelDocumentTypes()
		{
			AssertNotNull(Lookups.TravelDocumentTypes);
		}

		public void TestBondTypes()
		{
			AssertNotNull(Lookups.BondTypes);
		}

		public void TestCertifyingIndividualList()
		{
			Assert(Lookups.NHTSACertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(Lookups.NHTSACertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
			Assert(Lookups.NHTSACertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Owner));
			Assert(!Lookups.NHTSACertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Shipper));
		}

		#region Implementation

		USNHTSAAddInfoLookups Lookups
		{
			get { return Header.AddInfoLookups; }
		}

		NHTSAHeader Header
		{
			get { return header ?? (header = Factory.New<NHTSAHeader>()); }
		}
		NHTSAHeader header;

		#endregion
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USDrawbackNAFTAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUS_NAFTACountryCodeList()
		{
			AssertNotNull(Lookups.US_NAFTACountryCodeList);
		}

		#region Implementation

		USDrawbackNAFTAAddInfoLookups Lookups
		{
			get { return NAFTA.AddInfoLookups; }
		}

		DrawbackNAFTA NAFTA
		{
			get { return nafta ?? (nafta = Factory.New<DrawbackNAFTA>()); }
		}
		DrawbackNAFTA nafta;

		#endregion
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDOTAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCountries()
		{
			AssertNotNull(Lookups.USCountries);
		}

		public void TestBoxNumbers()
		{
			AssertNotNull(Lookups.BoxNumbers);
		}

		public void TestClarificationCodes()
		{
			AssertNotNull(Lookups.ClarificationCodes);
		}

		#region Implementation

		USDOTAddInfoLookups Lookups
		{
			get { return DOT.AddInfoLookups; }
		}

		DOT DOT
		{
			get { return dot ?? (dot = Factory.New<DOT>()); }
		}
		DOT dot;

		#endregion
	}
}

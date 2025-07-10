using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USFCCAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFCCImportConditionNumbers()
		{
			AssertNotNull(Lookups.FCCImportConditionNumbers);
		}

		#region Implementation

		USFCCAddInfoLookups Lookups
		{
			get { return FCC.AddInfoLookups; }
		}

		FCC FCC
		{
			get { return fcc ?? (fcc = Factory.New<FCC>()); }
		}
		FCC fcc;

		#endregion
	}
}

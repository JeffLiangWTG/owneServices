using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSAPermitAndLicenseAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLPCOTypes()
		{
			AssertNotNull(Lookups.LPCOTypes);
		}

		public void TestLPCODateTypes()
		{
			AssertNotNull(Lookups.LPCODateTypes);
		}

		#region Implementation

		USNHTSAPermitAndLicenseAddInfoLookups Lookups
		{
			get { return PermitAndLicenses.AddInfoLookups; }
		}

		NHTSAPermitAndLicenses PermitAndLicenses
		{
			get { return fPermitAndLicenses ?? (fPermitAndLicenses = Factory.New<NHTSAPermitAndLicenses>()); }
		}
		NHTSAPermitAndLicenses fPermitAndLicenses;

		#endregion
	}
}

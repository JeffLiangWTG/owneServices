using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobVoyAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHeaders()
		{
			AssertType(typeof(ShipsAgencyPrincipalCollectionWithSecurityCheck), Account.Lookups.Headers);
		}

		#region Implementation
		VoyageAccount Account
		{
			get
			{
				return account ?? (account = Factory.New<VoyageAccount>());
			}
		}

		VoyageAccount account;
		#endregion
	}
}

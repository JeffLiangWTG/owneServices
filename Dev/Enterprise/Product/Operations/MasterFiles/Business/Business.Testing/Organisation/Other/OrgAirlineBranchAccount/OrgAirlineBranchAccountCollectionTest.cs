using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAirlineBranchAccountCollection))]
	sealed class OrgAirlineBranchAccountCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgAirlineBranchAccountCollection>
	{
		#region Implementation

		protected override OrgAirlineBranchAccountCollection GetCollectionToTest()
		{
			return Factory.New<OrgHeader>().OrgAirlineBranchAccounts;
		}

		#endregion
	}
}

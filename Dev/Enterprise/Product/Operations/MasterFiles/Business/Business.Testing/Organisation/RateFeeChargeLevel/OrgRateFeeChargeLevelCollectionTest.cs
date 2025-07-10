using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateFeeChargeLevelCollection))]
	sealed class OrgRateFeeChargeLevelCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgRateFeeChargeLevelCollection>
	{
		protected override OrgRateFeeChargeLevelCollection GetCollectionToTest()
		{
			return new OrgRateFeeChargeLevelCollection(Factory);
		}
	}
}

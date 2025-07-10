using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgLandedCostingPrefChargesCollection))]
	sealed class OrgLandedCostingPrefChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs preferences = org.LandedCostingPreferences.AddNew();
			return preferences.Charges;
		}

		#endregion
	}
}

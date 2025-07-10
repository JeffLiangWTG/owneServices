using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSChargeCodeGroupPivotCollection))]
	sealed class AccPOSChargeCodeGroupPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			return new AccPOSChargeCodeGroupPivotCollection(group);
		}

		#endregion
	}
}

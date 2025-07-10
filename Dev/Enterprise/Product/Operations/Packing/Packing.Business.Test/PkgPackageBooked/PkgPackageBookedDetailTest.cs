using CargoWise.EntityFramework;
using Enterprise.Integration.Packing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageBookedDetail))]
	class PkgPackageBookedDetailTest : PackingBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var defaultUnits = (IPackageDefaultUQs)new PkgDefaultUnits();
			var bookedDimensions = (PkgPackageBookedDetail)GetNewBusinessObject();

			CombineAssertions("PkgPackageBookedDetail Default Values.", () =>
			{
				AssertEquals("KPB_PackageQty", 1, bookedDimensions.KPB_PackageQty);
				AssertEquals("KPB_WeightUQ", defaultUnits.DefaultWeightUnit, bookedDimensions.KPB_WeightUQ);
				AssertEquals("KPB_VolumeUQ", defaultUnits.DefaultVolumeUnit, bookedDimensions.KPB_VolumeUQ);
				AssertEquals("KPB_DimensionUQ", defaultUnits.DefaultDimensionUnit, bookedDimensions.KPB_DimensionUQ);
			});
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			return packageJob.Packages.AddNew().BookedDimensions;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}

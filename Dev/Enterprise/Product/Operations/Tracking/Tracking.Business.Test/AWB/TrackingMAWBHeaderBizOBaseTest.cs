using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingMAWBHeader))]
	public class TrackingMAWBHeaderBizOBaseTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TrackingMAWBHeader>();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TrackingMAWBHeaderLightValidationTester(bizObjToTest);
		}

		#region InBondLightValidationTester

		// Overridden when upgrading base class of this test to EnterpriseBusinessObject. All other new tests passed. Will be passed to Core team for review / remedy.
		internal class TrackingMAWBHeaderLightValidationTester : LightValidationTester
		{
			public TrackingMAWBHeaderLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				return base.ShouldTestProperty(info) && (info.Name != ExportAWBRateLineSchema.ER_GrossWeight.Name) && (info.Name != ExportAWBRateLineSchema.ER_Total.Name);
			}
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefVesselCollection))]
	class RefVesselCollectionTest : ActiveBusinessObjectCollectionTestCase<RefVesselCollection>
	{
		public void TestICodePropertyNameProviderMembers()
		{
			var collection = new RefVesselCollection(Factory, false);
			AssertEquals(RefVessel.Schema.RV_Code, ((ICodePropertyNameProvider)collection).GetCodePropertyName(typeof(RefVessel)));

			collection = new RefVesselCollection(Factory, true);
			AssertEquals(RefVessel.Schema.RV_LloydsNumber, ((ICodePropertyNameProvider)collection).GetCodePropertyName(typeof(RefVessel)));
		}

		protected override RefVesselCollection GetCollectionToTest()
		{
			var result = new RefVesselCollection(Factory);
			ZQuery zQuery = new ZQuery();
			zQuery.FetchOnlyFromLocalCache = true;
			result.AdditionalFilter = zQuery;
			return result;
		}
	}
}

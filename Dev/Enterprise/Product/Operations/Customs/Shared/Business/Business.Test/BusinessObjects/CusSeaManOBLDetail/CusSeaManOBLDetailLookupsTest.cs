using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class CusSeaManOBLDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestPackageTypes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.PackageTypes.GetType());
		}

		public virtual void TestGrossWeightCodes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.GrossWeightCodes.GetType());
		}

		public virtual void TestQuantityUnits()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.QuantityUnits.GetType());
		}

		public virtual void TestCargoTypes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.CargoTypes.GetType());
		}

		public virtual void TestContainerSizes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.ContainerSizes.GetType());
		}

		public virtual void TestTypesOfContainers()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(typeof(CodeDescriptionPairList), detail.Lookups.TypesOfContainers.GetType());
		}
	}
}

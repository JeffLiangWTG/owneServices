using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeal))]
	class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFillWithValidTestData()
		{
			var seal = Factory.NewWithValidTestData<CusSeal>();
			AssertEquals("NEW", seal.BK_UnloadingState);

			seal = Factory.New<CusSeal>();
			AssertEquals(ZString.Empty, seal.BK_UnloadingState);

			seal.FillWithValidTestData();
			AssertEquals("NEW", seal.BK_UnloadingState);
		}

		public void TestBK_UnloadingState()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeal), nameof(CusSeal.BK_UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadingStatesList");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		CusSeal GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var cusSeal = factory.New<CusSeal>();
			cusSeal.BK_SequenceNumber = 1;
			cusSeal.BK_SealNumber = "ABC";
			cusSeal.BK_ParentTableCode = CusInBondContainerSchema.Constants.Prefix;
			return cusSeal;
		}
	}
}

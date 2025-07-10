using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestMAFContainerTypeList()
		{
			CusContainerLookups lookups = (CusContainerLookups)GetCusContainerLookups();
			var containerTypeList = lookups.MAFContainerTypeList;
			AssertEquals("1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 20G0, 20G1, 20H0, 20P1, 20T0, 20T1, 20T2, 20T3, 20T4, 20T5, 20T6, 20T7, 20T8, 21, 22, 22B0, 22G0, 22G1, 22H0, 22K2, 22P0, 22P1, 22P3, 22P4, 22P7, 22P8, 22P9, 22PP, 22R0, 22R1, 22R7, 22R9, 22S1, 22T0, 22T1, 22T2, 22T3, 22T4, 22T5, 22T6, 22T7, 22T8, 22U1, 22U6, 22V0, 22V2, 22V3, 23, 24, 25, 25G0, 25G1, 25R0, 25R1, 25T3, 25T4, 26, 26G0, 26H0, 27, 28, 28T8, 28U1, 28V0, 29, 29P0, 2EG0, 3, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 4, 40, 41, 42, 42G0, 42G1, 42H0, 42P0, 42P1, 42P3, 42P4, 42P6, 42P8, 42P9, 42R1, 42R3, 42R9, 42S1, 42T2, 42T5, 42T6, 42T8, 42U1, 42U6, 43, 44, 45, 45B3, 45G0, 45G1, 45P0, 45P3, 45P8, 45R0, 45R1, 45R7, 45R8, 45R9, 45U1, 45U6, 46H0, 48T8, 48T9, 49P0, 4CG0, 4EG1, 5, 6, 7, 8, 9, L0G1, L2G1, L5G1, ULD", containerTypeList.CodesAsString);
			AssertSame(containerTypeList, lookups.MAFContainerTypeList);
		}

		public override void TestCO_FCL_LCL_NCT_List()
		{
			CusContainerLookups lookups = (CusContainerLookups)GetCusContainerLookups();
			AssertNotNull("Lookups.CO_FCL_LCL_NCT_List", lookups.CO_FCL_LCL_NCT_List);
			AssertEquals(typeof(ContainerModeList), lookups.CO_FCL_LCL_NCT_List.GetType());
		}

		public override void TestContainerSizeList()
		{
			CusContainerLookups lookups = (CusContainerLookups)GetCusContainerLookups();
			AssertNotNull("Lookups.CO_ContainerSizeList", lookups.CO_ContainerSizeList);
			AssertEquals(typeof(ContainerSizeList), lookups.CO_ContainerSizeList.GetType());
		}

		protected override Customs.Business.CusContainerLookups GetCusContainerLookups()
		{
			return new CusContainerLookups(Factory.New<CusContainer>());
		}
	}
}

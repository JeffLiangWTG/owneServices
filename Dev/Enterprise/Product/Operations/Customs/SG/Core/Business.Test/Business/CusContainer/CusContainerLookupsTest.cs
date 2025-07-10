namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestContainer()
		{
			CusContainer parent = Factory.New<CusContainer>();
			AssertEquals(parent.Lookups.Container, parent);
		}

		public override void TestCO_FCL_LCL_NCT_List()
		{
			AssertEquals(true, Lookups.CO_FCL_LCL_NCT_List is ContainerTypeCodeList);
		}

		public override void TestContainerSizeList()
		{
			AssertEquals(true, Lookups.ContainerSizeList is SGContainerSizeList);
		}

		#region Implementation
		protected override Customs.Business.CusContainerLookups GetCusContainerLookups()
		{
			return Lookups;
		}

		CusContainerLookups Lookups
		{
			get
			{
				return lookups ?? (lookups = new CusContainerLookups(Factory.New<CusContainer>()));
			}
		}

		CusContainerLookups lookups;
		#endregion
	}
}

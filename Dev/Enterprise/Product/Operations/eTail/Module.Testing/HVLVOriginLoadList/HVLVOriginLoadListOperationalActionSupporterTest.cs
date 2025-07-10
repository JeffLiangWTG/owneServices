namespace Enterprise.eTail.Module.Testing
{
	using CargoWise.Definitions;
	using Enterprise.eTail.Business;
	using Enterprise.Services.OperationalActions.Support.Testing;
	using Enterprise.ZArchitecture.Modules;
	using NUnit.Framework;

	[TestedType(typeof(HVLVOriginLoadListOperationalActionSupporter))]
	class HVLVOriginLoadListOperationalActionSupporterTest : OperationalActionSupporterTest<HVLVOriginLoadListOperationalActionSupporter>
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.HVLVOriginLoadList, Supporter.BusinessContext);
		}

		public void TestRootType()
		{
			AssertEquals(typeof(HVLVOriginLoadList), Supporter.RootType);
		}

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HVLVOriginLoadList; }
		}

		#endregion
	}
}

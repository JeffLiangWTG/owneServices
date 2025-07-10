namespace Enterprise.eTail.Module.Testing
{
	using CargoWise.Definitions;
	using Enterprise.eTail.Business;
	using Enterprise.Services.OperationalActions.Support;
	using Enterprise.Services.OperationalActions.Support.Testing;
	using Enterprise.ZArchitecture.Modules;
	using NUnit.Framework;

	[TestedType(typeof(HVLVConsignmentOperationalActionSupporter))]
	class HVLVConsignmentOperationalActionSupporterTest : OperationalActionSupporterTest<HVLVConsignmentOperationalActionSupporter>
	{
		#region TestActionMethodGroups

		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.HVLV },
				Supporter.Methods.GetAllIds());
		}

		#endregion

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.HVLVConsignment, Supporter.BusinessContext);
		}

		public void TestRootType()
		{
			AssertEquals(typeof(HVLVConsignment), Supporter.RootType);
		}

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HVLVConsignment; }
		}

		#endregion
	}
}

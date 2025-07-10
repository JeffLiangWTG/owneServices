using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerManagerActionSupporter))]
	internal class ContainerManagerActionSupporterTest : OperationalActionSupporterTest<ContainerManagerActionSupporter>
	{
		public void TestMethods()
		{
			AssertContainsExactElementsInAnyOrder("Action Methods", i => i.Name, new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Shipping }, Supporter.Methods.GetAllIds());
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyContainerManager;
			}
		}

		public override bool ShouldSupportDocuments
		{
			get
			{
				return false;
			}
		}
		#endregion
	}
}

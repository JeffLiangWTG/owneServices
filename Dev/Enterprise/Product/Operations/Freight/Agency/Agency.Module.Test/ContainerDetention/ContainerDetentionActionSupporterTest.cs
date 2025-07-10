using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerDetentionActionSupporter))]
	internal class ContainerDetentionActionSupporterTest : OperationalActionSupporterTest<ContainerDetentionActionSupporter>
	{
		public void TestActionMethodGroups()
		{
			var expectedIDs = new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting, };
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, expectedIDs, Supporter.Methods.GetAllIds());
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyContainerDetention;
			}
		}
		#endregion
	}
}

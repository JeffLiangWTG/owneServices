using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillContainersActionSupporter))]
	internal class BillContainersActionSupporterTest : OperationalActionSupporterTest<BillContainersActionSupporter>
	{
		public void TestActionMethodGroups()
		{
			var expectedIDs = new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Shipping, };
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, expectedIDs, Supporter.Methods.GetAllIds());
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyBillContainers;
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

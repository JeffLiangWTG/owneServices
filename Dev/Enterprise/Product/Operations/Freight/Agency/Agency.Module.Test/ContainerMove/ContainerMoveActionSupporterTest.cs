using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.ContainerMove.Testing
{
	[TestedType(typeof(ContainerMoveActionSupporter))]
	internal class ContainerMoveActionSupporterTest : OperationalActionSupporterTest<ContainerMoveActionSupporter>
	{
		public void TestNames()
		{
			AssertEquals("movement", Supporter.SingularElementNoun);
			AssertEquals("movements", Supporter.PluralElementNoun);
		}

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
				return ModuleIDs.AgencyContainerMove;
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

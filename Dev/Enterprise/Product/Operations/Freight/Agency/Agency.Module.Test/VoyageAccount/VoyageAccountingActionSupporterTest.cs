using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(VoyageAccountingActionSupporter))]
	internal class VoyageAccountingActionSupporterTest : OperationalActionSupporterTest<VoyageAccountingActionSupporter>
	{
		public void TestActionMethodGroups()
		{
			var expectedIDs = new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting, };
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, expectedIDs, Supporter.Methods.GetAllIds());
		}

		public void TestNouns()
		{
			AssertEquals("voyage account", Supporter.SingularElementNoun);
			AssertEquals("voyage accounts", Supporter.PluralElementNoun);
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyVoyageAccounting;
			}
		}
		#endregion
	}
}

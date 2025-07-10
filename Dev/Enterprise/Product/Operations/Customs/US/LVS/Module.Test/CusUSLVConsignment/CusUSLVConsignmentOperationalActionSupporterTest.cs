using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVConsignmentOperationalActionSupporter))]
	public class CusUSLVConsignmentOperationalActionSupporterTest : OperationalActionSupporterTest<CusUSLVConsignmentOperationalActionSupporter>
	{
		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.USLowValueBill },
				Supporter.Methods.GetAllIds());
		}

		public void TestOverrideProperties()
		{
			AssertEquals(Env.Security.USLVConsignment, Supporter.BaseCheckpoint);
			AssertEquals("Singular Noun", "Low Value Entries by Bill", Supporter.SingularElementNoun);
			AssertEquals("Plural Noun", "Low Value Entries by Bills", Supporter.PluralElementNoun);
		}

		#region Implementation

		public override bool ShouldSupportDocuments => false;

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USLowValueEntriesBill;

		#endregion
	}
}

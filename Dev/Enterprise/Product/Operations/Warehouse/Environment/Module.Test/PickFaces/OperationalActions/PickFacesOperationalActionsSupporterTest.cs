using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(PickFacesOperationalActionsSupporter))]
	public class PickFacesOperationalActionsSupporterTest : OperationalActionSupporterTest<PickFacesOperationalActionsSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigPickFaces;

		public override bool ShouldSupportDocuments => false;

		#endregion
	}
}

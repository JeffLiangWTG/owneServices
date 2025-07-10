using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetOperationalActionSupporter))]
	internal class DtbConsignmentRunSheetOperationalActionSupporterTest : OperationalActionSupporterTest<DtbConsignmentRunSheetOperationalActionSupporter>
	{
		#region TestSingularElementNoun

		public void TestSingularElementNoun()
		{
			AssertEquals("Consignment Run Sheet", Supporter.SingularElementNoun);
		}

		#endregion

		#region TestPluralElementNoun

		public void TestPluralElementNoun()
		{
			AssertEquals("Consignment Run Sheets", Supporter.PluralElementNoun);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbConsignmentRunSheet; }
		}

		#endregion
	}
}

using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class OrgSupplierPartActionSupporterTest<SupporterT> : OperationalActionSupporterTest<SupporterT>
			where SupporterT : OrgSupplierPartActionSupporter
	{
		public virtual void TestSingularElementNoun()
		{
			AssertEquals("product", Supporter.SingularElementNoun);
		}

		public virtual void TestPluralElementNoun()
		{
			AssertEquals("products", Supporter.PluralElementNoun);
		}

		#region TestActionMethodProviderIDsHasMasterFiles

		public void TestActionMethodProviderIDsHasMasterFiles()
		{
			var methods = Supporter.Methods.GetAllIds();
			AssertEquals("Must have MasterFiles ActionMethodProviderID", true, methods.Any(x => x == ActionMethodProviderIDs.MasterFiles));
			AssertEquals("Must have FRProduct ActionMethodProviderID", true, methods.Any(x => x == ActionMethodProviderIDs.FRProduct));
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SupplierPart; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}

		#endregion
	}
}

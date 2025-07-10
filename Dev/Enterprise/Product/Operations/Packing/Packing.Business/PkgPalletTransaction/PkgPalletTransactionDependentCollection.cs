using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPalletTransactionDependentCollection : ActiveBusinessObjectCollection<PkgPalletTransaction>
	{
		public PkgPalletTransactionDependentCollection(IPalletTransactionParent parent)
			: base(parent.Factory, new DependentRelationship((BusinessObject)parent, typeof(PkgPalletTransaction), new ZQuery(PkgPalletTransactionSchema.KTR_ParentID, parent.PK), PkgPalletTransactionSchema.KTR_ParentID))
		{
		}

		IPalletTransactionParent Master
		{
			get { return (IPalletTransactionParent)Relationship.Master; }
		}

		protected override void SetDefaultsForNewElementCore(PkgPalletTransaction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.RelatedJob = Master;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJobClusterCollection : DependentBusinessObjectCollection<AccDraftInvoiceJobCluster, AccDraftInvoiceHeader>
	{
		public AccDraftInvoiceJobClusterCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccDraftInvoiceJobClusterCollection(AccDraftInvoiceHeader parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		protected override string FkColumnName => AccDraftInvoiceJobClusterSchema.AIC_AIH_Header.Name;
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}

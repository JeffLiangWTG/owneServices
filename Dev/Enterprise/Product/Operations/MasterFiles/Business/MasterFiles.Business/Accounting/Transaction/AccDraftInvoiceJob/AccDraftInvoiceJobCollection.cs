using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJobCollection : DependentBusinessObjectCollection<AccDraftInvoiceJob, AccDraftInvoiceJobCluster>
	{
		public AccDraftInvoiceJobCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccDraftInvoiceJobCollection(AccDraftInvoiceJobCluster parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		protected override string FkColumnName => AccDraftInvoiceJobSchema.AIJ_AIC_Cluster.Name;
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}

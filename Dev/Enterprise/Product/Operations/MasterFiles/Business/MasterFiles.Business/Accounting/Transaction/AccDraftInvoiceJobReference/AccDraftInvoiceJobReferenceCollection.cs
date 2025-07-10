using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJobReferenceCollection : DependentBusinessObjectCollection<AccDraftInvoiceJobReference, AccDraftInvoiceHeader>
	{
		public AccDraftInvoiceJobReferenceCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccDraftInvoiceJobReferenceCollection(AccDraftInvoiceHeader parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		protected override string FkColumnName => AccDraftInvoiceJobReferenceSchema.AIR_AIH_Header.Name;
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}


using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassRequiredDocumentDependentCollection : CFSRequiredDocumentDependentCollection
	{
		public GatePassRequiredDocumentDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
			this.SetReadOnlyIncludingChildren(true);
		}

		public new GatePassRequiredDocument this[int index]
		{
			get { return (GatePassRequiredDocument)(Elements[index]); }
		}

		public new GatePassRequiredDocument AddNew()
		{
			return (GatePassRequiredDocument)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

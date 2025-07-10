
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassServiceDependentCollection : CFSServiceDependentCollection
	{
		public GatePassServiceDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public new GatePassService this[int index]
		{
			get { return (GatePassService)(Elements[index]); }
		}

		public new GatePassService AddNew()
		{
			return (GatePassService)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

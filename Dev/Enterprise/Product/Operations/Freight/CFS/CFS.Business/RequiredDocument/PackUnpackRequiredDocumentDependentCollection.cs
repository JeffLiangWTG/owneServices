
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackRequiredDocumentDependentCollection : CFSRequiredDocumentDependentCollection
	{
		public PackUnpackRequiredDocumentDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new PackUnpackRequiredDocument this[int index]
		{
			get { return (PackUnpackRequiredDocument)(Elements[index]); }
		}

		public new PackUnpackRequiredDocument AddNew()
		{
			return (PackUnpackRequiredDocument)base.AddNew();
		}
	}
}

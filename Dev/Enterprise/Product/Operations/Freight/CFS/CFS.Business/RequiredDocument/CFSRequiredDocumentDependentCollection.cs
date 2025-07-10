
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSRequiredDocumentDependentCollection : JobRequiredDocumentDependentCollection
	{
		public CFSRequiredDocumentDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new CFSRequiredDocument this[int index]
		{
			get { return (CFSRequiredDocument)(Elements[index]); }
		}

		public new CFSRequiredDocument AddNew()
		{
			return (CFSRequiredDocument)base.AddNew();
		}
	}
}

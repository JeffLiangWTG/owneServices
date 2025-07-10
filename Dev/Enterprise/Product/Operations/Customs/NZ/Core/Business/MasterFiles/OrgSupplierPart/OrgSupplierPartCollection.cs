using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}
	}
}

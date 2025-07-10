using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class OrgSupplierBuyerLinkPartCollection : OrgSupplierPartCollection
	{
		public OrgSupplierBuyerLinkPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierBuyerLinkPartCollection(BusinessObjectFactory factory, OrgSupplierBuyerLink link)
			: base(factory, link.Supplier, link.Buyer, NewGetSupplierOwnerFilterDontResolveDuplicates(link.Supplier, link.Buyer))
		{
		}

		public override void DefaultModuleFilterFields(OrgHeader supplier, OrgHeader owner)
		{
			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.Value)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "FilterCondition", (ZString)(NoResString)"exact"));

				if (owner != null)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", owner.PK));
				}

				if (supplier != null)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", supplier.PK));
				}
			}
		}
	}
}

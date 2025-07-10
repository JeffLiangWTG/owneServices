using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class OrdersOrgSupplierPartCollection : OrgSupplierPartCollection
	{
		public OrdersOrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrdersOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner)
			: base(factory, supplier, owner, NewGetSupplierOwnerFilterDontResolveDuplicates(supplier, owner))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter")]
		public override void DefaultModuleFilterFields(OrgHeader supplier, OrgHeader owner)
		{
			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.Value)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "FilterCondition", (ZString)"exact"));

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

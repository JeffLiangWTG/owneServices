using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ImportCollectionInfoImplForWhsDocketFlattened : ImportCollectionInfoImpl
	{
		public ImportCollectionInfoImplForWhsDocketFlattened(WhsDocketFlattenedCollection collection)
			: base(collection)
		{
			var type = typeof(WhsDocketFlattened);
			foreach (var property in type.GetProperties().OrderBy(x => x.Name).Where(x => !x.Name.EndsWith((NoResString)"Info", StringComparison.OrdinalIgnoreCase))) // ZPropertyInfo Name
			{
				if (property.Name.StartsWith(WhsDocketSchema.Constants.Prefix + "_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(WhsDocket));
				}
				else if (property.Name.StartsWith("Warehouse_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(WhsWarehouse), "Warehouse_", Res.GetString("fecfd7c8-5b75-45ec-9f62-5170935bb626", "Warehouse"));
				}
				else if (property.Name.StartsWith("Client_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(OrgHeader), "Client_", Res.GetString("734a35da-6517-4b6b-b1f9-6e44593a729e", "Client"));
				}
				else if (property.Name.StartsWith("Forwarder_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(OrgHeader), "Forwarder_", Res.GetString("c035f9b6-e309-4f2f-b2ed-71d00de8b308", "Forwarder"));
				}
				else if (property.Name.StartsWith("PickupAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(JobDocAddress), "PickupAddress_", Res.GetString("dae0db16-ec01-42e0-9be8-8a039032270e", "Pickup"));
				}
				else if (property.Name.StartsWith("DropOffAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(JobDocAddress), "DropOffAddress_", Res.GetString("8c106847-bf2e-401d-896e-18feabd15c1c", "Drop Off"));
				}
				else if (property.Name.StartsWith("ConsigneeAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(JobDocAddress), "ConsigneeAddress_", Res.GetString("607640ab-6036-4142-8367-3d8be702103b", "Consignee"));
				}
				else if (property.Name.StartsWith("SupplierAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(JobDocAddress), "SupplierAddress_", Res.GetString("c045da75-1ed2-4a48-a274-e53fcce1d35d", "Supplier"));
				}
				else if (property.Name.StartsWith("Line_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(WhsDocketLine), "Line_", Res.GetString("f8b4f1f9-a2c0-482f-afee-c4f08cb29421", "Line"));
				}
				else if (property.Name.StartsWith("LinePart_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<WhsDocketFlattened>(property.Name, typeof(OrgSupplierPart), "LinePart_", Res.GetString("f8b4f1f9-a2c0-482f-afee-c4f08cb29421", "Line"));
				}
			}
		}
	}
}

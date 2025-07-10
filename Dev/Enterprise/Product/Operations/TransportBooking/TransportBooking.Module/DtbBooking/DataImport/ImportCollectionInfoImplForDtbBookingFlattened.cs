using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	public class ImportCollectionInfoImplForDtbBookingFlattened : ImportCollectionInfoImpl
	{
		public ImportCollectionInfoImplForDtbBookingFlattened(DtbBookingFlattenedCollection collection)
			: base(collection)
		{
			var type = typeof(DtbBookingFlattened);
			foreach (var property in type.GetProperties().OrderBy(x => x.Name).Where(x => !x.Name.EndsWith((NoResString)"Info", StringComparison.OrdinalIgnoreCase)))
			{
				if (property.Name.StartsWith(DtbBookingSchema.Constants.Prefix + "_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBooking), DtbBookingSchema.Constants.TableName);
				}
				else if (property.Name.StartsWith("Pack_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(PkgPackage), "Pack_", Res.GetString("7e10b366-a67d-49a0-95e2-c47f388e07da", "Package"));
				}
				else if (property.Name.StartsWith("Pickup_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBookingInstruction), "Pickup_", Res.GetString("e0271e05-da60-4298-808c-066a4af4f499", "Pickup"));
				}
				else if (property.Name.StartsWith("PickupAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(JobDocAddress), "PickupAddress_", Res.GetString("e0271e05-da60-4298-808c-066a4af4f499", "Pickup"));
				}
				else if (property.Name.StartsWith("PickupConfirmation_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBookingConfirmation), "PickupConfirmation_", Res.GetString("e0271e05-da60-4298-808c-066a4af4f499", "Pickup"));
				}
				else if (property.Name.StartsWith("Delivery_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBookingInstruction), "Delivery_", Res.GetString("4bcc04c6-f627-4136-b2a6-15baf80f9843", "Delivery"));
				}
				else if (property.Name.StartsWith("DeliveryAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(JobDocAddress), "DeliveryAddress_", Res.GetString("4bcc04c6-f627-4136-b2a6-15baf80f9843", "Delivery"));
				}
				else if (property.Name.StartsWith("DeliveryConfirmation_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBookingConfirmation), "DeliveryConfirmation_", Res.GetString("4bcc04c6-f627-4136-b2a6-15baf80f9843", "Delivery"));
				}
				else if (property.Name.StartsWith("LocalClientAddress_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(JobDocAddress), "LocalClientAddress_", Res.GetString("3abc04c6-f627-4136-b2a6-15baf80f9843", "Local Client"));
				}
				else if (property.Name.StartsWith("Consolidation_", StringComparison.OrdinalIgnoreCase))
				{
					this.AddFlattenedProperty<DtbBookingFlattened>(property.Name, typeof(DtbBookingConsolidation), "Consolidation_", Res.GetString("5824f415-7b7b-49f9-ab73-88ba74cacaf1", "Consolidation"));
				}
			}
		}
	}
}

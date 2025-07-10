using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentCollection : ActiveBusinessObjectCollection<Shipment>
	{
		readonly Trip trip;
		public ShipmentCollection(Trip master)
			: base(master)
		{
			trip = master;
		}

		protected override void SetDefaultsForNewElementCore(Shipment newElement)
		{
			using (newElement.SuspendSettingHasChanges())
			{
				base.SetDefaultsForNewElementCore(newElement);

				if (trip != null)
				{
					newElement.B0_IssuerSCAC = trip.BH_CarrierSCAC;
				}
				newElement.UpdateShipmentTypeDependentsAsNeeded();
			}
		}

		protected override bool RunPreSaveValidationCore()
		{
			var result = true;
			using (trip.CacheDuplicateMasterBillNumbers())
			{
				if (trip.ShouldValidateChildren)
				{
					result = base.RunPreSaveValidationCore();
				}
				else
				{
					foreach (var shipment in this)
					{
						if (shipment.HasChanges)
						{
							shipment.RunPreSaveValidation();
						}
					}
				}
			}

			return result;
		}
	}
}

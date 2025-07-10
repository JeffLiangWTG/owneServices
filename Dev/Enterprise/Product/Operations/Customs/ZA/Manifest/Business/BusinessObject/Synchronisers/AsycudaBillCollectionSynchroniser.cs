using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaBillCollectionSynchroniser : ASYCUDA.Business.AsycudaBillCollectionSynchroniser
	{
		public AsycudaBillCollectionSynchroniser(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override ManifestBillSynchroniser<ASYCUDA.Business.AsycudaBill> GetNewManifestBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment source)
		{
			return new AsycudaBillSynchroniser((AsycudaBill)destination, source);
		}

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !Destination.AMA_OverrideFreightDefaults)
			{
				DeleteOrAddZABills();
			}
		}

		void DeleteOrAddZABills()
		{
			var shipments = new List<ForwardingShipment>(Source.Shipments.Cast<ForwardingShipment>());
			var bills = new List<AsycudaBill>(Destination.Bills.Cast<AsycudaBill>());

			while (bills.Count > 0)
			{
				var bill = bills[0];
				bills.Remove(bill);
				var synchroniser = ElementSynchronisers.FindMatchingDestination<AsycudaBillSynchroniser>(bill);
				if (bill.IsDeleted)
				{
					if (synchroniser != null)
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						ElementSynchronisers.Remove(synchroniser);
					}
				}
				else
				{
					if (synchroniser != null)
					{
						if (shipments.Contains(synchroniser.Source))
						{
							shipments.Remove(synchroniser.Source);
							synchroniser.Synchronise();
							continue;
						}
					}
					else
					{
						bill = FindMatchingShipmentAndAddSynchroniser(shipments, bills, bill);
					}

					if (bill != null && !bill.HasManifestBeenSubmittedToCustoms)
					{
						bill.Delete();
					}
				}
			}

			foreach (var shipment in shipments)
			{
				var bill = Destination.Bills.AddNew();
				var synchroniser = new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);
				ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
				synchroniser.Synchronise();
			}
		}

		AsycudaBill FindMatchingShipmentAndAddSynchroniser(List<ForwardingShipment> shipments, List<AsycudaBill> bills, AsycudaBill bill)
		{
			ForwardingShipment existingShipment = null;
			var alreadyProcessedShipments = new List<ForwardingShipment>();
			while ((existingShipment = shipments.FirstOrDefault(shipment => !alreadyProcessedShipments.Contains(shipment) && GetBillNumber(shipment) == bill.ABL_BillNumber && AreAdditionalKeysMatching(shipment, bill))) != null)
			{
				alreadyProcessedShipments.Add(existingShipment);
				var synchroniser = ElementSynchronisers.FindMatchingSource<AsycudaBillSynchroniser>(existingShipment, true);
				if (synchroniser != null)
				{
					shipments.Remove(existingShipment);
					bills.Remove((AsycudaBill)synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new AsycudaBillSynchroniser(bill, existingShipment);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					shipments.Remove(existingShipment);
					bill = null;
					break;
				}
			}
			return bill;
		}
	}
}

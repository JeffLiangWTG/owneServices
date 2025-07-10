using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public abstract class ShipmentCartageType : CartageType
	{
		public ShipmentCartageType(CommonShipment shipment) : base((ICartageParent)shipment) { }

		protected CommonShipment ShipmentParent
		{
			get { return (CommonShipment)CartageParent; }
		}

		protected JobSailing ShipmentSailing => ShipmentParent.Sailing ?? ShipmentParent.MostInterestingTransport?.Sailing;

		public override ZString PortOfLoading =>
			ShipmentSailing?.JX_JA_RL_NKPortOfLoading ??
			ShipmentParent.MostInterestingTransport?.JW_RL_NKLoadPort ?? ZString.Empty;

		public override ZString PortOfDischarge =>
			ShipmentSailing?.JX_JB_RL_NKPortOfDischarge ??
			ShipmentParent.MostInterestingTransport?.JW_RL_NKDiscPort ?? ZString.Empty;

		public override ZString Vessel =>
			ShipmentSailing?.JX_JV_NKVessel ??
			ShipmentParent.MostInterestingTransport?.JW_Vessel ?? ZString.Empty;

		public override ZString VoyageFlight =>
			ShipmentSailing?.JX_JV_VoyageFlight ??
			ShipmentParent.MostInterestingTransport?.JW_VoyageFlight ?? ZString.Empty;

		public override ZDateTime E_ARV =>
			ShipmentSailing?.JX_JB_E_ARV ??
			ShipmentParent.MostInterestingTransport?.JW_ETA ?? ZDateTime.Empty;

		public override ZDateTime E_DEP =>
			ShipmentSailing?.JX_JA_E_DEP ??
			ShipmentParent.MostInterestingTransport?.JW_ETD ?? ZDateTime.Empty;

		public override ZDateTime A_ARV =>
			ShipmentSailing?.JX_JB_A_ARV ??
			ShipmentParent.MostInterestingTransport?.JW_ATA ?? ZDateTime.Empty;

		public override ZDateTime A_DEP =>
			ShipmentSailing?.JX_JA_A_DEP ??
			ShipmentParent.MostInterestingTransport?.JW_ATD ?? ZDateTime.Empty;

		public override ZDateTime FCLAvailabilityDate =>
			!ShipmentParent.DocsAndCartage.JP_FCLAvailable.IsEmpty ?
			ShipmentParent.DocsAndCartage.JP_FCLAvailable :
			ShipmentSailing?.JX_JB_CTOAvailabilityDate ??
			ShipmentParent.MostInterestingTransport?.JW_TerminalAvailabilityDate ?? ZDateTime.Empty;

		public override ZDateTime FCLStorageDate =>
			!ShipmentParent.DocsAndCartage.JP_FCLStorageCommences.IsEmpty ?
			ShipmentParent.DocsAndCartage.JP_FCLStorageCommences :
			ShipmentSailing?.JX_JB_CTOStorageDate ??
			ShipmentParent.MostInterestingTransport?.JW_TerminalStorageDate ?? ZDateTime.Empty;

		public override ZDateTime LCLAvailabilityDate =>
			!ShipmentParent.DocsAndCartage.JP_LCLAvailable.IsEmpty ?
			ShipmentParent.DocsAndCartage.JP_LCLAvailable :
			ShipmentSailing?.JX_DepotAvailabilityDate ??
			ShipmentParent.MostInterestingTransport?.JW_DepotAvailabilityDate ?? ZDateTime.Empty;

		public override ZDateTime LCLStorageDate =>
			!ShipmentParent.DocsAndCartage.JP_LCLStorageCommences.IsEmpty ?
			ShipmentParent.DocsAndCartage.JP_LCLStorageCommences :
			ShipmentSailing?.JX_DepotStorageDate ??
			ShipmentParent.MostInterestingTransport?.JW_DepotStorageDate ?? ZDateTime.Empty;

		public override ZDateTime FCLCutOff =>
			ShipmentSailing?.JX_JA_CTOCutOff ??
			ShipmentParent.MostInterestingTransport?.JW_TerminalCutOff ?? ZDateTime.Empty;

		public override ZDateTime FCLReceivalCommences =>
			ShipmentSailing?.JX_JA_CTOReceivalCommences ??
			ShipmentParent.MostInterestingTransport?.JW_TerminalReceivalCommences ?? ZDateTime.Empty;

		public override ZDateTime LCLCutOff =>
			ShipmentSailing?.JX_DepotCutOff ??
			ShipmentParent.MostInterestingTransport?.JW_DepotCutOff ?? ZDateTime.Empty;

		public override ZDateTime LCLReceivalCommences =>
			ShipmentSailing?.JX_DepotReceivalCommences ??
			ShipmentParent.MostInterestingTransport?.JW_DepotReceivalCommences ?? ZDateTime.Empty;

		protected void AddOrSetCompletedPickupConfirm(CommonShipment shipment, CommonPickupDeliveryConfirmCollection confirms, ZDateTime timeOut)
		{
			AddOrSetCompletedConfirm(confirms, timeOut);
			if (!confirms.Relationship.SupportsAddToRelationship() && (shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty || shipment.DocsAndCartage.JP_PickupCartageCompletedInfo.HasChanges))
			{
				shipment.DocsAndCartage.JP_PickupCartageCompleted = timeOut;
			}
		}

		protected void AddOrSetCompletedDeliveryConfirm(CommonShipment shipment, CommonPickupDeliveryConfirmCollection confirms, ZDateTime timeOut)
		{
			AddOrSetCompletedConfirm(confirms, timeOut);
			if (!confirms.Relationship.SupportsAddToRelationship() && (shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty || shipment.DocsAndCartage.JP_DeliveryCartageCompletedInfo.HasChanges))
			{
				shipment.DocsAndCartage.JP_DeliveryCartageCompleted = timeOut;
			}
		}

		protected void AddOrSetCompletedConfirm(CommonPickupDeliveryConfirmCollection confirms, ZDateTime timeOut)
		{
			if (confirms.Count > 1)
			{
				return;
			}

			CommonPickupDeliveryConfirm confirm = null;
			if (confirms.Count == 1)
			{
				confirm = confirms[0];
			}
			else if (confirms.Relationship.SupportsAddToRelationship())
			{
				confirm = confirms.AddNew();
			}

			if (confirm != null)
			{
				confirm.EU_PickupDeliveryTime = timeOut;
			}
		}
	}
}

using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business.DangerousGoodsExtensions;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public class ForwardingUNDGDataItemJTTValidation : ForwardingUNDGDataItemValidation
	{
		public ForwardingUNDGDataItemJTTValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		#region CheckDI_DG

		protected override void CheckDI_DG()
		{
			base.CheckDI_DG();

			var shipment = Parent?.Shipment;
			if (shipment is null)
			{
				return;
			}

			CheckProhibitions(shipment);
		}

		void CheckProhibitions(ForwardingShipment shipment)
		{
			CheckShipmentIsValidForJTTDangerousGoods(shipment);
			CheckPickupOrDeliveryTransportCompanyHasTransportPermit(shipment);
		}

		void CheckShipmentIsValidForJTTDangerousGoods(ForwardingShipment shipment)
		{
			if (!shipment.IsValidForJTTStandard())
			{
				Parent.DI_DGInfo.AddError(Res.GetString("99da3436-bb8f-9b8d-4fe1-534f2e5bd000",
					"JT/T 617 Substances are only allowed for shipments with domestic or international movements traveling by Road to/from Chinese territories."));
			}
		}

		void CheckPickupOrDeliveryTransportCompanyHasTransportPermit(ForwardingShipment shipment)
		{
			var docsAndCartage = shipment.DocsAndCartage;
			CheckPickupTransportCompanyHasTransportPermit(docsAndCartage);
			CheckDeliveryTransportCompanyHasTransportPermit(docsAndCartage);
		}

		void CheckPickupTransportCompanyHasTransportPermit(ForwardingDocsAndCartage docsAndCartage)
		{
			var pickupCompany = docsAndCartage.PickupCartageCo;
			if (pickupCompany is null)
			{
				return;
			}

			if (CompanyHasTransportPermit(pickupCompany))
			{
				Parent.DI_DGInfo.AddWarning(Res.GetString("b3b04821-c6b7-639e-4eb5-ddebe6416df6",
					"Ensure the selected Pickup Transport Company has the required authorizations to transport the specified goods."));
			}
			else
			{
				Parent.DI_DGInfo.AddWarning(Res.GetString("26d4931a-3cdf-c68b-4ecf-f46e4f49710f",
					"A valid road transport permit is not attached to the organization profile for the selected Pickup Transport Company."));
			}
		}

		void CheckDeliveryTransportCompanyHasTransportPermit(ForwardingDocsAndCartage docsAndCartage)
		{
			var deliveryCompany = docsAndCartage.DeliveryCartageCo;
			if (deliveryCompany is null)
			{
				return;
			}

			if (CompanyHasTransportPermit(deliveryCompany))
			{
				Parent.DI_DGInfo.AddWarning(Res.GetString("bbad11be-5c6f-2980-4fbb-1c9befa34e5e",
					"Ensure the selected Delivery Transport Company has the required authorizations to transport the specified goods."));
			}
			else
			{
				Parent.DI_DGInfo.AddWarning(Res.GetString("ebd43729-2c71-7eb1-44c1-5e89b760b3bb",
					"A valid road transport permit is not attached to the organization profile for the selected Delivery Transport Company."));
			}
		}

		bool CompanyHasTransportPermit(OrgHeader orgHeader)
		{
			var permitDocument = orgHeader.DocManagerInfo()
				.AllEDocs
				.GetMostRecentEDoc(Constants.RefDocTypes.Permit);

			return permitDocument != null;
		}

		#endregion

		#region CheckDI_DGWeight

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();

			var substance = Parent?.Substance;
			if (substance is null)
			{
				return;
			}

			CheckWeight(substance);
		}

		void CheckWeight(UNDGSubstance substance)
		{
			CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(substance, ExceptedQuantityMeasurementType.Weight);
		}

		#endregion

		#region CheckDI_DGVolume

		protected override void CheckDI_DGVolume()
		{
			base.CheckDI_DGVolume();

			var substance = Parent?.Substance;
			if (substance is null)
			{
				return;
			}

			CheckVolume(substance);
		}

		void CheckVolume(UNDGSubstance substance)
		{
			CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(substance, ExceptedQuantityMeasurementType.Volume);
		}

		void CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(UNDGSubstance substance, ExceptedQuantityMeasurementType measurementType)
		{
			if (!IsSubstancePermittedInLimitedQuantities(substance))
			{
				return;
			}

			var packType = Parent?.ParentPackLine.UNDGs.Count == 1
				? UNDGPackType.SingleUNDGPack
				: UNDGPackType.MultiUNDGPack;

			if (JTTValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(
				Parent, packType, measurementType))
			{
				var exceptedQuantityValidationMessage = Res.GetString("d41821af-61b3-6eb6-4f0b-5d42066a0fe6", "This value exceeds the maximum quantity per pack so cannot be transported in Excepted Quantities, per JT/T 617.");
				switch (measurementType)
				{
					case ExceptedQuantityMeasurementType.Weight:
						Parent.DI_DGWeightInfo.AddWarning(exceptedQuantityValidationMessage);
						break;

					case ExceptedQuantityMeasurementType.Volume:
						Parent.DI_DGVolumeInfo.AddWarning(exceptedQuantityValidationMessage);
						break;
				}
			}
		}

		#endregion
	}
}

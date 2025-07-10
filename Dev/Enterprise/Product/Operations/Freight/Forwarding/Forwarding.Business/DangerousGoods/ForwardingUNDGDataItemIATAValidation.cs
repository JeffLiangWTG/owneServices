using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public class ForwardingUNDGDataItemIATAValidation : ForwardingUNDGDataItemValidation
	{
		public ForwardingUNDGDataItemIATAValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		#region CheckDI_DG

		protected override void CheckDI_DG()
		{
			base.CheckDI_DG();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null || shipment is null)
			{
				return;
			}

			CheckProhibitions(shipment, substance);
		}

		void CheckProhibitions(ForwardingShipment shipment, UNDGSubstance substance)
		{
			var forbiddenMessage = Res.GetString("1AF5C802-9C53-4FD5-B6E1-E1410F3A694B",
				"This substance is forbidden for uplift by Air.");
			var forbiddenWithSpecialProvisionMessage = Res.GetString("26692A87-A8C0-4A55-8049-E380D90DC978",
				"This substance is forbidden for uplift by Air. \r\nIf you have Competent Authority Approval Certificate for this substance, please attach it to eDocs (with document type SCAA) before adding the substance to the Shipment.");
			var cargoOkayMessage = Res.GetString("91003574-4805-4127-B0C3-AF083EE8F20C",
				"This substance is forbidden for uplift by Air on a passenger flight but OK for cargo aircraft.");
			var specialProvisionMessage = Res.GetString("442C2E06-32CA-46D8-BC70-EA4BAF0FEDC9",
				"This substance is forbidden for uplift by air, but it is allowed under the Competent Authority Approval. \r\nPlease correct/remove this DG and Competent Authority Approval Certificate if this is an impossible condition.");

			if (substance.DG_CargoPackAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode)
			{
				if (shipment.Consols.Count > 0 && ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment))
				{
					CheckProhibitionPassenger(shipment, substance, forbiddenMessage, specialProvisionMessage, cargoOkayMessage, forbiddenWithSpecialProvisionMessage);
				}
				else if (shipment.Consols.Count == 0 || ShipmentIsAir(shipment))
				{
					CheckProhibitionCargo(shipment, substance, forbiddenMessage, specialProvisionMessage, forbiddenWithSpecialProvisionMessage);
				}
			}
			else if (substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode)
			{
				if (shipment.Consols.Count == 0 || ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment))
				{
					CheckProhibitionPassenger(shipment, substance, forbiddenMessage, specialProvisionMessage, cargoOkayMessage, forbiddenWithSpecialProvisionMessage);
				}
			}

			if (LithiumBatteryConstants.UNNOCodes.CodesList.Contains(substance.DG_UNNO))
			{
				AddUNDGPermissableQuantityConsigmentCheck();
				AddUNDGPermissableQuantitiesForbiddenCheck();
			}
		}

		void CheckProhibitionCargo(ForwardingShipment shipment, UNDGSubstance substance, ZString forbiddenMessage, ZString specialProvisionMessage, ZString forbiddenWithSpecialProvisionMessage)
		{
			if (substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.CargoOnly)
						|| substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.PassengerAndCargo))
			{
				if (shipment.ContainsDocType(Core.Constants.RefDocTypes.CompetentAuthorityApproval))
				{
					Parent.DI_DGInfo.AddWarning(specialProvisionMessage);
				}
				else
				{
					Parent.DI_DGInfo.AddError(forbiddenWithSpecialProvisionMessage);
				}
			}
			else
			{
				Parent.DI_DGInfo.AddError(forbiddenMessage);
			}
		}

		void CheckProhibitionPassenger(ForwardingShipment shipment, UNDGSubstance substance, ZString forbiddenMessage, ZString specialProvisionMessage, ZString cargoOkayMessage, ZString forbiddenWithSpecialProvisionMessage)
		{
			if (substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.PassengerAndCargo)
						&& shipment.ContainsDocType(Core.Constants.RefDocTypes.CompetentAuthorityApproval))
			{
				Parent.DI_DGInfo.AddWarning(specialProvisionMessage);
			}
			else
			{
				if (shipment.Consols.Count == 0)
				{
					Parent.DI_DGInfo.AddWarning(cargoOkayMessage);
				}
				else if (substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.PassengerAndCargo))
				{
					Parent.DI_DGInfo.AddError(forbiddenWithSpecialProvisionMessage);
				}
				else
				{
					Parent.DI_DGInfo.AddError(forbiddenMessage);
				}
			}
		}

		void AddUNDGPermissableQuantityConsigmentCheck()
		{
			var shipment = Parent?.ParentPackLine?.Shipment;
			if (!ForwardingUNDGPermissableQuantitiesHelper.AreUNDGQuantitiesPermissableForShipmentsConsignments(shipment))
			{
				Parent.DI_DGInfo.AddError(Res.GetString("fec1ee33-5349-e2b2-47de-950321dd9ed1",
					"Lithium ion batteries packed in accordance with Section II of Packing instructions 965 and 968 have a limit of one package per consignment."));
			}
		}

		void AddUNDGPermissableQuantitiesForbiddenCheck()
		{
			if (Parent.DI_DGWeightInfo.HasErrors() || Parent.DI_UnitOfWeightInfo.HasErrors())
			{
				return;
			}

			var (packingInstruction, packingInstructionSection) = ForwardingUNDGPermissableQuantitiesHelper.GetUNDGPackingInstructionAndPackingInstructionSectionIfNotPermissable(Parent);
			if (!packingInstructionSection.IsEmpty
				&& packingInstruction == LithiumBatteryConstants.RefPackingInstructions.Forbidden)
			{
				Parent.DI_DGInfo.AddError(Res.GetString("c47e6335-09cb-cf82-456f-1d988221dfbf",
					"Lithium ion batteries for this Section and Packing Instruction are forbidden."));
			}
		}

		#endregion

		protected override void CheckDI_IsLimitedQuantity()
		{
			base.CheckDI_IsLimitedQuantity();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null || shipment is null)
			{
				return;
			}

			if (Parent.DI_IsLimitedQuantity
				&& substance.DG_LQMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode)
			{
				Parent.DI_IsLimitedQuantityInfo.AddWarning(Res.GetString("2C5C2BBE-694D-4791-8045-5DF95376541F",
					"This substance is forbidden for uplift by Air in Limited Quantities."));
			}
		}

		#region CheckDI_DGWeight

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null
				|| shipment is null)
			{
				return;
			}

			CheckWeight(substance, shipment);
			if (Parent.DI_DGWeight > 0 && (Parent.ParentPackLine?.AreMultipleDGsPacked ?? false))
			{
				ValidateQValue(Parent.DI_DGWeightInfo);
			}
		}

		void CheckWeight(UNDGSubstance substance, ForwardingShipment shipment)
		{
			if (LithiumBatteryConstants.UNNOCodes.CodesList.Contains(substance.DG_UNNO))
			{
				AddUNDGPermissableQuantitiesCheck();
			}

			CheckWeightDoesNotExceedAircraftLimits(substance, shipment);
			CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(substance, ExceptedQuantityMeasurementType.Weight);
			CheckWeightLimitTypeAsGrossWeightLimit(substance, ExceptedQuantityMeasurementType.Weight);
		}

		void AddUNDGPermissableQuantitiesCheck()
		{
			if (Parent.DI_DGWeightInfo.HasErrors() || Parent.DI_UnitOfWeightInfo.HasErrors())
			{
				return;
			}

			var (packingInstruction, packingInstructionSection) = ForwardingUNDGPermissableQuantitiesHelper.GetUNDGPackingInstructionAndPackingInstructionSectionIfNotPermissable(Parent);
			if (!packingInstruction.IsEmpty
				&& !packingInstructionSection.IsEmpty
				&& packingInstruction != LithiumBatteryConstants.RefPackingInstructions.Forbidden)
			{
				Parent.DI_DGWeightInfo.AddError(Res.GetString("0041f73d-4b39-8185-41ae-e175ad177cb5",
					"Lithium ion batteries packed in accordance with Section {0} of Packing Instruction {1} exceed the quantity allowed.",
					packingInstructionSection,
					packingInstruction));
			}
		}

		void CheckWeightDoesNotExceedAircraftLimits(UNDGSubstance substance, ForwardingShipment shipment)
		{
			if (!ValidUNDGMaximumQuantityChecker.ShouldCheckWeight(substance))
			{
				return;
			}

			if ((ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment) || ShipmentIsAir(shipment)) &&
				(Parent.DI_DGWeight > 0) && (Parent.DI_PackageCount == 0))
			{
				Parent.DI_DGWeightInfo.AddError(Res.GetString("08ed9b63-ba35-4833-9329-cbf1561107cb",
					"Please enter a pack count/type so the system can check if this quantity exceeds the maximum allowed for a Cargo Only/Passenger Aircraft."));
				return;
			}

			var maximumMessage = Res.GetString("BAA9A1E1-797C-4A8F-BE57-CB77A94E7BFD",
				"This quantity exceeds the maximum allowed for a Passenger Aircraft.");

			if (ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment))
			{
				if (Parent.DI_IsLimitedQuantity)
				{
					if (ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedLimitedQuantityLimit(Parent))
					{
						Parent.DI_DGWeightInfo.AddError(maximumMessage);
					}
				}
				else
				{
					if (ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(Parent))
					{
						Parent.DI_DGWeightInfo.AddError(maximumMessage);
					}
				}
			}
			else if (ShipmentIsAir(shipment))
			{
				if (ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(Parent))
				{
					Parent.DI_DGWeightInfo.AddError(Res.GetString("C73DE2F4-BF28-44F9-8FAB-A5DBEA698238",
						"This quantity exceeds the maximum allowed for a Cargo Aircraft."));
				}
			}
		}

		void CheckWeightLimitTypeAsGrossWeightLimit(UNDGSubstance substance, ExceptedQuantityMeasurementType measurementType)
		{
			if (substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode || substance.DG_CargoPackAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode)
			{
				var exceptedQuantityValidationMessage = Res.GetString("a6754638-4f80-42f9-95dd-2d90bab99b51",
				"Gross weight is required for this dangerous good substance or article. There is no validation for the maximum gross weight limit.");

				if (measurementType == ExceptedQuantityMeasurementType.Weight)
				{
					Parent.DI_DGWeightInfo.AddWarning(exceptedQuantityValidationMessage);
				}
			}
		}

		#endregion

		#region CheckDI_DGVolume

		protected override void CheckDI_DGVolume()
		{
			base.CheckDI_DGVolume();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null || shipment is null)
			{
				return;
			}

			CheckVolume(shipment, substance);
			if (Parent.ParentPackLine?.AreMultipleDGsPacked ?? false)
			{
				ValidateQValue(Parent.DI_DGVolumeInfo);
			}
		}

		void CheckVolume(ForwardingShipment shipment, UNDGSubstance substance)
		{
			if (!ValidUNDGMaximumQuantityChecker.ShouldCheckVolume(substance))
			{
				return;
			}

			CheckVolumeDoesNotExceedAircraftLimits(shipment);
			CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(substance, ExceptedQuantityMeasurementType.Volume);
		}

		void CheckVolumeDoesNotExceedAircraftLimits(ForwardingShipment shipment)
		{
			var maximumMessage = Res.GetString("EF852091-140A-49E4-81D8-00366630F4D2",
				"This quantity exceeds the maximum allowed for a Passenger Aircraft.");

			if ((ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment) || ShipmentIsAir(shipment)) &&
				(Parent.DI_DGVolume > 0) && (Parent.DI_PackageCount == 0))
			{
				Parent.DI_DGVolumeInfo.AddError(Res.GetString("08ed9b63-ba35-4833-9329-cbf1561107cb",
					"Please enter a pack count/type so the system can check if this quantity exceeds the maximum allowed for a Cargo Only/Passenger Aircraft."));
				return;
			}

			if (ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(shipment))
			{
				if (Parent.DI_IsLimitedQuantity
					&& ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedLimitedQuantityLimit(Parent))
				{
					Parent.DI_DGVolumeInfo.AddError(maximumMessage);
				}
				else if (!Parent.DI_IsLimitedQuantity
					&& ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(Parent))
				{
					Parent.DI_DGVolumeInfo.AddError(maximumMessage);
				}
			}
			else if (ShipmentIsAir(shipment)
				&& ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(Parent))
			{
				Parent.DI_DGVolumeInfo.AddError(Res.GetString("C73DE2F4-BF28-44F9-8FAB-A5DBEA698238",
					"This quantity exceeds the maximum allowed for a Cargo Aircraft."));
			}
		}

		void CheckExceptedQuantityMaximumQuantitiesAreNotExceeded(UNDGSubstance substance, ExceptedQuantityMeasurementType measurementType)
		{
			if (!IsSubstancePermittedInLimitedQuantities(substance))
			{
				return;
			}

			var packType = Parent?.ParentPackLine?.UNDGs.Count == 1
				? UNDGPackType.SingleUNDGPack
				: UNDGPackType.MultiUNDGPack;

			if (!ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(Parent, packType))
			{
				return;
			}

			var exceptedQuantityValidationMessage = Res.GetString("a5dbb89a-941f-d09e-430a-6de3e7b03e80",
				"This value exceeds the maximum quantity per pack so cannot be transported in Excepted Quantities, per IATA DGR.");

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

		bool ShipmentIsAir(ForwardingShipment shipment)
		{
			if (shipment is null)
			{
				return false;
			}

			return shipment.JS_TransportMode == Constants.TransportModes.Air
				|| shipment.Consols.OfType<ForwardingConsol>()
				.Any(consol => consol.Transports.OfType<Transport>()
					.Any(transport => transport.TransportMode == Constants.TransportModes.Air));
		}

		bool ShipmentIsAttachedToConsolWithPassengerAirTransportLegs(ForwardingShipment shipment)
		{
			if (shipment is null)
			{
				return false;
			}

			return shipment.Consols.OfType<ForwardingConsol>()
				.Any(consol => consol.Transports.OfType<Transport>()
					.Any(transport => transport.TransportMode == Constants.TransportModes.Air
									&& !transport.JW_IsCargoOnly));
		}

		#endregion

		#region CheckDI_UnitOfVolume

		protected override void CheckDI_UnitOfVolume()
		{
			base.CheckDI_UnitOfVolume();
			if (Parent.DI_DGVolume > 0 && (Parent.ParentPackLine?.AreMultipleDGsPacked ?? false))
			{
				ValidateQValue(Parent.DI_UnitOfVolumeInfo);
			}
		}

		#endregion

		#region CheckDI_UnitOfWeight

		protected override void CheckDI_UnitOfWeight()
		{
			base.CheckDI_UnitOfWeight();
			if (Parent.DI_DGWeight > 0 && (Parent.ParentPackLine?.AreMultipleDGsPacked ?? false))
			{
				ValidateQValue(Parent.DI_UnitOfWeightInfo);
			}
		}

		#endregion

		#region Q-value

		void ValidateQValue(ZPropertyInfo info)
		{
			if (Parent == null || Parent.ParentPackLine == null)
			{
				return;
			}
			var maxQValueAllowed = Parent.ParentPackLine.JL_PackageCount > 0 ? Parent.ParentPackLine.JL_PackageCount : new ZInt(1);
			var qValue = Parent?.ParentPackLine.QValue;
			if (qValue > maxQValueAllowed)
			{
				info.AddWarning(Res.GetString("ad7c6aed-5a7f-41db-8d00-e8524f249c99",
					"Each outer pack of this pack line contains multiple dangerous goods whose 'Q' value is calculated as more than 1 which is not allowed per IATA DGR 5.0.2.11. DGs in this pack line must be split into separate pack lines until 'Q' value is equal to or less than 1."));
			}
			else if (qValue <= maxQValueAllowed && qValue > 1)
			{
				info.AddWarning(Res.GetString("0aed053e-134e-47bd-a72b-0101650c7cbd", "The DG Q-value for this packline is greater than 1, ensure every outer pack has the same DG quantity or the Q-value for each pack is less than or equal to 1"));
			}
		}

		#endregion

		protected override void CheckDI_OC_DGContact()
		{
			base.CheckDI_OC_DGContact();

			var infectiousSubstanceUndgs = new ZString[] { "2814", "2900" };
			var substance = Parent?.Substance;
			if (!(substance != null && infectiousSubstanceUndgs.Contains(substance.DG_UNNO)))
			{
				return;
			}

			if (Parent.DGContact is null || Parent.DGContact.OC_ContactName.IsEmpty)
			{
				Parent.DI_OC_DGContactInfo.AddError(Res.GetString("8113CF91-3D16-4C0E-8B39-FB12A8311207",
					"This substance requires the name and telephone number of a responsible person to be included on the Shipper's Declaration."));
			}
			else if (Parent.DGContact.OC_Mobile.IsEmpty
				&& Parent.DGContact.OC_HomePhone.IsEmpty
				&& Parent.DGContact.OC_Phone.IsEmpty)
			{
				Parent.DI_OC_DGContactInfo.AddError(Res.GetString("4F69C623-F5F2-488C-BD44-A40700633141",
					"This substance requires the name and telephone number of a responsible person to be included on the Shipper's Declaration. The DG Contact entered does not have a contact number. Press F3 on the DG Contact to save a Work, Mobile or Fax Number against the Contact."));
			}
		}

		protected override void CheckDI_PackingInstructionSection()
		{
			base.CheckDI_PackingInstructionSection();

			if (Parent?.Substance is UNDGSubstance substance
				&& LithiumBatteryConstants.UNNOCodes.CodesList.Contains(substance.DG_UNNO))
			{
				MandatoryValidation.CheckEntered(Parent.DI_PackingInstructionSectionInfo);

				ListValidation.ErrorIfInvalidCode(Parent.DI_PackingInstructionSectionInfo);

				AddUNDGPermissableQuantityULDCheck();
				AddIATADGRCheck(Parent, substance);
			}
		}

		void AddUNDGPermissableQuantityULDCheck()
		{
			var shipment = Parent?.ParentPackLine?.Shipment;
			if (!ForwardingUNDGPermissableQuantitiesHelper.AreUNDGQuantitiesPermissableForULD(shipment))
			{
				Parent.DI_PackingInstructionSectionInfo.AddWarning(Res.GetString("daa54a2c-59ce-60b4-4397-37b7b9ca1672",
					"Lithium ion batteries packed in accordance with Section II of Packing instructions 965 and 968 must not be loaded into a unit load device(ULD) before being tendered to the airline."));
			}
		}

		void AddIATADGRCheck(ForwardingUNDGDataItem parent, UNDGSubstance substance)
		{
			if (parent.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII
								&& (substance.DG_UNNO == LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries
								|| substance.DG_UNNO == LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries))
			{
				Parent.DI_PackingInstructionSectionInfo.AddError(Res.GetString("804E403C-F12F-42B4-8C7C-E3C477BF6E75", "Section II of PI 965/968 is not available for use as per DGR 63rd Edition."));
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineValidation : JobPackLinesValidation
	{
		public PackLineValidation(PackLine parent)
			: base(parent)
		{
		}

		new PackLine Parent
		{
			get { return (PackLine)base.Parent; }
		}

		protected override void CheckJL_Outturn()
		{
			base.CheckJL_Outturn();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnInfo);
		}

		protected override void CheckJL_Pillaged()
		{
			base.CheckJL_Pillaged();
			CompareValidation.CheckNumberNotNegative(Parent.JL_PillagedInfo);
		}

		protected override void CheckJL_Damaged()
		{
			base.CheckJL_Damaged();
			CompareValidation.CheckNumberNotNegative(Parent.JL_DamagedInfo);
		}

		protected override void CheckJL_F3_NKPackType()
		{
			base.CheckJL_F3_NKPackType();

			MandatoryValidation.CheckEntered(Parent.JL_F3_NKPackTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JL_F3_NKPackTypeInfo, Parent.JL_F3_NKPackType_List);
			MandatoryValidation.CheckUnitEntered(Parent.JL_F3_NKPackTypeInfo, Parent.JL_PackageCountInfo, Parent.IsInnerPackType ? Res.GetString("d73cc581-cd11-4aae-9464-3ce2a42e9db9", "inner-packline Pack Type") : Res.GetString("41468100-bd8c-4f27-8072-ee6656979cb1", "Pack Type"));
		}

		protected override void CheckJL_ActualVolume()
		{
			base.CheckJL_ActualVolume();
			CompareValidation.CheckNumberNotNegative(Parent.JL_ActualVolumeInfo);

			if (Parent.JL_Width != 0 && Parent.JL_Height != 0 && Parent.JL_Length != 0 && Parent.JL_PackageCount != 0)
			{
				if (ZArchitecture.Core.Utilities.Round(Parent.CalculatedVolume, JobPackLinesSchema.JL_ActualVolume.Scale) != ZArchitecture.Core.Utilities.Round(Parent.JL_ActualVolume, JobPackLinesSchema.JL_ActualVolume.Scale))
				{
					Parent.JL_ActualVolumeInfo.AddWarning(Res.GetString("549c692f-7d9a-4b13-aafb-626f8417fd30", "The actual volume does not match the volume calculated by the length x width x height x pkgs."));
				}
			}
		}

		protected override void CheckJL_Height()
		{
			base.CheckJL_Height();
			CompareValidation.CheckNumberNotNegative(Parent.JL_HeightInfo);
		}

		protected override void CheckJL_Length()
		{
			base.CheckJL_Length();
			CompareValidation.CheckNumberNotNegative(Parent.JL_LengthInfo);
		}

		protected override void CheckJL_Width()
		{
			base.CheckJL_Width();
			CompareValidation.CheckNumberNotNegative(Parent.JL_WidthInfo);
		}

		protected override void CheckJL_OutturnedVolume()
		{
			base.CheckJL_OutturnedVolume();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnedVolumeInfo);

			if (Parent.IsOutturned && Parent.JL_OutturnedWidth != 0 && Parent.JL_OutturnedHeight != 0 && Parent.JL_OutturnedLength != 0)
			{
				if (!Parent.JL_OutturnedVolumeInfo.HasErrors())
				{
					if (ZArchitecture.Core.Utilities.Round(Parent.CalculatedOutturnedVolume, JobPackLinesSchema.JL_OutturnedVolume.Scale) != ZArchitecture.Core.Utilities.Round(Parent.JL_OutturnedVolume, JobPackLinesSchema.JL_OutturnedVolume.Scale))
					{
						Parent.JL_OutturnedVolumeInfo.AddWarning(Res.GetString("35d20176-a737-4452-b82c-8f3ce32f2862", "The outturned volume does not match the volume calculated by the length x width x height x Outturned packages"));
					}
				}
			}
		}

		protected override void CheckJL_UnitOfDimension()
		{
			base.CheckJL_UnitOfDimension();
			ListValidation.ErrorIfInvalidCode(Parent.JL_UnitOfDimensionInfo, Parent.JL_UnitOfDimension_List);

			if (Parent.JL_Width != 0 || Parent.JL_Length != 0 || Parent.JL_Height != 0)
			{
				MandatoryValidation.CheckEntered(Parent.JL_UnitOfDimensionInfo);
			}
		}

		protected override void CheckJL_ActualWeightUQ()
		{
			base.CheckJL_ActualWeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JL_ActualWeightUQInfo, Parent.JL_ActualWeightUQ_List);

			if (Parent.JL_ActualWeight != 0)
			{
				MandatoryValidation.CheckEntered(Parent.JL_ActualWeightUQInfo);
			}
		}

		protected override void CheckJL_ActualVolumeUQ()
		{
			base.CheckJL_ActualVolumeUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JL_ActualVolumeUQInfo, Parent.JL_ActualVolumeUQ_List);

			if (Parent.JL_ActualVolume != 0)
			{
				MandatoryValidation.CheckEntered(Parent.JL_ActualVolumeUQInfo);
			}
		}

		protected override void CheckJL_RN_NKOrigin()
		{
			base.CheckJL_RN_NKOrigin();
			ListValidation.ErrorIfInvalidCode(Parent.JL_RN_NKOriginInfo, Parent.Lookups.Origins);
		}

		protected override void CheckJL_OutturnedHeight()
		{
			base.CheckJL_OutturnedHeight();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnedHeightInfo);
		}

		protected override void CheckJL_OutturnedLength()
		{
			base.CheckJL_OutturnedLength();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnedLengthInfo);
		}

		protected override void CheckJL_OutturnedWidth()
		{
			base.CheckJL_OutturnedWidth();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnedWidthInfo);
		}

		protected override void CheckJL_OutturnedWeight()
		{
			base.CheckJL_OutturnedWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JL_OutturnedWeightInfo);
		}

		protected override void CheckJL_RH_NKCommodityCode()
		{
			if (Parent.Shipment?.Consols != null)
			{
				Parent.Shipment.Consols.MarkAsNeedingValidation();
			}
			base.CheckJL_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.JL_RH_NKCommodityCodeInfo, Parent.Lookups.CommodityCodes);
		}

		protected override void CheckJL_ActualWeight()
		{
			base.CheckJL_ActualWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JL_ActualWeightInfo);

			if (Parent.JL_ActualWeight > 0)
			{
				foreach (CommonContainer container in Parent.Containers)
				{
					if (!container.IsEffectiveGrossWeightValid)
					{
						Parent.JL_ActualWeightInfo.AddError(Res.GetString("b3d094aa-3795-4fc7-afda-cb596daa3fb2", "The container '{0}' is over-packed, try packing one or more lines into another container, or reduce the lines' weight.", container.JC_ContainerNum));
						break;
					}
				}
			}
		}

		protected void CheckJL_ContainerPackingOrder_IsUnique()
		{
			if (Parent.Shipment != null)
			{
				var container = Parent.GetContainer(Parent.CurrentConsol);
				if (container != null)
				{
					var duplicatePackLines = new List<string>();

					foreach (PackLine otherPackLine in container.PackLines)
					{
						if (otherPackLine.PK != Parent.PK)
						{
							if (otherPackLine.Containers.Count > 1)
							{
								duplicatePackLines.Clear();
								break;
							}

							if (Parent.Containers.Count <= 1 && Parent.JL_ContainerPackingOrder == otherPackLine.JL_ContainerPackingOrder && otherPackLine.Shipment != null)
							{
								duplicatePackLines.AddIfNotContains(otherPackLine.Shipment.JS_UniqueConsignRef);
							}
						}
					}

					if (duplicatePackLines.Any())
					{
						string message = Res.GetString("4859b871-2817-4177-9f93-a06113a55487", "This value must be unique on the container.");
						if (Parent.JL_ContainerPackingOrder == 0)
						{
							Parent.JL_ContainerPackingOrderInfo.AddWarning(message);
						}
						else
						{
							duplicatePackLines.AddIfNotContains(Parent.Shipment.JS_UniqueConsignRef);
							message += " " + Res.GetString("70fe8899-047d-4e9b-8060-54db96942494", "Packing Order '{0}' is entered on the shipment(s) {1}.", Parent.JL_ContainerPackingOrder, string.Join(", ", duplicatePackLines));
							Parent.JL_ContainerPackingOrderInfo.AddError(message);
						}
					}
				}
			}
		}

		protected override void CheckJL_PackageCount()
		{
			base.CheckJL_PackageCount();
			CompareValidation.CheckNumberNotNegative(Parent.JL_PackageCountInfo);
		}

		protected override void CheckJL_RefNumber()
		{
			base.CheckJL_RefNumber();

			if (Parent.Shipment != null && Parent.Shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff)
			{
				if (Parent.JL_RefNumber.Length > 17)
				{
					Parent.JL_RefNumberInfo.AddError(Res.GetString("5c2ca56b-fc9d-4649-9fd9-b9ccfdd2fd9a", "VIN number can be no more than 17 characters long."));
				}

				if (!Parent.JL_RefNumber.IsEmpty && Parent.JL_PackageCount > 1)
				{
					Parent.JL_RefNumberInfo.AddError(Res.GetString("e83c8bd4-f38a-4354-884f-f31d9fc2f6bf", "You can't enter a VIN if the package count is greater than one."));
				}
			}
		}

		protected override void CheckJL_VehicleTransmission()
		{
			base.CheckJL_VehicleTransmission();
			ListValidation.ErrorIfInvalidCode(Parent.JL_VehicleTransmissionInfo);
		}

		#region JL_InspectionTypeCode

		public void ValidateJL_InspectionTypeCode()
		{
			ValidateCalculatedProperty(Parent.JL_InspectionTypeCodeInfo);
		}

		protected virtual void CheckJL_InspectionTypeCode()
		{ }

		#endregion

		#region JL_AdditionalInspectionTypeCode

		public void ValidateJL_AdditionalInspectionTypeCode()
		{
			ValidateCalculatedProperty(Parent.JL_AdditionalInspectionTypeCodeInfo);
		}

		protected virtual void CheckJL_AdditionalInspectionTypeCode()
		{ }

		#endregion

		protected override void CheckJL_OA_LastKnownTransitWarehouseAddress()
		{
			base.CheckJL_OA_LastKnownTransitWarehouseAddress();

			var shipment = Parent.Shipment;

			if (shipment != null)
			{
				var lastKnownTransitWarehousePK = Parent.LastKnownTransitWarehouseAddress?.Header?.PK;
				if (lastKnownTransitWarehousePK != null)
				{
					if (lastKnownTransitWarehousePK != shipment.ExportReceivingDepot?.Header?.PK && lastKnownTransitWarehousePK != shipment.ImportReleaseDepot?.Header?.PK &&
						shipment.Consols.Cast<CommonConsol>().All(consol => lastKnownTransitWarehousePK != consol.PackDepotAddress?.Header?.PK && lastKnownTransitWarehousePK != consol.UnpackDepotAddress?.Header?.PK))
					{
						Parent.JL_OA_LastKnownTransitWarehouseAddressInfo.AddWarning(Res.GetString("2a4afb8a-c3c7-48ec-a1c8-16b1efd18a86", "Transit Warehouse/CFS Organization selected does not match any of CFS’s on the Shipment or Consol."));
					}
				}

				var lastKnownTransitWarehouseAddressPK = Parent.JL_OA_LastKnownTransitWarehouseAddress;
				if (lastKnownTransitWarehouseAddressPK != null && lastKnownTransitWarehousePK != null)
				{
					if (lastKnownTransitWarehousePK == shipment.ExportReceivingDepot?.Header?.PK && lastKnownTransitWarehouseAddressPK != shipment.ExportReceivingDepot?.PK)
					{
						Parent.JL_OA_LastKnownTransitWarehouseAddressInfo.AddWarning(Res.GetString("67ab7a96-4d2a-44cb-a03a-ae90278e86c7", "This Organization's Warehouse, {0}, has different address.", Parent.LastKnownTransitWarehouseAddress.Header.OH_Code));
					}
					else if (lastKnownTransitWarehousePK == shipment.ImportReleaseDepot?.Header?.PK && lastKnownTransitWarehouseAddressPK != shipment.ImportReleaseDepot?.PK)
					{
						Parent.JL_OA_LastKnownTransitWarehouseAddressInfo.AddWarning(Res.GetString("67ab7a96-4d2a-44cb-a03a-ae90278e86c7", "This Organization's Warehouse, {0}, has different address.", Parent.LastKnownTransitWarehouseAddress.Header.OH_Code));
					}
					else if (shipment.Consols.Cast<CommonConsol>().Any(consol => (lastKnownTransitWarehousePK == consol.PackDepotAddress?.Header?.PK && lastKnownTransitWarehouseAddressPK != consol.PackDepotAddress?.PK) ||
						(lastKnownTransitWarehousePK == consol.UnpackDepotAddress?.Header?.PK && lastKnownTransitWarehouseAddressPK != consol.UnpackDepotAddress?.PK)))
					{
						Parent.JL_OA_LastKnownTransitWarehouseAddressInfo.AddWarning(Res.GetString("67ab7a96-4d2a-44cb-a03a-ae90278e86c7", "This Organization's Warehouse, {0}, has different address.", Parent.LastKnownTransitWarehouseAddress.Header.OH_Code));
					}
				}
			}
		}

		protected override void CheckJL_LastKnownTransitWarehouseStatusDateTime()
		{
			base.CheckJL_LastKnownTransitWarehouseStatusDateTime();

			if (!Parent.JL_LastKnownTransitWarehouseStatusDateTime.IsEmpty)
			{
				if (Parent.JL_LastKnownTransitWarehouseStatusDateTime.Date > ZDateTime.Now.Date)
				{
					Parent.JL_LastKnownTransitWarehouseStatusDateTimeInfo.AddWarning(Res.GetString("72cc7303-e1fe-49da-b298-6436d5bb9540", "Last Known TW Date should only allow current or past date."));
				}
			}
		}

		protected override void CheckJL_LastKnownTransitWarehouseStatus()
		{
			base.CheckJL_LastKnownTransitWarehouseStatus();
			ListValidation.ErrorIfInvalidCode(Parent.JL_LastKnownTransitWarehouseStatusInfo, Parent.JL_LastKnownTransitWarehouseStatus_List);
		}

		#region Implementation

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJL_InspectionTypeCode();
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackLineValidation : PackLineValidation
	{
		public ForwardingPackLineValidation(ForwardingPackLine parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		readonly new ForwardingPackLine Parent;

		#region JL_ContainerPackingOrder

		protected override void CheckJL_ContainerPackingOrder()
		{
			base.CheckJL_ContainerPackingOrder();

			CheckJL_ContainerPackingOrder_IsUnique();
		}

		#endregion

		#region JL_InspectionTypeCode

		protected override void CheckJL_InspectionTypeCode()
		{
			if (Parent.Shipment != null && Parent.Shipment.JS_IsForwardRegistered)
			{
				base.CheckJL_InspectionTypeCode();

				var supplyChainSecurityConfiguration = Parent.Shipment?.AviationSecurity.SupplyChainSecurityConfiguration;
				if (supplyChainSecurityConfiguration != null)
				{
					supplyChainSecurityConfiguration.CheckJL_InspectionTypeCode_AdditionalValidation(Parent);

					if (Parent.JL_InspectionTypeCodeHasChanges && !Parent.JL_InspectionTypeCode.Equals(Parent.JL_InspectionTypeCodeOriginalValue))
					{
						var error = supplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent.Shipment);
						if (!error.IsEmpty)
						{
							Parent.JL_InspectionTypeCodeInfo.AddError(error);
						}
					}
				}
			}
		}

		#endregion

		#region JL_IsHighRisk

		protected override void CheckJL_IsHighRisk()
		{
			if (Parent.Shipment != null && Parent.Shipment.JS_IsForwardRegistered)
			{
				var supplyChainSecurityConfiguration = Parent.Shipment.AviationSecurity.SupplyChainSecurityConfiguration;
				if (supplyChainSecurityConfiguration != null)
				{
					if (Parent.JL_IsHighRiskHasChanges && !Parent.JL_IsHighRisk.Equals(Parent.JL_IsHighRiskOriginalValue))
					{
						var error = supplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent.Shipment);
						if (!error.IsEmpty)
						{
							Parent.JL_IsHighRiskInfo.AddError(error);
						}
					}
				}
			}
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		protected override void CheckJL_AdditionalInspectionTypeCode()
		{
			base.CheckJL_AdditionalInspectionTypeCode();

			var supplyChainSecurityConfiguration = Parent.Shipment?.AviationSecurity.SupplyChainSecurityConfiguration;
			if (supplyChainSecurityConfiguration != null && supplyChainSecurityConfiguration.IsHighRiskApplicable && Parent.JL_IsHighRisk)
			{
				MandatoryValidation.CheckEntered(Parent.JL_AdditionalInspectionTypeCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JL_AdditionalInspectionTypeCodeInfo, Parent.AdditionalInspectionTypes);

				if (Parent.JL_AdditionalInspectionTypeCodeHasChanges && !Parent.JL_AdditionalInspectionTypeCode.Equals(Parent.JL_AdditionalInspectionTypeCodeOriginalValue))
				{
					var error = supplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent.Shipment);
					if (!error.IsEmpty)
					{
						Parent.JL_AdditionalInspectionTypeCodeInfo.AddError(error);
					}
				}
			}
		}

		#endregion

		#region JL_PackageCount

		protected override void CheckJL_PackageCount()
		{
			base.CheckJL_PackageCount();

			var isPackedIntoEmptyContainer = Parent.Containers.Cast<ForwardingContainer>().Any(x => x.JC_IsEmptyContainer);
			if (isPackedIntoEmptyContainer && Parent.JL_PackageCount > 0)
			{
				Parent.JL_PackageCountInfo.AddError(Res.GetString("2c4695d9-20c9-4a48-a274-5051cb5fac5a", "{0} cannot have pack lines with pack count greater than 0 as it is flagged empty.", Parent.Containers.Cast<ForwardingContainer>().First(x => x.JC_IsEmptyContainer).HumanReadableName));
			}
			else if (!isPackedIntoEmptyContainer && Parent.JL_PackageCount == 0)
			{
				Parent.JL_PackageCountInfo.AddWarning(Res.GetString("8a4b63d1-d139-49d0-ba61-53b1825cfbe1", "Zero packages is valid only for an empty container or an unknown number of packages. If your container is empty, flag it accordingly from Consol > Containers tab."));
			}
		}

		#endregion

		#region JL_JL_OuterPackLine

		protected override void CheckJL_JL_OuterPackLine()
		{
			base.CheckJL_JL_OuterPackLine();

			if (!Parent.JL_JL_OuterPackLine.IsValid
				|| Parent.JL_FreightMode != FreightConstants.InnerPackType)
			{
				return;
			}

			var parentShipmentHasInnerPackLinesLinkedToOuterPackline = Parent
				.Shipment?
				.InnerPackLines
				.OfType<ForwardingPackLine>()
				.Any(packLine => packLine.JL_JL_OuterPackLine.IsEmpty) ?? false;

			if (parentShipmentHasInnerPackLinesLinkedToOuterPackline)
			{
				Parent.JL_JL_OuterPackLineInfo.AddError(Res.GetString("27a92099-10db-4db3-4e51-22e961bbec12",
					"If at least one inner pack line is linked, it is mandatory for all inner pack lines to be linked to an outer pack."));
			}
		}

		#endregion

		#region JL_JC

		public void ValidateJL_JC()
		{
			ValidateCalculatedProperty(Parent.JL_JCInfo);
		}

		protected void CheckJL_JC()
		{
			foreach (var emptyContainer in Parent.Containers.Cast<ForwardingContainer>().Where(x => x.JC_IsEmptyContainer && x.PackLines.Count > 1))
			{
				var otherShipments = emptyContainer.GetParentShipments().Where(x => x.PK != Parent.JL_JS);
				if (otherShipments.Any())
				{
					var sb = new ZStringBuilder();
					foreach (var otherShipment in otherShipments)
					{
						sb.Append(otherShipment.JS_UniqueConsignRef);
					}

					Parent.JL_JCInfo.AddError(Res.GetString("16e74efb-e8a6-4076-9dc0-8a0765275c7b", "{0} cannot contain more than one pack line as it is flagged empty. The following shipments have pack lines which are packed in this container:\r\n{1}",
						emptyContainer.HumanReadableName,
						sb.ToStringWithDelimiterBetweenAppends(",")));
				}
				else
				{
					Parent.JL_JCInfo.AddError(Res.GetString("afa47320-d952-40c2-af1c-39b962f5a184", "{0} cannot contain more than one pack line as it is flagged empty.", emptyContainer.HumanReadableName));
				}
			}
		}

		#endregion

		#region JL_HarmonisedCode

		protected override void CheckJL_HarmonisedCode()
		{
			base.CheckJL_HarmonisedCode();
			HarmonisedCodeValidator.Validate(Parent.JL_HarmonisedCodeInfo, Parent.IsInDatabase);

			if (Parent.IsOuterPackType && Parent.HarmonisedCodeTariff == null)
			{
				if (Parent.JL_HarmonisedCode.Length < 6 && !Parent.JL_HarmonisedCode.IsEmpty)
				{
					Parent.JL_HarmonisedCodeInfo.AddWarning(
						Res.GetString("505c7716-6cac-437d-9d81-ab99feb2306f", "Harmonized Code is less than 6 characters.")
					);
				}
				else if (Parent.JL_HarmonisedCode.Length >= 6 && Parent.JL_HarmonisedCode.Length <= Parent.JL_HarmonisedCodeInfo.MaxLength)
				{
					Parent.JL_HarmonisedCodeInfo.AddWarning(
						Res.GetString("8a4226ff-8a48-4fce-a87f-f5174d3ed9df", "Harmonized Code is not from WCO tariff list.")
					);
				}
			}

			if (Parent.IsOuterPackType && Parent.JL_HarmonisedCode.IsEmpty)
			{
				if (RequiresCMDMessaging)
				{
					Parent.JL_HarmonisedCodeInfo.AddMessageError(
						Res.GetString("254245d0-971b-4a42-8c01-35c4d20c2d7a", "Harmonized Code is required for SG CMD messaging.")
					);
				}

				if (Parent.Shipment != null
					&& !Parent.Shipment.IsDomestic()
					&& ShipmentOriginOrDestinationIsInMalaysia
					&& !Parent.HarmonisedCodes.Any(hs => !hs.JLH_Code.IsEmpty && hs.JLH_RN_NKCountry == Core.Constants.CountryCodes.Malaysia))
				{
					Parent.JL_HarmonisedCodeInfo.AddWarning(
						Res.GetString("6ed3bdcd-13a7-4bbc-bac0-d7fdfb2c6f83", "HS Code is required for exports and imports to/from Malaysia.")
					);
				}
			}
		}

		bool ShipmentOriginOrDestinationIsInMalaysia =>
			(Parent.Shipment.Origin?.RL_RN_NKCountryCode ?? ZString.Empty) == Core.Constants.CountryCodes.Malaysia ||
			(Parent.Shipment.Destination?.RL_RN_NKCountryCode ?? ZString.Empty) == Core.Constants.CountryCodes.Malaysia;

		bool RequiresCMDMessaging
		{
			get
			{
				var transportMode = Parent.Shipment?.JS_TransportMode ?? ZString.Empty;
				return (Parent.CurrentConsol != null &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore &&
					(transportMode == Core.Constants.TransportModes.Air ||
					 transportMode == Core.Constants.TransportModes.AirSea ||
					 transportMode == Core.Constants.TransportModes.SeaAir) &&
					(Parent.CurrentConsol.JK_RL_NKLoadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore ||
					 Parent.CurrentConsol.JK_RL_NKDischargePort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore));
			}
		}

		#endregion

		protected override void CheckJL_ExportRefNumber()
		{
			base.CheckJL_ExportRefNumber();

			if (!Parent.JL_ExportRefNumber.IsEmpty)
			{
				if (Parent.JL_ExportRefNumber.Contains(','))
				{
					Parent.JL_ExportRefNumberInfo.AddError(Res.GetString("78241C20-CE31-475B-969F-0145631DC598", "Enter just one Export Reference per pack line."));
				}

				if (Parent.Shipment != null &&
					Parent.Shipment.CustomsEntryNumberType == CusEntryNumberTypes.Standard.MovementReferenceNumber &&
					Parent.Shipment.CustomsEntryNumber.IsEmpty)
				{
					var errors = MRNValidationHelper.CheckMRNFormat(Parent.JL_ExportRefNumber, Parent.Factory);
					errors.ForEach(error => Parent.JL_ExportRefNumberInfo.AddWarning(error));
				}

				var countryCodes = new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Taiwan, Core.Constants.CountryCodes.HongKong };
				if (countryCodes.Contains(GlbCompany.CurrentCompany.Country.Code) && countryCodes.Contains(Parent.Shipment.JS_RL_NKOrigin.SubstringSafe(0, 2)))
				{
					var otherShipmentsHaveTheSameExportRefNumber = ShippingOrderNumberValidationHelper.GetDuplicatedShipmentsByShippingOrderNumber(Parent.JL_ExportRefNumber, Parent.Shipment.PK, Parent.Factory);
					if (otherShipmentsHaveTheSameExportRefNumber.Length > 0)
					{
						Parent.JL_ExportRefNumberInfo.AddWarning(Res.GetString("d7a63583-3fbf-4624-aad5-6ddc91771c09", "This Shi Lian Dan/Shipping Order Number is already in use on: {0}", string.Join(", ", otherShipmentsHaveTheSameExportRefNumber.Select(otherShipment => otherShipment.JS_UniqueConsignRef))));
					}
				}
			}
		}

		protected override void CheckJL_ImportRefNumber()
		{
			base.CheckJL_ImportRefNumber();

			if (!Parent.JL_ImportRefNumber.IsEmpty && Parent.JL_ImportRefNumber.Contains(','))
			{
				Parent.JL_ImportRefNumberInfo.AddError(Res.GetString("F69EAB1D-6638-4933-9FDF-BACBAFB21757", "Enter just one Import Reference per pack line."));
			}
		}

		#region JL_RequiresTemperatureControl

		protected override void CheckJL_RequiresTemperatureControl()
		{
			base.CheckJL_RequiresTemperatureControl();

			if (!Parent.JL_RequiresTemperatureControl
				&& (!Parent.JL_RequiredTemperatureMaximum.IsDefault
				|| !Parent.JL_RequiredTemperatureMinimum.IsDefault))
			{
				Parent.JL_RequiresTemperatureControlInfo.AddError(Res.GetString("89cf533e-b5f8-4197-4f92-b88906e11dbb",
					"Is Temperature Control flag should be set when a temperature range is specified."));
				return;
			}

			if (Parent.Shipment != null)
			{
				var invalidConsolidations = Parent.Shipment.Consols.OfType<ForwardingConsol>().Where(c => !c.ShipmentTemperatureRangeIsValid(Parent.Shipment)).Select(c => c.HumanReadableName).ToList();
				if (invalidConsolidations.Count > 0)
				{
					Parent.JL_RequiresTemperatureControlInfo.AddError(Res.GetString("53a214d7-ebad-68b7-4f84-09551bbdb186",
						"The following Consol(s) do not support the temperature range of this pack line: {0}", string.Join(System.Environment.NewLine, invalidConsolidations)));
				}

				if (!Parent.JL_RequiresTemperatureControl)
				{
					var consolidationsRequiringTemperatureControl = Parent.Shipment.Consols.OfType<ForwardingConsol>().Where(c => c.JK_RequiresTemperatureControl).Select(c => c.HumanReadableName).ToList();
					if (consolidationsRequiringTemperatureControl.Count > 0)
					{
						Parent.JL_RequiresTemperatureControlInfo.AddWarning(Res.GetString("98b61d2e-8fe5-4170-a5e2-a9bf993da00b",
							"This shipment is attached to the following temperature controlled Consol(s). Please verify the temperature range of the Consol(s) is suitable for this cargo: {0}",
							string.Join(System.Environment.NewLine, consolidationsRequiringTemperatureControl)));
					}
				}
			}
		}

		#endregion

		#region JL_RequiredTemperatureUnit

		protected override void CheckJL_RequiredTemperatureUnit()
		{
			base.CheckJL_RequiredTemperatureUnit();

			if (Parent.JL_RequiredTemperatureUnit != Core.Constants.Temperature.Centigrade && Parent.JL_RequiredTemperatureUnit != Core.Constants.Temperature.Fahrenheit)
			{
				Parent.JL_RequiredTemperatureUnitInfo.AddError(Res.GetString("49139d31-a8c3-43df-84b1-eb1dd5feb615", "Temperature must be set to C (Celsius) or F (Fahrenheit)"));
			}
		}

		#endregion

		#region JL_RequiredTemperature MinAndMax

		protected override void CheckJL_RequiredTemperatureMinimum()
		{
			base.CheckJL_RequiredTemperatureMinimum();

			if (Parent.JL_RequiredTemperatureMinimum > Parent.JL_RequiredTemperatureMaximum)
			{
				Parent.JL_RequiredTemperatureMinimumInfo.AddError(Res.GetString("2e5eecd5-6ed4-4292-a79d-01f237d1902a", "Minimum temperature cannot be higher than maximum temperature"));
			}

			if (Parent.JL_RequiredTemperatureUnit == Core.Constants.Temperature.Centigrade || Parent.JL_RequiredTemperatureUnit == Core.Constants.Temperature.Fahrenheit)
			{
				if (Parent.JL_RequiredTemperatureMinimum < GetAbsoluteZero(Parent.JL_RequiredTemperatureUnit))
				{
					Parent.JL_RequiredTemperatureMinimumInfo.AddError(Res.GetString("e60eaeab-37e5-4ec3-b750-e6bcc1cb47e7",
						"{0}°{2} is below the minimum possible temperature of absolute zero ({1}°{2})",
						Parent.JL_RequiredTemperatureMinimum, GetAbsoluteZero(Parent.JL_RequiredTemperatureUnit), Parent.JL_RequiredTemperatureUnit));
				}

				if (Parent.JL_RequiresTemperatureControl && Parent.CommodityCode != null)
				{
					var commodityMinTemp = decimal.Round(Core.Constants.Temperature.Convert(Parent.CommodityCode.RH_ReeferMinTemperature, Core.Constants.Temperature.Centigrade, Parent.JL_RequiredTemperatureUnit), 1);

					if (commodityMinTemp != Parent.JL_RequiredTemperatureMinimum)
					{
						Parent.JL_RequiredTemperatureMinimumInfo.AddWarning(Res.GetString("8f9e52e6-b8e4-4cd1-a4a1-e7584ae0c29b",
							"The temperature range entered does not match the temperature range of commodity {0} of this pack line", Parent.CommodityCode.RH_Code));
					}
				}
			}
		}

		protected override void CheckJL_RequiredTemperatureMaximum()
		{
			base.CheckJL_RequiredTemperatureMaximum();

			if (Parent.JL_RequiredTemperatureMaximum < Parent.JL_RequiredTemperatureMinimum)
			{
				Parent.JL_RequiredTemperatureMaximumInfo.AddError(Res.GetString("2e63dd1a-4025-419a-b6a8-07e8a28b40de", "Maximum temperature cannot be lower than minimum temperature"));
			}

			if (Parent.JL_RequiredTemperatureUnit == Core.Constants.Temperature.Centigrade || Parent.JL_RequiredTemperatureUnit == Core.Constants.Temperature.Fahrenheit)
			{
				if (Parent.JL_RequiredTemperatureMaximum < GetAbsoluteZero(Parent.JL_RequiredTemperatureUnit))
				{
					Parent.JL_RequiredTemperatureMaximumInfo.AddError(Res.GetString("e60eaeab-37e5-4ec3-b750-e6bcc1cb47e7",
						"{0}°{2} is below the minimum possible temperature of absolute zero ({1}°{2})",
						Parent.JL_RequiredTemperatureMaximum, GetAbsoluteZero(Parent.JL_RequiredTemperatureUnit), Parent.JL_RequiredTemperatureUnit));
				}

				if (Parent.JL_RequiresTemperatureControl && Parent.CommodityCode != null)
				{
					var commodityMaxTemp = decimal.Round(Core.Constants.Temperature.Convert(Parent.CommodityCode.RH_ReeferMaxTemperature, Core.Constants.Temperature.Centigrade, Parent.JL_RequiredTemperatureUnit), 1);

					if (commodityMaxTemp != Parent.JL_RequiredTemperatureMaximum)
					{
						Parent.JL_RequiredTemperatureMaximumInfo.AddWarning(Res.GetString("8f9e52e6-b8e4-4cd1-a4a1-e7584ae0c29b",
							"The temperature range entered does not match the temperature range of commodity {0} of this pack line", Parent.CommodityCode.RH_Code));
					}
				}
			}
		}

		ZDecimal GetAbsoluteZero(ZString unit) => Core.Constants.Temperature.Convert(0, Core.Constants.Temperature.Kelvin, unit);

		#endregion

		#region JL_Dimensions

		protected override void CheckJL_Height()
		{
			base.CheckJL_Height();
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JL_HeightInfo, result => result.ItemFitsHeight);
		}

		protected override void CheckJL_Length()
		{
			base.CheckJL_Length();
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JL_LengthInfo, result => result.ItemFitsWidthAndLength);
		}

		protected override void CheckJL_Width()
		{
			base.CheckJL_Width();
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JL_WidthInfo, result => result.ItemFitsWidthAndLength);
		}

		void CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(ZPropertyInfo propertyInfo, Func<CargoDimensionsHelpers.CheckFitsInConsolResult, bool> check)
		{
			var packLine = Parent.Cast<ForwardingPackLine>().FirstOrDefault();
			var consols = packLine?.Shipment?.Consols.Cast<ForwardingConsol>();
			var packLineCanFitInAllConsols = consols?.All(consol => check(CargoDimensionsHelpers.CheckCanFitInConsol(consol, packLine))) ?? true;

			if (!packLineCanFitInAllConsols)
			{
				var message = Res.GetString("a465993b-0fdc-7192-4ebf-d660e5a9c90f", "This dimension exceeds the maximum dimensions set on the consol.");
				CargoDimensionsHelpers.AddPreAllocationCheckDimensionsError(propertyInfo, message);
			}
		}

		#endregion

		protected override void CheckJL_PackLineId()
		{
			base.CheckJL_PackLineId();

			if (Parent.PkgPackageCollection.Any() && FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value)
			{
				var packLineWrapper = new ForwardingComparisonPackLineWrapper(Parent);
				var packageWrapperCollection = Parent.PkgPackageCollection.Select(p => new PkgComparisonPackageWrapper(p, Parent.Shipment.RequiresSecuredCargoFromWarehouse)).ToList();
				var discrepancies = new SortedSet<string>();

				if (packLineWrapper.PackageCount != Parent.PkgPackageCollection_TotalQty)
				{
					discrepancies.Add(Res.GetString("43fb0f1a-d240-28be-470f-392c9582f56e", "Pack Count"));
				}

				if (packLineWrapper.Volume != packLineWrapper.PkgPackageCollection_TotalVolume)
				{
					discrepancies.Add(Res.GetString("6fdb267a-aaad-7785-41c5-5b3778d53018", "Volume"));
				}

				if (packLineWrapper.Weight != packLineWrapper.PkgPackageCollection_TotalWeight)
				{
					discrepancies.Add(Res.GetString("a902d819-fc09-9585-428e-aacf9366b60f", "Weight"));
				}

				foreach (var packageWrapper in packageWrapperCollection)
				{
					discrepancies.UnionWith(packLineWrapper.CompareSharedAttributes(packageWrapper, true));
				}

				if (discrepancies.Any())
				{
					Parent.JL_PackLineIdInfo.AddWarning(Res.GetString("511E5DD7-9446-4197-9218-E34F24FC9A2B", "Discrepancies on the following properties are detected between the pack line and its packages: {0}", string.Join(", ", discrepancies)));
				}
			}
		}

		#region LooseCargoContainerType

		public void ValidateLooseCargoContainerType()
		{
			ValidateCalculatedProperty(Parent.LooseCargoContainerTypeInfo);
		}

		protected void CheckLooseCargoContainerType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.LooseCargoContainerTypeInfo, Parent.LooseCargoContainerType_List);
		}

		#endregion

		#region Implementation

		public override void ValidateAll()
		{
			ValidateJL_JC();
			ValidateJL_PackLineId();
			ValidateLooseCargoContainerType();
			base.ValidateAll();
		}

		#endregion
	}
}

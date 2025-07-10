using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentValidation : JobShipmentValidation
	{
		public CommonShipmentValidation(CommonShipment parent)
			: base(parent)
		{
		}

		#region Qtys, Weights, Volumes

		protected override void CheckJS_UnitOfWeight()
		{
			base.CheckJS_UnitOfWeight();

			MandatoryValidation.CheckUnitEntered(Parent.JS_UnitOfWeightInfo, Parent.JS_ActualWeightInfo);

			if (Parent.JS_ActualWeight <= 0)
			{
				MandatoryValidation.CheckUnitEntered(Parent.JS_UnitOfWeightInfo, Parent.JS_DocumentedWeightInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JS_UnitOfWeightInfo, Parent.Lookups.JS_UnitOfWeight_List);
		}

		protected override void CheckJS_UnitOfVolume()
		{
			base.CheckJS_UnitOfVolume();

			MandatoryValidation.CheckUnitEntered(Parent.JS_UnitOfVolumeInfo, Parent.JS_ActualVolumeInfo);

			if (Parent.JS_ActualVolume <= 0)
			{
				MandatoryValidation.CheckUnitEntered(Parent.JS_UnitOfVolumeInfo, Parent.JS_DocumentedVolumeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JS_UnitOfVolumeInfo, Parent.Lookups.JS_UnitOfVolume_List);
		}

		protected override void CheckJS_F3_NKPackType()
		{
			base.CheckJS_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.JS_F3_NKPackTypeInfo, Parent.Lookups.JS_PackType_List);
			MandatoryValidation.CheckUnitEntered(Parent.JS_F3_NKPackTypeInfo, Parent.JS_OuterPacksInfo);
		}

		protected override void CheckJS_ActualVolume()
		{
			if (Parent.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.FCL
				|| Parent.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.FTL)
			{
				// ClearAllNotifications
			}
			else
			{
				base.CheckJS_ActualVolume();

				if (Parent.IsCoLoadMaster || Parent.IsBlindCoLoadMaster)
				{
					ZDecimal subShipmentTotal = 0;

					foreach (CommonShipment shipment in Parent.CoLoadShipments)
					{
						if (Constants.Volume.ContainsCode(shipment.JS_UnitOfVolume) && Constants.Volume.ContainsCode(Parent.JS_UnitOfVolume))
						{
							subShipmentTotal += Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, Parent.JS_UnitOfVolume);
						}
					}

					if (subShipmentTotal > Parent.JS_ActualVolume)
					{
						Parent.JS_ActualVolumeInfo.AddWarning(
							 Res.GetString("84f0f77f-0c36-4d96-8238-5c527e3d1d89", "Total volumes are calculated from the master Shipment not sub-shipments. Yet this master has an entered volume less than the total of it's sub shipments."));
					}
				}

				if (ShouldCheckMeasuresAgainstPacklinesTotals && !Parent.JS_ActualVolumeInfo.HasNotifications() && Parent.JS_ActualVolume != Parent.TotalOuterPacksVolume)
				{
					Parent.JS_ActualVolumeInfo.AddWarning(Res.GetString("de54c548-5b2a-480c-8727-286747c4ff81", "Entered volume does not match total volume of the packlines."));
				}
			}

			if (Parent.JS_ActualVolume == 0)
			{
				var message = Res.GetString("35e297c3-e5ad-4d96-8ffa-82fc8b29d7b0", "You have not entered a Shipment Volume.");
				Parent.JS_ActualVolumeInfo.AddWarning(message);
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.JS_ActualVolumeInfo);
			}
		}

		protected virtual bool ShouldCheckMeasuresAgainstPacklinesTotals
		{
			get { return true; }
		}

		protected override void CheckJS_ActualChargeable()
		{
			base.CheckJS_ActualChargeable();
			CompareValidation.CheckNumberNotNegative(Parent.JS_ActualChargeableInfo);
		}

		protected override void CheckJS_DocumentedWeight()
		{
			base.CheckJS_DocumentedWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JS_DocumentedWeightInfo);
		}

		protected override void CheckJS_DocumentedVolume()
		{
			base.CheckJS_DocumentedVolume();
			CompareValidation.CheckNumberNotNegative(Parent.JS_DocumentedVolumeInfo);
		}

		protected override void CheckJS_DocumentedChargeable()
		{
			base.CheckJS_DocumentedChargeable();
			CompareValidation.CheckNumberNotNegative(Parent.JS_DocumentedChargeableInfo);
		}

		protected override void CheckJS_ManifestedWeight()
		{
			base.CheckJS_ManifestedWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JS_ManifestedWeightInfo);
		}

		protected override void CheckJS_ManifestedVolume()
		{
			base.CheckJS_ManifestedVolume();
			CompareValidation.CheckNumberNotNegative(Parent.JS_ManifestedVolumeInfo);
		}

		protected override void CheckJS_ManifestedChargeable()
		{
			base.CheckJS_ManifestedChargeable();
			CompareValidation.CheckNumberNotNegative(Parent.JS_ManifestedChargeableInfo);
		}

		protected override void CheckJS_TotalPackageCount()
		{
			base.CheckJS_TotalPackageCount();
			CompareValidation.CheckNumberNotNegative(Parent.JS_TotalPackageCountInfo);
		}

		protected override void CheckJS_OuterPacks()
		{
			base.CheckJS_OuterPacks();

			CompareValidation.CheckNumberNotNegative(Parent.JS_OuterPacksInfo);

			if (ShouldCheckMeasuresAgainstPacklinesTotals && !Parent.JS_OuterPacksInfo.HasNotifications() && Parent.TotalOuterPacks != Parent.JS_OuterPacks)
			{
				Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("a3f26f29-a73e-42bd-b110-5f474cd00f13", "Entered number of packs does not equal the total number in the Pack Lines."));
			}
		}

		#endregion

		#region Orign/Destination

		bool IsQuotedBookingValidationOnDestination
		{
			get { return Parent.JS_IsBooking && !Parent.JS_IsForwardRegistered && Parent.IsDomesticFreight && Parent.JS_RL_NKDestination.Length == 2; }
		}

		protected bool ShouldRunAdditionalValidationOnDestination
		{
			get { return !IsQuotedBookingValidationOnDestination; }
		}

		protected override void CheckJS_RL_NKDestination()
		{
			base.CheckJS_RL_NKDestination();
			// If it's used in QuotedBooking for pre-allocation
			if (IsQuotedBookingValidationOnDestination)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKDestinationInfo, Parent.Lookups.BindingLists.RefCountry_List);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKDestinationInfo, Parent.Lookups.RefUNLOCO_List);
				if (ShouldAddWarningToOriginAndDestination)
				{
					string warning = Res.GetString("3267eed0-0752-4351-8d59-2d440338590b", "Destination and Origin should generally be different locations.");
					if (!Parent.JS_RL_NKDestinationInfo.HasWarning(warning))
					{
						Parent.JS_RL_NKDestinationInfo.AddWarning(warning);
					}
					ValidateJS_RL_NKOrigin();
				}
			}
			ValidateJS_CommunityTransitStatus();
			Parent.Validation.ValidateIsDomesticFreight();
		}

		bool IsQuotedBookingValidationOnOrigin
		{
			get { return Parent.JS_IsBooking && !Parent.JS_IsForwardRegistered && Parent.IsDomesticFreight && Parent.JS_RL_NKOrigin.Length == 2; }
		}

		protected bool ShouldRunAdditionalValidationOnOrigin
		{
			get { return !IsQuotedBookingValidationOnOrigin; }
		}

		protected override void CheckJS_RL_NKOrigin()
		{
			base.CheckJS_RL_NKOrigin();

			if (IsQuotedBookingValidationOnOrigin)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKOriginInfo, Parent.Lookups.BindingLists.RefCountry_List);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKOriginInfo, Parent.Lookups.RefUNLOCO_List);
				if (ShouldAddWarningToOriginAndDestination)
				{
					string warning = Res.GetString("ad6d4d2e-8ba0-4117-b1bc-d93e576c1447", "Origin and Destination should generally be different locations.");
					if (!Parent.JS_RL_NKOriginInfo.HasWarning(warning))
					{
						Parent.JS_RL_NKOriginInfo.AddWarning(warning);
					}
					ValidateJS_RL_NKDestination();
				}
			}
			Parent.Validation.ValidateIsDomesticFreight();

			if (!Parent.JS_RL_NKOriginInfo.HasErrors() && Parent.ShipmentTravellingInReverseOfAConsol())
			{
				var errorMessage = $"Origin = {Parent.JS_RL_NKOrigin} and Destination = {Parent.JS_RL_NKDestination}"
					+ (NoResString)" but this Shipment is on a Consol moving in the opposite direction between the same ports. "
					+ (NoResString)"Origin and Destination may have been accidentally swapped.";

				Parent.JS_RL_NKOriginInfo.AddError(errorMessage);
			}
			ValidateJS_CommunityTransitStatus();
		}

		bool ShouldAddWarningToOriginAndDestination
		{
			get
			{
				if (!Parent.JS_RL_NKOrigin.IsEmpty && !Parent.JS_RL_NKDestination.IsEmpty)
				{
					if (Parent.JS_RL_NKOrigin == Parent.JS_RL_NKDestination)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Dates

		bool ShipmentCanTravelBackInTime => Parent.JS_RL_NKOrigin.StartsWith(CountryCodes.WesternSamoa) && Parent.JS_RL_NKDestination.StartsWith(CountryCodes.AmericanSamoa);

		protected override void CheckJS_E_ARV()
		{
			if (!ValidatingARV)
			{
				ValidatingARV = true;
				base.CheckJS_E_ARV();

				if (Parent.IsAir)
				{
					if (Parent.JS_E_DEP.IsValid && Parent.JS_E_ARV.IsValid && Parent.JS_E_ARV < Parent.JS_E_DEP.AddDays(-1))
					{
						Parent.JS_E_ARVInfo.AddError(Res.GetString("43c9c2bf-191d-451e-84b7-3b40540562a5", "ETA cannot be more than a day before Shipment ETD."));
					}
				}
				else if (Parent.JS_E_DEP.IsValid && Parent.JS_E_ARV.IsValid)
				{
					if (ShipmentCanTravelBackInTime)
					{
						if (Parent.JS_E_ARV < Parent.JS_E_DEP.AddDays(-1))
						{
							Parent.JS_E_ARVInfo.AddError(Res.GetString("ec25780b-f8dc-485b-8491-4a6ed7576e2f", "ETA cannot be before Shipment ETD by more than allowable 1 day limit exception for route WSAPW to ASPPG due to crossing over the International Date line."));
						}
					}
					else
					{
						if (Parent.JS_E_ARV < Parent.JS_E_DEP)
						{
							Parent.JS_E_ARVInfo.AddError(Res.GetString("13210036-a66c-4675-9144-c4539aa522f4", "ETA cannot be before Shipment ETD."));
						}
					}
				}
				if (Parent.JS_E_DEPInfo.HasNotifications())
				{
					ValidateJS_E_DEP();
				}

				foreach (CommonConsol consol in Parent.Consols)
				{
					if (Parent.JS_E_ARV.IsValid && consol.JK_JX_JB_E_ARV.IsValid && Parent.JS_E_ARV < consol.JK_JX_JB_E_ARV && !Parent.JS_E_ARVInfo.HasWarnings())
					{
						Parent.JS_E_ARVInfo.AddWarning(Res.GetString("d56aaad1-4011-463f-a83d-8d84caf691ae", "ETA of a Shipment cannot be before the ETA of a consol."));
						break;
					}
					else if (Parent.JS_E_ARV.IsValid && consol.JK_JX_JB_E_ARV.IsValid && Parent.JS_E_ARV > consol.JK_JX_JB_E_ARV.AddMonths(3) && !Parent.JS_E_ARVInfo.HasWarnings())
					{
						Parent.JS_E_ARVInfo.AddWarning(Res.GetString("c11d10ef-489f-4a0f-9b90-8a5a8b6bc4a1", "ETA of a Shipment cannot be more than 3 months after a consol ETA."));
						break;
					}
				}

				ValidatingARV = false;
			}
		}

		protected override void CheckJS_E_DEP()
		{
			if (!ValidatingDEP)
			{
				ValidatingDEP = true;
				base.CheckJS_E_DEP();

				if (Parent.IsAir)
				{
					if (Parent.JS_E_DEP.IsValid && Parent.JS_E_ARV.IsValid && Parent.JS_E_DEP > Parent.JS_E_ARV.AddDays(1))
					{
						Parent.JS_E_DEPInfo.AddError(Res.GetString("d34f2b28-5ebe-4764-9e13-51a04e60f57b", "ETD cannot be more than a day after Shipment ETA."));
					}
				}
				else if (Parent.JS_E_DEP.IsValid && Parent.JS_E_ARV.IsValid)
				{
					if (ShipmentCanTravelBackInTime)
					{
						if (Parent.JS_E_DEP > Parent.JS_E_ARV.AddDays(1))
						{
							Parent.JS_E_DEPInfo.AddError(Res.GetString("852b4075-28f7-4020-a55e-e88c5277c7b3", "ETD cannot be after Shipment ETA by more than allowable 1 day limit exception for route WSAPW to ASPPG due to crossing over the International Date line."));
						}
					}
					else
					{
						if (Parent.JS_E_DEP > Parent.JS_E_ARV)
						{
							Parent.JS_E_DEPInfo.AddError(Res.GetString("f82e7d82-08f3-4fbb-ae78-d580f557493d", "ETD cannot be after Shipment ETA."));
						}
					}
				}

				if (Parent.JS_E_ARVInfo.HasNotifications())
				{
					ValidateJS_E_ARV();
				}

				foreach (CommonConsol consol in Parent.Consols)
				{
					if (Parent.JS_E_DEP.IsValid && consol.JK_JX_JA_E_DEP.IsValid && Parent.JS_E_DEP < consol.JK_JX_JA_E_DEP.AddMonths(-3) && !Parent.JS_E_DEPInfo.HasWarnings())
					{
						Parent.JS_E_DEPInfo.AddWarning(Res.GetString("471292d3-02d7-4a06-884b-2da1983242fb", "Shipment ETD cannot be more than 3 months before Consol ETD") + " ");
						break;
					}
					else if (Parent.JS_E_DEP.IsValid && consol.JK_JX_JA_E_DEP.IsValid && Parent.JS_E_DEP > consol.JK_JX_JA_E_DEP && !Parent.JS_E_DEPInfo.HasWarnings())
					{
						Parent.JS_E_DEPInfo.AddWarning(Res.GetString("81351575-20fd-40f5-9e61-39833c639f7a", "Shipment ETD cannot be after the Consol ETD"));
						break;
					}
				}

				ValidatingDEP = false;
			}
		}

		bool ValidatingDEP;
		bool ValidatingARV;

		#endregion

		#region Other DropEdits and Findboxes

		protected override void CheckJS_ShipperCODPayMethod()
		{
			base.CheckJS_ShipperCODPayMethod();
			if (Parent.JS_ShipperCODAmount != 0)
			{
				MandatoryValidation.CheckEntered(Parent.JS_ShipperCODPayMethodInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.JS_ShipperCODPayMethodInfo, Parent.Lookups.ShipperCODPaymentTypes);
		}

		protected override void CheckJS_TransportMode()
		{
			base.CheckJS_TransportMode();
			MandatoryValidation.CheckEntered(Parent.JS_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_TransportModeInfo, Parent.Lookups.JS_TransportMode_List);

			if (!Parent.JS_TransportModeInfo.HasNotifications() && Parent.Consols.Count > 0)
			{
				ValidateJS_TransportModeAgainstConsol();
			}

			CheckHasCriticalChangesOnProperty(Parent.JS_TransportModeInfo);
		}

		protected override void CheckJS_ShipmentType()
		{
			base.CheckJS_ShipmentType();
			MandatoryValidation.CheckEntered(Parent.JS_ShipmentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_ShipmentTypeInfo);

			if (Parent.ContainsDirectConsol() && !Parent.CanBeDirect())
			{
				Parent.JS_ShipmentTypeInfo.AddError(Res.GetString("f1d7629b-7df3-4efa-9e3e-90ef50485070", "This Shipment is attached to at least one Direct Consol and must be marked as STD, HVL or ASM"));
			}

			if (Parent.JS_ShipmentType == Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse
				&& Parent.DocAddresses.FindByDocAddressType(MasterFiles.Integration.DocAddressType.Manufacturer) == null)
			{
				Parent.JS_ShipmentTypeInfo.AddError(Res.GetString("3352184a-6046-4d25-9fc1-c8d00c1ba265", "The Goods Manufacturer Address is mandatory if 3PT shipment type is selected, go to Addresses tab to add it."));
			}

			CheckHasCriticalChangesOnProperty(Parent.JS_ShipmentTypeInfo);
		}

		protected override void CheckJS_PackingMode()
		{
			base.CheckJS_PackingMode();
			MandatoryValidation.CheckEntered(Parent.JS_PackingModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_PackingModeInfo, Parent.Lookups.JS_PackingMode_List);

			if (!Parent.JS_PackingModeInfo.HasNotifications() && Parent.Consols.Count > 0)
			{
				ValidateJS_PackingModeAgainstConsol();
			}

			CheckHasCriticalChangesOnProperty(Parent.JS_PackingModeInfo);
		}

		protected override void CheckJS_RX_NKGoodsValueCurr()
		{
			base.CheckJS_RX_NKGoodsValueCurr();
			MandatoryValidation.CheckUnitEntered(Parent.JS_RX_NKGoodsValueCurrInfo, Parent.JS_GoodsValueInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKGoodsValueCurrInfo);
		}

		protected override void CheckJS_INCO()
		{
			base.CheckJS_INCO();
			ListValidation.ErrorIfInvalidCode(Parent.JS_INCOInfo, Parent.Lookups.JS_INCO_List);
			IncotermValidation.Instance.WarningIfExpired(Parent.JS_INCOInfo);
		}

		protected override void CheckJS_ReleaseType()
		{
			base.CheckJS_ReleaseType();
			ListValidation.ErrorIfInvalidCode(Parent.JS_ReleaseTypeInfo, Parent.Lookups.JS_ReleaseType_List);
		}

		protected override void CheckJS_ShippedOnBoard()
		{
			base.CheckJS_ShippedOnBoard();
			ListValidation.ErrorIfInvalidCode(Parent.JS_ShippedOnBoardInfo, Parent.Lookups.JS_ShippedOnBoard_List);
		}

		protected override void CheckJS_NoOriginalBills()
		{
			base.CheckJS_NoOriginalBills();
			ValidateJS_NoCopyBills();
			CheckNoOriginalBillsCount();
		}

		protected virtual void CheckNoOriginalBillsCount()
		{
			if (!IsNonForwardingBooking)
			{
				if (Parent.JS_NoOriginalBills == 0 && Parent.JS_NoCopyBills == 0)
				{
					Parent.JS_NoOriginalBillsInfo.AddError(Res.GetString("5d7abd0f-9d93-43f6-833f-664889e70276", "Number of Originals and Copies cannot both be 0."));
				}

				if (Parent.JS_NoOriginalBills == 0 && (Parent.JS_ReleaseType == Constants.ShipmentReleaseTypes.OriginalReq || Parent.JS_ReleaseType == Constants.ShipmentReleaseTypes.OriginalReqSurrender))
				{
					Parent.JS_NoOriginalBillsInfo.AddWarning(Res.GetString("01219955-083c-4a4a-b88c-502e99d61f79", "Number of Originals must be 1 or more, when original bill is chosen in Release Type."));
				}
			}
		}

		protected override void CheckJS_NoCopyBills()
		{
			base.CheckJS_NoCopyBills();
			ValidateJS_NoOriginalBills();
			CheckNoCopyBillsCount();
		}

		protected virtual void CheckNoCopyBillsCount()
		{
			if (!IsNonForwardingBooking && Parent.JS_NoOriginalBills == 0 && Parent.JS_NoCopyBills == 0)
			{
				Parent.JS_NoCopyBillsInfo.AddError(Res.GetString("5d7abd0f-9d93-43f6-833f-664889e70276", "Number of Originals and Copies cannot both be 0."));
			}
		}

		protected override void CheckJS_OA_ExportReceivingDepot()
		{
			base.CheckJS_OA_ExportReceivingDepot();
			Parent.JS_OA_ExportReceivingDepotInfo.RunAdditionalValidation();
		}

		protected override void CheckJS_FreightSpotRateAutoratingMode()
		{
			base.CheckJS_FreightSpotRateAutoratingMode();
			MandatoryValidation.CheckEntered(Parent.JS_FreightSpotRateAutoratingModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_FreightSpotRateAutoratingModeInfo);
		}

		protected override void CheckJS_RX_NKFrtRateCurrency()
		{
			base.CheckJS_RX_NKFrtRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKFrtRateCurrencyInfo);
		}

		protected override void CheckJS_FreightCostRateAutoratingMode()
		{
			base.CheckJS_FreightCostRateAutoratingMode();

			MandatoryValidation.CheckEntered(Parent.JS_FreightCostRateAutoratingModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_FreightCostRateAutoratingModeInfo);
		}

		protected override void CheckJS_RX_NKFreightCostRateCurrency()
		{
			base.CheckJS_RX_NKFreightCostRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKFreightCostRateCurrencyInfo);
		}

		protected override void CheckJS_FreightGatewaySellRateAutoratingMode()
		{
			base.CheckJS_FreightGatewaySellRateAutoratingMode();
			MandatoryValidation.CheckEntered(Parent.JS_FreightGatewaySellRateAutoratingModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_FreightGatewaySellRateAutoratingModeInfo);
		}

		protected override void CheckJS_RX_NKGatewayFreightSellRateCurrency()
		{
			base.CheckJS_RX_NKGatewayFreightSellRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKGatewayFreightSellRateCurrencyInfo);
		}

		protected override void CheckJS_GatewayFreightSellRate()
		{
			base.CheckJS_GatewayFreightSellRate();

			if (Parent.JS_GatewayFreightSellRate != ZDecimal.Zero && Parent.Consols.Any() && !FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(Parent))
			{
				string gatewayAgentIsNotIdentifiedWarning = Res.GetString("e1be811c-0322-4f75-9270-de385913bfca",
					@"There is a Gateway Sell amount entered for this shipment, but no Sending Agent identified as Gateway Agent on related consols.
Please setup the Sending Agent as the Gateway Agent for the transhipment port in Maintain>Reference Files>Organizations>Forwarder>Gateway Agent");

				Parent.JS_GatewayFreightSellRateInfo.AddWarning(gatewayAgentIsNotIdentifiedWarning);
			}
		}

		protected bool IsNonForwardingBooking
		{
			get { return Parent.JS_IsBooking && !Parent.JS_IsForwardRegistered; }
		}

		void CheckHasCriticalChangesOnProperty(ZPropertyInfo info)
		{
			if (Parent.IsInDatabase && Parent.HasCriticalChangesOnProperty(info))
			{
				var message = Res.GetString("24b99a62-9762-4362-98a5-7bfdaef834b0",
					@"Another user has made critical changes on the {0} that prevent your changes from being saved.
Please close and reopen this form in order to continue. Your changes might be lost."
, info.HumanReadableName);

				info.AddError(message);
			}
		}

		#endregion

		#region ValidateJS_HouseBill

		protected override void CheckJS_HouseBill()
		{
			base.CheckJS_HouseBill();
			CheckDuplicateHouseBills();
			CheckHouseBillBasedOnHVLVShipmentType();
		}

		protected void CheckHouseBillBasedOnHVLVShipmentType()
		{
			if (Parent.IsInDatabase
				&& Parent.IsHighVolumeLowValueLegacy
				&& Parent.JS_HouseBill.IsEmpty)
			{
				Parent.JS_HouseBillInfo.AddError(Res.GetString("33027e1f-efbe-4ded-a310-7dba77cf1e50", "'{0}' shipment type should have House Bill Number.", Parent.JS_ShipmentType));
			}
		}

		protected virtual void CheckDuplicateHouseBills()
		{
			if (!Parent.JS_HouseBill.IsEmpty && !Parent.IsPendingAllocationSetBySystem)
			{
				var message = GetDuplicateHouseBillMessage();
				if (!message.IsEmpty)
				{
					if (EnforceUniqueHouseBills)
					{
						Parent.JS_HouseBillInfo.AddError(message);
					}
					else
					{
						Parent.JS_HouseBillInfo.AddWarning(message);
					}
				}
			}
		}

		ZString GetDuplicateHouseBillMessage()
		{
			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, Parent.JS_HouseBill);
			query.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			var additionalConditions = GetHouseBillDuplicateCheckAdditionalConditions();
			if (additionalConditions != null)
			{
				query.AddToFilter(additionalConditions);
			}

			query.MaximumRows = 3;

			var shipmentsWithSameHouseBill = Parent.Factory.Load<CommonShipment>(query);
			if (shipmentsWithSameHouseBill.Any())
			{
				var message = new ZStringBuilder(DuplicateHouseBillWarningMessage);

				foreach (var shipmentWithSameHouseBill in shipmentsWithSameHouseBill)
				{
					string shipmentID = shipmentWithSameHouseBill.JS_UniqueConsignRef.IsEmpty
						? Res.GetString("169ecd35-6191-4644-bf57-cd068e69f346", "New Shipment")
						: shipmentWithSameHouseBill.JS_UniqueConsignRef.ToString();
					message.AppendLine(shipmentID);
				}

				return message.ToString();
			}

			return ZString.Empty;
		}

		protected virtual bool EnforceUniqueHouseBills => false;

		protected virtual ZQuery GetHouseBillDuplicateCheckAdditionalConditions()
		{
			return null;
		}

		public bool HasDuplicateHouseBillWarning
		{
			get { return Parent.JS_HouseBillInfo.GetWarnings().ContainsNotificationContaining(DuplicateHouseBillWarningMessage); }
		}
		protected static string DuplicateHouseBillWarningMessage
		{
			get { return Res.GetString("9dfef8b5-bbe6-481e-b724-6fd45bb1e10f", "This House Bill number is already in use on:") + " \r\n"; }
		}

		#endregion

		#region JS_JS_ColoadMasterShipment

		protected override void CheckJS_JS_ColoadMasterShipment()
		{
			base.CheckJS_JS_ColoadMasterShipment();

			var master = Parent.CoLoadMasterShipment;
			if (master != null)
			{
				if (master.HasErrors)
				{
					AddErrorsFromMasterShipment(master);
				}

				if (Parent.ContainsDirectConsol() && !Parent.CanBeDirect())
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("e43cdcbe-baee-4865-bb1b-d0651022b7cc",
						"A Shipment attached to a Direct Consol can not have a Master shipment."));
				}
				else if (Parent.PK == master.PK)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("b718e8ab-e1e0-4bcd-9525-110e8f154709",
						"A Shipment cannot be its own Lead or Master."));
				}
				else if (Parent.IsBuyersConsolLead || Parent.IsShippersConsolLead)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("127aab3b-005b-4032-997f-2649c2eebed0",
						"Only Standard House, Co-Load Master and Assembly Master shipments can be sub-shipments of a Lead or Master shipment."));
				}
				else if ((master.IsCoLoadMaster || master.IsBlindCoLoadMaster) && Parent.IsLeadOrMaster)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("02261ab1-6121-43e2-918f-a540c894f369",
						"Only Standard House and High Volume Low Value shipments can be sub-shipments of a Co-Load Master shipment."));
				}
				else if (master.IsStandardHouse)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("bce9cdaf-678f-4a0a-95b2-b8410d362352",
						"A Standard House shipment cannot be a Lead or Master Shipment."));
				}
				else if ((!master.IsBuyersConsolLead && !master.IsShippersConsolLead) && master.Consols.OfType<CommonConsol>().Any((masterConsol => !Parent.Consols.Contains(masterConsol.PK) && masterConsol.Shipments.GetRelationshipBusinessObject(Parent) == null)))
				{
					CheckUnmatchedConsols(master);
				}
				else
				{
					CheckMasterIsNotCoLoad(master);
				}
			}
		}

		#region AddErrorsFromMasterShipment

		void AddErrorsFromMasterShipment(CommonShipment master)
		{
			var propertyInfosWithErrors = master.PropertiesWithNotifications.Where(info => info.Notifications.HasErrors()).ToArray();

			if (propertyInfosWithErrors.Any())
			{
				var userFriendlyErrorMessages = propertyInfosWithErrors
					.SelectMany(property => property.GetErrors().Select(errorMsg => property.HumanReadableName + " - " + errorMsg.Message))
					.Distinct();

				var masterErrors = new ZStringBuilder(userFriendlyErrorMessages).ToStringWithNewLineBetweenAppends();

				var error = Res.GetString("3c5f95d2-be0c-4822-9481-41b5896bb3b2",
					"This shipment cannot be saved while attached to Master/Lead {0} due to the following errors on the Master:\r\n{1}",
					master.HumanReadableName,
					masterErrors);

				Parent.JS_JS_ColoadMasterShipmentInfo.AddError(error);
			}
		}

		#endregion

		void CheckUnmatchedConsols(CommonShipment master)
		{
			foreach (CommonConsol consol in master.Consols.Except(Parent.Consols))
			{
				if (consol.IsDirect)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("bbdfa7ac-b74f-4016-bb47-0841275ae041",
						"Consol {0} cannot be attached to the Shipment {1} from its proposed Master/Lead {2} as the Consol is a Direct Consol and it already has a Direct Shipment {2}.",
						consol.JK_UniqueConsignRef,
						Parent.JS_UniqueConsignRef,
						master.JS_UniqueConsignRef));
				}
				else if (!Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed && consol.JK_ConsolCutOffDate.IsValid && consol.JK_ConsolCutOffDate < ZDateTime.UtcNow)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("df102294-cdea-4715-bead-e1e28233142d",
						"Consol {0} cannot be attached to the Shipment {1} from its proposed Master/Lead {2} as the Consol Cut Off Date has now passed.\r\nSupervisor access is required to attach the Consol at this time.\r\n{3}",
						consol.JK_UniqueConsignRef,
						Parent.JS_UniqueConsignRef,
						master.JS_UniqueConsignRef,
						Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate)));
				}
				else
				{
					var errorMessage = CheckConsolFromMasterShipment(master, consol);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						Parent.JS_JS_ColoadMasterShipmentInfo.AddError(errorMessage);
					}
					else
					{
						Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("84cc35aa-c8fe-458a-9259-145165b3d53d",
							"Co-Load Master is no longer valid. It may have been detached."));
					}
				}
			}
		}

		protected virtual string CheckConsolFromMasterShipment(CommonShipment master, CommonConsol consol)
		{
			return string.Empty;
		}

		/// <summary>
		/// Checks that CoLoad Master is not a CoLoad of this Shipment (circular reference)
		/// Also checks if the CoLoad Master is already a CoLoad of another Shipment
		/// </summary>
		void CheckMasterIsNotCoLoad(CommonShipment master)
		{
			if (ParticipatesInCircularCoLoadMasterReference())
			{
				Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("559b8a55-5eb1-4127-ae6f-45ab1ac560b3",
					"Co-Load Master cannot be a Co-Load of this Shipment."));
			}
			else if (!master.IsAssemblyMaster && master.CoLoadMasterShipment != null)
			{
				Parent.JS_JS_ColoadMasterShipmentInfo.AddWarning(Res.GetString("e2003f9d-d2b6-435a-b9e0-f47a7e5d3d4b",
					"Co-Load Master is already a Co-Load of another Shipment."));
			}
		}

		internal bool ParticipatesInCircularCoLoadMasterReference()
		{
			List<CommonShipment> encountered = new List<CommonShipment>();

			for (CommonShipment current = Parent; current != null; current = current.CoLoadMasterShipment)
			{
				if (encountered.Contains(current))
				{
					return current == Parent;
				}

				encountered.Add(current);
			}

			return false;
		}

		#endregion

		#region ValidateJS_ActualWeight

		protected override void CheckJS_ActualWeight()
		{
			base.CheckJS_ActualWeight();

			if ((Parent.IsCoLoadMaster || Parent.IsBlindCoLoadMaster) && Constants.Weight.ContainsCode(Parent.JS_UnitOfWeight))
			{
				ZDecimal subShipmentTotal = 0;

				foreach (CommonShipment shipment in Parent.CoLoadShipments)
				{
					if (Constants.Weight.ContainsCode(shipment.JS_UnitOfWeight))
					{
						subShipmentTotal += Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Parent.JS_UnitOfWeight);
					}
				}

				if (subShipmentTotal > Parent.JS_ActualWeight)
				{
					Parent.JS_ActualWeightInfo.AddWarning(
						 Res.GetString("08643f76-0df0-41be-9b36-496c8a11bf29", "Total weights are calculated from the master Shipment not sub-shipments. Yet this master has an entered weight less than the total of it's sub shipments."));
				}
			}

			if (Parent.JS_ActualWeight == 0)
			{
				var message = Res.GetString("a0662829-9537-4217-beea-63775ff204e0", "You have not entered a Shipment Weight.");
				Parent.JS_ActualWeightInfo.AddWarning(message);
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.JS_ActualWeightInfo);
			}

			if (ShouldCheckMeasuresAgainstPacklinesTotals &&
				Parent.JS_PackingMode != Constants.ContainerModes.FCL &&
				Parent.JS_PackingMode != Constants.ContainerModes.FTL &&
				!Parent.JS_ActualWeightInfo.HasNotifications() &&
				Parent.JS_ActualWeight != Parent.TotalOuterPacksWeight)
			{
				Parent.JS_ActualWeightInfo.AddWarning(Res.GetString("5691b99c-09d1-4704-9256-42bef24f8bf9", "Entered weight does not match total weight of the packlines."));
			}
		}

		#endregion

		#region ValidateJS_TransportMode
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string TransportModeDoesNotMatch = Res.GetString("7d03f424-0fea-4468-99e2-9f206f911ad3", "The transport mode does not match the transport mode for this consol.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		protected void ValidateJS_TransportModeAgainstConsol()
		{
			bool needWarning = false;

			if (Parent.Consols.Count > 0)
			{
				switch (Parent.Consols[0].JK_TransportMode)
				{
					case Constants.TransportModes.Air:
						needWarning = !IsValidTransportMode(Parent.JS_TransportMode, Constants.TransportModes.Air);
						break;
					case Constants.TransportModes.Sea:
						needWarning = !IsValidTransportMode(Parent.JS_TransportMode, Constants.TransportModes.Sea);
						break;
					default:
						needWarning = !Parent.Consols[0].JK_TransportMode.IsEmpty && Parent.Consols[0].JK_TransportMode != Parent.JS_TransportMode;
						break;
				}
				if (needWarning && !Parent.Consols[0].IsDirect)
				{
					Parent.JS_TransportModeInfo.AddWarning(TransportModeDoesNotMatch);
				}
			}
		}

		bool IsValidTransportMode(ZString shipmentMode, ZString consolMode)
		{
			if (shipmentMode == consolMode)
			{
				return true;
			}

			if (shipmentMode == Constants.TransportModes.AirSea)
			{
				return true;
			}

			if (shipmentMode == Constants.TransportModes.SeaAir)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region ValidateJS_PackingMode

		protected bool IsOnBCNConsol
		{
			get
			{
				bool result = false;

				foreach (CommonConsol consol in Parent.Consols)
				{
					if (consol.JK_ConsolMode == Constants.ContainerModes.BuyersConsol ||
						consol.JK_ConsolMode == Constants.ContainerModes.Other)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		protected bool IsOnSCNConsol
		{
			get
			{
				bool result = false;

				foreach (CommonConsol consol in Parent.Consols)
				{
					if (consol.JK_ConsolMode == Constants.ContainerModes.ShippersConsol ||
						consol.JK_ConsolMode == Constants.ContainerModes.Other)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string ContainerModeDoesNotMatch = Res.GetString("81bfd9f1-0f0c-45e9-98d7-a1c6529635a9", "The Container mode does not match the Container mode for this consol.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		protected void ValidateJS_PackingModeAgainstConsol()
		{
			if (Parent.Consols.Count > 0 && !Parent.Consols[0].IsDirect)
			{
				if (!Parent.Consols[0].JK_ConsolMode.IsEmpty && !Parent.JS_PackingMode.IsEmpty &&
					Parent.Consols[0].JK_ConsolMode != Constants.ContainerModes.Other &&
					Parent.JS_PackingMode != FreightUtilities.ShipmentContainerMode(Parent.Consols[0].JK_ConsolMode, Parent.Consols[0].JK_TransportMode))
				{
					Parent.JS_PackingModeInfo.AddWarning(ContainerModeDoesNotMatch);
				}
			}
		}

		#endregion

		#region ValidateJS_RS_NKServiceLevel

		protected override void CheckJS_RS_NKServiceLevel()
		{
			base.CheckJS_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RS_NKServiceLevelInfo, Parent.Lookups.RefServiceLevel_List);
		}

		#endregion

		#region ValidateJS_RS_NKGatewayServiceLevel

		protected override void CheckJS_RS_NKGatewayServiceLevel()
		{
			base.CheckJS_RS_NKGatewayServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RS_NKGatewayServiceLevelInfo, Parent.Lookups.GatewayServiceLevels);

			var needsToBeChecked =
				Parent.RequestPermissionByImpersonation == null || // means, it's not been called from GUI
				Parent.JS_RS_NKGatewayServiceLevel_InitValue != null && Parent.JS_RS_NKGatewayServiceLevel_InitValue != Parent.JS_RS_NKGatewayServiceLevel;

			if (!needsToBeChecked)
			{
				return;
			}

			foreach (CommonConsol consol in Parent.Consols)
			{
				var errorMessage = Parent.ExclusiveGatewayServiceChecker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(
					AttachDetachAction.Attach,
					Parent,
					consol);

				if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					Parent.JS_RS_NKGatewayServiceLevelInfo.AddError(errorMessage);
					break;
				}
			}
		}

		#endregion

		#region ValidateJS_RX_NKInsuranceCurrency

		protected override void CheckJS_RX_NKInsuranceCurrency()
		{
			base.CheckJS_RX_NKInsuranceCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKInsuranceCurrencyInfo);
		}

		#endregion

		#region ValidateJS_EFreightStatus

		protected override void CheckJS_EFreightStatus()
		{
			base.CheckJS_EFreightStatus();
			ListValidation.ErrorIfInvalidCode(Parent.JS_EFreightStatusInfo, Parent.Lookups.EFreightStatus_List);
		}

		#endregion

		#region ValidateJS_RL_NKPlaceOfReceipt

		protected override void CheckJS_RL_NKPlaceOfReceipt()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKPlaceOfReceiptInfo, Parent.Lookups.RefUNLOCO_List);
			base.CheckJS_RL_NKPlaceOfReceipt();
		}

		#endregion

		#region ValidateJS_RL_NKPlaceOfDischarge

		protected override void CheckJS_RL_NKPlaceOfDischarge()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKPlaceOfDischargeInfo, Parent.Lookups.RefUNLOCO_List);
			base.CheckJS_RL_NKPlaceOfDischarge();
		}

		#endregion

		#region ValidateJS_RL_NKLoadPort

		protected override void CheckJS_RL_NKLoadPort()
		{
			base.CheckJS_RL_NKLoadPort();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKLoadPortInfo);
		}

		#endregion

		#region ValidateJS_RL_NKDischargePort

		protected override void CheckJS_RL_NKDischargePort()
		{
			base.CheckJS_RL_NKDischargePort();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKDischargePortInfo);
		}

		#endregion

		#region ValidateJS_RL_NKFreightRateDestination

		protected override void CheckJS_RL_NKFreightRateDestination()
		{
			base.CheckJS_RL_NKFreightRateDestination();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKFreightRateDestinationInfo);
		}

		#endregion

		#region ValidateJS_RL_NKFreightRateOrigin

		protected override void CheckJS_RL_NKFreightRateOrigin()
		{
			base.CheckJS_RL_NKFreightRateOrigin();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RL_NKFreightRateOriginInfo);
		}

		#endregion

		#region ValidateJS_HBLAWBChargesDisplay

		protected override void CheckJS_HBLAWBChargesDisplay()
		{
			if (IsAgreedCharge
				&& Parent.IsImportTo(Core.Constants.CountryCodes.Brazil)
				&& Parent.JS_HBLAWBChargesDisplay != DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges)
			{
				Parent.JS_HBLAWBChargesDisplayInfo.AddError(Res.GetString("be7436f7-59b4-4400-9113-7d173584e061", "\"As Agreed\" option cannot be used for imports to Brazil"));
			}

			base.CheckJS_HBLAWBChargesDisplay();
			ListValidation.ErrorIfInvalidCode(Parent.JS_HBLAWBChargesDisplayInfo, Parent.ChargesApplyLookup);
		}

		protected virtual bool IsAgreedCharge
		{
			get
			{
				return Parent.JS_HBLAWBChargesDisplay == Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed
					 || Parent.JS_HBLAWBChargesDisplay == Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges
					 || Parent.JS_HBLAWBChargesDisplay == Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges
					 || Parent.JS_HBLAWBChargesDisplay == Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges;
			}
		}

		#endregion

		#region ValidateJS_HBLContainerPackModeOverride

		protected override void CheckJS_HBLContainerPackModeOverride()
		{
			base.CheckJS_HBLContainerPackModeOverride();
			if (Parent.JS_HBLContainerPackModeOverrideInfo.HasChanges || !Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_HBLContainerPackModeOverrideInfo, Parent.Lookups.JS_HBLContainerPackModeOverride_List);
			}
		}

		#endregion

		#region ValidateJS_ScreeningStatus

		protected override void CheckJS_ScreeningStatus()
		{
			base.CheckJS_ScreeningStatus();
			MandatoryValidation.CheckEntered(Parent.JS_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}

		#endregion

		#region Calculated

		public void ValidateIsDomesticFreight()
		{
			ValidateCalculatedProperty(Parent.IsDomesticFreightInfo);
		}

		protected virtual void CheckIsDomesticFreight()
		{
			if (Parent.IsDomesticFreight && !Parent.IsDomestic())
			{
				Parent.IsDomesticFreightInfo.AddError(Res.GetString("84a45ba7-f404-4c41-8b59-d0870dcd459e", "You have marked this Shipment as Domestic Freight, but the origin and destination are not in the same country/region."));
			}
		}

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		protected virtual void CheckLocationString()
		{
			if (!Parent.LocationString.IsEmpty)
			{
				var helper = ObjectFactory.New<IWhsLocationDataHelper>();
				helper.ValidateLocation(Parent, Parent.LocationStringInfo);
			}
		}

		public void ValidateConsignorPK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsValid)
			{
				var organization = validation.Parent.Factory.Load<OrgHeader>(validation.Parent.OrganisationPK);

				if ((Parent.IsCoLoadMaster || Parent.IsBlindCoLoadMaster) && organization != null && !organization.OH_IsForwarder)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidForwarderErrorMessage);
				}
				else if (!Parent.IsCoLoadMaster && !Parent.IsBlindCoLoadMaster && organization != null && !organization.OH_IsConsignor)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidConsignorErrorMessage);
				}
				if (Parent.ConsignorDocumentaryAddress.HasChanges)
				{
					if (ConsignorHasBeenChanged() || !Parent.IsInDatabase)
					{
						CheckRelatedParties(validation);
					}
				}
			}
		}

		public bool ConsignorHasBeenChanged()
		{
			return ConsignorConsigneeHasBeenChanged(Parent.ConsignorDocumentaryAddress.E2_OA_AddressInfo, Parent.ConsignorPK);
		}

		public void ValidateConsigneePK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsValid)
			{
				var organization = validation.Parent.Factory.Load<OrgHeader>(validation.Parent.OrganisationPK);

				if ((Parent.IsCoLoadMaster || Parent.IsBlindCoLoadMaster) && organization != null && !organization.OH_IsForwarder)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidForwarderErrorMessage);
				}
				else if (!Parent.IsCoLoadMaster && !Parent.IsBlindCoLoadMaster && organization != null && !organization.OH_IsConsignee)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidConsigneeErrorMessage);
				}
				if (Parent.ConsigneeDocumentaryAddress.HasChanges)
				{
					if (ConsigneeHasBeenChanged() || !Parent.IsInDatabase)
					{
						CheckRelatedParties(validation);
					}
				}
			}
		}

		public void ValidateControllingCustomerPK(JobDocAddressValidation validation)
		{
			Argument.NotNull(validation, nameof(validation));
			Argument.NotNull(validation.Parent, nameof(validation.Parent));

			var docAddress = validation.Parent;

			if (docAddress.HasChanges
				&& OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& docAddress.Organisation != null
				&& !docAddress.Organisation.OH_IsControllingCustomer)
			{
				if (docAddress.E2_AddressOverride)
				{
					docAddress.OrganisationPKInfo.AddError(Res.GetString("4B603BE0-F10C-428E-B89C-DD829D63D79A", "It is not possible to override a Controlling Customer. Amend the details on the linked organization."));
				}
				else
				{
					docAddress.OrganisationPKInfo.AddError(Res.GetString("ed9466f8-9c1e-4117-84a8-54f7968f4e83", "This organization is not a valid Controlling Customer."));
				}
			}
		}

		public void ValidateControllingAgentPK(JobDocAddressValidation validation)
		{
			Argument.NotNull(validation, nameof(validation));
			Argument.NotNull(validation.Parent, nameof(validation.Parent));

			var docAddress = validation.Parent;

			if (docAddress.HasChanges
				&& OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value
				&& docAddress.Organisation != null
				&& !docAddress.Organisation.OH_IsControllingAgent)
			{
				if (docAddress.E2_AddressOverride)
				{
					docAddress.OrganisationPKInfo.AddError(Res.GetString("1C2B0896-053C-45F6-824D-848679F5C12F", "It is not possible to override a Controlling Agent. Amend the details on the linked organization."));
				}
				else
				{
					docAddress.OrganisationPKInfo.AddError(Res.GetString("5E732862-20C2-406B-8788-F066DBAE64B1", "This organization is not a valid Controlling Agent."));
				}
			}
		}

		public bool ConsigneeHasBeenChanged()
		{
			return ConsignorConsigneeHasBeenChanged(Parent.ConsigneeDocumentaryAddress.E2_OA_AddressInfo, Parent.ConsigneePK);
		}

		bool ConsignorConsigneeHasBeenChanged(ZPropertyInfo addressInfo, ZGuid orgPK)
		{
			if (!Parent.IsInDatabase && !orgPK.IsEmpty)
			{
				return true;
			}

			var originalAddressPK = (ZGuid)addressInfo.OriginalValue;
			var originalAddress = Parent.Factory.Load<OrgAddress>(originalAddressPK);
			var originalOrgHeader = originalAddress != null ? originalAddress.OA_OH : ZGuid.Empty;
			return originalOrgHeader != orgPK;
		}

		void CheckRelatedParties(JobDocAddressValidation validation)
		{
			string consolRelatedSendingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(new[] { Parent }, Parent.Consols.Cast<CommonConsol>());
			string consolRelatedReceivingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(new[] { Parent }, Parent.Consols.Cast<CommonConsol>());
			if (!string.IsNullOrEmpty(consolRelatedSendingAgentsWarning))
			{
				validation.Parent.OrganisationPKInfo.AddWarning(consolRelatedSendingAgentsWarning);
			}
			if (!string.IsNullOrEmpty(consolRelatedReceivingAgentsWarning))
			{
				validation.Parent.OrganisationPKInfo.AddWarning(consolRelatedReceivingAgentsWarning);
			}
		}

		string IsNotValidForwarderErrorMessage
		{
			get { return Res.GetString("e487c905-31ad-45da-92c9-cc59c4424a4b", "This organization is not a valid Forwarder"); }
		}

		string IsNotValidConsigneeErrorMessage
		{
			get { return Res.GetString("df6e0299-c451-42ce-8465-b24e4581e29c", "This organization is not a valid Consignee"); }
		}

		string IsNotValidConsignorErrorMessage
		{
			get { return Res.GetString("7cccd0e7-a4ef-4a1c-a25a-ef199537662d", "This organization is not a valid Consignor"); }
		}

		#region ValidateJS_MarksAndNumbers

		public void ValidateJS_MarksAndNumbers()
		{
			ValidateCalculatedProperty(Parent.JS_MarksAndNumbersInfo);
		}

		protected virtual void CheckJS_MarksAndNumbers()
		{
			if (Parent.MarksAndNumbersNote != null)
			{
				Parent.MarksAndNumbersNote.Validation.ValidateAll();
			}
		}

		#endregion

		#region Inner PackLines Totals Validation

		public void ValidateTotalInnerPackLinePackages()
		{
			ValidateCalculatedProperty(Parent.TotalInnerPackLinePackagesInfo);
		}

		protected virtual void CheckTotalInnerPackLinePackages()
		{
			if (Parent.JS_TotalPackageCount != 0 && Parent.InnerPackLines.Count > 0 && Parent.TotalInnerPackLinePackages != Parent.JS_TotalPackageCount)
			{
				Parent.TotalInnerPackLinePackagesInfo.AddWarning(Res.GetString("07b06899-4cf1-4354-9083-84274fd1f4e8", "Totals do not match"));
			}
		}

		public void ValidateTotalInnerPackLineLoadingMeters()
		{
			ValidateCalculatedProperty(Parent.TotalInnerPackLineLoadingMetersInfo);
		}

		protected virtual void CheckTotalInnerPackLineLoadingMeters()
		{
			if (Parent.IsRoadLoadingMetersEnabled && Parent.JS_LoadingMeters != 0 && Parent.InnerPackLines.Count > 0
				&& Utilities.Round(Parent.TotalInnerPackLineLoadingMeters, 3) != Utilities.Round(Parent.JS_LoadingMeters, 3))
			{
				Parent.TotalInnerPackLineLoadingMetersInfo.AddWarning(Res.GetString("121e79b9-b4bb-4edc-ba46-8f41e0ab667e", "Totals do not match"));
			}
		}

		#endregion

		#region Outer PackLines Totals Validation

		public void ValidateTotalOuterPacks()
		{
			ValidateCalculatedProperty(Parent.TotalOuterPacksInfo);
		}

		protected virtual void CheckTotalOuterPacks()
		{
			if (Parent.JS_OuterPacks != 0 && Parent.OuterPackLines.Count > 0 && Parent.TotalOuterPacks != Parent.JS_OuterPacks)
			{
				Parent.TotalOuterPacksInfo.AddWarning(Res.GetString("a8c65981-6dcd-4437-9632-475e3b644fbb", "Totals do not match"));
			}
		}

		public void ValidateTotalOuterPacksWeight()
		{
			ValidateCalculatedProperty(Parent.TotalOuterPacksWeightInfo);
		}

		protected virtual void CheckTotalOuterPacksWeight()
		{
			if (Parent.JS_ActualWeight != 0 && Parent.OuterPackLines.Count > 0 && Utilities.Round(Parent.TotalOuterPacksWeight, 3) != Utilities.Round(Parent.JS_ActualWeightReadOnly, 3))
			{
				Parent.TotalOuterPacksWeightInfo.AddWarning(Res.GetString("03388059-9922-4156-85c1-d6a00156ee30", "Totals do not match"));
			}
		}

		public void ValidateTotalOuterPacksVolume()
		{
			ValidateCalculatedProperty(Parent.TotalOuterPacksVolumeInfo);
		}

		protected virtual void CheckTotalOuterPacksVolume()
		{
			if (Parent.JS_ActualVolume != 0 && Parent.OuterPackLines.Count > 0 && Utilities.Round(Parent.TotalOuterPacksVolume, 3) != Utilities.Round(Parent.JS_ActualVolumeReadOnly, 3))
			{
				Parent.TotalOuterPacksVolumeInfo.AddWarning(Res.GetString("e78ea358-e0b1-4d05-9895-c80e01977c4e", "Totals do not match"));
			}
		}

		public void ValidateTotalOuterPacksLoadingMeters()
		{
			ValidateCalculatedProperty(Parent.TotalOuterPacksLoadingMetersInfo);
		}

		protected virtual void CheckTotalOuterPacksLoadingMeters()
		{
			if (Parent.IsRoadLoadingMetersEnabled && Parent.JS_LoadingMeters != 0 && Parent.OuterPackLines.Count > 0
				&& Utilities.Round(Parent.TotalOuterPacksLoadingMeters, 3) != Utilities.Round(Parent.JS_LoadingMeters, 3))
			{
				Parent.TotalOuterPacksLoadingMetersInfo.AddWarning(Res.GetString("7a504996-3706-4153-b633-78074c95161f", "Totals do not match"));
			}
		}

		#endregion

		protected override void CheckJS_CommunityTransitStatus()
		{
			var originCountry = Parent?.Origin?.Country;
			var destinationCountry = Parent?.Destination?.Country;
			var isOriginNorthernIreland = Parent?.Origin?.IsInNorthernIreland ?? false;
			var isDestinationNorthernIreland = Parent?.Destination?.IsInNorthernIreland ?? false;

			var brexitDate = new ZDateTime(2021, 1, 1);
			var shipmentSystemCreateDateTime = Parent.JS_SystemCreateTimeUtc.IsEmpty
								? ZDateTime.UtcNow
								: Parent.JS_SystemCreateTimeUtc;

			bool isEUOrigin = originCountry != null
							&& (originCountry.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion
								|| originCountry.Code == Constants.CountryCodes.UnitedKingdom && shipmentSystemCreateDateTime < brexitDate
								|| isOriginNorthernIreland);

			bool isEUDestination = destinationCountry != null
								&& (destinationCountry.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion
									|| destinationCountry.Code == Constants.CountryCodes.UnitedKingdom && shipmentSystemCreateDateTime < brexitDate
									|| isDestinationNorthernIreland);

			if (Parent.JS_CommunityTransitStatus == Customs.Common.EU.ExportCommunityTransitStatusList.Codes.C)
			{
				if (originCountry != null && destinationCountry != null && (!isEUOrigin || !isEUDestination))
				{
					var intraTransitErrorMessage = Res.GetString("25c45f9c-c6b8-4645-8431-148996f49b67", "C-status is only for intra-EU movements");
					var isGenevaOrEUOrigin = IsGeneva(Parent.Origin) || isEUOrigin;
					var isGenevaOrEUDest = IsGeneva(Parent.Destination) || isEUDestination;

					if (isGenevaOrEUDest && isGenevaOrEUOrigin)
					{
						Parent.JS_CommunityTransitStatusInfo.AddWarning(intraTransitErrorMessage);
					}
					else
					{
						Parent.JS_CommunityTransitStatusInfo.AddError(intraTransitErrorMessage);
					}
				}
			}
			else if (Parent.JS_CommunityTransitStatus == Customs.Common.EU.ExportCommunityTransitStatusList.Codes.X)
			{
				var isEUOrUKOrigin = isEUOrigin || (originCountry?.Code ?? ZString.Empty) == Constants.CountryCodes.UnitedKingdom;

				if (originCountry != null && destinationCountry != null
					&& (originCountry == destinationCountry
						|| isEUOrigin && isEUDestination
						|| !isEUOrUKOrigin))
				{
					Parent.JS_CommunityTransitStatusInfo.AddError(Res.GetString("F20EF193-2479-425B-BBF5-E2987D5F8A36", "X-status is only for exports out of the customs territory"));
				}
			}

			if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsCountryEuOrCtCountry(GlbCompany.CurrentCompany.Country.Code))  // same as visibility of the field on the GUI
			{
				ListValidation.WarnIfInvalidCode(Parent.JS_CommunityTransitStatusInfo, Enterprise.Customs.Common.CusEntryNumberTypes.EU.CommunityTransitStatusCodesList);
			}
		}

		bool IsGeneva(RefUNLOCO port)
		{
			return port.Code.EqualsIgnoringCase("CHGVA");
		}

		#endregion

		#region JS_InspectionTypeCode

		public void ValidateJS_InspectionTypeCode()
		{
			ValidateCalculatedProperty(Parent.JS_InspectionTypeCodeInfo);
		}

		protected virtual void CheckJS_InspectionTypeCode()
		{
		}

		#endregion

		#region JS_AdditionalInspectionTypeCode

		public void ValidateJS_AdditionalInspectionTypeCode()
		{
			ValidateCalculatedProperty(Parent.JS_AdditionalInspectionTypeCodeInfo);
		}

		protected virtual void CheckJS_AdditionalInspectionTypeCode()
		{
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateTotalInnerPackLinePackages();
			ValidateTotalInnerPackLineLoadingMeters();

			ValidateTotalOuterPacks();
			ValidateTotalOuterPacksWeight();
			ValidateTotalOuterPacksVolume();
			ValidateTotalOuterPacksLoadingMeters();

			ValidateJS_CommunityTransitStatus();
			Parent.Consols.CheckUniqueOriginAndDestination();
		}

		#endregion

		#region Implementation

		public new CommonShipment Parent
		{
			get { return (CommonShipment)base.Parent; }
		}

		#endregion
	}
}

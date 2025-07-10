using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ContractManagement.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingValidation : ZValidation
	{
		public QuotedBookingValidation(QuotedBooking parent)
			: base(parent)
		{
			this.Parent = parent;
			this.ParentListInternals = parent;
		}

		#region Overrides

		public override Type AutoValidationType
		{
			get { return typeof(QuotedBookingValidation); }
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateIsForwardRegistered();
				ValidateChargeable();
				ValidateCommodity();
				ValidateDeliveryEquipment();
				ValidateDestination();
				ValidateETD();
				ValidateETA();
				ValidateGoodsCurrency();
				ValidateGoodsValue();
				ValidateInsuranceCurrency();
				ValidateInsuranceValue();
				ValidateIsDomesticFreight();
				ValidateMode();
				ValidateTransportMode();
				ValidateContainerMode();
				ValidateShipmentStatus();
				ValidateOH_Carrier();
				ValidateCreditor();
				ValidateCarrierServiceLevel();
				ValidateExportReceivingDepot();
				ValidateClientAddrPK();
				ValidateOrigin();
				ValidatePaymentTerms();
				ValidatePickupEquipment();
				ValidateServiceLevel();
				ValidateVia();
				ValidateVolume();
				ValidateVolumeUnit();
				ValidateWeight();
				ValidateWeightUnit();
				ValidateTransitTime();
				ValidateFrequencyUnit();
				ValidateFrequency();
				ValidateCompanyTariffLevel();
				ValidateLoadPort();
				ValidateDischargePort();
				ValidateStartDate();
				ValidateEndDate();
				ValidateOneOffQuoteEntriesAndLines();
				ValidateDeliveryDueDate();
				ValidateCarrierContractNumber();
				ValidateAllocationLinePK();
			}
		}

		#endregion

		#region Validate Properties

		#region CarrierContractNumber

		public void ValidateCarrierContractNumber()
		{
			ValidateCalculatedProperty(Parent.CarrierContractNumberInfo);
		}

		protected virtual void CheckCarrierContractNumber()
		{
			if (Parent.IsValidationSuspended
				|| Parent.CarrierContractNumber.IsEmpty)
			{
				return;
			}

			var validationData = Parent as ICCACommonAssignmentValidationData;

			if (!CCAContractValidationHelper.DoesContractNumberExistWithAnyCarrier(Parent.CarrierContractNumber, Parent.Factory))
			{
				Parent.CarrierContractNumberInfo.AddWarning(CCAValidationMessageProvider.MissingContractRecord());
			}
			else if (!CCAContractValidationHelper.DoesContractNumberExistWithCarrier(Parent.CarrierContractNumber, Parent.Carrier, Parent.Factory))
			{
				var message = validationData.ContractServiceProvider?.PK.IsEmpty ?? true
					? CCAValidationMessageProvider.MissingCarrierOnJob(validationData)
					: CCAValidationMessageProvider.CarrierContractMismatch(validationData);

				var anyContainersAssignedToAllocationRoutes = validationData
					.Containers
					.Any(container => !container.JC_RCA_AllocationLine.IsEmpty);

				if (anyContainersAssignedToAllocationRoutes || validationData.AllocationRoute != null)
				{
					Parent.CarrierContractNumberInfo.AddError(message);
				}
				else
				{
					Parent.CarrierContractNumberInfo.AddWarning(message);
				}
			}

			if (validationData.CarrierContract is not IRatingContract contract)
			{
				return;
			}

			if (CCAContractValidationHelper.IsETDBeforeContractStart(contract, validationData))
			{
				Parent.CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.ContractStartDateViolated(contract, validationData));
			}
			else if (CCAContractValidationHelper.IsETDAfterContractExpiry(contract, validationData))
			{
				Parent.CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.ContractExpiryDateViolated(contract, validationData));
			}

			if (CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, validationData))
			{
				Parent.CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.ContainerTypesInvalid(contract, validationData));
			}

			if (CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, validationData))
			{
				Parent.CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.HazardousContainerCommoditiesInvalid(contract, validationData));
			}

			if (Parent.CarrierContractNumber.Length > AutoCusEntryNum.Schema.CE_EntryNumMaxLength)
			{
				Parent.CarrierContractNumberInfo.AddWarning(Res.GetString("468eff70-c8a9-9682-4b2f-5d67e71f24ae",
					"Additional Reference Numbers do not support values over 35 characters."));
			}

			if (Parent.TransportMode != contract.RCT_TransportMode)
			{
				Parent.CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.InvalidBookingTransportMode(contract, Parent));
			}

			if (!CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, Parent))
			{
				Parent.CarrierContractNumberInfo.AddWarning(CCAValidationMessageProvider.InvalidBookingForContractNamedAccounts(contract));
			}
		}

		#endregion

		#region AllocationLine

		public void ValidateAllocationLinePK()
		{
			ValidateCalculatedProperty(Parent.AllocationLinePKInfo);
		}

		protected virtual void CheckAllocationLinePK()
		{
			if (Parent.IsValidationSuspended || Parent.AllocationLinePK.IsEmpty)
			{
				return;
			}

			if (CCARouteValidationHelper.IsRouteAssignmentMissingCarrierContract(Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.MissingContractWhenAllocatedToRoute(Parent));
				return;
			}

			if (CCARouteValidationHelper.IsInvalidAllocationRouteAssigned(Parent.AllocationLinePK, Parent.Factory))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidAllocationRoute(Parent));
				return;
			}

			if (!CCARouteValidationHelper.IsRouteAssignedUnderParentContractAssigned(Parent.AllocationLinePK, (Parent as ICCACommonAssignmentValidationData).CarrierContract))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.AllocationRouteAndContractMismatch(Parent));
				return;
			}

			var validationData = Parent as ICCACommonAssignmentValidationData;
			if ((validationData.CarrierContract == null
				|| validationData.AllocationRoute is not RatingContractAllocationLine allocationRoute))
			{
				return;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				ValidateBookingForUnlinkedAllocationRoute(allocationRoute);
			}
			else if (allocationRoute.RCA_JX_SailingSchedule != Parent.Booking?.JS_JX)
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidBookingForLinkedAllocationRoute(allocationRoute));
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(Parent.Factory, allocationRoute);
				if (outstandingUtilization < 0)
				{
					if (allocationRoute.RCA_AllocatedUQ.EqualsIgnoringCase(Core.Constants.AllocationQuantityUnits.Containers))
					{
						Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.ContainerBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
					else
					{
						Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.TEUBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
				}
			}

			var namedAccounts = allocationRoute?.NamedAccountPivots?.GetAllNamedAccounts();
			if (CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddWarning(CCAValidationMessageProvider.InvalidBookingForAllocationRouteNamedAccounts(allocationRoute));
			}

			if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidContainerCodeInCollection(allocationRoute, Parent));
			}
			else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidContainerClassInCollection(allocationRoute, Parent));
			}

			if(FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				if (!allocationRoute.RCA_PlaceOfReceipt.IsEmpty && !CCARouteValidationHelper.IsBookingLordPortValidForPlaceOfReceipt(allocationRoute, Parent))
				{
					Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidPlaceOfReceiptLoadPortForBooking(allocationRoute, Parent));
				}

				if (!allocationRoute.RCA_PlaceOfDelivery.IsEmpty && !CCARouteValidationHelper.IsBookingDischargePortValidForPlaceOfDelievery(allocationRoute, Parent))
				{
					Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidPlaceOfDeliveryDestinationForBooking(allocationRoute, Parent));
				}
			}
		}
			
		void ValidateBookingForUnlinkedAllocationRoute(RatingContractAllocationLine allocationRoute)
		{
			if (CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.AllocationRouteStartDateViolated(allocationRoute, Parent));
			}
			else if (CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.AllocationRouteExpiryDateViolated(allocationRoute, Parent));
			}

			if (CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidLoadPort(allocationRoute, Parent));
			}

			if (CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidDischargePort(allocationRoute, Parent));
			}

			if (CCARouteValidationHelper.IsVoyageNumberMismatch(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidVoyageNumber(allocationRoute, Parent));
			}

			if (CCARouteValidationHelper.IsVesselMismatch(allocationRoute, Parent))
			{
				Parent.AllocationLinePKInfo.AddError(CCAValidationMessageProvider.InvalidVessel(allocationRoute, Parent));
			}
		}

		#endregion

		#region IsForwardRegistered

		public void ValidateIsForwardRegistered()
		{
			ValidateCalculatedProperty(Parent.IsForwardRegisteredInfo);
		}

		protected virtual void CheckIsForwardRegistered()
		{
			if (Parent.IsForwardRegistered && Parent.Quote == null)
			{
				Parent.IsForwardRegisteredInfo.AddError(Res.GetString("8b54dbe2-c471-4899-a46d-3513282c97e0", "The {0} has been Consolidated and can no longer be edited.", Parent.HumanReadableNameWithoutID));
			}
		}

		#endregion

		#region QuoteEntry

		#region Frequency

		public void ValidateFrequency()
		{
			ValidateCalculatedProperty(Parent.FrequencyInfo);
		}

		protected virtual void CheckFrequency()
		{
			if (Parent.Quote != null)
			{
				if (!Parent.FrequencyUnitInfo.Value.IsEmpty && Parent.FrequencyInfo.Value.IsEmpty)
				{
					Parent.FrequencyInfo.AddError(ErrorMessages.NoFrequency);
				}
			}
		}

		#endregion

		#region FrequencyUnit

		public void ValidateFrequencyUnit()
		{
			ValidateCalculatedProperty(Parent.FrequencyUnitInfo);
		}

		protected virtual void CheckFrequencyUnit()
		{
			if (Parent.Quote != null && Parent.FrequencyUnits != null)
			{
				if (!Parent.FrequencyUnit.IsEmpty && !Parent.FrequencyUnits.ContainsCode(Parent.FrequencyUnitInfo.Value))
				{
					Parent.FrequencyUnitInfo.AddError(Res.GetString("582930fa-f194-4713-8d2c-4b47d26b6885", "Please enter a frequency unit value from the available list."));
				}
			}
		}

		#endregion

		#region TransitTime

		public void ValidateTransitTime()
		{
			ValidateCalculatedProperty(Parent.TransitTimeInfo);
		}

		protected virtual void CheckTransitTime()
		{
			if (Parent.Quote != null && Parent.TransitTimesList != null)
			{
				if (!Parent.TransitTime.IsEmpty && !Parent.TransitTimesList.ContainsCode(Parent.TransitTimeInfo.Value))
				{
					Parent.TransitTimeInfo.AddError(ErrorMessages.InvalidTransitTime);
				}
			}
		}

		#endregion

		#endregion

		#region LoadPort

		public void ValidateLoadPort()
		{
			ValidateCalculatedProperty(Parent.LoadPortInfo);
		}

		protected virtual void CheckLoadPort()
		{
			if (!Parent.LoadPort.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.LoadPortInfo, Parent.LoadPortLocations);

				if (Parent.LoadPort == Parent.DischargePort)
				{
					Parent.LoadPortInfo.AddError(Res.GetString("24049555-df6c-0db0-4495-c4539837b3b5", "Load and Discharge Ports cannot be the same."));
				}

				if (!Parent.LoadPortInfo.HasErrors() && Parent.LoadPortUNLOCO != null)
				{
					if (Parent.TransportMode == Core.Constants.TransportModes.Sea && !Parent.LoadPortUNLOCO.RL_HasSeaport)
					{
						Parent.LoadPortInfo.AddWarning(Res.GetString("28476a57-27fd-4ba1-4392-78e0530207c1", "{0} does not have a sea port.", Parent.LoadPort));
					}
					else if (Parent.TransportMode == Core.Constants.TransportModes.Air && !Parent.LoadPortUNLOCO.RL_HasAirport)
					{
						Parent.LoadPortInfo.AddWarning(Res.GetString("b8d12390-4cdf-c287-4d03-8efd695129dd", "{0} does not have an air port.", Parent.LoadPort));
					}
				}
			}
		}

		#endregion

		#region DischargePort

		public void ValidateDischargePort()
		{
			ValidateCalculatedProperty(Parent.DischargePortInfo);
		}

		protected virtual void CheckDischargePort()
		{
			if (!Parent.DischargePort.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DischargePortInfo, Parent.DischargePortLocations);

				if (Parent.DischargePort == Parent.LoadPort)
				{
					Parent.DischargePortInfo.AddError(Res.GetString("e501503a-6181-1fb1-497a-3247d2d9b7d2", "Load and Discharge Ports cannot be the same."));
				}

				if (!Parent.DischargePortInfo.HasErrors() && Parent.DischargePortUNLOCO != null)
				{
					if (Parent.TransportMode == Core.Constants.TransportModes.Sea && !Parent.DischargePortUNLOCO.RL_HasSeaport)
					{
						Parent.DischargePortInfo.AddWarning(Res.GetString("8f34cca3-e76a-ef88-4f31-808c74cd2bc2", "{0} does not have a sea port.", Parent.DischargePort));
					}
					else if (Parent.TransportMode == Core.Constants.TransportModes.Air && !Parent.DischargePortUNLOCO.RL_HasAirport)
					{
						Parent.DischargePortInfo.AddWarning(Res.GetString("f6accd14-d175-119c-4d84-ed1a98d37f64", "{0} does not have an air port.", Parent.DischargePort));
					}
				}
			}
		}

		#endregion

		#region ValidateEndDate

		public void ValidateEndDate()
		{
			ValidateCalculatedProperty(Parent.EndDateInfo);
		}

		protected virtual void CheckEndDate()
		{
			if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.EndDateInfo, Parent.Quote.TH_QuoteEndDateInfo);
			}
		}

		#endregion

		#region ValidateStartDate

		public void ValidateStartDate()
		{
			ValidateCalculatedProperty(Parent.StartDateInfo);
		}

		protected virtual void CheckStartDate()
		{
			if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.StartDateInfo, Parent.Quote.TH_QuoteDateInfo);
			}
		}

		#endregion

		#region ClientPK

		public void ValidateClientAddrPK()
		{
			ValidateCalculatedProperty(Parent.ClientAddrPKInfo);
		}

		protected virtual void CheckClientAddrPK()
		{
			if (Parent.ValidateBookingProperty && Parent.Booking.ShipmentJobHeader != null)
			{
				ProxyValidation(Parent.ClientAddrPKInfo, Parent.Booking.ShipmentJobHeader.JH_OA_LocalChargesAddrInfo);
			}
		}

		public void ValidateClientPK()
		{
			ValidateCalculatedProperty(Parent.ClientPKInfo);
		}

		protected virtual void CheckClientPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.ClientPKInfo);
		}

		#endregion

		#region OH_Carrier

		public void ValidateOH_Carrier()
		{
			ValidateCalculatedProperty(Parent.OH_CarrierInfo);
		}

		protected virtual void CheckOH_Carrier()
		{
			ListValidation.ErrorIfInvalidPK(Parent.OH_CarrierInfo);

			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.OH_CarrierInfo, Parent.Booking.JS_OA_BookedShippingLineAddressInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.OH_CarrierInfo, Parent.Quote.CurrentOneOffQuote.TT_OH_CarrierInfo);
			}
		}

		#endregion

		#region CarrierServiceLevel

		public void ValidateCarrierServiceLevel()
		{
			ValidateCalculatedProperty(Parent.CarrierServiceLevelInfo);
		}

		protected virtual void CheckCarrierServiceLevel()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.CarrierServiceLevelInfo, Parent.Booking.JS_PL_NKCarrierServiceLevelInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.CarrierServiceLevelInfo, Parent.Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevelInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.CarrierServiceLevelInfo, Parent.CarrierServiceLevels);
		}

		#endregion

		#region Creditor

		public void ValidateCreditor()
		{
			ValidateCalculatedProperty(Parent.CreditorInfo);
		}

		protected virtual void CheckCreditor()
		{
			ListValidation.ErrorIfInvalidPK(Parent.CreditorInfo);

			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.CreditorInfo, Parent.Booking.JS_OA_BookedShippingLineAddressInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.CreditorInfo, Parent.Quote.CurrentOneOffQuote.TT_OH_CreditorInfo);
			}
		}

		#endregion

		#region ExportReceivingDepot

		public void ValidateExportReceivingDepot()
		{
			ValidateCalculatedProperty(Parent.ExportReceivingDepotInfo);
		}

		protected virtual void CheckExportReceivingDepot()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ExportReceivingDepotInfo, Parent.Booking.JS_OA_ExportReceivingDepotInfo);

				var receivingDepotAddressPK = Parent.ExportReceivingDepot;
				if (receivingDepotAddressPK.IsValid)
				{
					var receivingDepotAddress = Parent.Factory.Load<OrgAddress>(receivingDepotAddressPK);
					var receivingDepot = receivingDepotAddress?.Header;
					if (receivingDepot != null && !receivingDepot.MatchesFilter(Parent.Receiver_List.CompleteFilter))
					{
						Parent.ExportReceivingDepotInfo.AddError(Res.GetString("1308961d-9a87-4ee8-ae65-f333be35a345", "Please enter a valid organization."));
					}
				}
			}
		}

		#endregion

		#region ImportReleaseDepot

		public void ValidateImportReleaseDepot()
		{
			ValidateCalculatedProperty(Parent.ImportReleaseDepotInfo);
		}

		protected virtual void CheckImportReleaseDepot()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ImportReleaseDepotInfo, Parent.Booking.JS_OA_ImportReleaseDepotInfo);

				var releaseDepotAddressPK = Parent.ImportReleaseDepot;
				if (releaseDepotAddressPK.IsValid)
				{
					var releaseDepotAddress = Parent.Factory.Load<OrgAddress>(releaseDepotAddressPK);
					var releaseDepot = releaseDepotAddress?.Header;
					if (releaseDepot != null && !releaseDepot.MatchesFilter(Parent.Delivery_List.CompleteFilter))
					{
						Parent.ImportReleaseDepotInfo.AddError(Res.GetString("091cd77d-e856-4d53-bfa4-8a15cd49bcc2", "Please enter a valid organization."));
					}
				}
			}
		}

		#endregion

		#region CompanyTariffLevel

		public void ValidateCompanyTariffLevel()
		{
			if (!Parent.CompanyTariffLevelInfo.ReadOnly) // We don't need to validate CompanyTariffLevel when it is disabled.
			{
				ValidateCalculatedProperty(Parent.CompanyTariffLevelInfo);
			}
		}

		protected virtual void CheckCompanyTariffLevel()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CompanyTariffLevelInfo, Parent.CompanyTariffLevelOverrideList);
		}

		#endregion

		#region Mode

		public void ValidateMode()
		{
			ValidateCalculatedProperty(Parent.ModeInfo);
		}

		protected virtual void CheckMode()
		{
			if (!(Parent.IsCompareMode && Parent.SelectedComparisonModes.Count > 0))
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.Modes);
		}

		#region TransportMode

		public void ValidateTransportMode()
		{
			ValidateCalculatedProperty(Parent.TransportModeInfo);
		}

		protected virtual void CheckTransportMode()
		{
			if (Parent.ValidateQuoteProperty)
			{
				if (!(Parent.IsCompareMode && Parent.SelectedComparisonModes.Count > 0))
				{
					MandatoryValidation.CheckEntered(Parent.TransportModeInfo);
				}
				ListValidation.ErrorIfInvalidCode(Parent.TransportModeInfo, Parent.TransportModes);
			}
		}

		#endregion

		#region ContainerMode

		public void ValidateContainerMode()
		{
			ValidateCalculatedProperty(Parent.ContainerModeInfo);
		}

		protected virtual void CheckContainerMode()
		{
			if (Parent.ValidateQuoteProperty)
			{
				if (!(Parent.IsCompareMode && Parent.SelectedComparisonModes.Count > 0) && Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					MandatoryValidation.CheckEntered(Parent.ContainerModeInfo);
				}
				ListValidation.ErrorIfInvalidCode(Parent.ContainerModeInfo, Parent.ContainerModes);
			}
		}

		#endregion

		public void ValidateContainerPackModeOverride()
		{
			ValidateCalculatedProperty(Parent.ContainerPackModeOverrideInfo);
		}

		protected virtual void CheckContainerPackModeOverride()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ContainerPackModeOverrideInfo, Parent.HBLDeliveryModes);
		}

		#endregion

		#region ShipmentStatus

		public void ValidateShipmentStatus()
		{
			ValidateCalculatedProperty(Parent.ShipmentStatusInfo);
		}

		protected virtual void CheckShipmentStatus()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ShipmentStatusInfo, Parent.Booking.JS_ShipmentStatusInfo);
			}
		}

		#endregion

		#region Weight

		public void ValidateWeight()
		{
			ValidateCalculatedProperty(Parent.WeightInfo);
		}

		protected virtual void CheckWeight()
		{
			if (Parent.ValidateBookingProperty)
			{
				if (Parent.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote || Parent.ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
				{
					var booking = Parent.Booking;
					if (booking.JS_ActualWeight.IsEmpty && booking.JS_ActualVolume.IsEmpty)
					{
						if (booking.JS_PackingMode == Core.Constants.RateMode.LSE || booking.JS_PackingMode == Core.Constants.RateMode.LCL)
						{
							Parent.WeightInfo.AddError(Res.GetString("99a91062-43c6-443e-90dc-e10a89a99b40", "You must specify either a weight or volume if your shipment is LSE or LCL."));
						}
					}
					else if (booking.JS_ActualWeight != booking.TotalOuterPacksWeight)
					{
						Parent.WeightInfo.AddError(Res.GetString("01418c38-9a22-491c-9279-87910734a42c", "The weight you have specified does not match the details specified for Loose Cargo. Please check your calculations."));
					}
				}

				if (!Parent.WeightInfo.HasErrors())
				{
					ProxyValidation(Parent.WeightInfo, Parent.Booking.JS_ActualWeightInfo);
				}
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.WeightInfo, Parent.Quote.CurrentOneOffQuote.TT_ActualWeightInfo);
			}
		}

		#endregion

		#region WeightUnit

		public void ValidateWeightUnit()
		{
			ValidateCalculatedProperty(Parent.WeightUnitInfo);
		}

		protected virtual void CheckWeightUnit()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.WeightUnitInfo, Parent.Booking.JS_UnitOfWeightInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.WeightUnitInfo, Parent.Quote.CurrentOneOffQuote.TT_UnitOfWeightInfo);
			}
		}

		#endregion

		#region Volume

		public void ValidateVolume()
		{
			ValidateCalculatedProperty(Parent.VolumeInfo);
		}

		protected virtual void CheckVolume()
		{
			if (Parent.ValidateBookingProperty)
			{
				if (Parent.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote || Parent.ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
				{
					var booking = Parent.Booking;
					if (booking.JS_ActualVolume.IsEmpty && booking.JS_ActualWeight.IsEmpty)
					{
						if (booking.JS_PackingMode == Core.Constants.RateMode.LSE || booking.JS_PackingMode == Core.Constants.RateMode.LCL)
						{
							Parent.VolumeInfo.AddError(Res.GetString("99a91062-43c6-443e-90dc-e10a89a99b40", "You must specify either a weight or volume if your shipment is LSE or LCL."));
						}
					}
					else if (booking.JS_ActualVolume != booking.TotalOuterPacksVolume)
					{
						Parent.VolumeInfo.AddError(Res.GetString("bd811692-b5aa-4761-ac4b-62884d693eaf", "The volume you have specified does not match the details specified for Loose Cargo. Please check your calculations."));
					}
				}

				if (!Parent.VolumeInfo.HasErrors())
				{
					ProxyValidation(Parent.VolumeInfo, Parent.Booking.JS_ActualVolumeInfo);
				}
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.VolumeInfo, Parent.Quote.CurrentOneOffQuote.TT_ActualVolumeInfo);
			}
		}

		#endregion

		#region VolumeUnit

		public void ValidateVolumeUnit()
		{
			ValidateCalculatedProperty(Parent.VolumeUnitInfo);
		}

		protected virtual void CheckVolumeUnit()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.VolumeUnitInfo, Parent.Booking.JS_UnitOfVolumeInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.VolumeUnitInfo, Parent.Quote.CurrentOneOffQuote.TT_UnitOfVolumeInfo);
			}
		}

		#endregion

		#region Chargeable

		public void ValidateChargeable()
		{
			ValidateCalculatedProperty(Parent.ChargeableInfo);
		}

		protected virtual void CheckChargeable()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ChargeableInfo, Parent.Booking.JS_ActualChargeableInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.ChargeableInfo, Parent.Quote.CurrentOneOffQuote.TT_ChargeableInfo);
			}
		}

		#endregion

		#region IsDomesticFreight

		public void ValidateIsDomesticFreight()
		{
			ValidateCalculatedProperty(Parent.IsDomesticFreightInfo);
		}

		protected virtual void CheckIsDomesticFreight()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.IsDomesticFreightInfo, Parent.Booking.IsDomesticFreightInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.IsDomesticFreightInfo, Parent.Quote.CurrentOneOffQuote.IsDomesticFreightInfo);
			}
		}

		#endregion

		#region Origin

		public void ValidateOrigin()
		{
			ValidateCalculatedProperty(Parent.OriginInfo);
		}

		protected virtual void CheckOrigin()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.OriginInfo, Parent.Booking.JS_RL_NKOriginInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.OriginInfo, Parent.Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocationInfo);
			}
		}

		#endregion

		#region Destination

		public void ValidateDestination()
		{
			ValidateCalculatedProperty(Parent.DestinationInfo);
		}

		protected virtual void CheckDestination()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.DestinationInfo, Parent.Booking.JS_RL_NKDestinationInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.DestinationInfo, Parent.Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocationInfo);
			}
		}

		#endregion

		#region ETD

		public void ValidateETD()
		{
			ValidateCalculatedProperty(Parent.ETDInfo);
		}

		protected virtual void CheckETD()
		{
			if (!Parent.ETD.IsEmpty && Parent.ScheduleChooser?.Sailing != null && Parent.ETD > Parent.ScheduleChooser.Sailing.JX_JA_E_DEP)
			{
				Parent.ETDInfo.AddError(Res.GetString("1f41ffa6-5448-4656-ac3c-fb515a1c44d6", "Sailing Schedule ETD cannot be earlier than Shipment ETD. Please correct."));
			}

			if (!Parent.ETDInfo.HasErrors() && Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ETDInfo, Parent.Booking.JS_E_DEPInfo);
			}
		}

		#endregion

		#region ETA

		public void ValidateETA()
		{
			ValidateCalculatedProperty(Parent.ETAInfo);
		}

		protected virtual void CheckETA()
		{
			if (!Parent.ETA.IsEmpty && Parent.ScheduleChooser?.Sailing != null && Parent.ScheduleChooser.Sailing.JX_JB_E_ARV > Parent.ETA)
			{
				Parent.ETAInfo.AddWarning(Res.GetString("77f79532-87ac-49de-8472-860e7ce278b1", "Shipment ETA is earlier than Sailing Schedule ETA. Are you sure you want to continue?"));
			}

			if (!Parent.ETAInfo.HasErrors() && Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ETAInfo, Parent.Booking.JS_E_ARVInfo);
			}
		}

		#endregion

		#region Via

		public void ValidateVia()
		{
			ValidateCalculatedProperty(Parent.ViaInfo);
		}

		protected virtual void CheckVia()
		{
			if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.ViaInfo, Parent.Quote.CurrentOneOffQuote.TT_RL_NKViaLocationInfo);
			}
		}

		#endregion

		#region PaymentTerms

		public void ValidatePaymentTerms()
		{
			ValidateCalculatedProperty(Parent.PaymentTermsInfo);
		}

		protected virtual void CheckPaymentTerms()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.PaymentTermsInfo, Parent.Booking.JS_INCOInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.PaymentTermsInfo, Parent.Quote.CurrentOneOffQuote.TT_IncoTermInfo);
			}

			IncotermValidation.Instance.WarningIfExpired(Parent.PaymentTermsInfo);
		}

		#endregion

		#region ServiceLevel

		public void ValidateServiceLevel()
		{
			ValidateCalculatedProperty(Parent.ServiceLevelInfo);
		}

		protected virtual void CheckServiceLevel()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.ServiceLevelInfo, Parent.Booking.JS_RS_NKServiceLevelInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.ServiceLevelInfo, Parent.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevelInfo);
			}
		}

		#endregion

		#region Commodity

		public void ValidateCommodity()
		{
			ValidateCalculatedProperty(Parent.CommodityInfo);
		}

		protected virtual void CheckCommodity()
		{
			if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.CommodityInfo, Parent.Quote.CurrentOneOffQuote.TT_RH_NKCommodityInfo);
			}
		}

		#endregion

		#region GoodsValue

		public void ValidateGoodsValue()
		{
			ValidateCalculatedProperty(Parent.GoodsValueInfo);
		}

		protected virtual void CheckGoodsValue()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.GoodsValueInfo, Parent.Booking.JS_GoodsValueInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.GoodsValueInfo, Parent.Quote.CurrentOneOffQuote.TT_ValueOfGoodsInfo);
			}
		}

		#endregion

		#region GoodsCurrency

		public void ValidateGoodsCurrency()
		{
			ValidateCalculatedProperty(Parent.GoodsCurrencyInfo);
		}

		protected virtual void CheckGoodsCurrency()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.GoodsCurrencyInfo, Parent.Booking.JS_RX_NKGoodsValueCurrInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.GoodsCurrencyInfo, Parent.Quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo);
			}
		}

		#endregion

		#region InsuranceValue

		public void ValidateInsuranceValue()
		{
			ValidateCalculatedProperty(Parent.InsuranceValueInfo);
		}

		protected virtual void CheckInsuranceValue()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.InsuranceValueInfo, Parent.Booking.JS_InsuranceValueInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.InsuranceValueInfo, Parent.Quote.CurrentOneOffQuote.TT_InsureValInfo);
			}
		}

		#endregion

		#region InsuranceCurrency

		public void ValidateInsuranceCurrency()
		{
			ValidateCalculatedProperty(Parent.InsuranceCurrencyInfo);
		}

		protected virtual void CheckInsuranceCurrency()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.InsuranceCurrencyInfo, Parent.Booking.JS_RX_NKInsuranceCurrencyInfo);
			}
			else if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.InsuranceCurrencyInfo, Parent.Quote.CurrentOneOffQuote.TT_RX_NKInsureValCurrInfo);
			}
		}

		#endregion

		#region PickupEquipment

		public void ValidatePickupEquipment()
		{
			ValidateCalculatedProperty(Parent.PickupEquipmentInfo);
		}

		protected virtual void CheckPickupEquipment()
		{
			ListValidation.ErrorIfInvalidCode(Parent.PickupEquipmentInfo, Parent.Equipments);
		}

		#endregion

		#region DeliveryEquipment

		public void ValidateDeliveryEquipment()
		{
			ValidateCalculatedProperty(Parent.DeliveryEquipmentInfo);
		}

		protected virtual void CheckDeliveryEquipment()
		{
			ListValidation.ErrorIfInvalidCode(Parent.DeliveryEquipmentInfo, Parent.Equipments);
		}

		#endregion

		#region HBLAWBChargesDisplay

		public void ValidateHBLAWBChargesDisplay()
		{
			ValidateCalculatedProperty(Parent.HBLAWBChargesDisplayInfo);
			if (Parent.Booking != null)
			{
				ProxyValidation(Parent.HBLAWBChargesDisplayInfo, Parent.Booking.JS_HBLAWBChargesDisplayInfo);
			}
		}

		protected virtual void CheckHBLAWBChargesDisplay()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HBLAWBChargesDisplayInfo, Parent.HBLAWBChargesDisplay_List);
		}

		#endregion

		#region CurrentOneOffQuote

		public void ValidateOneOffQuoteEntriesAndLines()
		{
			if (Parent.ValidateQuoteProperty)
			{
				ProxyValidation(Parent.QuoteNumberOfEntriesInfo, Parent.Quote.CurrentOneOffQuote.TT_NumberOfEntriesInfo);
				ProxyValidation(Parent.QuoteNumberOfEntryLinesInfo, Parent.Quote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo);
			}
		}

		#endregion

		#region DeliveryDueDate

		public void ValidateDeliveryDueDate()
		{
			ValidateCalculatedProperty(Parent.DeliveryDueDateInfo);
		}

		protected virtual void CheckDeliveryDueDate()
		{
			if (Parent.ValidateBookingProperty)
			{
				ProxyValidation(Parent.DeliveryDueDateInfo, Parent.Booking.JS_DeliveryDueDateInfo);
			}
		}

		#endregion

		#region Validate Total CO2e

		public void ValidateTotalCO2eForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForBindingInfo);
		}

		protected void CheckTotalCO2eForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForBindingInfo);
		}

		#endregion

		#region Validate Total CO2e Sorting

		public void ValidateTotalCO2eForSorting()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForSortingInfo);
		}

		protected void CheckTotalCO2eForSorting()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForSortingInfo);
		}

		#endregion

		#endregion

		#region ProxyValidation

		bool ProxyValidation(ZPropertyInfo toInfo, ZPropertyInfo fromInfo)
		{
			bool success;

			if (!fromInfo.BizObj.IsValidationSuspended)
			{
				((IBusinessObjectInternals)fromInfo.BizObj).Validate(fromInfo);
				toInfo.AddAllNotificationsFrom(fromInfo);
				success = true;
			}
			else
			{
				success = false;
			}

			return success;
		}

		#endregion

		#region Implementation

		readonly QuotedBooking Parent;
		readonly ISingleElementListInternal ParentListInternals;

		#endregion
	}
}

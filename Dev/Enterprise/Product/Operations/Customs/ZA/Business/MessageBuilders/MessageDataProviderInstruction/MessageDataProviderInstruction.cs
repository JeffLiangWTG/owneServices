#define CODE_ANALYSIS
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.TransportModes;
using static Enterprise.Customs.Common.ZA.ZAJobMessageTypeList.Codes;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCategoryCodes;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes;

namespace Enterprise.Customs.ZA.Business
{
	internal class MessageDataProviderKeyFactor
	{
		internal ZString ShipmentType;
		internal ZString TransportMode;
		internal ZString ProcedureCategory;
		internal ZString CPC;
		internal ZString PPC;
		internal ZString MessageType;
		internal ZString RemovalTransportMode;
		internal RefCountry CountryOfDestination;
		internal RefCountry CountryOfOrigin;
		internal ZString FirstNonSpecificTariffTypeConcession;
		internal ZString RelationshipIndicator;
		internal ZString DeclarationType;
		internal ZString VDN;
		internal ZString Remover;
		internal ZString SubContractor;
	}

	static class MessageDataProviderInstruction
	{
		#region Messages DataProvider Instruction

		public static bool ShouldOutputDutiesAndFees(MessageDataProviderKeyFactor factor)
		{
			return factor.DeclarationType != DeclarationTypeList.Codes.RegularIncompleteDeclaration
					&& factor.DeclarationType != DeclarationTypeList.Codes.RegularProvisionalDeclaration;
		}

		public static bool ShouldOutputPartClearanceQuantity(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == ExBond || factor.ShipmentType == Import;
		}

		public static bool ShouldOutputFromWarehouse(MessageDataProviderKeyFactor factor)
		{
			return factor.PPC.IsWarehousingProcedure()
				|| factor.ShipmentType == ExBond
				|| IsFromWarehouseRequired(factor);
		}

		public static bool IsFromWarehouseRequired(MessageDataProviderKeyFactor factor) => CPCIsFromWarehouseRequired(factor.CPC);

		static bool CPCIsFromWarehouseRequired(ZString cpc)
		{
			return cpc == _52
				|| cpc == _53
				|| cpc == _67
				|| cpc == _68;
		}

		public static bool ShouldOutputConsignee(MessageDataProviderKeyFactor factor)
		{
			return factor.CPC.IsWarehousingProcedure();
		}

		public static bool ShouldOutputToWarehouse(MessageDataProviderKeyFactor factor)
		{
			return factor.CPC.IsWarehousingProcedure() || (factor.CPC == _20 && (factor.CountryOfDestination?.IsBLNS ?? false));
		}

		#region PortOfExit/Destination

		public static bool ShouldOutputPortOfExit(MessageDataProviderKeyFactor factor)
		{
			return IsPortOfDestinationRequired(factor) || IsPortOfExitRequired(factor);
		}

		public static bool IsPortOfDestinationRequired(MessageDataProviderKeyFactor factor)
		{
			return factor.CPC == _20 || factor.CPC == _21 || factor.CPC == _22
				|| factor.CPC == _40 || factor.CPC == _42;
		}

		public static bool IsPortOfExitRequired(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Export && (factor.TransportMode == Sea || factor.TransportMode == Air || factor.TransportMode == Road || factor.TransportMode == Rail)
				|| factor.ShipmentType == ExBond && (factor.CountryOfDestination?.IsBLNS ?? false);
		}

		#endregion

		public static bool ShouldOutputLocationOfGoods(MessageDataProviderKeyFactor factor)
		{
			return factor.TransportMode != Road && !factor.IsExBond();
		}

		public static bool ShouldOutputCountryOfExport(MessageDataProviderKeyFactor factor)
		{
			return !factor.IsExBond();
		}

		public static bool ShouldOutputTransportDocumentDate(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType != ExBond || factor.RemovalTransportMode == Road;
		}

		public static bool ShouldOutputTransportDocumentIssuedAt(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType != ExBond || (factor.CountryOfDestination?.IsBLNS ?? true);
		}

		public static bool ShouldOutputDateOfAssessment(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType == MessageSubTypeCodes.Codes.Change
				|| factor.MessageType == MessageSubTypeCodes.Codes.Replace
				|| factor.MessageType == MessageSubTypeCodes.Codes.Cancellation;
		}

		public static bool ShouldOutputDateOfArrival(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import && (factor.TransportMode == Sea || factor.TransportMode == Road);
		}

		public static bool ShouldOutputDateOfDepartureOrDateOfFlight(MessageDataProviderKeyFactor factor)
		{
			return ShouldOutputDateOfDeparture(factor) || ShouldOutputDateOfFlight(factor);
		}

		public static bool ShouldOutputDateOfDeparture(MessageDataProviderKeyFactor factor)
		{
			return (factor.ShipmentType != Import && (factor.TransportMode == Sea || factor.TransportMode == Road || factor.TransportMode == Air));
		}

		public static bool ShouldOutputDateOfFlight(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import && factor.TransportMode == Air;
		}

		public static bool ShouldOutputRelatedIndicatorAndValuationCode(MessageDataProviderKeyFactor factor)
		{
			bool shouldOutput = false;

			if (factor.ShipmentType == Import)
			{
				if (!CPCCodesRelevantToRelatedIndicatorAndValuationCode.Contains(factor.CPC))
				{
					shouldOutput = true;
				}
			}
			return shouldOutput;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly List<string> CPCCodesRelevantToRelatedIndicatorAndValuationCode = new List<string>() { _12, _20, _21, _22, _37, _78 };

		public static bool ShouldOutputFreeForPaymentMethod(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType == MessageSubTypeCodes.Codes.Cancellation;
		}

		public static bool ShouldOutputRefundAcknowledgementIndicator(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputCreditTerms(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExport();
		}

		public static bool ShouldOutputTransactionBankCode(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExport();
		}

		public static bool ShouldOutputChangeAcknowledgementIndicator(MessageDataProviderKeyFactor factor)
		{
			return factor.IsChangeOrCancel();
		}

		public static bool ShouldOutputFinancialAccountNumberFromOriginalMessage(MessageDataProviderKeyFactor factor)
		{
			return factor.IsVOCMessage();
		}

		public static bool ShouldOutputCaseNumber(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType != MessageSubTypeCodes.Codes.Original;
		}

		public static bool ShouldOutputContainers(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType != ExBond
				&&
				(factor.TransportMode == Core.Constants.TransportModes.Sea
				|| factor.TransportMode == Core.Constants.TransportModes.Road
				|| factor.TransportMode == Core.Constants.TransportModes.Rail
				|| factor.TransportMode == Core.Constants.TransportModes.Other
				|| factor.TransportMode == ZString.Empty);
		}

		#region SG1

		public static bool ShouldOutputTransportDocumentNumber(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType != ExBond || factor.RemovalTransportMode == Road;
		}

		public static bool ShouldOutputOriginalMRN(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType == MessageSubTypeCodes.Codes.Cancellation
				|| factor.MessageType == MessageSubTypeCodes.Codes.Change;
		}

		public static bool ShouldOutputMRNToBeReplaced(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType == MessageSubTypeCodes.Codes.Change
				|| factor.MessageType == MessageSubTypeCodes.Codes.Replace
				|| factor.MessageType == MessageSubTypeCodes.Codes.Cancellation;
		}

		#endregion

		#region SG2

		public static bool ShouldOutputTotalNoOfPacks(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		#endregion

		#region SG3

		public static bool ShouldOutputMarksAndNumbers(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		#endregion

		#region SG4

		public static bool ShouldOutputVoyageFlightNo(MessageDataProviderKeyFactor factor)
		{
			return (factor.ShipmentType != ExBond && (factor.TransportMode == Sea || factor.TransportMode == Air));
		}

		public static bool ShouldOutputTransportMode(MessageDataProviderKeyFactor factor) //TDT+20
		{
			return !factor.IsExBond() || (factor.CountryOfDestination != null && factor.CountryOfDestination.IsBLNS);
		}

		public static bool ShouldOutputUnknownRemovalTransportMode(MessageDataProviderKeyFactor factor) //TDT+20
		{
			return factor.CPC == _41 || (factor.CPC == _48 && factor.PPC == _42);
		}

		public static bool IsRemovalModeEmptyOrOther(MessageDataProviderKeyFactor factor) => factor.RemovalTransportMode == Core.Constants.TransportModes.Other || factor.RemovalTransportMode.IsEmpty;

		public static bool ShouldOutputRemovalTransportMode(MessageDataProviderKeyFactor factor) //TDT+20
		{
			var result = false;

			if (MessageDataProviderInstructionHelper.IsExport(factor))
			{
				result = IsFromWarehouseRequired(factor);
			}
			else if (IsShipmentImpOrExw(factor.ShipmentType))
			{
				result = ShouldOutputRemovalTransportModeWithBLNSCheck(factor.CPC, factor.PPC, factor.ProcedureCategory, factor.RemovalTransportMode, factor.CountryOfDestination);
			}

			return result;
		}

		public static bool ShouldOutputRemovalTransportModeWithBLNSCheck(ZString cpc, ZString ppc, ZString procedureCategory, ZString removalTransportMode, RefCountry countryOfDestination = null) //TDT+20
		{
			return RemovalTransportModeRequired(cpc, procedureCategory, removalTransportMode != Road, countryOfDestination) || RemovalTransportModeRequiredForPreviousWarehouseExport(cpc, ppc);
		}

		static bool RemovalTransportModeRequired(ZString cpc, ZString procedureCategory, bool checkForBLNS = false, RefCountry countryOfDestination = null)
		{
			return procedureCategory == _B
				|| (procedureCategory == _E && cpc != _41 && cpc != _47 && cpc != _48)
				|| cpc == _52 || cpc == _53
				|| cpc == _67 || cpc == _68
				|| cpc == _11 && (!checkForBLNS || IsBlns(countryOfDestination));
		}

		static bool RemovalTransportModeRequiredForPreviousWarehouseExport(ZString cpc, ZString ppc)
		{
			return cpc == _48 && (ppc == _48 || ppc == _49);
		}

		public static bool ShouldOutputTransportName(MessageDataProviderKeyFactor factor)
		{
			return ShouldOutputVessel(factor) || ShouldOutputRoadVehicle(factor);
		}

		public static bool ShouldOutputVessel(MessageDataProviderKeyFactor factor)
		{
			return factor.TransportMode == Sea && factor.ShipmentType != ExBond;
		}

		public static bool ShouldOutputRoadVehicle(MessageDataProviderKeyFactor factor)
		{
			return factor.TransportMode == Road;
		}

		#endregion

		#region SG5

		public static bool ShouldOutputInvoiceInformations(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import || factor.ShipmentType == Export;
		}

		#endregion

		#region SG6

		public static bool ShouldOutputImporter(MessageDataProviderKeyFactor factor)
		{
			return factor.IsImport() || factor.IsExBond();
		}

		public static bool ShouldOutputImporterForExportJob(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExport();
		}

		public static bool ShouldOutputExporter(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExport();
		}

		public static bool ShouldOutputRemoverTransporterCode(MessageDataProviderKeyFactor factor) //NAD+AF
		{
			return factor.RemovalTransportMode == Road && !factor.Remover.IsEmpty;
		}

		public static bool RemoverTransporterCodeRequired(MessageDataProviderKeyFactor factor)
		{
			return RemovalTransportModeRequired(factor.CPC, factor.ProcedureCategory, true, factor.CountryOfDestination) && (factor.RemovalTransportMode == Road || (factor.IsExport() && IsFromWarehouseRequired(factor)));
		}

		public static bool RemoverTransporterCodeRequiredForPreviousWarehouseExport(MessageDataProviderKeyFactor factor)
		{
			return RemovalTransportModeRequiredForPreviousWarehouseExport(factor.CPC, factor.PPC) && factor.RemovalTransportMode == Road;
		}

		public static bool ShouldOutputSupplier(MessageDataProviderKeyFactor factor)
		{
			return factor.IsImport();
		}

		public static bool ShouldOutputSupplierCode(MessageDataProviderKeyFactor factor)
		{
			return factor.IsImport() && !factor.VDN.IsEmpty;
		}

		public static bool ShouldOutputOwnerCode(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExBond();
		}

		public static bool ShouldOutputImporterUnregisteredTrader(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputSupplierUnregisteredTrader(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		#endregion

		#region SG10

		public static bool ShouldOutputInvoiceDetails(MessageDataProviderKeyFactor factor)
		{
			var isEnabled = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			var isExBond = factor.ShipmentType == ExBond && (factor.CPC == _46 || factor.CPC == _47);
			return isEnabled && (factor.IsImportOrExport() || isExBond);
		}

		#endregion

		#region SG30

		public static bool ShouldOutputLineLevelInformation(MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType != MessageSubTypeCodes.Codes.Cancellation;
		}

		public static bool ShouldOutputLineNumber(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputTariffCode(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputPreferenceCode(MessageDataProviderKeyFactor factor)
		{
			return !factor.IsExport();
		}

		public static bool ShouldOutputGoodsDescription(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputCustomsProcedureCode(MessageDataProviderKeyFactor factor)
		{
			// TODO: ToBeImplemented: Strange specification on GuideForApplication #Victor 20160422
			return true;
		}

		public static bool ShouldOutputTradeStatisticsIndicator(MessageDataProviderKeyFactor factor)
		{
			return factor.IsExport();
		}

		public static bool ShouldOutputCountryOfOrigin(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputCustomsQuantity(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputCustomsUnitQty(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputClassificationQuantity(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputClassificationUnitQty(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputWarehouseCountableQuantity(MessageDataProviderKeyFactor factor)
		{
			return ShouldOutputToWarehouse(factor) || ShouldOutputFromWarehouse(factor);
		}

		public static bool ShouldOutputWarehouseCountableUnitQty(MessageDataProviderKeyFactor factor)
		{
			return ShouldOutputToWarehouse(factor) || ShouldOutputFromWarehouse(factor);
		}

		public static bool ShouldOutputRebateUserCode(MessageDataProviderKeyFactor factor)
		{
			return ShouldOutputImporterCustomsCodeAsRebateUserCode(factor) || ShouldOutputImporterRebateUserCodeAsRebateUserCode(factor);
		}

		public static bool ShouldOutputImporterRebateUserCodeAsRebateUserCode(MessageDataProviderKeyFactor factor)
		{
			return factor.FirstNonSpecificTariffTypeConcession == UniversalReferenceConstants.Schedule._3;
		}

		public static bool ShouldOutputImporterCustomsCodeAsRebateUserCode(MessageDataProviderKeyFactor factor)
		{
			return factor.FirstNonSpecificTariffTypeConcession == UniversalReferenceConstants.Schedule._4;
		}

		public static bool ShouldOutputActualPrice(MessageDataProviderKeyFactor factor)
		{
			return (factor.IsImport() || factor.IsExBond());
		}

		public static bool ShouldOutputCustomsValue(MessageDataProviderKeyFactor factor)
		{
			return true;
		}

		public static bool ShouldOutputTotalDutiesDueWhenZero(MessageDataProviderKeyFactor factor)
		{
			return (factor.IsImport() || factor.IsExBond())
				&& factor.IsChangeOrCancel() && ShouldOutputDutiesAndFees(factor);
		}

		public static bool ShouldOutputTotalVATDueWhenZero(MessageDataProviderKeyFactor factor)
		{
			return (factor.IsImport() || factor.IsExBond())
				&& factor.IsChangeOrCancel() && ShouldOutputDutiesAndFees(factor);
		}

		public static bool ShouldOutputPreviousMRN(MessageDataProviderKeyFactor factor)
		{
			return factor.PPC != _00;
		}

		public static bool ShouldOutputPreviousMRNLineNumber(MessageDataProviderKeyFactor factor)
		{
			return factor.PPC.IsWarehousingProcedure()
				|| factor.CPC == _36
				|| factor.CPC == _38
				|| factor.CPC == _62
				|| factor.CPC == _65
				|| factor.CPC == _66
				|| factor.CPC == _83;
		}

		public static bool ShouldOutputAdditionalInformation(ZString additionalInformationCode, MessageDataProviderKeyFactor factor)
		{
			// TODO: ToBeChecked: Currently always true as we govern the source collection, will see later if we need to remove this method or add in logic #Victor 20160429
			return true;
		}

		public static bool ShouldOutputProvisionalPayments(MessageDataProviderKeyFactor factor)
		{
			return (factor.IsImport() || factor.IsExBond()) && ShouldOutputDutiesAndFees(factor);
		}

		public static bool ShouldOutputProvisionalPaymentsForDiamondLevy(MessageDataProviderKeyFactor factor)
		{
			return (factor.IsExport() && ShouldOutputDutiesAndFees(factor));
		}

		#endregion

		#region SG49

		public static bool ShouldOutputTotalCIFCAmount(MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import && ShouldOutputDutiesAndFees(factor);
		}

		public static bool ShouldOutputTotalTransactionValueAndCurrency(MessageDataProviderKeyFactor factor)
		{
			return factor.IsImportOrExport();
		}

		#endregion

		#endregion

		public static bool IsBlns(RefCountry country) => country?.IsBLNS ?? false;

		public static bool IsShipmentImpOrExw(string shipmentType) => new ZString[] { Import, ExBond }.Contains(shipmentType);
	}

	static class MessageDataProviderInstructionHelper
	{
		internal static bool IsImportOrExport(this MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import || factor.ShipmentType == Export;
		}

		internal static bool IsImport(this MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Import;
		}

		internal static bool IsExport(this MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == Export;
		}

		internal static bool IsExBond(this MessageDataProviderKeyFactor factor)
		{
			return factor.ShipmentType == ExBond;
		}

		internal static bool IsWarehousingProcedure(this ZString procedureCode)
		{
			return procedureCode == _40 ||
				procedureCode == _41 || procedureCode == _42 || procedureCode == _43 ||
				procedureCode == _44 || procedureCode == _45 || procedureCode == _46 ||
				procedureCode == _47 || procedureCode == _48 || procedureCode == _49;
		}

		internal static bool IsSpecialRebate(this MessageDataProviderKeyFactor factor) => false;

		internal static bool IsChangeOrCancel(this MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType == MessageSubTypeCodes.Codes.Change
				|| factor.MessageType == MessageSubTypeCodes.Codes.Cancellation
				|| factor.MessageType == MessageSubTypeCodes.Codes.Replace;
		}

		internal static bool IsVOCMessage(this MessageDataProviderKeyFactor factor)
		{
			return factor.MessageType != Enterprise.Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;
		}

		internal static bool IsOrdinaryLevyItem(this MessageDataProviderKeyFactor factor)
		{
			return factor.CPC == _10;
		}
	}
}

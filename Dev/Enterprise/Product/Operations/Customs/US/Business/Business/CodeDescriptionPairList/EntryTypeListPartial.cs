using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class EntryTypeList
	{
		/// <summary>
		/// EntrySummaryMessageBuilder has a logic to opt out some message blocks (eg. 22 block) based on entry type.
		/// When ACE has a new entry type, test around the corresponding blocks.
		/// </summary>
		public static CodeDescriptionPairList GetACEList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.ConsumptionFreeDutiable, Descriptions.ConsumptionFreeDutiable);
			result.AddPair(Codes.ConsumptionQuotaVisa, Descriptions.ConsumptionQuotaVisa);
			result.AddPair(Codes.ConsumptionADDCVD, Descriptions.ConsumptionADDCVD);
			result.AddPair(Codes.ConsumptionFTZ, Descriptions.ConsumptionFTZ);
			result.AddPair(Codes.ConsumptionADDCVDQuotaVisa, Descriptions.ConsumptionADDCVDQuotaVisa);
			result.AddPair(Codes.InformalFreeDutiable, Descriptions.InformalFreeDutiable);
			result.AddPair(Codes.InformalQuotaVisa, Descriptions.InformalQuotaVisa);
			result.AddPair(Codes.Warehouse, Descriptions.Warehouse);
			result.AddPair(Codes.ReWarehouse, Descriptions.ReWarehouse);
			result.AddPair(Codes.TemporaryImportationBond, Descriptions.TemporaryImportationBond);
			result.AddPair(Codes.WarehouseWithdrawalConsumption, Descriptions.WarehouseWithdrawalConsumption);
			result.AddPair(Codes.WarehouseWithdrawalQuota, Descriptions.WarehouseWithdrawalQuota);
			result.AddPair(Codes.WarehouseWithdrawalADDCVD, Descriptions.WarehouseWithdrawalADDCVD);
			result.AddPair(Codes.WarehouseWithdrawalADDCVDQuotaVisa, Descriptions.WarehouseWithdrawalADDCVDQuotaVisa);
			result.AddPair(Codes.DCASR, Descriptions.DCASR);
			result.AddPair(Codes.GovernmentDutiable, Descriptions.GovernmentDutiable);
			result.AddPair(Codes.LowValue, Descriptions.LowValue);

			return result;
		}

		public void RemoveLiquidationEntryTypes()
		{
			RemoveCode(Codes.ReconciliationSummary);
			RemoveCode(Codes.Drawback);
		}

		public void RemoveInBondEntryTypes()
		{
			if (ContainsCode(Codes.ImmediateTransportation))
			{
				RemoveCode(Codes.ImmediateTransportation);
			}

			if (ContainsCode(Codes.ImmediateExportation))
			{
				RemoveCode(Codes.ImmediateExportation);
			}

			if (ContainsCode(Codes.TransportationExportation))
			{
				RemoveCode(Codes.TransportationExportation);
			}
		}

		public void RemoveExWarehouseEntryTypes()
		{
			if (ContainsCode(Codes.WarehouseWithdrawalADDCVD))
			{
				RemoveCode(Codes.WarehouseWithdrawalADDCVD);
			}

			if (ContainsCode(Codes.WarehouseWithdrawalADDCVDQuotaVisa))
			{
				RemoveCode(Codes.WarehouseWithdrawalADDCVDQuotaVisa);
			}

			if (ContainsCode(Codes.WarehouseWithdrawalConsumption))
			{
				RemoveCode(Codes.WarehouseWithdrawalConsumption);
			}

			if (ContainsCode(Codes.WarehouseWithdrawalQuota))
			{
				RemoveCode(Codes.WarehouseWithdrawalQuota);
			}
		}

		public static CodeDescriptionPairList GetExWarehouseEntryTypeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Codes.WarehouseWithdrawalADDCVD, Descriptions.WarehouseWithdrawalADDCVD);
			result.AddPair(Codes.WarehouseWithdrawalADDCVDQuotaVisa, Descriptions.WarehouseWithdrawalADDCVDQuotaVisa);
			result.AddPair(Codes.WarehouseWithdrawalConsumption, Descriptions.WarehouseWithdrawalConsumption);
			result.AddPair(Codes.WarehouseWithdrawalQuota, Descriptions.WarehouseWithdrawalQuota);
			return result;
		}

		public void RemoveDrawbackSummaryEntryTypes()
		{
			if (ContainsCode(Codes.DirectIdentificationManufacturingDrawback))
			{
				RemoveCode(Codes.DirectIdentificationManufacturingDrawback);
			}

			if (ContainsCode(Codes.DirectIdentificationUnusedMerchandiseDrawback))
			{
				RemoveCode(Codes.DirectIdentificationUnusedMerchandiseDrawback);
			}

			if (ContainsCode(Codes.RejectedMerchandiseDrawback))
			{
				RemoveCode(Codes.RejectedMerchandiseDrawback);
			}

			if (ContainsCode(Codes.SubstitutionManufacturerDrawback))
			{
				RemoveCode(Codes.SubstitutionManufacturerDrawback);
			}

			if (ContainsCode(Codes.SubstitutionUnusedMerchandiseDrawback))
			{
				RemoveCode(Codes.SubstitutionUnusedMerchandiseDrawback);
			}

			if (ContainsCode(Codes.OtherDrawback))
			{
				RemoveCode(Codes.OtherDrawback);
			}
		}

		public static CodeDescriptionPairList GetDrawbackSummaryEntryTypeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Codes.DirectIdentificationManufacturingDrawback, Descriptions.DirectIdentificationManufacturingDrawback);
			result.AddPair(Codes.DirectIdentificationUnusedMerchandiseDrawback, Descriptions.DirectIdentificationUnusedMerchandiseDrawback);
			result.AddPair(Codes.RejectedMerchandiseDrawback, Descriptions.RejectedMerchandiseDrawback);
			result.AddPair(Codes.SubstitutionManufacturerDrawback, Descriptions.SubstitutionManufacturerDrawback);
			result.AddPair(Codes.SubstitutionUnusedMerchandiseDrawback, Descriptions.SubstitutionUnusedMerchandiseDrawback);
			result.AddPair(Codes.OtherDrawback, Descriptions.OtherDrawback);
			return result;
		}

		public static CodeDescriptionPairList GetDeliveryCertificateForDrawbackPurposeEntryTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.DirectIdentificationManufacturingDrawback, Descriptions.DirectIdentificationManufacturingDrawback);
			result.AddPair(Codes.SubstitutionManufacturerDrawback, Descriptions.SubstitutionManufacturerDrawback);
			result.AddPair(Codes.OtherDrawback, Descriptions.OtherDrawback);
			return result;
		}

		public static CodeDescriptionPairList GetLowValueDeclarationEntryTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.LowValue, Descriptions.LowValue);
			result.AddPair(Codes.InformalFreeDutiable, Descriptions.InformalFreeDutiable);
			return result;
		}

		public static bool IsAgriculturalLicenseEntryTypes(string code)
		{
			return code == Codes.ConsumptionQuotaVisa
				|| code == Codes.ConsumptionADDCVDQuotaVisa
				|| code == Codes.ConsumptionFTZ
				|| code == Codes.Warehouse
				|| code == Codes.ReWarehouse
				|| code == Codes.WarehouseFTZ
				|| code == Codes.WarehouseWithdrawalConsumption
				|| code == Codes.WarehouseWithdrawalQuota
				|| code == Codes.WarehouseWithdrawalADDCVD
				|| code == Codes.WarehouseWithdrawalADDCVDQuotaVisa;
		}

		public static bool IsQuotaVisa(string code)
		{
			return code == Codes.ConsumptionQuotaVisa ||
				code == Codes.ConsumptionADDCVDQuotaVisa ||
				code == Codes.InformalQuotaVisa ||
				code == Codes.WarehouseWithdrawalQuota ||
				 code == Codes.ConsumptionFTZ ||
				code == Codes.WarehouseWithdrawalADDCVDQuotaVisa;
		}

		public static bool IsQuotaProductExclusionType(string code)
		{
			return code == Codes.ConsumptionQuotaVisa ||
				code == Codes.ConsumptionFTZ ||
				code == Codes.ConsumptionADDCVDQuotaVisa ||
				code == Codes.InformalQuotaVisa ||
				code == Codes.TemporaryImportationBond ||
				code == Codes.WarehouseWithdrawalQuota ||
				code == Codes.WarehouseWithdrawalADDCVDQuotaVisa ||
				code == Codes.GovernmentDutiable;
		}

		public static bool IsExWarehouseType(string code)
		{
			return
				code == Codes.WarehouseWithdrawalConsumption ||
				code == Codes.WarehouseWithdrawalQuota ||
				//code == Codes.AircraftVesselSupplyIE || DN: Todo remove this when we know more info
				code == Codes.WarehouseWithdrawalADDCVD ||
				code == Codes.WarehouseWithdrawalADDCVDQuotaVisa;
		}

		/// <summary>
		/// Cargo has already entered into US Territory on another entry lodged earlier, hence cargo release has been performed  
		/// HMF has been paid
		/// </summary>
		public static bool HasCargoEnteredUSTerritory(string code)
		{
			return IsExWarehouseType(code) || code == Codes.ReWarehouse;
		}

		public static bool IsCargoManifestGroupingAllowed(string code, string transportModeCode)
		{
			return (!IsExWarehouseType(code) && !IsInbondCargoManifestGroupAllowed(code)) ||
				(code == Codes.ReWarehouse && (transportModeCode == TransportModeCodes.Codes.TruckContainer || transportModeCode == TransportModeCodes.Codes.TruckNonContainer)) ||
				(code == Codes.ConsumptionFTZ && !((ZString)transportModeCode).IsEmpty);
		}

		public static bool IsInbondCargoManifestGroupAllowed(string entryType)
		{
			return entryType == EntryTypeList.Codes.ConsumptionFTZ || entryType == EntryTypeList.Codes.ReWarehouse;
		}

		public static bool MayRequireWarehousing(string code)
		{
			return code == Codes.Warehouse ||
				code == Codes.ReWarehouse ||
				code == Codes.TemporaryImportationBond ||
				code == Codes.TradeFair ||
				code == Codes.PermanentExhibition ||
				code == Codes.WarehouseFTZ;
		}

		public static bool IsWarehouseType(string code)
		{
			return code == Codes.Warehouse ||
				code == Codes.ReWarehouse ||
				code == Codes.WarehouseFTZ;
		}

		public static bool IsWarehouseRelated(string code)
		{
			return IsWarehouseType(code) || IsExWarehouseType(code) || code == Codes.ConsumptionFTZ;
		}

		public static bool IsWarehouseRelatedExceptFTZ(string code)
		{
			return code == Codes.Warehouse ||
				code == Codes.ReWarehouse ||
				IsExWarehouseType(code);
		}

		public static bool IsExWarehouseOrReWarehouseType(string code)
		{
			return IsExWarehouseType(code) || code == Codes.ReWarehouse;
		}

		public static bool IsExWarehouseTypeOrFTZ(string code)
		{
			return IsExWarehouseType(code) || code == Codes.ConsumptionFTZ;
		}

		public static bool IsWarehouseOrTIB(string code)
		{
			return code == EntryTypeList.Codes.Warehouse ||
				code == EntryTypeList.Codes.ReWarehouse ||
				code == EntryTypeList.Codes.TemporaryImportationBond;
		}

		public static bool IsConsumptionMXCementImportLicense(string code)
		{
			return code == Codes.ConsumptionFreeDutiable ||
					code == Codes.ConsumptionQuotaVisa ||
					code == Codes.ConsumptionADDCVD ||
					code == Codes.ConsumptionADDCVDQuotaVisa;
		}

		public static bool IsConsumptionForNAFTARecon(string code)
		{
			return code == Codes.ConsumptionFreeDutiable ||
					code == Codes.ConsumptionQuotaVisa ||
					code == Codes.ConsumptionFTZ;
		}

		public static bool IsInBondEntryType(string code)
		{
			return code == Codes.ImmediateExportation ||
				code == Codes.ImmediateTransportation ||
				code == Codes.TransportationExportation;
		}

		public static bool IsInformal(string code)
		{
			return code == Codes.InformalFreeDutiable ||
				code == Codes.InformalQuotaVisa;
		}

		public static bool IsInformalImportWarningOrError(string code)
		{
			return code == Codes.ConsumptionFreeDutiable ||
				   code == Codes.ConsumptionQuotaVisa;
		}

		public static bool IsPaperBased(string code)
		{
			return code == Codes.Appraisement ||
				code == Codes.VesselRepair ||
				code == Codes.TradeFair ||
				code == Codes.PermanentExhibition ||
				code == Codes.AircraftVesselSupplyIE ||
				code == Codes.PermitToProceed ||
				code == Codes.BargeMovement ||
				code == Codes.WarehouseFTZ ||
				code == Codes.Baggage;
		}

		public static bool IsElectronicFilingAllowed(bool isAce, string code)
		{
			bool result = code == Codes.ConsumptionFreeDutiable || code == Codes.InformalFreeDutiable;

			if (isAce)
			{
				result |= (code == Codes.ConsumptionADDCVD);
			}

			return result;
		}

		public static bool IsWarehouseLocationNeededFor(string entryType)
		{
			return IsExWarehouseType(entryType) ||
				MayRequireWarehousing(entryType) ||
				entryType == Codes.ConsumptionFTZ ||
				entryType == Codes.AircraftVesselSupplyIE;
		}

		public static bool IsBondTypeCodeRequired(string entryType)
		{
			return entryType != EntryTypeList.Codes.Appraisement &&
					entryType != EntryTypeList.Codes.InformalFreeDutiable &&
					entryType != EntryTypeList.Codes.InformalQuotaVisa &&
					entryType != EntryTypeList.Codes.DCASR &&
					entryType != EntryTypeList.Codes.GovernmentDutiable;
		}

		public static bool IsValidForRecon(string entryType)
		{
			return entryType == EntryTypeList.Codes.ConsumptionFreeDutiable ||
					entryType == EntryTypeList.Codes.ConsumptionFTZ ||
					entryType == EntryTypeList.Codes.ConsumptionQuotaVisa;
		}

		public static bool IsSoftwoodLumberSection803FarmBillRequired(string entryType)
		{
			return entryType == EntryTypeList.Codes.ConsumptionFreeDutiable ||
				entryType == EntryTypeList.Codes.ConsumptionQuotaVisa ||
				entryType == EntryTypeList.Codes.ConsumptionADDCVD ||
				entryType == EntryTypeList.Codes.ConsumptionFTZ ||
				entryType == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa ||
				entryType == EntryTypeList.Codes.Warehouse ||
				entryType == EntryTypeList.Codes.ReWarehouse ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalConsumption ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalQuota ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalADDCVD ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa ||
				entryType == EntryTypeList.Codes.TemporaryImportationBond;
		}

		public static bool IsSoftwoodLumberPermitNumberRequired(string entryType)
		{
			return entryType == EntryTypeList.Codes.ConsumptionFreeDutiable ||
				entryType == EntryTypeList.Codes.ConsumptionQuotaVisa ||
				entryType == EntryTypeList.Codes.ConsumptionADDCVD ||
				entryType == EntryTypeList.Codes.ConsumptionFTZ ||
				entryType == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa ||
				entryType == EntryTypeList.Codes.Warehouse ||
				entryType == EntryTypeList.Codes.ReWarehouse ||
				entryType == EntryTypeList.Codes.InformalFreeDutiable ||
				entryType == EntryTypeList.Codes.InformalQuotaVisa;
		}

		public static bool IsHMFNotApplicable(string entryType)
		{
			return
				entryType == EntryTypeList.Codes.InformalFreeDutiable ||
				entryType == EntryTypeList.Codes.InformalQuotaVisa ||
				entryType == EntryTypeList.Codes.ConsumptionFTZ ||
				entryType == EntryTypeList.Codes.NAFTADutyDeferral ||
				entryType == EntryTypeList.Codes.AircraftVesselSupplyIE ||
				HasCargoEnteredUSTerritory(entryType);
		}

		/// <summary>
		/// ADD/CVD case no is allowed to be entered under this entry type.
		/// </summary>
		public static bool IsValidForADD_CVD(string entryType)
		{
			return IsADD_CVDInvolved(entryType) ||
				entryType == EntryTypeList.Codes.ConsumptionFTZ ||
				entryType == EntryTypeList.Codes.TemporaryImportationBond;
		}

		public static bool IsValidForADD_CVDForACE(string entryType)
		{
			return IsADD_CVDInvolved(entryType) ||
				entryType == EntryTypeList.Codes.ConsumptionFTZ ||
				entryType == EntryTypeList.Codes.Warehouse ||
				entryType == EntryTypeList.Codes.ReWarehouse ||
				entryType == EntryTypeList.Codes.TemporaryImportationBond;
		}

		public static bool IsADD_CVDInvolved(string entryType)
		{
			return entryType == EntryTypeList.Codes.ConsumptionADDCVD ||
				entryType == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalADDCVD ||
				entryType == EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
		}

		public static bool IsTIB(string entryType)
		{
			return entryType == EntryTypeList.Codes.TemporaryImportationBond;
		}

		public static bool IsCustomsChargeIncludedInTotalAmountDue(string entryType, string chargeType)
		{
			bool result = chargeType != Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred;

			if (result && MayRequireWarehousing(entryType))
			{
				result = chargeType == Core.Constants.USCustoms.FeeCodes.HMF;
			}

			return result;
		}

		/// <summary>
		/// CS00113346 - For Warehouse entry, duty & fees except HMF are not payable, yet are still sent in message and
		/// shown on the 7501 doc, but are not included in totals on the 7501 doc.
		/// Excise tax is NOT payable even if it is not deferred. AD/CVD is declared with entry types that support AD/CVD, not with 21
		/// </summary>
		public static bool IsOnlyHMFPayable(string code)
		{
			return code == Codes.Warehouse;
		}

		/// <summary>
		/// HMF is already paid at the time of Warehouse. Duty & Tax & other fees will be paid at the time of ex-warehouse
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public static bool IsNothingPayable(string code)
		{
			return code == Codes.ReWarehouse;
		}

		public static bool IsConsumption(string code)
		{
			return code == Codes.ConsumptionFreeDutiable ||
					code == Codes.ConsumptionADDCVD;
		}

		public static bool IsGovernmentSpecific(string code)
		{
			return code == EntryTypeList.Codes.DCASR ||
				   code == EntryTypeList.Codes.GovernmentDutiable;
		}

		public static bool IsEntryPortRequired(string code)
		{
			return code == EntryTypeList.Codes.ConsumptionQuotaVisa ||
				code == EntryTypeList.Codes.ConsumptionFTZ ||
				code == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa ||
				code == EntryTypeList.Codes.InformalQuotaVisa ||
				code == EntryTypeList.Codes.Warehouse ||
				code == EntryTypeList.Codes.ReWarehouse ||
				code == EntryTypeList.Codes.TemporaryImportationBond ||
				code == EntryTypeList.Codes.LowValue;
		}

		public static bool IsEntryTypeForAPHISNumberExempt(string code)
		{
			return code == EntryTypeList.Codes.InformalFreeDutiable ||
				code == EntryTypeList.Codes.InformalQuotaVisa ||
				code == EntryTypeList.Codes.LowValue;
		}

		public static bool ShouldBeFiledInACE(string code)
		{
			return code == EntryTypeList.Codes.ConsumptionFreeDutiable ||
				code == EntryTypeList.Codes.ConsumptionADDCVD ||
				code == EntryTypeList.Codes.ConsumptionFTZ ||
				code == EntryTypeList.Codes.InformalFreeDutiable ||
				code == EntryTypeList.Codes.TemporaryImportationBond ||
				code == EntryTypeList.Codes.DCASR ||
				code == EntryTypeList.Codes.GovernmentDutiable ||
				code == EntryTypeList.Codes.ConsumptionQuotaVisa ||
				code == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa ||
				code == EntryTypeList.Codes.Warehouse ||
				code == EntryTypeList.Codes.ReWarehouse ||
				code == EntryTypeList.Codes.WarehouseWithdrawalConsumption ||
				code == EntryTypeList.Codes.WarehouseWithdrawalADDCVD ||
				code == EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
		}

		public static bool IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(string code)
		{
			return code == Codes.ConsumptionFTZ ||
				code == Codes.Warehouse ||
				code == Codes.ReWarehouse ||
				code == Codes.TemporaryImportationBond;
		}

		public static bool IsDrawbackHMF_MPFClaimable(string code)
		{
			return code == Codes.DirectIdentificationUnusedMerchandiseDrawback || code == Codes.SubstitutionUnusedMerchandiseDrawback;
		}
	}
}

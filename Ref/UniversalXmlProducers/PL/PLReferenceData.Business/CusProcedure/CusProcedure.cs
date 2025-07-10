using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Constants;
using static CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure.Constants;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure
{
	public static class CusProcedure
	{
		internal static List<RefCusProcedure> GenerateRefCusProceduresFromRefCusCodeLists(
			IReadOnlyCollection<RefCusCodeList> allowedCombinations,
			IReadOnlyCollection<RefCusCodeList> procedureCodes,
			IReadOnlyCollection<RefCusCodeList> previousProcedureCodes,
			IReadOnlyCollection<RefCusCodeList> concessions,
			bool isImport)
		{
			var result = new List<RefCusProcedure>();

			foreach (var procedureCode in procedureCodes)
			{
				result.Add(GetNewRefCusProcedure(procedureCode, null, null, procedureCode.ZZD_StartDate, procedureCode.ZZD_EndDate, isImport));

				foreach (var previousProcedureCode in previousProcedureCodes)
				{
					var allowedCombination = allowedCombinations.FirstOrDefault(x => x.ZZD_Code == $"{procedureCode.ZZD_Code}{previousProcedureCode.ZZD_Code}");
					if (allowedCombination == default)
					{
						continue;
					}
					result.Add(GetNewRefCusProcedure(procedureCode, previousProcedureCode, null, allowedCombination.ZZD_StartDate, allowedCombination.ZZD_EndDate, isImport));

					result.AddRange(concessions.Select(concession => GetNewRefCusProcedure(procedureCode, previousProcedureCode, concession, concession.ZZD_StartDate, concession.ZZD_EndDate, isImport)));
				}
			}

			return result;
		}

		static RefCusProcedure GetNewRefCusProcedure(RefCusCodeList procedure, RefCusCodeList previousProcedure, RefCusCodeList concession, DateTime startDate, DateTime endDate, bool isImport)
		{
			var procedureCode = procedure.ZZD_Code;
			var previousProcedureCode = previousProcedure?.ZZD_Code ?? string.Empty;
			var concessionCode = concession?.ZZD_Code ?? string.Empty;
			var result = new RefCusProcedure()
			{
				ZZ6_ProcedureCode = procedureCode,
				ZZ6_PreviousProcedureCode = previousProcedureCode,
				ZZ6_Concession = concessionCode,
				ZZ6_StartDate = startDate,
				ZZ6_EndDate = endDate,
				ZZ6_Description = GetDescription(),
				ZZ6_ShipmentType = isImport ? ShipmentType.Import : ShipmentType.Export,
				ZZ6_CalculateDuty = IsCalculateDuty(),
				ZZ6_LandedCost = IsLandedCost(),
				ZZ6_IntoWarehouse = GetIsApplicable(IsIntoWarehouse()),
				ZZ6_OutOfWarehouse = GetIsApplicable(IsOutOfWarehouse()),
				ZZ6_IntoTemporaryImport = GetIsApplicable(IsTemporaryImport(procedureCode)),
				ZZ6_OutOfTemporaryImport = GetIsApplicable(IsTemporaryImport(previousProcedureCode)),
				ZZ6_IntoTemporaryExport = GetIsApplicable(IsTemporaryExport(procedureCode)),
				ZZ6_OutOfTemporaryExport = GetIsApplicable(IsTemporaryExport(previousProcedureCode)),
				ZZ6_IntoInwardProcessing = GetIsApplicable(IsInwardProcessing(procedureCode)),
				ZZ6_OutOfInwardProcessing = GetIsApplicable(IsInwardProcessing(previousProcedureCode)),
				ZZ6_IntoOutwardProcessing = GetIsApplicable(IsOutwardProcessing(procedureCode)),
				ZZ6_OutofOutwardProcessing = GetIsApplicable(IsOutwardProcessing(previousProcedureCode)),
			};
			result.ZZ6_CalculateVAT = result.ZZ6_CalculateDuty;

			return result;

			string GetDescription() => concession != null
				? concession.ZZD_Description
				: previousProcedure != null
					? $"{procedure.ZZD_Description} - {previousProcedure.ZZD_Description}"
					: procedure.ZZD_Description;

			bool IsCalculateDuty() => isImport
				&& procedureCode != ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure
				&& procedureCode != ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts
				&& procedureCode != ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended;

			bool IsLandedCost() => procedureCode == ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure;

			string GetIsApplicable(bool predicate) => predicate ? ApplicableXmlValue : NotApplicableXmlValue;

			bool IsIntoWarehouse() => procedureCode == ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure
				|| procedureCode == ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts;

			bool IsOutOfWarehouse() => previousProcedureCode == ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure
				|| previousProcedureCode == ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts
				|| previousProcedureCode == ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls;

			bool IsTemporaryImport(string code) => code == ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission;

			bool IsTemporaryExport(string code) => code == ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21
				|| code == ProcedureCodes.TemporaryExportForReturnInTheUnalteredState;

			bool IsInwardProcessing(string code) => code == ProcedureCodes.InwardProcessingProcedureSuspensionSystem;

			bool IsOutwardProcessing(string code) => code == ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure;
		}
	}
}

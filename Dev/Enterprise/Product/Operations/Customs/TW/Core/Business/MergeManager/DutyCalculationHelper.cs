using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class DutyCalculationHelper
	{
		public static ZString GetPaymentMethod(RefCusProcedure refCusProcedure, RateView tariffRate, ZDateTime dateOfValuation)
		{
			var result = ZString.Empty;
			var rateCode = tariffRate?.RateCode ?? ZString.Empty;
			if (rateCode == UniversalReferenceConstants.RefCusTaxOrFeeCodes.DDF)
			{
				result = EntryChargePaymentMethod.Codes.CAS;
			}
			else if (refCusProcedure != null)
			{
				var paymentMethodLookingFor = GetPaymentMethodRelatedToRateCode(refCusProcedure.Factory, rateCode, dateOfValuation);
				if (!paymentMethodLookingFor.IsEmpty)
				{
					result = GetPaymentMethodByName(refCusProcedure, paymentMethodLookingFor);
				}
			}
			return result;
		}

		public static ZString GetPaymentMethodByName(RefCusProcedure refCusProcedure, ZString name)
		{
			var result = ZString.Empty;
			var attributeValue = refCusProcedure.Attributes?.SingleOrDefault(x => x.ZXB_Name == name)?.ZXB_Value ?? ZString.Empty;
			switch (attributeValue)
			{
				case UniversalReferenceConstants.PaymentMethods.CASH:
				case UniversalReferenceConstants.PaymentMethods.OLDCASH:
					result = EntryChargePaymentMethod.Codes.CAS;
					break;
				case UniversalReferenceConstants.PaymentMethods.DEFERRED:
				case UniversalReferenceConstants.PaymentMethods.OLDDEFERRED:
					result = EntryChargePaymentMethod.Codes.DEF;
					break;
				default:
					result = ZString.Empty;
					break;
			}
			return result;
		}

		static ZString GetPaymentMethodRelatedToRateCode(BusinessObjectFactory factory, ZString rateCode, ZDateTime dateOfValuation)
		{
			var result = ZString.Empty;
			if (rateCode == UniversalReferenceConstants.RefCusRateCodes.TAT || rateCode == UniversalReferenceConstants.RefCusRateCodes.HWS)
			{
				result = UniversalReferenceConstants.RefCusProcedureAttributes.TATPaymentMethod;
			}
			else
			{
				result = ChargeTypeHelper.GetChargeTypes(factory, dateOfValuation).SingleOrDefault(x => x.RateCode == rateCode)?.RateType ?? ZString.Empty;
				if (!result.IsEmpty)
				{
					result += "PaymentMethod";
				}
			}

			return result;
		}

		public static IEnumerable<DutyCalculationIntermediateResult> GetGroupedDuties(this IEnumerable<DutyCalculationIntermediateResult> dutiesIntermediate, ZBool isImport)
		{
			foreach (var dutiesGroup in dutiesIntermediate.GroupBy(x => new { TypeCode = SharedHelper.GetTypeCodeForDutyTaxFee(x.RateCode, x.PaymentMethod, isImport), x.PaymentMethod }))
			{
				yield return new DutyCalculationIntermediateResult()
				{
					Amount = Math.Floor(dutiesGroup.Sum(x => x.Amount)),
					PaymentMethod = dutiesGroup.Key.PaymentMethod,
					TypeCode = dutiesGroup.Key.TypeCode
				};
			}
		}

		internal static IEnumerable<DutyCalculationIntermediateResult> GetGroupedFees(this IEnumerable<DutyCalculationIntermediateResult> dutiesIntermediate)
		{
			foreach (var dutiesGroup in dutiesIntermediate.GroupBy(x => new { x.RateCode, x.PaymentMethod, x.UnitOfCalculation, x.Rate }))
			{
				yield return new DutyCalculationIntermediateResult()
				{
					Amount = Utilities.Round(dutiesGroup.Sum(x => x.Amount), 3),
					BaseValue = dutiesGroup.Sum(x => x.BaseValue),
					PaymentMethod = dutiesGroup.Key.PaymentMethod,
					RateCode = dutiesGroup.Key.RateCode,
					UnitOfCalculation = dutiesGroup.Key.UnitOfCalculation,
					Rate = dutiesGroup.Key.Rate
				};
			}
		}

		internal static void AddEntryLineLevelIntermediateDuties(this IList<DutyCalculationIntermediateResult> entryHeaderIntermediateResults, IEnumerable<DutyCalculationIntermediateResult> entryLineIntermediatesResults)
		{
			foreach (var item in entryLineIntermediatesResults)
			{
				var newEntryHeaderIntermediatesResult = new DutyCalculationIntermediateResult
				{
					Amount = item.Amount,
					PaymentMethod = item.PaymentMethod,
					RateCode = item.RateCode,
					UnitOfCalculation = item.UnitOfCalculation,
					Rate = item.Rate
				};
				entryHeaderIntermediateResults.Add(newEntryHeaderIntermediatesResult);
			}
		}

		internal static ZInt CalculateDaysDelayedDeclaration(ZDateTime dateOfDeclaration, ZDateTime eta)
		{
			var daysDelayedDeclaration = ZInt.Zero;
			if (!dateOfDeclaration.IsEmpty && !eta.IsEmpty)
			{
				daysDelayedDeclaration = (dateOfDeclaration.Date - eta.Date).Days - 15;
				if (daysDelayedDeclaration < ZInt.Zero)
				{
					daysDelayedDeclaration = ZInt.Zero;
				}
			}

			return daysDelayedDeclaration;
		}
	}
}

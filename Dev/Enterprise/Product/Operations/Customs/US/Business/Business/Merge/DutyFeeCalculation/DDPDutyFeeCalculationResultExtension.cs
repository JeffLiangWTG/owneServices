using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.DDPDisbursementCalculation
{
	internal static class DDPDutyFeeCalculationResultExtensions
	{
		public static bool HasRateGreaterThanOne(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine)
		{
			var result = false;

			if (dutyFeeCalculationResult.TryGetValue(invoiceLine, out var dutyAndCharge))
			{
				var resultDatas = dutyAndCharge.Values.ToList().Where(x => x != null);
				result = resultDatas.Sum(x => x.InternalData.PercentOfRate / 100) > 1m;
			}

			return result;
		}

		public static void SetDutyFeeCalculationResult(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine, string chargeType, DDPCalculationResultData resultData)
		{
			Dictionary<string, DDPCalculationResultData> dutyAndCharge;

			if (!dutyFeeCalculationResult.TryGetValue(invoiceLine, out dutyAndCharge))
			{
				dutyAndCharge = new Dictionary<string, DDPCalculationResultData>();
			}

			DDPCalculationResultData existingData;

			if (!dutyAndCharge.TryGetValue(chargeType, out existingData))
			{
				existingData = new DDPCalculationResultData();
			}

			if (!existingData.Equals(resultData) || chargeType == USCustomsChargeTypeList.Codes.DisbursementCharge)
			{
				existingData.Amount = Enterprise.ZArchitecture.Core.Utilities.Round(resultData.Amount, 2);
				existingData.InternalData = new FeeCalculationInternalData(resultData.InternalData?.NoneCustomsValueAmount ?? decimal.Zero, resultData.InternalData?.PercentOfRate ?? decimal.Zero);
				dutyAndCharge[chargeType] = existingData;
			}

			dutyFeeCalculationResult[invoiceLine] = dutyAndCharge;
		}

		public static void RemoveChargesExcept(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine, IEnumerable<string> chargesToKeep)
		{
			Dictionary<string, DDPCalculationResultData> dutyAndCharge;

			if (dutyFeeCalculationResult.TryGetValue(invoiceLine, out dutyAndCharge))
			{
				var chargesToRemove = dutyAndCharge.Keys.Where(x => !chargesToKeep.Contains(x)).ToArray();
				foreach (string chargeType in chargesToRemove)
				{
					dutyAndCharge.Remove(chargeType);
				}
			}
		}

		public static decimal GetChargeOrFeeAmount(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine, string chargeType)
		{
			var ddpResultData = dutyFeeCalculationResult.GetDDPCalculationResultData(invoiceLine, chargeType);
			return ddpResultData?.Amount ?? decimal.Zero;
		}

		public static DDPCalculationResultData GetDDPCalculationResultData(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine, string chargeType)
		{
			DDPCalculationResultData ddpResultData = null;

			Dictionary<string, DDPCalculationResultData> dutyAndCharge;

			if (dutyFeeCalculationResult.TryGetValue(invoiceLine, out dutyAndCharge))
			{
				dutyAndCharge.TryGetValue(chargeType, out ddpResultData);
			}

			return ddpResultData;
		}

		public static decimal GetTotalAmounts(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, string chargeType)
		{
			decimal result = 0m;

			foreach (Dictionary<string, DDPCalculationResultData> dutyAndCharge in dutyFeeCalculationResult.Values)
			{
				DDPCalculationResultData resultData = null;

				dutyAndCharge.TryGetValue(chargeType, out resultData);

				result += resultData?.Amount ?? decimal.Zero;
			}

			return result;
		}

		public static decimal GetTotalAmountsInCombinedLines(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine)
		{
			decimal result = 0m;

			Dictionary<string, DDPCalculationResultData> dutyAndCharge;
			if (dutyFeeCalculationResult.TryGetValue(invoiceLine, out dutyAndCharge))
			{
				foreach (string charge in dutyAndCharge.Keys)
				{
					if (charge != USCustomsChargeTypeList.Codes.DisbursementCharge)
					{
						result += dutyAndCharge[charge].Amount;
					}
				}
			}
			return result;
		}

		public static decimal GetTotalAmounts(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceLine invoiceLine)
		{
			decimal result = 0m;

			Dictionary<string, DDPCalculationResultData> dutyAndCharge;

			decimal dDDAmount = 0m;
			bool hasDDD = false;

			if (dutyFeeCalculationResult.TryGetValue(invoiceLine, out dutyAndCharge))
			{
				foreach (string charge in dutyAndCharge.Keys)
				{
					if (charge == USCustomsChargeTypeList.Codes.DisbursementCharge)
					{
						hasDDD = true;
						dDDAmount = dutyAndCharge[charge]?.Amount ?? decimal.Zero;
					}
					else
					{
						result += dutyAndCharge[charge]?.Amount ?? decimal.Zero;
					}
				}
			}

			return hasDDD ? dDDAmount : result;
		}

		public static decimal GetTotalAmounts(this Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, JobComInvoiceHeader invoiceHeader)
		{
			decimal result = 0m;

			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				result += dutyFeeCalculationResult.GetTotalAmounts(invoiceLine);
			}

			return result;
		}
	}

	public struct CustomsValues
	{
		public ZDecimal SupCustomsValue;
		public ZDecimal CustomsValue;
	}

	static class DDPCustomsValuesExtensionMethods
	{
		public static CustomsValues GetCustomsValues(JobComInvoiceLine invoiceLine)
		{
			CustomsValues result = new CustomsValues();

			result.SupCustomsValue = CustomsValueDeciderForInvoiceLine.GetCustomsValueForDDP(invoiceLine, true);
			result.CustomsValue = CustomsValueDeciderForInvoiceLine.GetCustomsValueForDDP(invoiceLine, false);

			return result;
		}

		public static ZDecimal GetCustomsValue(this Dictionary<JobComInvoiceLine, CustomsValues> customsValues, JobComInvoiceLine invoiceLine, bool forSup)
		{
			CustomsValues values;
			ZDecimal result = ZDecimal.Zero;

			if (customsValues.TryGetValue(invoiceLine, out values))
			{
				result = forSup ? values.SupCustomsValue : values.CustomsValue;
			}

			return result;
		}

		public static ZDecimal GetTotalCustomsValue(this Dictionary<JobComInvoiceLine, CustomsValues> customsValues, JobComInvoiceLine invoiceLine)
		{
			CustomsValues values;
			ZDecimal result = ZDecimal.Zero;

			if (customsValues.TryGetValue(invoiceLine, out values))
			{
				result = values.SupCustomsValue + values.CustomsValue;
			}

			return result;
		}
	}
}

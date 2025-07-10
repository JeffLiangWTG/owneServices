using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public class LineDutyApportionManager
	{
		public void Apportion(JobDeclaration declaration)
		{
			var entry = declaration.CusEntryHeader;

			if (entry != null)
			{
				var lines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();

				decimal totalCustomValue = GetSumCustomValueAndClearExistValue(lines);
				var entryChargeTypeList = entry.Factory.GetCachedValue<EntryChargeTypeList>();
				foreach (CusEntryHeaderCharge charge in entry.Charges)
				{
					ApportionFeesToInvoiceLines(lines, totalCustomValue, charge.C1_ChargeAmount, charge.C1_ChargeType);
				}

				Dictionary<CusEntryLine, ZDecimal> totalCVs = GetTotalCV(entry);
				foreach (CusEntryLine entryLine in totalCVs.Keys)
				{
					totalCVs.TryGetValue(entryLine, out var totalCV);
					if (totalCV > 0 || entryLine.InvoiceLines.Count == 1)
					{
						ApportionLineFee(entryLine, totalCV, entryChargeTypeList);
					}
				}
			}
		}

		ZDecimal GetSumCustomValueAndClearExistValue(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			ZDecimal customValue = 0;

			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				invoiceLine.JI_EntryFeeAmount = 0m;
				invoiceLine.JI_GSTAmount = 0m;
				invoiceLine.JI_DutyAmount = 0m;
				invoiceLine.JI_LevyAmount = 0m;
				invoiceLine.JI_EntryFeeGSTAmount = 0m;
				invoiceLine.JI_IsApportioned = true;

				customValue += invoiceLine.JI_CustomsValue;
			}
			return customValue;
		}

		void ApportionFeesToInvoiceLines(IEnumerable<JobComInvoiceLine> invoiceLines, ZDecimal totalCV, ZDecimal feeAmount, string feeCode)
		{
			string feePropertyName = GetInvoiceLineFieldName(feeCode);
			if (!string.IsNullOrEmpty(feePropertyName))
			{
				JobComInvoiceLine lineWithHighestCV = null;
				decimal alreadyApportionedAmount = 0;
				foreach (JobComInvoiceLine invoiceLine in invoiceLines)
				{
					if (lineWithHighestCV == null || lineWithHighestCV.JI_CustomsValue < invoiceLine.JI_CustomsValue)
					{
						lineWithHighestCV = invoiceLine;
					}

					decimal ratio;

					if (totalCV > 0)
					{
						ratio = invoiceLine.JI_CustomsValue / totalCV;
					}
					else
					{
						ratio = invoiceLines.Count() == 1 ? 1 : 0;
					}

					var apportionedAmount = new ZDecimal(feeAmount * ratio).Round(2);
					if (apportionedAmount > 0)
					{
						var existingAmount = (ZDecimal)invoiceLine[feePropertyName];
						alreadyApportionedAmount += apportionedAmount;
						invoiceLine[feePropertyName] = existingAmount + apportionedAmount;
					}
				}
				var difference = feeAmount - alreadyApportionedAmount;
				if (difference != 0 && lineWithHighestCV != null)
				{
					var existingAmount = (ZDecimal)lineWithHighestCV[feePropertyName];
					lineWithHighestCV[feePropertyName] = existingAmount + difference;
				}
			}
		}

		#region Apportion CusEntryLineFee

		class Fee
		{
			internal ZString Type;
			internal ZDecimal Amount;
		}

		void ApportionLineFee(CusEntryLine entryLine, ZDecimal totalCV, EntryChargeTypeList entryChargeTypeList)
		{
			var feeToApportion = from CusEntryLineFee nonLevyFee in entryLine.Fees
								 where entryChargeTypeList.ContainsCode(nonLevyFee.CF_ChargeType) && nonLevyFee.CF_ChargeAmount > 0 && !EntryChargeTypeList.IsLevy(nonLevyFee.CF_ChargeType)
								 select new Fee() { Type = nonLevyFee.CF_ChargeType, Amount = nonLevyFee.CF_ChargeAmount };
			var totalLevyAmount = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => EntryChargeTypeList.IsLevy(x.CF_ChargeType)).Sum(x => x.CF_ChargeAmount);
			if (totalLevyAmount > 0)
			{
				feeToApportion = feeToApportion.Concat(new Fee[] { new Fee() { Type = LevyAmount, Amount = totalLevyAmount } });
			}
			foreach (Fee fee in feeToApportion)
			{
				var jobComInvoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();
				ApportionFeesToInvoiceLines(jobComInvoiceLines, totalCV, fee.Amount, fee.Type);
			}
		}

		Dictionary<CusEntryLine, ZDecimal> GetTotalCV(CusEntryHeader entry)
		{
			Dictionary<CusEntryLine, ZDecimal> result = new Dictionary<CusEntryLine, ZDecimal>();
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				result.Add(entryLine, GetTotalCustomsValueFromInvoiceLines(entryLine));
			}
			return result;
		}

		ZDecimal GetTotalCustomsValueFromInvoiceLines(CusEntryLine entryLine)
		{
			ZDecimal result = 0m;
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				result += invoiceLine.JI_CustomsValue;
			}
			return result;
		}

		string GetInvoiceLineFieldName(string chargeCode)
		{
			var chargeCodeToLookFor = NZJobComInvoiceLineSchema.Constants.Prefix + "_" + chargeCode;
			switch (chargeCodeToLookFor)
			{
				case NZJobComInvoiceLineSchema.Constants.JI_DTY:
					return JobComInvoiceLine.Schema.JI_DutyAmount;
				case NZJobComInvoiceLineSchema.Constants.JI_GST:
					return JobComInvoiceLine.Schema.JI_GSTAmount;
				case NZJobComInvoiceLineSchema.Constants.JI_ENF:
					return JobComInvoiceLine.Schema.JI_EntryFeeAmount;
				case NZJobComInvoiceLineSchema.Constants.JI_EFG:
					return JobComInvoiceLine.Schema.JI_EntryFeeGSTAmount;
				case NZJobComInvoiceLineSchema.Constants.JI_LVY:
					return JobComInvoiceLine.Schema.JI_LevyAmount;
				default:
					return string.Empty;
			}
		}

		const string LevyAmount = "LVY";
		#endregion
	}
}

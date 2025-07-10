using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class LineCustomsValues
	{
		public void Calculate(JobDeclaration declaration)
		{
			CalculateCore(declaration, entryLine => entryLine.CL_CustomsValue);
		}

		/// <summary>
		/// This is invoked by service task online upgrade. 
		/// At this point, derived duty calculation entry lines and invoice lines are manipulated to satisfy customs purpose
		/// Invoice Lines of the same parent line which is derived duty calculation point to the one cusentryline regardless of tariff & other merge key
		/// </summary>
		/// <param name="declaration"></param>
		public void CalculateOnMergedLines(JobDeclaration declaration)
		{
			CalculateCore(declaration, delegate(CusEntryLine entryLine)
			{
				var result = entryLine.CL_CustomsValue;

				if (entryLine.ParentLine != null && IsDerivedCalculation(entryLine.ParentLine))
				{
					result = entryLine.ParentLine.CL_CustomsValue;
				}

				return result;
			});
		}

		void CalculateCore(JobDeclaration declaration, Func<CusEntryLine, ZDecimal> getCustomsValue)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			if (entry == null)
			{
				entry = declaration.IsACE ? declaration.ActiveEntryHeaders.SimplifiedEntry : declaration.ActiveEntryHeaders.CargoReleaseEntry;
				if (entry == null)
				{
					entry = declaration.ActiveEntryHeaders.FTZEntry;
				}
			}

			if (entry != null)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.US_CustomsValue = new ZDecimal(invoiceLine.JI_CustomsValue - invoiceLine.TotalOriginalGoodsValueInUSD).Round(0);
					if (invoiceLine.US_CustomsValue < 0)
					{
						invoiceLine.US_CustomsValue = 0m;
					}
				}

				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					if (!entryLine.US_SupLine && !IsDerivedCalculation(entryLine))
					{
						var totalCustomsValue = ZDecimal.Zero;
						JobComInvoiceLine lineWithHighestValue = null;

						foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
						{
							totalCustomsValue += invoiceLine.US_CustomsValue;

							if (lineWithHighestValue == null || lineWithHighestValue.US_CustomsValue < invoiceLine.US_CustomsValue)
							{
								lineWithHighestValue = invoiceLine;
							}
						}

						var entryLineCustomsValue = getCustomsValue(entryLine);
						var parentLine = GetSupParentLineToAddCustomsValue(entryLine);
						if (parentLine != null)
						{
							entryLineCustomsValue += getCustomsValue(parentLine);
						}

						var difference = entryLineCustomsValue - totalCustomsValue;

						if (lineWithHighestValue != null && difference != 0m)
						{
							lineWithHighestValue.US_CustomsValue += difference;
						}
					}
				}
			}
		}

		CusEntryLine GetSupParentLineToAddCustomsValue(CusEntryLine entryLine)
		{
			ZBool IsSupTariffValidGetCustomsValue(ZString supTariff)
			{
				return !supTariff.IsEmpty && supTariff != TariffViewAsCodeDescription.NotApplicableCode && !CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(supTariff);
			}

			CusEntryLine result = null;

			var invoiceLine = entryLine.RandomLine;
			if (IsSupTariffValidGetCustomsValue(invoiceLine.US_SupAdditionalTariff1))
			{
				result = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true, (x) => x.US_SupAdditionalLine);
			}
			else if (IsSupTariffValidGetCustomsValue(invoiceLine.US_SupTariff))
			{
				result = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);
			}

			return result;
		}

		bool IsDerivedCalculation(CusEntryLine entryLine)
		{
			var tariff = entryLine.ImportTariff;
			return tariff != null && tariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived && entryLine.Header.IsFormalEntry;
		}
	}
}

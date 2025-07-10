// -----------------------------------------------------------------------
// <copyright file="CusEntryLineExtensionMethods.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.Types;

	public static class CusEntryLineExtensionMethods
	{
		public static JobComInvoiceLine GetInvoiceLineWithADD_CVDDetailsIncludingSecondaryLines(this CusEntryLine entryLine, string caseNoFieldName)
		{
			var result = GetInvoiceLineWithADD_CVDDetails(entryLine, caseNoFieldName);
			if (result == null)
			{
				foreach (CusEntryLine secondaryLine in entryLine.ChildSecondaryEntryLines)
				{
					result = GetInvoiceLineWithADD_CVDDetailsIncludingSecondaryLines(secondaryLine, caseNoFieldName);

					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		public static JobComInvoiceLine GetInvoiceLineWithADD_CVDDetails(this CusEntryLine entryLine, string caseNoFieldName)
		{
			JobComInvoiceLine result = null;

			var randomLine = entryLine.RandomLine;
			if ((!entryLine.IsCombinedLine() && (entryLine.IsSupLineOrNormalLine || (randomLine?.IsDerivedSetsPrentLine() ?? false))) || (entryLine.IsCombinedLine() && (entryLine.IsNormalTariffLine() || ((IDutyData)entryLine).SupTariffs.Count > 1)))
			{
				if (randomLine != null && !((ZString)randomLine[caseNoFieldName]).IsEmpty)
				{
					result = randomLine;
				}
				else
				{
					USCTariff importTariff;

					if (entryLine.IsSecondaryTariffLine)
					{
						importTariff = entryLine.ParentLine != null ? entryLine.ParentLine.ImportTariff : null;
					}
					else
					{
						importTariff = entryLine.ImportTariff;
					}

					if (importTariff != null && importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
					{
						foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
						{
							if (!((ZString)invoiceLine[caseNoFieldName]).IsEmpty && entryLine.Helper.MatchesTariff(invoiceLine))
							{
								result = invoiceLine;
								break;
							}
						}
					}
				}
			}

			return result;
		}
	}
}

using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class IInvoiceLineExtensions
	{
		public static bool MatchesTariff(this IInvoiceLine invoiceLine, bool isSupline, bool isSupAdditionalLine1, bool isSupAdditionalLine2, bool isSupAdditionalLine3, bool isSupAdditionalLine4, bool isSupAdditionalLine5, ZString adValoremTariff)
		{
			var tariffToMatch = invoiceLine.JI_Tariff;
			if (isSupAdditionalLine5)
			{
				tariffToMatch = invoiceLine.US_SupAdditionalTariff5;
			}
			else if (isSupAdditionalLine4)
			{
				tariffToMatch = invoiceLine.US_SupAdditionalTariff4;
			}
			else if (isSupAdditionalLine3)
			{
				tariffToMatch = invoiceLine.US_SupAdditionalTariff3;
			}
			else if (isSupAdditionalLine2)
			{
				tariffToMatch = invoiceLine.US_SupAdditionalTariff2;
			}
			else if (isSupAdditionalLine1)
			{
				tariffToMatch = invoiceLine.US_SupAdditionalTariff1;
			}
			else if (isSupline)
			{
				tariffToMatch = invoiceLine.US_SupTariff;
			}

			return tariffToMatch == adValoremTariff;
		}
	}
}

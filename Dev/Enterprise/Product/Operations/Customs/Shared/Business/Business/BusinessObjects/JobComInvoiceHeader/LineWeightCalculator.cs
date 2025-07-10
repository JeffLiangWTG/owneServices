using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class LineWeightCalculator
	{
		public LineWeightCalculator(BaseJobComInvoiceHeader invoiceHeader)
		{
			if ((object)invoiceHeader == null)
			{
				throw new ArgumentNullException(nameof(invoiceHeader));
			}
			this.invoiceHeader = invoiceHeader;
		}

		readonly BaseJobComInvoiceHeader invoiceHeader;

		public ZWeight GetWeight(BaseJobComInvoiceLine invoiceLine)
		{
			if ((object)invoiceLine == null)
			{
				throw new ArgumentNullException(nameof(invoiceLine));
			}

			ZWeight result = ZWeight.Empty;
			if (invoiceLine.InvoiceHeader != invoiceHeader)
			{
				throw new ApplicationException("The InvoiceLine passed to this method does not belong to the invoice header passed to the constructor");
			}
			if (invoiceLine.JI_Weight > 0)  // Direct from entered weight
			{
				result = new ZWeight(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ);
			}
			else if (invoiceHeader.JZ_Weight > 0)   // Split on remainder from invoice weight less each line
			{
				ZWeight invoiceWeight = new ZWeight(invoiceHeader.JZ_Weight, invoiceHeader.JZ_WeightUQ);
				result = GetWeightByApportionment(invoiceWeight, invoiceLine.LinePriceForWeightApportionCalculation);
			}
			else    // Need to look at every invoice on the dec to work out amount for this invoice
			{
				ZWeight effectiveInvoiceWeight = GetEffectiveInvoiceWeight();
				result = GetWeightByApportionment(effectiveInvoiceWeight, invoiceLine.JI_LinePrice);
			}
			return result;
		}

		ZWeight GetWeightByApportionment(ZWeight effectInvoiceWeight, ZDecimal invoiceLinePrice)
		{
			ZWeight result = ZWeight.Empty;

			try
			{
				decimal valueOfLinesWithNoWeight = 0m;
				ZWeight weightToApportion = effectInvoiceWeight;
				if (weightToApportion.InKilogramsSafe > 0m)
				{
					foreach (BaseJobComInvoiceLine line in invoiceHeader.JobComInvoiceLines)
					{
						if (line.JI_Weight > 0)
						{
							weightToApportion -= new ZWeight(line.JI_Weight, line.JI_WeightUQ);
						}
						else
						{
							valueOfLinesWithNoWeight += line.LinePriceForWeightApportionCalculation;
						}
					}
					if (valueOfLinesWithNoWeight > 0)
					{
						result = weightToApportion * invoiceLinePrice / valueOfLinesWithNoWeight;
					}
				}
			}
			catch (OverflowException)
			{
				result = ZWeight.Invalid;
			}

			return result;
		}

		ZWeight GetEffectiveInvoiceWeight()
		{
			ZWeight result = ZWeight.Empty;
			if (invoiceHeader.JobDeclaration != null)
			{
				if (invoiceHeader.JobDeclaration.Invoices.Count > 1)
				{
					Money valueOfInvoicesWithNoWeight = Money.Empty;
					ZWeight weightToApportion = invoiceHeader.JobDeclaration.GrossWeight;
					CurrencyConverter currencyConverter = invoiceHeader.CurrencyConverter;
					foreach (BaseJobComInvoiceHeader header in invoiceHeader.JobDeclaration.Invoices)
					{
						if (header.JZ_Weight > 0)
						{
							weightToApportion -= new ZWeight(header.JZ_Weight, header.JZ_WeightUQ);
						}
						else
						{
							valueOfInvoicesWithNoWeight = currencyConverter.Add(valueOfInvoicesWithNoWeight, header.InvoiceAmount);
						}
					}
					ZDecimal valueOfInvoicesWithNoWeightInInvoiceCurrency = currencyConverter.ConvertExact(valueOfInvoicesWithNoWeight, invoiceHeader.Invoice_Currency).Amount;
					if (valueOfInvoicesWithNoWeightInInvoiceCurrency > 0)
					{
						result = weightToApportion * invoiceHeader.JZ_InvoiceAmount / valueOfInvoicesWithNoWeightInInvoiceCurrency;
					}
				}
				else
				{
					result = invoiceHeader.JobDeclaration.GrossWeight;
				}
			}

			return result;
		}
	}
}

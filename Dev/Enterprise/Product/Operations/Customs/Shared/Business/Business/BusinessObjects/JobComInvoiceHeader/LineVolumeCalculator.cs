using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class LineVolumeCalculator
	{
		public LineVolumeCalculator(BaseJobComInvoiceHeader invoiceHeader)
		{
			if ((object)invoiceHeader == null)
			{
				throw new ArgumentNullException(nameof(invoiceHeader));
			}
			this.invoiceHeader = invoiceHeader;
		}

		readonly BaseJobComInvoiceHeader invoiceHeader;

		public ZVolume GetVolume(BaseJobComInvoiceLine invoiceLine)
		{
			if ((object)invoiceLine == null)
			{
				throw new ArgumentNullException(nameof(invoiceLine));
			}

			ZVolume result = ZVolume.Empty;
			if (invoiceLine.InvoiceHeader != invoiceHeader)
			{
				throw new ApplicationException("The InvoiceLine passed to this method does not belong to the invoice header passed to the constructor");
			}
			if (invoiceLine.JI_Volume > 0)  // Direct from entered Volume
			{
				result = new ZVolume(invoiceLine.JI_Volume, invoiceLine.JI_VolumeUQ);
			}
			else if (invoiceHeader.JZ_Volume > 0)   // Split on remainder from invoice Volume less each line
			{
				ZVolume invoiceVolume = new ZVolume(invoiceHeader.JZ_Volume, invoiceHeader.JZ_VolumeUQ);
				result = GetVolumeByApportionment(invoiceVolume, invoiceLine.JI_LinePrice);
			}
			else    // Need to look at every invoice on the dec to work out amount for this invoice
			{
				ZVolume effectiveInvoiceVolume = GetEffectiveInvoiceVolume();
				result = GetVolumeByApportionment(effectiveInvoiceVolume, invoiceLine.JI_LinePrice);
			}
			return result;
		}

		ZVolume GetVolumeByApportionment(ZVolume effectInvoiceVolume, ZDecimal invoiceLinePrice)
		{
			ZVolume result = ZVolume.Empty;

			decimal valueOfLinesWithNoVolume = 0m;
			ZVolume volumeToApportion = effectInvoiceVolume;
			if (volumeToApportion.InCubicMetres > 0m)
			{
				foreach (BaseJobComInvoiceLine line in invoiceHeader.JobComInvoiceLines)
				{
					if (line.JI_Volume > 0)
					{
						volumeToApportion -= new ZVolume(line.JI_Volume, line.JI_VolumeUQ);
					}
					else
					{
						valueOfLinesWithNoVolume += line.JI_LinePrice;
					}
				}
				if (valueOfLinesWithNoVolume > 0)
				{
					result = volumeToApportion * invoiceLinePrice / valueOfLinesWithNoVolume;
				}
			}
			return result;
		}

		ZVolume GetEffectiveInvoiceVolume()
		{
			ZVolume result = ZVolume.Empty;
			if (invoiceHeader.JobDeclaration.Invoices.Count > 1)
			{
				Money valueOfInvoicesWithNoVolume = Money.Empty;
				ZVolume volumeToApportion = invoiceHeader.JobDeclaration.Volume;
				CurrencyConverter currencyConverter = invoiceHeader.CurrencyConverter;
				foreach (BaseJobComInvoiceHeader header in invoiceHeader.JobDeclaration.Invoices)
				{
					if (header.JZ_Volume > 0)
					{
						volumeToApportion -= new ZVolume(header.JZ_Volume, header.JZ_VolumeUQ);
					}
					else
					{
						valueOfInvoicesWithNoVolume = currencyConverter.Add(valueOfInvoicesWithNoVolume, header.InvoiceAmount);
					}
				}
				ZDecimal valueOfInvoicesWithNoVolumeInInvoiceCurrency = currencyConverter.ConvertExact(valueOfInvoicesWithNoVolume, invoiceHeader.Invoice_Currency).Amount;
				if (valueOfInvoicesWithNoVolumeInInvoiceCurrency > 0)
				{
					result = volumeToApportion * invoiceHeader.JZ_InvoiceAmount / valueOfInvoicesWithNoVolumeInInvoiceCurrency;
				}
			}
			else
			{
				result = invoiceHeader.JobDeclaration.Volume;
			}
			return result;
		}
	}
}

using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class SPISupInvoiceLine : SPILine
	{
		public SPISupInvoiceLine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory, invoiceLine.EffectiveDateForDutyRate, invoiceLine.US_UC_NKCountryOfOrigin, SPILine.New(invoiceLine.IsCombinedLine() ? null : invoiceLine.ParentTariffLine), invoiceLine.US_UC_NKCountryOfExport)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override CargoWise.Types.ZString ImportTariffCodeCore
		{
			get { return invoiceLine.US_SupTariff; }
		}

		protected override USCTariff ImportTariffCore
		{
			get { return invoiceLine.ImportSupTariff; }
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (!invoiceLine.IsSecondaryTariffLine || (invoiceLine.IsCombinedLine() && invoiceLine.IsChildLine))
				{
					yield return new SPINormalInvoiceLine(invoiceLine);
				}

				if (!invoiceLine.IsCombinedLine())
				{
					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						if (!secondaryLine.HasEmptySupTariff && !secondaryLine.US_SupTariff.Equals(invoiceLine.US_SupTariff))
						{
							yield return new SPISupInvoiceLine(secondaryLine);
						}

						yield return new SPINormalInvoiceLine(secondaryLine);
					}
				}
			}
		}
	}
}

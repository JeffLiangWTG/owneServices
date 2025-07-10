using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class ReconOrigSupSPILine : SPILine
	{
		public ReconOrigSupSPILine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory, invoiceLine.EffectiveDateForDutyRate, invoiceLine.US_UC_NKCountryOfOrigin, SPILine.NewReconOrig(invoiceLine.ParentTariffLine), invoiceLine.US_UC_NKCountryOfExport)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override CargoWise.Types.ZString ImportTariffCodeCore
		{
			get { return invoiceLine.US_R_OrigSupTariff; }
		}

		protected override USCTariff ImportTariffCore
		{
			get { return invoiceLine.OriginalImportSupTariff; }
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (!invoiceLine.IsSecondaryTariffLine)
				{
					yield return new ReconOrigNormalSPILine(invoiceLine);
				}

				foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
				{
					if (!secondaryLine.US_R_OrigSupTariff.IsEmpty && !secondaryLine.US_R_OrigSupTariff.Equals(invoiceLine.US_R_OrigSupTariff))
					{
						yield return new ReconOrigSupSPILine(secondaryLine);
					}

					yield return new ReconOrigNormalSPILine(secondaryLine);
				}
			}
		}
	}
}

using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class ReconOrigNormalSPILine : SPILine
	{
		public ReconOrigNormalSPILine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory, invoiceLine.EffectiveDateForDutyRate, invoiceLine.US_UC_NKCountryOfOrigin, SPILine.NewReconOrig(invoiceLine.ParentTariffLine), invoiceLine.US_UC_NKCountryOfExport)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override CargoWise.Types.ZString ImportTariffCodeCore
		{
			get { return invoiceLine.US_R_OrigTariff; }
		}

		protected override USCTariff ImportTariffCore
		{
			get { return invoiceLine.OriginalImportTariff; }
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (invoiceLine.US_R_OrigSupTariff.IsEmpty)
				{
					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						yield return new ReconOrigNormalSPILine(secondaryLine);
					}
				}
			}
		}
	}
}

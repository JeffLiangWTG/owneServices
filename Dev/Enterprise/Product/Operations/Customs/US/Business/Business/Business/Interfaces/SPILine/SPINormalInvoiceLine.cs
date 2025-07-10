using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class SPINormalInvoiceLine : SPILine
	{
		public SPINormalInvoiceLine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory, invoiceLine.EffectiveDateForDutyRate, invoiceLine.US_UC_NKCountryOfOrigin, SPILine.New(invoiceLine.ParentTariffLine), invoiceLine.US_UC_NKCountryOfExport)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override ZString ImportTariffCodeCore
		{
			get
			{
				var result = invoiceLine.JI_Tariff;
				if (invoiceLine.IsCombinedLine())
				{
					if (invoiceLine.IsParentLine)
					{
						if (result.IsEmpty)
						{
							var normalTariffLine = invoiceLine.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine());
							if (normalTariffLine != null)
							{
								result = normalTariffLine.JI_Tariff;
							}
						}
					}
					else
					{
						result = ZString.Empty;
					}
				}

				return result;
			}
		}

		protected override USCTariff ImportTariffCore
		{
			get
			{
				var result = invoiceLine.ImportTariff;
				if (invoiceLine.IsCombinedLine())
				{
					if (invoiceLine.IsParentLine)
					{
						if (result == null)
						{
							var normalTariffLine = invoiceLine.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine());
							if (normalTariffLine != null)
							{
								result = normalTariffLine.ImportTariff;
							}
						}
					}
					else
					{
						result = null;
					}
				}

				return result;
			}
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (invoiceLine.HasEmptySupTariff)
				{
					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						yield return new SPINormalInvoiceLine(secondaryLine);
					}
				}
			}
		}
	}
}

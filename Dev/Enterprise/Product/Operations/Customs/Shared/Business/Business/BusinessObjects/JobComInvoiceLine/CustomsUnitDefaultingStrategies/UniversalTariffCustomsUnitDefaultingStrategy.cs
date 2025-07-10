using System;
using CargoWise.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class UniversalTariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine> : TariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine>
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public UniversalTariffCustomsUnitDefaultingStrategy(bool defaultFirstUnitOnly = false)
			: this((invoiceLine) => invoiceLine.UniversalTariff, defaultFirstUnitOnly)
		{
		}

		public UniversalTariffCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, TariffView> getUniversalTariff, bool defaultFirstUnitOnly = false)
			: base(getUniversalTariff, defaultFirstUnitOnly)
		{
		}

		public UniversalTariffCustomsUnitDefaultingStrategy((bool defaultFirstUnit, bool defaultSecondUnit, bool defaultThirdUnit, bool defaultFourthUnit, bool defaultFifthUnit) defaultUnits)
			: this((invoiceLine) => invoiceLine.UniversalTariff, defaultUnits)
		{
		}

		public UniversalTariffCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, TariffView> getUniversalTariff, (bool defaultFirstUnit, bool defaultSecondUnit, bool defaultThirdUnit, bool defaultFourthUnit, bool defaultFifthUnit) defaultUnits)
			: base(getUniversalTariff, defaultUnits)
		{
		}

		public override void DefaultUOMs(TJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.UseUniversalTariff)
			{
				base.DefaultUOMs(invoiceLine);
			}
			else
			{
				ErrorReporter.ReportOnce("The UniversalTariffCustomsUnitDefaultingStrategy cannot be used with a JobComInvoiceLine where UseUniversalTariff is set to False.");
			}
		}
	}
}

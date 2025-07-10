using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolDataCalculatorForPortTest : ConsolDataCalculator
	{
		public ConsolDataCalculatorForPortTest(ForwardingConsol consol)
			: base(consol, "")
		{ }

		public bool IsPortInTheCountryExposed(RefUNLOCO port)
		{
			return IsPortInTheCountry(port);
		}

		protected override ZString GetConsolTransportMode()
		{
			throw new NotImplementedException();
		}
	}
}

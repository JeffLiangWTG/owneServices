using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentExRateSource : IExchangeRateSource
	{
		public AgencyShipmentExRateSource(AgencyShipment shipment, JobVoyage voyage)
		{
			Argument.NotNull(shipment, nameof(shipment));
			Argument.NotNull(voyage, nameof(voyage));

			this.shipment = shipment;
			this.voyage = voyage;
		}

		readonly AgencyShipment shipment;
		readonly JobVoyage voyage;

		ControllerID IExchangeRateSource.SourceController => ((IExchangeRateSource)voyage).SourceController;

		ZGuid IExchangeRateSourceBase.SourcePK => ((IExchangeRateSource)voyage).SourcePK;

		ZString IExchangeRateSourceBase.Description => ((IExchangeRateSource)voyage).Description;

		IEnumerator<IExchangeRate> IEnumerable<IExchangeRate>.GetEnumerator()
		{
			var currencyDict = new SortedDictionary<ZString, IExchangeRate>();

			var voyageExRates = voyage.ExRates.Cast<VoyageExRate>().ToArray();
			var exRatePort = ExRatePort;

			foreach (var exRate in voyageExRates.Where(r => r.E8_RL_NKPort == exRatePort))
			{
				currencyDict.Add(exRate.E8_RX_NKExCurrency, exRate);
			}

			foreach (var exRate in voyageExRates.Where(r => exRatePort.IsEmpty
				? !r.E8_RL_NKPort.IsEmpty
				: r.E8_RL_NKPort.IsEmpty))
			{
				if (!currencyDict.ContainsKey(exRate.E8_RX_NKExCurrency))
				{
					currencyDict.Add(exRate.E8_RX_NKExCurrency, exRate);
				}
			}

			return currencyDict.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IExchangeRateSource)this).GetEnumerator();
		}

		ZDecimal? IExchangeRateSourceBase.GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger)
		{
			return currencyCode.IsEmpty ? null : this.FirstOrDefault(r => r.CurrencyCode == currencyCode)?.Rate;
		}

		ZString ExRatePort =>
			shipment.JS_INCO == Core.Constants.DomesticPaymentTerms.Prepaid
				? shipment.JS_RL_NKOrigin
				: shipment.JS_INCO == Core.Constants.DomesticPaymentTerms.Collect
					? shipment.JS_RL_NKDestination
					: ZString.Empty;
	}
}

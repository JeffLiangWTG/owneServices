using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IN5135Declaration : IBriefCusDeclaration
	{
		IDutyTaxFee DutyTaxFee { get; }

		IEnumerable<IN5135GoodsShipment> GoodsShipments { get; }

		IPartyDetails Importer { get; }
	}

	public interface IN5135GoodsShipment : IBCDGoodsShipment
	{
		ZDecimal InvoiceAmount { get; }

		ZString TaxFeeDeclared { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IPartyDetails Supplier { get; }
	}
}

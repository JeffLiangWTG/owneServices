using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX5105Declaration
	{
		ZDate AcceptanceDateTime { get; }

		ZString Authentication { get; }

		ZString ID { get; }

		[DecimalPlaces(2)]
		ZDecimal InvoiceAmount { get; }

		[DecimalPlaces(6)]
		ZDecimal TotalGrossMassMeasure { get; }

		ZInt TotalPackageQuantity { get; }

		ZString AssociatedGovernmentProcedureCode { get; }

		ZString CombinedNote { get; }

		ZString TypeCode { get; }

		IAdditionalInformation AdditionalInformation { get; }

		IPartyDetails Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IDutyTaxFee DutyTaxFee { get; }

		IGoodsShipment GoodsShipment { get; }

		IEnumerable<ZString> GovernmentProcedureDescriptions { get; }

		IPartyDetails Importer { get; }

		IDeclarationPackaging Packaging { get; }

		ZString RepresentativePersonName { get; }

		IEnumerable<IApplication> Applications { get; }
	}
}

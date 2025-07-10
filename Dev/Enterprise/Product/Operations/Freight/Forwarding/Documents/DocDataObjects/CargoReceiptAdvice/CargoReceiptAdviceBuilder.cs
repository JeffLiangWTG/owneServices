using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CargoReceiptAdviceBuilder
	{
		public CargoReceiptAdviceBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;

		public CargoReceiptAdvice Build()
		{
			var cargoReceiptAdvice = new CargoReceiptAdvice(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			var hirReference = shipment.Numbers.Cast<CusEntryNumber>()
				.FirstOrDefault(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.eHubInterchangeReference.HIR && x.CE_EntryNum.StartsWith("SHP"));
			cargoReceiptAdvice.HIRReference = hirReference?.CE_EntryNum ?? ZString.Empty;

			cargoReceiptAdvice.MarksAndNumbers = shipment.JS_MarksAndNumbers;

			cargoReceiptAdvice.InterimReceipt = shipment.JS_InterimReceipt;
			cargoReceiptAdvice.InterimReceiptDate = shipment.JS_A_RCV;

			PopulateAddresses(cargoReceiptAdvice);
			PopulatePackingLines(cargoReceiptAdvice);

			return cargoReceiptAdvice;
		}

		#region Implementation

		#region Organizations

		void PopulateAddresses(CargoReceiptAdvice cargoReceiptAdvice)
		{
			cargoReceiptAdvice.BookingParty = AddressBuilder.Create(context, shipment.BookingPartyDocumentaryAddress);
			cargoReceiptAdvice.DepartureCFSAddress = AddressBuilder.Create(context, shipment.ExportReceivingDepot);
		}

		#endregion

		#region PackingLines

		void PopulatePackingLines(CargoReceiptAdvice cargoReceiptAdvice)
		{
			cargoReceiptAdvice.PackingLines = GetPackingLines(shipment).ToArray();
		}

		IEnumerable<PackingLine> GetPackingLines(ForwardingShipment shipmentBO)
		{
			var packLineBuilder = new PackingLineBuilder();
			return new List<PackingLine>(shipmentBO.OuterPackLines.Cast<PackLine>().Select(x => packLineBuilder.Build(x)));
		}

		#endregion

		#endregion
	}
}

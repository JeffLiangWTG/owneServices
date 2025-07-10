using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business
{
	[CodeAlive("The new method is used to create wrapper")]
	public class InvoiceDocCusPackingList : DocCusPackingList
	{
		readonly JobComInvoiceHeader invoiceHeader;
		readonly IEnumerable<Transport> transports;

		protected InvoiceDocCusPackingList(CusPackingList cusPackingList, BusinessObjectFactory factoryToWrap = null) : base(cusPackingList, factoryToWrap)
		{
			invoiceHeader = PackingList.Invoice;
			transports = invoiceHeader?.Transports.Cast<Transport>() ?? Enumerable.Empty<Transport>();
		}

		public static new InvoiceDocCusPackingList New(CusPackingList cusPackingList, BusinessObjectFactory factoryToWrap)
		{
			return new InvoiceDocCusPackingList(cusPackingList, factoryToWrap);
		}

		CommercialInvoiceWrapper Invoice => invoice ??= CommercialInvoiceWrapper.New(invoiceHeader, Factory);
		CommercialInvoiceWrapper invoice;

		InvoiceTransportWrapper TransportWrapper => transportWrapper ??= new InvoiceTransportWrapper(Factory, transports);
		InvoiceTransportWrapper transportWrapper;

		protected override DocumentaryAddressDetailsWrapper GetSellerAddressDataCore() => Invoice.SellerAddressData;

		protected override DocJobDocAddress GetSellerDocAddressCore() => Invoice.SellerJobDocAddress;

		protected override DocumentaryAddressDetailsWrapper GetBuyerAddressDataCore() => Invoice.BuyerAddressData;

		protected override DocJobDocAddress GetBuyerDocAddressCore() => Invoice.BuyerJobDocAddress;

		protected override ZString GetMarksAndNumbersCore() => new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory).MarksAndNumbers;

		public override ZString PortOfOriginName => Factory.GetValue(ref portOfOriginNameCached, () => invoiceHeader.IsAttachedToPersistentDeclaration ? Invoice.Declaration.PortOfOriginName : TransportWrapper.PortOfOriginName);
		CachedProperty<ZString> portOfOriginNameCached;

		public override ZString FinalDestinationName => Factory.GetValue(ref finalDestinationNameCached, () => invoiceHeader.IsAttachedToPersistentDeclaration ? Invoice.Declaration.FinalDestinationName : TransportWrapper.FinalDestinationName);
		CachedProperty<ZString> finalDestinationNameCached;

		public override ZString Transportation => Factory.GetValue(ref transportationCached, () => invoiceHeader.IsAttachedToPersistentDeclaration ? Invoice.Declaration.Transportation : TransportWrapper.Transportation);
		CachedProperty<ZString> transportationCached;

		public override DocOrganisation Notify => Invoice.Declaration.Notify;

		public override IPartyDetails NotifyPartyDetails => Invoice.Declaration.NotifyPartyDetails;
	}
}

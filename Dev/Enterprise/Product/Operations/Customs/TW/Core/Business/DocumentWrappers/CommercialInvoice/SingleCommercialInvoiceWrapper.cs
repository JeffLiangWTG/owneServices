using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business
{
	[CodeAlive("The new method is used to create wrapper")]
	public class SingleCommercialInvoiceWrapper : CommercialInvoiceWrapper
	{
		SingleCommercialInvoiceWrapper(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
			: base(invoiceHeader, factory)
		{
			transports = invoiceHeaderBO.Transports.Cast<Transport>();
		}

		public new static SingleCommercialInvoiceWrapper New(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			return new SingleCommercialInvoiceWrapper(invoiceHeader, factoryToWrap);
		}

		readonly IEnumerable<Transport> transports;

		InvoiceTransportWrapper TransportWrapper => transportWrapper ??= new InvoiceTransportWrapper(Factory, transports);
		InvoiceTransportWrapper transportWrapper;

		protected override ZString GetTransportationCore() => TransportWrapper.Transportation;

		protected override ZString GetPortOfOriginNameCore() => TransportWrapper.PortOfOriginName;

		protected override ZString GetFinalDestinationNameCore() => TransportWrapper.FinalDestinationName;
	}
}

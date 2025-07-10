using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class InvoiceLineInformationForTest : IInvoiceLineInformation
	{
		public ZShort InvoiceLineNumber { get; set; }

		public ZShort RelatedDeclarationLineNumber { get; set; }

		public ZString ProductCode { get; set; }

		public ZDecimal Quantity { get; set; }

		public ZString QuantityUnit { get; set; }

		public ZDecimal PriceDetails { get; set; }

		public ZDecimal ItemAmount { get; set; }

		public ZDecimal RateDetails { get; set; }

		public ZString BrandName { get; set; }

		public ZString CommercialInvoiceItemDescription { get; set; }

		public IEnumerable<IInvoiceLineChargeInformation> InvoiceLineChargeInformations { get; set; }
	}
}

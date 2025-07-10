using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base
{
	internal class CertificateOfOriginBuilderFlags
	{
		public ContactValidation ValidateProducerPhone { get; set; } = ContactValidation.Never;
		public ContactValidation ValidateProducerEmail { get; set; } = ContactValidation.Never;
		public AddressValidation ValidateProducerAddress { get; set; } = AddressValidation.Full;

		public bool ValidateConsignorPhone { get; set; }
		public bool ValidateConsignorEmail { get; set; }

		public ContactValidation ValidateConsigneePhone { get; set; } = ContactValidation.Always;
		public ContactValidation ValidateConsigneeEmail { get; set; } = ContactValidation.Always;
		public bool ValidateConsigneeAddress { get; set; } = true;

		public bool ValidatePortOfLoading { get; set; }
		public bool ValidatePortOfDischarge { get; set; }
		public bool ValidatePortOfOrigin { get; set; }
		public bool ValidatePortOfDestination { get; set; } = true;

		public bool ValidateTransportReference { get; set; }

		public bool ValidateDepartureDate { get; set; }
		public bool ValidateArrivalDate { get; set; }

		public InvoiceType InvoiceType { get; set; } = InvoiceType.Short;

		public bool ValidateSignature { get; set; } = true;

		public bool ValidateLineItemOrigin { get; set; }
		public bool ValidateLineHarmonizedCode { get; set; } = true;
		public bool ValidateLineItemOriginCriterion { get; set; } = true;
		public bool ValidateLineItemItemNumber { get; set; } = true;
		public bool ValidateLineItemWeight {  get; set; } = true;
		public bool ValidateLineItemMarksAndNumbers { get; set; } = true;

		public bool ValidateCurrentUserCity { get; set; } = true;
	}

	internal enum AddressValidation { Full, NameOnly, Complex, None, FullIfDifferentFromConsignor }

	internal enum ContactValidation { Always, Never, IfAddressSet }

	public enum LineItemSource { PackLine, Invoice }
}

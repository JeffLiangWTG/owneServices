using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsShipmentSupportingDocumentProvider : ISupportingDocument
{
	public AESGoodsShipmentSupportingDocumentProvider(SupportingDocument document, int sequenceNumber)
	{
		this.document = Argument.NotNull(document, nameof(document));
		this.sequenceNumber = sequenceNumber;
	}

	readonly SupportingDocument document;
	readonly int sequenceNumber;

	public int? DocumentLineItemNumber => MessageProviderHelper.ReturnNullIfEmpty(document.CSI_ItemNumber);

	public string IssuingAuthorityName => MessageProviderHelper.ReturnNullIfEmpty(document.CSI_AdditionalDescription);

	public DateTime? ValidityDateValue => MessageProviderHelper.ReturnNullIfInvalid(document.CSI_DateOfExpiry);

	public string MeasurementUnitAndQualifier => null;

	public decimal? QuantityValue => null;

	public string Currency => null;

	public decimal? AmountValue => null;

	public int SequenceNumber => sequenceNumber;

	public string Type => document.CSI_Code;

	public string Description => document.CSI_ReferenceNumber;
}

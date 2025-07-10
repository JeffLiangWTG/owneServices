using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class SupportingDocumentWrapper : ISupportingDocument
{
	public SupportingDocumentWrapper(SupportingDocument supportingDocument, int sequenceNumeric)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly SupportingDocument supportingDocument;

	public string ExpirationDateTime => supportingDocument.CSI_DateOfExpiry.ToString("yyyyMMdd");

	public string CCQualifierCode => null;

	public int? LineNumericValue => supportingDocument.CSI_ItemNumber != 0 ? supportingDocument.CSI_ItemNumber : null;

	public string Submitter => supportingDocument.CSI_AdditionalDescription;

	public string Id => supportingDocument.CSI_ReferenceNumber;

	public string Code => supportingDocument.CSI_Code;

	public IWriteOff WriteOff => supportingDocument.CSI_Quantity.IsEmpty && supportingDocument.CSI_Value.IsEmpty ? null : new WriteOffWrapper(supportingDocument);

	public int SequenceNumeric { get; }
}

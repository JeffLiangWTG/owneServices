using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class ReleaseAdditionalDocumentWrapper : NonPersistentBusinessObject, ICusSupportingDocument
{
	readonly CusSupportingInfo document;

	public ReleaseAdditionalDocumentWrapper(CusSupportingInfo document) : base((document as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		this.document = Argument.NotNull(document, nameof(document));
	}

	public ZString Type => document.CSI_Code;

	public ZString Reference => document.CSI_ReferenceNumber;

	public ZDecimal? Quantity => document.CSI_Quantity;

	public ZDecimal? Value => document.CSI_Value;
}

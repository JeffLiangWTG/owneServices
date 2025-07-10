using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSupportingDocument : DocBaseWrapper
{
	NODocSupportingDocument(SupportingDocument supportingDocument, BusinessObjectFactory factoryToWrap) : base(supportingDocument, factoryToWrap)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}

	readonly SupportingDocument supportingDocument;

	public static NODocSupportingDocument New(SupportingDocument supportingDocument, BusinessObjectFactory factoryToWrap) => supportingDocument == null ? null : new NODocSupportingDocument(supportingDocument, factoryToWrap);

	public ZString ReferenceNumber => supportingDocument.CSI_ReferenceNumber;

	public ZString ReferenceType => supportingDocument.CSI_Code;
}

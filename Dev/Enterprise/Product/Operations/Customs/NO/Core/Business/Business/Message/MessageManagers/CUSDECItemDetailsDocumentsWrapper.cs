using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECItemDetailsDocumentsWrapper(SupportingDocument document) : ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments
{
	SupportingDocument Document { get; } = Argument.NotNull(document, nameof(document));

	ZString ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments.DocumentCode => Document.CSI_Code;

	ZString ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments.DocumentNumberOrText => Document.CSI_ReferenceNumber;
}

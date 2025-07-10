using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationDocumentWrapper))]
	sealed class ExportCustomsDeclarationDocumentWrapperTest : ExportCustomsDeclarationDocumentWrapperAbstractTest<ExportCustomsDeclarationDocumentWrapper>
	{
		protected override ExportCustomsDeclarationDocumentWrapper GetExportCustomsDeclarationDocumentWrapper(CusEntryHeader cusEntryHeader, BusinessObjectFactory factory) => ExportCustomsDeclarationDocumentWrapper.New(cusEntryHeader, factory);
	}
}

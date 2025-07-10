using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DocumentProvider))]
sealed class DocumentProviderBaseOnlyTest : DocumentProviderAbstractTest<DocumentProvider>
{
	protected override string DocType => CusSupportingInfoTypeList.Codes.AdditionalInfo;
}

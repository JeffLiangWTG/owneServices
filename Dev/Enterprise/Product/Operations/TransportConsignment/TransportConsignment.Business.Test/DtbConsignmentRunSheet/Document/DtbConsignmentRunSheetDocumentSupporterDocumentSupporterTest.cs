using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetDocumentSupporter))]
	sealed class DtbConsignmentRunSheetDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<DtbConsignmentRunSheet>();
		}
	}
}

using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDocTypeDocumentSupporter))]
	sealed class RefDocTypeDocumentSupporterTest : DocumentSupporterTest
	{
		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<RefDocType>();
		}

		#endregion
	}
}

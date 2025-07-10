using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(DocumentRequestHelper))]
	[HttpContextEnabledTest]
	sealed class DocumentRequestHandlerTest : DocumentRequestHandlerTestCase<DocumentRequestHelper>
	{
		protected override DocumentRequestHandler<DocumentRequestHelper> GetDocumentRequestHandler()
		{
			return new DocumentRequestHandler();
		}
	}
}

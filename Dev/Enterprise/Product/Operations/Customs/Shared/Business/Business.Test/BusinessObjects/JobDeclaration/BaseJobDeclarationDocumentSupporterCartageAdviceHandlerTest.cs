using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.TransportBookings.Shared.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			var dec = BaseJobDeclaration.New(Factory);
			return dec;
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			return ((BaseJobDeclarationDocumentSupporter)documentSupporter).Bookings;
		}

		protected override bool ChecksChildMenuItem => true;
		protected override bool DocumentSupportablesUseDifferentFactory => true;
	}
}

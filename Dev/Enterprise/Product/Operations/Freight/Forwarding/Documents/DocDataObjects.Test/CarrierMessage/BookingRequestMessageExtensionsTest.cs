using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ShippingInstruction
{
	class BookingRequestMessageExtensionsTest : OceanBookingMessagingValidationTest
	{
		protected override string DocumentName => ConsolDocumentNames.BookingRequest;

		protected override ZString DocumentDataStoreName => ConsolDocumentDataStoreNames.SeaBookingRequest2;

		protected override ZBool ShouldPopulateBookingRequestRequiredErrorMessage => true;

		protected override IMessagingExtensions GetMessageExtension(IDocument document, ForwardingConsol consol)
		{
			var messageInstructions = new Mock<IMessageInstructions>();
			return new DocDataObjects.CarrierMessagingExtensions(document, consol, messageInstructions.Object);
		}

		protected override Mock<IDocument> MockDocument(ForwardingConsol consol)
		{
			var data = new CarrierMessageData(nameof(ForwardingConsol), consol.CarrierShipperReferenceWithFallback, DataContext.BookingRequest);
			var dynamicData = new Mock<IDynamicData>();
			dynamicData.SetupGet(d => d.Value).Returns(data);
			var document = new Mock<IDocument>();
			document.Setup(d => d.Data).Returns(dynamicData.Object);
			return document;
		}

		protected override void SetShippingLineAvailableForMessaging(RefShippingLine shippingLine)
			=> shippingLine.RSL_BookingRequestAvailable = true;
	}
}

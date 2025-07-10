using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	class ShippingOrderExtensionsTest : OceanBookingMessagingValidationTest
	{
		protected override string DocumentName => ConsolDocumentNames.ShippingOrder;

		protected override ZString DocumentDataStoreName => ConsolDocumentDataStoreNames.ShippingOrder;

		protected override ZBool ShouldPopulateBookingRequestRequiredErrorMessage => true;

		protected override IMessagingExtensions GetMessageExtension(IDocument document, ForwardingConsol consol)
		{
			var messageInstructions = new Mock<IMessageInstructions>();
			return new ShippingOrderMessagingExtensions(document, consol, messageInstructions.Object);
		}

		protected override Mock<IDocument> MockDocument(ForwardingConsol consol)
		{
			var data = new ShippingOrder(
					nameof(ForwardingConsol),
					consol.CarrierShipperReferenceWithFallback,
					DataContext.ShippingOrder);
			var dynamicData = new Mock<IDynamicData>();
			dynamicData.SetupGet(d => d.Value).Returns(data);
			var document = new Mock<IDocument>();
			document.Setup(d => d.Data).Returns(dynamicData.Object);
			return document;
		}

		protected override void SetShippingLineAvailableForMessaging(RefShippingLine shippingLine)
			=> shippingLine.RSL_ShippingOrderAvailable = true;
	}
}

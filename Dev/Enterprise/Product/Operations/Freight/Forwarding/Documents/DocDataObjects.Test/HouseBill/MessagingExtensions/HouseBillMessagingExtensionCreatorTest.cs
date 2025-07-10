using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBillMessagingExtensionCreatorTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBill("zzz", "zzz");

			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(houseBill);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(d => d.DocumentName).Returns("Bill Of Lading");

			AssertType<DefaultHouseBillMessagingExtension>(HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object));

			shipment.JS_HouseBillOfLadingType = Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL;
			AssertType<FIATAHouseBillMessagingExtension>(HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object));

			houseBill.IsDraft = true;
			dynamicData.SetupGet(d => d.Value).Returns(houseBill);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			AssertType<CarrierHouseBillMessagingExtension>(HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object));
		}
	}
}

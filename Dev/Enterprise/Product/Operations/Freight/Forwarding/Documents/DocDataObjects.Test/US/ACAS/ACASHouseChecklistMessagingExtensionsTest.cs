using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class ACASHouseChecklistMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestGetMessageStatus()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Logs.AddNew(
				Events.MessagePendingProcessing,
				new ZDateTimeOffset(2019, 2, 1),
				new KeyValuePair<string, string>("MST", DocumentNames.AdvancedManifest),
				new KeyValuePair<string, string>("LOC", Core.Constants.CountryCodes.UnitedStates));

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(acas);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new ACASHouseChecklistMessagingExtensions(document.Object);
			var status = extensions.GetMessageStatus();

			AssertEquals("Message Pending Processing by Customs because Security Filing Received - Assessment in Progress.", status);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}
	}
}

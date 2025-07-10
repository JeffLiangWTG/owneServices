using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class FIATAHouseBillMessagingExtensionTest : TestCaseWithFactory
	{
		public void TestIsSendingAmendment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var messageInstructions = new Mock<IMessageInstructions>();

			AssertEquals(null, new FIATAHouseBillMessagingExtension(shipment, messageInstructions.Object).IsSendingAmendment());
		}

		public void TestGetMessageStatus()
		{
			AssertMessageStatus(Events.MessageSent, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);
			AssertMessageStatus(Events.MessageRejected, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);
		}

		public void TestGetMessageStatus_AdditionalEventCodes()
		{
			AssertMessageStatus(Events.Authorised, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type);
			AssertMessageStatus(Events.AuthorisationRejected, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type);
		}

		void AssertMessageStatus(Event @event, string eventReferenceParameterCode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBillOfLadingType = "FIA";

			var incoTermDefinitions = CreateIncoTermChargeCodesCollection();

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			{
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "COPY"
				};

				var builder = new HouseBillBuilder(shipment, parameters);
				var data = builder.Build();

				var documentData = CreateDocumentData(shipment) as IStmALogProvider;

				CreateLog(documentData, @event, new ZDateTime(2022, 06, 01), new KeyValuePair<string, string>(eventReferenceParameterCode, ShipmentDocumentNames.BillOfLading));

				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var messageInstructions = new Mock<IMessageInstructions>();

				dynamicData.SetupGet(d => d.Value).Returns(data);
				document.SetupGet(d => d.Data).Returns(dynamicData.Object);
				messageInstructions.Setup(m => m.DocumentName).Returns(ShipmentDocumentNames.BillOfLading);

				var extensions = HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object);
				AssertType<FIATAHouseBillMessagingExtension>(extensions);

				var authorizationRelatedEventCodes = new List<string>() { Events.AuthorisedCode, Events.AuthorisationRejectedCode };
				if (authorizationRelatedEventCodes.Contains(@event.Code))
				{
					Assert(!extensions.GetMessageStatus().Trim().Contains(@event.Description.Trim()));
					CreateLog(documentData, @event, new ZDateTime(2022, 06, 01),
						new KeyValuePair<string, string>(eventReferenceParameterCode, ShipmentDocumentNames.BillOfLading),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DocDataConstants.Department.FIATA));
				}

				Assert("Message Status", extensions.GetMessageStatus().Trim().Contains(@event.Description.Trim()));
			}
		}

		#region Implementation

		IVisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.BillOfLading;

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		IncoTermChargeCodesCollection CreateIncoTermChargeCodesCollection()
		{
			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.Origin = PaymentParty.Consignee;
			def.Loading = PaymentParty.Consignee;
			def.Freight = PaymentParty.Consignee;
			def.Insurance = PaymentParty.Consignee;
			def.Unloading = PaymentParty.Consignee;
			def.Destination = PaymentParty.Consignee;
			def.Brokerage = PaymentParty.Consignee;
			def.CustomsDuty = PaymentParty.Consignee;
			def.OriginBrokerage = PaymentParty.Consignee;
			def.IncoTerm = IncoTerms.CostAndFreight;

			return incoTermDefinitions;
		}

		#endregion
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ILGPMResetToOriginalMessageCommandTest : ILResetToOriginalMessageCommandBaseTest
	{
		public void TestInvoke()
		{
			const string expectedMsg = @"WARNING: Using this option without checking with Customs first might result in duplicate messages being processed by Customs. 
Resetting to Original should only be required when previous Cargo Movement was canceled or there is a serious messaging failure at the Customs end.
In the normal course of events, every message you send should be responded to, so the system knows what kind of message to send automatically. 
Before using this option, you should always check with Customs to make sure they have not already processed the message.";
			const string expectedCaption = "Warning";
			const string expectedPrompt = "If you have done so, please type the following to confirm:";
			const string expectedConformationStr = $"I confirm resetting to original and ignore the previously sent messages.";

			var hardRefreshCalled = false;
			var resetToOriginalCalled = false;

			(var documentInfo, var notificationService, var broker) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			shipment.JS_GMN = "1000003";

			var disposableResetToOriginal = broker.GetEvent<ResetToOriginalEvent>().Subscribe(_ =>
			{
				resetToOriginalCalled = true;
			});
			var disposableDocumentHardRefresh = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
			{
				hardRefreshCalled = true;
			});
			try
			{
				command.NotifyDocumentInfoCreated(documentInfo.Object);
				shipment.Factory.Save();
				var result = command.Invoke();

				CombineAssertions("when CheckBeforeInvoke return false", () =>
				{
					AssertEquals("result", false, result);
					notificationService.Verify(s => s.ShowMessage("Due to changes in the state of the entity, please reopen the form before resetting to original.", GetDocumentName()), Times.Once);
				});

				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-705));
				shipment.Factory.Save();
				result = command.Invoke();

				var shipmentSTULog = GetLastLog(shipment);
				CombineAssertions("When Reset To Original invoke", () =>
				{
					Assert("Hard refresh should be called, Due we want that all the screen will refresh", hardRefreshCalled);
					Assert("Reset To Original should be called", resetToOriginalCalled);
					Assert("reset to original has been done", result);

					notificationService.Verify(nf => nf.ShowConfirmation(expectedMsg, expectedCaption, expectedPrompt, expectedConformationStr), Times.Once);
					AssertNullOrEmpty("JS_GMN is reset", shipment.JS_GMN);

					var newFactory = new BusinessObjectFactory();
					var zQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, shipment.PK);
					zQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, "GMN");
					zQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, "IL");

					var cusEntryNumberGMN = newFactory.LoadTop1<CusEntryNumber>(zQuery);
					AssertNullOrEmpty("GMN is reset", cusEntryNumberGMN.CE_EntryNum);

					AssertEquals("shipment data logs", "STU Propagated: All Document Data|DEP=CargoWise Support|MST=Gatepass Movement|TYP=Reset To Original",
					shipmentSTULog);
				});
			}
			finally
			{
				disposableResetToOriginal.Dispose();
				disposableDocumentHardRefresh.Dispose();
			}
		}

		protected override ILCustomCommandBase CreateCommand(ForwardingShipment shipment)
			=> new ILGPMResetToOriginalMessageCommand(shipment?.GatePassMovementProvider);

		protected override DocDataObject GetMessageDocDataObject(ForwardingShipment shipment)
			=> new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

		protected override string GetDocumentName() => "Gatepass Movement";

		protected override void UpdateMessageReference(string reference)
		{
			shipment.JS_GMN = reference;
		}

		protected override string GetDataContext() => "ILGatePassMovement";

		protected override ZString MessageReference => shipment.JS_GMN;
	}
}

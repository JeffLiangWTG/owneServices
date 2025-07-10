using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingConsolDocumentSupporterGuiQueryProviderTest : TestCaseWithFactory
	{
		public void TestPrintFinalMasterHandlesDuplicateExRatesCausedByConcurrencyError()
		{
			var creator = new TestObjectCreator(Factory);

			var consol = Factory.New<ForwardingConsolForAWBPrintingTest>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_UniqueConsignRef = "C001";

			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment, false);
			Factory.Save();

			var factorySaveFinishedWhilePrinting = false;
			consol.UpdateAWBPrintedCoreImplementation = () =>
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var jobInOtherFactory = otherFactory.Load<Job>(job.PK);
				var rateInOtherFactory = jobInOtherFactory.ExchangeRates.AddRate(creator.USD, 5, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);
				rateInOtherFactory.JF_IsTransformed = true;

				var rate = job.ExchangeRates.AddRate(creator.USD, 5, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);
				rate.JF_IsTransformed = true;

				otherFactory.Save();
				Factory.Save();
				factorySaveFinishedWhilePrinting = true;
			};

			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			AssertNoExceptionThrown("Duplicate Exchange rate created in other factory does not cause unhandled exception when printing final master", () => queryProvider.PrintFinalMaster(consol, "AWB"));
			Assert("Exception was thrown by factory save, but caught and handled", !factorySaveFinishedWhilePrinting);
		}

		public void TestRegister()
		{
			IForwardingConsolDocumentSupporterQueryProvider provider = Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
			AssertNull("prerequisite", provider);

			ForwardingConsolDocumentSupporterGuiQueryProvider.Register(Factory);

			provider = Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();

			AssertNotNull(provider);
			Assert(provider is ForwardingConsolDocumentSupporterGuiQueryProvider);
		}

		public void TestGetDeliveryAgentsToPrint()
		{
			DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom = new DeliveryAgentToSelectFromForPrintingCollection(Factory);

			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();
			DeliveryAgentOrgHeader[] deliveryAgents = queryProvider.GetDeliveryAgentsToPrint(deliveryAgentsToSelectFrom);

			AssertNull(deliveryAgents);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("There are no Delivery Agents for this Consol.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Delivery Agents", UnitTestUserNotification.Instance.LastMessage.Caption);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryAgentToSelectFromForPrinting agent1 = deliveryAgentsToSelectFrom.AddNew();

			deliveryAgents = queryProvider.GetDeliveryAgentsToPrint(deliveryAgentsToSelectFrom);
			AssertContainsExactElementsInAnyOrder(new[] { Factory.Load<DeliveryAgentOrgHeader>(agent1.PK) }, deliveryAgents);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			DeliveryAgentToSelectFromForPrinting agent2 = deliveryAgentsToSelectFrom.AddNew();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			deliveryAgents = queryProvider.GetDeliveryAgentsToPrint(deliveryAgentsToSelectFrom);

			AssertNull(deliveryAgents);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(typeof(DocumentDeliveryAgentsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			deliveryAgents = queryProvider.GetDeliveryAgentsToPrint(deliveryAgentsToSelectFrom);

			AssertContainsExactElementsInAnyOrder(new[] { Factory.Load<DeliveryAgentOrgHeader>(agent1.PK), Factory.Load<DeliveryAgentOrgHeader>(agent2.PK) }, deliveryAgents);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(typeof(DocumentDeliveryAgentsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDataStateErrorMessageShouldNotBeEmptyWhenDeliveryAgentsToPrintIsNull()
		{
			ForwardingConsolDocumentSupporterGuiQueryProvider.Register(Factory);
			var provider = Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
			AssertNotNull(provider);
			Assert(provider is ForwardingConsolDocumentSupporterGuiQueryProvider);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Delivery Agent Pack";

			var consol = Factory.New<ForwardingConsol>();

			var dataState = consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(!dataState.IsValid);
			AssertEquals("dataState error message should not be empty when IsValid is false.", "No Delivery Agents selected for printing.", dataState.ErrorMessage);
		}

		public void TestGetImportCargoLabelToPrint()
		{
			DocumentImportCargoLabel importCargoLabel = new DocumentImportCargoLabel(Factory.New<ForwardingConsol>());
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(null, queryProvider.GetImportCargoLabelToPrint(importCargoLabel));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(importCargoLabel, queryProvider.GetImportCargoLabelToPrint(importCargoLabel));
		}

		public void TestPrintAWBLabels()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();
			queryProvider.PrintAWBLabels(consol);

			ConsolAWBActions actions = ZFormModaliser.LastIBusinessShownOnDialogForTest as ConsolAWBActions;
			AssertNotNull(actions);
			AssertEquals(typeof(LabelRangeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(consol, typeof(ConsolAWBActions).GetField("consol", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(actions));
			AssertEquals(AWBActions.ActionsModeType.LabelsOnly, typeof(ConsolAWBActions).GetField("ActionsMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(actions));
		}

		public void TestPrintFinalMaster()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();
			queryProvider.PrintFinalMaster(consol, "AWB");

			ConsolAWBActions actions = ZFormModaliser.LastIBusinessShownOnDialogForTest as ConsolAWBActions;
			AssertNotNull(actions);
			AssertEquals(typeof(AWBPrintForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(consol, typeof(ConsolAWBActions).GetField("consol", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(actions));
			AssertEquals(AWBActions.ActionsModeType.All, typeof(ConsolAWBActions).GetField("ActionsMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(actions));
		}

		public void TestPrintFinalMasterUpdateAWBPrinted()
		{
			bool updateAWBPrintedCoreImplementationFired = false;

			var consol = Factory.New<ForwardingConsolForAWBPrintingTest>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrintedCoreImplementation = () => updateAWBPrintedCoreImplementationFired = true;

			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			queryProvider.PrintFinalMaster(consol, "AWB");

			Assert(updateAWBPrintedCoreImplementationFired);
		}

		public void TestShowMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();
			queryProvider.ShowMessage("message", "title");

			AssertEquals("message", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("title", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestShowConfirmation() => CombineAssertions(() =>
		{
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("NO", false, queryProvider.ShowConfirmation("message1", "title1"));
			AssertEquals("message1", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("title1", UnitTestUserNotification.Instance.LastMessage.Caption);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("YES", true, queryProvider.ShowConfirmation("message2", "title2"));
			AssertEquals("message2", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("title2", UnitTestUserNotification.Instance.LastMessage.Caption);
		});

		#region Implementation

		public class ForwardingConsolForAWBPrintingTest : ForwardingConsol
		{
			public ForwardingConsolForAWBPrintingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Action UpdateAWBPrintedCoreImplementation { get; set; }
			protected override void UpdateAWBPrintedCore()
			{
				if (UpdateAWBPrintedCoreImplementation != null)
				{
					UpdateAWBPrintedCoreImplementation();
				}
				else
				{
					base.UpdateAWBPrintedCore();
				}
			}
		}

		#endregion
	}
}

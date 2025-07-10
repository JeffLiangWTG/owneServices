using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(QueryMessageModule))]
	sealed class QueryMessageModuleTest : MQEDIMessageModuleTest
	{
		public void TestFilterControl()
		{
			using (var module = new QueryMessageModule())
			{
				var coll = new QueryTransmitMessageCollection(Factory);
				coll.AddNew();
				using (var form = new ZForm(coll))
				{
					form.Size = new System.Drawing.Size(1024, 768);
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.Show();
					Assert(!control.FilteredGrid.Columns.Contains(MQEDIMessage.Schema.EM_ActionStatus));
					Assert(!control.FilteredGrid.Columns.Contains(MQEDIMessage.Schema.EM_Status));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendOrReceiveHumanReadable));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationCode));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationReference));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageNum));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubTypeDescription));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendingUser));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeSender));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeReceiver));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemCreateUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_DateTimeInterchangeSent));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeNumber));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeStatus));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditTimeUtc));
				}
			}
		}

		public void TestQueryManufacturerFireSaveButtonGetsCalled()
		{
			AssertClickCallZFormModaliser("Send Manufacturer Name And Address Query", typeof(ManufacturerQueryForm));
			MenuItem menuItem = null;
			ManufacturerQueryForm messageDataForm = null;
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				var messageData = new USMIDQuery(Factory);
				messageDataForm = new ManufacturerQueryForm(messageData);
				messageDataForm.IsOKToSendMessage = false;
				moduleMock.Setup(m => m.GetManufacturerQueryMessageData()).Returns(messageData);
				moduleMock.Setup(m => m.GetManufacturerQueryForm(messageData)).Returns(messageDataForm);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Manufacturer Name And Address Query");
			}

			menuItem.PerformClick();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			moduleMock.VerifyAll();
		}

		public void TestQueryManufacturerWhenEverythingIsOK()
		{
			AssertClickCallZFormModaliser("Send Manufacturer Name And Address Query", typeof(ManufacturerQueryForm));
			MenuItem menuItem = null;
			ManufacturerQueryForm messageDataForm = null;
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				var messageData = new USMIDQuery(Factory);
				messageDataForm = new ManufacturerQueryForm(messageData);
				messageDataForm.IsOKToSendMessage = true;
				moduleMock.Setup(m => m.GetManufacturerQueryMessageData()).Returns(messageData);
				moduleMock.Setup(m => m.GetManufacturerQueryForm(messageData)).Returns(messageDataForm);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Manufacturer Name And Address Query");
			}

			menuItem.PerformClick();

			AssertEquals("Manufacturer Name And Address Query sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No data is saved to CusAddInfo", 0, Factory.Load<Customs.Business.MultiLineAddInfos.CusAddInfo>(new ZQuery()).Length);
			messageDataForm.Dispose();
			moduleMock.VerifyAll();
		}

		public void TestSendImporterBondQuery_Click()
		{
			AssertClickCallZFormModaliser("Send Importer Bond Query", typeof(QueryImporterBondForm));
			MenuItem menuItem = null;
			QueryImporterBondForm messageDataForm = null;
			var number = "22-2222222**";
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				messageDataForm = new QueryImporterBondForm("");
				messageDataForm.NumberTextBox.ReadOnly = false;
				messageDataForm.NumberTextBox.Text = number;
				messageDataForm.IsOKToSendMessage = true;
				moduleMock.Setup(m => m.GetQueryImporterBondForm("")).Returns(messageDataForm);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Importer Bond Query");
			}

			menuItem.PerformClick();

			AssertEquals("A request has been sent for importer number : " + number + ". You will receive a response email shortly.", UnitTestUserNotification.Instance.LastMessage.Text);
			moduleMock.VerifyAll();
		}

		public void TestSendImporterBondQuery_DoesNotHavePermissionToSendSSN()
		{
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertClickCallZFormModaliser("Send Importer Bond Query", typeof(QueryImporterBondForm));
			MenuItem menuItem = null;
			QueryImporterBondForm messageDataForm = null;
			var number = "123-12-1234";
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				messageDataForm = new QueryImporterBondForm("");
				messageDataForm.NumberTextBox.ReadOnly = false;
				messageDataForm.NumberTextBox.Text = number;
				messageDataForm.IsOKToSendMessage = true;
				moduleMock.Setup(m => m.GetQueryImporterBondForm("")).Returns(messageDataForm);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Importer Bond Query");
			}

			menuItem.PerformClick();

			AssertEquals(SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			moduleMock.VerifyAll();
		}

		public void TestSendCargoManifestQuery_Click()
		{
			AssertClickCallZFormModaliser("Send Cargo Manifest Query", typeof(CargoManifestQueryGridForm));
			MenuItem menuItem = null;
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				var header = new CargoManifestQueryHeader(Factory);
				var obj1 = header.SendingObjects.AddNew();
				var obj2 = header.SendingObjects.AddNew();
				moduleMock.Setup(m => m.GetCargoManifestQueryHeader()).Returns(header);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Cargo Manifest Query");
			}

			menuItem.PerformClick();

			AssertEquals("Cargo/Manifest Query sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			moduleMock.VerifyAll();
		}

		public void TestSendEntrySummaryQuery_Click()
		{
			AssertClickCallZFormModaliser("Send Entry Summary Query", typeof(EntrySummaryQueryForm));
			MenuItem menuItem = null;
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				var bizObj = new EntrySummaryQueryBizObj(Factory);
				bizObj.SendMessage = true;
				moduleMock.Setup(m => m.GetEntrySummaryQueryBizObj()).Returns(bizObj);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Entry Summary Query");
			}

			AssertNull("Prerequirement", Factory.LoadTop1<EDIMessage>(new ZQuery()));
			menuItem.PerformClick();

			AssertEquals("Entry Summary Query sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("Message sent", message);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, message.EM_MessageType);
			moduleMock.VerifyAll();
		}

		public void TestSendEntrySummaryQueryForAce()
		{
			AssertClickCallZFormModaliser("Send Entry Summary Query", typeof(EntrySummaryQueryForm));
			MenuItem menuItem = null;
			var moduleMock = new Mock<QueryMessageModule>() { CallBase = true };
			using (var module = moduleMock.Object)
			{
				var bizObj = new EntrySummaryQueryBizObj(Factory);
				bizObj.CriteriaCode = CriteriaCodeList.Codes.PSC;
				bizObj.DateFrom = ZDateTime.Today.AddDays(-1);
				bizObj.DateTo = ZDateTime.Today;
				bizObj.SendMessage = true;
				moduleMock.Setup(m => m.GetEntrySummaryQueryBizObj()).Returns(bizObj);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText("Send Entry Summary Query");
			}

			menuItem.PerformClick();

			AssertEquals("Entry Summary Query sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			moduleMock.VerifyAll();
		}

		public void TestSendCensusWarningQueryMessage()
		{
			AssertClickCallZFormModaliser("Send Census Warning Query", typeof(CensusWarningQueryForm));
		}

		public override void TestSetToComplete()
		{
			AssertMenuItemIsNull(QueryMessageModule.SetToCompleteMenuName);
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = new QueryMessageModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.QueryMessages, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.QueryMessage;

		void AssertClickCallZFormModaliser(string menuName, Type expectedFormType)
		{
			using (var module = new QueryMessageModule())
			{
				var menuItem = module.ContextMenuExposedForTesting.FindByText("Actions").MenuItems.FindByText(menuName);
				ZFormModaliser.LastFormShownDialogForTest = null;
				menuItem.PerformClick();
				AssertEquals(expectedFormType, ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}
}

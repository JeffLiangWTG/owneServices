using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(MultiManifestBillSenderDialog))]
	class MultiManifestBillSenderDialogTest : ZFormBasherTest
	{
		public void TestButtonsClicking()
		{
			MultiManifestBillSenderTest.SetupReferenceDataForSG(Factory);
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			(AsycudaManifestHeader header1, AsycudaBill header1Bill1, AsycudaBill header1Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB3", "MB1");
			(AsycudaManifestHeader header2, AsycudaBill header2Bill1, AsycudaBill header2Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB1", "MB3");
			(AsycudaManifestHeader header3, AsycudaBill header3Bill1, AsycudaBill header3Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB2", "MB2");
			(AsycudaManifestHeader header4, AsycudaBill header4Bill1, AsycudaBill header4Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB4", "MB4");
			header3.AMA_Voyage = ZString.Empty;
			Factory.Save();
			var messageSender = new MultiManifestBillSender(new[] { header1Bill1, header1Bill2, header2Bill1, header3Bill1, header3Bill2, header4Bill1, header4Bill2 });
			var formIsClosed = false;
			Action<object, FormClosedEventArgs> formClosedAction = (object sender, FormClosedEventArgs e) => formIsClosed = true;
			using (var dialog = new MultiManifestBillSenderDialog(messageSender))
			{
				dialog.FormClosed += new FormClosedEventHandler(formClosedAction);
				dialog.Show();
				var buttonCancel = dialog.FindSingle<ZButton>(c => c.Name == "ButtonCancel");
				buttonCancel.PerformClick();
				AssertEquals(DialogResult.Cancel, dialog.DialogResult);
				AssertEquals("formIsClosed", true, formIsClosed);
				dialog.FormClosed -= new FormClosedEventHandler(formClosedAction);
			}

			formIsClosed = false;
			using (var dialog = new MultiManifestBillSenderDialogTestHelper(messageSender))
			{
				dialog.FormClosed += new FormClosedEventHandler(formClosedAction);
				dialog.Show();
				var sendButton = dialog.FindSingle<ZButton>(c => c.Name == "SendButton");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				AssertEquals(DialogResult.None, dialog.DialogResult);
				AssertEquals("formIsClosed", false, formIsClosed);
				AssertEquals("There is errors", "Please fix all the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				messageSender.CycleDate = ZDateTime.Today.AddDays(1);
				messageSender.CycleNumber = "10";
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, dialog.DialogResult);
				AssertEquals("formIsClosed", true, formIsClosed);
				AssertMultilineASCIIEquals("Send result", @"The following manifests could not be sent due to errors:
JOB2 - MB2
The following manifests were sent successfully:
JOB1 - MB3
JOB3 - MB1
The following manifests sending were aborted:
JOB4 - MB4", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertMultilineASCIIEquals("Progress Data", @"Validating manifest 'JOB1 - MB3'. - 25
Sending manifest 'JOB1 - MB3'. - 25
Validating manifest 'JOB2 - MB2'. - 50
Validating manifest 'JOB3 - MB1'. - 75
Sending manifest 'JOB3 - MB1'. - 75
Sending Message To Customs Completed - 100", dialog.ProgressData.ToStringWithNewLineBetweenAppends());
				dialog.FormClosed -= new FormClosedEventHandler(formClosedAction);
			}
		}

		class MultiManifestBillSenderDialogTestHelper : MultiManifestBillSenderDialog
		{
			public MultiManifestBillSenderDialogTestHelper(MultiManifestBillSender sender) : base(sender)
			{
			}

			public ZStringBuilder ProgressData => progressData ?? (progressData = new ZStringBuilder());
			ZStringBuilder progressData;
			protected override ProgressForm CreateProgressForm()
			{
				return new ProgressFormHelper(ProgressData);
			}
		}

		class ProgressFormHelper : ProgressForm
		{
			public ProgressFormHelper(ZStringBuilder progressData)
			{
				this.progressData = progressData;
			}

			readonly ZStringBuilder progressData;
			protected override void ModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
			{
				base.ModifyStatusAndPercentComplete(ref status, ref percentComplete);
				progressData.Append($"{status} - {percentComplete}");
				if (percentComplete == 75)
				{
					CancelProgressButton.PerformClick();
				}
			}
		}

		public void TestShipmentTreeView_DisplayDataInOrder()
		{
			(AsycudaManifestHeader header1, AsycudaBill header1Bill1, AsycudaBill header1Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB3", "MB1");
			(AsycudaManifestHeader header2, AsycudaBill header2Bill1, AsycudaBill header2Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB1", "MB3");
			(AsycudaManifestHeader header3, AsycudaBill header3Bill1, AsycudaBill header3Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB2", "MB2");
			Factory.Save();
			using (var dialog = new MultiManifestBillSenderDialog(new MultiManifestBillSender(new[] { header1Bill1, header1Bill2, header2Bill1, header3Bill1, header3Bill2 })))
			{
				dialog.Show();
				var shipmentTreeView = dialog.FindSingle<ZTreeView>(c => c.Name == "ShipmentTreeView");
				AssertEquals(3, shipmentTreeView.Nodes.Count);
				AssertTreeViewNode(shipmentTreeView.Nodes[0], "JOB1 - MB3", new[] { header2Bill1.ABL_BillNumber });
				AssertTreeViewNode(shipmentTreeView.Nodes[1], "JOB2 - MB2", new[] { header3Bill2.ABL_BillNumber, header3Bill1.ABL_BillNumber });
				AssertTreeViewNode(shipmentTreeView.Nodes[2], "JOB3 - MB1", new[] { header1Bill2.ABL_BillNumber, header1Bill1.ABL_BillNumber });
			}
		}

		void AssertTreeViewNode(TreeNode node, string text, ZString[] childrenText)
		{
			AssertEquals("Text", text, node.Text);
			AssertEquals("node.Nodes.Count", childrenText.Length, node.Nodes.Count);
			for (var i = 0; i < childrenText.Length; i++)
			{
				AssertEquals(i + " Child Node Text", childrenText[0], node.Nodes[0].Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			Factory.Save();
			var result = new MultiManifestBillSenderDialog(new MultiManifestBillSender(new[] { bill1, bill2 }));
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();
			return result;
		}
	}
}

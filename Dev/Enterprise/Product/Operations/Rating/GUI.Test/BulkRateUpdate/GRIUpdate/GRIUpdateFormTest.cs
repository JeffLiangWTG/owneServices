using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(GRIUpdateForm))]
	internal sealed class GRIUpdateFormTest : ZFormBasherTest
	{
		public void TestCancel()
		{
			var printer = Factory.New<StmPrintQueue>();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			Factory.Save();

			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();

				var updater = (RateUpdater)form.BusinessEntity;

				var rate1 = updater.Rates.AddNew();
				rate1.ClientPK = client.PK;
				rate1.IncludeInUpdate = true;

				form.PerformCancelClick();
				AssertEquals("form should be closed", false, form.Visible);
				AssertEquals("should not have shown a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestUpdate_Success()
		{
			var printer = Factory.New<StmPrintQueue>();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			Factory.Save();

			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();

				var updater = (RateUpdater)form.BusinessEntity;
				updater.LastRunDate = ZDateTime.Today.AddDays(-1);
				updater.PrinterPK = printer.PK;
				updater.Rates.RemoveAndDeleteAll();

				var rate1 = updater.Rates.AddNew();
				rate1.ClientPK = client.PK;
				rate1.IncludeInUpdate = true;

				form.PerformUpdateClick();
				AssertEquals("form should be closed", false, form.Visible);
				AssertEquals("should not have shown a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestUpdate_ValidationError()
		{
			var printer = Factory.New<StmPrintQueue>();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			Factory.Save();

			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();

				var updater = (RateUpdater)form.BusinessEntity;
				updater.LastRunDate = ZDateTime.Today.AddDays(-1);
				updater.PrinterPK = ZGuid.Empty;
				updater.Rates.RemoveAndDeleteAll();

				var rate1 = updater.Rates.AddNew();
				rate1.ClientPK = client.PK;
				rate1.IncludeInUpdate = true;

				form.PerformUpdateClick();
				AssertEquals("form should *NOT* be closed", true, form.Visible);
				AssertEquals("should have shown an error dialog", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestPreview()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			var contact = client.Contacts.AddNew();
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.Code;
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;
			clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;

			Factory.Save();

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();

				var updater = (RateUpdater)form.BusinessEntity;
				updater.LastRunDate = ZDateTime.Today.AddDays(-1);
				updater.PrinterPK = Guid.Empty;
				updater.Rates.RemoveAndDeleteAll();

				var rate1 = updater.Rates.AddNew();
				rate1.ClientPK = client.PK;
				rate1.IncludeInUpdate = true;

				form.bodyControl.clientGrid.Select(0);
				mockIPrintTaskUIProvider.Setup(m => m.ShowPreview(
					It.IsAny<System.IO.Stream>(),
					It.IsAny<DocumentEngine.DeliveryMethods.DeliveryInfo[]>(),
					It.IsAny<DocumentEngine.DocumentDelivery.IDeliverCapableForm>()));
				form.previewButton.PerformClick();
				mockIPrintTaskUIProvider.VerifyAll();
			}
		}

		public void TestPreview_ClientRateIsSelectedButNotSpecified_ShowInfoMessageBox()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			var contact = client.Contacts.AddNew();
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.Code;
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;
			clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;

			Factory.Save();

			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();

				var updater = (RateUpdater)form.BusinessEntity;
				updater.LastRunDate = ZDateTime.Today.AddDays(-1);
				updater.PrinterPK = Guid.Empty;
				updater.Rates.RemoveAndDeleteAll();

				var rate = updater.Rates.AddNew();
				rate.IncludeInUpdate = true;

				form.bodyControl.clientGrid.Select(0);

				form.previewButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestPreviewWithoutSelection()
		{
			using (var form = new GRIUpdateForm())
			{
				form.DisplayMode = ODisplayMode.New; // the controller does this.
				form.Show();
				Application.DoEvents();
				var updater = (RateUpdater)form.BusinessEntity;

				form.previewButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Select a valid client rate update document to preview", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new GRIUpdateForm();
		}

		#endregion
	}
}

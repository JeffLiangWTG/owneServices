using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class ExportersUnionInfoUserControlTest : TestCaseWithFactory
	{
		public void TestExportUnionPaymentsLink()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var frm = new ZForm(declaration))
			using (var control = new ExportersUnionInfoUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var exportUnionPaymentLinkLabel = (LinkLabel)control.Controls.Find("ExportUnionPaymentLinkLabel", true).First();
				AssertNotNull("ExportUnionPaymentLinkLabel Link Label should exist", exportUnionPaymentLinkLabel);
			}
		}

		public void TestShowMessageWhenFtpAddressOrUserCodeOrPasswordIsMissing()
		{
			var declaration = Factory.New<JobDeclaration>();

			var fTPSettings = TRCustomsDataRegistry.Instance.FTPSettings.GetFallBackValueAtAllLevels(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty);

			using (var frm = new ZForm(declaration))
			using (TRCustomsDataRegistry.Instance.FTPSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fTPSettings))
			using (var control = new ExportersUnionInfoUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var exportUnionPaymentLinkLabel = control.FindSingle<ZLinkLabel>("ExportUnionPaymentLinkLabel");

				exportUnionPaymentLinkLabel.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				AssertEquals("There are missing data to populate this link. Please refer to the registry item to provide them.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowMessageWhenFtpAddressOrUserCodeOrPasswordIsNotMissing()
		{
			var declaration = Factory.New<JobDeclaration>();

			var fTPSettings = TRCustomsDataRegistry.Instance.FTPSettings.GetFallBackValueAtAllLevels(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty);

			fTPSettings.FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;
			fTPSettings.ExportUnionUserCode = "user99";
			fTPSettings.ExportUnionUserPassword = "password99";

			using (var frm = new ZForm(declaration))
			using (TRCustomsDataRegistry.Instance.FTPSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fTPSettings))
			using (var control = new MiscOptionsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var exportUnionPaymentLinkLabel = control.FindSingle<ZLinkLabel>("ExportUnionPaymentLinkLabel");

				exportUnionPaymentLinkLabel.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("https://istanbul.ebirlik.net/ebnet/app?USERCODE=user99&PASSWORDFREE=password99", WebUrlLauncher.LastUrlLaunched);
			}
		}
	}
}

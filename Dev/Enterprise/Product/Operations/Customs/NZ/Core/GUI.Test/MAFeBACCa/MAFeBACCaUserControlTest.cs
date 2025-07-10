using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	[TestedType(typeof(ZForm))]
	public class MAFeBACCaUserControlTest : ZFormBasherTest
	{
		public void TestTSWTabPageVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(consol);
			using (var form = new ZForm(consol))
			{
				var control = new MAFeBACCaUserControl(mafMessaging)
				{ Dock = DockStyle.Fill };
				form.Controls.Add(control);
				form.Show();
				Assert("TSW Messages tab to be visible always now", (control.FindSingle<ZTabPage>("TSWMessagesTabPage")).TabVisible);
			}
		}

		public void TestLegacyMessagesTabPageVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(consol);
			using (var form = new ZForm(consol))
			{
				var control = new MAFeBACCaUserControl_ForTest(mafMessaging)
				{ Dock = DockStyle.Fill };
				form.Controls.Add(control);
				form.Show();
				Assert("Legacy eBACCa messages tab should now only be visible if legacy messages exist", !control.MessagesTabPage.TabVisible);
				Assert("TSW Messages tab to be visible always now", (control.FindSingle<ZTabPage>("TSWMessagesTabPage")).TabVisible);
			}

			// Both message tab pages should be displayed when IPI is in use and the Consol has existing eBACCa legacy messages
			mafMessaging.Messages.AddNew();
			using (var form = new ZForm(consol))
			{
				var control = new MAFeBACCaUserControl(mafMessaging)
				{ Dock = DockStyle.Fill };
				form.Controls.Add(control);
				form.Show();
				Assert("TSW Messages tab to always be visible", (control.FindSingle<ZTabPage>("TSWMessagesTabPage")).TabVisible);
				Assert("Legacy eBACCa messages tab should also be visible in this scenario where old legacy messages exist", (control.FindSingle<ZTabPage>("MessagesTabPage")).TabVisible);
			}
		}

		public void TestLegacyFieldsVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(consol);
			using (var form = new ZForm(consol))
			{
				var control = new MAFeBACCaUserControl(mafMessaging)
				{ Dock = DockStyle.Fill };
				form.Controls.Add(control);
				form.Show();
				AssertEquals("AttachedFilesGroupBox - should not be visible for TSW messaging", false, (control.FindSingle<ZGroupBox>("AttachedFilesGroupBox")).Visible);
				AssertEquals("ImporterContactGroupBox - should not be visible for TSW messaging", false, (control.FindSingle<ZGroupBox>("ImporterContactGroupBox")).Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			var message = mafMessaging.Messages.AddNew();
			using (message.SuspendSettingHasChanges())
			{
				message.EM_ReceiveTransmit = NZMMessage.Direction.Receive;
			}

			Factory.Save();
			var form = new ZForm(declaration)
			{ Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 700, true) };
			var control = new MAFeBACCaUserControl(mafMessaging)
			{ Dock = DockStyle.Fill };
			form.Controls.Add(control);
			form.CaptionRenderingEnabled = true;
			return form;
		}

		class MAFeBACCaUserControl_ForTest : MAFeBACCaUserControl
		{
			public MAFeBACCaUserControl_ForTest(MAFMessagingBO mafMessaging)
				: base(mafMessaging)
			{
			}

			public new ZTabPage MessagesTabPage => base.MessagesTabPage;
		}
	}
}

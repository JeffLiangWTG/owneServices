using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVBookingHeaderModule))]
	class HVLVBookingHeaderModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HVLVBookingHeader;

		public void TestOnlySupportCanCreateNew()
		{
			var barryWong = Factory.NewWithValidTestData<GlbStaff>();
			barryWong.GS_LoginName = "BarryWong";

			Factory.Save();

			using (var module = new HVLVBookingHeaderModule())
			{
				using (Env.SetTemporaryUserContext(barryWong.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					Assert("Precondition; user is not support", !Env.CurrentUser.IsSupportUser);
					Assert("Non-support user cannot create new", !module.AllowNew);
				}

				using (Env.SetTemporaryUserContext("CWSupport", Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					Assert("Precondition; user is support", Env.CurrentUser.IsSupportUser);
					Assert("Support user can create new", module.AllowNew);
				}
			}
		}

		public void TestDeniedPartyScreening_ScreeningNotEnabledMessage()
		{
			using (var form = new Form())
			using (var module = new HVLVBookingHeaderModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = false }))
				{
					var resynchronizeMenuItem = module.FormActionMenu.FindByText("Resynchronize Screening Status", true);
					AssertNotNull("Resynchronize Screening Status menu item exist", resynchronizeMenuItem);

					resynchronizeMenuItem.PerformClick();
					AssertEquals("HVLV Party Screening has not been enabled.\r\nTo enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					var viewComplianceStatusMenuItem = module.FormActionMenu.FindByText("View Compliance Status", true);
					AssertNotNull("View Compliance Status menu item exist", viewComplianceStatusMenuItem);

					viewComplianceStatusMenuItem.PerformClick();
					AssertEquals("HVLV Party Screening has not been enabled.\r\nTo enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestAllowDefaultActivateDeactivate()
		{
			using (var module = new HVLVBookingHeaderModule())
			{
				Assert("AllowDefaultActivateDeactivate is true", module.AllowDefaultActivateDeactivate);
			}
		}
	}
}

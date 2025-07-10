using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class CALINFMessagingPluginTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			return new CALINFMessagingPlugin(voyage);
		}

		public void TestUserControl()
		{
			using (var plugin = new CALINFPluginTester(voyage))
			{
				AssertType<CALINFMessageDisplayControl>(plugin.UserControl);
			}
		}

		public void TestBusinessEntity()
		{
			using (var plugin = new CALINFPluginTester(voyage))
			{
				AssertType<JobVoyage>(plugin.BusinessEntity);
			}
		}

		public void TestTopLevelMenu()
		{
			using (SetCALINFMessaging(false))
			{
				using (var plugin = new CALINFPluginTester(voyage))
				{
					var menu = plugin.ExposedGetTopLevelMenu();
					AssertNull("Should be no menu if not enabled PFUNC ZACALINF", menu);
				}
			}

			using (SetCALINFMessaging(true))
			{
				voyage.Messages.RemoveAll();
				using (var plugin = new CALINFPluginTester(voyage))
				{
					var menu = plugin.ExposedGetTopLevelMenu();
					AssertNotNull("Menu should be enabled PFUNC ZACALINF", menu);
					AssertEquals("Menu should have correct text", "Messages", menu.Text);
					AssertEquals("Should only be 1 Menu item", 1, menu.MenuItems.Count);
					AssertEquals("Sub-Menu 1 - Send Original Text", "Send ZA CALINF Message", menu.MenuItems[0].Text);
				}

				voyage.Messages.AddNew();
				using (var plugin = new CALINFPluginTester(voyage))
				{
					var menu = plugin.ExposedGetTopLevelMenu();
					AssertNotNull("Menu should be enabled PFUNC ZACALINF", menu);
					AssertEquals("Should only be 1 Menu item", 2, menu.MenuItems.Count);
					AssertEquals("Sub-Menu 1 - Send Amendment", "Send ZA CALINF Amendment", menu.MenuItems[0].Text);
					AssertEquals("Sub-Menu 2 - Send Cancellation", "Send ZA CALINF Cancellation", menu.MenuItems[1].Text);
				}
			}
		}

		IDisposable SetCALINFMessaging(bool active)
		{
			return Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZACALINFMessaging, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			voyage = Factory.NewWithValidTestData<JobVoyage>();
		}

		JobVoyage voyage;
		sealed class CALINFPluginTester : CALINFMessagingPlugin
		{
			public CALINFPluginTester(JobVoyage voyage) : base(voyage)
			{
			}

			public MenuItem ExposedGetTopLevelMenu()
			{
				return GetNewTopLevelMenu();
			}
		}
	}
}

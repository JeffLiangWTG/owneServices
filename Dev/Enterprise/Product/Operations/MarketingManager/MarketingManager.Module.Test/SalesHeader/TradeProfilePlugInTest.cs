using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	class TradeProfilePlugInTest : TestCaseWithFactory
	{
		public void TestName()
		{
			var orgHeader = Factory.New<OrgHeader>();
			using (var plugin = new TradeProfileForOrgPlugin(orgHeader))
			{
				AssertEquals("Value Analysis", plugin.Name);
			}
		}

		public void TestSynchronizeTradeProfileDataMenuItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var form = new ZForm(org))
			{
				form.PlugIns.Add(ControllerIDs.TradeProfileForOrgPlugin);
				form.Show();

				IFileMenuItemsProvider formMenuItemsProvider = form;
				var synchronizeTradeProfileDataMenuItem = formMenuItemsProvider.ActionsMenuItem.MenuItems.Cast<MenuItem>().First(x => x.Text == "Synchronize Value Analysis Data");
				synchronizeTradeProfileDataMenuItem.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Synchronize Value Analysis Data", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Text", "Synchronizing Value Analysis Data with operational data may take a couple of minutes. Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSynchronizeTradeProfileDataMenuItemWhenJCDServiceTaskNotRun()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(org))
			{
				form.PlugIns.Add(ControllerIDs.TradeProfileForOrgPlugin);
				form.Show();

				IFileMenuItemsProvider formMenuItemsProvider = form;
				var synchronizeTradeProfileDataMenuItem = formMenuItemsProvider.ActionsMenuItem.MenuItems.Cast<MenuItem>().First(x => x.Text == "Synchronize Value Analysis Data");
				synchronizeTradeProfileDataMenuItem.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Job Costing Data Queue Service Task Incomplete", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Text", "Before you can synchronize operational data, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf).", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSynchronizeTradeProfileDataMenuItem_WhenOrgDeletedInAnotherFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var form = new ZForm(org))
			{
				form.PlugIns.Add(ControllerIDs.TradeProfileForOrgPlugin);
				form.Show();

				var anotherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(anotherFactory))
				{
					var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
					orgInOtherFactory.Delete();
					anotherFactory.Save();
				}

				IFileMenuItemsProvider formMenuItemsProvider = form;
				var synchronizeTradeProfileDataMenuItem = formMenuItemsProvider.ActionsMenuItem.MenuItems.Cast<MenuItem>().First(x => x.Text == "Synchronize Value Analysis Data");
				synchronizeTradeProfileDataMenuItem.PerformClick();

				AssertEquals("This record has been deleted while you were editing it. This form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestSychronizeTradeProfileDataMenuItem_WhenDataExitsGreaterThen12MonthsAgo()
		{
			var currOrg = Factory.NewWithValidTestData<OrgHeader>();

			var collectionHelper = new OrgSalesCollectionTestHelper(currOrg.SalesCollection, Factory);

			var destOrg = collectionHelper.GetNewOrg("TESTORG");

			collectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", currOrg.PK, destOrg.PK, new ZDate(2013, 1, 1));
			collectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "LCL", currOrg.PK, destOrg.PK, new ZDate(2014, 1, 1));
			collectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "ROR", currOrg.PK, destOrg.PK, new ZDate(2015, 1, 1));

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var plugin = new TradeProfileForOrgPluginForTest(currOrg))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				plugin.SynchronizeTradeProfileDataMenuItemClick_Exposed();
			}

			AssertEquals(1, currOrg.SalesCollection.Count);
			var sales = collectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", currOrg.PK.ToGuid(), destOrg.PK.ToGuid(), isActual: true).Single();
			AssertEquals(3, sales.TradeDetails.Count);
		}

		public void TestUserControl()
		{
			var orgHeader = Factory.New<OrgHeader>();
			using (var plugin = new TradeProfileForOrgPlugin(orgHeader))
			using (var control = plugin.UserControl)
			{
				AssertType(typeof(TradeProfileForOrgUserControl), control);
			}
		}

		public class TradeProfileForOrgPluginForTest : TradeProfileForOrgPlugin
		{
			public TradeProfileForOrgPluginForTest(OrgHeader org) : base(org) { }

			public void SynchronizeTradeProfileDataMenuItemClick_Exposed()
			{
				base.SynchronizeTradeProfileDataMenuItemClick(null, null);
			}

			protected override void Synchronise(OrgHeader org)
			{
				SynchroniseCore(Db.Connection, org);
			}

			protected override OrgHeader GetOrg()
			{
				return (OrgHeader)base.HostBusinessEntity;
			}
		}
	}
}

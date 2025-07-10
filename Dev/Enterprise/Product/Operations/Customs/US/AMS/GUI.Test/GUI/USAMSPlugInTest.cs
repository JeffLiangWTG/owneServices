using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	sealed class USAMSPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		[ExpectNoExceptions]
		public void TestDeleteConsol()
		{
			using (var plugIn = new USAMSPlugIn(consol))
			{
				consol.Delete();
				Factory.Save();
			}
		}

		[TestDate(2012, 04, 05)]
		public void TestSendPTTMessage()
		{
			using (var plugIn = GetPlugInToTest())
			{
				var menuItem = plugIn.TopLevelMenu.MenuItems.FindByText(AMSMainMenuItem.Constants.Caption.PTTMenuItemText, true);
				AssertNotNull(menuItem);
			}
		}

		public void TestAMSMenuItemOnConsol()
		{
			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new TabPage());
				form.Controls.Add(tabControl);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.Transports[0].JW_IsLinked = false;
				consol.Transports[0].JW_Vessel = "AVSDP";
				consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-1);
				consol.Transports[0].JW_ETA = ZDateTime.Today;
				form.PlugIns.Add(ControllerIDs.Customs.US.AMS);
				form.Show();
				var plugIn = (USAMSPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.US.AMS);
				plugIn.Enabled = true;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems.FindByText("Send Manifest", true);
				AssertNull(plugIn.Header);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(MessageSender.Constants.Message.NoHeaderNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				tabControl.SelectedIndex = 1;
				var header = plugIn.Header;
				AssertNotNull(header);
				header.BH_OverrideFreightDefaults = true;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "MD321";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AMSMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertEquals(AMSMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenuItem.PerformClick();
				AssertEquals(MessageSender.Constants.Message.NoOrgProxySCACNotification(header), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.IsInDatabase);
			}
		}

		public void TestEnabledAndDisable()
		{
			using (var plugIn = new USAMSPlugIn(consol))
			{
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKDischargePort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKFirstForeignPort = "USLAX";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKFirstForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "USLAX";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "USLAX";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USLAX";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				// CS00111699 - Ignore Domestic leg
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USCHI";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
				AssertEquals("PlugIn is enabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "PRADJ";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("PlugIn is not enabled", false, plugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
			}
		}

		public void TestMutexIsUsedInCreationgOfInBond()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var consol = factory1.New<ForwardingConsol>();
			consol.Transports[0].JW_Vessel = "ABC VESSEL";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAS";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB324";
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var consolInFactory2 = factory2.Load<ForwardingConsol>(consol.PK);
			using (var pluginFactory2 = new USAMSPlugInForTesting(consolInFactory2))
			using (var pluginFactory1 = new USAMSPlugInForTesting(consol))
			{
				pluginFactory1.Enabled = true;
				pluginFactory2.Enabled = true;
				pluginFactory2.CreateAMS_Exposed();
				AssertEquals("pluginFactory1.Mutex.IsLocked", true, pluginFactory1.Mutex_Exposed.IsLocked);
				AssertEquals("pluginFactory2.Mutex.IsLocked", true, pluginFactory2.Mutex_Exposed.IsLocked);
				var inBondFactory1 = pluginFactory1.InternalHeader_Exposed;
				AssertNull("inBondFactory1 should be null due to the mutex", inBondFactory1);
				var inBondFactory2 = pluginFactory2.InternalHeader_Exposed;
				AssertNotNull("inBondFactory2", inBondFactory2);
				factory2.Save();
				AssertEquals("pluginFactory1.Mutex.IsLocked", false, pluginFactory1.Mutex_Exposed.IsLocked);
				AssertEquals("pluginFactory2.Mutex.IsLocked", false, pluginFactory2.Mutex_Exposed.IsLocked);
				inBondFactory1 = pluginFactory1.InternalHeader_Exposed;
				AssertEquals("inBondFactory1 should be matched", inBondFactory2.PK, inBondFactory1.PK);
				AssertEquals("inBondFactory2", inBondFactory2, pluginFactory2.InternalHeader_Exposed);
				AssertEquals("inBondFactory1.BH_ImportConveyanceName", "ABC VESSEL", inBondFactory1.BH_ImportConveyanceName);
				AssertEquals("inBondFactory1.BH_ImportConveyanceNameInfo.ReadOnly", true, inBondFactory1.BH_ImportConveyanceNameInfo.ReadOnly);
				AssertEquals("inBondFactory2.BH_ImportConveyanceName", "ABC VESSEL", inBondFactory2.BH_ImportConveyanceName);
				AssertEquals("inBondFactory2.BH_ImportConveyanceNameInfo.ReadOnly", true, inBondFactory2.BH_ImportConveyanceNameInfo.ReadOnly);
			}
		}

		public void TestNewAMSHeaderIsNVOCC()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var consol = factory.New<ForwardingConsol>();
			consol.Transports[0].JW_Vessel = "ABC VESSEL";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAS";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB324";
			factory.Save();
			using (var plugin = new USAMSPlugInForTesting(consol))
			{
				plugin.Enabled = true;
				plugin.CreateAMS_Exposed();
				var ams = plugin.InternalHeader_Exposed;
				AssertEquals(true, ams.IsNVOCCHeader);
				ams.Delete();
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			return new USAMSPlugIn(consol);
		}

		ForwardingConsol consol;
		CusInBondHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		sealed class USAMSPlugInForTesting : USAMSPlugIn
		{
			public USAMSPlugInForTesting(ForwardingConsol hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			public ZBool CreateAMS_Exposed() => CreateAMS();
			public CusInBondHeader InternalHeader_Exposed => InternalHeader;
			public ZGlobalMutex Mutex_Exposed => Mutex;
		}
	}
}

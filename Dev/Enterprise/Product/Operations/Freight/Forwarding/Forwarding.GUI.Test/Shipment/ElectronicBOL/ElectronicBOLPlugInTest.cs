using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ElectronicBOLPlugInTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void Test_Visibility()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var orgProxyFromBranch = GlbBranch.CurrentBranch.OrgProxy;
			orgProxyFromBranch.CustomsCodes.RemoveAll();
			orgProxyFromBranch.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, true);
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, ZString.Empty, true, true, false);
			Assert_Visibility(shipment, Core.Constants.TransportModes.SeaAir, "S00001001", true, true, true);
			Assert_Visibility(shipment, Core.Constants.TransportModes.Air, "S00001001", true, true, false);
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", false, true, false);
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, false, false);
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", false, false, false);
		}

		public void Test_Visibility_Fallback()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var orgProxyFromBranch = GlbBranch.CurrentBranch.OrgProxy;
			orgProxyFromBranch.CustomsCodes.RemoveAll();
			orgProxyFromBranch.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, true);

			orgProxyFromBranch.CustomsCodes.RemoveAll();

			var orgProxyFromCompany = GlbCompany.CurrentCompany.OrgProxy;
			orgProxyFromCompany.CustomsCodes.RemoveAll();
			orgProxyFromCompany.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, true);

			orgProxyFromCompany.CustomsCodes.RemoveAll();
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, false);
		}

		[RequiresSTA]
		public void Test_Visibility_IsOriginalBillRequired()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var orgProxyFromBranch = GlbBranch.CurrentBranch.OrgProxy;
			orgProxyFromBranch.CustomsCodes.RemoveAll();
			orgProxyFromBranch.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.NonNegotiable;
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, false);

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			Factory.Save();
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, true);

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			Factory.Save();
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, false);

			shipment.JS_ElectronicBillOfLadingStatus = "SUR";
			Factory.Save();
			Assert_Visibility(shipment, Core.Constants.TransportModes.Sea, "S00001001", true, true, true);
		}

		void Assert_Visibility(ForwardingShipment shipment, string transportMode, string houseBillNumber, bool allowPublishHBL, bool enableEBLIntegration, bool enablePlugIn)
		{
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = houseBillNumber;
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = allowPublishHBL;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = enableEBLIntegration,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (var form = new FormForTest(shipment))
			{
				form.Show();
				AssertEquals(enablePlugIn, form.ElectronicBOLPlugIn.Enabled);
				AssertEquals("Electronic Bill Of Lading", form.ElectronicBOLPlugIn.Name);
			}
		}

		#region TestPlugInNotDisplayedMessageWhenEBLIsNotSupported

		public void TestPlugInNotDisplayedMessageWhenEBLIsNotSupported_OriginCountry()
		{
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var rule = Factory.NewWithValidTestData<RefCountryRules>();
				rule.R7_RN_NKOrigin = "CN";
				rule.R7_RN_NKDestination = "";
				rule.R7_IsEBLNotSupported = true;

				var rule2 = Factory.NewWithValidTestData<RefCountryRules>();
				rule2.R7_RN_NKOrigin = "CN";
				rule2.R7_RN_NKDestination = "AU";
				rule2.R7_IsEBLNotSupported = false;

				var rule3 = Factory.NewWithValidTestData<RefCountryRules>();
				rule3.R7_RN_NKOrigin = "";
				rule3.R7_RN_NKDestination = "AU";
				rule3.R7_IsEBLNotSupported = true;

				var shipment = CreateShipment();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "AUSYD";
				Factory.Save();

				AssertContainsNotSupportedMessage(shipment);

				rule.R7_IsEBLNotSupported = false;
				rule2.R7_IsEBLNotSupported = true;
				Factory.Save();

				AssertContainsNotSupportedMessage(shipment);
			}

			void AssertContainsNotSupportedMessage(ForwardingShipment shipment)
			{
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					ErrorReporter.Clear();
					var mainTabControl = (ZTemplateTabControl)form.Controls["MainTabControl"];
					var electronicMessagingTabPage = mainTabControl.GetTabPage("ElectronicMessagingTabPage");
					mainTabControl.SelectedTab = electronicMessagingTabPage;
					mainTabControl.PerformLayout();

					var tabControl = electronicMessagingTabPage.Controls[0] as ZTabControl;
					AssertNotNull(tabControl);

					var plugIn = tabControl.PlugIns.GetPlugIn(ControllerIDs.ElectronicBOL);
					AssertNotNull(plugIn);

					AssertContains("Electronic Bill Of Lading cannot be shown at this time.\r\nElectronic Bills Of Lading are not supported in Origin Country China.", plugIn.PlugInNotDisplayedMessage);
				}
			}
		}

		public void TestPlugInNotDisplayedMessageWhenEBLIsNotSupported_DestinationCountry()
		{
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var rule = Factory.NewWithValidTestData<RefCountryRules>();
				rule.R7_RN_NKOrigin = "CN";
				rule.R7_RN_NKDestination = "AU";
				rule.R7_IsEBLNotSupported = true;

				var rule2 = Factory.NewWithValidTestData<RefCountryRules>();
				rule2.R7_RN_NKOrigin = "";
				rule2.R7_RN_NKDestination = "BR";
				rule2.R7_IsEBLNotSupported = true;

				var shipment = CreateShipment();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "BRUNA";
				Factory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					ErrorReporter.Clear();
					var mainTabControl = (ZTemplateTabControl)form.Controls["MainTabControl"];
					var electronicMessagingTabPage = mainTabControl.GetTabPage("ElectronicMessagingTabPage");
					mainTabControl.SelectedTab = electronicMessagingTabPage;
					mainTabControl.PerformLayout();

					var tabControl = electronicMessagingTabPage.Controls[0] as ZTabControl;
					AssertNotNull(tabControl);

					var plugIn = tabControl.PlugIns.GetPlugIn(ControllerIDs.ElectronicBOL);
					AssertNotNull(plugIn);

					AssertContains("Electronic Bill Of Lading cannot be shown at this time.\r\nElectronic Bills Of Lading are not supported in Destination Country Brazil.", plugIn.PlugInNotDisplayedMessage);
				}
			}
		}

		ForwardingShipment CreateShipment()
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST002";
			org.OH_FullName = "Test Organization2";
			org.OH_IsConsignee = true;

			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "123456";
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRRIO";
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;

			Factory.Save();

			shipment.JS_ElectronicBillOfLadingStatus = "OBA";
			shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;

			return shipment;
		}

		#endregion

		class FormForTest : ZTemplateForm
		{
			public FormForTest(ForwardingShipment businessEntity) : base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.ElectronicBOL);
			}

			public ElectronicBOLPlugIn ElectronicBOLPlugIn
			{
				get => (ElectronicBOLPlugIn)PlugIns.GetPlugIn(ControllerIDs.ElectronicBOL);
			}
		}
	}
}

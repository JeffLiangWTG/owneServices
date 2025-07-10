using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class WeightVolChargeableControlTest : BaseFreightTest
	{
		public void TestViewBillOfLadingNotWork()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";
			sendingForwarderContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.DoNotDeliver;
			var sendingForwarderContactDoc = sendingForwarderContact.Documents.AddNew();
			sendingForwarderContactDoc.OD_DocumentGroup = "FES";
			sendingForwarderContactDoc.OD_DeliverBy = Core.Constants.ContactNotifyModes.DoNotDeliver;
			sendingForwarder.Contacts.Add(sendingForwarderContact);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = "TAN";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			shipment.Consols.Add(consol);

			using (var form = new ZForm(shipment))
			using (var control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowDocEngineBillOfLading();
				AssertEquals("ExpectedMessage", "Please save your record before running View Bill Of Lading.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();

				control.ShowDocEngineBillOfLading();

				Form previewForm = null;

				try
				{
					previewForm = Application.OpenForms
						.OfType<Form>()
						.FirstOrDefault(f => f.GetType().FullName == "Enterprise.DocumentEngine.GUI.XLSPreviewForm");

					AssertNotNull("HBL preview form has been shown", previewForm);
				}
				finally
				{
					previewForm?.Close();
				}
			}
		}

		#region TestInitViewEditBillButtonText

		public void TestInitViewEditBillButtonText()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using var form = new ZForm(shipment);
			using var control = new WeightVolChargeableControl();
			form.Controls.Add(control);
			form.Show();
			control.SetDataBinding(shipment, "");

			AssertEquals("TransportMode: Air", "View/Edit AWB", control.ViewEditBillButton.Text);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("TransportMode: Sea", "View Bill Of Lading", control.ViewEditBillButton.Text);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("TransportMode: Rail", "View Bill Of Lading", control.ViewEditBillButton.Text);

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = HomePort;
			shipment.Consols.Add(consol);
			AssertEquals("TransportMode: SeaAir", "View/Edit AWB", control.ViewEditBillButton.Text);
		}

		public void TestInitViewEditBillButtonText_DoesNotThrowNRE()
		{
			var notAShipmentBO = Factory.NewWithValidTestData<ForwardingConsol>();

			using (ZForm form = new ZForm())
			using (WeightVolChargeableControl control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();
				AssertEquals("View/Edit Bill", control.ViewEditBillButton.Text);
			}
		}

		[RequiresSTA]
		public void TestViewEditBillButton_NeedSaveBeforeRunning()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = "AAA";

			using (var form = new ZForm(shipment))
			using (var control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();
				AssertEquals("ExpectedMessage", "Please save your record before running View Bill Of Lading.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();

				control.ViewEditBillButton.PerformClick();
				AssertEquals("ExpectedMessage", "Bill of Lading applicable to this shipment could not be found.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowDocEngineBillOfLading()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = "IAU";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 1;

			Factory.Save();

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ZForm form = new ZForm(shipment))
			using (WeightVolChargeableControl control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();

				AssertDocEngineBillOfLadingPreviewFormIsOpened();
			}
		}

		public void TestShowDocEngineBillOfLading_CustomBillOfLadingType()
		{
			const string customBillOfLadingType = "XXX";

			var docEngineBillOfLadingMenuPK = Guid.Parse("161640e4-d57d-4f12-a4de-0ca601a95a54");
			var docEngineBillofLadingITCTemplatePK = Guid.Parse("e5497388-b7c5-48b6-9008-09c25cfd726b");
			var hblDocTypePK = Guid.Parse("6d918117-dda3-4d07-b054-23e64e274491");

			var menuItemQuery = new ZQuery(StmMenuItemSchema.PK, docEngineBillOfLadingMenuPK);
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.Documents);
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment));

			var menuItem = Factory.LoadTop1<StmMenuItemBase>(menuItemQuery);
			AssertNotNull("precondition; found docengine system Bill Of Lading menu item", menuItem);

			var templateQuery = new ZQuery(StmTemplateSchema.PK, docEngineBillofLadingITCTemplatePK);
			templateQuery.AddToFilter(StmTemplateSchema.SO_TemplateType, Constants.StmMenuItemTypes.Documents);

			var template = Factory.LoadTop1<StmTemplate>(templateQuery);
			AssertNotNull("precondition; found docengine system Bill Of Lading template", template);

			var docTypeQuery = new ZQuery(RefDocTypeSchema.PK, hblDocTypePK);
			docTypeQuery.AddToFilter(RefDocTypeSchema.RT_DocType, "HBL");

			var docType = Factory.LoadTop1<RefDocType>(docTypeQuery);
			AssertNotNull("precondition; found HBL system doctype", docType);

			var customTemplate = Factory.New<StmTemplate>();
			customTemplate.SO_Name = $"{template.SO_Name} (custom)";
			customTemplate.SO_TemplateType = template.SO_TemplateType;
			customTemplate.SO_DataContext = template.SO_DataContext;
			customTemplate.SO_IsClientSpecific = true;
			customTemplate.SO_IsSystemDefined = true;
			customTemplate.SO_Template = template.SO_Template;
			customTemplate.SO_TemplateType = template.SO_TemplateType;
			customTemplate.SO_UDFFieldCache = template.SO_UDFFieldCache;

			var original = menuItem.Documents.AddNew();
			original.SI_SU = menuItem.PK;
			original.SI_SO = customTemplate.PK;
			original.SI_DocumentTitle = "ORIGINAL";
			original.SI_PrintCopyType = nameof(PrintType.PRN);
			original.SI_RT_DocType = docType.PK;
			original.SI_IsClientSpecific = true;
			original.SI_IsSystemDefined = true;
			original.SI_MenuTemplateFilter = $@"HBL={customBillOfLadingType}";

			var copy = menuItem.Documents.AddNew();
			copy.SI_SU = menuItem.PK;
			copy.SI_SO = customTemplate.PK;
			copy.SI_DocumentTitle = "COPY";
			copy.SI_PrintCopyType = nameof(PrintType.EML);
			copy.SI_RT_DocType = docType.PK;
			copy.SI_IsClientSpecific = true;
			copy.SI_IsSystemDefined = true;
			copy.SI_MenuTemplateFilter = $@"HBL={customBillOfLadingType}";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = customBillOfLadingType;
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 1;

			Factory.Save();

			var temporaryCodeList = new SystemDefinableCodeDescriptionBoolCollection();
			temporaryCodeList.Add(customBillOfLadingType);

			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryCodeList))
			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(shipment))
			using (var control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();

				AssertDocEngineBillOfLadingPreviewFormIsOpened();
			}
		}

		void AssertDocEngineBillOfLadingPreviewFormIsOpened()
		{
			Form previewForm = null;

			try
			{
				previewForm = Application.OpenForms
					.OfType<Form>()
					.FirstOrDefault(f => f.GetType().FullName == "Enterprise.DocumentEngine.GUI.XLSPreviewForm");

				AssertNotNull("HBL preview form has been shown", previewForm);
			}
			finally
			{
				previewForm?.Close();
			}
		}

		public void TestShowFormBuilderBillOfLading_TUS_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates, true);
		public void TestShowFormBuilderBillOfLading_TUS_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates, false);
		public void TestShowFormBuilderBillOfLading_TUP_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStatesPreprinted, true);
		public void TestShowFormBuilderBillOfLading_TUP_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStatesPreprinted, false);
		public void TestShowFormBuilderBillOfLading_Yusen() => AssertShowAdditionalFormBuilderBillOfLading(Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL);
		public void TestShowFormBuilderBillOfLading_DHL() => AssertShowAdditionalFormBuilderBillOfLading(Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL);
		[RequiresSTA]
		public void TestShowFormBuilderBillOfLading_DHK_RegisterOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.DataHawkBill, true);
		public void TestShowFormBuilderBillOfLading_DHK_RegisterOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.DataHawkBill, false);
		public void TestShowFormBuilderBillOfLading_TNZ_RegisterOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ, true);
		public void TestShowFormBuilderBillOfLading_TNZ_RegisterOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ, false);
		public void TestShowFormBuilderBillOfLading_FIATA_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL, true);
		public void TestShowFormBuilderBillOfLading_FIATA_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL, false);
		[RequiresSTA]
		public void TestShowFormBuilderBillOfLading_TAN_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TANHBL, true);
		public void TestShowFormBuilderBillOfLading_TAN_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TANHBL, false);
		public void TestShowFormBuilderBillOfLading_TANPreprinted_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted, true);
		public void TestShowFormBuilderBillOfLading_TANPreprinted_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted, false);
		public void TestShowFormBuilderBillOfLading_CargowiseBill_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.CargowiseBill, true);
		public void TestShowFormBuilderBillOfLading_CargowiseBill_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.CargowiseBill, false);
		public void TestShowFormBuilderBillOfLading_CargowiseBillPreprinted_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted, true);
		[RequiresSTA]
		public void TestShowFormBuilderBillOfLading_CargowiseBillPreprinted_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted, false);
		public void TestShowFormBuilderBillOfLading_IAU_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustralia, true);
		public void TestShowFormBuilderBillOfLading_IAU_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustralia, false);
		public void TestShowFormBuilderBillOfLading_ISI_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTerms, true);
		public void TestShowFormBuilderBillOfLading_ISI_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTerms, false);
		public void TestShowFormBuilderBillOfLading_INN_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw, true);
		[RequiresSTA]
		public void TestShowFormBuilderBillOfLading_INN_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw, false);
		public void TestShowFormBuilderBillOfLading_TTP_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted, true);
		public void TestShowFormBuilderBillOfLading_TTP_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted, false);
		public void TestShowFormBuilderBillOfLading_ITClubAustraliaPrePrinted_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted, true);
		public void TestShowFormBuilderBillOfLading_ITClubAustraliaPrePrinted_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted, false);
		public void TestShowFormBuilderBillOfLading_INZ_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand, true);
		public void TestShowFormBuilderBillOfLading_INZ_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand, false);
		public void TestShowFormBuilderBillOfLading_INPPreprinted_RegistryOn() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted, true);
		public void TestShowFormBuilderBillOfLading_INPPreprinted_RegistryOff() => AssertShowSystemDefinedFormBuilderBillOfLading(Core.Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted, false);

		public void TestShowFormBuilderBillOfLading_CustomBillOfLadingType()
		{
			const string customBillOfLadingType = "XXX";

			var menuItemQuery = new ZQuery(StmMenuItemSchema.PK, ShipmentSystemFormMenuItems.BillOfLadingPK);
			var menuItem = Factory.LoadTop1<StmMenuItemBase>(menuItemQuery);
			AssertNotNull("Precondition.", menuItem);
			menuItem.SU_FilterList = ZString.Empty;

			var templateQuery = new ZQuery(StmTemplateSchema.SO_TemplateType, "FRM");
			var template = Factory.LoadTop1<StmTemplate>(templateQuery);
			AssertNotNull("Precondition.", template);

			var newPivot = menuItem.Documents.AddNew();
			newPivot.SI_SU = menuItem.PK;
			newPivot.SI_SO = template.PK;
			newPivot.SI_MenuTemplateFilter = $@"JS_HouseBillOfLadingType == ""{customBillOfLadingType}""";
			newPivot.SI_DataStoreName = "BillOfLading";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = customBillOfLadingType;
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 1;

			Factory.Save();

			var temporaryCodeList = new SystemDefinableCodeDescriptionBoolCollection();
			temporaryCodeList.Add(customBillOfLadingType);

			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryCodeList))
			using (var form = new ZForm(shipment))
			using (var control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();
			}

			var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
			AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
		}

		void AssertShowAdditionalFormBuilderBillOfLading(string houseBillOfLadingCode)
		{
			var defaultValuesOverride = new AdditionalHouseBillOfLadingTypeCollection();

			var yusType = new AdditionalHouseBillOfLadingType
			{
				Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
				Description = (NoResString)"Yusen HBL",
				Enable = true,
				EnableMessaging = true
			};

			var dhlType = new AdditionalHouseBillOfLadingType
			{
				Code = Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL,
				Description = (NoResString)"DHL HBL",
				Enable = true,
				EnableMessaging = true
			};

			defaultValuesOverride.Add(yusType);
			defaultValuesOverride.Add(dhlType);

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValuesOverride))
			{
				AssertShowFormBuilderBillOfLading(houseBillOfLadingCode, true);
			}
		}

		void AssertShowSystemDefinedFormBuilderBillOfLading(string houseBillOfLadingCode, bool useFormBuilderHouseBills)
		{
			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useFormBuilderHouseBills))
			{
				AssertShowFormBuilderBillOfLading(houseBillOfLadingCode, useFormBuilderHouseBills);
			}
		}

		void AssertShowFormBuilderBillOfLading(string houseBillOfLadingCode, bool expectToShow)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = houseBillOfLadingCode;
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 1;

			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			using (WeightVolChargeableControl control = new WeightVolChargeableControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewEditBillButton.PerformClick();
			}

			var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;

			if (expectToShow)
			{
				AssertEquals("Form Builder HBL form has been shown",
					"Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
			}
			else
			{
				AssertNull("Form Builder HBL form has not been shown", lastShownFormTypeName);

				var previewForm = Application.OpenForms
					.OfType<Form>()
					.FirstOrDefault(f => f.GetType().FullName == "Enterprise.DocumentEngine.GUI.XLSPreviewForm");

				previewForm?.Close();
			}
		}

		#endregion

		#region LoadingMetersVisibility

		public void TestLoadingMetersVisibility()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (ZForm form = new ZForm(shipment))
			{
				WeightVolChargeableControlForTesting weightVolumeControl = new WeightVolChargeableControlForTesting();
				form.Controls.Add(weightVolumeControl);
				weightVolumeControl.SetDataBinding(shipment, "");
				form.Show();

				shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);
				AssertEquals(true, weightVolumeControl.JS_DocumentedLoadingMetersCalcEdit.Visible);
				AssertEquals(true, weightVolumeControl.JS_ManifestedLoadingMetersCalcEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);
				AssertEquals(false, weightVolumeControl.JS_DocumentedLoadingMetersCalcEdit.Visible);
				AssertEquals(false, weightVolumeControl.JS_ManifestedLoadingMetersCalcEdit.Visible);
			}
		}

		#endregion

		#region TestGetDocumentPack

		public void TestGetDocumentPack()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			using (WeightVolChargeableControlForTesting control = new WeightVolChargeableControlForTesting())
			{
				control.SetDataBinding(shipment, "");

				shipment.JS_HouseBillOfLadingType = "IAU";
				Assert(control.GetDocumentPackExp().Count > 0);

				shipment.JS_HouseBillOfLadingType = "ITP";
				Assert(control.GetDocumentPackExp().Count > 0);
			}
		}

		class WeightVolChargeableControlForTesting : WeightVolChargeableControl
		{
			public DocumentPack GetDocumentPackExp()
			{
				return GetDocumentPack();
			}

			public new ZCalcEdit JS_DocumentedLoadingMetersCalcEdit
			{
				get { return base.JS_DocumentedLoadingMetersCalcEdit; }
			}

			public new ZCalcEdit JS_ManifestedLoadingMetersCalcEdit
			{
				get { return base.JS_ManifestedLoadingMetersCalcEdit; }
			}
		}

		#endregion
	}
}

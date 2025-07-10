using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class ShipmentReceivalFormTest : BaseFreightTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisplayFieldsShouldBeMatchAdpterImportFields()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CFSShipment>();

			using (var form = new ShipmentReceivalForm(shipment))
			{
				form.Show();
				Application.DoEvents();

				var actual = GetBindTo(form);
				var expected = GetExpectedBindTo();

				var message = @"CFSShipmentValueObjectDataAdapter imports only fields visible on CFS shipment form.
If you need to add or remove any controls on CFS shipment form.
1. Please investigate and do a proper job on the CFSShipmentValueObjectDataAdapter.ImportFromValueObjectCore method.
2. 
    a) Please update ImportFields.txt (Path:Enterprise\Product\Operations\Freight\CFS\CFS.Business\ValueObjectDataAdapters\Test\ImportFields.txt)
    OR
    b) Please update the SourceFieldsThatHaveExposedWrappers list in ShipmentReceivalForm.cs
";

				AssertContainsExactElementsInAnyOrder(message, expected, actual);
			}
		}

		IEnumerable<string> GetBindTo(Control control)
		{
			var controls = control.Controls.Cast<Control>();
			var result = new List<string>();

			result.AddRange(controls.OfType<IBindTo>().Select(x => x.BindTo).Where(x => !string.IsNullOrEmpty(x)).Distinct());
			result.AddRange(controls.SelectMany(GetBindTo));

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		string[] GetExpectedBindTo()
		{
			var result = new List<string>();
			var sourceFields = System.IO.File.ReadAllLines(BaseSourcePath + @"Enterprise\Product\Operations\Freight\CFS\CFS.Business\ValueObjectDataAdapters\Testing\ImportFields.txt");
			foreach (var sourceField in sourceFields)
			{
				string[] exposedWrapper;
				if (SourceFieldsThatHaveExposedWrappers.TryGetValue(sourceField, out exposedWrapper))
				{
					result.AddRange(exposedWrapper);
				}
				else
				{
					result.Add(sourceField);
				}
			}
			return result.ToArray();
		}

		Dictionary<string, string[]> SourceFieldsThatHaveExposedWrappers
		{
			get
			{
				var result = new Dictionary<string, string[]>();
				result.Add("E2_Fax", new[] { "FormattedForBinding", "IsPublishedForBinding" });
				result.Add("E2_Phone", new[] { "FormattedForBinding", "IsPublishedForBinding" });
				result.Add("E2_Mobile", new[] { "FormattedForBinding", "IsPublishedForBinding" });
				return result;
			}
		}

		public void TestFormIsGarbageCollected()
		{
			var formReference = CreateShipmentShowFormAndCloseFormAndGetWeakReference();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals("IsAlive", false, formReference.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateShipmentShowFormAndCloseFormAndGetWeakReference()
		{
			var cleanFactory = new BusinessObjectFactory();
			var bO = cleanFactory.New<CFSShipment>();

			var form = new ShipmentReceivalForm(bO);
			var formReference = new WeakReference(form, false);

			form.Show();
			form.Close();
			Application.DoEvents();

			return formReference;
		}

		public void TestGuiFactoryServices()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();

			ICommonShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new ShipmentReceivalForm(shipment))
			{
				shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		[ExpectNoExceptions]
		public void TestSavingWhenPacklineWeightIsTooBigToFitInChargeable_Volume()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var packline = shipment.OuterPackLines.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			packline.JL_ActualVolume = 60000;

			using (var form = new ShipmentReceivalForm(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = form.FireSaveButton();
				AssertEquals("Cannot save: Shipment volume has been set to a value that makes chargeable invalid", ContinueWithSave.No, result);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!shipment.IsInDatabase);

				packline.JL_ActualVolume = 30;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				result = form.FireSaveButton();
				AssertEquals("Cannot save: Shipment volume hasn't changed so chargeable is still invalid", ContinueWithSave.No, result);
				AssertContains("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!shipment.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = form.FireSaveButton();
				AssertEquals("Saved: Shipment volume has changed so chargeable is now valid", ContinueWithSave.Yes, result);
				AssertNotContains("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				Assert("Should be saved successfully", shipment.IsInDatabase);
			}
		}

		[ExpectNoExceptions]
		public void TestSavingWhenPacklineWeightIsTooBigToFitInChargeable_Weight()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var packline = shipment.OuterPackLines.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			packline.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			packline.JL_ActualWeight = 25000;

			using (var form = new ShipmentReceivalForm(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = form.FireSaveButton();
				AssertEquals("Cannot save: Shipment weight has been set to a value that makes chargeable invalid", ContinueWithSave.No, result);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!shipment.IsInDatabase);

				packline.JL_ActualWeight = 50;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				result = form.FireSaveButton();
				AssertEquals("Cannot save: Shipment weight hasn't changed so chargeable is still invalid", ContinueWithSave.No, result);
				AssertContains("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!shipment.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = form.FireSaveButton();
				AssertEquals("Saved: Shipment weight has changed so chargeable is now valid", ContinueWithSave.Yes, result);
				AssertNotContains("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				Assert("Should be saved successfully", shipment.IsInDatabase);
			}
		}

		#region Lead-Master changing

		public void TestUnsetShipmentLeadOrMaster_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.StandardHouse, false, DialogResult.Cancel, unsetShipmentAsLeadOrMasterText);
		}

		public void TestUnsetShipmentLeadOrMaster_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.StandardHouse, true, DialogResult.OK, unsetShipmentAsLeadOrMasterText);
		}

		public void TestSetShipmentMaster_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.CoLoadMaster, false, DialogResult.Cancel, setShipmentAsCoLoadOrAssemblyMasterText);
		}

		public void TestSetShipmentMaster_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.OK, setShipmentAsCoLoadOrAssemblyMasterText);
		}

		public void TestSetHighVolumeLowValue_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.HighVolumeLowValue, false, DialogResult.Cancel, setHighVolumeLowValueText);
		}

		public void TestSetHighVolumeLowValue_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.HighVolumeLowValue, true, DialogResult.OK, setHighVolumeLowValueText);
		}

		public void TestSetHighVolumeLowValueLegacy_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.HighVolumeLowValueLegacy, false, DialogResult.Cancel, setHighVolumeLowValueText);
		}

		public void TestSetHighVolumeLowValueLegacy_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.HighVolumeLowValueLegacy, true, DialogResult.OK, setHighVolumeLowValueText);
		}

		void AssertShipmentTypeChangingEventHandler(ZString initialShipmentType, ZString selectedShipmentType, bool shouldChangeType, DialogResult dialogAnswer, string notificationMessage)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(dialogAnswer);

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_ShipmentType = initialShipmentType;

			switch (selectedShipmentType)
			{
				case Constants.ShipmentTypes.HighVolumeLowValue:
				case Constants.ShipmentTypes.HighVolumeLowValueLegacy:
				case Constants.ShipmentTypes.StandardHouse:
					shipment.CoLoadShipments.AddNew();
					break;
				case Constants.ShipmentTypes.AssemblyMaster:
				case Constants.ShipmentTypes.CoLoadMaster:
					shipment.OuterPackLines.AddNew();
					break;
			}

			using (new ShipmentReceivalForm(shipment))
			{
				shipment.JS_ShipmentType = selectedShipmentType;
				AssertEquals("Should show correct message", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Shipment type", shouldChangeType ? selectedShipmentType : initialShipmentType, shipment.JS_ShipmentType);
			}
		}

		const string setShipmentAsCoLoadOrAssemblyMasterText = @"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?";

		const string unsetShipmentAsLeadOrMasterText = @"Changing this shipment to be a Standard shipment will remove all related shipments from it.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?";

		const string setHighVolumeLowValueText = @"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?";

		#endregion

		public void TestChangingTransportModeChangesMasterBillLabel()
		{
			using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
			{
				shipmentForm.Show();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Label for master bill should change", "Master Bill No", shipmentForm.ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Label for master bill should change", "Ocean Bill No", shipmentForm.ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Label for master bill should change", "Master Bill No", shipmentForm.ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new ShipmentReceivalForm(Factory.New<CFSShipment>()));
		}

		public void TestRNSCFSShipmentPlugIn()
		{
			using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
			{
				shipmentForm.Show();

				AssertNull("RNSCFSShipmentPlugIn should not be plugged in", shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn));
				AssertNull("RNS menus should not be plugged in", shipmentForm.Menu.MenuItems.FindByName("RNS"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				Shipment.JS_RL_NKOrigin = HomePort;
				Shipment.JS_RL_NKDestination = "";

				using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
				{
					shipmentForm.Show();

					var plugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
					AssertNotNull("RNSCFSShipmentPlugIn should be plugged in", plugIn);
					Assert("RNSCFSShipmentPlugIn should not be enabled", !plugIn.Enabled);

					var menu = shipmentForm.Menu.MenuItems.FindByText("RNS");
					AssertNotNull("RNS menus should be plugged in", menu);
					Assert("RNS menus should be invisible", !menu.Visible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				Shipment.JS_RL_NKOrigin = "";
				Shipment.JS_RL_NKDestination = HomePort;

				using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
				{
					shipmentForm.Show();

					var plugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
					AssertNotNull("RNSCFSShipmentPlugIn should be plugged in", plugIn);
					Assert("RNSCFSShipmentPlugIn should be enabled", plugIn.Enabled);

					var menu = shipmentForm.Menu.MenuItems.FindByText("RNS");
					AssertNotNull("RNS menus should be plugged in", menu);
					Assert("RNS menus should be visible", menu.Visible);

					Assert("Menu 'Send Release Status Query' should be visible", menu.MenuItems.FindByText("Send Release Status Query").Visible);
					Assert("Menu 'Arrival Certification Message' should be visible", menu.MenuItems.FindByText("Arrival Certification Message").Visible);
				}
			}
		}

		public void TestShipmentLinkingMessagesSupporter()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = "REL";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+1+CUSRES:D:96A:UN'BGM+:::257+88888+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+4'RFF+XC:10207000067891'UNT+7+1'";
			message.EM_MessageSubType = "CLR";
			message.EM_ApplicationReference = "10207000067891";
			message.EM_Status = EDIMessage.Status.Received;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var num = Shipment.Numbers.AddNew();

				using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
				{
					shipmentForm.Show();

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
					num.CE_EntryNum = "10207000067891";

					AssertEquals(1, Shipment.Messages.Count);
					AssertEquals(message.PK, Shipment.Messages[0].PK);

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;

					AssertEquals(0, Shipment.Messages.Count);
				}

				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

				AssertCollectionNotContains(message, Shipment.Messages);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
				{
					shipmentForm.Show();

					var num = Factory.New<CusEntryNumber>();
					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
					num.CE_EntryNum = "1020 7000067891";
					Shipment.Numbers.Add(num);

					AssertEquals(0, Shipment.Messages.Count);

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;

					AssertEquals(0, Shipment.Messages.Count);
				}
			}
		}

		#region Implementation

		CFSShipment fShipment;
		CFSShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					SetupLocalBranchAsLocalCartage();
					fShipment = (CFSShipment)GetImportShipment(typeof(CFSShipment));
					CFSLoadListConsol consol = Shipment.Consols.AddNew();
					consol.Transports[0].JW_JX = ImportSailing1.PK;
					consol.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
					Factory.Save();
				}
				return fShipment;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupLocalBranchAsLocalCartage();
		}

		#endregion
	}
}

using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace Enterprise.Freight.SailingDataVendor.GUI.Testing
{
	[TestedType(typeof(VesselRoutingVoyagesImportPreviewForm))]
	sealed class VesselRoutingVoyagesImportPreviewFormTest : ZFormBasherTest
	{
		public override void TestBashingForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				base.TestBashingForm();
			}
		}

		public void TestFormCaption()
		{
			Form.Show();
			Application.DoEvents();
			AssertEquals("Sailing Schedule Feed", Form.Text);
		}

		public void TestVendorDataStatusLabel_WhenVendorDataCurrent()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
				Form.Show();
				AssertEquals("Vendor data current", Form.VendorDataStatusLabel.Text);
				AssertEquals("Font colour should be normal", Form.ForeColor, Form.VendorDataStatusLabel.ForeColor);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestVendorDataStatusLabel_WhenVendorDataNotCurrent()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
				Form.Show();
				AssertEquals("Vendor data out of date", Form.VendorDataStatusLabel.Text);
				AssertEquals("Font colour should be Red", Color.Red, Form.VendorDataStatusLabel.ForeColor);
				AssertEquals("Font should be Bold", true, Form.VendorDataStatusLabel.Font.Bold);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestVoyagesNoAllowNewOrDelete()
		{
			Form.Show();
			Application.DoEvents();
			AssertEquals("Cannot add voyages", false, ((BusinessObjectCollection)Form.VoyagesGrid.List).AllowNew);
			AssertEquals("Cannot delete voyages", RemoveAction.NoRemovePossible, Form.VoyagesGrid.RemoveAction);
		}

		public void TestPortPairsNoAllowNewOrDelete()
		{
			Form.Show();
			Application.DoEvents();

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(32), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(63), ZDateTime.Today.AddDays(94), "Voyage", "Voyage");
			Factory.Save();

			FilterBusinessObject.E9_Voyage = "Voyage";
			FilterBusinessObject.PerformSearch();

			AssertEquals("Cannot add port pairs", false, ((BusinessObjectCollection)Form.PortPairsGrid.List).AllowNew);
			AssertEquals("Cannot delete port pairs", RemoveAction.NoRemovePossible, Form.PortPairsGrid.RemoveAction);
		}

		#region Find / Clear

		public void TestPerformSearch()
		{
			TestPerformSearch(Form.PerformSearch);
		}

		public void TestKeyboardControlEnterToSearch()
		{
			TestPerformSearch(delegate
			{
				typeof(Form).InvokeMember("ProcessDialogKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Form, new object[] { Keys.Control | Keys.Enter });
			});
		}

		void TestPerformSearch(MethodInvoker performSearch)
		{
			Form.Show();
			Application.DoEvents();

			var today = ZDateTime.Now;

			NewJobVesselSchedule("AUSYD", today.AddDays(-30), today.AddDays(-1), "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUMEL", today.AddDays(1), today.AddDays(30), "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUSYD", today.AddDays(-30), today.AddDays(-1), "Voyage2", "Voyage2");
			NewJobVesselSchedule("AUMEL", today.AddDays(1), today.AddDays(30), "Voyage2", "Voyage2");
			Factory.Save();

			FilterBusinessObject.E9_DateType = "Invalid";
			performSearch();
			AssertEquals("There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			FilterBusinessObject.ResetToDefaultValues();
			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			FilterBusinessObject.E9_Voyage = "Voyage1";
			FilterBusinessObject.E9_DateFrom = today;
			performSearch();
			AssertEquals("Expected no error messages should be shown to the user", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Expected no search result feedback", "", Form.SearchResultsLabel.Text);
			AssertEquals("Expected no search result feedback warning icon", "", Form.SearchResultsLabelIconProvider.GetError(Form.SearchResultsLabel));

			AssertEquals("Should load the correct voyage", 1, FilterBusinessObject.Voyages.Count);
			AssertEquals("Should load the correct voyage", "Voyage1", FilterBusinessObject.Voyages[0].E8_Voyage);
			AssertEquals("Grid should be focused after searching is performed", true, Form.VoyagesGrid.ContainsFocus);
		}

		public void TestPerformSearch_WhenNoResultsFound()
		{
			Form.Show();
			Application.DoEvents();

			FilterBusinessObject.E9_DateFrom = ZDateTime.Now;
			FilterBusinessObject.E9_Voyage = "NoResults";
			Form.PerformSearch();

			AssertEquals("There are no records that match your search criteria.", Form.SearchResultsLabel.Text);
			AssertEquals("Expected no warning icon", "", Form.SearchResultsLabelIconProvider.GetError(Form.SearchResultsLabel));
		}

		public void TestKeyboardAltEnterToClear()
		{
			Form.Show();
			Application.DoEvents();

			FilterBusinessObject.E9_Voyage = "XXX";
			typeof(Form).InvokeMember("ProcessDialogKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Form, new object[] { Keys.Alt | Keys.Enter });
			AssertEquals("Alt-Enter should clear the filter screen", "", FilterBusinessObject.E9_Voyage);
		}

		#endregion

		#region Select All / None Buttons

		public void TestVoyagesSelectAllNoneButtons()
		{
			Form.Show();
			Application.DoEvents();

			VesselRoutingVoyage voyage1 = FilterBusinessObject.Voyages.AddNew();
			VesselRoutingPortPair portPair1 = voyage1.PortPairs.AddNew();
			portPair1.E9_IsSelected = true;

			VesselRoutingVoyage voyage2 = FilterBusinessObject.Voyages.AddNew();
			VesselRoutingPortPair portPair2 = voyage2.PortPairs.AddNew();
			portPair2.E9_IsSelected = true;

			Form.VoyagesSelectNoneButton.PerformClick();
			AssertEquals("After select none is pressed", false, voyage1.E8_IsSelected);
			AssertEquals("After select none is pressed", false, voyage2.E8_IsSelected);
		}

		public void TestPortPairsSelectAllNoneButtons()
		{
			Form.Show();
			Application.DoEvents();

			FilterBusinessObject.Voyages.RemoveAll();
			VesselRoutingVoyage voyage = FilterBusinessObject.Voyages.AddNew();
			VesselRoutingPortPair portPair1 = voyage.PortPairs.AddNew();
			portPair1.E9_RL_NKLoadPort = "AUSYD";
			portPair1.E9_RL_NKDischargePort = "MYPKG";
			VesselRoutingPortPair portPair2 = voyage.PortPairs.AddNew();
			portPair2.E9_RL_NKLoadPort = "AUMEL";
			portPair2.E9_RL_NKDischargePort = "MYPKG";

			Form.PortPairsSelectNoneButton.PerformClick();
			AssertEquals("After select none is pressed", false, portPair1.E9_IsSelected);
			AssertEquals("After select none is pressed", false, portPair2.E9_IsSelected);

			Form.PortPairsSelectAllButton.PerformClick();
			AssertEquals("After select all is pressed", true, portPair1.E9_IsSelected);
			AssertEquals("After select all is pressed", true, portPair2.E9_IsSelected);

			Form.PortPairsSelectNoneButton.PerformClick();
			AssertEquals("After select none is pressed", false, portPair1.E9_IsSelected);
			AssertEquals("After select none is pressed", false, portPair2.E9_IsSelected);
		}

		#endregion

		#region Adding Foreign Ports

		#region Adding Foreign Ports

		public void TestAddForeignPortButton()
		{
			CreateSydneyToMelbournePortPairAndPerformSearch();

			AssertEquals("Should find 1 voyage initially", 1, FilterBusinessObject.Voyages.Count);
			AssertEquals("Should find 5 port pair initially", 5, FilterBusinessObject.Voyages[0].PortPairs.Count);
			AssertEquals("1st load port     ", "", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKLoadPort);
			AssertEquals("1st discharge port", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKDischargePort);
			AssertEquals("2nd load port     ", "", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKLoadPort);
			AssertEquals("2nd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKDischargePort);
			AssertEquals("3rd load port     ", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKLoadPort);
			AssertEquals("3rd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKDischargePort);

			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "MYPKG";
			Form.AddForeignPortButton.PerformClick();

			AssertEquals("Added foreign ports should appear", 5, FilterBusinessObject.Voyages[0].PortPairs.Count);
			AssertEquals("1st load port     ", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKLoadPort);
			AssertEquals("1st discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKDischargePort);
			AssertEquals("2nd load port     ", "MYPKG", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKLoadPort);
			AssertEquals("2nd discharge port", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKDischargePort);
			AssertEquals("3rd load port     ", "MYPKG", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKLoadPort);
			AssertEquals("3rd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKDischargePort);
		}

		public void TestAddForeignPortButton_WhenInvalidForeignPortEntered()
		{
			CreateSydneyToMelbournePortPairAndPerformSearch("Estimated Arrival");

			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "";
			Form.AddForeignPortButton.PerformClick();
			AssertEquals("Enter a foreign load port", UnitTestUserNotification.Instance.LastMessage.Text);

			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "AUSYD"; // ie. not foreign
			Form.AddForeignPortButton.PerformClick();
			AssertEquals("Enter a foreign load port", UnitTestUserNotification.Instance.LastMessage.Text);

			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "XXXXX"; // ie. not valid
			Form.AddForeignPortButton.PerformClick();
			AssertEquals("Enter a valid foreign load port", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAddForeignPortButton_WhenAlreadyAdded_ForLoadPort()
		{
			TestAddForeignPortButton_WhenAlreadyAdded(true);
		}

		public void TestAddForeignPortButton_WhenAlreadyAdded_ForDischargePort()
		{
			TestAddForeignPortButton_WhenAlreadyAdded(false);
		}

		void TestAddForeignPortButton_WhenAlreadyAdded(bool forLoadPort)
		{
			CreateSydneyToMelbournePortPairAndPerformSearch(forLoadPort ? "Estimated Arrival" : "Estimated Departure");
			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "SGSIN";

			Form.AddForeignPortButton.PerformClick();
			AssertEquals("Expect no dialog after foreign port is first added", null, UnitTestUserNotification.Instance.LastMessage.Text);

			FilterBusinessObject.Voyages[0].E8_ForeignPortToAdd = "SGSIN";
			Form.AddForeignPortButton.PerformClick();
			string loadOrDischarge = forLoadPort ? "load" : "discharge";
			AssertEquals("This " + loadOrDischarge + " port is already selected", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Removing Foreign Ports

		public void TestRemoveForeignPortButton()
		{
			CreateSydneyToMelbournePortPairAndPerformSearch();

			FilterBusinessObject.Voyages[0].ForeignPorts.Add("MYPKG");
			FilterBusinessObject.Voyages[0].PortPairs.Load();

			AssertEquals("1st load port     ", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKLoadPort);
			AssertEquals("1st discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKDischargePort);
			AssertEquals("2nd load port     ", "MYPKG", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKLoadPort);
			AssertEquals("2nd discharge port", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKDischargePort);
			AssertEquals("3rd load port     ", "MYPKG", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKLoadPort);
			AssertEquals("3rd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKDischargePort);

			Form.PortPairsGrid.SelectAllElements();
			Form.RemoveSelectedForeignPortButton.PerformClick();

			AssertEquals("1st load port     ", "", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKLoadPort);
			AssertEquals("1st discharge port", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[0].E9_RL_NKDischargePort);
			AssertEquals("2nd load port     ", "", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKLoadPort);
			AssertEquals("2nd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[1].E9_RL_NKDischargePort);
			AssertEquals("3rd load port     ", "AUSYD", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKLoadPort);
			AssertEquals("3rd discharge port", "AUMEL", FilterBusinessObject.Voyages[0].PortPairs[2].E9_RL_NKDischargePort);
		}

		public void TestRemoveForeignPortButton_WhenNoPortPairsSelected_ForLoadPort()
		{
			TestRemoveForeignPortButton_WhenNoPortPairsSelected(true);
		}

		public void TestRemoveForeignPortButton_WhenNoPortPairsSelected_ForDischargePort()
		{
			TestRemoveForeignPortButton_WhenNoPortPairsSelected(false);
		}

		void TestRemoveForeignPortButton_WhenNoPortPairsSelected(bool forLoadPort)
		{
			Form.Show();
			Application.DoEvents();

			FilterBusinessObject.E9_DateType = forLoadPort ? "Estimated Arrival" : "Estimated Departure";
			Form.RemoveSelectedForeignPortButton.PerformClick();

			string loadOrDischarge = forLoadPort ? "load" : "discharge";
			AssertEquals("You must select a port pair whose " + loadOrDischarge + " port you want to remove", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Resetting Port Pairs

		public void TestResetPortPairsButton()
		{
			NewJobVesselRouting("MYPKG", "Lloyds", "Voyage");
			Factory.Save();
			CreateSydneyToMelbournePortPairAndPerformSearch();

			Form.BusinessEntity.PerformSearch();
			AssertEquals("1 foreign port initially", 1, Form.BusinessEntity.Voyages[0].ForeignPorts.Count);

			Form.BusinessEntity.Voyages[0].ForeignPorts.Add("SGSIN");
			AssertEquals("2 foreign ports after manually adding one", 2, Form.BusinessEntity.Voyages[0].ForeignPorts.Count);

			Form.ResetPortPairsButton.PerformClick();
			AssertEquals("Back to 1 foreign port when resetting the port pairs", 1, Form.BusinessEntity.Voyages[0].ForeignPorts.Count);
		}

		[ExpectNoExceptions]
		public void TestResetPortPairsButton_WhenNoVoyagesAvailable()
		{
			Form.BusinessEntity.Voyages.RemoveAll();
			Form.ResetPortPairsButton.PerformClick();
		}

		#endregion

		#endregion

		#region Import Button

		public void TestImportButton()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(32), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(63), ZDateTime.Today.AddDays(94), "Voyage", "Voyage");
			Factory.Save();
			FilterBusinessObject.E9_Voyage = "Voyage";
			FilterBusinessObject.PerformSearch();

			FilterBusinessObject.Voyages[0].PortPairs[0].E9_IsSelected = true;
			Form.ImportButton.PerformClick();
			AssertEquals("No error messages should be shown to the user", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportButton_WhenVoyageToImportHasErrors()
		{
			Form.Show();
			Application.DoEvents();

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(32), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(63), ZDateTime.Today.AddDays(94), "Voyage", "Voyage");
			Factory.Save();
			FilterBusinessObject.E9_Voyage = "Voyage";
			FilterBusinessObject.PerformSearch();

			FilterBusinessObject.Voyages[0].E8_OH_LineOperator = ZGuid.Invalid;
			AssertEquals("Voyage to import should have an error for the test", true, FilterBusinessObject.Voyages[0].HasErrors);

			Form.ImportButton.PerformClick();
			AssertEquals("There are errors. Please correct these before importing.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportButton_WhenNoPortPairsSelected()
		{
			Form.Show();
			Application.DoEvents();

			AssertEquals("Expected no selected port pairs for the test", 0, FilterBusinessObject.Voyages.Count);
			Form.ImportButton.PerformClick();
			AssertEquals("Select the port pairs you wish to import.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Form Event Handlers

		public void TestConfirmFormClose_WhenVoyagesSelected()
		{
			Form.Show();
			Application.DoEvents();

			VesselRoutingVoyage voyage = Form.BusinessEntity.Voyages.AddNew();
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "AUSYD";
			portPair.E9_RL_NKDischargePort = "MYPKG";
			portPair.E9_IsSelected = true;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Form.Close();
			AssertEquals("Any port pair selections made will be lost.\r\nAre you sure you want to close the form?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form should not be disposed if the user chooses No", false, Form.IsDisposed);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Form.Close();
			AssertEquals("Form should be disposed if the user chooses Yes", true, Form.IsDisposed);
		}

		public void TestDontConfirmFormClose_WhenNoVoyagesSelected()
		{
			Form.Show();
			Application.DoEvents();

			VesselRoutingVoyage voyage = Form.BusinessEntity.Voyages.AddNew();
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_IsSelected = false;

			Form.Close();
			AssertEquals("User should not have to confirm closing of the form", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("User should not have to confirm closing of the form", true, Form.IsDisposed);
		}

		#endregion

		#region Test Classes

		class TestVesselRoutingVoyagesImportForm : VesselRoutingVoyagesImportPreviewForm
		{
			public TestVesselRoutingVoyagesImportForm(VesselRoutingVoyagesFilter businessEntity)
				: base(businessEntity)
			{
			}

			public new void PerformSearch()
			{
				base.PerformSearch();
			}

			public new ZGrid VoyagesGrid
			{
				get { return base.VoyagesGrid; }
			}

			public new ZGrid PortPairsGrid
			{
				get { return base.PortPairsGrid; }
			}

			public new ZButton AddForeignPortButton
			{
				get { return base.AddForeignPortButton; }
			}

			public new ZButton RemoveSelectedForeignPortButton
			{
				get { return base.RemoveSelectedForeignPortButton; }
			}

			public new ZButton ResetPortPairsButton
			{
				get { return base.ResetPortPairsButton; }
			}

			public new ZButton VoyagesSelectNoneButton
			{
				get { return base.VoyagesSelectNoneButton; }
			}

			public new ZButton PortPairsSelectNoneButton
			{
				get { return base.PortPairsSelectNoneButton; }
			}

			public new ZButton PortPairsSelectAllButton
			{
				get { return base.PortPairsSelectAllButton; }
			}

			public new ZLabel NoPortPairsAvailableLabel
			{
				get { return base.NoPortPairsAvailableLabel; }
			}

			public new ZLabel SearchResultsLabel
			{
				get { return base.SearchResultsLabel; }
			}

			public new ErrorProvider SearchResultsLabelIconProvider
			{
				get { return base.SearchResultsLabelIconProvider; }
			}

			public new ZLabel VendorDataStatusLabel
			{
				get { return base.VendorDataStatusLabel; }
			}

			public new ZButton ImportButton
			{
				get { return base.ImportButton; }
			}
		}

		#endregion

		#region Implementation

		void CreateSydneyToMelbournePortPairAndPerformSearch()
		{
			CreateSydneyToMelbournePortPairAndPerformSearch("");
		}

		void CreateSydneyToMelbournePortPairAndPerformSearch(ZString dateType)
		{
			Form.Show();
			Application.DoEvents();

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Voyage", "Voyage");
			Factory.Save();

			FilterBusinessObject.E9_DateType = dateType;
			FilterBusinessObject.E9_Voyage = "Voyage";
			FilterBusinessObject.PerformSearch();
		}

		JobVesselSchedule NewJobVesselSchedule(ZString portName, ZDateTime eTA, ZDateTime eTD, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_IMOLloydsNumber = "Lloyds";
			return result;
		}

		JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = lloyds;
			result.E1_VoyageNumber = voyage;
			return result;
		}

		TestVesselRoutingVoyagesImportForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestVesselRoutingVoyagesImportForm(FilterBusinessObject);
				}
				return fForm;
			}
		}
		TestVesselRoutingVoyagesImportForm fForm;

		VesselRoutingVoyagesFilter FilterBusinessObject
		{
			get
			{
				if (fBusinessEntity == null)
				{
					fBusinessEntity = new VesselRoutingVoyagesFilter(Factory);
				}
				return fBusinessEntity;
			}
		}
		VesselRoutingVoyagesFilter fBusinessEntity;

		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get { return false; }
		}

		protected override Form GetFormToBashCore()
		{
			return new TestVesselRoutingVoyagesImportForm(FilterBusinessObject);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fForm != null)
			{
				fForm.Dispose();
			}
		}

		#endregion
	}
}

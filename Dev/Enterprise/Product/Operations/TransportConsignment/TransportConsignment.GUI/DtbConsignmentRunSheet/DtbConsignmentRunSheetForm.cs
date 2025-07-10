using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbConsignmentRunSheetForm : ZTemplateForm, INotifications
	{
		#region Construction

		public DtbConsignmentRunSheetForm(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
			InitializeComponent();
			MakeInstructionsGridEditable();
			ControllerID = ControllerIDs.DtbConsignmentRunSheet;
			AddPlugIns();
			WorkflowTabPage.Initialize(runSheet);
		}

		#endregion

		#region PlugIns

		void AddPlugIns()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Apportionment);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
			PlugIns.GetPlugIn(ControllerIDs.Apportionment).TabPage.CaptionResourceString = Res.GetData("DtbConsignmentRunSheetForm|CostingTab", "Costing");

			var apportionedChargesGrid = PlugIns.GetPlugIn(ControllerIDs.Apportionment).UserControl.Controls.Find("ApportionedChargesGrid", true).OfType<ZGrid>().First();
			apportionedChargesGrid.SetColumnCaption("JR_PrepaidCollect", Res.GetString("173deaf5-7be9-4eab-af49-aa90e3424397", "PIC/DLV")); // don't want to reference Accouting.Business. The tests would fail if the column name is changed.
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region eDocs

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		#endregion

		#region Events

		protected ZGrid InstructionsGrid
		{
			get { return RunSheetInstructionsControl != null ? RunSheetInstructionsControl.InstructionsGrid : null; }
		}

		#endregion

		#region OnVisible

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				OnVisible();
			}
		}

		void OnVisible()
		{
			SuspendLayout();
			try
			{
				ControlDpiScalingHelper.SetWidth(StaffDriverFindBox.CodeBox, VechicleFindBox.CodeBox.Width, false);
				UpdateDetailsPaneVisibility();

				// get rid of the ugly blue but we may have to change this to respect the user-defined colours
				BackColor = SystemColors.Control;
				TopGroupBox.BackColor = SystemColors.Control;
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region RunSheet

		DtbConsignmentRunSheet RunSheet
		{
			get { return (DtbConsignmentRunSheet)DataSource; }
		}

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region ReadOnly

		void MakeInstructionsGridEditable()
		{
			InstructionsGrid.IsWholeRowSelectedOnClick = false;
			InstructionsGrid.ReadOnly = false;
		}

		#endregion

		#region Actions

		#region Details Button

		void DetailsButton_Click(object sender, EventArgs e)
		{
			UpdateDetailsPaneVisibility();
		}

		void UpdateDetailsPaneVisibility()
		{
			InstructionsSplitPanel.Panel2Collapsed = !DetailsButton.Checked;
		}

		#endregion

		#region GlowLinkLabel_LinkClicked

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenRunSheetViaGlowPortalInBrowser();
		}

		void OpenRunSheetViaGlowPortalInBrowser()
		{
			var urlResult = GlowHelper.GenerateGotoGlowUrlForExistingEntityAsync(GlowHelper.EntityName.RunSheet, RunSheet.PK).Result;

			if (urlResult.ErrorMessage != null)
			{
				var errorMessage = ResString.GetMultilingualString("364f1625-d812-4308-8bf6-b5e7c322e768", "This Run Sheet cannot be opened in a browser.");
				Globals.Message.ShowError(errorMessage + System.Environment.NewLine + urlResult.ErrorMessage);
				return;
			}

			WebUrlLauncher.Launch(urlResult.Uri.ToString());
		}

		#endregion

		#endregion

		#region Save

		protected override void HandleSaveException(Exception ex)
		{
			var saveException = ex as ZSaveException;
			if (saveException != null && saveException.FriendlyMessage.Contains(DtbConsignmentRunSheet.RunSheetAlreadyExistsTriggerErrorMessage))
			{
				Globals.Message.ShowError(Res.GetString("DtbConsignmentRunSheetForm|HandleSaveException|TriggerMessage", "Run Sheet already exists for this Driver, Truck, Transport Co and Date Range."));

				// re-run validation (validation looks at the DB) to load in runsheets created by other users that caused our concurrency error.
				// this is so that we can show the error icons in the GUI.
				RunSheet.Factory.ClearQueryCache(); // catch new Run Sheets created by other users
				RunSheet.Factory.ReloadAllSafe<DtbConsignmentRunSheet>(); // catch existing Run Sheets modified by other users
				RunSheet.Validation.ValidateAll();
				RunSheet.RefreshBinding();
			}
			else
			{
				base.HandleSaveException(ex);
			}
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region INotifications Members
		public void Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}
		#endregion
	}
}

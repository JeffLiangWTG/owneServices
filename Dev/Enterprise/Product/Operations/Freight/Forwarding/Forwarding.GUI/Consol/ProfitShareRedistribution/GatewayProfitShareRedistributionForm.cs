using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed partial class GatewayProfitShareRedistributionForm : ZChildForm
	{
		public GatewayProfitShareRedistributionForm(ForwardingProfitShareRedistribution profitShareRedistribution)
			: base(profitShareRedistribution)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl);

			ConsolsGroupBox.Text = Res.GetString("a577d394-6943-4bd2-a437-950a38d81426", "G/W Consol for Profit Redistribution");
			RulesGroupBox.Text = Res.GetString("12e1f78d-e507-46f2-afaa-6c4d7dfb2389", "Profit Share Setup");
			ShipmentsGroupBox.Text = Res.GetString("b55fe7e7-5d0b-4e9f-94cb-8b8e55758b7c", "G/W Consol Profit Redistributed to Shipments");
			Text = Res.GetString("56b066bc-68af-4e80-804c-ed5d48f0babf", "Gateway Profit Share Redistribution");

			RedistributeProfitSharesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;

			splitContainer1.SplitterMoved += SplitterMoved;
			splitContainer2.SplitterMoved += SplitterMoved;

			ConsolModuleButtonGrid.OnAttach += ConsolModuleButtonGrid_OnAttach;
			ConsolModuleButtonGrid.BeforeDetach += ConsolModuleButtonGrid_BeforeDetach;

			ProfitShareRuleModuleButtonGrid.OnAttach += ProfitShareRuleModuleButtonGrid_OnAttach;
			ProfitShareRuleModuleButtonGrid.Detached += ProfitShareRuleModuleButtonGrid_Detached;

			RedistributionLogButton.Click += RedistributionLogButton_Click;
			RedistributeProfitSharesButton.Click += RedistributeProfitSharesButton_Click;
		}

		#region Revalidate

		void Revalidate()
		{
			ResumeLayout(false);
			PerformLayout();
			Invalidate(true);
		}

		protected override void OnResize(EventArgs e)
		{
			Revalidate();
		}

		void SplitterMoved(object sender, SplitterEventArgs e)
		{
			Revalidate();
		}

		#endregion

		ForwardingProfitShareRedistribution RedistributionProcess => BusinessEntity as ForwardingProfitShareRedistribution;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (DisplayMode == ZArchitecture.Core.ODisplayMode.ReadOnly)
			{
				SetFormInReadOnlyMode();
			}
			else
			{
				SetActionButtonsStates();
			}
		}

		void SetFormInReadOnlyMode()
		{
			ProfitShareRuleModuleButtonGrid.ShowAttachButton = false;
			ProfitShareRuleModuleButtonGrid.ShowDetachButton = false;
			zPostingButtonsUserControl.SaveButton.Enabled = false;
			zPostingButtonsUserControl.SaveAndCloseButton.Enabled = false;
			ConsolModuleButtonGrid.SetToolStripVisibility(false);
			RedistributionLogButton.Enabled = true;
			RedistributeProfitSharesButton.Visible = false;
			var prevNextControl = Controls.Find("ZPreviousNextControl", false).FirstOrDefault();
			if (prevNextControl != null)
			{
				prevNextControl.Visible = false;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				ConsolModuleButtonGrid.OnAttach -= ConsolModuleButtonGrid_OnAttach;
				ConsolModuleButtonGrid.BeforeDetach -= ConsolModuleButtonGrid_BeforeDetach;

				ProfitShareRuleModuleButtonGrid.OnAttach -= ProfitShareRuleModuleButtonGrid_OnAttach;
				ProfitShareRuleModuleButtonGrid.Detached -= ProfitShareRuleModuleButtonGrid_Detached;

				RedistributionLogButton.Click -= RedistributionLogButton_Click;
				RedistributeProfitSharesButton.Click -= RedistributeProfitSharesButton_Click;

				components.Dispose();
			}

			base.Dispose(disposing);
		}

		void ConsolModuleButtonGrid_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs eventArgs)
		{
			if (BusinessEntity is ForwardingProfitShareRedistribution gatewayProfitShareRedistribution)
			{
				gatewayProfitShareRedistribution.AddConsols(eventArgs.AttachedBusinessObjects.OfType<ForwardingConsol>());
				SetActionButtonsStates();
			}
		}

		void ConsolModuleButtonGrid_BeforeDetach(object sender, ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			if (BusinessEntity is ForwardingProfitShareRedistribution gatewayProfitShareRedistribution)
			{
				gatewayProfitShareRedistribution.RemoveConsols(eventArgs.ToDetachBusinessObjects.OfType<ProfitShareForwardingConsolWrapper>());
				eventArgs.Cancel = true;
				SetActionButtonsStates();
			}
		}

		void ProfitShareRuleModuleButtonGrid_Detached(object sender, ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			if (BusinessEntity is ForwardingProfitShareRedistribution gatewayProfitShareRedistribution)
			{
				gatewayProfitShareRedistribution.RemoveProfitShareRules(eventArgs.DetachedBusinessObjects.OfType<OrgProfitShareDetails>());
				SetActionButtonsStates();
			}
		}

		void ProfitShareRuleModuleButtonGrid_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs eventArgs)
		{
			if (BusinessEntity is ForwardingProfitShareRedistribution gatewayProfitShareRedistribution)
			{
				gatewayProfitShareRedistribution.AddProfitShareRules(eventArgs.AttachedBusinessObjects.OfType<OrgAgentRelationship>());
				SetActionButtonsStates();
			}
		}

		void RedistributeProfitSharesButton_Click(object sender, EventArgs e)
		{
			if (ProcessForm == null)
			{
				ProcessForm = new ProfitShareRedistributionLogForm((ForwardingProfitShareRedistribution)BusinessEntity);
				ProcessForm.Closed += OnProcessFormClosed;
				// The form is required to be modal.
				ProcessForm.ShowDialog();
			}
			SetActionButtonsStates();
		}
		ProfitShareRedistributionLogForm ProcessForm;

		void OnProcessFormClosed(object sender, EventArgs eventArgs)
		{
			RedistributionStatus = ProcessForm.Result
				? RedistributionStatus.Succeeded
				: RedistributionStatus.DoneWithError;

			ProcessForm.Dispose();
			ProcessForm = null;
			SetActionButtonsStates();
		}

		void RedistributionLogButton_Click(object sender, EventArgs e)
		{
			StmNotePopupHelper.ShowNoteForm(this, BusinessEntity as IStmNoteParent, Res.GetString("a5d00307-1bab-4e18-83c0-05273be8ca8c", "Profit Share Redistribution Log"), false, null);
		}

		public void SetActionButtonsStates()
		{
			var isReadyForRedistribution = ProcessForm == null && RedistributionProcess.Shipments.Any() && RedistributionProcess.ProfitShareRules.Any();
			if (!isReadyForRedistribution)
			{
				RedistributionStatus = RedistributionStatus.NotReady;
			}
			RedistributeProfitSharesButton.Enabled = isReadyForRedistribution;
			SetButtonsStatus(RedistributionStatus == RedistributionStatus.Succeeded);
		}

		void SetButtonsStatus(bool isEnable)
		{
			zPostingButtonsUserControl.SaveButton.Enabled = isEnable;
			zPostingButtonsUserControl.SaveAndCloseButton.Enabled = isEnable;
		}

		public RedistributionStatus RedistributionStatus { get; set; } = RedistributionStatus.NotStarted;

		#region Save

		public override ODisplayMode DisplayMode
		{
			get => base.DisplayMode;
			set => base.DisplayMode = RedistributionProcess.IsInDatabase ? ODisplayMode.ReadOnly : value;
		}

		protected override void SaveInternal()
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(RedistributionProcess.Factory.Save, null, true);
			SetFormInReadOnlyMode();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var caption = Res.GetString("6714370A-DC61-47B5-B691-B4F847BECB2F", "Save Results");
			var message = Res.GetString("CDA8F1A6-6DFB-4465-A2DD-22E1DE1D024C",
				@"You are about to save the charges resulting from Gateway Consol Profit Redistribution to the relevant Shipments. This action cannot be reversed.
Press YES to proceed with Saving the results or Press NO to review again.");

			return (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			// do nothing please
		}

		#endregion
	}

	public enum RedistributionStatus
	{
		NotStarted,
		NotReady,
		DoneWithError,
		Succeeded
	}
}

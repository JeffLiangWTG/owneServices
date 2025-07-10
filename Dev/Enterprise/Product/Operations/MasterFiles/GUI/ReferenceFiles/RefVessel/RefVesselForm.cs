using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefVesselForm : ZTemplateForm, ISupportViewDpsLogsTab
	{
		public RefVesselForm(RefVessel vessel)
			: base(vessel)
		{
			this.Vessel = vessel;

			InitializeComponent();
			PlugIns.Add(ControllerIDs.Audit);

			#region Custom Fields

			int startingY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(IsActiveCheckBox.Location.Y) + 20;
			int nextY = startingY;
			int scaleX;

			if (vessel.CustomAttribute1Used)
			{
				CustomAttribute1TextBox.GetExtension<ILabelCaptionRenderer>().Caption = vessel.CustomAttribute1Caption;
				CustomAttribute1TextBox.Visible = true;

				scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CustomAttribute1TextBox.Location.X);

				CustomAttribute1TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(scaleX, nextY);
				nextY += 24;
			}

			if (vessel.CustomAttribute2Used)
			{
				CustomAttribute2TextBox.GetExtension<ILabelCaptionRenderer>().Caption = vessel.CustomAttribute2Caption;
				CustomAttribute2TextBox.Visible = true;

				scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CustomAttribute2TextBox.Location.X);

				CustomAttribute2TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(scaleX, nextY);
				nextY += 24;
			}

			if (vessel.CustomAttribute3Used)
			{
				CustomAttribute3TextBox.GetExtension<ILabelCaptionRenderer>().Caption = vessel.CustomAttribute3Caption;
				CustomAttribute3TextBox.Visible = true;

				scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CustomAttribute2TextBox.Location.X);

				CustomAttribute3TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(scaleX, nextY);
				nextY += 24;
			}

			if (vessel.CustomFlag1Used)
			{
				CustomFlag1CheckBox.GetExtension<ILabelCaptionRenderer>().Caption = vessel.CustomFlag1Caption;
				CustomFlag1CheckBox.Visible = true;

				scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CustomFlag1CheckBox.Location.X);

				CustomFlag1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(scaleX, nextY);
				nextY += 20;
			}

			if (vessel.CustomDecimal1Used)
			{
				CustomDecimal1CalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = vessel.CustomDecimal1Caption;
				CustomDecimal1CalcEdit.Visible = true;

				scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CustomDecimal1CalcEdit.Location.X);

				CustomDecimal1CalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(scaleX, nextY);
				nextY += CustomDecimal1CalcEdit.Height;
			}

			#endregion

			AddScreeningLogsTabPage();
			new DeniedPartyScreeningPresentationManager().CreateMenusForScreeningEntity(this);
		}

		protected RefVessel Vessel;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				SetRadioCallSignOverrideLabelVisibility();
				Vessel.RV_RadioCallSignInfo.ValueChanged += RadioCallSignRelatedInfo_ValueChanged;
			}
		}

		void RadioCallSignRelatedInfo_ValueChanged(object sender, EventArgs e)
		{
			SetRadioCallSignOverrideLabelVisibility();
		}

		void SetRadioCallSignOverrideLabelVisibility()
		{
			var systemVesselRadioCallSign = Vessel.SystemVessel?.ZZO_RadioCallSign ?? ZString.Empty;
			RadioCallSignOverrideLabel.Visible = !systemVesselRadioCallSign.IsEmpty && Vessel.RV_RadioCallSign != systemVesselRadioCallSign;
			if (RadioCallSignOverrideLabel.Visible)
			{
				RadioCallSignOverrideLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("d839015a-0fe6-4da0-896e-a5dce50eceb3", "(Default: {0})", Vessel.SystemVessel?.ZZO_RadioCallSign ?? CargoWise.Types.ZString.Empty);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave baseContinueWithSave = base.ValidateAndSave();

			if (baseContinueWithSave == ContinueWithSave.Yes && Vessel.ShouldUpdateRelatedJobs)
			{
				PromptProgressBarAndUpdateRelatedJobs(this, Vessel);
				Vessel.ShouldUpdateRelatedJobs = false;
			}

			return baseContinueWithSave;
		}

		void PromptProgressBarAndUpdateRelatedJobs(Form parentForm, RefVessel vessel)
		{
			var progressForm = new ProgressForm();
			try
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowModalTo(parentForm);

				ScreeningStatusUpdater.ProgressUpdaterDelegate progressUpdater = (string partyCode) =>
				{
					string msg = Res.GetString("abdac6d4-c822-4d55-95d8-3145da720e6c", "Updating screening status of jobs related to party: {0}", partyCode);
					progressForm.SetStatusAndPercentComplete(msg, 50);
				};

				ScreeningStatusUpdater.UpdateRelatedJobsForChangedParty(vessel, progressUpdater);
				progressForm.SetStatusAndPercentComplete(Res.GetString("da3a786b-5296-443d-a433-df38dbdb8c3c", "Processed all jobs related to this party."), 100);
			}
			finally
			{
				progressForm.Close();
			}
		}

		string DpsLogsTabName => Res.GetString("MasterFiles|RefVesselForm|DeniedPartyScreeningLogs", "Denied Party Screening Logs");

		void AddScreeningLogsTabPage()
		{
			var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
			screenStatusControl.SetBindingMember("RelatedShippingLineScreeningStatusCollection");
			LogsTabPage.AddAdditionalTab(DpsLogsTabName, screenStatusControl);
		}

		public void SwitchToDpsLogsTab()
		{
			if (LogsTabPage != null)
			{
				initialTabPage = LogsTabPage;
				MainTabControl.SelectedTab = LogsTabPage;
				LogsTabPage.SetSelectedTab(DpsLogsTabName);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (initialTabPage != null)
			{
				MainTabControl.SelectedTab = initialTabPage;
			}
		}

		ZTabPage initialTabPage;

		internal async void ScreenButton_Click(object sender, EventArgs e)
		{
			IScreeningPartyProvider provider = BusinessEntity as IScreeningPartyProvider;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening(this, true, !DeniedPartyScreenerAsync.HasExcludedList(BusinessEntity.Factory));
			}
		}

		void IsActiveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.IsActiveCheckBox.Checked && Vessel.RV_ScreeningStatus != ScreeningStatusesList.Codes.Unknown)
			{
				var dialogResult = Globals.Message.Show(Res.GetString("ced5a236-4ebf-4357-8434-212ea79ede68", @"Setting the vessel to inactive will remove this entity from the Denied Party re-screening process and reset the screening status to UNK.
Would you like to continue?"),
							Res.GetString("e151eb82-09d0-416b-9986-7acd1f4c1ba3", "Continue?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult == DialogResult.No)
				{
					this.IsActiveCheckBox.Checked = true;
				}
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (!IsDisposed)
			{
				if (Vessel != null)
				{
					Vessel.RV_RadioCallSignInfo.ValueChanged -= RadioCallSignRelatedInfo_ValueChanged;
				}
			}
		}
	}
}

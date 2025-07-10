using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsDeclarationUserControl : FrontPageUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public BaseCustomsDeclarationUserControl()
		{
			InitializeComponent();
			LoadNumbersUserControl();
			shipmentCustomFieldsControl1.NothingSetupMessageLabelText = Res.GetString("cc823f4f-01c8-4783-b611-2a6036c8d25c", "To make use of this tab, please setup customized fields against Workflow Templates.");

			if (!DesignModeFinder.IsDesigning)
			{
				ImporterOrganisationControl.Enter += new EventHandler(ImporterOrganisationControl_Enter);
			}

			OverrideValuesCheckBox.AllowOutsideOfParent();
		}

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => JobDeclaration;

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			declaration.MultipleKeyToUseChanged += Declaration_MultipleKeyToUseChanged;
			declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
			Declaration_MultipleKeyToUseChanged(this, null);
			JE_MessageTypeInfo_ValueChanged(this, null);
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			declaration.MultipleKeyToUseChanged -= Declaration_MultipleKeyToUseChanged;
			declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		protected virtual void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e)
		{
		}

		protected virtual void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTransportDetailsLayout();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				if (declaration.Shipment != null || ((IComplianceItemRiskStatusProvider)declaration).IsEnabledComplianceWise)
				{
					this.ScreenButton.Visible = false;
					this.ScreeningStatusDropEdit.Visible = false;
				}
			}
		}

		void ImporterOrganisationControl_Enter(object sender, EventArgs e)
		{
			if (JobDeclaration != null && !ImporterOrganisationControl.ReadOnly && JobDeclaration.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				ImporterOrganisationControl.SelectFromPopupForm();
			}
		}

		protected override Control ControlWithFocusWhenFormIsOpened
		{
			get { return SupplierOrganisationControl; }
		}

		#region Job Declaration

		public override BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				if (JobDeclaration != null)
				{
					UnHookFreightToCustomsSyncronisation(JobDeclaration);
					UnHookCommissionRelatedProperties(JobDeclaration);
				}
				base.JobDeclaration = value;
				if (JobDeclaration != null)
				{
					SetupForDeclaration(JobDeclaration);
					HookFreightToCustomsSyncronisation(JobDeclaration);
					HookCommissionRelatedProperties(JobDeclaration);
				}
			}
		}

		protected virtual void SetupForDeclaration(BaseJobDeclaration declaration)
		{
			if (!declaration.AttachedOrdersVisible)
			{
				OrdersTabPage.Dispose();
			}
			if (!declaration.ServiceLevelVisible && JE_RS_NKServiceLevelBoundFindBox.Visible)
			{
				MakeServiceLevelInvisible();
			}

			SetRightTabControlSelectTab();

			declarationFormLayoutProvider = null;
			SetDeclarationFormLayout();

			JobDeclaration_ControlVisibilityChanged(this, EventArgs.Empty);
		}

		void HookFreightToCustomsSyncronisation(BaseJobDeclaration declaration)
		{
			OverrideValuesCheckBox.Visible = !declaration.IsStandAlone;
			declaration.SetSynchroniserFieldsReadOnly(!declaration.IsStandAlone && declaration.ShouldSynchroniseWithShipment());
			declaration.OnOverrideFreightDefaultsChanging += new CancelEventHandler(JobDeclaration_OnOverrideFreightDefaultsChanging);
		}

		void UnHookFreightToCustomsSyncronisation(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.OnOverrideFreightDefaultsChanging -= new CancelEventHandler(JobDeclaration_OnOverrideFreightDefaultsChanging);
			}
		}

		void HookCommissionRelatedProperties(BaseJobDeclaration jobDeclaration)
		{
			jobDeclaration.JE_TransportModeInfo.ValueChanged += JE_TransportModeInfo_ValueChanged;
			jobDeclaration.JE_RL_NKOriginInfo.ValueChanged += JE_RL_NKOriginInfo_ValueChanged;
			jobDeclaration.JE_RL_NKFinalDestinationInfo.ValueChanged += JE_RL_NKFinalDestinationInfo_ValueChanged;
		}

		void JE_RL_NKFinalDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(JobDeclaration.JE_RL_NKFinalDestinationInfo);
		}

		void JE_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(JobDeclaration.JE_RL_NKOriginInfo);
		}

		protected virtual void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(JobDeclaration.JE_TransportModeInfo);
		}

		void UnHookCommissionRelatedProperties(BaseJobDeclaration jobDeclaration)
		{
			jobDeclaration.JE_TransportModeInfo.ValueChanged -= JE_TransportModeInfo_ValueChanged;
			jobDeclaration.JE_RL_NKOriginInfo.ValueChanged -= JE_RL_NKOriginInfo_ValueChanged;
			jobDeclaration.JE_RL_NKFinalDestinationInfo.ValueChanged -= JE_RL_NKFinalDestinationInfo_ValueChanged;
		}

		void ConfirmReversal(ZPropertyInfo info)
		{
			if (JobDeclaration.IsInDatabase && !info.Value.Equals(info.OriginalValue))
			{
				if (HasCommissions())
				{
					Globals.Message.Show(
						JobHeader.GetCommissionReversalWarningMessage(new[]
						{
								JobDeclaration.JE_RL_NKOriginInfo,
								JobDeclaration.JE_RL_NKFinalDestinationInfo,
								JobDeclaration.JE_TransportModeInfo
						}),
						JobHeader.CommissionReversalWarningMessageHeader,
						MessageBoxButtons.OK, MessageBoxIcon.Question);
				}
			}
		}

		protected virtual bool HasCommissions()
		{
			return JobDeclaration.Job != null && JobDeclaration.Job.HasNonReversedCommissionHeaders();
		}

		protected virtual void SetRightTabControlSelectTab()
		{
			if (RightTabControl != null)
			{
				RightTabControl.SelectedTab = DocsTabPage;
			}
		}

		protected virtual void MakeServiceLevelInvisible()
		{
			JE_RS_NKServiceLevelBoundFindBox.Visible = false;
		}

		void JobDeclaration_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			if (ParentForm is ZForm parentForm && !parentForm.IsSavingInProgress && !JobDeclaration.Factory.IsInSaveTransaction)
			{
				DialogResult result = Globals.Message.Show(Res.GetString("560d9c1d-7fde-4285-925d-39e0aeea9677", "Removing the override will reset your customs declaration data.\r\nYou will lose changes that you have made to the customs declaration data.\r\n\r\nProceed?"), Res.GetString("cd3e5299-ad4a-4416-b004-421a2d6f7986", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				e.Cancel = result == DialogResult.No;
			}
		}

		#endregion

		#region INCOTERMS Explanation

		void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			ShowIncoTermDescriptionForm();
		}

		protected void ShowIncoTermDescriptionForm()
		{
			if (!JobDeclaration.JE_ShipmentIncoTermInfo.HasErrors() && !JobDeclaration.JE_ShipmentIncoTermInfo.HasMessageErrors())
			{
				IncoTermDescriptionForm form = new IncoTermDescriptionForm(JobDeclaration.JE_ShipmentIncoTerm);
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
		}

		#endregion

		#region Screening Status

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			IScreeningPartyProvider provider = JobDeclaration;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, false, !DeniedPartyScreenerAsync.HasExcludedList(JobDeclaration.Factory));
			}
		}

		#endregion

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible && JobDeclaration != null)
			{
				JobDeclaration_ControlVisibilityChanged(this, EventArgs.Empty);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!shipmentCustomFieldsControl1.IsDisposed)
			{
				shipmentCustomFieldsControl1.ForceBindingIncludingParents();
			}
		}

		#region HandleDeclarationControlVisibilityChangedCore

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			SetContainerMode();

			bool bondedWarehouseVisible = JobDeclaration.BondedWarehouseEditable;
			BondedWarehouseDocAddressControl.Visible = bondedWarehouseVisible;

			HandleMasterBillControlVisibility();

			JE_ExportDateBoundDateEdit.GetExtension<ILabelCaptionRenderer>().Refresh();
			JE_DateOfArrivalBoundDateEdit.GetExtension<ILabelCaptionRenderer>().Refresh();
			HouseBillParcelPostTextEdit.GetExtension<ILabelCaptionRenderer>().Refresh();

			SetVesselFindBoxVisible();

			SetVoyageFlightNoVisible();
			SetFolioNumberVisible();

			SetContainerCountAndNoOfPieces();
			SetOrganisationsLayout();
		}

		#region DeclarationFormLayout

		void SetShipmentDetailsLayout()
		{
			var shipmentDetailsLayout = declarationFormLayoutProvider?.GetDeclarationShipmentDetailsLayout();
			SetGroupBoxDetailsLayout(shipmentDetailsLayout, ref ShipmentDetailsLayoutPanel, "ShipmentDetailsLayoutPanel", ControlDpiScalingHelper.NewScaledSize(447, 261, true), ShipmentDetailsGroupBox);
		}

		void SetGroupBoxDetailsLayout(IPanelLayoutProvider layoutProvider, ref DynamicLayoutPanel layoutPanel, string name, [DpiState(DpiState.ScaledVariant)] System.Drawing.Size size, ZGroupBox groupBoxForPanel)
		{
			if (layoutProvider != null)
			{
				if (layoutPanel == null)
				{
					layoutPanel = new DynamicLayoutPanel();
					layoutPanel.Name = name;
					layoutPanel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
					layoutPanel.Size = size;
					layoutPanel.TabIndex = 0;
					layoutPanel.TabStop = true;
					layoutPanel.Dock = DockStyle.Fill;

					groupBoxForPanel.Controls.RemoveAndDisposeAll();
					groupBoxForPanel.Controls.Add(layoutPanel);
				}
				layoutPanel.UpdateLayout(layoutProvider);
			}
		}

		#endregion

		protected void HandleMasterBillControlVisibility()
		{
			JE_MasterBillForSeaBoundTextBox.Visible = IsJE_MasterBillForSeaBoundTextBoxVisible;
			if (JE_MasterBillForSeaBoundTextBox.Visible)
			{
				JE_MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
			JE_MasterBillForAirBoundTextBox.Visible = IsJE_MasterBillForAirBoundTextBoxVisible;
			if (JE_MasterBillForAirBoundTextBox.Visible)
			{
				JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		protected virtual bool IsJE_MasterBillForSeaBoundTextBoxVisible
		{
			get { return (JobDeclaration != null && JobDeclaration.IsSea) || Env.Registry.IsExpress; }
		}

		protected virtual bool IsJE_MasterBillForAirBoundTextBoxVisible
		{
			get { return JobDeclaration != null && JobDeclaration.IsAir && !Env.Registry.IsExpress; }
		}

		protected virtual void SetVesselFindBoxVisible()
		{
			VesselFindBox.Visible = JobDeclaration.IsSea;
		}

		protected void SetVoyageFlightNoVisible()
		{
			JE_VoyageFlightNoBoundTextBox.Visible = IsVoyageFlightNoVisible;
			if (JE_VoyageFlightNoBoundTextBox.Visible)
			{
				JE_VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		protected virtual bool IsVoyageFlightNoVisible
		{
			get
			{
				return JobDeclaration.IsAir || JobDeclaration.IsSea || JobDeclaration.IsRoad;
			}
		}

		protected virtual void SetFolioNumberVisible()
		{
			FolioNumberTextBox.Visible = JobDeclaration.IsAir;
		}

		protected virtual void SetContainerMode()
		{
			JE_ContainerModeBoundDropDownEdit.Visible = JE_ContainerModeBoundDropDownEditVisible;
		}

		protected virtual void SetContainerCountAndNoOfPieces()
		{
			if (JobDeclaration.IsExport && JobDeclaration.IsSea)
			{
				JE_ContainerCountCalcEdit.Visible = true;
				JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
				JE_ContainerCountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("22c28cbe-5343-4ba3-94df-40d82618c069", "Container Count");
			}
			else if (JobDeclaration.IsImport)
			{
				JE_ContainerCountCalcEdit.Visible = false;
				JE_TotalNoOfPiecesBoundCalcEdit.Visible = true;
				JE_TotalNoOfPiecesBoundCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("bc0066ad-247a-4862-a5cd-7ea7e18c21a3", "Units");
			}
			else
			{
				JE_ContainerCountCalcEdit.Visible = false;
				JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
			}
		}

		protected virtual bool JE_ContainerModeBoundDropDownEditVisible => JobDeclaration?.ContainerModeVisible ?? false;

#if DEBUG
		public bool JE_ContainerModeBoundDropDownEditVisibleForTesting
		{
			get
			{
				return JE_ContainerModeBoundDropDownEditVisible;
			}
		}
#endif

		protected virtual bool ContainerCountOrInnerPacksLabelVisible
		{
			get
			{
				return JobDeclaration.IsExport && JobDeclaration.IsSea || JobDeclaration.IsImport;
			}
		}

		protected virtual bool OwnerReferenceVisible
		{
			get { return JobDeclaration.IsImport; }
		}

		#endregion

		#region Exposing Controls for unit tests
#if DEBUG
		public ZCalcEdit TotalNoOfPiecesBoundCalcEdit
		{
			get { return JE_TotalNoOfPiecesBoundCalcEdit; }
		}

		public ZDropEdit ContainerModeBoundDropDownEdit
		{
			get { return JE_ContainerModeBoundDropDownEdit; }
		}

		public Control VesselBoundFindBox
		{
			get { return VesselFindBox; }
		}

		public ZTextBox VoyageFlightNoBoundTextBox
		{
			get { return JE_VoyageFlightNoBoundTextBox; }
		}

		public ZDropEdit MessageTypeBoundDropDownEdit
		{
			get { return JE_MessageTypeBoundDropDownEdit; }
		}

		public ZDropEdit TransportModeBoundDropDownEdit
		{
			get { return JE_TransportModeBoundDropDownEdit; }
		}

		public ZCodeFindBox PortOfLoadingBoundFindBox
		{
			get { return PortOfLoadingFindBox; }
		}

		public ZCodeFindBox OriginBoundFindBox
		{
			get { return OriginFindBox; }
		}

		public ZCodeFindBox PortOfDischargeBoundFindBox
		{
			get { return PortOfDischargeFindBox; }
		}

		public ZTextBox HouseBillParcelPostBoundTextEdit
		{
			get { return HouseBillParcelPostTextEdit; }
		}

		public ZCodeFindBox FinalDestinationBoundFindBox
		{
			get { return FinalDestinationFindBox; }
		}

		public ZTextBox ExportDeclarationNumberTextBox
		{
			get { return ExportDeclarationNumberBoundTextBox; }
		}

		public ZDateEdit DateOfArrivalBoundDateEdit2
		{
			get { return JE_DateOfArrivalBoundDateEdit2; }
		}

		public ZTextBox MasterBillForSeaBoundTextBox
		{
			get { return JE_MasterBillForSeaBoundTextBox; }
		}

		public ZDateEdit ExportDateBoundDateEdit2
		{
			get { return JE_ExportDateBoundDateEdit2; }
		}
#endif
		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (JobDeclaration is BaseJobDeclaration declaration)
				{
					UnHookFreightToCustomsSyncronisation(declaration);
					UnHookCommissionRelatedProperties(declaration);
					declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				}

				OrganisationsUserControl?.Dispose();
			}

			if (isNotFinalizing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Numbers UserControl

		NumbersUserControl numbersUserControl;

		protected virtual NumbersUserControl GetNumbersUserControl() => new NumbersUserControl();

		void LoadNumbersUserControl()
		{
			NumbersTabPage.SuspendLayout();

			numbersUserControl = GetNumbersUserControl();
			numbersUserControl.SuspendLayout();

			numbersUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(numbersUserControl, ".");
			numbersUserControl.Dock = DockStyle.Fill;
			numbersUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			numbersUserControl.Name = nameof(numbersUserControl);
			numbersUserControl.Size = ControlDpiScalingHelper.NewScaledSize(253, 477, true);
			numbersUserControl.TabIndex = 0;

			NumbersTabPage.Controls.Add(numbersUserControl);

			numbersUserControl.ResumeLayout(true);
			numbersUserControl.PerformLayout();

			NumbersTabPage.ResumeLayout(false);
			NumbersTabPage.PerformLayout();
		}

		#endregion

		#region DeclarationFormLayout

		IDeclarationFormLayoutProvider DeclarationFormLayoutProvider
		{
			get
			{
				if (declarationFormLayoutProvider == null)
				{
					declarationFormLayoutProvider = GUI.DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
				}
				return declarationFormLayoutProvider;
			}
		}
		IDeclarationFormLayoutProvider declarationFormLayoutProvider;

		void SetDeclarationFormLayout()
		{
			if (DeclarationFormLayoutProvider != null)
			{
				SetDeclarationDetailsLayout();
				SetShipmentTypeLayout();
				SetShipmentDetailsLayout();
			}
			SetOrganisationsLayout();
		}

		void SetDeclarationDetailsLayout()
		{
			var newDeclarationDetailsLayout = DeclarationFormLayoutProvider?.GetDeclarationDetailsLayout();
			if (newDeclarationDetailsLayout != null)
			{
				if (DeclarationDetailsLayoutPanel == null)
				{
					DeclarationDetailsLayoutPanel = new DynamicLayoutPanel();
					DeclarationDetailsLayoutPanel.Name = "DeclarationDetailsLayoutPanel";
					DeclarationDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 16, true);
					DeclarationDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 44, true);
					DeclarationDetailsLayoutPanel.TabIndex = 2;
					DeclarationDetailsLayoutPanel.TabStop = false;

					DeclarationDetailsGroupBox.Controls.RemoveAndDisposeAll();
					DeclarationDetailsGroupBox.Controls.Add(DeclarationDetailsLayoutPanel);
					AdjustHeightAndMakeRoomForDeclarationDetailsGroupBox();
					DeclarationDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				}
				DeclarationDetailsLayoutPanel.UpdateLayout(newDeclarationDetailsLayout);
			}
		}

		protected virtual void AdjustHeightAndMakeRoomForDeclarationDetailsGroupBox() => AdjustHeightAndMakeRoomForControl(DeclarationDetailsGroupBox, 71, RightTabControl, TransportDetailsGroupBox, ShipmentDetailsGroupBox);

		protected static void AdjustHeightAndMakeRoomForControl(Control controlToExpand, int newHeightUnscaled, params Control[] controlsBelow)
		{
			var newHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(newHeightUnscaled);
			var diffY = newHeight - controlToExpand.Height;
			if (diffY == 0)
			{
				return;
			}
			controlToExpand.Height += diffY;
			controlsBelow.ForEach(x =>
			{
				x.Top += diffY;

				if (x.Bottom > x.Parent.DisplayRectangle.Height)
				{
					x.Height -= diffY;
				}
			});
		}

		void SetShipmentTypeLayout()
		{
			var newShipmentypeLayout = DeclarationFormLayoutProvider?.GetDeclarationShipmentTypeLayout();
			if (newShipmentypeLayout != null)
			{
				if (ShipmentTypeLayoutPanel == null)
				{
					ShipmentTypeLayoutPanel = new DynamicLayoutPanel();
					ShipmentTypeLayoutPanel.Name = "ShipmentTypeLayoutPanel";
					ShipmentTypeLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
					ShipmentTypeLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 160, true);
					ShipmentTypeLayoutPanel.TabIndex = 2;
					ShipmentTypeLayoutPanel.TabStop = false;

					ShipmentTypeGroupBox.Controls.RemoveAndDisposeAll();
					ShipmentTypeGroupBox.Controls.Add(ShipmentTypeLayoutPanel);
					ShipmentTypeLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				}
				ShipmentTypeLayoutPanel.UpdateLayout(newShipmentypeLayout);
			}
		}

		void SetTransportDetailsLayout()
		{
			var newTransportDetailsLayout = DeclarationFormLayoutProvider?.GetDeclarationTransportDetailsLayout(JobDeclaration);
			if (newTransportDetailsLayout != null)
			{
				if (TransportDetailsLayoutPanel == null)
				{
					TransportDetailsLayoutPanel = new DynamicLayoutPanel();
					TransportDetailsLayoutPanel.Name = "TransportDetailsLayoutPanel";
					TransportDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
					TransportDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 168, true);
					TransportDetailsLayoutPanel.TabIndex = 4;
					TransportDetailsLayoutPanel.TabStop = true;

					TransportDetailsGroupBox.Controls.RemoveAndDisposeAll();
					TransportDetailsGroupBox.Controls.Add(TransportDetailsLayoutPanel);
					TransportDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				}
				TransportDetailsLayoutPanel.UpdateLayout(newTransportDetailsLayout);
			}
		}
		#endregion

		#region Organisation Dynamic Layout

		void SetOrganisationsLayout()
		{
			var newOrganisationsLayout = DeclarationFormLayoutProvider?.GetDeclarationOrganisationsLayout();
			if (newOrganisationsLayout != null)
			{
				if (OrganisationDetailsLayoutPanel == null)
				{
					OrganisationDetailsLayoutPanel = new DynamicLayoutPanel();
					OrganisationDetailsLayoutPanel.Name = "OrganisationDetailsLayoutPanel";
					OrganisationDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
					OrganisationDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
					OrganisationDetailsLayoutPanel.TabIndex = 0;
					OrganisationDetailsLayoutPanel.TabStop = false;

					OrganisationsTabPage.Controls.RemoveAndDisposeAll();
					OrganisationsTabPage.Controls.Add(OrganisationDetailsLayoutPanel);
					OrganisationDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				}
				OrganisationDetailsLayoutPanel.UpdateLayout(newOrganisationsLayout);
			}
		}

		#endregion
	}
}

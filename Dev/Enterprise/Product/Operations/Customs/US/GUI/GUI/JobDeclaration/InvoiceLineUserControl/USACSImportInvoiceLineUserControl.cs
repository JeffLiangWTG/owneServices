using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USACSImportInvoiceLineUserControl : USImportInvoiceLineUserControl
	{
		public USACSImportInvoiceLineUserControl()
		{
			InitializeComponent();
			SetPGAFDATabPageTabVisible(false);
			LoadPGAFDATabPage();

			TextileClassificationDetailsGroupBox.AllowOverlap(CanadaSoftwoodLumberGroupBox);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (CurrentDataItem != null)
			{
				RefreshMiscLicenceLabel();
			}

			if (Visible)
			{
				OGATabsVisibility();
			}
		}

		void OGATabsVisibility()
		{
			var declaration = currentInvoiceLine != null ? currentInvoiceLine.Declaration : null;
			if (declaration != null && declaration.IsFTZAdmission)
			{
				if (declaration.US_EnableSPN)
				{
					var fdaOtherTabPageVisible = !declaration.CanHavePGAFDA && (currentInvoiceLine.IsFDADeclared || currentInvoiceLine.HasFDAData);
					SetTabPageTabVisible(FDAOtherTabPage, null, "", fdaOtherTabPageVisible);
					SetPGAFDATabPageTabVisible(declaration.CanHavePGAFDA && (currentInvoiceLine.IsFDADeclared || currentInvoiceLine.HasACE_FDALines));
				}
				else
				{
					SetTabPageTabVisible(FDAOtherTabPage, null, "", false);
					SetPGAFDATabPageTabVisible(false);
				}
			}
		}

		void SetPGAFDATabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(PGAFDATabPage, acefdaUserControl, "FilteredInvoiceLines.ACE_FDALines", visible);
		}

		void SetTabPageTabVisible(ZTabPage tabPage, ZUserControl userControl, string dataMember, bool visible)
		{
			var oldValue = tabPage.TabVisible;
			if (visible)
			{
				tabPage.TabVisible = visible;
			}
			if (userControl != null)
			{
				if (oldValue != visible)
				{
					userControl.Visible = visible;
					userControl.Enabled = visible;
				}
				if (visible)
				{
					userControl.SetDataBinding(CurrentDataItem, dataMember);
				}
				else
				{
					userControl.SetDataBinding(null, "");
				}
			}
			if (!visible)
			{
				using (CustomsInvoiceLinesBoundGrid?.SuspendCancelOfNonEditedRowOnLeaving())
				{
					tabPage.TabVisible = visible;
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			this.ADDutyCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingConst);
			this.CVDutyCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingConst);

			if (dataSource != null)
			{
				this.ADDutyCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoiceLines.IsADDManual", false, DataSourceUpdateMode.Never));
				this.CVDutyCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoiceLines.IsCVDManual", false, DataSourceUpdateMode.Never));
			}
		}
		const string IsVisibleForBindingConst = "IsVisibleForBinding";

		protected override void HookInvoiceLineEvents(JobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);
			RefreshMiscLicenceLabel();

			if (invoiceLine != null)
			{
				invoiceLine.US_FDAIndicatorInfo.ValueChanged += US_FDAIndicatorInfo_ValueChanged;
			}
			OGATabsVisibility();
		}

		void US_FDAIndicatorInfo_ValueChanged(object sender, EventArgs e)
		{
			OGATabsVisibility();
		}

		void RefreshMiscLicenceLabel()
		{
			if (currentInvoiceLine != null)
			{
				MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = currentInvoiceLine.CalcMiscLicenseTypeLabel;
			}
		}

		protected override void OnCurrentInvoiceLineTariffChanged()
		{
			base.OnCurrentInvoiceLineTariffChanged();
			RefreshMiscLicenceLabel();
		}

		protected override void ShowOrHideLicencePermitsPage(ZBool isVisible)
		{
			if (isVisible)
			{
				SetTabPageTabVisible(LicencePermitsDetailsTabPage, null, "", isVisible);
				TextileClassificationDetailsGroupBox.Visible = !isVisible;
				CanadaSoftwoodLumberGroupBox.Visible = !isVisible;
				LicenceNumbersGroupBox.Controls.OfType<Control>().ForEach(c => c.Visible = !isVisible);

				LicenseTypeCodeDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(110, 15, true);
				LicenseTypeCodeDropEdit.Visible = isVisible;
				MiscNoTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(350, 15, true);
				MiscNoTextBox.Visible = isVisible;
			}
			else
			{
				base.ShowOrHideLicencePermitsPage(isVisible);
			}

			ShorOrHideADDCVDControls(isVisible);
		}

		void ShorOrHideADDCVDControls(ZBool isVisible)
		{
			PermitLicenseRightBottomPanel.Location = ControlDpiScalingHelper.NewScaledPoint(isVisible ? 3 : 267, 116, true);
			PermitLicenseRightBottomPanel.Size = ControlDpiScalingHelper.NewScaledSize(isVisible ? 999 : 735, 182, true);

			ADDDepositValueCalcFindBox.Visible = !isVisible;
			IsADDBondedCheckBox.Visible = !isVisible;
			ADDDepositRateDropEdit.Visible = !isVisible;
			ADDDepositRateZTextBox.Visible = !isVisible;

			CVDDepositValueCalcFindBox.Visible = !isVisible;
			IsCVDBondedCheckBox.Visible = !isVisible;
			CVDDepositRateDropEdit.Visible = !isVisible;
			CVDDepositRateZTextBox.Visible = !isVisible;
		}

		void LoadPGAFDATabPage()
		{
			this.LineDetailTabControl.Controls.Add(PGAFDATabPage);

			if (PGAFDATabPage.Controls.Count == 0)
			{
				acefdaUserControl = new ACEFDAUserControl();
				this.BindingSource.SetBindingMember(this.acefdaUserControl, "FilteredInvoiceLines.ACE_FDALines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).ACE_FDALines);
				this.acefdaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.acefdaUserControl.Name = "acefdaUserControl";
				PGAFDATabPage.Controls.Add(acefdaUserControl);
			}

			SetTabPageTabVisible(PGAFDATabPage, null, "", false);
		}
		internal ACEFDAUserControl acefdaUserControl;
	}
}

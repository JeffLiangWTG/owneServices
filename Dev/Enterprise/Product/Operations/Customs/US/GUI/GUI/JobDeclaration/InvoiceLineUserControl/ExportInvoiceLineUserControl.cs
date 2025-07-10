using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USExportInvoiceLineUserControl : USInvoiceLineUserControl
	{
		public USExportInvoiceLineUserControl()
		{
			InitializeComponent();
			LineDetailsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				VehicleDetailsControlVisibilityChanged();
			});

			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(InvoiceLinesGridColumnNamesInSortOrder);
			}

			SetupTariffCodeFindBox();
			this.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			CustomsInvoiceLinesBoundGrid.SetColumnModuleID(Customs.Business.BaseJobComInvoiceLine.Schema.JI_CC, ZArchitecture.Modules.ModuleIDs.NotAssigned);
			this.DDTCCategoryXXIDeterminationNumberTextBox.Visible = ZZCustomsFunctionality.IsAESJurisdictionNumberEffective;

			ExportPGATabsVisibility();
			InitializeLazyCreate();
		}

		void SetupTariffCodeFindBox()
		{
			TariffCodeFindBox.Hide();
			UniversalTariffCodeFindBox.GetTariffType = GetUniversalTariffType;
		}

		internal new ZString GetUniversalTariffType()
		{
			var tariffType = ZString.Empty;
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			var currentInvoiceLine = listManager != null && listManager.Count > 0 ? (JobComInvoiceLine)listManager.GetCurrent() : null;
			if (currentInvoiceLine != null)
			{
				tariffType = currentInvoiceLine.US_TariffType == TariffTypeList.Codes.HTS ? (ZString)ClassificationType.EXP : currentInvoiceLine.US_TariffType;
			}
			return tariffType;
		}

		protected override void HookInvoiceLineEvents(JobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.US_TariffTypeInfo.ValueChanged += US_TariffTypeInfo_ValueChanged;
				invoiceLine.US_AMSIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_PSTIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_ATFIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_FWSIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_DEAIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_NMFSHMSIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_TTBIndInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.OnExportPGAIndicatorChangedEvent += delegate(ZString agencyCode, ZBool hasExportPGAData)
				{
					var result = true;

					if (hasExportPGAData)
					{
						result = Globals.Message.Show(Res.GetString("d80d941e-fcb5-4481-a283-097717a1f58e", "You have just cleared the AES {0} PGA indicator, system will remove all entered AES {0} data.\r\nDo you wish to proceed?", agencyCode), Res.GetString("ce4f5299-ad4a-4416-b004-421a2d6f7986", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
					}

					return result;
				};
			}

			ExportPGATabsVisibility();
		}

		protected override void UnHookInvoiceLineEvents(JobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.US_TariffTypeInfo.ValueChanged -= US_TariffTypeInfo_ValueChanged;
				invoiceLine.US_AMSIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_PSTIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_ATFIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_FWSIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_DEAIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_NMFSHMSIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.US_TTBIndInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				invoiceLine.OnExportPGAIndicatorChangedEvent = null;
			}

			ExportPGATabsVisibility();
		}

		void US_TariffTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeExportClassificationFindBoxModuleID();
		}

		void ChangeExportClassificationFindBoxModuleID()
		{
			var exportClassificationModuleId = GetExportTariffModuleId();
			if (exportClassificationModuleId != null)
			{
				CustomsInvoiceLinesBoundGrid.SetColumnModuleID(Customs.Business.BaseJobComInvoiceLine.Schema.JI_CC, exportClassificationModuleId);
			}
		}

		ZArchitecture.Modules.ModuleIdentifier GetExportTariffModuleId()
		{
			ZArchitecture.Modules.ModuleIdentifier identifier = null;
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			var currentLine = listManager != null && listManager.Count > 0 ? (JobComInvoiceLine)listManager.GetCurrent() : null;

			if (currentLine != null)
			{
				if (currentLine.UseScheduleB)
				{
					identifier = Enterprise.ZArchitecture.Modules.ModuleIDs.ExportClassification;
				}
				else
				{
					identifier = Enterprise.ZArchitecture.Modules.ModuleIDs.ImportClassification;
				}
			}

			return identifier;
		}

		protected override ZArchitecture.Modules.ModuleIdentifier ClassificationModuleID
		{
			get
			{
				var exportClassificationModuleId = GetExportTariffModuleId();
				return exportClassificationModuleId ?? Enterprise.ZArchitecture.Modules.ModuleIDs.ExportClassification;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			ECCNCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ECCNTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				ECCNCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.ECCNCodeFindBoxVisible", false, DataSourceUpdateMode.Never));
				ECCNTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.ECCNTextBoxVisible", false, DataSourceUpdateMode.Never));
			}
		}

		const string IsVisibleForBindingString = "IsVisibleForBinding";

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

		#region InvoiceLinesGridColumnNamesInSortOrder
		string[] InvoiceLinesGridColumnNamesInSortOrder
		{
			get
			{
				if (invoiceLinesGridColumnNamesInSortOrder == null)
				{
					List<string> columnNamesInSortOrderList = new List<string>();
					columnNamesInSortOrderList.Add("JI_LineNo");
					columnNamesInSortOrderList.Add("JI_Calc_Invoice");
					columnNamesInSortOrderList.Add("JI_PartNo");
					columnNamesInSortOrderList.Add("JI_CC");
					columnNamesInSortOrderList.Add("US_TariffType");
					columnNamesInSortOrderList.Add("JI_FormattedTariff");
					columnNamesInSortOrderList.Add("JI_InvoiceQuantity");
					columnNamesInSortOrderList.Add("JI_InvoiceUQ");
					columnNamesInSortOrderList.Add("JI_CustomsQuantity");
					columnNamesInSortOrderList.Add("JI_CustomsUnitQty");
					columnNamesInSortOrderList.Add("JI_CustomsSecondQuantity");
					columnNamesInSortOrderList.Add("JI_CustomsSecondUnitQty");
					columnNamesInSortOrderList.Add("JI_LinePrice");
					columnNamesInSortOrderList.Add("JI_Description");
					columnNamesInSortOrderList.Add("US_MarksAndNumbers");
					columnNamesInSortOrderList.Add("US_ExportCode");
					columnNamesInSortOrderList.Add("US_ECCN");
					columnNamesInSortOrderList.Add("US_LicenseType");
					columnNamesInSortOrderList.Add("US_LicenseNo");
					columnNamesInSortOrderList.Add("US_LicenseValue");
					columnNamesInSortOrderList.Add("US_AESOriginIndicator");
					columnNamesInSortOrderList.Add("US_IsUsedVehicle");
					columnNamesInSortOrderList.Add("US_VehicleID");
					columnNamesInSortOrderList.Add("US_VehicleIDType");
					columnNamesInSortOrderList.Add("US_VehicleTitleNo");
					columnNamesInSortOrderList.Add("US_VehicleTitleState");
					columnNamesInSortOrderList.Add("JI_Calc_MergedLineNumber");
					columnNamesInSortOrderList.Add("JI_Calc_EntryNumber");
					columnNamesInSortOrderList.Add("JI_Calc_XTN");
					invoiceLinesGridColumnNamesInSortOrder = columnNamesInSortOrderList.ToArray();
				}
				return invoiceLinesGridColumnNamesInSortOrder;
			}
		}
		string[] invoiceLinesGridColumnNamesInSortOrder;
		#endregion

		void UsedVehicleCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			VehicleDetailsControlVisibilityChanged();
		}

		void VehicleDetailsControlVisibilityChanged()
		{
			bool showUsedVehicleControls = UsedVehicleCheckBox.Checked;
			VehicleIDTextBox.Visible = showUsedVehicleControls;
			VehicleIDTypeDropEdit.Visible = showUsedVehicleControls;
			VehicleTitleNoTextBox.Visible = showUsedVehicleControls;
			VehicleTitleStateDropEdit.Visible = showUsedVehicleControls;
		}

		#region PGA

		void ExportPGAIndicator_ValueChanged(object sender, EventArgs e)
		{
			ExportPGATabsVisibility();
		}

		void ExportPGATabsVisibility()
		{
			SetAMSTabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_AMSInd), currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_PSTIndicator));
			SetNMFSTabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFSHMSInd));
			SetATFTabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_ATFInd));
			SetDEATabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_DEAInd));
			SetFWSTabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_FWSInd));
			SetTTBTabPageVisibility(currentInvoiceLine != null && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(currentInvoiceLine.US_TTBInd));
		}

		void InitializeLazyCreate()
		{
			this.AMSPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.NMFSPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.ATFPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.DEAPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.FWSPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.TTBPGATabPage.LazyCreateControls += this.LazyCreateControlsFired;
		}

		void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == AMSPGATabPage)
			{
				LoadAMSAndEPAUserControls();
			}
			else if (sender == NMFSPGATabPage)
			{
				LoadNMFSUserControls();
			}
			else if (sender == ATFPGATabPage)
			{
				LoadATFUserControls();
			}
			else if (sender == DEAPGATabPage)
			{
				LoadDEAUserControls();
			}
			else if (sender == FWSPGATabPage)
			{
				LoadFWSUserControls();
			}
			else if (sender == TTBPGATabPage)
			{
				LoadTTBUserControls();
			}
		}

		void LoadAMSAndEPAUserControls()
		{
			if (AMSPGATabPage.Controls.Count == 0 && AMSPGATabPage.TabVisible)
			{
				exportAMSEPAUserControl = new ExportAMSEPAUserControl();
				this.BindingSource.SetBindingMember(this.exportAMSEPAUserControl, "FilteredInvoiceLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)));
				this.exportAMSEPAUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportAMSEPAUserControl.Name = "exportAMSEPAUserControl";
				AMSPGATabPage.Controls.Add(exportAMSEPAUserControl);
				SetAMSTabPageVisibility(OGAIndicatorList.IsToBeDeclared(currentInvoiceLine?.US_AMSInd), OGAIndicatorList.IsToBeDeclared(currentInvoiceLine?.US_PSTIndicator));
			}
		}
		ExportAMSEPAUserControl exportAMSEPAUserControl;

		internal void LoadNMFSUserControls()
		{
			if (NMFSPGATabPage.Controls.Count == 0 && NMFSPGATabPage.TabVisible)
			{
				exportNMFSUserControl = new ExportNMFSUserControl();
				this.BindingSource.SetBindingMember(this.exportNMFSUserControl, "FilteredInvoiceLines.NMFSLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).NMFSLines);
				this.exportNMFSUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportNMFSUserControl.Name = "exportNMFSUserControl";
				NMFSPGATabPage.Controls.Add(this.exportNMFSUserControl);
			}
		}
		internal ExportNMFSUserControl exportNMFSUserControl;

		internal void LoadATFUserControls()
		{
			if (ATFPGATabPage.Controls.Count == 0 && ATFPGATabPage.TabVisible)
			{
				exportATFUserControl = new ExportATFUserControl();
				this.BindingSource.SetBindingMember(this.exportATFUserControl, "FilteredInvoiceLines.ExportATF");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).ExportATF);
				this.exportATFUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportATFUserControl.Name = "exportATFUserControl";
				ATFPGATabPage.Controls.Add(this.exportATFUserControl);
			}
		}
		internal ExportATFUserControl exportATFUserControl;

		internal void LoadDEAUserControls()
		{
			if (DEAPGATabPage.Controls.Count == 0 && DEAPGATabPage.TabVisible)
			{
				exportDEAUserControl = new ExportDEAUserControl();
				this.BindingSource.SetBindingMember(this.exportDEAUserControl, "FilteredInvoiceLines.DEAHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)));
				this.exportDEAUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportDEAUserControl.Name = "exportDEAUserControl";
				DEAPGATabPage.Controls.Add(exportDEAUserControl);
			}
		}
		internal ExportDEAUserControl exportDEAUserControl;

		void LoadFWSUserControls()
		{
			if (FWSPGATabPage.Controls.Count == 0 && FWSPGATabPage.TabVisible)
			{
				exportFWSUserControl = new ExportFWSUserControl();
				this.BindingSource.SetBindingMember(this.exportFWSUserControl, "FilteredInvoiceLines.ExportFWS");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).ExportFWS);
				this.exportFWSUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportFWSUserControl.Name = "exportFWSUserControl";
				FWSPGATabPage.Controls.Add(exportFWSUserControl);
			}
		}
		ExportFWSUserControl exportFWSUserControl;

		internal void LoadTTBUserControls()
		{
			if (TTBPGATabPage.Controls.Count == 0 && TTBPGATabPage.TabVisible)
			{
				exportTTBUserControl = new ExportTTBUserControl();
				this.BindingSource.SetBindingMember(this.exportTTBUserControl, "FilteredInvoiceLines.TTBLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)));
				this.exportTTBUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportTTBUserControl.Name = "exportTTBUserControl";
				TTBPGATabPage.Controls.Add(exportTTBUserControl);
			}
		}
		internal ExportTTBUserControl exportTTBUserControl;

		void SetAMSTabPageVisibility(ZBool isAMSDeclared, ZBool isEPADeclared)
		{
			if (exportAMSEPAUserControl != null)
			{
				exportAMSEPAUserControl.SetGroupBoxesVisibility(isAMSDeclared, isEPADeclared);
			}

			var amsTabPageVisible = isAMSDeclared || isEPADeclared;
			SetTabPageTabVisible(AMSPGATabPage, null, null, "", amsTabPageVisible);
			if (amsTabPageVisible)
			{
				AMSPGATabPage.Text = isAMSDeclared ? (isEPADeclared ? "AMS/EPA" : "AMS") : "EPA";
			}
		}

		void SetNMFSTabPageVisibility(ZBool isNMFSDeclared)
		{
			SetTabPageTabVisible(NMFSPGATabPage, exportNMFSUserControl, CurrentDataItem, "FilteredInvoiceLines.NMFSLines", isNMFSDeclared);
		}

		void SetATFTabPageVisibility(ZBool isATFDeclared)
		{
			SetTabPageTabVisible(ATFPGATabPage, exportATFUserControl, currentInvoiceLine?.ExportATF, "", isATFDeclared);
		}

		void SetDEATabPageVisibility(ZBool isDEADeclared)
		{
			SetTabPageTabVisible(DEAPGATabPage, exportDEAUserControl, CurrentDataItem, "FilteredInvoiceLines.DEAHeaders", isDEADeclared);
		}

		void SetFWSTabPageVisibility(ZBool isFWSDeclared)
		{
			SetTabPageTabVisible(FWSPGATabPage, exportFWSUserControl, currentInvoiceLine?.ExportFWS, "", isFWSDeclared);
		}

		void SetTTBTabPageVisibility(ZBool isTTBDeclared)
		{
			SetTabPageTabVisible(TTBPGATabPage, exportTTBUserControl, CurrentDataItem, "FilteredInvoiceLines.TTBLines", isTTBDeclared);
		}

		void SetTabPageTabVisible(Customs.GUI.BaseDeclarationTabPage tabPage, ZUserControl userControl, object dataSource, string dataMember, bool visible)
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
				if (visible && userControl.CurrentDataItem != dataSource)
				{
					userControl.SetDataBinding(dataSource, dataMember);
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

		#endregion
	}
}

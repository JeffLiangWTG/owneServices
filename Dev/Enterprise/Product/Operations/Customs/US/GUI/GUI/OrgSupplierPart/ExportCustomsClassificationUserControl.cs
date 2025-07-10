using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportCustomsClassificationUserControl : ZUserControl
	{
		public ExportCustomsClassificationUserControl(ZGrid relatedGrid)
		{
			this.relatedGrid = relatedGrid;
			InitializeComponent();
			ExportPGATabsVisibility();
			InitializeLazyCreate();
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.Visible = ZZCustomsFunctionality.IsAESJurisdictionNumberEffective;
		}

		readonly ZGrid relatedGrid;

		public CusClassPartPivot CurrentPivot
		{
			get { return fCurrentPartPivot; }
			set
			{
				var haschange = value != null && fCurrentPartPivot != value;
				if (haschange)
				{
					UnHookEvent();
					fCurrentPartPivot = value;
					AddHookEvent();
				}
				ExportPGATabsVisibility(haschange);
			}
		}
		CusClassPartPivot fCurrentPartPivot;

		void UnHookEvent()
		{
			if (CurrentPivot != null)
			{
				CurrentPivot.CD_AMSIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_ATFIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_DEAIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_PSTIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_FWSIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_NMFSHMSIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_TTBIndicatorInfo.ValueChanged -= new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.OnExportPGAIndicatorChangedEvent = null;
			}
		}

		void AddHookEvent()
		{
			if (CurrentPivot != null)
			{
				CurrentPivot.CD_AMSIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_ATFIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_DEAIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_PSTIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_FWSIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_NMFSHMSIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.CD_TTBIndicatorInfo.ValueChanged += new EventHandler(ExportPGAIndicator_ValueChanged);
				CurrentPivot.OnExportPGAIndicatorChangedEvent += delegate(ZString agencyCode, ZBool hasExportPGAData)
				{
					var result = true;

					if (hasExportPGAData)
					{
						result = Globals.Message.Show(Res.GetString("d80d941e-fcb5-4481-a283-097717a1f58e", "You have just cleared the AES {0} PGA indicator, system will remove all entered AES {0} data.\r\nDo you wish to proceed?", agencyCode), Res.GetString("ce4f5299-ad4a-4416-b004-421a2d6f7986", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
					}

					return result;
				};
			}
		}

		void ExportPGAIndicator_ValueChanged(object sender, EventArgs e)
		{
			ExportPGATabsVisibility();
		}

		void ExportPGATabsVisibility(bool isListChange = false)
		{
			var currentPivot = CurrentPivot;
			SetAMSTabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_AMSIndicator), currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_PSTIndicator));
			SetNMFSTabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_NMFSHMSIndicator), isListChange);
			SetATFTabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_ATFIndicator));
			SetDEATabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_DEAIndicator), isListChange);
			SetFWSTabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclared(currentPivot.CD_FWSIndicator));
			SetTTBTabPageVisibility(currentPivot != null && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(currentPivot.CD_TTBIndicator), isListChange);
		}

		void InitializeLazyCreate()
		{
			this.AMSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.NMFSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.ATFTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.DEATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.FWSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.TTBTabPage.LazyCreateControls += this.LazyCreateControlsFired;
		}

		void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == AMSTabPage)
			{
				LoadAMSAndEPAUserControls();
			}
			else if (sender == NMFSTabPage)
			{
				LoadNMFSUserControls();
			}
			else if (sender == ATFTabPage)
			{
				LoadATFUserControls();
			}
			else if (sender == DEATabPage)
			{
				LoadDEAUserControls();
			}
			else if (sender == FWSTabPage)
			{
				LoadFWSUserControls();
			}
			else if (sender == TTBTabPage)
			{
				LoadTTBUserControls();
			}
		}

		void LoadAMSAndEPAUserControls()
		{
			if (AMSTabPage.Controls.Count == 0 && AMSTabPage.TabVisible)
			{
				this.exportProductAMSEPAUserControl = new ExportProductAMSEPAUserControl();
				this.BindingSource.SetBindingMember(this.exportProductAMSEPAUserControl, ".");
				this.exportProductAMSEPAUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportProductAMSEPAUserControl.Name = "exportProductAMSEPAUserControl";
				AMSTabPage.Controls.Add(exportProductAMSEPAUserControl);
				SetAMSTabPageVisibility(OGAIndicatorList.IsToBeDeclared(CurrentPivot?.CD_AMSIndicator), OGAIndicatorList.IsToBeDeclared(CurrentPivot?.CD_PSTIndicator));
			}
		}
		ExportProductAMSEPAUserControl exportProductAMSEPAUserControl;

		internal void LoadNMFSUserControls()
		{
			if (NMFSTabPage.Controls.Count == 0 && NMFSTabPage.TabVisible)
			{
				exportNMFSUserControl = new ExportNMFSUserControl();
				this.BindingSource.SetBindingMember(this.exportNMFSUserControl, "NMFSLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(null)).NMFSLines);
				this.exportNMFSUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportNMFSUserControl.Name = "exportNMFSUserControl";
				this.exportNMFSUserControl.RemoveIrrevelantColumnsForProduct();
				NMFSTabPage.Controls.Add(exportNMFSUserControl);
			}
		}
		internal ExportNMFSUserControl exportNMFSUserControl;

		internal void LoadATFUserControls()
		{
			if (ATFTabPage.Controls.Count == 0 && ATFTabPage.TabVisible)
			{
				exportATFUserControl = new ExportATFUserControl();
				this.BindingSource.SetBindingMember(this.exportATFUserControl, "ExportATF");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(null)).ExportATF);
				this.exportATFUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportATFUserControl.Name = "exportATFUserControl";
				this.exportATFUserControl.HideIrrelevantControlsForProduct();
				ATFTabPage.Controls.Add(exportATFUserControl);
			}
		}
		internal ExportATFUserControl exportATFUserControl;

		internal void LoadDEAUserControls()
		{
			if (DEATabPage.Controls.Count == 0 && DEATabPage.TabVisible)
			{
				exportDEAUserControl = new ExportDEAUserControl();
				this.BindingSource.SetBindingMember(this.exportDEAUserControl, "DEAHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(null)).DEAHeaders);
				this.exportDEAUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportDEAUserControl.Name = "exportDEAUserControl";
				this.exportDEAUserControl.RemoveIrrevelantColumnsForProduct();
				DEATabPage.Controls.Add(exportDEAUserControl);
			}
		}
		internal ExportDEAUserControl exportDEAUserControl;

		void LoadFWSUserControls()
		{
			if (FWSTabPage.Controls.Count == 0 && FWSTabPage.TabVisible)
			{
				exportFWSUserControl = new ExportFWSUserControl();
				this.BindingSource.SetBindingMember(this.exportFWSUserControl, "ExportFWS");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(null)).ExportFWS);
				this.exportFWSUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportFWSUserControl.Name = "exportFWSUserControl";
				FWSTabPage.Controls.Add(exportFWSUserControl);
			}
		}
		ExportFWSUserControl exportFWSUserControl;

		internal void LoadTTBUserControls()
		{
			if (TTBTabPage.Controls.Count == 0 && TTBTabPage.TabVisible)
			{
				exportTTBUserControl = new ExportTTBUserControl();
				this.BindingSource.SetBindingMember(this.exportTTBUserControl, "TTBLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(null)).TTBLines);
				this.exportTTBUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.exportTTBUserControl.Name = "exportTTBUserControl";
				TTBTabPage.Controls.Add(exportTTBUserControl);
			}
		}
		internal ExportTTBUserControl exportTTBUserControl;

		void SetAMSTabPageVisibility(ZBool isAMSDeclared, ZBool isEPADeclared)
		{
			if (exportProductAMSEPAUserControl != null)
			{
				exportProductAMSEPAUserControl.SetGroupBoxesVisibility(isAMSDeclared, isEPADeclared);
			}

			var amsTabPageVisible = isAMSDeclared || isEPADeclared;
			SetTabPageTabVisible(AMSTabPage, null, null, "", amsTabPageVisible);
			if (amsTabPageVisible)
			{
				AMSTabPage.Text = isAMSDeclared ? (isEPADeclared ? "AMS/EPA" : "AMS") : "EPA";
			}
		}

		void SetNMFSTabPageVisibility(ZBool isNMFSDeclared, ZBool isListChange)
		{
			SetTabPageTabVisible(NMFSTabPage, exportNMFSUserControl, CurrentPivot, "NMFSLines", isNMFSDeclared, isListChange);
		}

		void SetATFTabPageVisibility(ZBool isATFDeclared)
		{
			SetTabPageTabVisible(ATFTabPage, exportATFUserControl, CurrentPivot?.ExportATF, "", isATFDeclared);
		}

		void SetDEATabPageVisibility(ZBool isDEADeclared, ZBool isListChange)
		{
			SetTabPageTabVisible(DEATabPage, exportDEAUserControl, CurrentPivot, "DEAHeaders", isDEADeclared, isListChange);
		}

		void SetFWSTabPageVisibility(ZBool isFWSDeclared)
		{
			SetTabPageTabVisible(FWSTabPage, exportFWSUserControl, CurrentPivot?.ExportFWS, "", isFWSDeclared);
		}

		void SetTTBTabPageVisibility(ZBool isTTBDeclared, ZBool isListChange)
		{
			SetTabPageTabVisible(TTBTabPage, exportTTBUserControl, CurrentPivot, "TTBLines", isTTBDeclared, isListChange);
		}

		void SetTabPageTabVisible(Customs.GUI.BaseDeclarationTabPage tabPage, ZUserControl userControl, object dataSource, string dataMember, bool visible, bool isListChange = false)
		{
			var oldValue = tabPage.TabVisible;
			if (visible)
			{
				tabPage.TabVisible = visible;
			}
			if (userControl != null)
			{
				if (oldValue != visible || (visible && isListChange))
				{
					userControl.Visible = visible;
					userControl.Enabled = visible;
				}
				if (visible)
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
				using (relatedGrid?.SuspendCancelOfNonEditedRowOnLeaving())
				{
					tabPage.TabVisible = visible;
				}
			}
		}
	}
}

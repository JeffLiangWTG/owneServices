using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class USACEImportInvoiceLineUserControl : USImportInvoiceLineUserControl
	{
		public USACEImportInvoiceLineUserControl()
		{
			InitializeComponent();

			this.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import) + JobApplicationCodeList.Codes.ACE;

			AddGridColumns();
			UpdateGridColumnCaption();

			FDAOtherTabPage.TabVisible = false;
			OGATabPage.TabVisible = false;
			SetODSTabPageTabVisible(false);
			SetDDTCTabPageTabVisible(false);
			SetVNETabPageTabVisible(false);
			FSISTabPage.TabVisible = false;
			SetPSTTabPageTabVisible(false, false);
			SetHFCTabPageTabVisible(false);
			SetNMFSTabPageTabVisible(false);
			SetACEFDATabPageTabVisible(false);
			SetAMSTabPageTabVisible(false, false);
			SetTTBTabPageTabVisible(false);
			SetAPHISTabPageTabVisible(false);
			SetFWSTabPageTabVisible(false);
			SetOMCTabPageTabVisible(false);
			SetNHTSATabPageTabVisible(false);
			SetCPSCTabPageTabVisible(false, false);
			SetDEATabPageTabVisible(false);
			SetSanctionsTabPageTabVisible(false);

			DOTLinesTopPanel.Visible = false;
			FCCLinesTopPanel.Visible = false;
			FDAGroupBox.Visible = false;
			FSISReqTopPanel.Visible = false;
			fsisUserControl.Dock = System.Windows.Forms.DockStyle.Fill;

			ReorderTabPages();
			InitializeLazyCreate();

			SupTariffGoodsValueCalcDropEdit.Visible = false;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				OGATabsVisibility();
				SanctionsTabVisible();
			}
		}

		void InitializeLazyCreate()
		{
			this.NMFSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.DDTCTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.TTBTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.APHISTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.FWSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.ACEFDATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.AMSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.ATFTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.ODSTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.VNETabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.PSTTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.HFCTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.OMCTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.OGAReqTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.NHTSATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.CPSCTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.DEATabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.SupAdditionalTariffsTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			this.SanctionsTabPage.LazyCreateControls += this.LazyCreateControlsFired;
		}

		void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == NMFSTabPage)
			{
				LoadNMFSTabPage();
			}
			else if (sender == DDTCTabPage)
			{
				LoadDDTCTabPage();
			}
			else if (sender == TTBTabPage)
			{
				LoadTTBTabPage();
			}
			else if (sender == APHISTabPage)
			{
				LoadAPHISTabPage();
			}
			else if (sender == FWSTabPage)
			{
				LoadFWSTabPage();
			}
			else if (sender == ACEFDATabPage)
			{
				LoadACEFDATabPage();
			}
			else if (sender == AMSTabPage)
			{
				LoadAMSTabPage();
			}
			else if (sender == ATFTabPage)
			{
				LoadATFTabPage();
			}
			else if (sender == ODSTabPage)
			{
				LoadODSTabPage();
			}
			else if (sender == VNETabPage)
			{
				LoadVNETabPage();
			}
			else if (sender == PSTTabPage)
			{
				LoadPSTTabPage();
			}
			else if (sender == OGAReqTabPage)
			{
				LoadOGAReqTabPage();
			}
			else if (sender == NHTSATabPage)
			{
				LoadNHTSATabPage();
			}
			else if (sender == CPSCTabPage)
			{
				LoadCPSCTabPage();
			}
			else if (sender == OMCTabPage)
			{
				LoadOMCTabPage();
			}
			else if (sender == DEATabPage)
			{
				LoadDEATabPage();
			}
			else if (sender == HFCTabPage)
			{
				LoadHFCTabPage();
			}
			else if (sender == SupAdditionalTariffsTabPage)
			{
				LoadSupAdditionalTariffsTabPage();
			}
			else if (sender == SanctionsTabPage)
			{
				LoadSanctionsTabPage();
			}
		}

		void LoadNHTSATabPage()
		{
			if (NHTSATabPage.Controls.Count == 0 && NHTSATabPage.TabVisible)
			{
				// 
				// nhtsaUserControl
				// 
				this.nhtsaUserControl = new NHTSAUserControl();
				this.BindingSource.SetBindingMember(this.nhtsaUserControl, "FilteredInvoiceLines.NHTSALines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).NHTSALines);
				this.nhtsaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.nhtsaUserControl.Name = "nhtsaUserControl";
				this.NHTSATabPage.Controls.Add(this.nhtsaUserControl);
			}
		}
		internal NHTSAUserControl nhtsaUserControl;

		void SetNHTSATabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(NHTSATabPage, nhtsaUserControl, "FilteredInvoiceLines.NHTSALines", visible);
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

		void LoadOGAReqTabPage()
		{
			if (OGAReqTabPage.Controls.Count == 0 && OGAReqTabPage.TabVisible)
			{
				// 
				// ogapgaRequirementsControl1
				// 
				this.ogapgaRequirementsControl = new OGAPGARequirementsControl();
				this.BindingSource.SetBindingMember(this.ogapgaRequirementsControl, "FilteredInvoiceLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)));
				this.ogapgaRequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.ogapgaRequirementsControl.Name = "ogapgaRequirementsControl";
				this.OGAReqTabPage.Controls.Add(this.ogapgaRequirementsControl);
			}
		}
		internal OGAPGARequirementsControl ogapgaRequirementsControl;

		void LoadPSTTabPage()
		{
			if (PSTTabPage.Controls.Count == 0 && PSTTabPage.TabVisible)
			{
				// 
				// pstUserControl
				// 
				this.pstUserControl = new PSTUserControl();
				this.BindingSource.SetBindingMember(this.pstUserControl, "FilteredInvoiceLines.PSTLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).PSTLines);
				this.pstUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.pstUserControl.Name = "pstUserControl";
				this.PSTTabPage.Controls.Add(this.pstUserControl);
				// 
				// pstDisclaimControl
				// 
				this.pstDisclaimControl = new PSTDisclaimControl();
				this.BindingSource.SetBindingMember(this.pstDisclaimControl, "FilteredInvoiceLines");
				this.pstDisclaimControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.pstDisclaimControl.Name = "pstDisclaimControl";
				this.pstDisclaimControl.Visible = false;
				this.PSTTabPage.Controls.Add(this.pstDisclaimControl);

				SetPSTTabPageTabVisible(IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.PST), IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.PST));
			}
		}
		internal PSTUserControl pstUserControl;
		internal PSTDisclaimControl pstDisclaimControl;

		void SetPSTTabPageTabVisible(bool isDeclaredOrHasPSTLines, bool isDisclaimed)
		{
			if (pstUserControl != null)
			{
				var oldValue = pstUserControl.Enabled;
				pstUserControl.Enabled = isDeclaredOrHasPSTLines;
				pstUserControl.Visible = isDeclaredOrHasPSTLines;
				if (oldValue != isDeclaredOrHasPSTLines)
				{
					if (isDeclaredOrHasPSTLines)
					{
						pstUserControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines.PSTLines");
					}
					else
					{
						pstUserControl.SetDataBinding(null, "");
					}
				}
			}

			if (pstDisclaimControl != null)
			{
				var oldValue = pstDisclaimControl.Enabled;
				pstDisclaimControl.Enabled = isDisclaimed;
				pstDisclaimControl.Visible = isDisclaimed;
				if (oldValue != isDisclaimed)
				{
					if (isDisclaimed)
					{
						pstDisclaimControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines");
					}
					else
					{
						pstDisclaimControl.SetDataBinding(null, "");
					}
				}
			}

			SetTabPageTabVisible(PSTTabPage, null, "", isDeclaredOrHasPSTLines || isDisclaimed);
		}

		void LoadHFCTabPage()
		{
			if (HFCTabPage.Controls.Count == 0 && HFCTabPage.TabVisible)
			{
				HFCUserControl = new HFCUserControl();
				this.BindingSource.SetBindingMember(this.HFCUserControl, "FilteredInvoiceLines.USHFCHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).USHFCHeaders);
				this.HFCUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.HFCUserControl.Name = "HFCUserControl";
				HFCTabPage.Controls.Add(HFCUserControl);
			}
		}
		internal HFCUserControl HFCUserControl;

		void SetHFCTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(HFCTabPage, HFCUserControl, "FilteredInvoiceLines.USHFCHeaders", visible);
		}

		void LoadVNETabPage()
		{
			if (VNETabPage.Controls.Count == 0 && VNETabPage.TabVisible)
			{
				// 
				// vneUserControl1
				// 
				this.vneUserControl = new VNEUserControl();
				this.BindingSource.SetBindingMember(this.vneUserControl, "FilteredInvoiceLines.VehicleLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).VehicleLines);
				this.vneUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.vneUserControl.Name = "vneUserControl";
				VNETabPage.Controls.Add(this.vneUserControl);
			}
		}
		internal VNEUserControl vneUserControl;

		void SetVNETabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(VNETabPage, vneUserControl, "FilteredInvoiceLines.VehicleLines", visible);
		}

		void LoadODSTabPage()
		{
			if (ODSTabPage.Controls.Count == 0 && ODSTabPage.TabVisible)
			{
				// 
				// odsAndTSCAControl
				// 
				this.odsAndTSCAControl = new ODSAndTSCAControl();
				this.BindingSource.SetBindingMember(this.odsAndTSCAControl, "FilteredInvoiceLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)));
				this.odsAndTSCAControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.odsAndTSCAControl.Name = "odsAndTSCAControl";
				ODSTabPage.Controls.Add(odsAndTSCAControl);
			}
		}
		internal ODSAndTSCAControl odsAndTSCAControl;

		void SetODSTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(ODSTabPage, null, "", visible);
		}

		void LoadATFTabPage()
		{
			if (ATFTabPage.Controls.Count == 0 && ATFTabPage.TabVisible)
			{
				// 
				// atfUserControl
				// 
				this.atfUserControl = new ATFUserControl();
				this.BindingSource.SetBindingMember(this.atfUserControl, "FilteredInvoiceLines.ATFLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).ATFLines);
				this.atfUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.atfUserControl.Name = "atfUserControl";
				ATFTabPage.Controls.Add(this.atfUserControl);
			}
		}
		internal ATFUserControl atfUserControl;

		void SetATFTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(ATFTabPage, atfUserControl, "FilteredInvoiceLines.ATFLines", visible);
		}

		void LoadAMSTabPage()
		{
			if (AMSTabPage.Controls.Count == 0 && AMSTabPage.TabVisible)
			{
				// 
				// amsUserControl
				// 
				this.amsUserControl = new AMSUserControl();
				this.BindingSource.SetBindingMember(this.amsUserControl, "FilteredInvoiceLines.AMSLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).AMSLines);
				this.amsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.amsUserControl.Name = "amsUserControl";
				// 
				// amsDisclaimControl
				// 
				this.amsDisclaimControl = new AMSDisclaimControl();
				this.BindingSource.SetBindingMember(this.amsDisclaimControl, "FilteredInvoiceLines");
				this.amsDisclaimControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.amsDisclaimControl.Name = "amsDisclaimControl";
				this.amsDisclaimControl.Visible = false;
				this.AMSTabPage.Controls.Add(this.amsUserControl);
				this.AMSTabPage.Controls.Add(this.amsDisclaimControl);

				SetAMSTabPageTabVisible(IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.AMS), IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.AMS));
			}
		}
		internal AMSUserControl amsUserControl;
		internal AMSDisclaimControl amsDisclaimControl;

		void SetAMSTabPageTabVisible(bool isDeclaredOrHasAMSLines, bool isDisclaimed)
		{
			if (amsUserControl != null)
			{
				var oldValue = amsUserControl.Enabled;
				amsUserControl.Enabled = isDeclaredOrHasAMSLines;
				amsUserControl.Visible = isDeclaredOrHasAMSLines;
				if (oldValue != isDeclaredOrHasAMSLines)
				{
					if (isDeclaredOrHasAMSLines)
					{
						amsUserControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines.AMSLines");
					}
					else
					{
						amsUserControl.SetDataBinding(null, "");
					}
				}
			}

			if (amsDisclaimControl != null)
			{
				var oldValue = amsUserControl.Enabled;
				amsDisclaimControl.Enabled = isDisclaimed;
				amsDisclaimControl.Visible = isDisclaimed;
				if (oldValue != isDisclaimed)
				{
					if (isDisclaimed)
					{
						amsDisclaimControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines");
					}
					else
					{
						amsDisclaimControl.SetDataBinding(null, "");
					}
				}
			}

			SetTabPageTabVisible(AMSTabPage, null, "", isDeclaredOrHasAMSLines || isDisclaimed);
		}

		void LoadACEFDATabPage()
		{
			if (ACEFDATabPage.Controls.Count == 0 && ACEFDATabPage.TabVisible)
			{
				// 
				// ACEFDAUserControl
				// 
				acefdaUserControl = new ACEFDAUserControl();
				this.BindingSource.SetBindingMember(this.acefdaUserControl, "FilteredInvoiceLines.ACE_FDALines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).ACE_FDALines);
				this.acefdaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.acefdaUserControl.Name = "acefdaUserControl";
				ACEFDATabPage.Controls.Add(acefdaUserControl);
			}
		}
		internal ACEFDAUserControl acefdaUserControl;

		void SetACEFDATabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(ACEFDATabPage, acefdaUserControl, "FilteredInvoiceLines.ACE_FDALines", visible);
		}

		void LoadAPHISTabPage()
		{
			if (APHISTabPage.Controls.Count == 0 && APHISTabPage.TabVisible)
			{
				aphisUserControl = new APHISUserControl();
				this.BindingSource.SetBindingMember(this.aphisUserControl, "FilteredInvoiceLines.APHISHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).APHISHeaders);
				this.aphisUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.aphisUserControl.Name = "aphisUserControl";
				APHISTabPage.Controls.Add(aphisUserControl);
			}
		}
		internal APHISUserControl aphisUserControl;

		void SetAPHISTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(APHISTabPage, aphisUserControl, "FilteredInvoiceLines.APHISHeaders", visible);
		}

		void LoadCPSCTabPage()
		{
			if (CPSCTabPage.Controls.Count == 0 && CPSCTabPage.TabVisible)
			{
				cpscUserControl = new CPSCUserControl();
				this.BindingSource.SetBindingMember(this.cpscUserControl, "FilteredInvoiceLines.CPSCHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).CPSCHeaders);
				this.cpscUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.cpscUserControl.Name = "cpscUserControl";
				CPSCTabPage.Controls.Add(cpscUserControl);
				// 
				// cpscDisclaimControl
				// 
				this.cpscDisclaimControl = new CPSCDisclaimPivotControl();
				this.BindingSource.SetBindingMember(this.cpscDisclaimControl, "FilteredInvoiceLines.CPSCHeaders");
				this.cpscDisclaimControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.cpscDisclaimControl.Name = "cpscDisclaimControl";
				this.cpscDisclaimControl.Visible = false;
				this.CPSCTabPage.Controls.Add(this.cpscDisclaimControl);

				SetCPSCTabPageTabVisible(IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.CPSC), IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.CPSC));
			}
		}
		internal CPSCUserControl cpscUserControl;
		internal CPSCDisclaimPivotControl cpscDisclaimControl;

		void SetCPSCTabPageTabVisible(bool isDeclared, bool isDisclaimedAndReasonIsA)
		{
			if (cpscUserControl != null)
			{
				var oldValue = cpscUserControl.Enabled;
				cpscUserControl.Enabled = isDeclared;
				cpscUserControl.Visible = isDeclared;
				if (oldValue != isDeclared)
				{
					if (isDeclared)
					{
						cpscUserControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines.CPSCHeaders");
					}
					else
					{
						cpscUserControl.SetDataBinding(null, "");
					}
				}
			}

			if (cpscDisclaimControl != null)
			{
				var oldValue = cpscDisclaimControl.Enabled;
				cpscDisclaimControl.Enabled = isDisclaimedAndReasonIsA;
				cpscDisclaimControl.Visible = isDisclaimedAndReasonIsA;
				if (oldValue != isDisclaimedAndReasonIsA)
				{
					if (isDisclaimedAndReasonIsA)
					{
						cpscDisclaimControl.SetDataBinding(CurrentDataItem, "FilteredInvoiceLines.CPSCHeaders");
					}
					else
					{
						cpscDisclaimControl.SetDataBinding(null, "");
					}
				}
			}

			SetTabPageTabVisible(CPSCTabPage, null, "", isDeclared || isDisclaimedAndReasonIsA);
		}

		void LoadFWSTabPage()
		{
			if (FWSTabPage.Controls.Count == 0 && FWSTabPage.TabVisible)
			{
				fwsUserControl = new FWSUserControl();
				this.BindingSource.SetBindingMember(this.fwsUserControl, "FilteredInvoiceLines.FWSHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).FWSHeaders);
				this.fwsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.fwsUserControl.Name = "fwsUserControl";
				FWSTabPage.Controls.Add(fwsUserControl);
			}
		}
		internal FWSUserControl fwsUserControl;

		void LoadOMCTabPage()
		{
			if (OMCTabPage.Controls.Count == 0 && OMCTabPage.TabVisible)
			{
				omcUserControl = new OMCUserControl();
				this.BindingSource.SetBindingMember(this.omcUserControl, "FilteredInvoiceLines.OMCHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).OMCHeaders);
				this.omcUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.omcUserControl.Name = "omcUserControl";
				OMCTabPage.Controls.Add(omcUserControl);
			}
		}
		internal OMCUserControl omcUserControl;

		void SetOMCTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(OMCTabPage, omcUserControl, "FilteredInvoiceLines.OMCHeaders", visible);
		}

		void SetFWSTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(FWSTabPage, fwsUserControl, "FilteredInvoiceLines.FWSHeaders", visible);
		}

		void LoadTTBTabPage()
		{
			if (TTBTabPage.Controls.Count == 0 && TTBTabPage.TabVisible)
			{
				ttbUserControl = new TTBUserControl();
				this.BindingSource.SetBindingMember(this.ttbUserControl, "FilteredInvoiceLines.TTBLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).TTBLines);
				this.ttbUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.ttbUserControl.Name = "ttbUserControl";
				TTBTabPage.Controls.Add(ttbUserControl);
			}
		}
		internal TTBUserControl ttbUserControl;

		void SetTTBTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(TTBTabPage, ttbUserControl, "FilteredInvoiceLines.TTBLines", visible);
		}

		void LoadNMFSTabPage()
		{
			if (NMFSTabPage.Controls.Count == 0 && NMFSTabPage.TabVisible)
			{
				nmfsUserControl = new NMFSUserControl();
				this.BindingSource.SetBindingMember(this.nmfsUserControl, "FilteredInvoiceLines.NMFSLines");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).NMFSLines);
				this.nmfsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.nmfsUserControl.Name = "nmfsUserControl";
				NMFSTabPage.Controls.Add(nmfsUserControl);
			}
		}
		internal NMFSUserControl nmfsUserControl;

		void SetNMFSTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(NMFSTabPage, nmfsUserControl, "FilteredInvoiceLines.NMFSLines", visible);
		}

		void LoadDDTCTabPage()
		{
			if (DDTCTabPage.Controls.Count == 0 && DDTCTabPage.TabVisible)
			{
				ddtcUserControl = new DDTCUserControl();
				this.BindingSource.SetBindingMember(this.ddtcUserControl, "FilteredInvoiceLines");
				ddtcUserControl.Dock = DockStyle.Fill;
				this.ddtcUserControl.Name = "ddtcUserControl";
				DDTCTabPage.Controls.Add(ddtcUserControl);
			}
		}
		internal DDTCUserControl ddtcUserControl;

		void SetDDTCTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(DDTCTabPage, null, "", visible);
		}

		void LoadDEATabPage()
		{
			if (DEATabPage.Controls.Count == 0 && DEATabPage.TabVisible)
			{
				DEAUserControl = new DEAUserControl();
				this.BindingSource.SetBindingMember(this.DEAUserControl, "FilteredInvoiceLines.DEAHeaders");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).DEAHeaders);
				this.DEAUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.DEAUserControl.Name = "DEAUserControl";
				DEATabPage.Controls.Add(DEAUserControl);
			}
		}
		internal DEAUserControl DEAUserControl;

		void SetDEATabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(DEATabPage, DEAUserControl, "FilteredInvoiceLines.DEAHeaders", visible);
		}

		void LoadSupAdditionalTariffsTabPage()
		{
			if (SupAdditionalTariffsTabPage.Controls.Count == 0 && SupAdditionalTariffsTabPage.TabVisible)
			{
				AdditionalTariffsUserControl = new AdditionalSupTariffsUserControl();
				this.BindingSource.SetBindingMember(this.AdditionalTariffsUserControl, "FilteredInvoiceLines");
				this.AdditionalTariffsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.AdditionalTariffsUserControl.Name = "AdditionalTariffsUserControl";
				SupAdditionalTariffsTabPage.Controls.Add(AdditionalTariffsUserControl);

				if (currentInvoiceLine != null && !currentInvoiceLine.IsDeleted)
				{
					AdditionalTariffsUserControl.OnSupTariffFormattedFieldTypeChanged(currentInvoiceLine.SupTariffFormattedFieldType == nameof(FieldType.TextDropEdit));
				}
			}
		}
		internal AdditionalSupTariffsUserControl AdditionalTariffsUserControl;

		void LoadSanctionsTabPage()
		{
			if (SanctionsTabPage.Controls.Count == 0 && SanctionsTabPage.TabVisible)
			{
				SanctionsUserControl = new SanctionsUserControl();
				this.BindingSource.SetBindingMember(this.SanctionsUserControl, "FilteredInvoiceLines");
				this.SanctionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.SanctionsUserControl.Name = "SanctionsUserControl";
				SanctionsTabPage.Controls.Add(SanctionsUserControl);
			}
		}
		internal SanctionsUserControl SanctionsUserControl;

		void TariffInfo_ValueChanged(object sender, EventArgs e)
		{
			SanctionsTabVisible();
		}

		void SanctionsTabVisible()
		{
			if (currentInvoiceLine != null && !currentInvoiceLine.IsDeleted)
			{
				var isVisible = ZZCustomsFunctionality.IsSanctionsEffective && (currentInvoiceLine.TariffMatchesFishingCondition || currentInvoiceLine.TariffMatchesMiningCondition);
				SetSanctionsTabPageTabVisible(isVisible);
				if (isVisible)
				{
					LineDetailTabControl.TabPages.Remove(CustomFieldsTabPage);
					LineDetailTabControl.TabPages.Add(CustomFieldsTabPage);

					if (SanctionsUserControl != null)
					{
						SanctionsUserControl.ChangeSanctionsGroupVisibility(currentInvoiceLine.TariffMatchesFishingCondition, currentInvoiceLine.TariffMatchesMiningCondition);
					}
				}
			}
		}

		void SetSanctionsTabPageTabVisible(bool visible)
		{
			SetTabPageTabVisible(SanctionsTabPage, SanctionsUserControl, "FilteredInvoiceLines", visible);
		}

		class Indicator
		{
			public string Caption { get; set; }
			public string ColumnName { get; set; }
			public string EditType { get; set; }
			public int Width { get; set; }
			public ResourceStringData GroupName { get; set; }
		}

		void AddGridColumns()
		{
			List<Indicator> list = new List<Indicator>();
			list.Add(new Indicator() { Caption = "EPA VNE", ColumnName = "US_VNEInd", Width = 60, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{B4EEBFDA-76F0-4C3A-BC8B-0C2BB83F961F}", "EPA VNE") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_VNEDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{B4EEBFDA-76F0-4C3A-BC8B-0C2BB83F961F}", "EPA VNE") });

			list.Add(new Indicator() { Caption = "NHTSA", ColumnName = "US_NHTSAIndicator", Width = 50, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{14AE17BF-1803-4A8A-848D-30635CCEFDC5}", "NHTSA") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_NHTDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{14AE17BF-1803-4A8A-848D-30635CCEFDC5}", "NHTSA") });

			list.Add(new Indicator() { Caption = "NMFS COA", ColumnName = JobComInvoiceLine.Schema.US_NMFSCOAInd, Width = 65, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{486E5302-080B-47A1-915C-D3598BE7998C}", "NMFS COA") });

			list.Add(new Indicator() { Caption = "NMFS 370", ColumnName = JobComInvoiceLine.Schema.US_NMFS370Ind, Width = 65, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{88F74B64-4E1E-4824-89C5-1442FE4908A0}", "NMFS 370") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_NMFS370DisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{88F74B64-4E1E-4824-89C5-1442FE4908A0}", "NMFS 370") });

			list.Add(new Indicator() { Caption = "NMFS AMR", ColumnName = JobComInvoiceLine.Schema.US_NMFSAMRInd, Width = 68, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7BA2D916-63C9-42AA-809F-663D987F0934}", "NMFS AMR") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_NMFSAMRDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7BA2D916-63C9-42AA-809F-663D987F0934}", "NMFS AMR") });

			list.Add(new Indicator() { Caption = "NMFS HMS", ColumnName = JobComInvoiceLine.Schema.US_NMFSHMSInd, Width = 68, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{81589BAE-27E7-404F-8F66-0886AC0548EB}", "NMFS HMS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_NMFSHMSDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{81589BAE-27E7-404F-8F66-0886AC0548EB}", "NMFS HMS") });

			list.Add(new Indicator() { Caption = "EPA ODS", ColumnName = "US_ODSInd", Width = 60, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{B0F22209-AECF-42BC-82CB-4468893142B1}", "EPA ODS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_ODSDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{B0F22209-AECF-42BC-82CB-4468893142B1}", "EPA ODS") });

			list.Add(new Indicator() { Caption = "OMC", ColumnName = "US_OMCInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{45455EEA-B80D-42DD-B0A3-F819350434FC}", "OMC") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_OMCDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{45455EEA-B80D-42DD-B0A3-F819350434FC}", "OMC") });

			list.Add(new Indicator() { Caption = "APHIS", ColumnName = "US_APHISInd", Width = 45, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{C878871D-53E6-4048-97C1-D7157363DAB1}", "APHIS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_APHISDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{C878871D-53E6-4048-97C1-D7157363DAB1}", "APHIS") });

			list.Add(new Indicator() { Caption = "TTB", ColumnName = "US_TTBInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7DE31DC1-4EE6-4B9E-9634-A7525E54EB5F}", "TTB") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_TTBDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7DE31DC1-4EE6-4B9E-9634-A7525E54EB5F}", "TTB") });

			list.Add(new Indicator() { Caption = "FSIS", ColumnName = "US_FSISInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{CF00C290-56D6-4A3A-AF14-E3AC004336B8}", "FSIS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_FSISDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{CF00C290-56D6-4A3A-AF14-E3AC004336B8}", "FSIS") });

			list.Add(new Indicator() { Caption = "EPA PST", ColumnName = "US_PSTIndicator", Width = 60, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{8A07F2D6-4C43-4E5D-B179-1E757661D956}", "EPA PST") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_PSTDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{8A07F2D6-4C43-4E5D-B179-1E757661D956}", "EPA PST") });
			list.Add(new Indicator() { Caption = "Disclaim Pragram", ColumnName = "US_PSTDisclaimProgram", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{8A07F2D6-4C43-4E5D-B179-1E757661D956}", "EPA PST") });

			list.Add(new Indicator() { Caption = "Lacey", ColumnName = "US_LaceyIndicator", Width = 40, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{35ECA144-33EC-401B-8CD5-BC0DB4291275}", "Lacey") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_LaceyDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{35ECA144-33EC-401B-8CD5-BC0DB4291275}", "Lacey") });

			list.Add(new Indicator() { Caption = "NMFS SIM", ColumnName = JobComInvoiceLine.Schema.US_NMFSSIMPInd, Width = 68 });
			list.Add(new Indicator() { Caption = "DDTC", ColumnName = "US_DDTCInd", Width = 51 });

			list.Add(new Indicator() { Caption = "EPA TSCA", ColumnName = "US_TSCAInd", Width = 68, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{98732C46-FFE6-4073-897C-1E66DABCB437}", "EPA TSCA") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_TSCADisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{98732C46-FFE6-4073-897C-1E66DABCB437}", "EPA TSCA") });

			list.Add(new Indicator() { Caption = "FDA", ColumnName = "US_FDAIndicator", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{A42C1D12-C760-4EE9-9DF1-B908F99A1806}", "FDA") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_FDADisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{A42C1D12-C760-4EE9-9DF1-B908F99A1806}", "FDA") });

			list.Add(new Indicator() { Caption = "FWS", ColumnName = "US_FWSInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{195D6A11-CF5B-466A-928E-3B84C9F77FA0}", "FWS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_FWSDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{195D6A11-CF5B-466A-928E-3B84C9F77FA0}", "FWS") });

			list.Add(new Indicator() { Caption = "AMS", ColumnName = "US_AMSInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{2B4BD712-1F85-458A-852E-00114AF0D01E}", "AMS") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_AMSDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{2B4BD712-1F85-458A-852E-00114AF0D01E}", "AMS") });

			list.Add(new Indicator() { Caption = "NOP", ColumnName = "US_NOPInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7DCB9F5E-CEDA-43FF-9A00-02DAB40FD342}", "NOP") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_NOPDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{7DCB9F5E-CEDA-43FF-9A00-02DAB40FD342}", "NOP") });

			list.Add(new Indicator() { Caption = "CPSC", ColumnName = "US_CPSCInd", Width = 40, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{CD75792F-900B-46CF-BA9A-5E5318521D34}", "CPSC") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_CPSCDisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{CD75792F-900B-46CF-BA9A-5E5318521D34}", "CPSC") });

			list.Add(new Indicator() { Caption = "Set Indicator", ColumnName = "US_SetInd", Width = 90 });

			list.Add(new Indicator() { Caption = "DEA", ColumnName = "US_DEAInd", Width = 35, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{34674EEB-E82D-4D2F-B9D5-711D1CC02CC1}", "DEA") });
			list.Add(new Indicator() { Caption = "Disclaim Reason", ColumnName = "US_DEADisclaimReason", Width = 90, GroupName = Enterprise.Customs.US.GUI.Res.GetData("{34674EEB-E82D-4D2F-B9D5-711D1CC02CC1}", "DEA") });

			list.Add(new Indicator() { Caption = "ATF", ColumnName = "US_ATFInd", Width = 35 });
			list.Add(new Indicator() { Caption = "FCC", ColumnName = "US_FCCIndicator", Width = 35 });

			List<string> columsList = new List<string>();
			foreach (var indicator in list)
			{
				if (columsList.Contains(indicator.ColumnName))
				{
					continue;
				}
				columsList.Add(indicator.ColumnName);
				var dropEditColumnStyleInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(indicator.ColumnName);
				if (dropEditColumnStyleInfo != null)
				{
					CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(dropEditColumnStyleInfo);
				}
				else
				{
					dropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
					dropEditColumnStyleInfo.Caption = indicator.Caption;
					dropEditColumnStyleInfo.ColumnName = indicator.ColumnName;
				}
				if (indicator.GroupName != null)
				{
					dropEditColumnStyleInfo.GroupName = indicator.GroupName;
				}
				dropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(indicator.Width);
				dropEditColumnStyleInfo.IsVisible = false;
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(dropEditColumnStyleInfo);
			}

			var privilegedStatusDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			var fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			var zOrganisationFindBoxColumnStyleInfoForSeller = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			var zAddressDropEditColumnStyleInfoForSeller = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			var zOrganisationFindBoxColumnStyleInfoForShipToParty = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			var zAddressDropEditColumnStyleInfoForShipToParty = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			var productExclusionDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			var exclusionNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			privilegedStatusDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("76b9e624-3371-41c5-b56e-35de164eb917", "Privileged Status Date");
			privilegedStatusDateEditColumnStyleInfo.ColumnName = "US_PrivilegedStatusDate";
			privilegedStatusDateEditColumnStyleInfo.IsVisible = false;
			privilegedStatusDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("622f024a-c5c9-4d29-b060-edaa5f0b5383", "FTZ Current Tariff");
			fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo.ColumnName = "FTZCurrentTariffFormatted";
			fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo.IsVisible = false;
			fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			zOrganisationFindBoxColumnStyleInfoForSeller.Caption = "Seller";
			zOrganisationFindBoxColumnStyleInfoForSeller.ColumnName = "SellerOrgPK";
			zOrganisationFindBoxColumnStyleInfoForSeller.GroupName = Enterprise.Customs.US.GUI.Res.GetData("a69a3105-8221-4e8e-8dfd-7d89d75f626a", "Seller");
			zOrganisationFindBoxColumnStyleInfoForSeller.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfoForSeller.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zAddressDropEditColumnStyleInfoForSeller.GroupName = Enterprise.Customs.US.GUI.Res.GetData("a69a3105-8221-4e8e-8dfd-7d89d75f626a", "Seller");
			zAddressDropEditColumnStyleInfoForSeller.Caption = "Seller Address";
			zAddressDropEditColumnStyleInfoForSeller.ColumnName = "JI_OA_Seller";
			zAddressDropEditColumnStyleInfoForSeller.IsVisible = false;
			zAddressDropEditColumnStyleInfoForSeller.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			zOrganisationFindBoxColumnStyleInfoForShipToParty.Caption = "Ship To Party";
			zOrganisationFindBoxColumnStyleInfoForShipToParty.ColumnName = "ShipToPartyOrgPK";
			zOrganisationFindBoxColumnStyleInfoForShipToParty.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8c2aaece-c02f-4175-beec-c1df12c3103b", "ShipToParty");
			zOrganisationFindBoxColumnStyleInfoForShipToParty.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfoForShipToParty.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfoForShipToParty.Caption = "ShipToParty Address";
			zAddressDropEditColumnStyleInfoForShipToParty.ColumnName = "JI_OA_ShipToPartyAddress";
			zAddressDropEditColumnStyleInfoForShipToParty.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8c2aaece-c02f-4175-beec-c1df12c3103b", "ShipToParty");
			zAddressDropEditColumnStyleInfoForShipToParty.IsVisible = false;
			zAddressDropEditColumnStyleInfoForShipToParty.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			productExclusionDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("91e4f4c6-e0ff-4217-8ca7-c5fccc86cc63", "Product Exclusion");
			productExclusionDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			productExclusionDropEditColumnStyleInfo.ColumnName = "US_ProductExclusion";
			productExclusionDropEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("85bef032-58bb-465e-bf42-f5e65fe8ea5a", "Product Exclusion");
			productExclusionDropEditColumnStyleInfo.IsVisible = false;
			productExclusionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			exclusionNumberTextBoxColumnStyleInfo.Caption = "Exclusion Number";
			exclusionNumberTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			exclusionNumberTextBoxColumnStyleInfo.ColumnName = "US_ExclusionNumber";
			exclusionNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			exclusionNumberTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("85bef032-58bb-465e-bf42-f5e65fe8ea5a", "Product Exclusion");
			exclusionNumberTextBoxColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(privilegedStatusDateEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(fTZCurrentTariffFormattedCodeFindBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfoForSeller);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfoForSeller);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfoForShipToParty);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfoForShipToParty);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(productExclusionDropEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(exclusionNumberTextBoxColumnStyleInfo);

			#region CMBA Related Columns

			var controlledGroupNameDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			controlledGroupNameDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			controlledGroupNameDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_ControlledGroupName;
			controlledGroupNameDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
			controlledGroupNameDropEditColumnStyleInfo.IsVisible = false;
			controlledGroupNameDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(controlledGroupNameDropEditColumnStyleInfo);

			var foreignProducerIdentifierMultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			foreignProducerIdentifierMultiControlColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_FPI;
			foreignProducerIdentifierMultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			foreignProducerIdentifierMultiControlColumnStyleInfo.FieldTypeColumnName = "FPIFieldType";
			foreignProducerIdentifierMultiControlColumnStyleInfo.IsVisible = false;
			foreignProducerIdentifierMultiControlColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(foreignProducerIdentifierMultiControlColumnStyleInfo);

			var allocationQuantityCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			allocationQuantityCalcEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_AllocationQuantity;
			allocationQuantityCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			allocationQuantityCalcEditColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(allocationQuantityCalcEditColumnStyleInfo);

			var flavorContentCreditIndCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			flavorContentCreditIndCheckBoxColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_FlavorContentCreditInd;
			flavorContentCreditIndCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(172);
			flavorContentCreditIndCheckBoxColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(flavorContentCreditIndCheckBoxColumnStyleInfo);

			var cbmaRateDesignationCodeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			cbmaRateDesignationCodeDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			cbmaRateDesignationCodeDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_TTBRateDesignationCode;
			cbmaRateDesignationCodeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			cbmaRateDesignationCodeDropEditColumnStyleInfo.IsVisible = false;
			cbmaRateDesignationCodeDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cbmaRateDesignationCodeDropEditColumnStyleInfo);

			var cbmaRateCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			cbmaRateCalcEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_CBMADefaultTaxRate;
			cbmaRateCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			cbmaRateCalcEditColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cbmaRateCalcEditColumnStyleInfo);

			#endregion

			#region License/Permit Columns

			var firstPermitLicenseTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			firstPermitLicenseTypeColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_FirstPermitLicenseType;
			firstPermitLicenseTypeColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b7681824-7d5e-412b-8def-2908cd1976df", "License/Permit Type");
			firstPermitLicenseTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			firstPermitLicenseTypeColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(firstPermitLicenseTypeColumnStyleInfo);

			var firstPermitLicenseNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			firstPermitLicenseNumberColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_FirstPermitLicenseNumber;
			firstPermitLicenseNumberColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f6008a59-15ad-45f3-b048-763e5dcc1be8", "License/Permit Number");
			firstPermitLicenseNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			firstPermitLicenseNumberColumnStyleInfo.IsVisible = false;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(firstPermitLicenseNumberColumnStyleInfo);

			#endregion

			#region Aluminum Smelt Columns

			var primaryCountryNAColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			primaryCountryNAColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_Prim_NA;
			primaryCountryNAColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8d581ed8-29f7-4261-bac1-6aab092fae68", "Smelt");
			primaryCountryNAColumnStyleInfo.IsVisible = false;
			primaryCountryNAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(primaryCountryNAColumnStyleInfo);

			var primaryCountryCodeColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			primaryCountryCodeColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_RN_NKPrimCtry;
			primaryCountryCodeColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8d581ed8-29f7-4261-bac1-6aab092fae68", "Smelt");
			primaryCountryCodeColumnStyleInfo.IsVisible = false;
			primaryCountryCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(primaryCountryCodeColumnStyleInfo);

			var secondaryCountryNAColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			secondaryCountryNAColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_Sec_NA;
			secondaryCountryNAColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8d581ed8-29f7-4261-bac1-6aab092fae68", "Smelt");
			secondaryCountryNAColumnStyleInfo.IsVisible = false;
			secondaryCountryNAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(secondaryCountryNAColumnStyleInfo);

			var secondaryCountryCodeColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			secondaryCountryCodeColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_RN_NKSecCtry;
			secondaryCountryCodeColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8d581ed8-29f7-4261-bac1-6aab092fae68", "Smelt");
			secondaryCountryCodeColumnStyleInfo.IsVisible = false;
			secondaryCountryCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(secondaryCountryCodeColumnStyleInfo);

			var castCountryCodeColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			castCountryCodeColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_RN_NKCastCtry;
			castCountryCodeColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("8d581ed8-29f7-4261-bac1-6aab092fae68", "Smelt");
			castCountryCodeColumnStyleInfo.IsVisible = false;
			castCountryCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(castCountryCodeColumnStyleInfo);

			#endregion

			#region Steel/Iron

			var certOfOrigColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			certOfOrigColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_RN_NKCertOrigin;
			certOfOrigColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("3DE704BF-7FEA-4B53-8421-E0F8244FCBFE", "Steel/Iron");
			certOfOrigColumnStyleInfo.IsVisible = false;
			certOfOrigColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(certOfOrigColumnStyleInfo);

			var meltedCtryColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			meltedCtryColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.US_RN_NKMeltCtry;
			meltedCtryColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("3DE704BF-7FEA-4B53-8421-E0F8244FCBFE", "Steel/Iron");
			meltedCtryColumnStyleInfo.IsVisible = false;
			meltedCtryColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(meltedCtryColumnStyleInfo);

			#endregion

			#region Additional Sup Tariffs

			var supFormattedAdditionalTariff1MultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			var supFormattedAdditionalTariff2MultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			var supFormattedAdditionalTariff3MultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			var supFormattedAdditionalTariff4MultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			var supFormattedAdditionalTariff5MultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			var overrideSupAdditionalTariff1DutyCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			var supAdditionalTariff1DutyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var overrideSupAdditionalTariff2DutyCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			var supAdditionalTariff2DutyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var overrideSupAdditionalTariff3DutyCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			var supAdditionalTariff3DutyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var overrideSupAdditionalTariff4DutyCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			var supAdditionalTariff4DutyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var overrideSupAdditionalTariff5DutyCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			var supAdditionalTariff5DutyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff1QtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff1UQTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			var supAdditionalTariff2QtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff2UQTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			var supAdditionalTariff3QtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff3UQTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			var supAdditionalTariff4QtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff4UQTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			var supAdditionalTariff5QtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var supAdditionalTariff5UQTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();

			supFormattedAdditionalTariff1MultiControlColumnStyleInfo.ColumnName = "SupFormattedAdditionalTariff1";
			supFormattedAdditionalTariff1MultiControlColumnStyleInfo.FieldTypeColumnName = "SupTariffFormattedFieldType";
			supFormattedAdditionalTariff1MultiControlColumnStyleInfo.IsVisible = false;
			supFormattedAdditionalTariff1MultiControlColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			supFormattedAdditionalTariff1MultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			overrideSupAdditionalTariff1DutyCheckBoxColumnStyleInfo.ColumnName = "US_OverrideSupAdditionalTariff1Duty";
			overrideSupAdditionalTariff1DutyCheckBoxColumnStyleInfo.IsVisible = false;
			overrideSupAdditionalTariff1DutyCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			supAdditionalTariff1DutyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff1DutyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff1Duty";
			supAdditionalTariff1DutyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff1DutyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			supFormattedAdditionalTariff2MultiControlColumnStyleInfo.ColumnName = "SupFormattedAdditionalTariff2";
			supFormattedAdditionalTariff2MultiControlColumnStyleInfo.FieldTypeColumnName = "SupTariffFormattedFieldType";
			supFormattedAdditionalTariff2MultiControlColumnStyleInfo.IsVisible = false;
			supFormattedAdditionalTariff2MultiControlColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			supFormattedAdditionalTariff2MultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			overrideSupAdditionalTariff2DutyCheckBoxColumnStyleInfo.ColumnName = "US_OverrideSupAdditionalTariff2Duty";
			overrideSupAdditionalTariff2DutyCheckBoxColumnStyleInfo.IsVisible = false;
			overrideSupAdditionalTariff2DutyCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			supAdditionalTariff2DutyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff2DutyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff2Duty";
			supAdditionalTariff2DutyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff2DutyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			supFormattedAdditionalTariff3MultiControlColumnStyleInfo.ColumnName = "SupFormattedAdditionalTariff3";
			supFormattedAdditionalTariff3MultiControlColumnStyleInfo.FieldTypeColumnName = "SupTariffFormattedFieldType";
			supFormattedAdditionalTariff3MultiControlColumnStyleInfo.IsVisible = false;
			supFormattedAdditionalTariff3MultiControlColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			supFormattedAdditionalTariff3MultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			overrideSupAdditionalTariff3DutyCheckBoxColumnStyleInfo.ColumnName = "US_OverrideSupAdditionalTariff3Duty";
			overrideSupAdditionalTariff3DutyCheckBoxColumnStyleInfo.IsVisible = false;
			overrideSupAdditionalTariff3DutyCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			supAdditionalTariff3DutyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff3DutyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff3Duty";
			supAdditionalTariff3DutyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff3DutyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			supFormattedAdditionalTariff4MultiControlColumnStyleInfo.ColumnName = "SupFormattedAdditionalTariff4";
			supFormattedAdditionalTariff4MultiControlColumnStyleInfo.FieldTypeColumnName = "SupTariffFormattedFieldType";
			supFormattedAdditionalTariff4MultiControlColumnStyleInfo.IsVisible = false;
			supFormattedAdditionalTariff4MultiControlColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			supFormattedAdditionalTariff4MultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			overrideSupAdditionalTariff4DutyCheckBoxColumnStyleInfo.ColumnName = "US_OverrideSupAdditionalTariff4Duty";
			overrideSupAdditionalTariff4DutyCheckBoxColumnStyleInfo.IsVisible = false;
			overrideSupAdditionalTariff4DutyCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			supAdditionalTariff4DutyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff4DutyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff4Duty";
			supAdditionalTariff4DutyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff4DutyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			supFormattedAdditionalTariff5MultiControlColumnStyleInfo.ColumnName = "SupFormattedAdditionalTariff5";
			supFormattedAdditionalTariff5MultiControlColumnStyleInfo.FieldTypeColumnName = "SupTariffFormattedFieldType";
			supFormattedAdditionalTariff5MultiControlColumnStyleInfo.IsVisible = false;
			supFormattedAdditionalTariff5MultiControlColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			supFormattedAdditionalTariff5MultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			overrideSupAdditionalTariff5DutyCheckBoxColumnStyleInfo.ColumnName = "US_OverrideSupAdditionalTariff5Duty";
			overrideSupAdditionalTariff5DutyCheckBoxColumnStyleInfo.IsVisible = false;
			overrideSupAdditionalTariff5DutyCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			supAdditionalTariff5DutyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff5DutyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff5Duty";
			supAdditionalTariff5DutyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff5DutyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff1QtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff1QtyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff1Qty";
			supAdditionalTariff1QtyCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|63fcfa86-1717-4fb5-ac6c-d00a74e6a392", "Prov Add. Qty 1");
			supAdditionalTariff1QtyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff1QtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff1UQTextBoxColumnStyleInfo.ColumnName = "US_SupAdditionalTariff1UQ";
			supAdditionalTariff1UQTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|63fcfa86-1717-4fb5-ac6c-d00a74e6a392", "Prov Add. Qty 1");
			supAdditionalTariff1UQTextBoxColumnStyleInfo.IsVisible = false;
			supAdditionalTariff1UQTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			supAdditionalTariff2QtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff2QtyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff2Qty";
			supAdditionalTariff2QtyCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|0af0cd59-c7ea-4002-a4e9-db07de3fc0ab", "Prov Add. Qty 2");
			supAdditionalTariff2QtyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff2QtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff2UQTextBoxColumnStyleInfo.ColumnName = "US_SupAdditionalTariff2UQ";
			supAdditionalTariff2UQTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|0af0cd59-c7ea-4002-a4e9-db07de3fc0ab", "Prov Add. Qty 2");
			supAdditionalTariff2UQTextBoxColumnStyleInfo.IsVisible = false;
			supAdditionalTariff2UQTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			supAdditionalTariff3QtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff3QtyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff3Qty";
			supAdditionalTariff3QtyCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|2095aa94-33dc-40c2-9a4a-04a0805cd2e6", "Prov Add. Qty 3");
			supAdditionalTariff3QtyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff3QtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff3UQTextBoxColumnStyleInfo.ColumnName = "US_SupAdditionalTariff3UQ";
			supAdditionalTariff3UQTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|2095aa94-33dc-40c2-9a4a-04a0805cd2e6", "Prov Add. Qty 3");
			supAdditionalTariff3UQTextBoxColumnStyleInfo.IsVisible = false;
			supAdditionalTariff3UQTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			supAdditionalTariff4QtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff4QtyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff4Qty";
			supAdditionalTariff4QtyCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|4c9c5bdb-4919-45a2-900f-f0f22e591ef9", "Prov Add. Qty 4");
			supAdditionalTariff4QtyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff4QtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff4UQTextBoxColumnStyleInfo.ColumnName = "US_SupAdditionalTariff4UQ";
			supAdditionalTariff4UQTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|4c9c5bdb-4919-45a2-900f-f0f22e591ef9", "Prov Add. Qty 4");
			supAdditionalTariff4UQTextBoxColumnStyleInfo.IsVisible = false;
			supAdditionalTariff4UQTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			supAdditionalTariff5QtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			supAdditionalTariff5QtyCalcEditColumnStyleInfo.ColumnName = "US_SupAdditionalTariff5Qty";
			supAdditionalTariff5QtyCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|370269d1-5aa5-4fe3-b889-9247fe868348", "Prov Add. Qty 5");
			supAdditionalTariff5QtyCalcEditColumnStyleInfo.IsVisible = false;
			supAdditionalTariff5QtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			supAdditionalTariff5UQTextBoxColumnStyleInfo.ColumnName = "US_SupAdditionalTariff5UQ";
			supAdditionalTariff5UQTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|370269d1-5aa5-4fe3-b889-9247fe868348", "Prov Add. Qty 5");
			supAdditionalTariff5UQTextBoxColumnStyleInfo.IsVisible = false;
			supAdditionalTariff5UQTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supFormattedAdditionalTariff1MultiControlColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(overrideSupAdditionalTariff1DutyCheckBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff1DutyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supFormattedAdditionalTariff2MultiControlColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(overrideSupAdditionalTariff2DutyCheckBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff2DutyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supFormattedAdditionalTariff3MultiControlColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(overrideSupAdditionalTariff3DutyCheckBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff3DutyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supFormattedAdditionalTariff4MultiControlColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(overrideSupAdditionalTariff4DutyCheckBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff4DutyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supFormattedAdditionalTariff5MultiControlColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(overrideSupAdditionalTariff5DutyCheckBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff5DutyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff1QtyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff1UQTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff2QtyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff2UQTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff3QtyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff3UQTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff4QtyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff4UQTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff5QtyCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(supAdditionalTariff5UQTextBoxColumnStyleInfo);

			#endregion
		}

		void UpdateGridColumnCaption()
		{
			var columnStyleInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(USAddInfoSchema.Constants.US_SecondarySPI);
			if (columnStyleInfo != null)
			{
				columnStyleInfo.Caption = "Product Claim";
			}
		}

		void OGAIndicator_ValueChanged(object sender, EventArgs e)
		{
			OGATabsVisibility();
		}

		void DisclaimProgram_ValueChanged(object sender, EventArgs e)
		{
			SetAMSTabPageTabVisible(IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.AMS), IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.AMS));
		}

		void CPSCDisclaimReason_ValueChanged(object sender, EventArgs e)
		{
			SetCPSCTabPageTabVisible(IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.CPSC), IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.CPSC));
		}

		void OGATabsVisibility()
		{
			if (currentInvoiceLine != null && !currentInvoiceLine.IsDeleted)
			{
				bool isACECargoCertificationMode = currentInvoiceLine.IsACECargoCertificationMode;
				var declaration = currentInvoiceLine.Declaration;
				if (declaration != null)
				{
					SetTabPageTabVisible(OGAReqTabPage, null, "", declaration.IsACE);
				}
				var fdaOtherTabPageVisible = declaration != null && !declaration.CanHavePGAFDA && (currentInvoiceLine.IsFDADeclared || currentInvoiceLine.HasFDAData);
				SetTabPageTabVisible(FDAOtherTabPage, null, "", fdaOtherTabPageVisible);
				SetACEFDATabPageTabVisible(declaration != null && declaration.CanHavePGAFDA && (currentInvoiceLine.IsFDADeclared || currentInvoiceLine.HasACE_FDALines));
				var ogaTabPageVisible = currentInvoiceLine.IsFCCDeclared || currentInvoiceLine.HasFCCData || !isACECargoCertificationMode && (currentInvoiceLine.IsDOTDeclared || currentInvoiceLine.HasDOTData);
				SetTabPageTabVisible(OGATabPage, null, "", ogaTabPageVisible);
				if (declaration != null)
				{
					OGATabPage.Text = declaration.IsNHTSARelevant ? "FCC" : "FCC/DOT";
					DOTGroupBox.Visible = !declaration.IsNHTSARelevant;
				}

				SetVNETabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_VNEInd) || currentInvoiceLine.HasVNEDetails));
				SetODSTabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_ODSInd) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_TSCAInd) || !currentInvoiceLine.US_ODSTrackingStatus.IsEmpty || !currentInvoiceLine.US_TSCATrackingStatus.IsEmpty));
				var fsisTabPageVisible = OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_FSISInd) || currentInvoiceLine.HasFSISDetails; //FSIS is on ACS jobs as well.
				SetTabPageTabVisible(FSISTabPage, null, "", fsisTabPageVisible);

				SetPSTTabPageTabVisible(isACECargoCertificationMode && IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.PST), isACECargoCertificationMode && IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.PST));

				var laceyActTabVisible = OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_LaceyIndicator) || currentInvoiceLine.HasLaceyActData;
				SetTabPageTabVisible(PGATabPage, null, "", laceyActTabVisible);

				LaceyPanel.Visible = !isACECargoCertificationMode;
				PGAContainersGroupBox.Visible = !isACECargoCertificationMode;
				ACELaceyActUserControl.Visible = isACECargoCertificationMode;

				SetNMFSTabPageTabVisible(isACECargoCertificationMode && (currentInvoiceLine.HasNMFSLines || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFS370Ind) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFSAMRInd) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFSHMSInd) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFSSIMPInd) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NMFSCOAInd)));
				SetDDTCTabPageTabVisible(isACECargoCertificationMode && (currentInvoiceLine.ShouldDeclareACEDDTCData || !currentInvoiceLine.US_DDTCTrackingStatus.IsEmpty));
				SetTTBTabPageTabVisible(isACECargoCertificationMode && (currentInvoiceLine.HasTTBLines || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_TTBInd)));
				SetAPHISTabPageTabVisible(currentInvoiceLine.HasAPHISHeaders || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_APHISInd));
				SetFWSTabPageTabVisible(currentInvoiceLine.HasFWSHeaders || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_FWSInd));

				SetAMSTabPageTabVisible(isACECargoCertificationMode && IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.AMS), isACECargoCertificationMode && IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.AMS));
				SetNHTSATabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NHTSAIndicator) || currentInvoiceLine.HasNHTSADetails));
				SetOMCTabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_OMCInd) || currentInvoiceLine.HasOMCHeaders));
				SetATFTabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_ATFInd) || currentInvoiceLine.HasATFDetails));
				SetCPSCTabPageTabVisible(isACECargoCertificationMode && IsDeclaredOrHasData(GovernmentAgencyProgramCodeList.Codes.CPSC), isACECargoCertificationMode && IsDisclaimed(GovernmentAgencyProgramCodeList.Codes.CPSC));
				SetDEATabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_DEAInd) || currentInvoiceLine.HasDEAHeaders));
				SetHFCTabPageTabVisible(isACECargoCertificationMode && (OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_HFCInd) || currentInvoiceLine.HasUSHFCHeaders));
			}
			else
			{
				SetTabPageTabVisible(FDAOtherTabPage, null, "", false);
				SetACEFDATabPageTabVisible(false);
				SetTabPageTabVisible(OGATabPage, null, "", false);
				DOTGroupBox.Visible = false;
				SetVNETabPageTabVisible(false);
				SetODSTabPageTabVisible(false);
				SetTabPageTabVisible(FSISTabPage, null, "", false);
				SetPSTTabPageTabVisible(false, false);
				SetTabPageTabVisible(PGATabPage, null, "", false);
				LaceyPanel.Visible = false;
				PGAContainersGroupBox.Visible = false;
				ACELaceyActUserControl.Visible = false;
				SetNMFSTabPageTabVisible(false);
				SetDDTCTabPageTabVisible(false);
				SetTTBTabPageTabVisible(false);
				SetAPHISTabPageTabVisible(false);
				SetFWSTabPageTabVisible(false);
				SetAMSTabPageTabVisible(false, false);
				SetNHTSATabPageTabVisible(false);
				SetATFTabPageTabVisible(false);
				SetCPSCTabPageTabVisible(false, false);
				SetHFCTabPageTabVisible(false);
			}
		}

		protected override void HookInvoiceLineEvents(JobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.US_FDAIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FCCIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DOTIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_VNEIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_ODSIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FSISIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_PSTIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_LaceyIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFS370IndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSCOAIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSAMRIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSHMSIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSSIMPIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DDTCIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);

				invoiceLine.US_AMSIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NOPIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_TSCAIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_TTBIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_APHISIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FWSIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NHTSAIndicatorInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_ATFIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_CPSCIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_OMCIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DEAIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_HFCIndInfo.ValueChanged += new EventHandler(OGAIndicator_ValueChanged);

				invoiceLine.US_AMSDisclaimProgramInfo.ValueChanged += new EventHandler(DisclaimProgram_ValueChanged);
				invoiceLine.US_CPSCDisclaimReasonInfo.ValueChanged += new EventHandler(CPSCDisclaimReason_ValueChanged);

				invoiceLine.JI_TariffInfo.ValueChanged += new EventHandler(TariffInfo_ValueChanged);
				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged += new EventHandler(TariffInfo_ValueChanged);
			}

			OGATabsVisibility();
			SanctionsTabVisible();
		}

		protected override void UnHookInvoiceLineEvents(JobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.US_FDAIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FCCIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DOTIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_VNEIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_ODSIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FSISIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_PSTIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_LaceyIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFS370IndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSCOAIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSAMRIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSHMSIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NMFSSIMPIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DDTCIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_TSCAIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_AMSIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NOPIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_TTBIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_APHISIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_FWSIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_NHTSAIndicatorInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_ATFIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_CPSCIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_OMCIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_DEAIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);
				invoiceLine.US_HFCIndInfo.ValueChanged -= new EventHandler(OGAIndicator_ValueChanged);

				invoiceLine.US_AMSDisclaimProgramInfo.ValueChanged -= new EventHandler(DisclaimProgram_ValueChanged);
				invoiceLine.US_CPSCDisclaimReasonInfo.ValueChanged -= new EventHandler(CPSCDisclaimReason_ValueChanged);

				invoiceLine.JI_TariffInfo.ValueChanged -= new EventHandler(TariffInfo_ValueChanged);
				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged -= new EventHandler(TariffInfo_ValueChanged);
			}
		}

		void ReorderTabPages()
		{
			LineDetailTabControl.TabPages.Remove(LineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(LineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(SupAdditionalTariffsTabPage);
			LineDetailTabControl.TabPages.Insert(SupAdditionalTariffsTabPage, 1);

			LineDetailTabControl.TabPages.Remove(ContainersTabPage);
			LineDetailTabControl.TabPages.Insert(ContainersTabPage, 2);

			LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 3);

			LineDetailTabControl.TabPages.Remove(FeesTabPage);
			LineDetailTabControl.TabPages.Insert(FeesTabPage, 4);

			LineDetailTabControl.TabPages.Remove(LicencePermitsDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(LicencePermitsDetailsTabPage, 5);

			LineDetailTabControl.TabPages.Remove(LineGroupingTabPage);
			LineDetailTabControl.TabPages.Insert(LineGroupingTabPage, 6);

			LineDetailTabControl.TabPages.Remove(ElectronicInvoiceTabPage);
			LineDetailTabControl.TabPages.Insert(ElectronicInvoiceTabPage, 7);

			LineDetailTabControl.TabPages.Remove(CensusWarningTabPage);
			LineDetailTabControl.TabPages.Insert(CensusWarningTabPage, 8);

			LineDetailTabControl.TabPages.Remove(OtherTabPage);
			LineDetailTabControl.TabPages.Insert(OtherTabPage, 9);

			LineDetailTabControl.TabPages.Remove(OGAReqTabPage);
			LineDetailTabControl.TabPages.Insert(OGAReqTabPage, 10);

			LineDetailTabControl.TabPages.Remove(AMSTabPage);
			LineDetailTabControl.TabPages.Insert(AMSTabPage, 11);

			LineDetailTabControl.TabPages.Remove(CPSCTabPage);
			LineDetailTabControl.TabPages.Insert(CPSCTabPage, 12);

			LineDetailTabControl.TabPages.Remove(OMCTabPage);
			LineDetailTabControl.TabPages.Insert(OMCTabPage, 13);

			LineDetailTabControl.TabPages.Remove(DEATabPage);
			LineDetailTabControl.TabPages.Insert(DEATabPage, 14);

			LineDetailTabControl.TabPages.Remove(SanctionsTabPage);
			LineDetailTabControl.TabPages.Insert(SanctionsTabPage, 15);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			ADDQtyCalcDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CVDQtyCalcDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			ADDDepositValueCalcFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CVDDepositValueCalcFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			FlavorContentCreditIndCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			ControlledGroupNameDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ForeignProducerIdentifierDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AllocationQuantityCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			ForeignProducerIdentifierTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CBMARateDesignationCodeDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CBMARateCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				ADDQtyCalcDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.ADDQtyVisible", false, DataSourceUpdateMode.Never));
				CVDQtyCalcDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.CVDQtyVisible", false, DataSourceUpdateMode.Never));

				ADDDepositValueCalcFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.ADDValueVisible", false, DataSourceUpdateMode.Never));
				CVDDepositValueCalcFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.CVDValueVisible", false, DataSourceUpdateMode.Never));

				FlavorContentCreditIndCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsCBMAProductClaim", false, DataSourceUpdateMode.Never));

				ControlledGroupNameDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsCBMAProductClaimAndIsNotCBMA23Effective", false, DataSourceUpdateMode.Never));
				ForeignProducerIdentifierDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.FPIDropEditIsVisiable", false, DataSourceUpdateMode.Never));
				AllocationQuantityCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsCBMAProductClaimAndIsNotCBMA23Effective", false, DataSourceUpdateMode.Never));

				ForeignProducerIdentifierTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.FPITextBoxIsVisiable", false, DataSourceUpdateMode.Never));
				CBMARateDesignationCodeDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsCBMAProductClaimAndIsCBMA23Effective", false, DataSourceUpdateMode.Never));
				CBMARateCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsCBMAProductClaimAndIsCBMA23Effective", false, DataSourceUpdateMode.Never));
			}
		}

		protected override void ShowOrHideOtherConsumptionFTZControlsCore(ZBool isVisibleForConsumptionFTZ, ZBool isACECargoCertificationMode)
		{
			base.ShowOrHideOtherConsumptionFTZControlsCore(isVisibleForConsumptionFTZ, isACECargoCertificationMode);

			if (isVisibleForConsumptionFTZ)
			{
				CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.US_PrivilegedStatusDate);

				if (isACECargoCertificationMode)
				{
					CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.FTZCurrentTariffFormatted);
				}
				else
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.FTZCurrentTariffFormatted);
				}

				UpdateColumnCaption(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ManifestQty, FTZPackQtyCaption);
				UpdateColumnGroupName(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ManifestUQ, FTZPackQtyCaption);
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.US_PrivilegedStatusDate);
				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.FTZCurrentTariffFormatted);

				UpdateColumnCaption(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ManifestQty, InnermostPackQtyCaption);
				UpdateColumnGroupName(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ManifestUQ, InnermostPackQtyCaption);
			}

			MostInnerPackQtyCalcDropEdit.Visible = isVisibleForConsumptionFTZ;
			JI_RH_NKCommodity_CodeBoundFindBox.Visible = !isVisibleForConsumptionFTZ;
		}

		ResourceStringData InnermostPackQtyCaption
		{
			get { return innermostPackQtyCaption ?? (innermostPackQtyCaption = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|f4439e2c-ebac-4ed9-bddc-dcf0da52b4e7", "Innermost Pack Qty")); }
		}
		ResourceStringData innermostPackQtyCaption;

		ResourceStringData FTZPackQtyCaption
		{
			get { return ftzPackQtyCaption ?? (ftzPackQtyCaption = Enterprise.Customs.US.GUI.Res.GetData("USACEImportInvoiceLineUserControl|B3633192-ED25-4956-BA12-BC8665572B09", "FTZ Pack Qty")); }
		}
		ResourceStringData ftzPackQtyCaption;

		bool IsDeclaredOrHasData(ZString agencyCode)
		{
			var result = false;
			if (currentInvoiceLine != null)
			{
				switch (agencyCode)
				{
					case GovernmentAgencyProgramCodeList.Codes.AMS:
						result = OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_AMSInd) || OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_NOPInd) || currentInvoiceLine.HasAMSDetails;
						break;
					case GovernmentAgencyProgramCodeList.Codes.PST:
						result = OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_PSTIndicator) || currentInvoiceLine.HasPSTLines;
						break;
					case GovernmentAgencyProgramCodeList.Codes.CPSC:
						result = OGAIndicatorList.IsToBeDeclared(currentInvoiceLine.US_CPSCInd) || (currentInvoiceLine.US_CPSCDisclaimReason != PGADisclaimReasonList.Codes.A && currentInvoiceLine.HasCPSCHeaders);
						break;
					default:
						break;
				}
			}
			return result;
		}

		bool IsDisclaimed(ZString agencyCode)
		{
			var result = false;
			if (currentInvoiceLine != null)
			{
				switch (agencyCode)
				{
					case GovernmentAgencyProgramCodeList.Codes.AMS:
						var isDisclaimed = OGAIndicatorList.IsToBeDisclaimed(currentInvoiceLine.US_AMSInd);
						var isEG1 = currentInvoiceLine.US_AMSDisclaimProgram == AMSProgramList.Codes.EG1;
						result = isDisclaimed && !isEG1;
						break;
					case GovernmentAgencyProgramCodeList.Codes.PST:
						result = OGAIndicatorList.IsToBeDisclaimed(currentInvoiceLine.US_PSTIndicator);
						break;
					case GovernmentAgencyProgramCodeList.Codes.CPSC:
						result = OGAIndicatorList.IsToBeDisclaimed(currentInvoiceLine.US_CPSCInd) && currentInvoiceLine.US_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A;
						break;
					default:
						break;
				}
			}
			return result;
		}

		protected override void OnSupTariffFormattedFieldTypeChanged(bool shouldUseDropEdit)
		{
			base.OnSupTariffFormattedFieldTypeChanged(shouldUseDropEdit);

			if (AdditionalTariffsUserControl != null)
			{
				AdditionalTariffsUserControl.OnSupTariffFormattedFieldTypeChanged(shouldUseDropEdit);
			}
		}
	}
}

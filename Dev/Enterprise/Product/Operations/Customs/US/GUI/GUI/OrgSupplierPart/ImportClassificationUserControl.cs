using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportClassificationUserControl : ZUserControl
	{
		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			cPSCControl.LotsGridVisible = false;
		}

		public ImportClassificationUserControl(ZGrid relatedGrid)
		{
			this.relatedGrid = relatedGrid;
			InitializeComponent();
			this.attributesLeftSplitContainer.Panel2MinSize = 300;
			this.attributesRightSplitContainer.Panel2MinSize = 150;

			this.acefdaUserControl.SetContainerControlsHideAndRemoveTransactionalColumns();
			this.laceyUserControl.RemoveColumns();
			this.nhtsaUserControl.RemoveUnavailableComlumnsForProduct();
			this.atfUserControl.RemoveColumns();
			this.ttbUserControl.RemoveUnavailableComlumnsForProduct();
			this.nmfsUserControl.RemoveUnavailableControlsForProduct();

			this.cPSCControl.SetControlsHideAndRemoveTransactionalColumns();
			this.oMCControl.SetControlsHideAndRemoveTransactionalColumns();
			this.DEAControl.RemoveUnavailableComlumnsForProduct();
			this.aphisUserControl.RemoveUnavailableComlumnsForProduct();
			this.amsUserControl.RemoveUnavailableComlumnsForProduct();
			this.fwsUserControl.RemoveColumns();
			this.hfcUserControl.RemoveColumns();

			SetLaceyActTabPageVisible(false);

			SetPGAFDATabPageVisible(false);
			SetNHTSATabPageVisible(false);
			SetODSTSCATabPageVisible(false);
			SetPSTTabPageVisible(false, false);
			SetHFCTabPageVisible(false);
			SetVNETabPageVisible(false);
			pstUserControl.SetPropertyForProduct();
			vneUserControl.SetPropertyForProduct();
			odsAndTSCAControl.SetPropertyForProduct();
			SetOMCTabPageVisible(false);
			SetATFTabPageVisible(false);
			SetAMSTabPageVisible(false, false);
			SetFWSTabPageVisible(false);
			SetNMFSTabPageVisible(false);
			SetTTBTabPageVisible(false);
			SetCPSCTabPageVisible(false, false);
			SetDEATabPageVisible(false);
			SetAPHISTabPageVisible(false);
			SetDDTCTabPageVisible(false);

			ReorderTabPages();
		}

		void ReorderTabPages()
		{
			ImportTabControl.TabPages.Remove(detailsTabPage);
			ImportTabControl.TabPages.Insert(detailsTabPage, 0);

			ImportTabControl.TabPages.Remove(additionalTariffsTabPage);
			ImportTabControl.TabPages.Insert(additionalTariffsTabPage, 1);

			ImportTabControl.TabPages.Remove(LicenceNoTabPage);
			ImportTabControl.TabPages.Insert(LicenceNoTabPage, 2);

			ImportTabControl.TabPages.Remove(otherTabPage);
			ImportTabControl.TabPages.Insert(otherTabPage, 3);

			ImportTabControl.TabPages.Remove(cWTabPage);
			ImportTabControl.TabPages.Insert(cWTabPage, 4);

			ImportTabControl.TabPages.Remove(AttributesTabPage);
			ImportTabControl.TabPages.Insert(AttributesTabPage, 5);

			ImportTabControl.TabPages.Remove(oGAPGATabPage);
			ImportTabControl.TabPages.Insert(oGAPGATabPage, 6);

			ImportTabControl.TabPages.Remove(pGAFDATabPage);
			ImportTabControl.TabPages.Insert(pGAFDATabPage, 7);
			SetPGAFDATabPageVisible(false);

			ImportTabControl.TabPages.Remove(laceyTabPage);
			ImportTabControl.TabPages.Insert(laceyTabPage, 8);
			SetLaceyActTabPageVisible(false);

			ImportTabControl.TabPages.Remove(nHTSATabPage);
			ImportTabControl.TabPages.Insert(nHTSATabPage, 9);
			SetNHTSATabPageVisible(false);

			ImportTabControl.TabPages.Remove(oDSTSCATabPage);
			ImportTabControl.TabPages.Insert(oDSTSCATabPage, 10);
			SetODSTSCATabPageVisible(false);

			ImportTabControl.TabPages.Remove(vNETabPage);
			ImportTabControl.TabPages.Insert(vNETabPage, 11);
			SetVNETabPageVisible(false);

			ImportTabControl.TabPages.Remove(pSTTabPage);
			ImportTabControl.TabPages.Insert(pSTTabPage, 12);
			SetPSTTabPageVisible(false, false);

			ImportTabControl.TabPages.Remove(hfcTabPage);
			ImportTabControl.TabPages.Insert(hfcTabPage, 13);
			SetHFCTabPageVisible(false);

			ImportTabControl.TabPages.Remove(aTFTabPage);
			ImportTabControl.TabPages.Insert(aTFTabPage, 14);
			SetATFTabPageVisible(false);

			ImportTabControl.TabPages.Remove(tTBTabPage);
			ImportTabControl.TabPages.Insert(tTBTabPage, 15);
			SetTTBTabPageVisible(false);

			ImportTabControl.TabPages.Remove(cPSCTabPage);
			ImportTabControl.TabPages.Insert(cPSCTabPage, 16);
			SetCPSCTabPageVisible(false, false);

			ImportTabControl.TabPages.Remove(oMCTabPage);
			ImportTabControl.TabPages.Insert(oMCTabPage, 17);
			SetOMCTabPageVisible(false);

			ImportTabControl.TabPages.Remove(DEATabPage);
			ImportTabControl.TabPages.Insert(DEATabPage, 18);
			SetDEATabPageVisible(false);

			ImportTabControl.TabPages.Remove(aPHISTabPage);
			ImportTabControl.TabPages.Insert(aPHISTabPage, 19);
			SetAPHISTabPageVisible(false);

			ImportTabControl.TabPages.Remove(aMSTabPage);
			ImportTabControl.TabPages.Insert(aMSTabPage, 20);
			SetAMSTabPageVisible(false, false);

			ImportTabControl.TabPages.Remove(DDTCTabPage);
			ImportTabControl.TabPages.Insert(DDTCTabPage, 21);
			SetDDTCTabPageVisible(false);

			ImportTabControl.TabPages.Remove(fWSTabPage);
			ImportTabControl.TabPages.Insert(fWSTabPage, 22);
			SetFWSTabPageVisible(false);

			ImportTabControl.TabPages.Remove(nMFSTabPage);
			ImportTabControl.TabPages.Insert(nMFSTabPage, 23);
			SetNMFSTabPageVisible(false);
		}

		void SetDDTCTabPageVisible(bool visible)
		{
			SetTabPageTabVisible(DDTCTabPage, null, "", visible);
		}

		void SetPGAFDATabPageVisible(bool visible)
		{
			SetTabPageTabVisible(pGAFDATabPage, null, "", visible);
		}

		void SetLaceyActTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(laceyTabPage, laceyUserControl, "PGAs", visible, isListChange);
		}

		void SetNHTSATabPageVisible(bool visible)
		{
			SetTabPageTabVisible(nHTSATabPage, null, "", visible);
		}

		void SetODSTSCATabPageVisible(bool visible)
		{
			SetTabPageTabVisible(oDSTSCATabPage, null, "", visible);
		}

		void SetPSTTabPageVisible(bool isDeclaredOrHasPSTLines, bool isDisclaimed)
		{
			SetTabPageControlVisible(pstUserControl, "PSTLines", isDeclaredOrHasPSTLines);
			SetTabPageControlVisible(pstDisclaimControl, "", isDisclaimed);
			SetTabPageTabVisible(pSTTabPage, null, "", isDeclaredOrHasPSTLines || isDisclaimed);
		}

		void SetVNETabPageVisible(bool visible)
		{
			SetTabPageTabVisible(vNETabPage, null, "", visible);
		}

		void SetHFCTabPageVisible(bool visible)
		{
			SetTabPageTabVisible(hfcTabPage, hfcUserControl, "HFCHeaders", visible);
		}

		void SetOMCTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(oMCTabPage, oMCControl, "OMCHeaders", visible, isListChange);
		}

		void SetAMSTabPageVisible(bool isDeclaredOrHasAMSLines, bool isDisclaimed, bool isListChange = false)
		{
			SetTabPageControlVisible(amsUserControl, "AMSLines", isDeclaredOrHasAMSLines, isListChange);
			SetTabPageControlVisible(amsDisclaimControl, "", isDisclaimed, isListChange);
			SetTabPageTabVisible(aMSTabPage, null, "", isDeclaredOrHasAMSLines || isDisclaimed, isListChange);
		}

		void SetATFTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(aTFTabPage, atfUserControl, "ATFLines", visible, isListChange);
		}

		void SetTTBTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(tTBTabPage, ttbUserControl, "TTBLines", visible, isListChange);
		}

		void SetFWSTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(fWSTabPage, fwsUserControl, "FWSLines", visible, isListChange);
		}

		void SetNMFSTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(nMFSTabPage, nmfsUserControl, "NMFSLines", visible, isListChange);
		}

		void SetCPSCTabPageVisible(bool isDeclaredOrHasCPSCLines, bool isDisclaimedAndReasonIsA, bool isListChange = false)
		{
			SetTabPageControlVisible(cPSCControl, "CPSCLines", isDeclaredOrHasCPSCLines, isListChange);
			SetTabPageControlVisible(cPSCDisclaimPivotControl, "CPSCLines", isDisclaimedAndReasonIsA, isListChange);
			SetTabPageTabVisible(cPSCTabPage, null, "", isDeclaredOrHasCPSCLines || isDisclaimedAndReasonIsA, isListChange);
		}

		void SetDEATabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(DEATabPage, DEAControl, "DEAHeaders", visible, isListChange);
		}

		void SetAPHISTabPageVisible(bool visible, bool isListChange = false)
		{
			SetTabPageTabVisible(aPHISTabPage, aphisUserControl, "APHISHeaders", visible, isListChange);
		}

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
				OGATabsVisibility(haschange);
			}
		}
		CusClassPartPivot fCurrentPartPivot;

		void SetTabPageControlVisible(ZUserControl userControl, string dataMember, bool visible, bool isListChange = false)
		{
			if (userControl != null)
			{
				var oldValue = userControl.Enabled;
				userControl.Enabled = visible;
				userControl.Visible = visible;

				if (oldValue != visible || (visible && isListChange))
				{
					if (visible)
					{
						userControl.SetDataBinding(CurrentDataItem, dataMember);
					}
					else
					{
						userControl.SetDataBinding(null, string.Empty);
					}
				}
			}
		}

		void SetTabPageTabVisible(ZTabPage tabPage, ZUserControl userControl, string dataMember, bool visible, bool isListChange = false)
		{
			if (visible)
			{
				tabPage.TabVisible = visible;
			}

			SetTabPageControlVisible(userControl, dataMember, visible, isListChange);

			if (!visible)
			{
				using (relatedGrid?.SuspendCancelOfNonEditedRowOnLeaving())
				{
					tabPage.TabVisible = visible;
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			taxRateCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (!string.IsNullOrEmpty(dataMember))
			{
				manufacturerAddressControl.BindToOrgList = dataMember + "." + manufacturerAddressControl.BindToOrgList;
				exporterAddressControl.BindToOrgList = dataMember + "." + exporterAddressControl.BindToOrgList;

				if (dataSource != null)
				{
					taxRateCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, dataMember + ".IsTaxRateSpecifiedManually", false, DataSourceUpdateMode.Never));
				}
			}
			manufacturerAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			exporterAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;

			var pivot = dataSource as CusClassPartPivot;
			if (pivot != null)
			{
				AttributesTabPage.TabVisible = false;
			}
		}

		public void AddHookEvent()
		{
			if (CurrentPivot != null && !CurrentPivot.IsDeleted)
			{
				CurrentPivot.Details.CD_LaceyActIndicatorInfo.ValueChanged += new EventHandler(LaceyIndicator_ValueChanged);
				CurrentPivot.Details.CD_ACEFDAIndicatorInfo.ValueChanged += new EventHandler(PGAFDAIndicator_ValueChanged);
				CurrentPivot.Details.CD_NHTSAIndicatorInfo.ValueChanged += new EventHandler(NHTSAIndicator_ValueChanged);
				CurrentPivot.Details.CD_ODSIndicatorInfo.ValueChanged += new EventHandler(ODSTSCAIndicator_ValueChanged);
				CurrentPivot.Details.CD_TSCAClaimIndicatorInfo.ValueChanged += new EventHandler(ODSTSCAIndicator_ValueChanged);
				CurrentPivot.Details.CD_VNEIndicatorInfo.ValueChanged += new EventHandler(VNEIndicator_ValueChanged);
				CurrentPivot.Details.CD_PSTIndicatorInfo.ValueChanged += new EventHandler(PSTIndicator_ValueChanged);
				CurrentPivot.Details.CD_HFCIndicatorInfo.ValueChanged += new EventHandler(HFCIndicator_ValueChanged);
				CurrentPivot.Details.CD_AMSIndicatorInfo.ValueChanged += new EventHandler(AMSIndicator_ValueChanged);
				CurrentPivot.Details.CD_ATFIndicatorInfo.ValueChanged += new EventHandler(ATFIndicator_ValueChanged);
				CurrentPivot.Details.CD_TTBIndicatorInfo.ValueChanged += new EventHandler(TTBIndicator_ValueChanged);
				CurrentPivot.Details.CD_CPSCIndicatorInfo.ValueChanged += new EventHandler(CPSCIndicator_ValueChanged);
				CurrentPivot.Details.CD_CPSCDisclaimReasonInfo.ValueChanged += new EventHandler(CPSCDisclaimReason_ValueChanged);
				CurrentPivot.Details.CD_OMCIndicatorInfo.ValueChanged += new EventHandler(OMCIndicator_ValueChanged);
				CurrentPivot.Details.CD_DEAIndicatorInfo.ValueChanged += new EventHandler(DEAIndicator_ValueChanged);
				CurrentPivot.Details.CD_APHISIndicatorInfo.ValueChanged += new EventHandler(APHISIndicator_ValueChanged);
				CurrentPivot.Details.CD_DDTCIndicatorInfo.ValueChanged += new EventHandler(DDTCIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFS370IndicatorInfo.ValueChanged += new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSCOAIndicatorInfo.ValueChanged += new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSAMRIndicatorInfo.ValueChanged += new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSHMSIndicatorInfo.ValueChanged += new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSSIMPIndicatorInfo.ValueChanged += new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_FWSIndicatorInfo.ValueChanged += new EventHandler(FWSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NOPIndicatorInfo.ValueChanged += new EventHandler(AMSIndicator_ValueChanged);
			}
		}

		public void UnHookEvent()
		{
			if (CurrentPivot != null && !CurrentPivot.IsDeleted)
			{
				CurrentPivot.Details.CD_LaceyActIndicatorInfo.ValueChanged -= new EventHandler(LaceyIndicator_ValueChanged);

				CurrentPivot.Details.CD_ACEFDAIndicatorInfo.ValueChanged -= new EventHandler(PGAFDAIndicator_ValueChanged);
				CurrentPivot.Details.CD_NHTSAIndicatorInfo.ValueChanged -= new EventHandler(NHTSAIndicator_ValueChanged);
				CurrentPivot.Details.CD_ODSIndicatorInfo.ValueChanged -= new EventHandler(ODSTSCAIndicator_ValueChanged);
				CurrentPivot.Details.CD_TSCAClaimIndicatorInfo.ValueChanged -= new EventHandler(ODSTSCAIndicator_ValueChanged);
				CurrentPivot.Details.CD_VNEIndicatorInfo.ValueChanged -= new EventHandler(VNEIndicator_ValueChanged);
				CurrentPivot.Details.CD_PSTIndicatorInfo.ValueChanged -= new EventHandler(PSTIndicator_ValueChanged);
				CurrentPivot.Details.CD_HFCIndicatorInfo.ValueChanged -= new EventHandler(HFCIndicator_ValueChanged);
				CurrentPivot.Details.CD_ATFIndicatorInfo.ValueChanged -= new EventHandler(ATFIndicator_ValueChanged);
				CurrentPivot.Details.CD_AMSIndicatorInfo.ValueChanged -= new EventHandler(AMSIndicator_ValueChanged);
				CurrentPivot.Details.CD_TTBIndicatorInfo.ValueChanged -= new EventHandler(TTBIndicator_ValueChanged);
				CurrentPivot.Details.CD_CPSCIndicatorInfo.ValueChanged -= new EventHandler(CPSCIndicator_ValueChanged);
				CurrentPivot.Details.CD_CPSCDisclaimReasonInfo.ValueChanged -= new EventHandler(CPSCDisclaimReason_ValueChanged);
				CurrentPivot.Details.CD_OMCIndicatorInfo.ValueChanged -= new EventHandler(OMCIndicator_ValueChanged);
				CurrentPivot.Details.CD_DEAIndicatorInfo.ValueChanged -= new EventHandler(DEAIndicator_ValueChanged);
				CurrentPivot.Details.CD_APHISIndicatorInfo.ValueChanged -= new EventHandler(APHISIndicator_ValueChanged);
				CurrentPivot.Details.CD_DDTCIndicatorInfo.ValueChanged -= new EventHandler(DDTCIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFS370IndicatorInfo.ValueChanged -= new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSCOAIndicatorInfo.ValueChanged -= new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSAMRIndicatorInfo.ValueChanged -= new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSHMSIndicatorInfo.ValueChanged -= new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NMFSSIMPIndicatorInfo.ValueChanged -= new EventHandler(NMFSIndicator_ValueChanged);
				CurrentPivot.Details.CD_FWSIndicatorInfo.ValueChanged -= new EventHandler(FWSIndicator_ValueChanged);
				CurrentPivot.Details.CD_NOPIndicatorInfo.ValueChanged -= new EventHandler(AMSIndicator_ValueChanged);
			}
		}

		public void ODSTSCAIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetODSTSCAIndicator();
		}

		void SetODSTSCAIndicator()
		{
			SetODSTSCATabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_ODSIndicator) || OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_TSCAClaimIndicator));
		}

		public void OMCIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetOMCIndicator();
		}

		void SetOMCIndicator(bool isListChange = false)
		{
			SetOMCTabPageVisible((OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_OMCIndicator) || CurrentPivot.HasOMCHeaders), isListChange);
		}

		public void VNEIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetVNEIndicator();
		}

		void SetVNEIndicator()
		{
			SetVNETabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_VNEIndicator) || CurrentPivot.HasVNELines);
		}

		public void PSTIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetPSTIndicator();
		}

		void SetPSTIndicator()
		{
			var isDeclaredOrHasPSTLines = OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_PSTIndicator) || CurrentPivot.HasPSTLines;
			var isDisclaimed = OGAIndicatorList.IsToBeDisclaimed(CurrentPivot.Details.CD_PSTIndicator);
			SetPSTTabPageVisible(isDeclaredOrHasPSTLines, isDisclaimed);
		}

		public void HFCIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetHFCIndicator();
		}

		void SetHFCIndicator()
		{
			SetHFCTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_HFCIndicator) || CurrentPivot.HasHFCHeaders);
		}

		public void LaceyIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetLaceyIndicator();
		}
		void SetLaceyIndicator(bool isListChange = false)
		{
			SetLaceyActTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_LaceyActIndicator) || CurrentPivot.HasLaceyActData, isListChange);
		}

		public void DDTCIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetDDTCIndicator();
		}
		void SetDDTCIndicator()
		{
			SetDDTCTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_DDTCIndicator) || CurrentPivot.HasImportDDTCData);
		}

		public void PGAFDAIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetPGAFDAIndicator();
		}

		void SetPGAFDAIndicator()
		{
			SetPGAFDATabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_ACEFDAIndicator) || CurrentPivot.HasACEFDAs);
		}

		public void TTBIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetTTBIndicator();
		}

		void SetTTBIndicator(bool isListChange = false)
		{
			SetTTBTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_TTBIndicator) || CurrentPivot.HasTTBData, isListChange);
		}

		void SetNMFSIndicator(bool isListChange = false)
		{
			SetNMFSTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NMFS370Indicator)
				|| OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NMFSAMRIndicator)
				|| OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NMFSHMSIndicator)
				|| OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NMFSSIMPIndicator)
				|| OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NMFSCOAIndicator)
				|| CurrentPivot.HasNMFSLines, isListChange);
		}

		public void NMFSIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetNMFSIndicator();
		}

		void SetFWSIndicator(bool isListChange = false)
		{
			SetFWSTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_FWSIndicator) || CurrentPivot.HasFWSLines, isListChange);
		}

		public void FWSIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetFWSIndicator();
		}

		public void AMSIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetAMSIndicator();
		}

		void SetAMSIndicator(bool isListChange = false)
		{
			var isDeclaredOrHasAMSLines = OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NOPIndicator) || OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_AMSIndicator) || CurrentPivot.HasAMSData;
			var isDisclaimed = OGAIndicatorList.IsToBeDisclaimed(CurrentPivot.Details.CD_AMSIndicator);
			SetAMSTabPageVisible(isDeclaredOrHasAMSLines, isDisclaimed, isListChange);
		}

		public void CPSCIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetCPSCIndicatorOrDisclaimReason();
		}

		public void CPSCDisclaimReason_ValueChanged(object sender, EventArgs e)
		{
			SetCPSCIndicatorOrDisclaimReason();
		}

		void SetCPSCIndicatorOrDisclaimReason(bool isListChange = false)
		{
			var isDisclaimedAndReasonIsA = OGAIndicatorList.IsToBeDisclaimed(CurrentPivot.Details.CD_CPSCIndicator) && CurrentPivot.Details.CD_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A;
			var isDeclaredOrHasCPSCLines = OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_CPSCIndicator) || (CurrentPivot.HasCPSCLines && !isDisclaimedAndReasonIsA);
			SetCPSCTabPageVisible(isDeclaredOrHasCPSCLines, isDisclaimedAndReasonIsA, isListChange);
		}

		public void NHTSAIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetNHTSAIndicator();
		}

		void SetNHTSAIndicator()
		{
			SetNHTSATabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_NHTSAIndicator) || CurrentPivot.HasNHTSALines);
		}

		public void ATFIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetATFIndicator();
		}

		void SetATFIndicator(bool isListChange = false)
		{
			SetATFTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_ATFIndicator) || CurrentPivot.HasATFLines, isListChange);
		}

		public void DEAIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetDEAIndicator();
		}

		void SetDEAIndicator(bool isListChange = false)
		{
			SetDEATabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_DEAIndicator) || CurrentPivot.HasDEAHeaders, isListChange);
		}

		public void OGAIndicator_ValueChanged(object sender, EventArgs e)
		{
			OGATabsVisibility();
		}

		public void APHISIndicator_ValueChanged(object sender, EventArgs e)
		{
			SetAPHISIndicator();
		}

		void SetAPHISIndicator(bool isListChange = false)
		{
			SetAPHISTabPageVisible(OGAIndicatorList.IsToBeDeclared(CurrentPivot.Details.CD_APHISIndicator) || CurrentPivot.HasAPHISLines, isListChange);
		}

		void OGATabsVisibility(bool isListChange = false)
		{
			if (CurrentPivot != null && !CurrentPivot.IsDeleted)
			{
				SetLaceyIndicator(isListChange);

				SetPGAFDAIndicator();
				SetNHTSAIndicator();
				SetODSTSCAIndicator();
				SetVNEIndicator();
				SetPSTIndicator();
				SetHFCIndicator();
				SetATFIndicator(isListChange);
				SetAMSIndicator(isListChange);
				SetTTBIndicator(isListChange);
				SetCPSCIndicatorOrDisclaimReason(isListChange);
				SetOMCIndicator(isListChange);
				SetDEAIndicator(isListChange);
				SetAPHISIndicator(isListChange);
				SetDDTCIndicator();
				SetNMFSIndicator(isListChange);
				SetFWSIndicator(isListChange);
			}
			else
			{
				SetLaceyActTabPageVisible(false);

				SetPGAFDATabPageVisible(false);
				SetNHTSATabPageVisible(false);
				SetODSTSCATabPageVisible(false);
				SetVNETabPageVisible(false);
				SetPSTTabPageVisible(false, false);
				SetHFCTabPageVisible(false);
				SetATFTabPageVisible(false);
				SetAMSTabPageVisible(false, false);
				SetTTBTabPageVisible(false);
				SetCPSCTabPageVisible(false, false);
				SetOMCTabPageVisible(false);
				SetDEATabPageVisible(false);
				SetAPHISTabPageVisible(false);
				SetDDTCTabPageVisible(false);
				SetNMFSTabPageVisible(false);
				SetFWSTabPageVisible(false);
			}
		}
	}
}

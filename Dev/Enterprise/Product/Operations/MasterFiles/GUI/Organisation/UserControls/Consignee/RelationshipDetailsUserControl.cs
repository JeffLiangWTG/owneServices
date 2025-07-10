using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RelationshipDetailsUserControl : OrganisationContainerControl
	{
		public RelationshipDetailsUserControl()
		{
			InitializeComponent();
		}

		public new OrgSupplierBuyerLink CurrentDataItem
		{
			get { return (OrgSupplierBuyerLink)base.CurrentDataItem; }
		}

		#region Plugins

		public void SetupPlugIn(ZGrid linkBoundGrid)
		{
			modesAndTracking.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.US.OrgBuyerSupplierLinkAdditionalCustomsDetails, linkBoundGrid);
			modesAndTracking.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.CN.OrgBuyerSupplierLinkChinaCustomsDetails, linkBoundGrid);
			relationshipTabControl.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.CN.OrgSupBuyLinkTrnModeAdditionalCustomsDetails, relationshipsGrid);
		}
		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				var prefix = dataMember + ".";
				pickupAddressControl.CorrectBindToOrgList(prefix);
				deliverAddressControl.CorrectBindToOrgList(prefix);
				notifyPartyAddressControl.CorrectBindToOrgList(prefix);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OL_RN_NKImporterCountryInfo.ValueChanged -= new EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				CurrentDataItem.OL_RN_NKImporterCountryInfo.ValueChanged -= new EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
				CurrentDataItem.OL_RN_NKImporterCountryInfo.ValueChanged += new EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}
		}

		void OL_RN_NKImporterCountryInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateUSPortsLocation();
		}

		void UpdateUSPortsLocation()
		{
			if (supBuyLinkTrnMode != null)
			{
				int ladingY = USPortOfLadingCodeFindBox.Top;
				int unladingY = USPortOfUnLadingCodeFindBox.Top;
				bool ladingOnTop = ladingY < unladingY;
				bool preferLadingOnTop = supBuyLinkTrnMode.IsUSImporterCountry;

				if (ladingOnTop != preferLadingOnTop)
				{
					ControlDpiScalingHelper.SetTop(ref USPortOfLadingCodeFindBox, unladingY, false);
					ControlDpiScalingHelper.SetTop(ref USPortOfUnLadingCodeFindBox, ladingY, false);
					supBuyLinkTrnMode.PF_USPortOfLadingInfo.RefreshBinding();
					supBuyLinkTrnMode.PF_USPortOfUnLadingInfo.RefreshBinding();
				}
			}
		}

		void SetUSPortsVisibility()
		{
			if (relationshipsGrid.ListManager != null)
			{
				bool isUSImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates;

				USPortOfLadingCodeFindBox.Visible = isUSImporterCountry;
				USPortOfUnLadingCodeFindBox.Visible = isUSImporterCountry;

				if (isUSImporterCountry)
				{
					relationshipsGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
					ListManager_CurrentChanged(this, EventArgs.Empty);
				}
			}
		}

		void relationshipGrid_AfterBind(object sender, EventArgs e)
		{
			SetUSPortsVisibility();
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (relationshipsGrid.ListManager != null && relationshipsGrid.ListManager.Position > -1)
			{
				supBuyLinkTrnMode = (OrgSupBuyLinkTrnMode)relationshipsGrid.ListManager.GetCurrent();
				UpdateUSPortsLocation();
			}
		}
		OrgSupBuyLinkTrnMode supBuyLinkTrnMode;
	}
}

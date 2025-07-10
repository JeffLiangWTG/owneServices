using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.GUI;

#region Disable warnings

#pragma warning disable WTG1001 // Do not use the 'private' keyword.
#pragma warning disable SA1508 // Closing braces should not be preceded by blank line
#pragma warning disable IDE0003 // Remove 'this' qualification

#endregion

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class TransportDetailsUserControl : ZUserControl
	{
		public TransportDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChangeLoadingUNLOCOPortsComponentVisibility();
			ChangeDischargeUNLOCOPortsComponentVisibility();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			const string IsVisibleForBindingString = "IsVisibleForBinding";

			if (dataSource != null)
			{
				panelTransportDetailGroup.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxFlightNo.DataBindings.RemoveBinding(IsVisibleForBindingString);
				masterBillControl.DataBindings.RemoveBinding(IsVisibleForBindingString);
				codeFindBoxVessel.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxVoyageNo.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxOceanBill.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxMailReference.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxJourney.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxMasterBill.DataBindings.RemoveBinding(IsVisibleForBindingString);
				textBoxTripID.DataBindings.RemoveBinding(IsVisibleForBindingString);
				dropEditContainerMode.DataBindings.RemoveBinding(IsVisibleForBindingString);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				panelTransportDetailGroup.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsNotEmptyForBinding"), false, DataSourceUpdateMode.Never));
				textBoxFlightNo.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsAirForBinding"), false, DataSourceUpdateMode.Never));
				masterBillControl.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsAirForBinding"), false, DataSourceUpdateMode.Never));
				codeFindBoxVessel.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsSeaForBinding"), false, DataSourceUpdateMode.Never));
				textBoxVoyageNo.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsSeaForBinding"), false, DataSourceUpdateMode.Never));
				textBoxOceanBill.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsSeaForBinding"), false, DataSourceUpdateMode.Never));
				textBoxMailReference.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsMailForBinding"), false, DataSourceUpdateMode.Never));
				textBoxJourney.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsRailForBinding"), false, DataSourceUpdateMode.Never));
				textBoxMasterBill.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsRailOrRoadForBinding"), false, DataSourceUpdateMode.Never));
				textBoxTripID.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsRailOrRoadOrMailForBinding"), false, DataSourceUpdateMode.Never));
				dropEditContainerMode.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "IsContainerSupported"), false, DataSourceUpdateMode.Never));
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var clearance = (CusUSLVClearance)CurrentDataItem;
			if (clearance != null)
			{
				clearance.ULH_TransportModeInfo.ValueChanged -= ChangeDischargeUNLOCOPortsComponentType;
				clearance.ULH_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeLoadingUNLOCOPortsComponentType;
				clearance.ULH_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeDischargeUNLOCOPortsComponentType;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var clearance = (CusUSLVClearance)CurrentDataItem;
			if (clearance != null)
			{
				clearance.ULH_TransportModeInfo.ValueChanged += ChangeDischargeUNLOCOPortsComponentType;
				clearance.ULH_RL_NKPortOfLoadingInfo.ValueChanged += ChangeLoadingUNLOCOPortsComponentType;
				clearance.ULH_RL_NKPortOfDischargeInfo.ValueChanged += ChangeDischargeUNLOCOPortsComponentType;
			}
		}

		void ChangeLoadingUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeLoadingUNLOCOPortsComponentVisibility();
		}

		void ChangeLoadingUNLOCOPortsComponentVisibility()
		{
			var clearance = (CusUSLVClearance)CurrentDataItem;
			if (clearance != null)
			{
				dropEditLoadingPort.Visible = clearance.ULH_PortOfLoadingIsDropEdit;
				codeFindBoxLoadingPort.Visible = !clearance.ULH_PortOfLoadingIsDropEdit;
			}
		}

		void ChangeDischargeUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeDischargeUNLOCOPortsComponentVisibility();
		}

		void ChangeDischargeUNLOCOPortsComponentVisibility()
		{
			var clearance = (CusUSLVClearance)CurrentDataItem;
			if (clearance != null)
			{
				dropEditDischargePort.Visible = clearance.ULH_PortOfDischargeIsDropEdit;
				codeFindBoxDischargePort.Visible = !clearance.ULH_PortOfDischargeIsDropEdit;
			}
		}

		string GetBindingMemberString(object dataSource, string dataMember)
		{
			var result = dataMember;
			if (dataSource is CusUSLVConsignment)
			{
				result = "Shipment." + dataMember;
			}
			return result;
		}
	}
}

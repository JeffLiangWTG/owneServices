using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingMainPage : ZUserControl
	{
		public BillOfLadingMainPage()
		{
			InitializeComponent();

			JS_GoodsDescriptionTextBoxBoundTextBox.ButtonText = Res.GetString("e19d4f89-12e9-49dd-a5bd-ff766c0e325e", "Detail");
			JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.ButtonText = Res.GetString("e19d4f89-12e9-49dd-a5bd-ff766c0e325e", "Detail");
		}

		public void SetForContainerMode(ZString containerMode)
		{
			bool isVisible = containerMode != Core.Constants.ContainerModes.FCL;

			JS_OuterPacksBoundZCalcDropEdit.Visible = isVisible;
			JS_ActualWeightBoundCalcDropEdit.Visible = isVisible;
			JS_ActualVolumeBoundZCalcDropEdit.Visible = isVisible;

			if (containerMode == Constants.ContainerModes.RollOnRollOff)
			{
				JS_OuterPacksBoundZCalcDropEdit.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("3DC31276-F3E7-4506-B5EB-6BE01052303F", "Vehicles");
			}
			else
			{
				JS_OuterPacksBoundZCalcDropEdit.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("353C284D-E9CC-40F3-83DB-2983F9024C35", "Packages");
			}
		}

		#region Overrides

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_PackingModeInfo.ValueChanged += JS_PackingModeInfo_ValueChanged;
				Shipment.JS_RL_NKOriginInfo.ValueChanged += JS_RL_NKOriginInfo_ValueChanged;
				Shipment.JS_RL_NKDestinationInfo.ValueChanged += JS_RL_NKDestinationInfo_ValueChanged;
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				if (Shipment != null)
				{
					Shipment.JS_PackingModeInfo.ValueChanged -= JS_PackingModeInfo_ValueChanged;
					Shipment.JS_RL_NKOriginInfo.ValueChanged -= JS_RL_NKOriginInfo_ValueChanged;
					Shipment.JS_RL_NKDestinationInfo.ValueChanged -= JS_RL_NKDestinationInfo_ValueChanged;
				}
			}
		}

		void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_PackingModeInfo);
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKDestinationInfo);
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKOriginInfo);
		}

		protected virtual void ConfirmReversal(ZPropertyInfo info)
		{
			CommissionReversalConfirmationHelper.ConfirmReversal(Shipment, info, new[]
							{
								Shipment.JS_RL_NKOriginInfo,
								Shipment.JS_RL_NKDestinationInfo,
								Shipment.JS_PackingModeInfo,
							});
		}

		#endregion

		#region Implementation

		BillOfLading Shipment
		{
			get { return (BillOfLading)CurrentDataItem; }
		}

		#endregion

		#region Events

		void ConsignorDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!Shipment.ConsignorDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
			{
				ConsignorDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		void ConsigneeDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!Shipment.ConsigneeDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				ConsigneeDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		#endregion
	}
}



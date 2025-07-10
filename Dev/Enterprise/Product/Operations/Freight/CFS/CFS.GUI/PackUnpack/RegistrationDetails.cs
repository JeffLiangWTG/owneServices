using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class RegistrationDetails : ZUserControl
	{
		public RegistrationDetails()
		{
			InitializeComponent();
			HookEvents();
		}

		#region Binding / Loading

		CFSContainer CFSContainer
		{
			get { return (CFSContainer)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				UnhookEvents();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				HookEvents();

				SetControlStateOnPurposeTypeChanged();
				SetCFSControlState();
				SetTransportModeControlState();

				AUContainerArrivalPanel.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia;
				AUContainerDispatchPanel.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia;
			}
		}

		#endregion

		#region Events

		void HookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_PurposeInfo.ValueChanged += new EventHandler(JC_PurposeInfo_ValueChanged);
				CFSContainer.JC_JA_NKPortOfLoadingInfo.ValueChanged += new EventHandler(SetCFSControlState);
				CFSContainer.JC_JB_NKPortOfDischargeInfo.ValueChanged += new EventHandler(SetCFSControlState);
				CFSContainer.JC_TransportModeInfo.ValueChanged += new EventHandler(JC_TransportModeInfo_ValueChanged);
				CFSContainer.JC_JKInfo.ValueChanged += new EventHandler(SetCFSControlState);
			}
		}

		void UnhookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_PurposeInfo.ValueChanged -= new EventHandler(JC_PurposeInfo_ValueChanged);
				CFSContainer.JC_JA_NKPortOfLoadingInfo.ValueChanged -= new EventHandler(SetCFSControlState);
				CFSContainer.JC_JB_NKPortOfDischargeInfo.ValueChanged -= new EventHandler(SetCFSControlState);
				CFSContainer.JC_TransportModeInfo.ValueChanged -= new EventHandler(JC_TransportModeInfo_ValueChanged);
				CFSContainer.JC_JKInfo.ValueChanged -= new EventHandler(SetCFSControlState);
			}
		}

		void JC_PurposeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetControlStateOnPurposeTypeChanged();
		}

		void SetCFSControlState(object sender, EventArgs e)
		{
			SetCFSControlState();
		}

		void JC_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTransportModeControlState();
		}

		#endregion

		#region Form and Control State

		void SetCFSControlState()
		{
			JC_IsSealOkCheckBox.Visible = CFSContainer.IsImport() || CFSContainer.IsDomestic();

			switch (CFSContainer.PackOrUnpackStatus)
			{
				case PackUnpackStatusHelper.PackUnpackStatus.Pack:
					ArrivalEmptyPanel.Visible = true;
					ArrivalCTOSlotPanel.Visible = false;
					DispatchEmptyPanel.Visible = false;
					DispatchCTOSlotPanel.Visible = true;
					PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("53D46716-A4CD-4702-9BB0-FA9E21F876AB", "Pack Details");
					JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("EEAF9211-F6A5-4274-B89E-9CB5E8558C0A", "Pack Date");
					AvailableStorageDatesPanel.Visible = false;
					break;
				case PackUnpackStatusHelper.PackUnpackStatus.Unpack:
					ArrivalEmptyPanel.Visible = false;
					ArrivalCTOSlotPanel.Visible = true;
					DispatchEmptyPanel.Visible = true;
					DispatchCTOSlotPanel.Visible = false;
					PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("40AD730A-B023-44ad-B3DA-B3DB5AA150A2", "Unpack Details");
					JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("601717FB-DB99-4422-8B15-616E8F4B9456", "Unpack Date");
					AvailableStorageDatesPanel.Visible = true;
					break;
				default:
					PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("B845D38F-F6DF-4722-9245-FC95321E8C43", "Pack/Unpack Details");
					JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("B72377D6-DF40-44b0-9889-4AF891502FD7", "Pack/Unpack Date");
					AvailableStorageDatesPanel.Visible = true;
					break;
			}
		}

		void SetTransportModeControlState()
		{
			ZString mastebBillNoResString = Res.GetString("D820D068-8077-483f-A3FB-DC823631C902", "Master Bill No");
			ZString oceanBillNoResString = Res.GetString("2FB20DDC-ECE0-4ba7-9927-8A3E9C337992", "Ocean Bill No");

			if (CFSContainer.JC_TransportMode == Constants.TransportModes.Air)
			{
				JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = mastebBillNoResString;
				StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = mastebBillNoResString;
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Rail)
			{
				JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
				StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Road)
			{
				JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
				StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
			else
			{
				JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
				StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
		}

		void SetControlStateOnPurposeTypeChanged()
		{
			PurposeSpecificDetailsTabControl.TabPages.Remove(CFSTabPage);
			PurposeSpecificDetailsTabControl.TabPages.Remove(StorageTabPage);

			if (CFSContainer.JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS)
			{
				SwitchTabPages(CFSTabPage, StorageTabPage);
			}
			else
			{
				SwitchTabPages(StorageTabPage, CFSTabPage);
			}
		}

		void SwitchTabPages(ZTabPage showThisTabPage, ZTabPage hideThisTabPage)
		{
			if (!showThisTabPage.TabVisible)
			{
				PurposeSpecificDetailsTabControl.TabPages.Insert(showThisTabPage, 0);
			}
			PurposeSpecificDetailsTabControl.SelectedTab = showThisTabPage;

			if (hideThisTabPage.TabVisible)
			{
				PurposeSpecificDetailsTabControl.TabPages.Remove(hideThisTabPage);
			}
		}

		#endregion

	}
}


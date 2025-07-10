using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConfigUserControl : OrganisationSecurityContainerControl
	{
		public ConfigUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning && !loaded)
			{
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.US.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.CA.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.ZA.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.DE.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.CN.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.FR.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.TW.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.KR.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.BR.OrganisationDetailsPlugIn);
				ConfigTabControl.PlugIns.Add(ControllerIDs.Customs.IN.OrganisationDetailsPlugIn);

				CusCodesGrid.FindBoxColumnModuleShowing += new EventHandler<FindBoxColumnModuleShowingEventArgs>(CusCodesGrid_FindBoxColumnModuleShowing);
				AddNewMenuItemsForCusCodesGrid();
				HookEventsForMultiControlColumn();
				loaded = true;
			}
		}

		void HookEventsForMultiControlColumn()
		{
			foreach (var column in CusCodesGrid.Columns)
			{
				var multiControlColumnStyle = column.ColumnStyle as ZMultiControlColumnStyle;
				if (multiControlColumnStyle != null)
				{
					multiControlColumnStyle.EditControl.ControlAdded -= new ControlEventHandler(EditControl_ControlAdded);
					multiControlColumnStyle.EditControl.ControlAdded += new ControlEventHandler(EditControl_ControlAdded);
				}
			}
		}

		void AddNewMenuItemsForCusCodesGrid()
		{
			ExportCodeListToExcelMenuItem = new ZMenuItem(ResString.GetMultilingualString("e77fa707-46c1-424e-a23c-d52619f849cc", "Export Code Type List"), ExportCodeList);
			CusCodesGrid.ContextMenu.MenuItems.Add(CusCodesGrid.ExportAllColumnsToExcelMenuItem.Index, ExportCodeListToExcelMenuItem);

			MarkVerifiedMenuItem = new ZMenuItem(ResString.GetMultilingualString("FE63CF4E-6F43-44C6-9B05-41D7E5C20CE5", "Mark as Verified"), new EventHandler(HandleMarkAsVerified));
			CusCodesGrid.ContextMenu.MenuItems.Add(MarkVerifiedMenuItem);

			MarkNotVerifiedMenuItem = new ZMenuItem(ResString.GetMultilingualString("C7E6258B-4D81-426E-A5BE-41E8C7CB7E62", "Mark as Not Verified"), new EventHandler(HandleMarkAsNotVerified));
			CusCodesGrid.ContextMenu.MenuItems.Add(MarkNotVerifiedMenuItem);

			CusCodesGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ExportCodeList(object sender, EventArgs e)
		{
			if (OrgHeader.ExportExcelCheckPoint.IsAllowed)
			{
				new ExcelExporter(new OrgRegistrationCodeTypeReader(), OrgRegistrationCodeTypeCollection.ColumnsForExcel, new ExcelExporterGuiNotifications(FindForm())).ExportIntoAndOpenExcel();
			}
			else
			{
				OrgHeader.ExportExcelCheckPoint.ShowError();
			}
		}

		bool loaded;

		void HandleMarkAsVerified(object sender, EventArgs e)
		{
			HandleMarkAsVerifiedOrNotVerified(true);
		}

		void HandleMarkAsNotVerified(object sender, EventArgs e)
		{
			HandleMarkAsVerifiedOrNotVerified(false);
		}

		void HandleMarkAsVerifiedOrNotVerified(ZBool verified)
		{
			if (CusCodesGrid.ListManager.GetCurrent() is OrgCusCode orgCusCode)
			{
				orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(verified, ZString.Empty, ZString.Empty);
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var cusCode = CusCodesGrid.ListManager.GetCurrent() as OrgCusCode;
			MarkVerifiedMenuItem.Visible = cusCode != null && cusCode.VerificationSupported && !cusCode.IsVerified;
			MarkNotVerifiedMenuItem.Visible = cusCode != null && cusCode.VerificationSupported && cusCode.IsVerified;
		}

		#region GUI Setup

		public OrgHeader Organisation
		{
			get { return (OrgHeader)CurrentDataItem; }
		}

		#region XmlUniversal

		EDICommunicationsMode lastSubscribedMode;

		void EdiCommsGrid_AfterBind(object sender, EventArgs eventArgs)
		{
			HandleCommunicationModeChanged();
			if (EdiCommsGrid.ListManager != null)
			{
				EdiCommsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			HandleCommunicationModeChanged();
		}

		void EdiCommsGrid_Disposed(object sender, EventArgs eventArgs)
		{
			SubscribeToModeChanges(null);
		}

		void HandleCommunicationModeChanged()
		{
			EDICommunicationsMode communicationMode = null;
			if (Organisation != null && 0 <= EdiCommsGrid.CurrentRowIndex && EdiCommsGrid.CurrentRowIndex < Organisation.EDICommunicationsModes.Count)
			{
				communicationMode = Organisation.EDICommunicationsModes[EdiCommsGrid.CurrentRowIndex];
				SetupXmlUniversalControlsVisibility(communicationMode);
				RefreshDestinationLabelText(communicationMode);
			}
			else
			{
				XmlUniversalGroupBox.Visible = false;
				DestinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("d78e37a3-8db6-43a2-a805-f3ed34099ecb", "Address");
			}

			SubscribeToModeChanges(communicationMode);
		}

		void SetupXmlUniversalControlsVisibility(EDICommunicationsMode communicationMode)
		{
			var fileFormat = communicationMode.EK_FileFormat;

			XmlUniversalGroupBox.Visible =
				(fileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment || fileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction) &&
				communicationMode.EK_CommsDirection == EDICommunicationsModeCommsDirectionList.Codes.Transmit;

			if (XmlUniversalGroupBox.Visible)
			{
				XmlUniversalGroupBox.CaptionResourceString = fileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction ?
						Res.GetData("ConfigUserControl|D6960112-DFA1-4D26-96E3-B6183333F676", "XML Universal Transaction") :
						Res.GetData("ConfigUserControl|f220ef33-b1ef-4e8d-9286-7a1210f8ce1b", "XML Universal Shipment");
				XmlUniversalGroupBox.UpdateCaption();
			}
		}

		void SubscribeToModeChanges(EDICommunicationsMode communicationMode)
		{
			if (lastSubscribedMode != null)
			{
				lastSubscribedMode.EK_FileFormatInfo.ValueChanged -= OnCommunicationModeChanged;
				lastSubscribedMode.EK_CommsDirectionInfo.ValueChanged -= OnCommunicationModeChanged;
				lastSubscribedMode.EK_CommunicationsTransportInfo.ValueChanged -= OnCommunicationModeChanged;
			}
			lastSubscribedMode = communicationMode;
			if (lastSubscribedMode != null)
			{
				lastSubscribedMode.EK_FileFormatInfo.ValueChanged += OnCommunicationModeChanged;
				lastSubscribedMode.EK_CommsDirectionInfo.ValueChanged += OnCommunicationModeChanged;
				lastSubscribedMode.EK_CommunicationsTransportInfo.ValueChanged += OnCommunicationModeChanged;
			}
		}

		void OnCommunicationModeChanged(object sender, EventArgs eventArgs)
		{
			HandleCommunicationModeChanged();
		}

		#endregion // XmlUniversalShipment

		#endregion

		#region Event Handlers

#if DEBUG
		internal
#endif
 void CusCodesGrid_FindBoxColumnModuleShowing(object sender, FindBoxColumnModuleShowingEventArgs e)
		{
			if (e.ColumnStyle.MappingName == "SecuredCustomsRegNo")
			{
				ZPopupFindBox findBox = (ZPopupFindBox)((ZMultiCombinationControl)((ZMultiControlColumnStyle)e.ColumnStyle).EditControl).CurrentEditor;
				if (findBox != null)
				{
					findBox.GetCountryCode = () => ((OrgCusCode)e.CurrentBusinessObject).OK_RN_NKCodeCountry;
				}
			}
		}

		void EditControl_ControlAdded(object sender, ControlEventArgs e)
		{
			ZLinkLabel linkLabel = e.Control as ZLinkLabel;
			if (linkLabel != null)
			{
				linkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(LinkLabel_Clicked);
				linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_Clicked);
			}
		}

		void LinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (CusCodesGrid.ListManager.GetCurrent() is OrgCusCode orgCusCode && !orgCusCode.SnapshotContextForDisplay.IsEmpty)
			{
				using (var form = new VerificationInterpretationForm(orgCusCode.SnapshotContextForDisplay))
				{
					ZFormModaliser.ShowDialogAndDispose(form);
				}
			}
		}

		void EdiCommsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			HandleCommunicationModeChanged();
		}

		void RefreshDestinationLabelText(EDICommunicationsMode communicationMode)
		{
			var caption = Res.GetString("664600c5-0c3e-458b-95d8-72bb6af7956d", "Address");
			var commsDirection = communicationMode.EK_CommsDirection;
			var commsTransport = communicationMode.EK_CommunicationsTransport;

			if (commsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)
			{
				caption = Res.GetString("2796a70c-0a46-48b4-97ae-c91858f2d9aa", "eHub Client ID");
			}
			else if (commsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
			{
				caption = Res.GetString("2796a70c-0a46-48b4-97ae-c91858f2d9ab", "Recipient ID");
			}
			else if (commsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface)
			{
				caption = Res.GetString("6af90b0c-252d-4813-be35-db66071bcd7d", "xT Client ID");
			}
			else
			{
				if (commsDirection == EDICommunicationsModeCommsDirectionList.Codes.Transmit)
				{
					caption = Res.GetString("b255b586-1400-4e22-8d22-9d8336b08ce5", "Dest. Address");
				}
				else if (commsDirection == EDICommunicationsModeCommsDirectionList.Codes.Receive)
				{
					caption = Res.GetString("0080e927-f556-466d-b184-c12f8d2393d2", "Origin Address");
				}
			}
			DestinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption = caption;
		}

		#endregion

		#region Dispose

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

		#endregion

	}

	#region ZCusCodesGrid

	public class ZCusCodesGrid : ZGrid
	{
		protected override void ExportIntoAndOpenExcel()
		{
			var checkPoint = OrgHeader.ExportExcelCheckPoint;
			if (checkPoint.IsAllowed)
			{
				base.ExportIntoAndOpenExcel();
			}
			else
			{
				checkPoint.ShowError();
			}
		}
	}

	#endregion
}

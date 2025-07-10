using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using ConsignmentAddressType = Enterprise.Integration.Customs.ConsignmentAddressType;

namespace Enterprise.eTail.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class HVLVConsignmentDetailsUserControl : ZUserControl
	{
		public HVLVConsignmentDetailsUserControl()
		{
			InitializeComponent();
			zTextBoxStandAloneDeclaration.Hotkeys.RegisterHotKey(Keys.F3, PopupDeclarationEditFormIfExists, Res.GetString("e841a451-28d6-47d9-b438-9624c82ec6f1", "Open Declaration"));
			zTextBoxExportCustomsClearanceStatus.VisibleChanged += ZTextBox_VisibleChanged;
			zTextBoxExportReleaseStatus.VisibleChanged += ZTextBox_VisibleChanged;

			zButtonConsigneeConvertToOrganisation.AllowOverlap(tabConsignee);
			zButtonShipperConvertToOrganisation.AllowOverlap(tabShipper);
			zButtonReturnConvertToOrganisation.AllowOverlap(tabReturn);
			zButtonConvertToStandAloneDeclaration.AllowOverlap(zButtonEditStandAloneDeclaration);
			zButtonAcceptedFailedAsEntered.AllowOverlap(zButtonReScreen);
			zAddressConsignee.AllowOutsideOfParent();
			zAddressShipper.AllowOutsideOfParent();
			zAddressReturn.AllowOutsideOfParent();
		}

		void ZTextBox_VisibleChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem == null && sender is ZTextBox textBox)
			{
				textBox.Visible = false;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			const string IsVisibleForBindingString = "IsVisibleForBinding";

			if (dataSource != null)
			{
				zButtonReScreen.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zButtonAcceptedFailedAsEntered.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zButtonConvertToStandAloneDeclaration.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zButtonEditStandAloneDeclaration.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zTextBoxImportCustomsClearanceStatus.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zTextBoxExportCustomsClearanceStatus.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zTextBoxImportReleaseStatus.DataBindings.RemoveBinding(IsVisibleForBindingString);
				zTextBoxExportReleaseStatus.DataBindings.RemoveBinding(IsVisibleForBindingString);
				DataBindings.RemoveBinding(nameof(IsACASTabPageVisibleForBinding));
				zAddressConsignee.BindToOrgList = GetBindingMemberString(dataSource, nameof(HVLVConsignment.Lookups) + "." + nameof(HVLVConsignment.Lookups.ConsigneeOrganisation_List));
				zAddressShipper.BindToOrgList = GetBindingMemberString(dataSource, nameof(HVLVConsignment.Lookups) + "." + nameof(HVLVConsignment.Lookups.ShipperOrganisation_List));
				zAddressControlDestinationDepot.BindToOrgList = GetBindingMemberString(dataSource, nameof(HVLVConsignment.Lookups) + "." + nameof(HVLVConsignment.Lookups.DestinationDepotOrgCollection));
				zAddressReturn.BindToOrgList = GetBindingMemberString(dataSource, nameof(HVLVConsignment.Lookups) + "." + nameof(HVLVConsignment.Lookups.ReturnOrganisation_List));
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				zButtonReScreen.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.HasUnknownPrescreeningStatus)), false, DataSourceUpdateMode.Never));
				zButtonAcceptedFailedAsEntered.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.HasFailedPreScreeningStatus)), false, DataSourceUpdateMode.Never));
				zButtonConvertToStandAloneDeclaration.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.CanConvertToStandAloneDeclaration)), false, DataSourceUpdateMode.Never));
				zButtonEditStandAloneDeclaration.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.HasStandAloneDeclarationForCurrentDirection)), false, DataSourceUpdateMode.Never));
				zTextBoxImportCustomsClearanceStatus.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.ShowImport)), false, DataSourceUpdateMode.Never));
				zTextBoxExportCustomsClearanceStatus.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.ShowExport)), false, DataSourceUpdateMode.Never));
				zTextBoxImportReleaseStatus.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.ShowImport)), false, DataSourceUpdateMode.Never));
				zTextBoxExportReleaseStatus.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.ShowExport)), false, DataSourceUpdateMode.Never));
				DataBindings.Add(new KBinding(nameof(IsACASTabPageVisibleForBinding), dataSource, GetBindingMemberString(dataSource, nameof(HVLVConsignment.RequiresACAS)), false, DataSourceUpdateMode.Never));
			}
		}

		public ZBool IsACASTabPageVisibleForBinding
		{
			get { return tabPageACAS.TabVisible; }
			set
			{
				tabPageACAS.TabVisible = value;
			}
		}

		string GetBindingMemberString(object dataSource, string dataMember)
		{
			var result = dataMember;
			if (dataSource is IHVLVConsignmentCollectionParent)
			{
				result = "ConsignmentsFilteredView." + dataMember;
			}

			return result;
		}

		void EditStandAloneDeclarationButton_Click(object sender, EventArgs e)
		{
			PopupDeclarationEditFormIfExists();
		}

		void PopupDeclarationEditFormIfExists()
		{
			var declaration = CurrentConsignment?.StandAloneDeclarationForCurrentCompany;

			if (declaration != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
				controller.ShowEditForm(declaration);
			}
		}

		void ShowStatusDetail(object sender, EventArgs e)
		{
			var consignment = CurrentConsignment;
			if (consignment != null)
			{
				if (consignment.ShowImport)
				{
					if (consignment.HVC_ImportCustomsClearanceStatus.IsEmpty)
					{
						Globals.Message.ShowInformation(StatusNotAvailableImport, StatusMessageHeader);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("9b768742-3732-46ec-9172-eb78c6aae9b0", "{0}{1}", ImportStatusHeader, consignment.ImportCustomsClearanceStatusDescription), StatusMessageHeader);
					}
				}
				else
				{
					if (consignment.HVC_ExportCustomsClearanceStatus.IsEmpty)
					{
						Globals.Message.ShowInformation(StatusNotAvailableExport, StatusMessageHeader);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("e689787b-49e2-4d22-9c14-55734f191603", "{0}{1}", ExportStatusHeader, consignment.ExportCustomsClearanceStatusDescription), StatusMessageHeader);
					}
				}
			}
		}

		static string StatusNotAvailableImport
		{
			get
			{
				return Res.GetString("6a381eae-1508-4851-95c2-3706b54653e4", "Import Customs clearance status is empty.");
			}
		}

		static string ImportStatusHeader
		{
			get
			{
				return Res.GetString("d6a7f00d-39ed-4d2c-b4a8-f37bd5072f6b", "Import Customs clearance status: ");
			}
		}

		static string StatusNotAvailableExport
		{
			get
			{
				return Res.GetString("1ea5eb45-85bc-453b-a7f0-5835237d2b2d", "Export Customs clearance status is empty.");
			}
		}

		static string ExportStatusHeader
		{
			get
			{
				return Res.GetString("4b7c90b2-4d8f-4679-bd95-0508b3b8d572", "Export Customs clearance status: ");
			}
		}

		static string StatusMessageHeader
		{
			get
			{
				return Res.GetString("ac335fdf-6ea1-4e2a-b8b1-6919079b3d88", "HVLV Consignment Information");
			}
		}

		void UpdatePreScreeningStatus(object sender, EventArgs e)
		{
			var consignment = CurrentConsignment;
			var provider = new ETailPreScreeningProvider(consignment);
			provider.Screen();
			provider.SyncScreeningResult();
			provider.AddLog();
			if (!provider.ScreeningResult.ErrorMessageDetail.IsNullOrEmpty())
			{
				Globals.Message.Show(provider.ScreeningResult.ErrorMessageDetail);
			}
		}

		void AcceptPreScreeningStatus_FailedAsEntered(object sender, EventArgs e)
		{
			var consignment = CurrentConsignment;
			if (consignment != null)
			{
				var message = ResString.GetMultilingualString("c012ef0f-f2f4-44ba-88b4-8fc491c7ed5e", "You have chosen to manually confirm that the pre-screening status is valid. Only click OK if you are certain the pre-screening status entered is valid.");
				var title = ResString.GetMultilingualString("f0b92268-cf96-40eb-9856-a7971193df73", "Confirm pre-screening status is correct");
				var failedAcceptedAsEntered = Globals.Message.Show(message, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
				if (failedAcceptedAsEntered)
				{
					consignment.AcceptFailedPreScreeningAsEntered();
				}
			}
		}

		void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			var consignment = CurrentConsignment;
			if (consignment != null && !consignment.HVC_INCOInfo.HasErrors())
			{
				ZFormModaliser.Show(new Freight.GUI.IncoTermDescriptionForm(consignment.HVC_INCO), ParentForm as ZForm);
			}
		}

		void ConvertToStandAloneDeclaration(object sender, EventArgs args)
		{
			ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclaration(CurrentConsignment);
		}

		void ConvertShipperToOrganisationButton_Click(object sender, EventArgs e)
		{
			if (CurrentConsignment != null)
			{
				HVLVSimilarAddressSelectionForm.Convert(CurrentConsignment, ConsignmentAddressType.Shipper);
			}
		}

		void ConvertConsigneeToOrganisationButton_Click(object sender, EventArgs e)
		{
			if (CurrentConsignment != null)
			{
				HVLVSimilarAddressSelectionForm.Convert(CurrentConsignment, ConsignmentAddressType.Consignee);
			}
		}

		void ConvertReturnLocationToOrganisationButton_Click(object sender, EventArgs e)
		{
			if (CurrentConsignment != null)
			{
				HVLVSimilarAddressSelectionForm.Convert(CurrentConsignment, ConsignmentAddressType.Return);
			}
		}

		internal ZTextBox VolumeWeightTextBox => zTextBoxVolumeWeight;

		HVLVConsignment CurrentConsignment => CurrentDataItem as HVLVConsignment;

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<HVLVConsignmentDetailsUserControl>()
				.Property("IsACASTabPageVisibleForBinding", ZBool.False, false)
				.Result;
		}

		#endregion
	}
}

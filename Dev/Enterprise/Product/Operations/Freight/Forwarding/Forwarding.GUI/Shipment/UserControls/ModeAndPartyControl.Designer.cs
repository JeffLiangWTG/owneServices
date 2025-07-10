using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ModeAndPartyControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LocalClientOrgControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.ACIConsigneeDestinationZoneBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ACIConsignorOriginZoneBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JS_ShipmentTypeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_TransportModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_PackingModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsignorDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JobHeaderClientCoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ACIConsignorOriginZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ACIConsigneeDestinationZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ModeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// LocalClientOrgControl
			// 
			this.BindingSource.SetBindingMember(this.LocalClientOrgControl, "ShipmentJobHeader+JH_OA_LocalChargesAddr_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress)));
			this.LocalClientOrgControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Local Client");
			this.LocalClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.LocalClientOrgControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.Name = "LocalClientOrgControl";
			this.LocalClientOrgControl.PopupCaption = "";
			this.LocalClientOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.LocalClientOrgControl.TabIndex = 5;
			// 
			// ACIConsigneeDestinationZoneBoundLabel
			// 
			this.ACIConsigneeDestinationZoneBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACIConsigneeDestinationZoneBoundLabel, "JS_Calc_ACIConsigneeDestinationZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_Calc_ACIConsigneeDestinationZone)));
			this.ACIConsigneeDestinationZoneBoundLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|beebe6b0-5af2-43c4-aa5b-88dd7cb5f290", "ACI Zone");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ACIConsigneeDestinationZoneBoundLabel, false);
			this.ACIConsigneeDestinationZoneBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 287, true);
			this.ACIConsigneeDestinationZoneBoundLabel.Name = "ACIConsigneeDestinationZoneBoundLabel";
			this.ACIConsigneeDestinationZoneBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.ACIConsigneeDestinationZoneBoundLabel.TabIndex = 4;
			// 
			// ACIConsignorOriginZoneBoundLabel
			// 
			this.ACIConsignorOriginZoneBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACIConsignorOriginZoneBoundLabel, "JS_Calc_ACIConsignorOriginZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_Calc_ACIConsignorOriginZone)));
			this.ACIConsignorOriginZoneBoundLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|af8df0a3-7643-43ee-b0fa-ac45d7e2fcf6", "ACI Zone");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ACIConsignorOriginZoneBoundLabel, false);
			this.ACIConsignorOriginZoneBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 106, true);
			this.ACIConsignorOriginZoneBoundLabel.Name = "ACIConsignorOriginZoneBoundLabel";
			this.ACIConsignorOriginZoneBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.ACIConsignorOriginZoneBoundLabel.TabIndex = 2;
			// 
			// ModeGroupBox
			// 
			this.ModeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|72f0873f-982c-4f8d-a88f-83af4aae8aae", "Mode");
			this.ModeGroupBox.Controls.Add(this.JS_ShipmentTypeBoundDropDownEdit);
			this.ModeGroupBox.Controls.Add(this.JS_TransportModeBoundDropDownEdit);
			this.ModeGroupBox.Controls.Add(this.JS_PackingModeBoundDropDownEdit);
			this.ModeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModeGroupBox.Name = "ModeGroupBox";
			this.ModeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 85, true);
			this.ModeGroupBox.TabIndex = 0;
			this.ModeGroupBox.TabStop = false;
			// 
			// JS_ShipmentTypeBoundDropDownEdit
			// 
			this.JS_ShipmentTypeBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JS_ShipmentTypeBoundDropDownEdit, "JS_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ShipmentType)));
			this.JS_ShipmentTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 57, true);
			this.JS_ShipmentTypeBoundDropDownEdit.Name = "JS_ShipmentTypeBoundDropDownEdit";
			this.JS_ShipmentTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JS_ShipmentTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.JS_ShipmentTypeBoundDropDownEdit.TabIndex = 2;
			// 
			// JS_TransportModeBoundDropDownEdit
			// 
			this.JS_TransportModeBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JS_TransportModeBoundDropDownEdit, "JS_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_TransportMode)));
			this.JS_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 15, true);
			this.JS_TransportModeBoundDropDownEdit.Name = "JS_TransportModeBoundDropDownEdit";
			this.JS_TransportModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JS_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.JS_TransportModeBoundDropDownEdit.TabIndex = 0;
			// 
			// JS_PackingModeBoundDropDownEdit
			// 
			this.JS_PackingModeBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JS_PackingModeBoundDropDownEdit, "JS_PackingMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_PackingMode)));
			this.JS_PackingModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 36, true);
			this.JS_PackingModeBoundDropDownEdit.Name = "JS_PackingModeBoundDropDownEdit";
			this.JS_PackingModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JS_PackingModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.JS_PackingModeBoundDropDownEdit.TabIndex = 1;
			// 
			// ConsignorDocumentaryDocAddressControl
			// 
			this.BindingSource.SetBindingMember(this.ConsignorDocumentaryDocAddressControl, "ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).ConsignorDocumentaryAddress)));
			this.ConsignorDocumentaryDocAddressControl.BindToOrganisations = "Lookups.ConsignorForwarder_List";
			this.ConsignorDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|e6b5efca-0d70-45ac-a3eb-1042686e1414", "Shipper");
			this.ConsignorDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.ConsignorDocumentaryDocAddressControl.Name = "ConsignorDocumentaryDocAddressControl";
			this.ConsignorDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsignorDocumentaryDocAddressControl.TabIndex = 1;
			// 
			// ConsigneeDocumentaryDocAddressControl
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeDocumentaryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocumentaryDocAddressControl.BindToOrganisations = "Lookups.ConsigneeForwarder_List";
			this.ConsigneeDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|46703631-e98e-41eb-b66c-2fc8ed67ccde", "Consignee");
			this.ConsigneeDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 270, true);
			this.ConsigneeDocumentaryDocAddressControl.Name = "ConsigneeDocumentaryDocAddressControl";
			this.ConsigneeDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeDocumentaryDocAddressControl.TabIndex = 3;
			// 
			// JobHeaderClientCoveringLabel
			// 
			this.JobHeaderClientCoveringLabel.ForeColor = System.Drawing.Color.Red;
			this.JobHeaderClientCoveringLabel.IsFontBold = true;
			this.JobHeaderClientCoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 433, true);
			this.JobHeaderClientCoveringLabel.Name = "JobHeaderClientCoveringLabel";
			this.JobHeaderClientCoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 137, true);
			this.JobHeaderClientCoveringLabel.TabIndex = 6;
			this.JobHeaderClientCoveringLabel.Text = "Job Header Mutex Error Label";
			this.JobHeaderClientCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.JobHeaderClientCoveringLabel.Visible = false;
			// 
			// ACIConsignorOriginZoneLabel
			// 
			this.ACIConsignorOriginZoneLabel.AutoSize = true;
			this.ACIConsignorOriginZoneLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|640a398c-9457-4245-954a-a74a748e9f3e", "ACI Zone:");
			this.ACIConsignorOriginZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 106, true);
			this.ACIConsignorOriginZoneLabel.Name = "ACIConsignorOriginZoneLabel";
			this.ACIConsignorOriginZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.ACIConsignorOriginZoneLabel.TabIndex = 7;
			// 
			// ACIConsigneeDestinationZoneLabel
			// 
			this.ACIConsigneeDestinationZoneLabel.AutoSize = true;
			this.ACIConsigneeDestinationZoneLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ModeAndPartyControl|f3de8c9d-b1d4-45cb-b281-711160d5bb6d", "ACI Zone:");
			this.ACIConsigneeDestinationZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 287, true);
			this.ACIConsigneeDestinationZoneLabel.Name = "ACIConsigneeDestinationZoneLabel";
			this.ACIConsigneeDestinationZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.ACIConsigneeDestinationZoneLabel.TabIndex = 8;
			// 
			// ModeAndPartyControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ACIConsigneeDestinationZoneLabel);
			this.Controls.Add(this.ACIConsignorOriginZoneLabel);
			this.Controls.Add(this.LocalClientOrgControl);
			this.Controls.Add(this.ACIConsigneeDestinationZoneBoundLabel);
			this.Controls.Add(this.ACIConsignorOriginZoneBoundLabel);
			this.Controls.Add(this.ModeGroupBox);
			this.Controls.Add(this.ConsignorDocumentaryDocAddressControl);
			this.Controls.Add(this.ConsigneeDocumentaryDocAddressControl);
			this.Controls.Add(this.JobHeaderClientCoveringLabel);
			this.Name = "ModeAndPartyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 585, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ModeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.MasterFiles.GUI.ZOrgAddressControl LocalClientOrgControl;
		private Enterprise.ZArchitecture.ZLabel ACIConsigneeDestinationZoneBoundLabel;
		private Enterprise.ZArchitecture.ZLabel ACIConsignorOriginZoneBoundLabel;
		private ZGroupBox ModeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_TransportModeBoundDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_PackingModeBoundDropDownEdit;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl ConsignorDocumentaryDocAddressControl;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl ConsigneeDocumentaryDocAddressControl;
		internal Enterprise.ZArchitecture.ZLabel JobHeaderClientCoveringLabel;
		internal Enterprise.ZArchitecture.ZLabel ACIConsignorOriginZoneLabel;
		internal Enterprise.ZArchitecture.ZLabel ACIConsigneeDestinationZoneLabel;
		private ZDropEdit JS_ShipmentTypeBoundDropDownEdit;

	}
}

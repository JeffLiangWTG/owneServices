using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class RelatedShipmentsControl
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
			this.RelatedShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelatedShipmentsGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShipmentModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ShipmentModuleButtonGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.masterConsolsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CoLoadsGridLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JS_JS_ColoadMasterShipmentFindbox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedShipmentsGroupBox.SuspendLayout();
			this.RelatedShipmentsGridPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// RelatedShipmentsGroupBox
			// 
			this.RelatedShipmentsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("RelatedShipmentsControl|6f9b32f1-a375-4303-a41c-f6ed314ffb06", "Related Shipment Details");
			this.RelatedShipmentsGroupBox.Controls.Add(this.RelatedShipmentsGridPanel);
			this.RelatedShipmentsGroupBox.Controls.Add(this.zPanel1);
			this.RelatedShipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedShipmentsGroupBox.Name = "RelatedShipmentsGroupBox";
			this.RelatedShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 577, true);
			this.RelatedShipmentsGroupBox.TabIndex = 0;
			this.RelatedShipmentsGroupBox.TabStop = false;
			// 
			// RelatedShipmentsGridPanel
			// 
			this.RelatedShipmentsGridPanel.Controls.Add(this.ShipmentModuleButtonGrid);
			this.RelatedShipmentsGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedShipmentsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 60, true);
			this.RelatedShipmentsGridPanel.Name = "RelatedShipmentsGridPanel";
			this.RelatedShipmentsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 514, true);
			this.RelatedShipmentsGridPanel.TabIndex = 2;
			// 
			// ShipmentModuleButtonGrid
			// 
			this.ShipmentModuleButtonGrid.AllowDrop = true;
			this.ShipmentModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.ShipmentModuleButtonGrid, "CoLoadShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).CoLoadShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Lookups.CoLoadShipment_List)));
			this.ShipmentModuleButtonGrid.BindToFindBoxList = "Lookups+CoLoadShipment_List";
			this.ShipmentModuleButtonGrid.GridId = "731b586f-fb33-41e9-b7eb-e193d5c53321";
			this.ShipmentModuleButtonGrid.DetachMessage = Enterprise.Freight.Forwarding.GUI.Res.GetData("edfbd9e7-b5ad-4317-bbf8-132f08c8476f", "Are you sure you want to detach the selected Shipment(s)?");
			this.ShipmentModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.ShipmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ShipmentModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ShipmentModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.ShipmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ShipmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 476, true);
			this.ShipmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ShipmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentModuleButtonGrid.Name = "ShipmentModuleButtonGrid";
			this.ShipmentModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("82AF003D-CF0C-4D26-8CC0-D137C466001B", "Shipment");
			this.ShipmentModuleButtonGrid.ReadOnly = false;
			this.ShipmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 514, true);
			this.ShipmentModuleButtonGrid.TabIndex = 2;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.masterConsolsTextBox);
			this.zPanel1.Controls.Add(this.CoLoadsGridLabel);
			this.zPanel1.Controls.Add(this.JS_JS_ColoadMasterShipmentFindbox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 44, true);
			this.zPanel1.TabIndex = 3;
			// 
			// masterConsolsTextBox
			// 
			this.masterConsolsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.masterConsolsTextBox, "CoLoadMasterShipment+JS_JK_ConsolID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).CoLoadMasterShipment.JS_JK_ConsolID)));
			this.masterConsolsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 5, true);
			this.masterConsolsTextBox.Name = "masterConsolsTextBox";
			this.masterConsolsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 20, true);
			this.masterConsolsTextBox.TabIndex = 2;
			// 
			// CoLoadsGridLabel
			// 
			this.CoLoadsGridLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CoLoadsGridLabel, "JS_Calc_CoLoadsGridLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_Calc_CoLoadsGridLabel)));
			this.CoLoadsGridLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("RelatedShipmentsControl|6cdf0129-740a-48db-9c53-97600171fb4e", "Co-Load Sub Shipments / Buyers Consol Related Shipments");
			this.CoLoadsGridLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			this.CoLoadsGridLabel.Name = "CoLoadsGridLabel";
			this.CoLoadsGridLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 13, true);
			this.CoLoadsGridLabel.TabIndex = 3;
			// 
			// JS_JS_ColoadMasterShipmentFindbox
			// 
			this.JS_JS_ColoadMasterShipmentFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_JS_ColoadMasterShipmentFindbox, "JS_JS_ColoadMasterShipmentForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_JS_ColoadMasterShipmentForBinding)));
			this.JS_JS_ColoadMasterShipmentFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 5, true);
			this.JS_JS_ColoadMasterShipmentFindbox.Name = "JS_JS_ColoadMasterShipmentFindbox";
			this.JS_JS_ColoadMasterShipmentFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 20, true);
			this.JS_JS_ColoadMasterShipmentFindbox.TabIndex = 1;
			// 
			// RelatedShipmentsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelatedShipmentsGroupBox);
			this.Name = "RelatedShipmentsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 577, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedShipmentsGroupBox.ResumeLayout(false);
			this.RelatedShipmentsGridPanel.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZGroupBox RelatedShipmentsGroupBox;
		private ZPanel RelatedShipmentsGridPanel;
		internal ShipmentModuleButtonGrid ShipmentModuleButtonGrid;
		private ZPanel zPanel1;
		internal ZGuidFindBox JS_JS_ColoadMasterShipmentFindbox;
		private Enterprise.ZArchitecture.ZLabel CoLoadsGridLabel;
		private ZArchitecture.ZTextBox masterConsolsTextBox;





	}
}

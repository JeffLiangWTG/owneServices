using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class OrderManagerRequestMappingControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.orderManagerRequestMappingGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderManagerRequestMappingGrid)).BeginInit();
            this.orderManagerRequestMappingGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Registry.OrderManagerRequestMappingCollection);
            // 
            // orderManagerRequestMappingGrid
            // 
            this.orderManagerRequestMappingGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.orderManagerRequestMappingGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.OrderManagerRequestMapping)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.OrderManagerRequestMapping)(null)).RequestDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.OrderManagerRequestMapping)(null)).RequestType)));
            this.orderManagerRequestMappingGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2b9f7580-1ed1-4c30-abf2-2426d5f37fdf", "Request");
            zTextBoxColumnStyleInfo1.ColumnName = "RequestDescription";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCodeFindBoxColumnStyleInfo1.Caption = "";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("70464e30-fe3d-4ab5-a1d0-047c5eeca282", "Request Type");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "RequestType";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ExternalRequestTypes;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.orderManagerRequestMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.orderManagerRequestMappingGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.orderManagerRequestMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderManagerRequestMappingGrid.GridId = "71d8fc0c-c4c0-46eb-b4d3-f5c14e994e86";
            this.orderManagerRequestMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.orderManagerRequestMappingGrid.LayoutKey = "orderManagerRequestMappingGrid";
            this.orderManagerRequestMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.orderManagerRequestMappingGrid.Name = "orderManagerRequestMappingGrid";
            this.orderManagerRequestMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
            this.orderManagerRequestMappingGrid.TabIndex = 0;
            // 
            // OrderManagerRequestMappingControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.orderManagerRequestMappingGrid);
            this.Name = "OrderManagerRequestMappingControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderManagerRequestMappingGrid)).EndInit();
            this.orderManagerRequestMappingGrid.ResumeLayout(false);
            this.orderManagerRequestMappingGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid orderManagerRequestMappingGrid;
	}
}

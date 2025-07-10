namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbRoutePlannerDetailsUserControlBase
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
			this.components = new System.ComponentModel.Container();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ConfirmationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConfirmationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsTabControl.SuspendLayout();
			this.ConfirmationsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfirmationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbAddressPoint);
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.ConfirmationsTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
			this.DetailsTabControl.TabIndex = 0;
			// 
			// ConfirmationsTabPage
			// 
			this.ConfirmationsTabPage.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("296de439-d34b-47be-904f-274a24938473", "Consignments");
			this.ConfirmationsTabPage.Controls.Add(this.ConfirmationsGrid);
			this.ConfirmationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConfirmationsTabPage.Name = "ConfirmationsTabPage";
			this.ConfirmationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfirmationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 123, true);
			this.ConfirmationsTabPage.TabIndex = 0;
			this.ConfirmationsTabPage.UseVisualStyleBackColor = true;
			// 
			// ConfirmationsGrid
			// 
			this.ConfirmationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConfirmationsGrid, "Confirmations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbAddressPoint)(null)).Confirmations)));
			this.ConfirmationsGrid.CaptionVisible = false;
			this.ConfirmationsGrid.CopySelectedRowsAllowed = true;
			this.ConfirmationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationsGrid.GridId = "2d22df03-00d2-413f-aea7-c3fa76e853e9";
			this.ConfirmationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfirmationsGrid.IsWholeRowSelectedOnClick = true;
			this.ConfirmationsGrid.LayoutKey = "ConsignmentsGrid";
			this.ConfirmationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConfirmationsGrid.Name = "ConfirmationsGrid";
			this.ConfirmationsGrid.ReadOnly = true;
			this.ConfirmationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 117, true);
			this.ConfirmationsGrid.TabIndex = 2;
			// 
			// DtbRoutePlannerDetailsUserControlBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsTabControl);
			this.Name = "DtbRoutePlannerDetailsUserControlBase";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsTabControl.ResumeLayout(false);
			this.ConfirmationsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ConfirmationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl DetailsTabControl;
		private ZArchitecture.GUI.ZTabPage ConfirmationsTabPage;
		protected internal ZArchitecture.ZGrid ConfirmationsGrid;
	}
}

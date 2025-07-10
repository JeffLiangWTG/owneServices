namespace Enterprise.Customs.PL.GUI
{
	partial class AdditionalTranCircumstancesForm
	{
		#region Component Designer generated code

		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.AdditionalTranCircumstanceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTranCircumstanceGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.TranCircumstanceCollection);
			// 
			// OrderItemsGrid
			// 
			this.AdditionalTranCircumstanceGrid.AllowNavigation = false;
			this.AdditionalTranCircumstanceGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.AdditionalTranCircumstanceGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.Declaration.TranCircumstance)(null)).CY_Code);
			this.AdditionalTranCircumstanceGrid.CaptionVisible = false;
			this.AdditionalTranCircumstanceGrid.GridId = "3406A193-78C5-4F1B-AB14-CA4E4EB7F8D8";
			this.AdditionalTranCircumstanceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalTranCircumstanceGrid.LayoutKey = "AdditionalTranCircumstanceGrid";
			this.AdditionalTranCircumstanceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalTranCircumstanceGrid.Name = "AdditionalTranCircumstanceGrid";
			this.AdditionalTranCircumstanceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 206, true);
			this.AdditionalTranCircumstanceGrid.TabIndex = 1;
			this.AdditionalTranCircumstanceGrid.AllowSorting = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CloseButton.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("AdditionalTranCircumstancesForm|8F5943DD-F2ED-4DDA-A87D-93FFA3B26E49", "OK");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// AdditionalTranCircumstancesForm
			// 
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("AdditionalTranCircumstancesForm|76E565F3-5A93-44D7-A6E6-75BB6FFC77B6", "Additional Transaction Circumstances");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.AdditionalTranCircumstanceGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.PL.Business";
			this.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.TranCircumstanceCollection);
			this.DataSourceTypeName = "Enterprise.Customs.PL.Business.TranCircumstanceCollection";
			this.Name = "AdditionalTranCircumstancesForm";
			this.Controls.SetChildIndex(this.AdditionalTranCircumstanceGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTranCircumstanceGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		Enterprise.ZArchitecture.ZGrid AdditionalTranCircumstanceGrid;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}

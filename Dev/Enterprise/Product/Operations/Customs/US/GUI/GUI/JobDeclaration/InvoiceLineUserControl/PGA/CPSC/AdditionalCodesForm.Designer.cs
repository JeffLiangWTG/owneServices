namespace Enterprise.Customs.US.GUI
{
	partial class AdditionalCodesForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.SealNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealNumbersSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GaveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).BeginInit();
			this.SealNumbersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersSplitContainer)).BeginInit();
			this.SealNumbersSplitContainer.Panel1.SuspendLayout();
			this.SealNumbersSplitContainer.Panel2.SuspendLayout();
			this.SealNumbersSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 262, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CommaSeparatedNumberCollection);
			// 
			// SealNumbersGrid
			// 
			this.SealNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SealNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CommaSeparatedNumberCollection)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CommaSeparatedNumber)(null)).Number)));
			this.SealNumbersGrid.CaptionVisible = false;
			this.SealNumbersGrid.CopySelectedRowsAllowed = true;
			this.SealNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumbersGrid.GridId = "7cdcdaec-ce23-4181-9b27-7057d2cadd27";
			this.SealNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealNumbersGrid.LayoutKey = "SealNumbersGrid";
			this.SealNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumbersGrid.Name = "SealNumbersGrid";
			this.SealNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 231, true);
			this.SealNumbersGrid.TabIndex = 0;
			// 
			// SealNumbersSplitContainer
			// 
			this.SealNumbersSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumbersSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumbersSplitContainer.Name = "SealNumbersSplitContainer";
			this.SealNumbersSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SealNumbersSplitContainer.Panel1
			// 
			this.SealNumbersSplitContainer.Panel1.Controls.Add(this.SealNumbersGrid);
			// 
			// SealNumbersSplitContainer.Panel2
			// 
			this.SealNumbersSplitContainer.Panel2.Controls.Add(this.GaveUpButton);
			this.SealNumbersSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.SealNumbersSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 262, true);
			this.SealNumbersSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(231);
			this.SealNumbersSplitContainer.TabIndex = 1;
			// 
			// GaveUpButton
			// 
			this.GaveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GaveUpButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a83a2591-3dc0-4cc7-9e34-ad6025357a0d", "Cancel");
			this.GaveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GaveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 3, true);
			this.GaveUpButton.Name = "GaveUpButton";
			this.GaveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GaveUpButton.TabIndex = 1;
			this.GaveUpButton.UseVisualStyleBackColor = true;
			this.GaveUpButton.Click += new System.EventHandler(this.GaveUpButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2b9f8d31-e9a6-4a3a-ac8a-9610560b6142", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AdditionalCountriesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.GaveUpButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 286, true);
			this.Controls.Add(this.SealNumbersSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CommaSeparatedNumberCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AdditionalCountriesForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SealNumbersSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).EndInit();
			this.SealNumbersGrid.ResumeLayout(false);
			this.SealNumbersGrid.PerformLayout();
			this.SealNumbersSplitContainer.Panel1.ResumeLayout(false);
			this.SealNumbersSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersSplitContainer)).EndInit();
			this.SealNumbersSplitContainer.ResumeLayout(false);
			this.SealNumbersSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SealNumbersGrid;
		private CargoWise.Windows.UI.KSplitContainer SealNumbersSplitContainer;
		private ZArchitecture.GUI.ZButton GaveUpButton;
		internal ZArchitecture.GUI.ZButton OKButton;
	}
}
using System.Windows.Forms;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class ChargesItemTemplate
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
		private void InitializeComponent()
		{
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblIncluded = new Enterprise.ZArchitecture.ZLabel();
            this.lblLocalCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblAmount = new Enterprise.ZArchitecture.ZLabel();
            this.tbCharge = new Enterprise.Rating.GUI.RateSelector.UIControls.ChargeToggleButton();
            this.tbvLocalAmount = new Enterprise.Rating.GUI.RateSelector.UIControls.TextWithValidation();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.tbCharge.SuspendLayout();
            this.tbvLocalAmount.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.ColumnCount = 5;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.20202F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.13131F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.pnlMain.Controls.Add(this.lblIncluded, 4, 0);
            this.pnlMain.Controls.Add(this.lblLocalCurrency, 4, 0);
            this.pnlMain.Controls.Add(this.lblCurrency, 2, 0);
            this.pnlMain.Controls.Add(this.lblAmount, 1, 0);
            this.pnlMain.Controls.Add(this.tbCharge, 0, 0);
            this.pnlMain.Controls.Add(this.tbvLocalAmount, 3, 0);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 1;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
            this.pnlMain.TabIndex = 0;
            // 
            // lblIncluded
            // 
            this.lblIncluded.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblIncluded, "IncludedText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel)(null)).IncludedText)));
            this.lblIncluded.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIncluded.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblIncluded.IsFontBold = true;
            this.lblIncluded.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
            this.lblIncluded.Name = "lblIncluded";
            this.lblIncluded.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
            this.lblIncluded.TabIndex = 7;
            this.lblIncluded.Text = "IncludedText";
            this.lblIncluded.Visible = false;
            // 
            // lblLocalCurrency
            // 
            this.lblLocalCurrency.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblLocalCurrency, "LocalCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel)(null)).LocalCurrency)));
            this.lblLocalCurrency.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLocalCurrency.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblLocalCurrency.IsFontBold = true;
            this.lblLocalCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.lblLocalCurrency.Name = "lblLocalCurrency";
            this.lblLocalCurrency.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 0, 0, true);
            this.lblLocalCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
            this.lblLocalCurrency.TabIndex = 5;
            this.lblLocalCurrency.Text = "AUD";
            // 
            // lblCurrency
            // 
            this.lblCurrency.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblCurrency, "Currency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel)(null)).Currency)));
            this.lblCurrency.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCurrency.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCurrency.IsFontBold = true;
            this.lblCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 0, true);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
            this.lblCurrency.TabIndex = 4;
            this.lblCurrency.Text = "AUD";
            // 
            // lblAmount
            // 
            this.lblAmount.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblAmount, "Amount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel)(null)).Amount)));
            this.lblAmount.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAmount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblAmount.IsFontBold = true;
            this.lblAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 1, 0, true);
            this.lblAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
            this.lblAmount.TabIndex = 3;
            this.lblAmount.Text = "0";
            this.lblAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbCharge
            // 
            this.tbCharge.AllowDrop = true;
            this.tbCharge.AutoSize = true;
            this.tbCharge.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.tbCharge, ".");
            this.tbCharge.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbCharge.IsSelected = false;
            this.tbCharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.tbCharge.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.tbCharge.Name = "tbCharge";
            this.tbCharge.SelectionChanged = null;
            this.tbCharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
            this.tbCharge.TabIndex = 0;
            // 
            // tbvLocalAmount
            // 
            this.tbvLocalAmount.AllowDrop = true;
            this.tbvLocalAmount.AutoSize = true;
            this.tbvLocalAmount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tbvLocalAmount.Dock = System.Windows.Forms.DockStyle.Right;
            this.tbvLocalAmount.Error = null;
            this.tbvLocalAmount.ErrorLevel = Enterprise.Rating.GUI.RateSelector.Models.ErrorLevel.None;
            this.tbvLocalAmount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.tbvLocalAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 0, true);
            this.tbvLocalAmount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.tbvLocalAmount.Name = "tbvLocalAmount";
            this.tbvLocalAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
            this.tbvLocalAmount.TabIndex = 6;
            // 
            // ChargesItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.pnlMain);
            this.Name = "ChargesItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.tbCharge.ResumeLayout(true);
            this.tbCharge.PerformLayout();
            this.tbvLocalAmount.ResumeLayout(true);
            this.tbvLocalAmount.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private TableLayoutPanel pnlMain;
		private ChargeToggleButton tbCharge;
		private ZArchitecture.ZLabel lblAmount;
		private ZArchitecture.ZLabel lblCurrency;
		private ZArchitecture.ZLabel lblLocalCurrency;
		private TextWithValidation tbvLocalAmount;
		private ZArchitecture.ZLabel lblIncluded;
	}
}

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	partial class PageNavigator
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
            this.btnFirstPage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnPreviousPage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnNextPage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnLastPage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.lblPageSize = new Enterprise.ZArchitecture.ZLabel();
            this.lblPageNumber = new Enterprise.ZArchitecture.ZLabel();
            this.cbPageSize = new CargoWise.Windows.UI.KComboBox();
            this.cbPageNumber = new CargoWise.Windows.UI.KComboBox();
            this.lblGotoPage = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(System.Collections.IEnumerable);
            // 
            // btnFirstPage
            // 
            this.btnFirstPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFirstPage.ForeColor = System.Drawing.Color.Black;
            this.btnFirstPage.IsCaptionOverridden = true;
            this.btnFirstPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
            this.btnFirstPage.Name = "btnFirstPage";
            this.btnFirstPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 23, true);
            this.btnFirstPage.TabIndex = 0;
            this.btnFirstPage.Text = "|<";
            this.btnFirstPage.ToolTipCaption = null;
            this.btnFirstPage.UseVisualStyleBackColor = true;
            this.btnFirstPage.Click += new System.EventHandler(this.btnFirstPage_Click);
            // 
            // btnPreviousPage
            // 
            this.btnPreviousPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreviousPage.ForeColor = System.Drawing.Color.Black;
            this.btnPreviousPage.IsCaptionOverridden = true;
            this.btnPreviousPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 2, true);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 23, true);
            this.btnPreviousPage.TabIndex = 1;
            this.btnPreviousPage.Text = "<";
            this.btnPreviousPage.ToolTipCaption = null;
            this.btnPreviousPage.UseVisualStyleBackColor = true;
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNextPage.ForeColor = System.Drawing.Color.Black;
            this.btnNextPage.IsCaptionOverridden = true;
            this.btnNextPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 2, true);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 23, true);
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.Text = ">";
            this.btnNextPage.ToolTipCaption = null;
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // btnLastPage
            // 
            this.btnLastPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLastPage.ForeColor = System.Drawing.Color.Black;
            this.btnLastPage.IsCaptionOverridden = true;
            this.btnLastPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 2, true);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 23, true);
            this.btnLastPage.TabIndex = 3;
            this.btnLastPage.Text = ">|";
            this.btnLastPage.ToolTipCaption = null;
            this.btnLastPage.UseVisualStyleBackColor = true;
            this.btnLastPage.Click += new System.EventHandler(this.btnLastPage_Click);
            // 
            // lblPageSize
            // 
            this.lblPageSize.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PageNavigator|2a8090c9-81be-4288-9039-308be537bdee", "Page Size");
            this.lblPageSize.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblPageSize.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 2, true);
            this.lblPageSize.Name = "lblPageSize";
            this.lblPageSize.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
            this.lblPageSize.TabIndex = 4;
            this.lblPageSize.UseMnemonic = false;
            // 
            // lblPageNumber
            // 
            this.lblPageNumber.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblPageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 2, true);
            this.lblPageNumber.Name = "lblPageNumber";
            this.lblPageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 23, true);
            this.lblPageNumber.TabIndex = 5;
            this.lblPageNumber.Text = "Page 1 of 1";
            this.lblPageNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPageNumber.UseMnemonic = false;
            // 
            // cbPageSize
            // 
            this.cbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPageSize.FormattingEnabled = true;
            this.cbPageSize.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 3, true);
            this.cbPageSize.Name = "cbPageSize";
            this.cbPageSize.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 21, true);
            this.cbPageSize.TabIndex = 5;
            this.cbPageSize.SelectedIndexChanged += new System.EventHandler(this.cbPageSize_SelectedIndexChanged);
            // 
            // cbPageNumber
            // 
            this.cbPageNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPageNumber.FormattingEnabled = true;
            this.cbPageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 3, true);
            this.cbPageNumber.Name = "cbPageNumber";
            this.cbPageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 21, true);
            this.cbPageNumber.TabIndex = 4;
            this.cbPageNumber.SelectedIndexChanged += new System.EventHandler(this.cbPageNumber_SelectedIndexChanged);
            // 
            // lblGotoPage
            // 
            this.lblGotoPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PageNavigator|FB715ACC-0BAC-4B40-B024-89223F3D95F3", "Go to Page");
            this.lblGotoPage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblGotoPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 2, true);
            this.lblGotoPage.Name = "lblGotoPage";
            this.lblGotoPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
            this.lblGotoPage.TabIndex = 7;
            this.lblGotoPage.UseMnemonic = false;
            // 
            // PageNavigator
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.cbPageNumber);
            this.Controls.Add(this.lblGotoPage);
            this.Controls.Add(this.cbPageSize);
            this.Controls.Add(this.lblPageNumber);
            this.Controls.Add(this.lblPageSize);
            this.Controls.Add(this.btnLastPage);
            this.Controls.Add(this.btnNextPage);
            this.Controls.Add(this.btnPreviousPage);
            this.Controls.Add(this.btnFirstPage);
            this.Name = "PageNavigator";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 28, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton btnFirstPage;
		private ZArchitecture.GUI.ZButton btnPreviousPage;
		private ZArchitecture.GUI.ZButton btnNextPage;
		private ZArchitecture.GUI.ZButton btnLastPage;
		private ZArchitecture.ZLabel lblPageSize;
		private ZArchitecture.ZLabel lblPageNumber;
		private CargoWise.Windows.UI.KComboBox cbPageSize;
		private CargoWise.Windows.UI.KComboBox cbPageNumber;
		private ZArchitecture.ZLabel lblGotoPage;
	}
}

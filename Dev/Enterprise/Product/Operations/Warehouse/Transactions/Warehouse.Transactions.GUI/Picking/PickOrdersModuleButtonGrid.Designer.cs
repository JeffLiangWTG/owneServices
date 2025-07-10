using System;
using System.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickOrdersModuleButtonGrid
	{
		ZButton CancelPickButton;
		ZButton ReleaseButton;

		public ZButton AutoPickButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.AutoPickButton = new ZButton();
			this.CancelPickButton = new ZButton();
			this.ReleaseButton = new ZButton();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPick);
			// 
			// AutoPickButton
			// 
			this.AutoPickButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AutoPickButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|00425afa-479e-482b-9ccf-524a9ed285e4", "Auto Pick");
			this.AutoPickButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 329, true);
			this.AutoPickButton.Name = "AutoPickButton";
			this.AutoPickButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.AutoPickButton.TabIndex = 5;
			this.AutoPickButton.UseVisualStyleBackColor = true;
			this.AutoPickButton.Click += new EventHandler(this.AutoPickButton_Click);
			// 
			// CancelPickButton
			// 
			this.CancelPickButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CancelPickButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|0c3c05ee-5b51-4339-a799-ad159b43449d", "Cancel Pick");
			this.CancelPickButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 329, true);
			this.CancelPickButton.Name = "CancelPickButton";
			this.CancelPickButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.CancelPickButton.TabIndex = 6;
			this.CancelPickButton.UseVisualStyleBackColor = true;
			this.CancelPickButton.Click += new EventHandler(this.CancelPickButton_Click);
			// 
			// ReleaseButton
			// 
			this.ReleaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ReleaseButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickOrdersModuleButtonGrid|75a9a343-3233-46d0-9236-36fe46ee319a", "Release");
			this.ReleaseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 329, true);
			this.ReleaseButton.Name = "ReleaseButton";
			this.ReleaseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.ReleaseButton.TabIndex = 7;
			this.ReleaseButton.UseVisualStyleBackColor = true;
			this.ReleaseButton.Click += new EventHandler(this.ReleaseButton_Click);
			// 
			// PickOrdersModuleButtonGrid
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CancelPickButton);
			this.Controls.Add(this.AutoPickButton);
			this.Controls.Add(this.ReleaseButton);
			this.Name = "PickOrdersModuleButtonGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 357, true);
			this.Controls.SetChildIndex(this.AutoPickButton, 0);
			this.Controls.SetChildIndex(this.CancelPickButton, 0);
			this.Controls.SetChildIndex(this.ReleaseButton, 0);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

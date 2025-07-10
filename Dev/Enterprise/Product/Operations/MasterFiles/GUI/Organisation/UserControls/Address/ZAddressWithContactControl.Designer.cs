namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Address
{
	partial class ZAddressWithContactControl
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
			this.InvoiceContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ContactsLink
			// 
			this.ContactsLink.TabStop = true;
			// 
			// AddressesLink
			// 
			this.AddressesLink.TabStop = true;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Controls.Add(this.InvoiceContactTabPage);
			this.DetailsTabControl.Controls.SetChildIndex(this.InvoiceContactTabPage, 0);
			this.DetailsTabControl.Controls.SetChildIndex(this.ContactInfoTab, 0);
			this.DetailsTabControl.Controls.SetChildIndex(this.AddressTab, 0);
			// 
			// InvoiceContactTabPage
			// 
			this.InvoiceContactTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.InvoiceContactTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZAddressWithContactControl|800a3c83-62ca-484f-aa69-07c616e8f14e", "Invoice Contact");
			this.InvoiceContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceContactTabPage.Name = "InvoiceContactTabPage";
			this.InvoiceContactTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InvoiceContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 86, true);
			this.InvoiceContactTabPage.TabIndex = 2;
			// 
			// ZAddressWithContactControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ZAddressWithContactControl";
			this.GroupBox.ResumeLayout(false);
			this.DetailsTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage InvoiceContactTabPage;
	}
}

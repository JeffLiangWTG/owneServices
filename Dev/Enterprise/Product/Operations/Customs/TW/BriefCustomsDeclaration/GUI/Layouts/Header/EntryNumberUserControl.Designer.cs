namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class EntryNumberUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.ModifyEntryNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllocateEntryNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader);
			// 
			// ModifyEntryNumberButton
			// 
			this.ModifyEntryNumberButton.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("502252A3-EF2C-4070-B3AD-F98501402F28", "Modify");
			this.ModifyEntryNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 0, true);
			this.ModifyEntryNumberButton.Name = "ModifyEntryNumberButton";
			this.ModifyEntryNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.ModifyEntryNumberButton.TabIndex = 9;
			this.ModifyEntryNumberButton.ToolTipCaption = null;
			this.ModifyEntryNumberButton.UseVisualStyleBackColor = true;
			this.ModifyEntryNumberButton.Click += new System.EventHandler(this.ModifyEntryNumberButton_Click);
			// 
			// AllocateEntryNumberButton
			// 
			this.AllocateEntryNumberButton.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("61D76438-F1BA-4024-A39F-9B0125A75F80", "Allocate");
			this.AllocateEntryNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 0, true);
			this.AllocateEntryNumberButton.Name = "AllocateEntryNumberButton";
			this.AllocateEntryNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 21, true);
			this.AllocateEntryNumberButton.TabIndex = 8;
			this.AllocateEntryNumberButton.ToolTipCaption = null;
			this.AllocateEntryNumberButton.UseVisualStyleBackColor = true;
			this.AllocateEntryNumberButton.Click += new System.EventHandler(this.AllocateEntryNumberButton_Click);
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "DeclarationNumberDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).DeclarationNumberDisplay)));
			this.EntryNumberTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.ReadOnly = true;
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.EntryNumberTextBox.TabIndex = 7;
			// 
			// EntryNumberUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ModifyEntryNumberButton);
			this.Controls.Add(this.AllocateEntryNumberButton);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Name = "EntryNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton ModifyEntryNumberButton;
		internal ZArchitecture.GUI.ZButton AllocateEntryNumberButton;
		private ZArchitecture.ZTextBox EntryNumberTextBox;
	}
}

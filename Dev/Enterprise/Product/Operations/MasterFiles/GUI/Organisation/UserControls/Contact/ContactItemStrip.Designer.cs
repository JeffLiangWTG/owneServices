namespace Enterprise.MasterFiles.GUI
{
	partial class ContactItemStrip
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
			this.deleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionDropDownList = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ContactItemProxy);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteButton.BackColor = System.Drawing.Color.Transparent;
			this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.deleteButton.FlatAppearance.BorderSize = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.deleteButton, false);
			this.deleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 3, true);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.deleteButton.TabIndex = 3;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// DescriptionDropDownList
			// 
			this.DescriptionDropDownList.AllowDrop = true;
			this.DescriptionDropDownList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DescriptionDropDownList, "DisplayDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ContactItemProxy)(null)).DisplayDescription)));
			this.DescriptionDropDownList.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DescriptionDropDownList, false);
			this.DescriptionDropDownList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.DescriptionDropDownList.Name = "DescriptionDropDownList";
			this.DescriptionDropDownList.PreBoundMaxLength = 4;
			this.DescriptionDropDownList.ShowDescriptionBox = false;
			this.DescriptionDropDownList.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DescriptionDropDownList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DescriptionDropDownList.TabIndex = 0;
			// 
			// ContactItemStrip
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionDropDownList);
			this.Controls.Add(this.deleteButton);
			this.Name = "ContactItemStrip";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZButton deleteButton;
		protected internal ZArchitecture.GUI.ZDropEditWithFixedWidth DescriptionDropDownList;
	}
}

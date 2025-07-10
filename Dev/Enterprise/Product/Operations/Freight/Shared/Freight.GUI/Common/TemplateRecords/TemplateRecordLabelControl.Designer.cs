
namespace Enterprise.Freight.GUI.Common.TemplateRecords
{
	partial class TemplateRecordLabelControl
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
			this.templateRecordLabel = new Enterprise.ZArchitecture.ZLabel();
			this.templateNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.StmTemplateRecord);
			// 
			// templateRecordLabel
			// 
			this.templateRecordLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.templateRecordLabel.AutoSize = true;
			this.templateRecordLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("f6402e21-8ecb-4ece-9e22-8dd7dd68f614", "Template Record");
			this.templateRecordLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.templateRecordLabel.IsFontBold = true;
			this.templateRecordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 4, true);
			this.templateRecordLabel.Name = "templateRecordLabel";
			this.templateRecordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 13, true);
			this.templateRecordLabel.TabIndex = 0;
			this.templateRecordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// templateNameTextBox
			// 
			this.templateNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.templateNameTextBox, "STR_TemplateName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmTemplateRecord)(null)).STR_TemplateName)));
			this.templateNameTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("91391ce0-3943-4c5c-8386-fd96412d8eff", "Template Name", "The name of the template record.");
			this.templateNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.templateNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1289, 0, true);
			this.templateNameTextBox.Name = "templateNameTextBox";
			this.templateNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.templateNameTextBox.TabIndex = 1;
			// 
			// TemplateRecordLabelControl
			// 
			this.BackColor = System.Drawing.Color.Yellow;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.templateNameTextBox);
			this.Controls.Add(this.templateRecordLabel);
			this.Name = "TemplateRecordLabelControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1469, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel templateRecordLabel;
		private Enterprise.ZArchitecture.ZTextBox templateNameTextBox;
	}
}

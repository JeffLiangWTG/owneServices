using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.GUI
{
	public partial class FrontPageUserControl
	{
		private void InitializeComponent()
		{
			this.ExportDeclarationNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportDeclarationNumberBoundTextBox, "DeclarationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DeclarationNumber)));
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.ExportDeclarationNumberBoundTextBox.Name = "ExportDeclarationNumberBoundTextBox";
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.ExportDeclarationNumberBoundTextBox.TabIndex = 1;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FrontPageUserControl|137eaff8-bfd4-4c55-b087-09f62348f811", "Declaration Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.ExportDeclarationNumberBoundTextBox);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			this.DeclarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 43, true);
			this.DeclarationDetailsGroupBox.TabIndex = 0;
			this.DeclarationDetailsGroupBox.TabStop = false;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "JE_EntryStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_EntryStatusDescription)));
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FrontPageUserControl|f4f509d5-6346-4cf5-9a67-ab7e791adc6e", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 16, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
			this.StatusTextBox.TabIndex = 3;
			// 
			// FrontPageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclarationDetailsGroupBox);
			this.Name = "FrontPageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 494, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}

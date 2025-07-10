using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.GUI
{
	public partial class CusClassificationUserControl
	{
		void InitializeComponent()
		{
			this.cC_TariffNumBoundFindBox = new Common.GUI.TariffFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.cC_TariffNumBoundFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 141, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cC_TariffNumBoundFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			// 
			// LookupCodeTextBox
			// 
			this.LookupCodeTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 17, true);
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 20, true);
			this.LookupCodeTextBox.TabIndex = 1;
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 104, true);
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CC_IsActiveCheckBox.TabIndex = 10;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 66, true);
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 32, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// LastAuditDateEdit
			// 
			this.LastAuditDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 102, true);
			this.LastAuditDateEdit.TabIndex = 9;
			// 
			// AuditStaffCodeFindBox
			// 
			this.AuditStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 102, true);
			this.AuditStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.AuditStaffCodeFindBox.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusClassification);
			// 
			// CC_TariffNumBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.cC_TariffNumBoundFindBox, "CC_FormattedTariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusClassification)(null)).CC_FormattedTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CusClassification)(null)).Lookups.Tariffs)));
			this.cC_TariffNumBoundFindBox.BindToList = "Lookups+Tariffs";
			this.cC_TariffNumBoundFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CusClassificationUserControl|747f9aa1-cdfb-4fc3-8c7d-94f53572bc43", "HTS");
			this.cC_TariffNumBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 41, true);
			this.cC_TariffNumBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.cC_TariffNumBoundFindBox.Name = "CC_TariffNumBoundFindBox";
			this.cC_TariffNumBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 21, true);
			this.cC_TariffNumBoundFindBox.TabIndex = 3;
			// 
			// CusClassificationUserControl
			// 
			this.Name = "CusClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 141, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		protected Common.GUI.TariffFindBox cC_TariffNumBoundFindBox;
	}
}

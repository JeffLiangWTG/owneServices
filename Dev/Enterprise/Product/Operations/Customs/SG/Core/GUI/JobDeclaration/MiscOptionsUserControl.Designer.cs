using System;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		ZArchitecture.ZTextBox zTextBox3;
		ZArchitecture.ZTextBox zTextBox2;
		ZArchitecture.ZTextBox zTextBox1;
		BaseRelatedDeclarationsUserControl relatedDeclarationsUserControl2;

		void InitializeComponent()
		{
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.zTextBox2 = new ZArchitecture.ZTextBox();
			this.zTextBox3 = new ZArchitecture.ZTextBox();
			this.relatedDeclarationsUserControl2 = new BaseRelatedDeclarationsUserControl();
			this.MiscOptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Visible = false;
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.zTextBox3);
			this.MiscOptionsGroupBox.Controls.Add(this.zTextBox2);
			this.MiscOptionsGroupBox.Controls.Add(this.zTextBox1);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 149, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.zTextBox1, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.zTextBox2, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.zTextBox3, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 50, true);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "SG_AdditionalRecipientID1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobDeclaration)(null)).SG_AdditionalRecipientID1)));
			this.zTextBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("MiscOptionsUserControl|749f304d-71a4-4c1d-af35-904893d58892", "Additional Rcpt ID");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 74, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.zTextBox1.TabIndex = 9;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "SG_AdditionalRecipientID2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobDeclaration)(null)).SG_AdditionalRecipientID2)));
			this.zTextBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("MiscOptionsUserControl|cf58c928-8ad8-46f9-89d3-f2346237bc56", "Additional Rcpt ID");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.zTextBox2.TabIndex = 11;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "SG_AdditionalRecipientID3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobDeclaration)(null)).SG_AdditionalRecipientID3)));
			this.zTextBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("MiscOptionsUserControl|a28e397b-944b-4a40-a115-abe8f7532cb7", "Additional Rcpt ID");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 120, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.zTextBox3.TabIndex = 13;
			// 
			// relatedDeclarationsUserControl2
			// 
			this.relatedDeclarationsUserControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.relatedDeclarationsUserControl2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Customs.Business.BaseJobDeclaration)(((Business.JobDeclaration)(null)))));
			this.relatedDeclarationsUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 163, true);
			this.relatedDeclarationsUserControl2.Name = "relatedDeclarationsUserControl2";
			this.relatedDeclarationsUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 309, true);
			this.relatedDeclarationsUserControl2.TabIndex = 1;
			// 
			// MiscOptionsUserControl
			// 
			this.Controls.Add(this.relatedDeclarationsUserControl2);
			this.Name = "MiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 475, true);
			this.Load += new EventHandler(this.MiscOptionsUserControl_Load);
			this.Controls.SetChildIndex(this.relatedDeclarationsUserControl2, 0);
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}


namespace Enterprise.Customs.ZA.GUI
{
	public partial class MiscOptionsUserControl
	{


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.BOESightGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.BOESightNumberTextBox = new ZArchitecture.ZTextBox();
			this.BOESightDateDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.agentControl = new MasterFiles.GUI.ZOrganisationControl();
			this.vATClaimBackIndicatorDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.PaidByDropEdit.SuspendLayout();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BOESightGroupBox.SuspendLayout();
			this.BOESightDateDateEdit.SuspendLayout();
			this.agentControl.SuspendLayout();
			this.vATClaimBackIndicatorDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "");
			this.PaymentPartyDropEdit.BindToList = "";
			this.PaymentPartyDropEdit.Enabled = false;
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 97, true);
			this.PaymentPartyDropEdit.TabIndex = 4;
			this.PaymentPartyDropEdit.Visible = false;
			// 
			// PaidByDropEdit
			// 
			this.PaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 121, true);
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.vATClaimBackIndicatorDropEdit);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 152, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.vATClaimBackIndicatorDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaidByDropEdit, 0);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 25, true);
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 73, true);
			this.MergeByDropEdit.TabIndex = 3;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 49, true);
			this.BrokerCodeFindBox.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// BOESightGroupBox
			// 
			this.BOESightGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("MiscOptionsUserControl|c15152f2-4dc6-46c3-8f69-3bd594734826", "BOE Sight");
			this.BOESightGroupBox.Controls.Add(this.BOESightNumberTextBox);
			this.BOESightGroupBox.Controls.Add(this.BOESightDateDateEdit);
			this.BOESightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 165, true);
			this.BOESightGroupBox.Name = "BOESightGroupBox";
			this.BOESightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 100, true);
			this.BOESightGroupBox.TabIndex = 3;
			this.BOESightGroupBox.TabStop = false;
			// 
			// BOESightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BOESightNumberTextBox, nameof(ZA.Business.JobDeclaration.JE_BOESightNumber));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.JobDeclaration)(null)).JE_BOESightNumber)));
			this.BOESightNumberTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("MiscOptionsUserControl|5313a5da-b82f-4879-b495-ed153b88b80d", "BOE Sight Number");
			this.BOESightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 16, true);
			this.BOESightNumberTextBox.Name = "BOESightNumberTextBox";
			this.BOESightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.BOESightNumberTextBox.TabIndex = 0;
			// 
			// BOESightDateDateEdit
			// 
			this.BOESightDateDateEdit.AllowDrop = true;
			this.BOESightDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BOESightDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BOESightDateDateEdit, nameof(ZA.Business.JobDeclaration.JE_BOESightDate));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobDeclaration)(null)).JE_BOESightDate)));
			this.BOESightDateDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("MiscOptionsUserControl|9ed9cd31-01a6-46e6-b88f-f48d67282f2e", "BOE Sight Date");
			this.BOESightDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
			this.BOESightDateDateEdit.Name = "BOESightDateDateEdit";
			this.BOESightDateDateEdit.TabIndex = 1;
			// 
			// AgentControl
			// 
			this.agentControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.agentControl, nameof(ZA.Business.JobDeclaration.JE_OH_AgentOverride));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.JobDeclaration)(null)).JE_OH_AgentOverride)));
			this.agentControl.BindToOrganisations = "Lookups+Organisations";
			this.agentControl.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("313da6fa-7a1f-4666-80e3-38bf38d47384", "Agent");
			this.agentControl.Captions = new string[] {
		"Agent" };
			this.agentControl.IsCaptionOverridden = true;
			this.agentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 7, true);
			this.agentControl.Name = "AgentControl";
			this.agentControl.PopupCaption = "";
			this.agentControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.agentControl.TabIndex = 1;
			// 
			// VATClaimBackIndicatorDropEdit
			// 
			this.vATClaimBackIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vATClaimBackIndicatorDropEdit, nameof(ZA.Business.JobDeclaration.JE_VATClaimBackIndicator));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobDeclaration)(null)).JE_VATClaimBackIndicator)));
			this.vATClaimBackIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("24243f99-658d-4519-a939-09a95addb1c6", "VAT Indicator");
			this.vATClaimBackIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 97, true);
			this.vATClaimBackIndicatorDropEdit.Name = "VATClaimBackIndicatorDropEdit";
			this.vATClaimBackIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.vATClaimBackIndicatorDropEdit.TabIndex = 5;
			// 
			// MiscOptionsUserControl
			// 
			this.Controls.Add(this.agentControl);
			this.Controls.Add(this.BOESightGroupBox);
			this.Name = "MiscOptionsUserControl";
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.BOESightGroupBox, 0);
			this.Controls.SetChildIndex(this.agentControl, 0);
			this.PaidByDropEdit.ResumeLayout(true);
			this.PaidByDropEdit.PerformLayout();
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BOESightGroupBox.ResumeLayout(false);
			this.BOESightGroupBox.PerformLayout();
			this.BOESightDateDateEdit.ResumeLayout(true);
			this.BOESightDateDateEdit.PerformLayout();
			this.agentControl.ResumeLayout(true);
			this.agentControl.PerformLayout();
			this.vATClaimBackIndicatorDropEdit.ResumeLayout(true);
			this.vATClaimBackIndicatorDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
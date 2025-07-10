using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsMiscOptionUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mPIAccountDetailsUserControl = new MPIAccountDetailsUserControl();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mPIAccountDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.BindToList = "Lookups+PaymentPartyList";
			this.PaymentPartyDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsMiscOptionUserControl|ac39fcf9-49c1-4d9f-9db2-ce72b65976e9", "Payment Method:", "The method of payment of duty and charges for this declaration.");
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 157, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// MPIAccountDetailsUserControl
			// 
			this.mPIAccountDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mPIAccountDetailsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.mPIAccountDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 171, true);
			this.mPIAccountDetailsUserControl.Name = "MPIAccountDetailsUserControl";
			this.mPIAccountDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 181, true);
			this.mPIAccountDetailsUserControl.TabIndex = 1;
			// 
			// CustomsMiscOptionUserControl
			// 
			this.Controls.Add(this.mPIAccountDetailsUserControl);
			this.Name = "CustomsMiscOptionUserControl";
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.mPIAccountDetailsUserControl, 0);
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
			this.mPIAccountDetailsUserControl.ResumeLayout(true);
			this.mPIAccountDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}

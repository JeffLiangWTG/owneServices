namespace Enterprise.Customs.GUI.CommercialInvoice
{
	partial class IncotermAndIncotermPlaceUserControl
	{

		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.JZ_IncoTermBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JZ_IncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.JZ_IncoTermBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_IncoTermBoundDropDownEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).Lookups.JZ_IncoTerm_List)));
			this.JZ_IncoTermBoundDropDownEdit.BindToList = "Lookups.JZ_IncoTerm_List";
			this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JZ_IncoTermBoundDropDownEdit.Name = "JZ_IncoTermBoundDropDownEdit";
			this.JZ_IncoTermBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JZ_IncoTermBoundDropDownEdit.ShowDescriptionBox = false;
			this.JZ_IncoTermBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 25, true);
			this.JZ_IncoTermBoundDropDownEdit.TabIndex = 0;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.IsCaptionOverridden = true;
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 1, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 16, true);
			this.IncoTermExplainButton.TabIndex = 1;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.ToolTipCaption = null;
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// JZ_IncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_IncoTermPlaceTextBox, "JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_IncoTermPlace)));
			this.JZ_IncoTermPlaceTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6766632B-30BB-40F0-B13D-400AA0CADD46", "Agreed Place", "Place where the seller will deliver the goods to the carrier or another person nominated, based on an agreement between the parties.");
			this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 0, true);
			this.JZ_IncoTermPlaceTextBox.Name = "JZ_IncoTermPlaceTextBox";
			this.JZ_IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 25, true);
			this.JZ_IncoTermPlaceTextBox.TabIndex = 2;
			// 
			// IncotermAndIncotermPlaceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermExplainButton);
			this.Controls.Add(this.JZ_IncoTermBoundDropDownEdit);
			this.Controls.Add(this.JZ_IncoTermPlaceTextBox);
			this.Name = "IncotermAndIncotermPlaceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		internal Enterprise.ZArchitecture.GUI.ZDropEdit JZ_IncoTermBoundDropDownEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton IncoTermExplainButton;
		internal ZArchitecture.ZTextBox JZ_IncoTermPlaceTextBox;

		#endregion
	}
}

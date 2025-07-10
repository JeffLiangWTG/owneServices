using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CommercialInvoiceDetailsIncoTermsUserControl
	{
		private void InitializeComponent()
		{
			this.IncoTermBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncoTermBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// IncoTermBoundDropEdit
			// 
			this.IncoTermBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermBoundDropEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_IncoTerm)));
			this.IncoTermBoundDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IncoTermBoundDropEdit, false);
			this.IncoTermBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncoTermBoundDropEdit.Name = "IncoTermDropEdit";
			this.IncoTermBoundDropEdit.PreBoundMaxLength = 3;
			this.IncoTermBoundDropEdit.ShowDescriptionBox = false;
			this.IncoTermBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IncoTermBoundDropEdit.TabIndex = 4;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.IsCaptionOverridden = true;
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 1, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.IncoTermExplainButton.TabIndex = 5;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.ToolTipCaption = null;
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// CommercialInvoiceDetailsIncoTermsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermBoundDropEdit);
			this.Controls.Add(this.IncoTermExplainButton);
			this.Name = "CommercialInvoiceDetailsIncoTermsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncoTermBoundDropEdit.ResumeLayout(true);
			this.IncoTermBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZDropEdit IncoTermBoundDropEdit;
		public ZButton IncoTermExplainButton;
	}
}

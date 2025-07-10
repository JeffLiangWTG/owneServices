using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageCustomFieldsControl
	{
		IContainer components = new System.ComponentModel.Container();
		ZGroupBox zGroupBox1;
		protected ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl1;

		void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.processTemplateCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartage);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageCustomFieldsControl|e7244f79-750f-4ba7-b93a-d407ea6a3cb3", "Workflow Custom Fields");
			this.zGroupBox1.Controls.Add(this.processTemplateCustomFieldsControl1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 461, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// processTemplateCustomFieldsControl1
			// 
			this.processTemplateCustomFieldsControl1.AllowDrop = true;
			this.processTemplateCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.processTemplateCustomFieldsControl1.Name = "processTemplateCustomFieldsControl1";
			this.processTemplateCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup custom fields in Workflow Manager.";
			this.processTemplateCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 442, true);
			this.processTemplateCustomFieldsControl1.TabIndex = 0;
			// 
			// CartageCustomFieldsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "CartageCustomFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 461, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}

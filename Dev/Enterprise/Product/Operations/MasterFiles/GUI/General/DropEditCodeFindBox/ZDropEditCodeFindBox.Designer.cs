using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZDropEditCodeFindBox : ZUserControl, IBindingMemberForCompileTimeCheckProvider, IResourceStringBindingMember
	{
		ZEmbeddedCodeFindBox CodeFindBox;
		ZDropEdit CodeTypeDropEdit;

		void InitializeComponent()
		{
			this.CodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CodeFindBox = new ZEmbeddedCodeFindBox(this);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CodeTypeDropEdit
			// 
			this.CodeTypeDropEdit.AllowDrop = true;
			this.CodeTypeDropEdit.CaptionResourceString = null;
			this.CodeTypeDropEdit.Dock = System.Windows.Forms.DockStyle.Left;
			this.CodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeTypeDropEdit.Name = "CodeTypeDropEdit";
			this.CodeTypeDropEdit.ShowDescriptionBox = false;
			this.CodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CodeTypeDropEdit.TabIndex = 0;
			// 
			// CodeFindBox
			// 
			this.CodeFindBox.AllowDrop = true;
			this.CodeFindBox.CaptionResourceString = null;
			this.CodeFindBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.CodeFindBox.Name = "CodeFindBox";
			this.CodeFindBox.ShowDescriptionBox = false;
			this.CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CodeFindBox.TabIndex = 1;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CodeFindBox, false);
			// 
			// ZCodeFindBoxWithCodeType
			// 
			this.Controls.Add(this.CodeFindBox);
			this.Controls.Add(this.CodeTypeDropEdit);
			this.Name = "ZCodeFindBoxWithCodeType";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

namespace Enterprise.MasterFiles.Module.DialogDefault.OwnerFilter
{
	partial class DialogDefaultOwnerControl
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
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.DialogDefault.DialogDefaultOwnerModuleFilter);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEdit1, "LevelCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.DialogDefault.DialogDefaultOwnerModuleFilter)(null)).LevelCode)));
			this.zDropEdit1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("d8cd2f66-9289-4c8f-9778-7542854a1ff4", "Level");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 3, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.zGuidFindBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "OwnerPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.DialogDefault.DialogDefaultOwnerModuleFilter)(null)).OwnerPk)));
			this.zGuidFindBox1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("f88f084d-f6c6-48bf-a53d-4f8c6b55e981", "Owner");
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 29, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox1.TabIndex = 1;
			// 
			// DialogDefaultOwnerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGuidFindBox1);
			this.Controls.Add(this.zDropEdit1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 56, true);
			this.Name = "DialogDefaultOwnerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
	}
}

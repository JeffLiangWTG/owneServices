using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	partial class OrgForwarderFilterControl
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
		private void InitializeComponent()
		{
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgForwarderModuleFilter);
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgForwarderModuleFilter)(null)).Property)));
			this.zDropEdit1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgForwarderFilterControl|34f1205f-bdc7-484c-b592-f053665c704d", "Mode");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 2, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit2, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgForwarderModuleFilter)(null)).Status)));
			this.zDropEdit2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgForwarderFilterControl|cff4b181-528b-41b9-93df-ffc699c4bc8b", "Status");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 2, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.zDropEdit2.TabIndex = 1;
			// 
			// OrgForwarderFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.zDropEdit1);
			this.Name = "OrgForwarderFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
	}
}

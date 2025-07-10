using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class TouchReceivedFilterControl
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
			this.HorizontalsDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.VerticalsDropEdit = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TouchReceivedModuleFilter);
			// 
			// StaffAssignmentPersonFindBox
			// 
			this.BindingSource.SetBindingMember(this.HorizontalsDropEdit, "HorizontalId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TouchReceivedModuleFilter)(null)).Property)));
			this.HorizontalsDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchReceivedFilterControl|4C4371D3-48EF-474C-8BC4-C28A114760D9", "Horizontal");
			this.HorizontalsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 1, true);
			this.HorizontalsDropEdit.Name = "HorizontalsDropEdit";
			this.HorizontalsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.HorizontalsDropEdit.TabIndex = 3;
			this.HorizontalsDropEdit.ShowDescriptionBox = false;
			this.HorizontalsDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			// 
			// StaffAssignmentRoleDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VerticalsDropEdit, "VerticalId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.TouchReceivedModuleFilter)(null)).VerticalId)));
			this.VerticalsDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("TouchReceivedFilterControl|0B5CEEAE-A3A2-4BC1-9D32-FDD3B0056B6F", "Vertical");
			this.VerticalsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 23, true);
			this.VerticalsDropEdit.Name = "VerticalsDropEdit";
			this.VerticalsDropEdit.PreBoundMaxLength = 2;
			this.VerticalsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.VerticalsDropEdit.TabIndex = 4;
			// 
			// StaffAssignmentPersonAndRoleFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HorizontalsDropEdit);
			this.Controls.Add(this.VerticalsDropEdit);
			this.Name = "TouchReceivedFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit HorizontalsDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit VerticalsDropEdit;
	}
}

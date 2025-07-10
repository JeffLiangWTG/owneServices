namespace Enterprise.MarketingManager.GUI
{
	partial class LinkActivityFilterControl
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
			this.ContextURLDropEdit = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.LinkActivityModuleFilter);
			// 
			// ContextURLDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ContextURLDropEdit, "TypeProperty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.LinkActivityModuleFilter)(null)).TypeProperty)));
			this.ContextURLDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("LinkActivityFilterControl|ae2dfc57-65a0-4d77-9d59-e21e8b21f2f9", "Type");
			this.ContextURLDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 23, true);
			this.ContextURLDropEdit.Name = "ContextURLDropEdit";
			this.ContextURLDropEdit.BindToList = "LinkTrackingList";
			this.ContextURLDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ContextURLDropEdit.TabIndex = 4;
			// 
			// LinkActivityFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContextURLDropEdit);
			this.Name = "LinkActivityFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 43, true);
			this.Controls.SetChildIndex(this.ContextURLDropEdit, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit ContextURLDropEdit;
	}
}

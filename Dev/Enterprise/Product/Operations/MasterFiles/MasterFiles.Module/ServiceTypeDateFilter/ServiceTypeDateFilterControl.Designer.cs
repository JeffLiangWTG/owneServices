using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	partial class ServiceTypeDateFilterControl : ZDateRangeControl
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
			this.jobServiceTypeEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// locationFindBox
			// 
			this.jobServiceTypeEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jobServiceTypeEdit, "JobServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.jobServiceTypeEdit.BindToList = "JobServiceType_List";
			this.jobServiceTypeEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("5FC6B305-0E30-40BA-B459-4C7196019696", "Service Type");
			this.jobServiceTypeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 49, true);
			this.jobServiceTypeEdit.Name = "jobServiceTypeEdit";
			this.jobServiceTypeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.jobServiceTypeEdit.TabIndex = 6;
			// 
			// DateLocationFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.jobServiceTypeEdit);
			this.Name = "ServiceTypeDateFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			this.Controls.SetChildIndex(this.jobServiceTypeEdit, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit jobServiceTypeEdit;
	}
}

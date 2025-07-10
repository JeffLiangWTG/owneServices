using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Module
{
	partial class DateLocationFilterControl : ZDateRangeControl
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
			this.locationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// locationFindBox
			// 
			this.locationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.locationFindBox, "Property3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.locationFindBox.BindToList = "LocationList";
			this.locationFindBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("21edb403-6ab2-4e66-ad60-602ec757be09", "Load Port");
			this.locationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 49, true);
			this.locationFindBox.Name = "locationFindBox";
			this.locationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.locationFindBox.TabIndex = 6;
			// 
			// DateLocationFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.locationFindBox);
			this.Name = "DateLocationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			this.Controls.SetChildIndex(this.locationFindBox, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox locationFindBox;
	}
}

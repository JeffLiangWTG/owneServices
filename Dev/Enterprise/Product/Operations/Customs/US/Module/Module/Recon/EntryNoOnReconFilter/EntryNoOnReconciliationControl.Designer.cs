using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class EntryNoOnReconciliationControl
	{
		private void InitializeComponent()
		{
			this.refNoTextBox = new ZArchitecture.ZTextBox();
			this.entryFilerTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EntryNoOnReconciliationFilter);
			// 
			// RefNoTextBox
			// 
			this.refNoTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.refNoTextBox, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EntryNoOnReconciliationFilter)(null)).Property)));
			this.refNoTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("EntryNoOnReconciliationControl|327e115d-9ab2-44dd-ab22-56ae92572a83", "Entry Number");
			this.refNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 1, true);
			this.refNoTextBox.Name = "RefNoTextBox";
			this.refNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.refNoTextBox.TabIndex = 1;
			// 
			// EntryFilerTextBox
			// 
			this.entryFilerTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.entryFilerTextBox, "EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EntryNoOnReconciliationFilter)(null)).EntryFilerCode)));
			this.entryFilerTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("EntryNoOnReconciliationControl|e72778be-c1ad-435f-b581-2a15e06dfe4a", "Entry Filer Code");
			this.entryFilerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 1, true);
			this.entryFilerTextBox.Name = "EntryFilerTextBox";
			this.entryFilerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
			this.entryFilerTextBox.TabIndex = 0;
			// 
			// EntryNoOnReconciliationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.entryFilerTextBox);
			this.Controls.Add(this.refNoTextBox);
			this.Name = "EntryNoOnReconciliationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.ZTextBox refNoTextBox;
		private ZArchitecture.ZTextBox entryFilerTextBox;
	}
}

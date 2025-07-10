using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class USExportManifestUserControl
	{
		void InitializeComponent()
		{
			this.DeparturePortUNLOCOCodeFindBox = new ZCodeFindBox();
			this.ScheduleDCodeFindBox = new ZCodeFindBox();
			this.IssuerSCACUserControl = new IssuerSCACUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeparturePortUNLOCOCodeFindBox.SuspendLayout();
			this.ScheduleDCodeFindBox.SuspendLayout();
			this.IssuerSCACUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(USExportAsycudaManifestHeader);
			// 
			// DeparturePortUNLOCOCodeFindBox
			// 
			this.DeparturePortUNLOCOCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeparturePortUNLOCOCodeFindBox, "AMA_RL_NKPortOfFinalDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((USExportAsycudaManifestHeader)(null)).AMA_RL_NKPortOfFinalDeparture)));
			this.DeparturePortUNLOCOCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("EF1BA08F-A598-4958-B956-0D2951C06B1F", "Departure Port UNLOCO");
			this.DeparturePortUNLOCOCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 99, true);
			this.DeparturePortUNLOCOCodeFindBox.Name = "DeparturePortUNLOCOCodeFindBox";
			this.DeparturePortUNLOCOCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeparturePortUNLOCOCodeFindBox.ParentType = null;
			this.DeparturePortUNLOCOCodeFindBox.PreBoundMaxLength = 5;
			this.DeparturePortUNLOCOCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.DeparturePortUNLOCOCodeFindBox.TabIndex = 11;
			// 
			// ScheduleDCodeFindBox
			// 
			this.ScheduleDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScheduleDCodeFindBox, "MasterBill+ABL_CustomsLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((USExportAsycudaManifestHeader)(null)).MasterBill.ABL_CustomsLoadPort)));
			this.ScheduleDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("7C01B11C-9D7B-4607-B9A3-622E0322535A", "Schedule D");
			this.ScheduleDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 99, true);
			this.ScheduleDCodeFindBox.Name = "ScheduleDCodeFindBox";
			this.ScheduleDCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ScheduleDCodeFindBox.ParentType = null;
			this.ScheduleDCodeFindBox.PreBoundMaxLength = 4;
			this.ScheduleDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ScheduleDCodeFindBox.TabIndex = 12;
			//
			// IssuerSCACUserControl
			//
			this.BindingSource.SetBindingMember(this.IssuerSCACUserControl, ".");
			this.IssuerSCACUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 99, true);
			this.IssuerSCACUserControl.Name = "IssuerSCACUserControl";
			this.IssuerSCACUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 60, true);
			this.IssuerSCACUserControl.TabIndex = 3;
			this.IssuerSCACUserControl.TabStop = false;
			// 
			// USExportManifestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeparturePortUNLOCOCodeFindBox);
			this.Controls.Add(this.ScheduleDCodeFindBox);
			this.Controls.Add(this.IssuerSCACUserControl);
			this.Name = "USExportManifestUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeparturePortUNLOCOCodeFindBox.ResumeLayout(true);
			this.DeparturePortUNLOCOCodeFindBox.PerformLayout();
			this.ScheduleDCodeFindBox.ResumeLayout(true);
			this.ScheduleDCodeFindBox.PerformLayout();
			this.IssuerSCACUserControl.ResumeLayout(true);
			this.IssuerSCACUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZCodeFindBox DeparturePortUNLOCOCodeFindBox;
		internal ZCodeFindBox ScheduleDCodeFindBox;
		internal IssuerSCACUserControl IssuerSCACUserControl;
	}
}

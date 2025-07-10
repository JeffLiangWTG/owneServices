using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("coming soon")]
	public partial class RefFacilityForm : ZTemplateForm
	{
		public RefFacilityForm(RefFacility refFacility)
			: base(refFacility)
		{
			InitializeComponent();
			this.CaptionRenderingEnabled = true;
			ControllerID = ControllerID ?? ControllerIDs.RefFacility;
			this.Text = "";
		}

		void RefFacilityLocalCodeTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.RefFacilityLocalCodeControl = new RefFacilityLocalCodeControl();
			this.RefFacilityLocalCodeControl.SuspendLayout();
			this.RefFacilityLocalCodesTabPage.Controls.Add(this.RefFacilityLocalCodeControl);
			// 
			// RefFacilityLocalCodeControl
			// 
			this.RefFacilityLocalCodeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefFacilityLocalCodeControl, ".");
			this.RefFacilityLocalCodeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefFacilityLocalCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RefFacilityLocalCodeControl.Name = "RefFacilityLocalCodeControl";
			this.RefFacilityLocalCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 136, true);
			this.RefFacilityLocalCodeControl.TabIndex = 0;
			this.RefFacilityLocalCodeControl.ResumeLayout(true);
			this.RefFacilityLocalCodeControl.PerformLayout();
		}
	}
}

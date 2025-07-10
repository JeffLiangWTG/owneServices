using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.GUI
{
	public partial class ZUSOrganisationControl
	{
		void InitializeComponent()
		{
            this.defaultControl = new Enterprise.Customs.US.GUI.Internal.ZUSOrganisationWrappedControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.defaultControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // defaultControl
            // 
            this.defaultControl.AllowDrop = true;
            this.defaultControl.Captions = new string[0];
            this.defaultControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defaultControl.IsCaptionOverridden = false;
            this.defaultControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.defaultControl.Name = "defaultControl";
            this.defaultControl.OrgAddressFormatter = null;
            this.defaultControl.PopupCaption = "";
            this.defaultControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 182, true);
            this.defaultControl.TabIndex = 0;
            // 
            // ZUSOrganisationControl
            // 
            this.Controls.Add(this.defaultControl);
            this.Name = "ZUSOrganisationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 182, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.defaultControl.ResumeLayout(true);
            this.defaultControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		Internal.ZUSOrganisationWrappedControl defaultControl;
	}
}

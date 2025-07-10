using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.GUI
{
	public partial class InBondContainerTrackingUserControl
	{
		void InitializeComponent()
		{
			this.DetailTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsGroupBox, false);
			// 
			// InBondContainerTrackingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "InBondContainerTrackingUserControl";
			this.DetailTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

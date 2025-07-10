using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgAddressesFilterControl
	{
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			this.FilteredGrid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgAddress);
			// 
			// OrgAddressesFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "OrgAddressesFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ProfitShareConsolWrapperModuleButtonGrid : ZModuleButtonGrid, IGridControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ProfitShareForwardingConsolWrapper);
			// 
			// ProfitShareConsolWrapperModuleButtonGrid
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ProfitShareConsolWrapperModuleButtonGrid";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

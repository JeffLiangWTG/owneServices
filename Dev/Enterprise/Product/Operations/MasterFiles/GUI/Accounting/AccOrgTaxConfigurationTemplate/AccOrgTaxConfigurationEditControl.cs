using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccOrgTaxConfigurationEditControl : ZUserControl
	{
		public AccOrgTaxConfigurationEditControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Template != null && !Template.OCT_IsReceivable)
			{
				taxConfigGrid.RemoveFromAvailableColumns(AccOrgTaxConfigurationSchema.OTC_RecoverTax.Name);
			}
		}

		#region Binding

		AccOrgTaxConfigurationTemplate Template
		{
			get { return (AccOrgTaxConfigurationTemplate)CurrentDataItem; }
		}

		#endregion
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public partial class IncotermAndIncotermPlaceUserControl : ZUserControl, ITopLevelDataSourceType
	{
		public IncotermAndIncotermPlaceUserControl()
		{
			InitializeComponent();
		}
		protected void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			var form = new IncoTermDescriptionForm(JZ_IncoTermBoundDropDownEdit.Text);
			ZFormModaliser.Show(form, ParentForm as ZForm);
		}

		Type ITopLevelDataSourceType.DataSourceType => DesignModeFinder.IsDesigning ? DataSourceType : Parent is DynamicLayoutPanel panel ? panel.DataSourceType : DataSourceType;
	}
}

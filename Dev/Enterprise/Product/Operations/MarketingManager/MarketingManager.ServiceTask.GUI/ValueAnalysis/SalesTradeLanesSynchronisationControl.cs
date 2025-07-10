using System;
using System.Globalization;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.MarketingManager.ServiceTask.GUI
{
	public partial class SalesTradeLanesSynchronisationControl : ZUserControl
	{
		public SalesTradeLanesSynchronisationControl()
		{
			InitializeComponent();

			var date = SystemDataRegistry.Instance.LastYearlySyncTLS.Value.Date;

			lastYearlySyncDate.Text = date == DateTime.MinValue
				? Res.GetString("DA7E3273-DB77-4110-8BA4-F1343E5D8778", "N/A")
				: date.ToString(DateTimeFormatStrings.ShortDateFormat, CultureInfo.CurrentCulture);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var configObj = new SalesTradeLanesSyncTaskConfig(ObjectFactory.Get<IServiceTaskAccessor>().GetServiceTask(dataSource));
				base.SetDataBinding(configObj, "");
			}
			else
			{
				base.SetDataBinding(null, "");
			}
		}

		void RunOnDayOfMonth_ValueChanged(object sender, EventArgs e)
		{
			zLabel1.Visible = runOnDayOfMonth.Value == 31;
		}
	}
}

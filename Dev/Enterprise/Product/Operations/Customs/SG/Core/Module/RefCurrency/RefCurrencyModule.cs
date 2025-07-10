using System;

using Enterprise.Customs.SG.V4.GUI;

namespace Enterprise.Customs.SG.V4.Module
{
	public class RefCurrencyModule : MasterFiles.Module.RefCurrencyModule
	{
		public RefCurrencyModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Customs Exchange Rates", new EventHandler(ImportExchangeRates));
		}

		#region Import Customs Exchange Rates

		public void ImportExchangeRates(object sender, EventArgs e)
		{
			new ImportCustomsExchangeRatesForm().Show();
		}

		#endregion
	}
}

using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestsSubclassesOf(typeof(TradeDetailsControl))]
	public abstract class TradeDetailsControlBaseTest : ZFormBasherTest
	{
		protected sealed override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_OH_Primary = org.PK;
			var detail = sales.TradeDetails.AddNew();
			detail.ProspectDetail.PAP_ConversionCertainty = 1;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			Factory.Save();

			var entitySales = EntitySalesWrapper.Get(sales, org);

			var form = new TradeDetailsControlFormForTest(entitySales);
			form.CaptionRenderingEnabled = true;
			form.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 800, true);

			var control = GetNewControlForTest();
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);

			return form;
		}

		public override Type FormToBashType => typeof(TradeDetailsControlFormForTest);

		protected abstract TradeDetailsControl GetNewControlForTest();
	}
}

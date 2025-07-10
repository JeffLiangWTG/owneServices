using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestsSubclassesOf(typeof(TradeLanesControl))]
	public abstract class TradeLanesControlBaseTest : ZFormBasherTest
	{
		protected sealed override Form GetFormToBashCore()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			var form = new TradeLanesControlFormForTest(salesHeader);
			form.CaptionRenderingEnabled = true;
			form.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 800, true);

			var control = GetNewControlForTest();
			form.Controls.Add(control);

			return form;
		}

		protected abstract TradeLanesControl GetNewControlForTest();

		public override Type FormToBashType => typeof(TradeLanesControlFormForTest);
	}
}

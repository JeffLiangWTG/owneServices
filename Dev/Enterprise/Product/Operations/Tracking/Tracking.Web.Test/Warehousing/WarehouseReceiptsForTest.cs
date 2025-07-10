using System;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseReceiptsForTest : WarehouseReceipts
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/cwWeb/test.aspx?data=xyz");

		protected override BrowserType GetBrowserType() => BrowserType.Mozilla;
		protected override BusinessObject GetNewDataSource() => TrackingHelper.Get(Factory.New<WhsReceive>());

		public void CallOnInit()
		{
			OnInit(EventArgs.Empty);
		}

		public void SetupPageForTesting()
		{
			TitleLabel = new ZTextLabel();
			UnauthorisedLabel = new ZTextLabel();
			SearchControlHolder = new HtmlGenericControl();
			LoadOrCreateDataSource();
		}
	}
}

using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestOrders : Orders.Orders
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void InitControls()
		{
			SearchControlHolder = new HtmlGenericControl();
			TimeLineLegendHolder = new HtmlGenericControl();
		}

		public BusinessObject GetNewDataSourceForTest()
		{
			return GetNewDataSource();
		}

		public void SetupPageForTesting()
		{
			SwitchHyperLink = new LinkButton();
		}

		public LinkButton SwitchHyperLinkForTesting
		{
			get { return SwitchHyperLink; }
		}

		public void GetTimeLineViewForTest()
		{
			CheckViewMode();
		}

		public bool IsTimeLineForTest
		{
			get { return IsTimeLine; }
		}

		public void HandleModeSwitchClickForTest()
		{
			HandleModeSwitchClick();
		}

		protected override string ViewModeInSession
		{
			get
			{
				return ViewModeInSessionForTest;
			}
			set
			{
				ViewModeInSessionForTest = value;
			}
		}
		public string ViewModeInSessionForTest = TimeLine;

		protected override void WriteToRegistry(string mode)
		{
			base.WriteToRegistry(mode);
			WriteCount++;
		}

		public bool IsSwitchHyperLinkClickForTest
		{
			get { return isSwitchHyperLinkClick; }
			set { isSwitchHyperLinkClick = value; }
		}

		protected override bool IsSwitchHyperLinkClick
		{
			get { return IsSwitchHyperLinkClickForTest; }
		}

		bool isSwitchHyperLinkClick;

		protected override string ReadFromRegistry()
		{
			ReadCount++;
			return base.ReadFromRegistry();
		}

		public int ReadCount;

		public int WriteCount;
	}
}

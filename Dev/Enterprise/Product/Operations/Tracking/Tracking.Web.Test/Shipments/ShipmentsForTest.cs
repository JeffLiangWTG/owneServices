using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ShipmentsForTest : Shipments
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetupPageForTest()
		{
			SwitchHyperLink = new HyperLink();
			SearchControlHolder = new HtmlGenericControl();
			UnshippedOrdersSearchControlHolder = new HtmlGenericControl();
			UnshippedOrdersContentPane = new HtmlGenericControl();
		}

		public HyperLink SwitchHyperLinkForTest
		{
			get { return SwitchHyperLink; }
		}

		public void SetupSearchControlsForTest()
		{
			SetupSearchControl();
			SetupUnshippedOrdersSearchControl();
			SearchControlForTest.Page = this;
			UnshippedOrdersSearchControlForTest.Page = this;
			OnLoad(EventArgs.Empty);
		}

		public ZSearchControl SearchControlForTest => (ZSearchControl)SearchControl;

		public ZSearchControl UnshippedOrdersSearchControlForTest => UnshippedOrdersSearchControl;

		public bool ModeInSessionForTest
		{
			get
			{
				return ModeInSession;
			}
			set
			{
				ModeInSession = value;
			}
		}

		public void ZeroModeInSession()
		{
			Session[ShowUnshippedOrdersIndexer] = null;
		}

		protected override void WriteToRegistry(string mode)
		{
			base.WriteToRegistry(mode);
			RegistryWriteCount++;
		}

		protected override string ReadFromRegistry()
		{
			RegistryReadCount++;
			return base.ReadFromRegistry();
		}

		public int RegistryReadCount;
		public int RegistryWriteCount;

		public string ModeInRegistry
		{
			get
			{
				return base.ReadFromRegistry();
			}
			set
			{
				base.WriteToRegistry(value);
			}
		}

		public void ZeroRegistryVariables()
		{
			RegistryReadCount = RegistryWriteCount = 0;
			ModeInRegistry = string.Empty;
		}

		public bool SearchParamForTest
		{
			get
			{
				return searchParam;
			}
			set
			{
				searchParam = value;
			}
		}

		protected override bool SearchParam
		{
			get
			{
				return searchParam;
			}
		}

		bool searchParam;

		public bool SearchParamExistForTest
		{
			get
			{
				return searchParamExist;
			}
			set
			{
				searchParamExist = value;
			}
		}

		protected override bool SearchParamExist
		{
			get
			{
				return searchParamExist;
			}
		}

		bool searchParamExist;

		public bool ModeParamForTest
		{
			get
			{
				return modeParam;
			}
			set
			{
				modeParam = value;
			}
		}

		protected override bool ModeParam
		{
			get
			{
				return modeParam;
			}
		}

		bool modeParam;

		public bool ShowUnshippedOrdersForTest
		{
			get
			{
				return ShowUnshippedOrders;
			}
		}
	}
}

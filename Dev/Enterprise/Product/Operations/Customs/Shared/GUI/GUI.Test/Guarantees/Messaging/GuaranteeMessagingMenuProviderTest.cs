using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	class GuaranteeMessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new GuaranteeMessagingMenuProvider(null));
		}

		public void TestCreateMenuItems()
		{
			AssertEquals(0, menuItems.Count());
		}

		public void TestRefreshMenu()
		{
			provider.RefreshMenu();
			AssertEquals(0, menuItems.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<BaseCusGuaranteeHeader>();
			provider = new GuaranteeMessagingMenuProvider(header);
			menuItems = provider.CreateMenuItems();
		}

		BaseCusGuaranteeHeader header;
		GuaranteeMessagingMenuProvider provider;
		IEnumerable<ZMenuItem> menuItems;
	}
}

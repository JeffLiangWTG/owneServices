using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class GuaranteeMessagingMenuItemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new GuaranteeMessagingMenuItem(null));
		}

		public void TestAddMenuItems_Default()
		{
			using (var menu = new GuaranteeMessagingMenuItem(Factory.New<BaseCusGuaranteeHeader>()))
			{
				AssertEquals(0, menu.MenuItems.Count);
			}
		}

		public void TestAddMenuItems_IE()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			header.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			using (var menu = new GuaranteeMessagingMenuItem(header))
			{
				AssertContainsExactElementsInExactOrder(new[] { "Guarantee Voucher Sold", "Update Access Code" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestAddMenuItems()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Eritrea, new TestObjectHandle(new GuaranteeMessagingMenuProviderForTest(header)) }
			};
			using (ObjectFactory.Substitute("GuaranteeMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var menu = new GuaranteeMessagingMenuItem(header))
			{
				AssertContainsExactElementsInExactOrder(new[] { "Test Menu Item", "Invisible Menu Item" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestOnPopup()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Eritrea, new TestObjectHandle(new GuaranteeMessagingMenuProviderForTest(header)) }
			};
			using (ObjectFactory.Substitute("GuaranteeMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var menu = new GuaranteeMessagingMenuItem(header))
			{
				menu.ShowPopupMenu();
				AssertEquals($"&Messaging{System.Environment.NewLine}   Test Menu Item", menu.GetVisibleMenuItemsCaptions());
			}
		}

		class GuaranteeMessagingMenuProviderForTest : GuaranteeMessagingMenuProvider
		{
			public GuaranteeMessagingMenuProviderForTest(BaseCusGuaranteeHeader header)
				: base(header)
			{
			}

			protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
			{
				yield return new ZMenuItem("Test Menu Item");
				yield return invisibleMenuItem = new ZMenuItem("Invisible Menu Item");
			}
			ZMenuItem invisibleMenuItem;

			public override void RefreshMenu()
			{
				SetMenuItemVisibility(invisibleMenuItem, () => false);
			}
		}
	}
}

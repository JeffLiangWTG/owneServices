using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ContactItemStripComparerForListControlTest : TestCaseWithFactory
	{
		public void TestSort_WithContactItemStripWithNoDataSource()
		{
			var sorter = new DummyContactItemStripComparerForListControl();
			using (var contactItemStripA = new ContactItemStrip())
			using (var contactItemStripB = new ContactItemStrip())
			{
				contactItemStripA.DescriptionDropDownList.Text = "AAA";
				contactItemStripB.DescriptionDropDownList.Text = "BBB";

				var contactItemStrips = new[] { contactItemStripA, contactItemStripB };

				AssertArrayEqualsByElements(
					new[] { "AAA", "BBB" },
					contactItemStrips.OrderBy(x => x, sorter).Select(strip => strip.DescriptionDropDownList.Text).ToArray()
				);
			}
		}

		class DummyContactItemStripComparerForListControl : ContactItemsListControl.ContactItemStripComparerForListControl
		{
			public DummyContactItemStripComparerForListControl()
				: base(new CodeDescriptionPairList(), System.Array.Empty<string>())
			{
			}
		}
	}
}

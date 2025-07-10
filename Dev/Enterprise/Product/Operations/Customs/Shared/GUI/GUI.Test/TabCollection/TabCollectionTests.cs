using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TabCollectionTests : TestCase
	{
		public void TestContentCreation()
		{
			using (ZTabControl tabControl = new ZTabControl())
			{
				visibleTabKeys.Add("Tab1");
				TabCollection tabCollection = new TabCollection(tabControl, null);
				tabCollection.Add(() => visibleTabKeys.Contains("Tab1"), "Tab1", () => new ZUserControl { Name = "TabContent1" });
				tabCollection.Add(() => visibleTabKeys.Contains("Tab2"), "Tab2", () => new ZUserControl { Name = "TabContent2" });
				tabCollection.UpdateTabs();
				// both tab pages are created immediately
				AssertEquals(2, tabControl.AllTabPages.Length);
				TabPage tabPage1 = tabControl.AllTabPages[0];
				TabPage tabPage2 = tabControl.AllTabPages[1];
				// only first tab is visible
				AssertArrayEqualsByElements(new string[] { "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				// only first tab has content
				AssertEquals(1, tabPage1.Controls.Count);
				AssertEquals(0, tabPage2.Controls.Count);
				AssertEquals("TabContent1", tabPage1.Controls[0].Name);
				// hiding first tab and showing second tab
				visibleTabKeys.Remove("Tab1");
				visibleTabKeys.Add("Tab2");
				tabCollection.UpdateTabs();
				// only second tab is visible
				AssertArrayEqualsByElements(new string[] { "Tab2" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				// both tabs has content
				AssertEquals(1, tabPage1.Controls.Count);
				AssertEquals(1, tabPage2.Controls.Count);
				AssertEquals("TabContent1", tabPage1.Controls[0].Name);
				AssertEquals("TabContent2", tabPage2.Controls[0].Name);
			}
		}

		public void TestDispose()
		{
			ZTabControl tabControl;
			ZUserControl dynamiContent1 = null;
			ZUserControl dynamiContent2 = null;
			ZUserControl staticContent1 = new ZUserControl();
			ZUserControl staticContent2 = new ZUserControl();
			ZUserControl staticContent3 = new ZUserControl();
			using (tabControl = new ZTabControl())
			{
				TabCollection tabCollection = new TabCollection(tabControl, null);
				// adding one visible and one invisible tab dynamic tab
				tabCollection.Add(() => true, "Static1", staticContent1);
				tabCollection.Add(() => false, "Static2", staticContent2);
				// adding one visible and one invisible tab dynamic tab
				tabCollection.Add(() => true, "Dynamic1", () => dynamiContent1 = new ZUserControl());
				tabCollection.Add(() => false, "Dynamic2", () => dynamiContent2 = new ZUserControl());
				// (Invalid Usage Example!) adding invisible dynamic tab with static content
				tabCollection.Add(() => false, "Dynamic2", () => staticContent3);
				tabCollection.UpdateTabs();
			}

			AssertEquals(true, tabControl.IsDisposed);
			// both visible and invisible static tabs contents are disposed
			AssertEquals(true, staticContent1.IsDisposed);
			AssertEquals(true, staticContent2.IsDisposed);
			// content of visible dynamic tab is disposed, content of invisible dynamic tab was never created
			AssertEquals(true, dynamiContent1.IsDisposed);
			AssertNull(dynamiContent2);
			// (Invalid Usage Example!) content of the dynamic tab that was created incorrectly is not disposed
			AssertEquals(false, staticContent3.IsDisposed);
			staticContent3.Dispose();
		}

		public void TestDirectTabOrder()
		{
			using (ZTabControl tabControl = new ZTabControl())
			{
				TabCollection tabCollection = new TabCollection(tabControl, null);
				tabCollection.Add(() => visibleTabKeys.Contains("Tab1"), "Tab1", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab2"), "Tab2", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab3"), "Tab3", () => new ZUserControl());
				tabCollection.UpdateTabs();
				AssertEquals(0, tabControl.TabPages.Count);
				visibleTabKeys.Add("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab2" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(Array.Empty<string>(), tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
			}
		}

		public void TestReverseTabOrder()
		{
			using (ZTabControl tabControl = new ZTabControl())
			{
				TabCollection tabCollection = new TabCollection(tabControl, null);
				tabCollection.Add(() => visibleTabKeys.Contains("Tab1"), "Tab1", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab2"), "Tab2", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab3"), "Tab3", () => new ZUserControl());
				tabCollection.UpdateTabs();
				AssertEquals(0, tabControl.TabPages.Count);
				visibleTabKeys.Add("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab2" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(Array.Empty<string>(), tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
			}
		}

		public void TestNonlinearTabOrder()
		{
			using (ZTabControl tabControl = new ZTabControl())
			{
				TabCollection tabCollection = new TabCollection(tabControl, null);
				tabCollection.Add(() => visibleTabKeys.Contains("Tab1"), "Tab1", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab2"), "Tab2", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab3"), "Tab3", () => new ZUserControl());
				tabCollection.UpdateTabs();
				AssertEquals(0, tabControl.TabPages.Count);
				visibleTabKeys.Add("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(Array.Empty<string>(), tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
			}
		}

		public void TestOrderWithExistingTabs()
		{
			using (ZTabControl tabControl = new ZTabControl())
			using (ZTabPage existingTab = new ZTabPage())
			{
				existingTab.Text = "EXISTING";
				tabControl.TabPages.Add(existingTab);
				TabCollection tabCollection = new TabCollection(tabControl, null);
				tabCollection.Add(() => visibleTabKeys.Contains("Tab1"), "Tab1", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab2"), "Tab2", () => new ZUserControl());
				tabCollection.Add(() => visibleTabKeys.Contains("Tab3"), "Tab3", () => new ZUserControl());
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING", "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING", "Tab1", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Add("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING", "Tab1", "Tab2", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab2");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING", "Tab1", "Tab3" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab3");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING", "Tab1" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
				visibleTabKeys.Remove("Tab1");
				tabCollection.UpdateTabs();
				AssertArrayEqualsByElements(new string[] { "EXISTING" }, tabControl.FindAll<ZTabPage>(t => t.TabVisible).Select(t => t.Text).ToArray());
			}
		}

		readonly ISet<string> visibleTabKeys = new HashSet<string>();
	}
}

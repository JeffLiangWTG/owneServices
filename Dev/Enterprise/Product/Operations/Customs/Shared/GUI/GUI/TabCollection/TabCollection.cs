using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	/// <summary>
	/// This class can add / remove tabs to ZTabControl based on the results of a boolean function.
	/// </summary>
	public class TabCollection
	{
		readonly ZTabControl tabControl;
		readonly List<TabInfo> tabs = new List<TabInfo>();

		public TabCollection(ZTabControl tabControl, ZGrid relatedGrid)
		{
			this.relatedGrid = relatedGrid;
			this.tabControl = tabControl;
		}

		readonly ZGrid relatedGrid;

		/// <summary>
		/// Adds the given tab to this tab collection and associated <see cref="ZTabControl"/>.
		/// <para/>
		/// All tabs should be added to this collection before the first call of the <see cref="UpdateTabs"/> method to ensure correct tab order.
		/// </summary>
		/// <param name="isVisible">A function that indicates whether this tab should be visible or not.</param>
		/// <param name="caption">A caption for the tab.</param>
		/// <param name="content">The content of the tab.</param>
		/// <param name="dataSourceGetter">The getter that will be used to obtain datasource for this tab. If it is null, then no data binding will happen.</param>
		/// <param name="dataMemberGetter">The getter that will be user to obtain datamember for this tab.</param>
		public void Add(Func<bool> isVisible, string caption, ZUserControl content, Func<object> dataSourceGetter = null, Func<string> dataMemberGetter = null)
		{
			Add(isVisible, caption, () => content, true, dataSourceGetter, dataMemberGetter);
		}

		/// <summary>
		/// Adds the given tab to this tab collection and associated <see cref="ZTabControl"/>.
		/// <para/>
		/// All tabs should be added to this collection before the first call of the <see cref="UpdateTabs"/> method to ensure correct tab order.
		/// <para/>
		/// <b>
		/// Only use this method for dynamically created content. Otherwise, it is not guaranteed that content will be disposed.
		/// <para/>
		/// For static content use <see cref="Add(Func{bool},string,ZUserControl,Func{object},Func{string})"/>
		/// <para/>
		/// See TabCollectionTests.TestDispose for examples of correct and incorrect usage.
		/// </b>
		/// </summary>
		/// <param name="isVisible">A function that indicates whether this tab should be visible or not.</param>
		/// <param name="caption">A caption for the tab.</param>
		/// <param name="createContent">A method that will create content for the tab if tab should be visible.</param>
		/// <param name="dataSourceGetter">The getter that will be used to obtain datasource for this tab. If it is null, then no data binding will happen.</param>
		/// <param name="dataMemberGetter">The getter that will be user to obtain datamember for this tab.</param>
		public void Add(Func<bool> isVisible, string caption, Func<ZUserControl> createContent, Func<object> dataSourceGetter = null, Func<string> dataMemberGetter = null)
		{
			Add(isVisible, caption, createContent, false, dataSourceGetter, dataMemberGetter);
		}

		void Add(Func<bool> isVisible, string caption, Func<ZUserControl> createContent, bool includeToTree, Func<object> dataSourceGetter, Func<string> dataMemberGetter)
		{
			var tabPage = new ZTabPage();
			tabPage.SuspendLayout();

			tabPage.Text = caption;
			tabPage.Dock = DockStyle.Fill;
			tabControl.TabPages.Add(tabPage);
			tabPage.CheckForChildrenControlsVisibilityChange = false;
			tabPage.ResumeLayout();

			tabs.Add(new TabInfo(tabPage, relatedGrid, isVisible, createContent, includeToTree, dataSourceGetter, dataMemberGetter));
		}

		/// <summary>
		/// Updates visibility of the tabs and creates content for them if necessary.
		/// </summary>
		public void UpdateTabs()
		{
			tabControl.SuspendLayout();

			foreach (TabInfo tabInfo in tabs)
			{
				tabInfo.UpdateTab();
			}

			tabControl.ResumeLayout();
		}

		class TabInfo
		{
			readonly ZTabPage tabPage;
			readonly ZGrid relatedGrid;
			readonly Func<bool> isVisible;
			readonly Func<ZUserControl> createContent;
			readonly Func<object> dataSourceGetter;
			readonly Func<string> dataMemberGetter;

			ZUserControl content;

			public TabInfo(ZTabPage tabPage, ZGrid relatedGrid, Func<bool> isVisible, Func<ZUserControl> createContent, bool includeToTree, Func<object> dataSourceGetter, Func<string> dataMemberGetter)
			{
				this.tabPage = tabPage;
				this.relatedGrid = relatedGrid;
				this.isVisible = isVisible;
				this.createContent = createContent;
				this.dataSourceGetter = dataSourceGetter;
				this.dataMemberGetter = dataMemberGetter;

				if (includeToTree)
				{
					CreateContent();
				}
			}

			public void UpdateTab()
			{
				bool visible = isVisible();

				if (visible && content == null)
				{
					CreateContent();
				}

				if (visible && dataSourceGetter != null)
				{
					object dataSource = dataSourceGetter();
					string dataMember = dataMemberGetter?.Invoke();
					content.SetDataBinding(dataSource, dataMember);
				}

				using (relatedGrid?.SuspendCancelOfNonEditedRowOnLeaving())
				{
					tabPage.TabVisible = visible;
				}
				tabPage.CheckForNotifications = visible;
			}

			void CreateContent()
			{
				content = createContent();
				content.SuspendLayout();
				content.Dock = DockStyle.Fill;
				tabPage.Controls.Add(content);
				content.ResumeLayout();
			}
		}
	}
}

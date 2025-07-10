using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolContainerUserControl : ZUserControl
	{
		public ConsolContainerUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				JobContainerBoundGrid.InnerGrid.LayoutCategoryPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			}

			SetContextMenu();
		}

		void SetContextMenu()
		{
			var splitContainersMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.SplitContainers", "Split"));
			JobContainerBoundGrid.InnerGrid.ContextMenu.MenuItems.Add(0, splitContainersMenuItem);
			splitContainersMenuItem.Click += (sender, e) =>
			{
				ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
				splitter.Split(SelectedContainer);
			};

			var mergeContainersMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.MergeContainers", "Merge"));
			JobContainerBoundGrid.InnerGrid.ContextMenu.MenuItems.Add(1, mergeContainersMenuItem);
			mergeContainersMenuItem.Click += (sender, e) =>
			{
				var containers = JobContainerBoundGrid.InnerGrid.SelectedElements.Cast<ForwardingContainer>();
				var merger = new ForwardingContainerMerger(containers);
				var helper = new BusinessObjectMergerHelper<ForwardingContainer>(merger, ParentForm);
				helper.Merge();
			};
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Parent is TabPage page && page != null)
			{
				page.AutoScroll = true;
				page.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 550, true);
			}

			base.OnLoad(e);
		}

		internal bool VGMVisible
		{
			get => containersUserControl1.VGMTabPage.TabVisible;
			set => containersUserControl1.VGMTabPage.TabVisible = value;
		}

		#region SetupPlugIn

		public void SetupPlugIn()
		{
			containersUserControl1.SetupPlugIn(JobContainerBoundGrid.InnerGrid);
		}

		#endregion

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (ContainerListManager != null)
			{
				ContainerListManager.ListChanged -= ListManager_CurrentOrListChanged;
				ContainerListManager.CurrentChanged -= ListManager_CurrentOrListChanged;
			}

			Consol = (ForwardingConsol)dataSource;
			base.SetDataBinding(Consol == null ? null : Consol.Containers, "");

			if (ContainerListManager != null)
			{
				ContainerListManager.ListChanged += ListManager_CurrentOrListChanged;
				ContainerListManager.CurrentChanged += ListManager_CurrentOrListChanged;
				ContainerChange();
			}
		}

#if DEBUG
		public CurrencyManager ContainerListManagerForDebugging => ContainerListManager;
#endif

		protected virtual CurrencyManager ContainerListManager
		{
			get
			{
				return (CurrencyManager)GetBindingManager("");
			}
		}

		#endregion

		#region Selected Container

		void ListManager_CurrentOrListChanged(object sender, EventArgs e)
		{
			ContainerChange();
		}

		void ContainerChange()
		{
			if (ContainerListManager != null)
			{
				if (ContainerListManager.List.Count >= 1 && ContainerListManager.Position >= 0)
				{
					SelectedContainer = (ForwardingContainer)ContainerListManager.GetCurrent();
				}
				else
				{
					SelectedContainer = null;
				}
			}
		}

		ForwardingConsol Consol { get; set; }

		ForwardingContainer SelectedContainer
		{
			get { return (ForwardingContainer)containersUserControl1.CurrentContainer; }
			set { containersUserControl1.CurrentContainer = value; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && ContainerListManager != null)
			{
				ContainerListManager.ListChanged -= ListManager_CurrentOrListChanged;
				ContainerListManager.CurrentChanged -= ListManager_CurrentOrListChanged;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}

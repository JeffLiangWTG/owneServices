using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ForwardingContainersPackingPlugIn : ContainersPackingPlugIn
	{
		public ForwardingContainersPackingPlugIn(CommonConsol hostEntity)
			: base(hostEntity)
		{
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ForwarderContainersPacking; }
		}

		protected override void OnUserControlCreated()
		{
			base.OnUserControlCreated();
			ContainersModuleButtonGrid.ShowAttachButton = false;
			ContainersModuleButtonGrid.ShowDetachButton = false;
			ContainersModuleButtonGrid.ShowNewButton = false;
		}

		MenuItem splitContainerMenuItem;
		MenuItem mergeContainersMenuItem;
		MenuItem mergePackLineMenuItem;
		MenuItem mergeAllPackLinesMenuItem;
		MenuItem mergeDividerMenuItem;

		protected override void SetUpContextMenus()
		{
			base.SetUpContextMenus();

			containersModuleMenuIndex = 0;

			splitContainerMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.SplitContainer", "Split"));
			AddContainersModuleMenuItem(splitContainerMenuItem);

			mergeContainersMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.MergeContainers", "Merge"));
			AddContainersModuleMenuItem(mergeContainersMenuItem);

			splitContainerMenuItem.Click += SplitContainerClick;
			mergeContainersMenuItem.Click += MergeContainers_Click;

			mergePackLineMenuItem = new ZMenuItem(ResString.GetMultilingualString("ForwardingContainersPackingPlugIn|Merge", "Merge"));
			AddPackLineMenuItem(mergePackLineMenuItem);

			mergeAllPackLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("ForwardingContainersPackingPlugIn|MergeAll", "Merge All"));
			AddPackLineMenuItem(mergeAllPackLinesMenuItem);

			mergeDividerMenuItem = new ZMenuItem("-");
			AddPackLineMenuItem(mergeDividerMenuItem);

			mergePackLineMenuItem.Click += MergePackLine_Click;
			mergeAllPackLinesMenuItem.Click += MergeAllPackLines_Click;
		}

		int containersModuleMenuIndex;

		void AddContainersModuleMenuItem(MenuItem menuItem)
		{
			ContainersModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(containersModuleMenuIndex, menuItem);
			containersModuleMenuIndex++;
		}

		void SplitContainerClick(object sender, EventArgs eventArgs)
		{
			var splitter = new ForwardingConsolContainerSplitter();
			splitter.Split((ForwardingContainer)GetSelectedContainer());
		}

		void MergeContainers_Click(object sender, EventArgs eventArgs)
		{
			var containers = ContainersModuleButtonGrid.InnerGrid.SelectedElements.Cast<ForwardingContainer>();
			var merger = new ForwardingContainerMerger(containers);
			var helper = new BusinessObjectMergerHelper<ForwardingContainer>(merger, PackingDetailsUserControl.ParentForm);
			helper.Merge();
		}

		void MergePackLine_Click(object sender, EventArgs eventArgs)
		{
			var seleсtedPackLines = GetSelectedUnAllocatedPackLines().Cast<ForwardingPackLine>();
			MergePackLines(seleсtedPackLines, MergeOption.Merge);
		}

		void MergeAllPackLines_Click(object sender, EventArgs eventArgs)
		{
			var seleсtedPackLines = UnAllocatedPackLines.Cast<ForwardingPackLine>().ToArray();
			if (seleсtedPackLines.Any())
			{
				MergePackLines(seleсtedPackLines, MergeOption.MergeAll);
			}
			else
			{
				NoPackLines();
			}
		}

		void MergePackLines(IEnumerable<ForwardingPackLine> seleсtedPackLines, MergeOption mergeOption)
		{
			var consol = HostBusinessEntity as ForwardingConsol;
			var merger = new ForwardingPackLineMerger(seleсtedPackLines, mergeOption, consol);
			var helper = new BusinessObjectMergerHelper<ForwardingPackLine>(merger, PackingDetailsUserControl.ParentForm);
			helper.Merge();
		}

		void NoPackLines()
		{
			Globals.Message.Show(Res.GetString("b94598d0-6ebe-43ee-88ca-4889deecdb5b", "There are no pack lines."));
		}
	}
}

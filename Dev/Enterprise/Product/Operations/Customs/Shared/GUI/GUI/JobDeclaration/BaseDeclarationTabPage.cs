using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class BaseDeclarationTabPage : ZBindingTabPage
	{
		public event EventHandler LazyCreateControls;
		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			OnLazyCreateControls(EventArgs.Empty);
			base.SetDataBindingCore(dataSource, dataMember);
		}

		protected virtual void OnLazyCreateControls(EventArgs e)
		{
			if (LazyCreateControls != null)
			{
				LazyCreateControls(this, e);
			}
		}
	}

	public class LazyLoadedTabControl : ZTemplateTabControl
	{
		protected internal IBusiness GetTopLevelBusinessEntityForPlugInsInternal() => GetTopLevelBusinessEntityForPlugIns();
		protected override IBusiness GetTopLevelBusinessEntityForPlugIns() => Parent.JobDeclaration;

		protected internal void OnSelectedIndexChangedInternal(EventArgs e) => OnSelectedIndexChanged(e);
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (Parent != null)
				{
					if (SelectedTab == Parent.ContainerTabPage)
					{
						Parent.LoadContainerTabPage();
					}
					else if (SelectedTab == Parent.PackingTabPage)
					{
						Parent.LoadPackingTabPage();
					}
					else if (SelectedTab == Parent.InvoiceGroupingTabPage)
					{
						Parent.LoadInvoiceGroupingTabPage();
					}
					else if (SelectedTab == Parent.InvoicesTabPage)
					{
						Parent.LoadInvoicesTabPage();
					}
					else if (SelectedTab == Parent.MessagesTabPage)
					{
						Parent.LoadMessageTabPage();
					}
					else if (SelectedTab == Parent.InvoiceLinesTabPage)
					{
						Parent.LoadInvoiceLinesTabPage();
					}
					else if (SelectedTab == Parent.MiscOptionsTabPage)
					{
						Parent.LoadMiscOptionsUserControl();
					}
					else if (SelectedTab == Parent.EntryInstructionDetailsTabPage)
					{
						Parent.LoadEntryInstructionDetailsTabPage();
					}
					else if (SelectedTab == Parent.PickupTabPage)
					{
						Parent.LoadPickupTabPage();
					}
					else if (SelectedTab == Parent.DeliveryTabPage)
					{
						Parent.LoadDeliveryTabPage();
					}
				}
			}
			base.OnSelectedIndexChanged(e);
		}

		new BaseCustomsBrokerageUserControl Parent => (BaseCustomsBrokerageUserControl)base.Parent;
	}
}

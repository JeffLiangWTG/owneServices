using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class DeclarationInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public DeclarationInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		public virtual ICommonInvoiceDataProvider JobDeclaration
		{
			get { return fJobDeclaration; }
			set
			{
				if (fJobDeclaration != value)
				{
					if (declarationValueChangedAnnouncer != null)
					{
						declarationValueChangedAnnouncer.Dispose();
					}
					fJobDeclaration = value;

					declarationValueChangedAnnouncer = JobDeclaration.GetValueChangedAnnouncer();

					if (declarationValueChangedAnnouncer != null)
					{
						declarationValueChangedAnnouncer.OnValueChanged -= new EventHandler(JobDeclaration_ControlVisibilityChanged);
						declarationValueChangedAnnouncer.OnValueChanged += new EventHandler(JobDeclaration_ControlVisibilityChanged);
					}
					UpdateControlsVisiblity();
				}
			}
		}
		ICommonInvoiceDataProvider fJobDeclaration;

		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected virtual void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				UpdateControlsVisiblity();
			}
		}

		void UpdateControlsVisiblity()
		{
			ChangeGridColumnsVisibility();
			ChangeControlsVisibility();
		}

		protected virtual void ChangeGridColumnsVisibility()
		{
		}

		protected virtual void ChangeControlsVisibility()
		{
		}

		public void InitializeGridLayout()
		{
			InitializeGridLayoutCore();
		}

		protected virtual void InitializeGridLayoutCore()
		{
		}

		protected virtual bool SupportsBOMExpander
		{
			get { return false; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (SupportsBOMExpander && CurrentInvoiceLine != null)
			{
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e18ba2b3-a989-407d-9a8f-cdd58defaea7", "Expand this Line by Bill Of Materials"), new EventHandler(ExpandBOMLine)));
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("06583d14-3d44-4d7f-be49-7b87e0a54054", "Collapse Bill Of Materials of this Line (Remove Expanded Lines)"), new EventHandler(CollapseBOMLine)));
			}
		}

		void ExpandBOMLine(object sender, EventArgs args)
		{
			if (CurrentInvoiceLine != null)
			{
				JobDeclaration.ExpandOneBOMProductLine(CurrentInvoiceLine);
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("afc2b340-1fc3-498e-993a-eabf38975b5d", "Please select a row before attempting to Expand by Bill Of Materials."));
			}
		}

		void CollapseBOMLine(object sender, EventArgs args)
		{
			if (CurrentInvoiceLine != null)
			{
				JobDeclaration.CollapseOneBOMProductLine(CurrentInvoiceLine);
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("a123c27f-6695-4ea4-b257-83ca45f4f3d7", "Please select a row before attempting to Collapse Bill Of Materials."));
			}
		}
	}
}

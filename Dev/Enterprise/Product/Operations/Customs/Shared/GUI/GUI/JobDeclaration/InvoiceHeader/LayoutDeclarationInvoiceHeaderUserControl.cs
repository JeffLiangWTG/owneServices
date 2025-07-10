using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class LayoutDeclarationInvoiceHeaderUserControl : LayoutCustomsSupplierHeaderUserControl, IJobDeclarationGuiHandler
	{
		public LayoutDeclarationInvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		public virtual BaseJobDeclaration JobDeclaration
		{
			get { return fJobDeclaration; }
			set
			{
				if (fJobDeclaration != value)
				{
					UnHookDeclarationEvents(fJobDeclaration);
					if (declarationValueChangedAnnouncer != null)
					{
						declarationValueChangedAnnouncer.Dispose();
					}
					fJobDeclaration = value;
					HookDeclarationEvents(fJobDeclaration);
					declarationValueChangedAnnouncer = ((IInvoicesProvider)JobDeclaration).GetValueChangedAnnouncer();

					if (declarationValueChangedAnnouncer != null)
					{
						declarationValueChangedAnnouncer.OnValueChanged -= new EventHandler(JobDeclaration_ControlVisibilityChanged);
						declarationValueChangedAnnouncer.OnValueChanged += new EventHandler(JobDeclaration_ControlVisibilityChanged);
					}
					UpdateDeclarationFormLayoutProvider();
					UpdateControlsVisiblity();
					UpdateControlsVisiblityWhenJobDeclarationChanged();
				}
			}
		}
		BaseJobDeclaration fJobDeclaration;

		void UpdateDeclarationFormLayoutProvider()
		{
			declarationFormLayoutProvider = DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
		}

		IDeclarationFormLayoutProvider declarationFormLayoutProvider;

		protected virtual void UpdateControlsVisiblityWhenJobDeclarationChanged()
		{
		}

		void HookDeclarationEvents(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				UnHookDeclarationEvents(declaration);
				HookDeclarationEventsCore(declaration);
			}
		}

		protected virtual void HookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
		}

		void UnHookDeclarationEvents(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				UnHookDeclarationEventsCore(declaration);
			}
		}

		protected virtual void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
		}

		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
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
			if (declarationFormLayoutProvider == null)
			{
				return;
			}
			if (declarationFormLayoutProvider.GetInvoiceChargesGridColumnLayout(JobDeclaration) is { } invoiceChargesGridColumnLayout)
			{
				this.InvoiceChargesGrid.ApplyGridColumnLayout(invoiceChargesGridColumnLayout);
			}
			if (declarationFormLayoutProvider.GetApportionedInvoiceChargesGridColumnLayout(JobDeclaration) is { } apportionedInvoiceChargesGridColumnLayout)
			{
				this.ApportionedChargesGrid.ApplyGridColumnLayout(apportionedInvoiceChargesGridColumnLayout);
			}
			if (declarationFormLayoutProvider.GetGroupChargesGridColumnLayout(JobDeclaration) is { } groupChargesGridColumnLayout)
			{
				this.BaseGroupChargesGrid.ApplyGridColumnLayout(groupChargesGridColumnLayout);
			}
		}

		protected BaseJobComInvoiceHeader CurrentInvoiceHeader
		{
			get
			{
				BaseJobComInvoiceHeader invoiceHeader = null;
				var listManager = JobComInvoiceHeadersBoundGrid.InnerGrid.ListManager;
				if (listManager != null)
				{
					invoiceHeader = (BaseJobComInvoiceHeader)listManager.GetCurrent();
				}
				return invoiceHeader;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (declarationValueChangedAnnouncer != null)
				{
					declarationValueChangedAnnouncer.Dispose();
				}
				UnHookDeclarationEvents(JobDeclaration);
			}
			base.Dispose(disposing);
		}
	}
}

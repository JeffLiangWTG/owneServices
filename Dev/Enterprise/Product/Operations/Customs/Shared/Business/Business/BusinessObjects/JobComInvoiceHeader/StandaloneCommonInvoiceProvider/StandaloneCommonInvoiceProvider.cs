using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.Business
{
	public class StandaloneCommonInvoiceProvider : NonPersistentBusinessObject<StandaloneCommonInvoiceProviderValidation>, ICommonInvoiceDataProvider
	{
		public StandaloneCommonInvoiceProvider(BaseJobComInvoiceHeader invoice)
			: base(invoice.Factory)
		{
			this.invoice = Argument.NotNull(invoice, nameof(invoice));
		}

		public override StandaloneCommonInvoiceProviderValidation GetNewValidation() => new StandaloneCommonInvoiceProviderValidation(this);

		public void ExpandOneBOMProductLine(BaseJobComInvoiceLine invoiceLine)
		{
		}

		public void CollapseOneBOMProductLine(BaseJobComInvoiceLine invoiceLine)
		{
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer() => new StandaloneInvoiceValueChangedAnnouncer(invoice);

		protected readonly BaseJobComInvoiceHeader invoice;

		ICustomsFileParent ICommonInvoiceDataProvider.CustomsFileParent => invoice;
		bool ICommonInvoiceDataProvider.IsDeclarationIntegrated => false;

		public bool ApportionmentDirty { get; set; }

		public virtual ZBool IsExWarehouse => invoice.JZ_MessageType == JobMessageTypeList.Codes.ExWarehouse;

		public ZBool IsImport => invoice.IsImport;

		public ZBool IsExport => invoice.IsExport;

		public virtual ZString CustomsVATTypeCaption => ZString.Empty;

		public bool CopyLastLineDetailsToNewLines { get; set; }

		public IEnumerable<BaseJobComInvoiceLine> FilteredInvoiceLines => invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>();

		public IEnumerable<BaseJobComInvoiceHeader> Invoices => new[] { invoice };

		public ZString JE_MessageType { get => invoice.JZ_MessageType; set => invoice.JZ_MessageType = value; }

		public ZPropertyInfo JE_MessageTypeInfo => GetWrappedZPropertyInfo(nameof(JE_MessageType), (x) => invoice.JZ_MessageTypeInfo);

		public ZBool JE_CopyLastInvoiceLineDetailsToNewLines => CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.GetFallBackValueAtAllLevels(invoice.RegistryCompanyPK, Guid.Empty, Guid.Empty);

		public WeightApportionManager WeightApportionManager
		{
			get { return fWeightApportionManager ?? (fWeightApportionManager = new WeightApportionManager()); }
		}
		WeightApportionManager fWeightApportionManager;

		public ZBool JE_AutoWeightApportion { get; set; }
		void ICommonInvoiceDataProvider.ApportionWeightIfNeeded(IWeightHolder weightHolder, IWeightApportionee uncommittedApportionee)
		{
			if (WeightApportionmentEnabled && JE_AutoWeightApportion)
			{
				WeightApportionManager.ApportionAll(weightHolder, uncommittedApportionee);
			}
		}

		public IDisposable SuspendWeightApportionment()
		{
			return new WeightApportionmentSuspender(this);
		}

		public bool WeightApportionmentEnabled => weightApportionmentSuspendedCounter == 0;
		int weightApportionmentSuspendedCounter;

		class WeightApportionmentSuspender : IDisposable
		{
			public WeightApportionmentSuspender(StandaloneCommonInvoiceProvider provider)
			{
				this.provider = provider;
				this.provider.weightApportionmentSuspendedCounter++;
			}

			readonly StandaloneCommonInvoiceProvider provider;

			public void Dispose()
			{
				this.provider.weightApportionmentSuspendedCounter--;
			}
		}
	}
}

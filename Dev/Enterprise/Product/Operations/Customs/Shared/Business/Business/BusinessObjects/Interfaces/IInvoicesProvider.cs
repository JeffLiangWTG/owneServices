using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IInvoicesProvider : IInvoicesProviderValueChangedAnnouncerProvider
	{
		IEnumerable<BaseJobComInvoiceHeader> Invoices { get; }

		IInvoiceLineViewCollection<BaseJobComInvoiceLine> FilteredInvoiceLines { get; }
		bool ApportionmentDirty { get; }

		event BaseJobDeclaration.ApportionmentDirtyChangedEventHandler OnApportionmentDirtyChanged;
		event BaseJobDeclaration.ApportionmentProgressEventHandler OnApportionmentProgressChanged;

		event EventHandler OnContainerDetailsChanged;

		event EventHandler OnPartAttributeCaptionDetailsChanged;

		/// <summary>
		/// Value Changed event that affects Invoice Line tab visibility
		/// </summary>
		event EventHandler OnInvoiceLinesVisibilityChanged;

		/// <summary>
		/// The actual object needs to be a public interface and this interface member should be a proxy for binding
		/// </summary>
		IInvoicesProviderLookups Lookups { get; }

		bool ShouldCreateDummyInvoiceLinesForMerge { get; }

		OrgHeader Importer { get; }

		ZBool ContainersRequired { get; }

		void SelectContainerForAllInvoiceLines(NonPersistentCusContainer selectedContainer);
		void UnSelectContainerForAllInvoiceLines(NonPersistentCusContainer selectedContainer);

		ZPropertyInfo MessageTypeInfo { get; }
		OrgHeader NewOwner { get; }
		event EventHandler OnNewOwnerPartAttributeCaptionDetailsChanged;
	}

	public interface IInvoicesProviderValueChangedAnnouncer : IDisposable
	{
		event EventHandler OnValueChanged;
	}

	public interface IInvoicesProviderLookups
	{
		IBusinessObjectCollection InvoicesToAttach { get; }

		RefCurrencyCollection CurrencyList { get; }
	}
}

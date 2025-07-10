using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Global Commercial Invoice header business logic.
	/// </summary>
	/// <param name="factory"><see cref="BusinessObjectFactory"/> object instance.</param>
	/// <param name="row"><see cref="DataRow"/> containing the header data.</param>
	[CodeProperty(Schema.GIH_InvoiceNumber), DescriptionProperty(Schema.GIH_Description)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public sealed class GlobalCommercialInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: AutoGlobalCommercialInvoiceHeader(factory, row)
	{
		/// <summary>
		/// The collection of invoice headers.
		/// Initially, we assign a fake value because we cannot create a correct list at this point.
		/// See <see cref="GlobalCommercialInvoiceLineIntegratedCollection"/> where the real value is assigned.
		/// </summary>
		public GlobalCommercialInvoiceHeaderCollection Headers { get; set; } = GlobalCommercialInvoiceHeaderCollection.GetEmpty(factory);

		/// <summary>
		/// A method that will provide a value to <see cref="ProvideMetaDataPropertyAttribute"/>.
		/// </summary>
		/// <param name="property">Property to get.</param>
		/// <returns></returns>
		public bool GetReadOnlySecurity(PropertyDescriptor property) => InvoiceSecurityProvider.IsReadOnly(Factory, GIH_ParentID);

		/// <summary>
		/// Sets the class instance default values.
		/// </summary>
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			GIH_InvoiceDate = ZDate.Today;
			GIH_RX_NKInvoiceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		[List($"{nameof(Lookups)}.{nameof(Lookups.Importers)}")]
		public override ZGuid GIH_OH_Importer
		{
			get => base.GIH_OH_Importer;
			set => InvoiceOrgHeaderAddressLinkUpdater.SetOrgHeader(InvoiceOrgHeaderIndex.Importer, value);
		}

		[List($"{nameof(Lookups)}.{nameof(Lookups.Importers)}")]
		public override ZGuid GIH_OA_ImporterAddress
		{
			get => base.GIH_OA_ImporterAddress;
			set => InvoiceOrgHeaderAddressLinkUpdater.SetOrgAddress(InvoiceOrgHeaderIndex.Importer, value);
		}

		[List($"{nameof(Lookups)}.{nameof(Lookups.Suppliers)}")]
		public override ZGuid GIH_OH_Supplier
		{
			get => base.GIH_OH_Supplier;
			set => InvoiceOrgHeaderAddressLinkUpdater.SetOrgHeader(InvoiceOrgHeaderIndex.Supplier, value);
		}

		[List($"{nameof(Lookups)}.{nameof(Lookups.Suppliers)}")]
		public override ZGuid GIH_OA_SupplierAddress
		{
			get => base.GIH_OA_SupplierAddress;
			set => InvoiceOrgHeaderAddressLinkUpdater.SetOrgAddress(InvoiceOrgHeaderIndex.Supplier, value);
		}

		InvoiceOrgHeaderAddressLinkUpdater invoiceOrgHeaderAddressLinkUpdater;

		/// <summary>
		/// Used to update organization addresses.
		/// </summary>
		InvoiceOrgHeaderAddressLinkUpdater InvoiceOrgHeaderAddressLinkUpdater => invoiceOrgHeaderAddressLinkUpdater
			??= new InvoiceOrgHeaderAddressLinkUpdater(new Dictionary<InvoiceOrgHeaderIndex, InvoiceOrgHeaderAddressLink>
		{
			[InvoiceOrgHeaderIndex.Supplier] = new(GIH_OA_SupplierAddress_ZAddress, () => base.GIH_OH_Supplier, x => base.GIH_OH_Supplier = x, x => base.GIH_OA_SupplierAddress = x),
			[InvoiceOrgHeaderIndex.Importer] = new(GIH_OA_ImporterAddress_ZAddress, () => base.GIH_OH_Importer, x => base.GIH_OH_Importer = x, x => base.GIH_OA_ImporterAddress = x)
			});
	}
}

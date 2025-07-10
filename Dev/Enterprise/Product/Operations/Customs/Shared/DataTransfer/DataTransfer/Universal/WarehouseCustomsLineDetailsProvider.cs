using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class WarehouseCustomsLineDetailsProvider<TDeclaration, TInvoice> : WarehouseCustomsLineDetailsProvider
		where TDeclaration : BaseJobDeclaration
		where TInvoice : BaseJobComInvoiceHeader
	{
		protected WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected abstract ITableSchema GetDeclarationAddInfoSchema();

		protected abstract ITableSchema GetInvoiceAddInfoSchema();

		protected sealed override Dictionary<string, SchemaColumn> GetDeclarationSchemaDictionary()
		{
			var addInfoSchema = GetDeclarationAddInfoSchema();
			return addInfoSchema != null ? Factory.GetAddInfoSchemaDictionary<TDeclaration>(addInfoSchema) : null;
		}

		protected sealed override Dictionary<string, SchemaColumn> GetInvoiceSchemaDictionary()
		{
			var addInfoSchema = GetInvoiceAddInfoSchema();
			return addInfoSchema != null ? Factory.GetAddInfoSchemaDictionary<TInvoice>(addInfoSchema) : null;
		}
	}

	public class WarehouseCustomsLineDetailsProvider : IWarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
		{
			this.shipment = shipment;
		}
		protected readonly Shipment shipment;

		IEnumerable<IWarehouseCustomsLineDetails> IWarehouseCustomsLineDetailsProvider.GetLineDetails()
		{
			return GetLineDetails(shipment.CommercialInfo);
		}

		IEnumerable<IWarehouseCustomsLineDetails> GetLineDetails(CommercialInfo commercialInfo)
		{
			if (commercialInfo != null)
			{
				if (commercialInfo.CommercialInvoiceCollection != null)
				{
					foreach (var invoiceHeader in commercialInfo.CommercialInvoiceCollection)
					{
						if (invoiceHeader.CommercialInvoiceLineCollection != null)
						{
							foreach (var invoiceLine in invoiceHeader.CommercialInvoiceLineCollection)
							{
								if (invoiceLine.BondedWarehouseQuantity.HasValue)
								{
									yield return GetNewLineDetail(invoiceLine, invoiceHeader);
								}
							}
						}
					}
				}

				if (commercialInfo.SubGroupCollection != null)
				{
					foreach (var subGroup in commercialInfo.SubGroupCollection)
					{
						var supLineDetails = GetLineDetails(subGroup);
						if (supLineDetails != null)
						{
							foreach (var lineDetails in supLineDetails)
							{
								yield return lineDetails;
							}
						}
					}
				}
			}
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected virtual IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
		{
			return new WarehouseCustomsLineDetails(Factory, invoiceLine, GetFallbackDetail(invoice), shipment);
		}

		protected WarehouseCustomsFallbackDetail DeclarationDetail => declarationDetail ?? (declarationDetail = GetNewDeclarationDetail());
		WarehouseCustomsFallbackDetail declarationDetail;

		protected virtual WarehouseCustomsFallbackDetail GetNewDeclarationDetail()
		{
			var messageType = shipment.MessageType.GetCodeAsUpperCase();
			return new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = JobMessageTypeList.Codes.ExWarehouse == messageType,
				SupplierAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch(),
				InvoiceLineAddInfosApplicableForInwardWarehousing = InvoiceLineAddInfosApplicableForInwardWarehousing
			};
		}

		protected virtual WarehouseCustomsFallbackDetail GetFallbackDetail(CommercialInvoiceHeader invoice)
		{
			var result = DeclarationDetail.Clone();
			result.SupplierAddress = invoice.Supplier ?? (invoice.OrganizationAddressCollection.FindBestSupplierMatch() ?? result.SupplierAddress);
			result.FallbackAddInfos = GetFallbackAddInfos(invoice);
			return result;
		}

		IDictionary<ZString, UniversalAddInfo> GetFallbackAddInfos(CommercialInvoiceHeader invoice)
		{
			IDictionary<ZString, UniversalAddInfo> result = null;
			if (!FallBackAddInfosDictionary.TryGetValue(invoice, out result))
			{
				result = new Dictionary<ZString, UniversalAddInfo>();
				if (invoice.AddInfoCollection != null)
				{
					foreach (var invoiceLineProperty in InvoiceSchemaMatchingInvoiceLineSchema)
					{
						var addInfo = invoice.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == invoiceLineProperty);
						if (addInfo == null)
						{
							ShipmentAddInfosDictionary.TryGetValue(invoiceLineProperty, out addInfo);
						}
						if (addInfo != null)
						{
							result.Add(invoiceLineProperty, addInfo);
						}
					}
				}
				FallBackAddInfosDictionary.Add(invoice, result);
			}
			return result ?? new Dictionary<ZString, UniversalAddInfo>();
		}

		protected IEnumerable<string> InvoiceSchemaMatchingInvoiceLineSchema
		{
			get
			{
				if (invoiceSchemaMatchingInvoiceLineSchema == null)
				{
					invoiceSchemaMatchingInvoiceLineSchema = new List<string>();
					var invoiceSchemaDictionary = GetInvoiceSchemaDictionary();
					if (invoiceSchemaDictionary != null)
					{
						invoiceSchemaMatchingInvoiceLineSchema.AddRange(GetInvoiceLineAddInfosApplicableForInwardWarehousing().Where(x => invoiceSchemaDictionary.ContainsKey(x)));
					}
					invoiceSchemaMatchingInvoiceLineSchema.Sort();
				}
				return invoiceSchemaMatchingInvoiceLineSchema;
			}
		}
		List<string> invoiceSchemaMatchingInvoiceLineSchema;

		protected IEnumerable<string> InvoiceLineAddInfosApplicableForInwardWarehousing => invoiceLineAddInfosApplicableForInwardWarehousing ?? (invoiceLineAddInfosApplicableForInwardWarehousing = new List<string>(GetInvoiceLineAddInfosApplicableForInwardWarehousing()));
		List<string> invoiceLineAddInfosApplicableForInwardWarehousing;

		protected virtual IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing() => Enumerable.Empty<string>();

		protected virtual Dictionary<string, SchemaColumn> GetInvoiceSchemaDictionary() => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IDictionary<CommercialInvoiceHeader, IDictionary<ZString, UniversalAddInfo>> FallBackAddInfosDictionary => fallBackAddInfosDictionary ?? (fallBackAddInfosDictionary = new Dictionary<CommercialInvoiceHeader, IDictionary<ZString, UniversalAddInfo>>());
		IDictionary<CommercialInvoiceHeader, IDictionary<ZString, UniversalAddInfo>> fallBackAddInfosDictionary;

		protected IDictionary<ZString, UniversalAddInfo> ShipmentAddInfosDictionary
		{
			get
			{
				if (shipmentAddInfosDictionary == null)
				{
					shipmentAddInfosDictionary = new Dictionary<ZString, UniversalAddInfo>();
					if (shipment != null && shipment.AddInfoCollection != null)
					{
						var declarationSchemaDictionary = GetDeclarationSchemaDictionary();
						if (declarationSchemaDictionary != null)
						{
							foreach (var key in InvoiceSchemaMatchingInvoiceLineSchema)
							{
								if (declarationSchemaDictionary.ContainsKey(key))
								{
									var addInfo = shipment.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
									if (addInfo != null)
									{
										shipmentAddInfosDictionary.Add(key, addInfo);
									}
								}
							}
						}
					}
				}
				return shipmentAddInfosDictionary;
			}
		}
		IDictionary<ZString, UniversalAddInfo> shipmentAddInfosDictionary;

		protected virtual Dictionary<string, SchemaColumn> GetDeclarationSchemaDictionary() => null;
	}
}

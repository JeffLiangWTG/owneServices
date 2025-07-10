using System.Linq;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectReader : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectReader<JobComInvoiceHeader, JobComInvoiceGroupHeader>
	{
		public StandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, invoiceDataObject, logger, factory)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new UniversalDataObjectReaderHelper(factory, dataObject.GetTargetCountryCode());
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader);
		}

		protected override OrgHeader GetMatchedSupplier()
		{
			OrgHeader result = null;
			var supplierData = invoiceHeaderDataObject.Supplier;
			if (supplierData == null)
			{
				if (dataObject.MessageType.GetCodeAsUpperCase() == JobMessageTypeList.Codes.Export)
				{
					supplierData = GetMatchedSupplierBasedOnExport() ?? GetMatchedSupplierBasedOnImport();
				}
				else
				{
					supplierData = GetMatchedSupplierBasedOnImport() ?? GetMatchedSupplierBasedOnExport();
				}
				supplierData = supplierData ?? invoiceHeaderDataObject.OrganizationAddressCollection.FindBestSupplierMatch();
			}
			if (supplierData != null)
			{
				var supplierAddress = new OrganisationDataObjectReader(supplierData, logger, factory).GetMatched();
				result = supplierAddress == null ? null : supplierAddress.Header;
			}
			return result;
		}

		OrganizationAddress GetMatchedSupplierBasedOnImport()
		{
			return invoiceHeaderDataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Supplier,
				nameof(DocAddressType.Manufacturer),
				Constants.AddressType.Exporter,
				Constants.AddressType.Invoicer,
				Constants.AddressType.Seller,
				Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent,
				Constants.AddressType.FDAShipper,
				Constants.AddressType.ForeignExporter);
		}

		OrganizationAddress GetMatchedSupplierBasedOnExport()
		{
			return invoiceHeaderDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.USPrincipalPartyInInterest));
		}

		protected override OrgHeader GetMatchedBuyer()
		{
			var isExport = dataObject.MessageType.GetCodeAsUpperCase() == JobMessageTypeList.Codes.Export;
			OrgHeader result = null;
			OrganizationAddress buyerData = null;
			if (!isExport)
			{
				buyerData = invoiceHeaderDataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Importer);
			}
			buyerData = buyerData ?? invoiceHeaderDataObject.Buyer;
			if (buyerData == null)
			{
				if (isExport)
				{
					buyerData = GetMatchedBuyerBasedOnExport() ?? GetMatchedBuyerBasedOnImport();
				}
				else
				{
					buyerData = GetMatchedBuyerBasedOnImport() ?? GetMatchedBuyerBasedOnExport();
				}
				buyerData = buyerData ?? invoiceHeaderDataObject.OrganizationAddressCollection.FindBestImporterMatch();
			}
			if (buyerData != null)
			{
				var buyerAddress = new OrganisationDataObjectReader(buyerData, logger, factory).GetMatched();
				result = buyerAddress == null ? null : buyerAddress.Header;
			}
			return result;
		}

		OrganizationAddress GetMatchedBuyerBasedOnExport()
		{
			return invoiceHeaderDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.UltimateConsignee), Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);
		}

		OrganizationAddress GetMatchedBuyerBasedOnImport()
		{
			return invoiceHeaderDataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Importer,
				nameof(DocAddressType.BuyerDocumentaryAddress),
				nameof(DocAddressType.UltimateConsignee),
				Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent,
				Constants.AddressType.SoldToParty);
		}

		protected override bool ShouldJZ_OH_BuyerAndJZ_OA_BuyerAddressBelongingToSameOrganization => false;
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		internal CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override void PopulateCommercialInvoiceOrganizationAddressData(Customs.Business.BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var invoice = (JobComInvoiceHeader)invoiceBO;
			if (invoice.IsExport)
			{
				if (invoice.IsAttachedToPersistentDeclaration)
				{
					var docAddressWriterManager = new JobDocAddressDataObjectWriter(writeManager);
					invoiceData.Supplier = docAddressWriterManager.GetDataObject(invoice.USPPIDocAddress);
					invoiceData.Buyer = docAddressWriterManager.GetDataObject(invoice.UltimateConsigneeDocAddress);

					var additionalDocAddresses = invoice.DocAddresses.OfType<JobDocAddress>().Where(x => x.DocAddressType != DocAddressType.USPrincipalPartyInInterest && x.DocAddressType != DocAddressType.UltimateConsignee);
					invoiceData.OrganizationAddressCollection = ProcessCollection(additionalDocAddresses, new JobDocAddressDataObjectWriter(writeManager));
				}
				else
				{
					var address = helper.Load<OrgAddress>(invoice.JZ_OA_SupplierAddress);
					if (address == null)
					{
						invoiceData.Supplier = helper.CreateOrganizationAddressFromOrganization(writeManager, invoiceBO.Supplier, nameof(DocAddressType.USPrincipalPartyInInterest));
					}
					else
					{
						invoiceData.Supplier = helper.CreateOrganizationAddressFromAddress(writeManager, address, nameof(DocAddressType.USPrincipalPartyInInterest));
					}

					address = helper.Load<OrgAddress>(invoice.JZ_OA_BuyerAddress);
					if (address == null)
					{
						invoiceData.Buyer = helper.CreateOrganizationAddressFromOrganization(writeManager, invoice.Importer_Effective, nameof(DocAddressType.UltimateConsignee));
					}
					else
					{
						invoiceData.Buyer = helper.CreateOrganizationAddressFromAddress(writeManager, address, nameof(DocAddressType.UltimateConsignee));
					}
					address = helper.Load<OrgAddress>(invoice.JZ_OA_IntermediateConsigneeAddress);
					if (address == null)
					{
						PopulateConsignee(invoice, invoiceData);
					}
					else
					{
						invoiceData.AddOrgAddress(writeManager, address, nameof(DocAddressType.IntermediateConsignee));
					}
				}
			}
			else
			{
				var address = helper.Load<OrgAddress>(invoice.JZ_OA_SupplierAddress);
				if (address == null)
				{
					invoiceData.Supplier = helper.CreateOrganizationAddressFromOrganization(writeManager, invoiceBO.Supplier, AddressTypes.Supplier);
				}
				else
				{
					invoiceData.Supplier = helper.CreateOrganizationAddressFromAddress(writeManager, address, AddressTypes.Supplier);
				}
				invoiceData.Buyer = helper.CreateOrganizationAddressFromOrganization(writeManager, invoice.USBuyer, nameof(DocAddressType.BuyerDocumentaryAddress));

				if (invoice.IsImport)
				{
					PopulateManufacturer(invoice, invoiceData);
					PopulateSeller(invoice, invoiceData);
					PopulateShipToParty(invoice, invoiceData);
					PopulateSellingAgent(invoice, invoiceData);
					PopulateBuyerAgent(invoice, invoiceData);
					PopulateSoldToPartyAddress(invoice, invoiceData);
					PopulateConsigneeAddress(invoice, invoiceData);
					invoiceData.AddOrgAddress(writeManager, invoice.InvoicerDocAddress, Constants.AddressType.Invoicer);
					invoiceData.AddOrgAddress(writeManager, invoice.FDAShipperAddress, Constants.AddressType.FDAShipper);
					invoiceData.AddOrgAddress(writeManager, helper.Load<OrgHeader>(invoiceBO, JobComInvoiceHeaderSchema.JZ_OH_Buyer), AddressTypes.Importer);
					var declaration = invoice.JobDeclaration;
					if (declaration != null)
					{
						invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoice.JZ_OA_ExporterAddress), Constants.AddressType.Exporter);
					}
				}
			}
		}

		protected override void PopulateCommercialInvoiceLineOrganizationAddressData(Customs.Business.BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;
			var invoice = invoiceLine.InvoiceHeader;
			var declaration = invoice.JobDeclaration;
			var isACE = declaration != null && declaration.IsACE;
			if (invoice.IsImport)
			{
				// Not currently exposed on invoiceline JI_OA_FDAShipperAddress
				if (isACE)
				{
					invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLine.JI_OA_ExporterAddress), Constants.AddressType.Exporter);
					PopulateSoldToPartyAddress(invoiceLine, invoiceLineData);
				}
				PopulateManufacturer(invoiceLine, invoiceLineData);
				PopulateSeller(invoiceLine, invoiceLineData);
				PopulateShipToParty(invoiceLine, invoiceLineData);
				PopulateConsigneeAddress(invoiceLine, invoiceLineData);
			}

			PopulateDocAddresses(invoiceLine, invoiceLineData);
		}

		protected override UniversalCustoms.CommercialInvoiceHeader PopulateDataObject(Customs.Business.BaseJobComInvoiceHeader invoiceBO)
		{
			try
			{
				var declarationBO = invoiceBO.JobDeclaration as JobDeclaration;
				var isWarehouseEntryType = false;
				var isExWarehouseEntryType = false;
				var isConsumptionFTZ = false;
				shouldIncludeBondedWarehouseDetails = false;
				if (declarationBO != null && declarationBO.SupportsBondedWarehousing)
				{
					isWarehouseEntryType = declarationBO.IsWarehouseEntryType;
					isExWarehouseEntryType = declarationBO.IsExWarehouseEntryType;
					isConsumptionFTZ = declarationBO.IsENSFormalImportAndConsumptionFTZ;
					shouldIncludeBondedWarehouseDetails = isWarehouseEntryType || isExWarehouseEntryType || isConsumptionFTZ;
				}
				isOutwardData = isExWarehouseEntryType || isConsumptionFTZ;
				return base.PopulateDataObject(invoiceBO);
			}
			finally
			{
				shouldIncludeBondedWarehouseDetails = false;
			}
		}
		bool shouldIncludeBondedWarehouseDetails;
		bool isOutwardData;

		protected override System.Collections.Generic.List<UniversalDataBuss.DataObjects.Universal.AddInfo> GetInvoiceLineAddInfoCollection(Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);

			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;
			if (!invoiceLine.US_JI_ParentProduct.IsEmpty)
			{
				var parentLine = invoiceLine.Factory.Load<JobComInvoiceLine>(invoiceLine.US_JI_ParentProduct);
				if (parentLine != null)
				{
					helper.Update(result, Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo, parentLine.JI_LineNo);
				}
			}

			if (!invoiceLine.JI_CustomsSecondQuantity.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.CustomsSecondQuantity, invoiceLine.JI_CustomsSecondQuantity);
			}

			if (!invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.CustomsSecondQuantityUnit, invoiceLine.JI_CustomsSecondUnitQty);
			}

			if (!invoiceLine.JI_CustomsThirdQuantity.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.CustomsThirdQuantity, invoiceLine.JI_CustomsThirdQuantity);
			}

			if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.CustomsThirdQuantityUnit, invoiceLine.JI_CustomsThirdUnitQty);
			}
			return result;
		}

		protected override ZBool IsPopulateBondedWarehouseDetails(Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			var usInvoiceLineBO = (JobComInvoiceLine)invoiceLineBO;
			return shouldIncludeBondedWarehouseDetails && usInvoiceLineBO.JI_PartNo_CanBeSetByCustomer && usInvoiceLineBO.SupplierPart != null;
		}

		protected override void PopulateBondedWarehouseQuantityAndUnit(Customs.Business.BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			var usInvoiceLineBO = (JobComInvoiceLine)invoiceLineBO;
			if (isOutwardData)
			{
				invoiceLineData.BondedWarehouseQuantity = usInvoiceLineBO.JI_InvoiceQuantity;
				invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(usInvoiceLineBO.JI_InvoiceUQ, invoiceLineBO.Lookups.InvoiceUQList);
			}
			else if (!usInvoiceLineBO.JI_BondedWhsQuantity.IsEmpty || usInvoiceLineBO.WHSPackLines.Count > 0)
			{
				invoiceLineData.BondedWarehouseQuantity = usInvoiceLineBO.JI_BondedWhsQuantity.IsEmpty ? (ZDecimal)usInvoiceLineBO.WHSPackLines.Sum(x => x.US_PackedQty) : usInvoiceLineBO.JI_InvoiceQuantity;
				invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(usInvoiceLineBO.JI_InvoiceUQ, invoiceLineBO.Lookups.InvoiceUQList);
			}
		}
	}
}

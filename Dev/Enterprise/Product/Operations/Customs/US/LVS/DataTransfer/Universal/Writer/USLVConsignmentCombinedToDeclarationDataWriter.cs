using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsignmentCombinedToDeclarationDataWriter : USLVConsignmentToDeclarationDataWriter
	{
		public USLVConsignmentCombinedToDeclarationDataWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override void PopulateEntryType(UniversalShipment dataObject)
		{
		}

		protected override void PopulateTotalNoOfPacks(CusUSLVConsignment consignment, UniversalShipment dataObject)
		{
			var convertableConsignments = consignment.Shipment.CombinedConsignmentsToConvert;
			dataObject.TotalNoOfPacks = convertableConsignments.Sum(c => c.ULB_NumberOfPacks);
		}

		int packingLineIndexer = 1;

		protected override void PopulateCommercialInfoAndPackingLines(CusUSLVConsignment consignment, UniversalShipment dataObject)
		{
			var convertableConsignments = consignment.Shipment.CombinedConsignmentsToConvert;

			var packages = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };

			var commercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() };

			foreach (var convertableConsignment in convertableConsignments)
			{
				var packingLine = new PackingLine(writeManager.WriterStrategy)
				{
					BillNumber = convertableConsignment.ULB_HouseBill,
					BillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					},
					ContainerNumber = convertableConsignment.ULB_EquipmentNumber,
					Link = packingLineIndexer++,
					PackQty = new ZLong(convertableConsignment.ULB_NumberOfPacks),
					PackType = new PackageType()
					{
						Code = convertableConsignment.ULB_PackType,
						Description = convertableConsignment.ULB_PackType
					},
				};
				packingLine.SetPackedItemCollection(() => new List<PackedItem>());
				packages.Add(packingLine);

				var itemsGroupByCurrency = convertableConsignment.CusUSLVItems.OfType<CusUSLVItem>().GroupBy(t => t.ULI_RX_NKCurrency);
				var invoiceNo = convertableConsignment.ULB_HouseBill;
				foreach (var group in itemsGroupByCurrency)
				{
					var invoiceHeader = new CommercialInvoiceHeader(writeManager.WriterStrategy);
					invoiceHeader.InvoiceNumber = itemsGroupByCurrency.ToArray().Length > 1 ? ZString.Format("{0}{1}", invoiceNo, group.Key) : invoiceNo;
					invoiceHeader.InvoiceCurrency = new Currency() { Code = group.Key };
					PopulateInvoiceHeaderOrganizationAddressCollection(convertableConsignment, invoiceHeader);
					PopulateCommercialInvoiceAndPackedItems(group.ToList(), invoiceHeader, packingLine);
					commercialInfo.CommercialInvoiceCollection.Add(invoiceHeader);
				}
			}

			dataObject.SetPackingLineCollection(() => packages);

			dataObject.CommercialInfo = commercialInfo;
		}

		protected override void PopulateAdditionalBillCollection(CusUSLVConsignment sourceBO, UniversalShipment shipment)
		{
			if (shipment.AdditionalBillCollection != null || shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>()))
			{
				var convertableConsignments = sourceBO.Shipment.CombinedConsignmentsToConvert;

				foreach (var consignment in convertableConsignments)
				{
					var additionalBill = new AdditionalBill
					{
						BillNumber = consignment.ULB_HouseBill,
						BillType = new WayBillType
						{
							Code = WayBillTypeList.Codes.House,
							Description = WayBillTypeList.Descriptions.House,
						},
						ParentBillNumber = consignment.Shipment.ULH_MasterBill,
						NoOfPacks = (ZDecimal)consignment.ULB_NumberOfPacks,
						PackType = new PackageType()
						{
							Code = consignment.ULB_PackType,
							Description = consignment.ULB_PackType
						},
					};

					var listAddInfo = new List<UAddInfo>
					{
						new UAddInfo
						{
							Key = LVSConstants.AddInfoConstants.UI_NKBillIssuerSCAC,
							Value = consignment.ULB_HouseBillIssuerSCAC,
						}
					};
					additionalBill.AddInfoCollection = listAddInfo;

					shipment.AdditionalBillCollection.Add(additionalBill);
				}
			}
		}

		protected override void PopulateInvoiceHeaderOrganizationAddressCollection(CusUSLVConsignment sourceBO, CommercialInvoiceHeader invoiceHeader)
		{
			if (sourceBO.Consignee != null)
			{
				AddOrgAddress(invoiceHeader, writeManager, sourceBO.Consignee, Constants.AddressTypes.UltimateConsignee);
			}
			else if (!sourceBO.ULB_ConsigneeName.IsEmpty && !sourceBO.ULB_ConsigneeAddress1.IsEmpty)
			{
				AddOrgAddress(invoiceHeader, CreateOrganizationAddress(Constants.AddressTypes.UltimateConsignee, sourceBO.ULB_ConsigneeName, sourceBO.ULB_ConsigneeAddress1, sourceBO.ULB_ConsigneeAddress2, sourceBO.ULB_ConsigneeCity, sourceBO.ULB_ConsigneePostCode, sourceBO.ULB_ConsigneeState, Country.New(sourceBO.ConsigneeCountry)));
			}

			if (sourceBO.Seller != null)
			{
				AddOrgAddress(invoiceHeader, writeManager, sourceBO.Seller, AddressType.Seller);
			}
			else if (!sourceBO.ULB_SellerName.IsEmpty && !sourceBO.ULB_SellerAddress1.IsEmpty)
			{
				AddOrgAddress(invoiceHeader, CreateOrganizationAddress(AddressType.Seller, sourceBO.ULB_SellerName, sourceBO.ULB_SellerAddress1, sourceBO.ULB_SellerAddress2, sourceBO.ULB_SellerCity, sourceBO.ULB_SellerPostCode, sourceBO.ULB_SellerState, Country.New(sourceBO.SellerCountry)));
			}
		}

		void AddOrgAddress(IOrganizationAddressCollectionParent invoiceHeader, IDataWritingManager manager, OrgAddress orgAddress, ZString addressType)
		{
			if (orgAddress != null)
			{
				var addressDataObject = OrganizationAddressHelper.GetAddressDataObject(orgAddress, manager, addressType.ToString());
				AddOrgAddress(invoiceHeader, addressDataObject);
			}
		}

		void AddOrgAddress(IOrganizationAddressCollectionParent invoiceHeader, OrganizationAddress address)
		{
			if (invoiceHeader != null && address != null)
			{
				invoiceHeader.SetOrganizationAddressCollection(() => invoiceHeader.OrganizationAddressCollection ?? new List<OrganizationAddress>());
				invoiceHeader.OrganizationAddressCollection?.Add(address);
			}
		}
	}
}

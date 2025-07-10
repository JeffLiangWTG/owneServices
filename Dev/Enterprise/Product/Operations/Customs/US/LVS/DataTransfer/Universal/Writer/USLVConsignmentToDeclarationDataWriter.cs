using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsignmentToDeclarationDataWriter : USLVConsignmentDataWriter
	{
		public USLVConsignmentToDeclarationDataWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		#region Overrides

		protected override void InsertParents(CusUSLVConsignment sourceBO, ref UniversalShipment dataObject)
		{
			base.InsertParents(sourceBO, ref dataObject);
			dataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
		}

		protected override void PopulateDataObject(CusUSLVConsignment consignment, UniversalShipment dataObject)
		{
			base.PopulateDataObject(consignment, dataObject);
			PopulateMasterBillInfo(consignment, dataObject);
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					ReferenceNumber = consignment.CE_RailReferenceNumber,
					Type = new EntryType()
					{
						Code = ReferenceIdentifierCodeList.Codes.RailReferenceNumber
					}
				}
			});
			dataObject.GoodsDescription = LVSConstants.AddInfoConstants.MixedGoodsDescription;
			dataObject.IsPersonalEffects = ZBool.False;
			dataObject.MessageType = new CodeDescriptionPair() { Code = Common.US.USJobMessageTypeList.Codes.Import };
			dataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			PopulateEntryType(dataObject);
			dataObject.AddInfoCollection?.Add(AddInfo.New(LVSConstants.AddInfoConstants.HasMPF, YesNoList.Codes.No));
			dataObject.AddInfoCollection?.Add(AddInfo.New(Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration.MasterWayBillNumber, consignment.Shipment.ULH_MasterBill));
			dataObject.AddInfoCollection?.Add(AddInfo.New(LVSConstants.AddInfoConstants.EnableCRL, YesNoList.Codes.Yes));
		}

		void PopulateMasterBillInfo(CusUSLVConsignment consignment, UniversalShipment dataObject)
		{
			var clearanceDataWriter = new USLVClearanceDataWriter(writeManager);
			var shipment = consignment.Shipment;
			clearanceDataWriter.PopulateShipmentBasicInfos(shipment, dataObject);
			clearanceDataWriter.PopulateOrganizationAddressCollection(shipment, dataObject);
			dataObject.AddOrgAddress(writeManager, shipment.Importer, AddressTypes.Importer);
			clearanceDataWriter.PopulateAdditionalBillCollection(shipment, dataObject);
			clearanceDataWriter.PopulateAddInfoCollection(shipment, dataObject);
			clearanceDataWriter.PopulateDateCollection(shipment, dataObject);
		}

		protected override void PopulateCommercialInfoAndPackingLines(CusUSLVConsignment consignment, UniversalShipment dataObject)
		{
			var packages = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
			var packingLine = new PackingLine(writeManager.WriterStrategy)
			{
				BillNumber = consignment.ULB_HouseBill,
				BillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.House,
					Description = WayBillTypeList.Descriptions.House
				},
				ContainerNumber = consignment.ULB_EquipmentNumber,
				Link = 1,
				PackQty = new ZLong(consignment.ULB_NumberOfPacks),
				PackType = new PackageType()
				{
					Code = consignment.ULB_PackType
				},
			};
			packingLine.SetPackedItemCollection(() => new List<PackedItem>());
			packages.Add(packingLine);
			dataObject.SetPackingLineCollection(() => packages);

			var commercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() };
			commercialInfo.CommercialInvoiceCollection.AddRange(PopulateCommercialInvoices(consignment, packingLine));

			dataObject.CommercialInfo = commercialInfo;
		}

		public IEnumerable<CommercialInvoiceHeader> PopulateCommercialInvoices(CusUSLVConsignment consignment, PackingLine packingLine, int startingInvoiceNumber = 1)
		{
			var itemsGroupByCurrency = consignment.CusUSLVItems.OfType<CusUSLVItem>().GroupBy(t => t.ULI_RX_NKCurrency);
			var invoiceNo = startingInvoiceNumber;
			foreach (var group in itemsGroupByCurrency)
			{
				var invoiceHeader = new CommercialInvoiceHeader(writeManager.WriterStrategy);
				invoiceHeader.InvoiceNumber = invoiceNo.ToString(CultureInfo.InvariantCulture);
				invoiceNo++;
				invoiceHeader.InvoiceCurrency = new Currency() { Code = group.Key };
				PopulateCommercialInvoiceAndPackedItems(group.ToList(), invoiceHeader, packingLine);
				PopulateInvoiceHeaderOrganizationAddressCollection(consignment, invoiceHeader);

				invoiceHeader.SetAddInfoCollection(() =>
				{
					return new List<AddInfo>([new AddInfo() { Key = USAddInfoSchema.Constants.US_ReleaseEntryNumber.Substring(3), Value = consignment.Shipment.ULH_EntryFilerCode + consignment.CE_EntryNum }]);
				});

				yield return invoiceHeader;
			}
		}

		protected virtual void PopulateInvoiceHeaderOrganizationAddressCollection(CusUSLVConsignment consignment, CommercialInvoiceHeader invoiceHeader)
		{
			if (!consignment.ULB_OA_Seller.IsEmpty)
			{
				invoiceHeader.Supplier = OrganizationAddressHelper.GetAddressDataObject(consignment.Seller, writeManager, AddressTypes.Supplier);
			}

			if (!consignment.ULB_OA_Consignee.IsEmpty)
			{
				invoiceHeader.Buyer = OrganizationAddressHelper.GetAddressDataObject(consignment.Consignee, writeManager, AddressTypes.Recipient);
			}

			invoiceHeader.SetOrganizationAddressCollection(() =>
			{
				var result = new List<OrganizationAddress>();

				if (invoiceHeader.Supplier != null)
				{
					var exporter = invoiceHeader.Supplier.Clone() as OrganizationAddress;
					exporter.AddressType = US.DataTransfer.Universal.Constants.AddressType.Exporter;
					result.Add(exporter);

					var seller = invoiceHeader.Supplier.Clone() as OrganizationAddress;
					seller.AddressType = US.DataTransfer.Universal.Constants.AddressType.Seller;
					result.Add(seller);
				}

				if (invoiceHeader.Buyer != null)
				{
					var consignee = invoiceHeader.Buyer.Clone() as OrganizationAddress;
					consignee.AddressType = Customs.DataTransfer.Universal.Constants.AddressTypes.UltimateConsignee;
					result.Add(consignee);

					var soldToParty = consignee.Clone() as OrganizationAddress;
					soldToParty.AddressType = Customs.DataTransfer.Universal.Constants.AddressTypes.SoldToParty;
					result.Add(soldToParty);
				}

				return result;
			});
		}

		int invoiceLineLinkIndexer = 1;

		protected void PopulateCommercialInvoiceAndPackedItems(List<CusUSLVItem> items, CommercialInvoiceHeader invoice, PackingLine packingLine)
		{
			var collection = new DataObjectList<CommercialInvoiceLine>();
			var lineNo = 1;
			var sumGoodsValue = 0M;
			foreach (var item in items)
			{
				var invoiceLine = new CommercialInvoiceLine()
				{
					LineNo = lineNo++,
					Link = invoiceLineLinkIndexer,
					CountryOfOrigin = new Country() { Code = item.ULI_RN_NKCountryOfOrigin },
					Description = item.ULI_GoodsDescription,
					HarmonisedCode = item.ULI_Tariff,
					CustomsValue = item.ULI_GoodsValue,
					LinePrice = item.ULI_GoodsValue,
					PartNo = item.ULI_PartNo
				};

				sumGoodsValue += item.ULI_GoodsValue;

				packingLine?.PackedItemCollection?.Add(new PackedItem
				{
					CommercialInvoiceLineLink = invoiceLineLinkIndexer++
				});

				invoiceLine.AddInfoCollection = GetInvoiceLineAddInfo(item);
				collection.Add(invoiceLine);
			}

			invoice.SetCommercialInvoiceLineCollection(() => collection);
			invoice.InvoiceAmount = sumGoodsValue;
		}

		protected override List<AddInfo> PopulateAdditionalAddInfoFields(CusUSLVItem item)
		{
			var additionalList = base.PopulateAdditionalAddInfoFields(item);
			additionalList.Add(new AddInfo
			{
				Key = LVSConstants.AddInfoConstants.UC_NKCountryOfOrigin,
				Value = item.ULI_RN_NKCountryOfOrigin,
			});
			return additionalList;
		}

		protected override List<AddInfo> PopulatePGADisclaimReasonFields(CusUSLVItem cusUSLVItem)
		{
			var listAddInfo = new List<AddInfo>();
			foreach (var pgaWrapper in cusUSLVItem.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>())
			{
				if (!pgaWrapper.Requirement.IsEmpty)
				{
					var indicatorAddInfo = new AddInfo
					{
						Key = PGAHelper.GetIndicatorCode(pgaWrapper.Agency, pgaWrapper.AgencyProgram),
						Value = pgaWrapper.Indicator.IsEmpty ? new ZString(OGAIndicatorList.Codes.Declared) : pgaWrapper.Indicator,
					};
					listAddInfo.Add(indicatorAddInfo);

					if (!pgaWrapper.DisclaimReason.IsEmpty && pgaWrapper.PGA != null)
					{
						listAddInfo.Add(GenerateDisclaimReasonAddInfo(pgaWrapper.PGA));
					}
				}
			}

			return listAddInfo;
		}

		protected virtual void PopulateEntryType(UniversalShipment dataObject)
		{
			dataObject.AddInfoCollection.Add(AddInfo.New(LVSConstants.AddInfoConstants.EntryType, EntryTypeList.Codes.LowValue));
		}

		#endregion
	}
}

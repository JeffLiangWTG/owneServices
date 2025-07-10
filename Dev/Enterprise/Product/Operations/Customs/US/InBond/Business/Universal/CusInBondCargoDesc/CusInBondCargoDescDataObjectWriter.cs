using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondCargoDescDataObjectWriter : DataTransfer.Universal.CusInBondCargoDescDataObjectWriter
	{
		public CusInBondCargoDescDataObjectWriter(IDataWritingManager manager, Shipment headerData, InBondDataObjectWriterHelper helper, bool includeWarehouseData)
			: base(manager, helper)
		{
			this.headerData = Argument.NotNull(headerData, "headerData");
			this.includeWarehouseData = includeWarehouseData;
		}
		readonly Shipment headerData;
		readonly bool includeWarehouseData;

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
			base.PopulateInBondSpecificData(commodityBO, commodityData);
			var inBondCommodityBO = (CusInBondCargoDesc)commodityBO;
			commodityData.PackQty = new ZLong(inBondCommodityBO.BY_PieceCount);
			commodityData.PackType = ListHelper.GetWithDescription<PackageType>(inBondCommodityBO.BY_ManifestUnitCode, inBondCommodityBO.Lookups.ManifestUnitList);
			commodityData.Weight = inBondCommodityBO.BY_GrossWeight;
			commodityData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(inBondCommodityBO.BY_GrossWeightUnit, inBondCommodityBO.Lookups.WeightUnitList);
			PopulateChildCommodities(inBondCommodityBO, commodityData);
		}

		protected override List<PackedItem> PopulatePackedItemsData(Customs.Business.CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
			var inBondCommodityBO = (CusInBondCargoDesc)commodityBO;
			return new List<PackedItem>(GetPackedItems(inBondCommodityBO));
		}

		void PopulateChildCommodities(CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
			commodityData.SetPackingLineCollection(() =>
			{
				var list = new List<PackingLine>();
				var commodityWriter = new CusInBondCargoDescDataObjectWriter(writeManager, headerData, Helper, includeWarehouseData);
				var childCommodities = Helper.Load<CusInBondCargoDesc>(commodityBO.ChildCommodities.CompleteFilter);
				foreach (var childCommodityBO in childCommodities.OrderBy(x => CusInBondCargoDescComparer.GetOrderKey(x)))
				{
					var childCommodityData = commodityWriter.GetDataObject(childCommodityBO);
					list.Add(childCommodityData);
				}
				return list;
			});
		}

		IEnumerable<PackedItem> GetPackedItems(CusInBondCargoDesc commodityBO)
		{
			yield return new PackedItem()
			{
				CommercialInvoiceLineLink = GetCommercialInvoiceLineLink(commodityBO)
			};
		}

		ZInt? GetCommercialInvoiceLineLink(CusInBondCargoDesc commodityBO)
		{
			if (headerData.CommercialInfo.CommercialInvoiceCollection != null && headerData.CommercialInfo.CommercialInvoiceCollection.Count > 0)
			{
				var commercialInvoiceLineCollection = headerData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
				if (commercialInvoiceLineCollection != null)
				{
					var link = commercialInvoiceLineCollection.Count + 1;
					var partNumber = commodityBO.BY_PartNumber;
					var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(writeManager.WriterStrategy)
					{
						Link = link,
						LineNo = link,
						PartNo = partNumber,
						InvoiceQuantity = commodityBO.BY_InvoiceQuantity,
						OrganizationAddressCollection = new List<OrganizationAddress>()
					};
					if (includeWarehouseData && !partNumber.IsEmpty)
					{
						invoiceLineData.BondedWarehouseQuantity = invoiceLineData.InvoiceQuantity;
					}
					var supplier = commodityBO.Supplier;
					if (supplier != null)
					{
						invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);
					}
					var addInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>();
					Helper.Update(addInfoCollection, JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3), commodityBO.BY_WarehouseEntryLineNo);
					Helper.Update(addInfoCollection, JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), commodityBO.BY_WarehouseEntryNumber);
					invoiceLineData.AddInfoCollection = addInfoCollection;
					CustomLabelsCustomizedFieldDataObjectWriter.Write(CusInBondCargoDescSchema.Instance, commodityBO, invoiceLineData, Helper.GetCusInBondCargoDescCustomLabelsProvider());
					commercialInvoiceLineCollection.Add(invoiceLineData);
					return link;
				}
			}

			return null;
		}
	}
}

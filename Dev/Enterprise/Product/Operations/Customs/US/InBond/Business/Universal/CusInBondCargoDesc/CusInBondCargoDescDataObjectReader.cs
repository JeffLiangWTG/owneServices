using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondCargoDescDataObjectReader : DataTransfer.Universal.CusInBondCargoDescDataObjectReader<CusInBondCargoDesc>
	{
		public CusInBondCargoDescDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid containerPK)
			: base(dataObject, logger, helper, containerPK)
		{
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override void FillInBondSpecificData(IColumnIndexer commodityRow, Dictionary<string, ValueSetter> delaySetters, CusInBondCargoDesc commodityBO)
		{
			base.FillInBondSpecificData(commodityRow, delaySetters, commodityBO);
			PopulateCommercialRelatedData(dataObject, commodityBO, commodityRow, delaySetters);
		}

		protected override void PopulateChildCommodities(CusInBondCargoDesc commodityBO)
		{
			if (dataObject.PackingLineCollection != null)
			{
				var commodityRow = GetColumnIndexer(commodityBO);
				var commodityPK = commodityRow.GetValue(CusInBondCargoDescSchema.PK);
				var childCommodiesAddedBySystem = new List<CusInBondCargoDesc>();
				var parentCommodityHasPart = !commodityRow.GetValue(CusInBondCargoDescSchema.BY_PartNumber).IsEmpty;
				if (parentCommodityHasPart)
				{
					childCommodiesAddedBySystem.AddRange(factory.Load<CusInBondCargoDesc>(new ZQuery(CusInBondCargoDescSchema.BY_ParentID, commodityPK)));
				}
				var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
				foreach (var childPackingLineData in dataObject.PackingLineCollection)
				{
					if (delaySetters != null)
					{
						delaySetters.Clear();
					}
					var childCommodityBO = childCommodiesAddedBySystem.FirstOrDefault(x => x.BY_HarmonisedTariff == childPackingLineData.HarmonisedCode.GetValueOrDefault());
					if (childCommodityBO == null)
					{
						childCommodityBO = GetNewBusinessObject();
					}
					else
					{
						childCommodiesAddedBySystem.Remove(childCommodityBO);
					}
					var childCommodityRow = GetColumnIndexer(childCommodityBO);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_ParentID, commodityPK);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_ParentTableCode, CusInBondCargoDescSchema.Constants.Prefix);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, childPackingLineData.HarmonisedCode, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_MonetaryValue, childPackingLineData.LinePrice, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_GrossWeight, childPackingLineData.Weight, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, childPackingLineData.WeightUnit, delaySetters);
					if (!parentCommodityHasPart)
					{
						PopulateCommercialRelatedData(childPackingLineData, childCommodityBO, childCommodityRow, delaySetters);
					}
					delaySetters.SetValueInSpecificOrder(GetSettingOrder(childCommodityBO));
					if (!parentCommodityHasPart)
					{
						PopulateChildOfChildCommodities(childCommodityBO, childPackingLineData.PackingLineCollection);
					}
					commodityBO.ChildCommodities.Add(childCommodityBO);
				}
			}
		}

		void PopulateChildOfChildCommodities(CusInBondCargoDesc commodityBO, List<PackingLine> packingLineCollection)
		{
			if (packingLineCollection != null)
			{
				var commodityRow = GetColumnIndexer(commodityBO);
				var commodityPK = commodityRow.GetValue(CusInBondCargoDescSchema.PK);
				var childCommodiesAddedBySystem = new List<CusInBondCargoDesc>();
				if (!commodityRow.GetValue(CusInBondCargoDescSchema.BY_OP_Part).IsEmpty)
				{
					childCommodiesAddedBySystem.AddRange(factory.Load<CusInBondCargoDesc>(new ZQuery(CusInBondCargoDescSchema.BY_ParentID, commodityPK)));
				}
				var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
				foreach (var childPackingLineData in packingLineCollection)
				{
					if (delaySetters != null)
					{
						delaySetters.Clear();
					}
					var childCommodityBO = childCommodiesAddedBySystem.FirstOrDefault(x => x.BY_HarmonisedTariff == childPackingLineData.HarmonisedCode.GetValueOrDefault());
					if (childCommodityBO == null)
					{
						childCommodityBO = GetNewBusinessObject();
					}
					else
					{
						childCommodiesAddedBySystem.Remove(childCommodityBO);
					}
					var childCommodityRow = GetColumnIndexer(childCommodityBO);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_ParentID, commodityPK);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_ParentTableCode, CusInBondCargoDescSchema.Constants.Prefix);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, childPackingLineData.HarmonisedCode, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_MonetaryValue, childPackingLineData.LinePrice, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_GrossWeight, childPackingLineData.Weight, delaySetters);
					SetValue(childCommodityRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, childPackingLineData.WeightUnit, delaySetters);
					delaySetters.SetValueInSpecificOrder(GetSettingOrder(childCommodityBO));
					commodityBO.ChildCommodities.Add(childCommodityBO);
				}
			}
		}

		protected override IEnumerable<ZString> GetSettingOrder(CusInBondCargoDesc commodity)
		{
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_OH_Supplier);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PartNumber);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PartAttrib1);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PartAttrib2);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PartAttrib3);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_SerialNumber);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_InvoiceQuantity);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_WarehouseEntryNumber);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_WarehouseEntryLineNo);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PieceCount);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_ManifestUnitCode);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_HarmonisedTariff);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_MonetaryValue);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_Description);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_GrossWeight);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_GrossWeightUnit);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_MarksAndNumbers);
		}

		void PopulateCommercialRelatedData(PackingLine packingLineData, CusInBondCargoDesc commodityBO, IColumnIndexer commodityRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var commercialInvoiceLineData = GetCommercialInvoiceLineData(packingLineData);
			if (commercialInvoiceLineData == null)
			{
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_OH_Supplier, ZGuid.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PartNumber, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PartAttrib1, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PartAttrib2, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PartAttrib3, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_SerialNumber, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_InvoiceQuantity, ZDecimal.Zero, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_WarehouseEntryNumber, ZString.Empty, delaySetters);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_WarehouseEntryLineNo, ZShort.Zero, delaySetters);
			}
			else
			{
				FillSupplier(commodityRow, commercialInvoiceLineData, delaySetters);
				var partNo = commercialInvoiceLineData.PartNo;
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PartNumber, partNo, delaySetters);
				var customLabelsProvider = Helper.GetCusInBondCargoDescCustomLabelsProvider();
				if (customLabelsProvider != null)
				{
					new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(CusInBondCargoDescSchema.Instance, commodityRow, commercialInvoiceLineData, customLabelsProvider, delaySetters);
				}
				if (!IsDefaultingEnabled && !partNo.GetValueOrDefault().IsEmpty)
				{
					var supplier = Helper.Load<OrgHeader>(commodityRow, CusInBondCargoDescSchema.BY_OH_Supplier);
					OrgHeader buyer = null;
					var containerRow = GetColumnIndexer(Helper.Load<CusInBondContainer>(ContainerPK));
					var moveDetailRow = containerRow == null ? null : GetColumnIndexer(Helper.Load<CusInBondMoveDetail>(containerRow, CusInBondContainerSchema.BC_ParentID));
					var moveHeaderRow = moveDetailRow == null ? null : GetColumnIndexer(Helper.Load<CusInBondMoveHeader>(moveDetailRow, CusInBondMoveDetailSchema.B9_BM));
					var headerRow = moveHeaderRow == null ? null : GetColumnIndexer(Helper.Load<CusInBondHeader>(moveHeaderRow, CusInBondMoveHeaderSchema.BM_BH));
					if (headerRow != null)
					{
						var buyerAddressRow = GetColumnIndexer(Helper.Load<OrgAddress>(headerRow, CusInBondHeaderSchema.BH_OA_Importer));
						buyer = buyerAddressRow == null ? null : Helper.Load<OrgHeader>(buyerAddressRow, OrgAddressSchema.OA_OH);
						if (supplier == null)
						{
							supplier = Helper.Load<OrgHeader>(headerRow, CusInBondHeaderSchema.BH_OH_Supplier);
						}
					}
					var part = new Customs.Business.OrgSupplierPart.Loader(commodityBO.Factory, typeof(US.Business.OrgSupplierPart)).Load(partNo.GetValueOrDefault(), buyer, supplier);
					if (part != null)
					{
						SetValue(commodityRow, CusInBondCargoDescSchema.BY_OP_Part, part.PK);
					}
				}
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_InvoiceQuantity, commercialInvoiceLineData.InvoiceQuantity, delaySetters);
				FillEntryDetails(commodityRow, commercialInvoiceLineData, delaySetters);
			}
		}

		UniversalCustoms.CommercialInvoiceLine GetCommercialInvoiceLineData(PackingLine packingLineData)
		{
			UniversalCustoms.CommercialInvoiceLine result = null;
			if (packingLineData.PackedItemCollection != null)
			{
				var packedItem = packingLineData.PackedItemCollection.FirstOrDefault();
				if (packedItem != null)
				{
					result = Helper.GetCommercialInvoiceLine(packedItem.CommercialInvoiceLineLink);
				}
			}
			return result;
		}

		void FillSupplier(IColumnIndexer commodityRow, UniversalCustoms.CommercialInvoiceLine commercialInvoiceLineData, Dictionary<string, ValueSetter> delaySetters)
		{
			if (commercialInvoiceLineData.OrganizationAddressCollection != null)
			{
				var supplierAddress = commercialInvoiceLineData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.SupplierDocumentaryAddress));
				if (supplierAddress != null)
				{
					var reader = new OrganisationDataObjectReader(supplierAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					SetValue(commodityRow, CusInBondCargoDescSchema.BY_OH_Supplier, orgAddress == null ? ZGuid.Empty : orgAddress.OA_OH, delaySetters);
				}
			}
		}

		void FillEntryDetails(IColumnIndexer commodityRow, UniversalCustoms.CommercialInvoiceLine commercialInvoiceLineData, Dictionary<string, ValueSetter> delaySetters)
		{
			if (commercialInvoiceLineData.AddInfoCollection != null)
			{
				var whsEntryFilerCode = commercialInvoiceLineData.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3));
				var whsEntryNumber = commercialInvoiceLineData.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryNumber.Substring(3)).GetValueOrDefault();
				ZString? warehouseEntryNumber = null;
				if (whsEntryFilerCode.HasValue)
				{
					warehouseEntryNumber = whsEntryFilerCode.Value + "-" + whsEntryNumber;
				}
				else if (!whsEntryNumber.IsEmpty)
				{
					warehouseEntryNumber = whsEntryNumber;
				}
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_WarehouseEntryNumber, warehouseEntryNumber, delaySetters);

				var whsEntryLineNo = commercialInvoiceLineData.AddInfoCollection.GetZShortValue(JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_WarehouseEntryLineNo, whsEntryLineNo, delaySetters);
			}
		}
	}
}

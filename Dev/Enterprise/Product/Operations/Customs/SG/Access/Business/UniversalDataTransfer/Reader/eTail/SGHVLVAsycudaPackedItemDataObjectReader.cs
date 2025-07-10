using CargoWise.Common;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGHVLVAsycudaPackedItemDataObjectReader : DataObjectReader<PackedItem, ASYCUDA.Business.AsycudaPackedItem>
	{
		public SGHVLVAsycudaPackedItemDataObjectReader(PackedItem dataObject, CommercialInvoiceLine commercialInvoiceLine, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaPack pack)
			: base(dataObject, logger, factory)
		{
			this.pack = Argument.NotNull(pack, "pack");
			this.commercialInvoiceLine = commercialInvoiceLine;
		}
		readonly ASYCUDA.Business.AsycudaPack pack;
		readonly CommercialInvoiceLine commercialInvoiceLine;

		protected override ASYCUDA.Business.AsycudaPackedItem GetExistingBusinessObject() => pack.PackedItem;

		protected override ASYCUDA.Business.AsycudaPackedItem GetNewBusinessObject() => (AsycudaPackedItem)pack.PackedItems.AddNewPackedItem();

		protected override void PopulateBusinessObject(ASYCUDA.Business.AsycudaPackedItem packedItem)
		{
			var packedItemRow = GetColumnIndexer(packedItem);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsDescription, dataObject.Description);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsQty, dataObject.PackedQuantity);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsUQ, UnitOfQuantityCodeList.Codes.NMB);
			if (commercialInvoiceLine != null)
			{
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, commercialInvoiceLine.CountryOfOrigin);
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsValue, commercialInvoiceLine.CustomsValue);
			}
		}
	}
}

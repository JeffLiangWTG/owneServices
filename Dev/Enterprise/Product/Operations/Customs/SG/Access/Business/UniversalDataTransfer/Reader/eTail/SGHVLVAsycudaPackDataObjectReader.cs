using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGHVLVAsycudaPackDataObjectReader : DataObjectReader<PackingLine, ASYCUDA.Business.AsycudaPack>
	{
		public SGHVLVAsycudaPackDataObjectReader(PackingLine dataObject, PackedItem packedItemDataObject, CommercialInvoiceLine commercialInvoiceLine, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill)
			: base(dataObject, logger, factory)
		{
			this.bill = Argument.NotNull(bill, "bill");
			this.packeditem = packedItemDataObject;
			this.commercialInvoiceLine = commercialInvoiceLine;
		}
		readonly PackedItem packeditem;
		readonly CommercialInvoiceLine commercialInvoiceLine;
		readonly ASYCUDA.Business.AsycudaBill bill;

		protected override ASYCUDA.Business.AsycudaPack GetExistingBusinessObject()
		{
			return null;
		}

		protected override ASYCUDA.Business.AsycudaPack GetNewBusinessObject()
		{
			return bill.Packs.AddNew();
		}

		protected override void PopulateBusinessObject(ASYCUDA.Business.AsycudaPack pack)
		{
			var packingLineRow = GetColumnIndexer(pack);
			if (!pack.HasManifestBeenSubmittedToCustomsIncludingChildren)
			{
				SetValue(packingLineRow, AsycudaPackSchema.APA_PackUQ, dataObject.PackType);
				SetValue(packingLineRow, AsycudaPackSchema.APA_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription());
				SetValue(packingLineRow, AsycudaPackSchema.APA_Weight, dataObject.Weight);
				SetValue(packingLineRow, AsycudaPackSchema.APA_WeightUQ, dataObject.WeightUnit);
				SetValue(packingLineRow, AsycudaPackSchema.APA_Volume, dataObject.Volume);
				SetValue(packingLineRow, AsycudaPackSchema.APA_VolumeUQ, dataObject.VolumeUnit);

				if (packeditem != null)
				{
					SetValue(packingLineRow, AsycudaPackSchema.APA_PackQty, packeditem.PackedQuantity?.ToZInt() ?? ZInt.Zero);

					new SGHVLVAsycudaPackedItemDataObjectReader(packeditem, commercialInvoiceLine, logger, factory, pack).ReadIntoBusinessObject();
				}
			}
		}
	}
}

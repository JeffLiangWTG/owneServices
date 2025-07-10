using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaBillDataObjectWriter : DataObjectWriter<AsycudaBill, Shipment>
	{
		public AsycudaBillDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "Helper");
		}

		protected override Shipment PopulateDataObject(AsycudaBill billBO)
		{
			if (billBO != null)
			{
				var uxmlBillData = new Shipment(writeManager.WriterStrategy);
				var helper = new AsycudaManifestUniversalCommonHelper(billBO.Factory);
				uxmlBillData.WayBillNumber = billBO.ABL_BillNumber;
				uxmlBillData.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

				PopulateAsycudaBillAddInfos(billBO, uxmlBillData);

				uxmlBillData.SetPackingLineCollection(() => CreatePackingLineCollection(helper.Load<AsycudaPack>(billBO.Packs?.CompleteFilter).OfType<AsycudaPack>()));

				return uxmlBillData;
			}
			return null;
		}

		protected DataObjectList<PackingLine> CreatePackingLineCollection(IEnumerable<AsycudaPack> packs)
		{
			if (packs != null)
			{
				var writer = new AsycudaPackDataObjectWriter(writeManager, helper);
				var list = new DataObjectList<PackingLine>(packs.Select(p => writer.GetDataObject(p)));
				list.Content = CollectionContent.Complete;
				return list;
			}
			return null;
		}

		protected void PopulateAsycudaBillAddInfos(AsycudaBill billBO, Shipment billData)
		{
			if (billBO != null)
			{
				billData.SetAddInfoCollection(() =>
				{
					var list = new List<AddInfo>();
					list.AddOrUpdate(AddInfoConstants.AsycudaBill.BillIssuer, billBO.ABL_BillIssuer);
					list.AddOrUpdate(AddInfoConstants.AsycudaBill.MRN, billBO.MRN);
					if (billBO.Header.AMA_Nature == NatureList.Codes.Export22)
					{
						list.AddOrUpdate(AddInfoConstants.AsycudaBill.CustomsCPC, billBO.CustomsCPC);
						list.AddOrUpdate(AddInfoConstants.AsycudaBill.LRN, billBO.LRN);
					}
					list.AddOrUpdate(AddInfoConstants.AsycudaBill.BillStatus, billBO.ABL_BillStatus);
					return list;
				});
			}
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;
	}
}

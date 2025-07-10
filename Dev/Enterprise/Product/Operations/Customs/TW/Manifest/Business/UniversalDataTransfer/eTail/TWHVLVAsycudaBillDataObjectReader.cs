using System.Linq;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer
{
	sealed class TWHVLVAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public TWHVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			var billRow = GetColumnIndexer(bill);
			helper.FilterAndSet(AsycudaBillSchema.ABL_Remarks, dataObject, (column) => SetValue(billRow, column, PopulateGoodsDescription()));
			helper.FilterAndSet(AsycudaBillSchema.ABL_UCRNumber, dataObject, (column) => SetValue(billRow, column, GetConsignmentIdFromDataContext()));
			helper.FilterAndSet(AsycudaBillSchema.ABL_RL_NKPortOfLoading, dataObject, (column) => SetValue(billRow, column, header.MasterBill.ABL_RL_NKPortOfLoading));

			FillArrivalDetails(bill);
		}

		string PopulateGoodsDescription()
		{
			var packLine = dataObject.PackingLineCollection.FirstOrDefault();
			var goodsDescription = packLine?.GetCleanSingleLineGoodsDescription();
			if (string.IsNullOrEmpty(goodsDescription))
			{
				goodsDescription = dataObject.GoodsDescription;
			}

			return goodsDescription;
		}

		string GetConsignmentIdFromDataContext()
		{
			var hvlvConsignmentDataSource = dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment);

			return hvlvConsignmentDataSource?.Key;
		}

		void FillArrivalDetails(ASYCUDA.Business.AsycudaBill bill)
		{
			var arrivalHeader = header.ArrivalHeaders.FirstOrDefault() ?? header.ArrivalHeaders.AddNew();
			var arrivalLine = arrivalHeader.ArrivalDetails.FirstOrDefault(l => l.ATL_ABL_AsycudaBill == bill.PK) ?? arrivalHeader.ArrivalDetails.AddNew();

			var arrivalLineRow = GetColumnIndexer(arrivalLine);
			SetValue(arrivalLineRow, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK);
			SetValue(arrivalLineRow, AsycudaArrivalLineSchema.ATL_Quantity, dataObject.TotalNoOfPacks);
		}

		protected override AsycudaPackDataObjectReader GetAsycudaPackDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new TWHVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, factory, bill, helper, isUpdateEnabled, dataObject);
		}
	}
}

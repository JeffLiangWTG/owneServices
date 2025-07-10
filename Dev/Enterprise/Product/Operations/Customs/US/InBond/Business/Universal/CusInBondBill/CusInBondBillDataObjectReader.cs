using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondBillDataObjectReader : DataTransfer.Universal.CusInBondBillDataObjectReader<CusInBondBill>
	{
		public CusInBondBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, InBondDataObjectReaderHelper helper, CusInBondHeader header)
			: base(additionalBillDataObject, logger, factory, helper, header)
		{
		}

		protected override void FillDataFromAddInfos(IColumnIndexer billRow)
		{
			base.FillDataFromAddInfos(billRow);
			if (dataObject.AddInfoCollection != null)
			{
				SetValue(billRow, CusInBondBillSchema.B0_IssuerCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.IssuerCode));
				SetValue(billRow, CusInBondBillSchema.B0_PortOfLadingKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfLadingScheduleK));
				SetValue(billRow, CusInBondBillSchema.B0_ManifestUQ, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ManifestUnit));
				SetValue(billRow, CusInBondBillSchema.B0_Weight, dataObject.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Weight));
				SetValue(billRow, CusInBondBillSchema.B0_WeightUQ, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.WeightUnit));
				SetValue(billRow, CusInBondBillSchema.B0_Volume, dataObject.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Volume));
				SetValue(billRow, CusInBondBillSchema.B0_VolumeUQ, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.VolumeUnit));
				SetValue(billRow, CusInBondBillSchema.B0_PlaceOfReceipt, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PlaceOfReceiptScheduleD));
			}
		}

		protected override void FillCustomsReferenceData(IColumnIndexer billRow)
		{
			base.FillCustomsReferenceData(billRow);
			var customsReferenceCollection = dataObject.CustomsReferenceCollection;
			if (customsReferenceCollection != null && customsReferenceCollection.Count > 0)
			{
				var additionalReferenceCollection = customsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.Type);
				if (additionalReferenceCollection.Any())
				{
					var billPK = billRow.GetValue(CusInBondBillSchema.PK);
					factory.Load<CusInbondBillAddRef>(new ZQuery(CusInbondBillAddRefSchema.BR_B0, billPK)).DeleteAll();
					foreach (var additionalReference in additionalReferenceCollection)
					{
						var additionalReferenceRow = GetColumnIndexer(factory.New<CusInbondBillAddRef>());
						SetValue(additionalReferenceRow, CusInbondBillAddRefSchema.BR_B0, billPK);
						SetValue(additionalReferenceRow, CusInbondBillAddRefSchema.BR_Qualifier, additionalReference.SubType);
						SetValue(additionalReferenceRow, CusInbondBillAddRefSchema.BR_ReferenceNum, additionalReference.Reference);
					}
				}
			}
		}

		protected override IEnumerable<ZString> GetBillSettingOrder(CusInBondBill bill)
		{
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_IssuerCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_MasterBillNumber);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_HouseBillNumber);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_PortOfLadingKCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ManifestQty);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ManifestUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_Weight);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_WeightUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_Volume);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_VolumeUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_PlaceOfReceipt);
		}
	}
}

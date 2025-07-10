using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondBillDataObjectReader : DataTransfer.Universal.CusInBondBillDataObjectReader<CusInBondBill>
	{
		public CusInBondBillDataObjectReader(Shipment parentDataObject, AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, InBondDataObjectReaderHelper helper, CusInBondHeader header)
			: base(additionalBillDataObject, logger, factory, helper, header)
		{
			this.parentDataObject = parentDataObject;
		}
		readonly Shipment parentDataObject;

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override CusInBondBill GetExistingBusinessObject()
		{
			Helper.ThrowReadFailureExceptionWhenNonSupportedElementsFound(parentDataObject, elementsNotSupportedList =>
			{
				if (dataObject.AddInfoGroupCollection != null && dataObject.AddInfoGroupCollection.Any(x => x.Type.Code.GetValueOrDefault() == Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.USDisposition && x.AddInfoCollection.Count > 0))
				{
					elementsNotSupportedList.Add("AddInfoGroupCollection><AddInfoGroup><Type><Code>UDP</Code></Type></AddInfoGroup></AddInfoGroupCollection");
				}
			});

			return base.GetExistingBusinessObject();
		}

		protected override ZQuery GetExistingBillQueryCore(ZGuid inBondHeaderPK)
		{
			var query = new ZQuery(CusInBondBillSchema.B0_BH, inBondHeaderPK);
			var wayBillType = dataObject.BillType.GetCodeAsUpperCase();
			if (wayBillType == WayBillTypeList.Codes.Master)
			{
				query.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, dataObject.BillNumber.GetValueOrDefault());

				var shipmentType = dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ShipmentType).GetValueOrDefault();
				if (!shipmentType.IsEmpty)
				{
					query.AddToFilter(CusInBondBillSchema.B0_ShipmentType, shipmentType);
				}

				var issuerCode = dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.IssuerCode).GetValueOrDefault();
				if (!issuerCode.IsEmpty)
				{
					query.AddToFilter(CusInBondBillSchema.B0_IssuerCode, issuerCode);
				}
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			return query;
		}

		protected override void FillInBondSpecificData(IColumnIndexer billRow)
		{
			base.FillInBondSpecificData(billRow);
			SetValue(billRow, CusInBondBillSchema.B0_ManifestUQ, dataObject.PackType);
		}

		protected override void FillDataFromAddInfos(IColumnIndexer billRow)
		{
			base.FillDataFromAddInfos(billRow);
			SetValue(billRow, CusInBondBillSchema.B0_ShipmentType, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ShipmentType));
			SetValue(billRow, CusInBondBillSchema.B0_IssuerCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.IssuerCode));
			SetValue(billRow, CusInBondBillSchema.B0_BillStatus, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.BillStatus));
			SetValue(billRow, CusInBondBillSchema.B0_RL_NKPortOfLading, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfLading));
			SetValue(billRow, CusInBondBillSchema.B0_PortOfLadingKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfLadingScheduleK));
			SetValue(billRow, CusInBondBillSchema.B0_PlaceOfReceipt, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PlaceOfReceiptScheduleD));
			SetValue(billRow, CusInBondBillSchema.B0_TransportModeToPortOfLading, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.TransportModeToPortOfLading));
			SetValue(billRow, CusInBondBillSchema.B0_RL_NKLastForeignPort, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.LastForeignPort));
			SetValue(billRow, CusInBondBillSchema.B0_LastForeignPortKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.LastForeignPortScheduleK));
			SetValue(billRow, CusInBondBillSchema.B0_RL_NKForeignPortOfContract, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ForeignPortOfContract));
			SetValue(billRow, CusInBondBillSchema.B0_ForeignPortOfContractKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ForeignPortOfContractScheduleK));
			SetValue(billRow, CusInBondBillSchema.B0_Weight, dataObject.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Weight));
			SetValue(billRow, CusInBondBillSchema.B0_WeightUQ, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.WeightUnit));
			SetValue(billRow, CusInBondBillSchema.B0_Volume, dataObject.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Volume));
			SetValue(billRow, CusInBondBillSchema.B0_VolumeUQ, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.VolumeUnit));
			SetValue(billRow, CusInBondBillSchema.B0_RL_NKInBondPortOfDest, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfUnlading));
			SetValue(billRow, CusInBondBillSchema.B0_InBondPortOfDestDCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfUnladingScheduleD));
			SetValue(billRow, CusInBondBillSchema.B0_DateOfDischarge, dataObject.AddInfoCollection.GetZDateTimeValue(Constants.Bill.AddInfo.EstimatedUnloadDate));
			SetValue(billRow, CusInBondBillSchema.B0_MasterInBondIndicator, dataObject.AddInfoCollection.GetZBoolValue(Constants.Bill.AddInfo.MasterInBondIndicator));
			SetValue(billRow, CusInBondBillSchema.B0_Firms, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.FIRMS));
			SetValue(billRow, CusInBondBillSchema.B0_TransportPaymentMethod, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PaymentMethod));
			SetValue(billRow, CusInBondBillSchema.B0_BillActionCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ActionCode));
			SetValue(billRow, CusInBondBillSchema.B0_BillAmendmentCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.AmendmentCode));
		}

		protected override void FillCustomsReferenceData(IColumnIndexer billRow)
		{
			base.FillCustomsReferenceData(billRow);
			var customsReferenceCollection = dataObject.CustomsReferenceCollection;
			if (customsReferenceCollection != null)
			{
				var billPK = billRow.GetValue(CusInBondBillSchema.PK);
				FillInBondBillAddRef(billPK, customsReferenceCollection);
				FillSecondaryNotifyParty(billPK, customsReferenceCollection);
			}
		}

		protected override IEnumerable<ZString> GetBillSettingOrder(CusInBondBill bill)
		{
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ShipmentType);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_IssuerCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_MasterBillNumber);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_BillStatus);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_RL_NKPortOfLading);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_PortOfLadingKCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_PlaceOfReceipt);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_TransportModeToPortOfLading);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_RL_NKLastForeignPort);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_LastForeignPortKCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_RL_NKForeignPortOfContract);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ForeignPortOfContractKCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ManifestQty);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_ManifestUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_Weight);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_WeightUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_Volume);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_VolumeUQ);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_TransportPaymentMethod);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_RL_NKInBondPortOfDest);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_InBondPortOfDestDCode);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_DateOfDischarge);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_MasterInBondIndicator);
			yield return ColumnValueSetter.GetKey(bill.PK, CusInBondBillSchema.B0_Firms);
		}

		void FillInBondBillAddRef(ZGuid billPK, List<CustomsReference> customsReferenceCollection)
		{
			UpdateIfExistOrDeleteFromReference(billPK, customsReferenceCollection,
				() => new List<CusInbondBillAddRef>(factory.Load<CusInbondBillAddRef>(new ZQuery(CusInbondBillAddRefSchema.BR_B0, billPK))),
				() => customsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.Type),
				(itemInDb, cusRef) => itemInDb.BR_Qualifier.Equals(cusRef.SubType.Code),
				(row, cusRef) =>
				{
					SetValue(row, CusInbondBillAddRefSchema.BR_B0, billPK);
					SetValue(row, CusInbondBillAddRefSchema.BR_Qualifier, cusRef.SubType);
					SetValue(row, CusInbondBillAddRefSchema.BR_ReferenceNum, cusRef.Reference);
				});
		}

		void FillSecondaryNotifyParty(ZGuid billPK, List<CustomsReference> customsReferenceCollection)
		{
			UpdateIfExistOrDeleteFromReference(billPK, customsReferenceCollection,
				() =>
				{
					var secondaryNotifyPartyQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, billPK);
					secondaryNotifyPartyQuery.AddToFilter(CusCodeDataSchema.CY_Type, SecondaryNotifyParty.SNPType);
					return new List<SecondaryNotifyParty>(factory.Load<SecondaryNotifyParty>(secondaryNotifyPartyQuery));
				},
				() => customsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type),
				(itemInDb, cusRef) => itemInDb.CY_Data.Equals(cusRef.SubType.Code),
				(row, cusRef) =>
				{
					SetValue(row, CusCodeDataSchema.CY_ParentTableCode, CusInBondBillSchema.Constants.Prefix);
					SetValue(row, CusCodeDataSchema.CY_ParentID, billPK);
					SetValue(row, CusCodeDataSchema.CY_Type, SecondaryNotifyParty.SNPType);
					SetValue(row, CusCodeDataSchema.CY_Data, cusRef.SubType);
					SetValue(row, CusCodeDataSchema.CY_Order, ZShort.ParseSafe(cusRef.Reference.GetValueOrDefault(), ZShort.Zero));
				});
		}

		void UpdateIfExistOrDeleteFromReference<T>(ZGuid billPK, List<CustomsReference> customsReferenceCollection, Func<List<T>> objectsInDbSupplier, Func<IEnumerable<CustomsReference>> objectsInReferenceSupplier, Func<T, CustomsReference, bool> itemExistInDbCondition, Action<IColumnIndexer, CustomsReference> fillInFields) where T : BusinessObject
		{
			var objectsInDb = objectsInDbSupplier();
			var objectsInReference = objectsInReferenceSupplier();
			if (objectsInReference.Any())
			{
				foreach (var itemInReference in objectsInReference)
				{
					var itemExist = objectsInDb.FirstOrDefault(f => itemExistInDbCondition(f, itemInReference));
					IColumnIndexer itemRow;
					if (itemExist != null)
					{
						itemRow = GetColumnIndexer(itemExist);
						objectsInDb.Remove(itemExist);
					}
					else
					{
						itemRow = GetColumnIndexer(factory.New<T>());
					}
					fillInFields(itemRow, itemInReference);
				}
			}
			if (objectsInDb.Count > 0)
			{
				objectsInDb.DeleteAll();
			}
		}
	}
}

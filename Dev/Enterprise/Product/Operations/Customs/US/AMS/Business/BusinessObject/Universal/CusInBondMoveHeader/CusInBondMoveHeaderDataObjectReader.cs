using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondMoveHeaderDataObjectReader : DataTransfer.Universal.CusInBondMoveHeaderDataObjectReader<CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondMoveHeaderDataObjectReader(Shipment parentDataObject, InBondMoveHeader moveHeaderDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, CusInBondHeader header)
			: base(moveHeaderDataObject, logger, helper, header)
		{
			this.parentDataObject = parentDataObject;
		}
		readonly Shipment parentDataObject;

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		protected override CusInBondMoveHeader GetExistingBusinessObject()
		{
			Helper.ThrowReadFailureExceptionWhenNonSupportedElementsFound(parentDataObject, elementsNotSupportedList =>
			{
				if (dataObject.CustomsStatus != null && !dataObject.CustomsStatus.Code.GetValueOrDefault().IsEmpty)
				{
					elementsNotSupportedList.Add(nameof(dataObject.CustomsStatus));
				}
			});

			CusInBondMoveHeader result = null;
			var applicationCode = dataObject.MessagingApplicationCode.GetCodeAsUpperCase();
			var sequenceNumber = dataObject.SequenceNumber.GetValueOrDefault();
			if (applicationCode == SubApplicationCodeList.Codes.AMS && !sequenceNumber.IsEmpty)
			{
				var headerRow = GetColumnIndexer(Header);
				var headerPK = headerRow.GetValue(CusInBondHeaderSchema.PK);
				var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, headerPK);
				query.OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name;
				query.FetchOnlyFromLocalCache = !Header.IsInDatabase;
				var moveHeaders = factory.Load<CusInBondMoveHeader>(query);
				foreach (var moveHeader in moveHeaders)
				{
					var sequenceNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeader.PK);
					sequenceNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusInBondMoveHeader.ManifestSequenceNumberEntryType);
					sequenceNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, sequenceNumber);
					if (factory.LoadTop1<CusEntryNumber>(sequenceNumberQuery) != null)
					{
						result = moveHeader;
						break;
					}
				}
			}
			else if (applicationCode == SubApplicationCodeList.Codes.MasterInBond || applicationCode == SubApplicationCodeList.Codes.SubsequentInBond)
			{
				return base.GetExistingBusinessObject();
			}

			return result;
		}

		protected override void FillInBondSpecificData(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters, CusInBondMoveHeader moveHeaderBO)
		{
			base.FillInBondSpecificData(moveHeaderRow, delaySetters, moveHeaderBO);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_SubApplicationCode, dataObject.MessagingApplicationCode, delaySetters);
			FillInBondSequenceNumber(moveHeaderRow);
		}

		void FillInBondSequenceNumber(IColumnIndexer moveHeaderRow)
		{
			var moveHeaderPK = moveHeaderRow.GetValue(CusInBondMoveHeaderSchema.PK);
			var applicationCode = dataObject.MessagingApplicationCode.GetCodeAsUpperCase();
			var sequenceNumber = dataObject.SequenceNumber.GetValueOrDefault();
			if (applicationCode == SubApplicationCodeList.Codes.AMS && !sequenceNumber.IsEmpty)
			{
				if (sequenceNumber.Length > CusInBondMoveHeader.Schema.BM_ManifestSequenceNumberMaxLength)
				{
					logger.Log(LogType.Error, ZString.Format("Sequence number '{0}' is too long, only {1} characters is allowed.", sequenceNumber, CusInBondMoveHeader.Schema.BM_ManifestSequenceNumberMaxLength));
				}
				else
				{
					var query = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeaderPK);
					query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusInBondMoveHeader.ManifestSequenceNumberEntryType);
					query.FetchOnlyFromLocalCache = true;
					var entryNumberBORow = GetColumnIndexer(factory.LoadTop1<CusEntryNumber>(query) ?? factory.New<CusEntryNumber>());
					SetValue(entryNumberBORow, CusEntryNumSchema.CE_ParentTable, CusInBondMoveHeaderSchema.Constants.TableName);
					SetValue(entryNumberBORow, CusEntryNumSchema.CE_EntryType, CusInBondMoveHeader.ManifestSequenceNumberEntryType);
					SetValue(entryNumberBORow, CusEntryNumSchema.CE_ParentID, moveHeaderPK);
					SetValue(entryNumberBORow, CusEntryNumSchema.CE_EntryNum, sequenceNumber);
				}
			}
		}

		protected override void FillInBondNumber(CusInBondMoveHeader moveHeaderBO)
		{
			// WI00185439 Don't import inbond number
		}

		protected override IEnumerable<ZString> GetMoveHeaderSettingOrder(CusInBondMoveHeader moveHeader)
		{
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_SubApplicationCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_InBondEntryType);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_BTAIndicator);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_InBondCarrierID);
		}

		protected override void FillMovementDetails(IColumnIndexer moveHeaderRow, CusInBondMoveHeader moveHeader)
		{
			if (dataObject.InBondMoveDetailCollection != null)
			{
				Helper.MarkUnprocessedExistingMoveDetailsFor(moveHeader);
				var moveHeaderPK = moveHeaderRow.GetValue(CusInBondMoveHeaderSchema.PK);
				foreach (var inBondMoveDetail in dataObject.InBondMoveDetailCollection)
				{
					var bill = Helper.GetBill(inBondMoveDetail.AdditionalBillLink);
					var inBondNumberValue = InBondNumber.GetValueOrDefault();
					if (bill == null)
					{
						LogInBondMoveDetailNotMatched(inBondNumberValue, inBondMoveDetail);
					}
					else
					{
						var moveDetailReaded = InBondMoveDetailDataObjectReader(inBondMoveDetail, logger, Helper, moveHeaderPK, bill.PK, inBondNumberValue).ReadIntoBusinessObject();
						Helper.MarkProcessed(moveDetailReaded);
					}
				}
				Helper.DeleteUnprocessedMoveDetailsFor(moveHeader, logger);
			}
		}

		protected override DataTransfer.Universal.CusInBondMoveDetailDataObjectReader<CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondMoveDetailDataObjectReader(InBondMoveDetail moveDetailData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNo)
		{
			return new CusInBondMoveDetailDataObjectReader(parentDataObject, moveDetailData, logger, Helper, Header, moveHeaderPK, billPK, inBondNo);
		}
	}
}

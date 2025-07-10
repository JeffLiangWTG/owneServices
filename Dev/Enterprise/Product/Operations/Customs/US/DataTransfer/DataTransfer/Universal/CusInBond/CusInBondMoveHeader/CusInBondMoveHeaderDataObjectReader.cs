using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class CusInBondMoveHeaderDataObjectReader<TMoveHeader, TMoveDetail, TContainer, TCommodity> : DataObjectReader<InBondMoveHeader, TMoveHeader>
		where TMoveHeader : CusInBondMoveHeader
		where TMoveDetail : CusInBondMoveDetail
		where TContainer : Customs.Business.CusInBondContainer
		where TCommodity : Customs.Business.CusInBondCargoDesc
	{
		protected CusInBondMoveHeaderDataObjectReader(InBondMoveHeader moveHeaderDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, Customs.Business.CusInBondHeader header)
			: base(moveHeaderDataObject, logger, helper.Factory)
		{
			this.header = Argument.NotNull(header, "header");
			var headerRow = GetColumnIndexer(header);
			this.headerPK = headerRow.GetValue(CusInBondHeaderSchema.PK);
			this.readerHelper = Argument.NotNull(helper, "helper");
		}
		readonly Customs.Business.CusInBondHeader header;
		readonly InBondDataObjectReaderHelper readerHelper;
		readonly ZGuid headerPK;

		protected Customs.Business.CusInBondHeader Header
		{
			get { return header; }
		}

		protected InBondDataObjectReaderHelper Helper
		{
			get { return readerHelper; }
		}

		protected ZString? InBondNumber
		{
			get
			{
				if (!hasCalculateInBondNumber)
				{
					hasCalculateInBondNumber = true;
					inBondNumber = GetEntryNumber(Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
				}
				return inBondNumber;
			}
		}
		ZString? inBondNumber;
		bool hasCalculateInBondNumber;

		ZString? GetEntryNumber(string type)
		{
			ZString? result = null;
			if (dataObject.EntryNumberCollection != null)
			{
				var inBondNumberDataObject = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == type);
				if (inBondNumberDataObject != null)
				{
					result = inBondNumberDataObject.Number;
				}
			}
			return result;
		}

		protected override TMoveHeader GetExistingBusinessObject()
		{
			TMoveHeader result = null;
			if (InBondNumber.HasValue)
			{
				var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, headerPK);
				query.OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name;
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				var moveHeaders = factory.Load<TMoveHeader>(query);
				foreach (var moveHeader in moveHeaders)
				{
					var inBondNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeader.PK);
					inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
					inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, InBondNumber.Value);
					if (factory.LoadTop1<CusEntryNumber>(inBondNumberQuery) != null)
					{
						result = moveHeader;
						break;
					}
				}
			}
			return result;
		}

		protected override void PopulateBusinessObject(TMoveHeader moveHeaderBO)
		{
			var moveHeaderRow = GetColumnIndexer(moveHeaderBO);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_BH, headerPK);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_InBondEntryType, dataObject.EntryType, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_BTAIndicator, dataObject.BioterrorismActIndicator, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_DestinationPortCode, dataObject.DestinationPortScheduleD, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ForeignDestPortKCode, dataObject.ForeignDestinationPortScheduleK, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ExportLadenOn, dataObject.ExportVesselName, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_GS_NKCusAgent, dataObject.CustomsAgent, delaySetters);
			FillFillOrganizationAddresses(moveHeaderRow, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_InBondCarrierID, dataObject.InBondCarrierID, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_InBondCarrierSCAC, dataObject.InBondCarrierSCAC, delaySetters);
			FillTransferOfLiabilityData(moveHeaderRow, delaySetters);
			FillDates(moveHeaderRow, delaySetters);
			FillInBondNumber(moveHeaderBO);
			FillInBondSpecificData(moveHeaderRow, delaySetters, moveHeaderBO);
			delaySetters.SetValueInSpecificOrder(GetMoveHeaderSettingOrder(moveHeaderBO));
			FillMovementDetails(moveHeaderRow, moveHeaderBO);
		}

		protected virtual void FillFillOrganizationAddresses(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var inBondCarrierAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.CusInBond.AddressTypes.InBondCarrier);
				if (inBondCarrierAddress != null)
				{
					var reader = new OrganisationDataObjectReader(inBondCarrierAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_OA_InBondCarrier, orgAddress == null ? ZGuid.Empty : orgAddress.PK, delaySetters);
				}
				var tolCarrierAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.CusInBond.AddressTypes.TransferOfLiabilityCarrier);
				if (tolCarrierAddress != null)
				{
					var reader = new OrganisationDataObjectReader(tolCarrierAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_OA_TOLCarrier, orgAddress == null ? ZGuid.Empty : orgAddress.PK, delaySetters);
				}
			}
		}

		void FillTransferOfLiabilityData(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCarrierCode, dataObject.TransferOfLiabilityCarrierCode, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCarrierID, dataObject.TransferOfLiabilityCarrierID, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCityName, dataObject.TransferOfLiabilityCityName, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLStateCode, dataObject.TransferOfLiabilityStateCode, delaySetters);
		}

		protected virtual void FillDates(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(moveHeaderRow, dataObject.DateCollection, ZBool.True, delaySetters,
					new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_EntryDate, new[] { DateType.EntryDate }),
					new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_ArrivalDate, new[] { DateType.Arrival }),
					new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_ExportDate, new[] { DateType.Departure }),
					new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_TOLDate, new[] { DateType.TransferOfLiability }));
			}
		}

		protected virtual void FillInBondNumber(TMoveHeader moveHeaderBO)
		{
			if (InBondNumber.HasValue)
			{
				var inBondNumberBO = CusEntryNumber.Load(moveHeaderBO, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
				if (InBondNumber.Value.IsEmpty)
				{
					if (inBondNumberBO != null)
					{
						inBondNumberBO.Delete();
					}
				}
				else
				{
					inBondNumberBO = inBondNumberBO ?? CusEntryNumber.New(moveHeaderBO, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
					SetValue(inBondNumberBO, CusEntryNumSchema.CE_EntryNum, InBondNumber.Value);
				}
			}
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters, TMoveHeader moveHeaderBO)
		{
		}

		protected virtual IEnumerable<ZString> GetMoveHeaderSettingOrder(TMoveHeader moveHeader)
		{
			return System.Array.Empty<ZString>();
		}

		protected virtual void FillMovementDetails(IColumnIndexer moveHeaderRow, TMoveHeader moveHeader)
		{
			if (dataObject.InBondMoveDetailCollection != null)
			{
				var moveHeaderPK = moveHeaderRow.GetValue(CusInBondMoveHeaderSchema.PK);
				var query = new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeaderPK);
				query.FetchOnlyFromLocalCache = !moveHeader.IsInDatabase;
				factory.Load<CusInBondMoveDetail>(query).DeleteAll();
				foreach (var moveDetailData in dataObject.InBondMoveDetailCollection)
				{
					var bill = readerHelper.GetBill(moveDetailData.AdditionalBillLink);
					var inBondNumberValue = InBondNumber.GetValueOrDefault();
					if (bill == null)
					{
						LogInBondMoveDetailNotMatched(inBondNumberValue, moveDetailData);
					}
					else
					{
						InBondMoveDetailDataObjectReader(moveDetailData, logger, readerHelper, moveHeaderPK, bill.PK, inBondNumberValue).ReadIntoBusinessObject();
					}
				}
			}
		}

		protected abstract CusInBondMoveDetailDataObjectReader<TMoveDetail, TContainer, TCommodity> InBondMoveDetailDataObjectReader(InBondMoveDetail moveDetailData, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNo);

		protected void LogInBondMoveDetailNotMatched(ZString inBondNumberValue, InBondMoveDetail moveDetailData)
		{
			logger.Log(Integration.LogType.Error, ZString.Format("Cannot process InBondMoveDetail for In-Bond Number '{0}' as AdditionalBillLink '{1}' was not able to be matched", inBondNumberValue, moveDetailData.AdditionalBillLink.GetValueOrDefault()));
		}
	}
}

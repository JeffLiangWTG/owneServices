using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondHeaderDataObjectReader : DataTransfer.Universal.CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
			: base(dataObject, logger, factory, parentBO)
		{
		}

		public override bool CheckUpdateDataIsAllowed(CusInBondHeader header)
		{
			return (dataObject.AllowUpdateOfCustomsDeclarationAfterCommencement ?? false) || base.CheckUpdateDataIsAllowed(header);
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USAMS; }
		}

		protected override ZString CusInBondApplicationCode
		{
			get { return Common.CusInBondApplicationCodeList.Codes.AMS; }
		}

		protected override ZString InBondDescription
		{
			get { return "US AMS"; }
		}

		protected override CusInBondHeader GetMatchedExistingInBondHeader()
		{
			CusInBondHeader result = null;
			existingMatched = null;
			var transitDirection = GetTransitDirection();
			if (transitDirection == DirectionTypeList.Codes.MVOCC)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCode);
				var vesselName = dataObject.GetVesselName();
				var voyageNumber = dataObject.VoyageFlightNo.GetValueOrDefault();
				if (!vesselName.IsEmpty && !voyageNumber.IsEmpty)
				{
					multipleMatchedReason = "Vessel Name: " + vesselName + ", Voyage: " + voyageNumber;
					headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ImportConveyanceName, vesselName);
					headerQuery.AddToFilter(CusInBondHeaderSchema.BH_VoyageNumber, voyageNumber);

					var arrivalDate = GetDate(DateType.Arrival, true).GetValueOrDefault();
					if (!arrivalDate.IsEmpty)
					{
						multipleMatchedReason += " and Estimate Time: " + arrivalDate;
						headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.GreaterThanOrEqualTo, arrivalDate.AddMonths(-6));
						headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.LessThanOrEqualTo, arrivalDate.AddMonths(6));
					}

					existingMatched = factory.Load<CusInBondHeader>(headerQuery);
				}
			}
			else if (transitDirection == DirectionTypeList.Codes.NVOCC)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCode);

				if (dataObject.AdditionalBillCollection != null)
				{
					var oceanBills = dataObject.AdditionalBillCollection.Where(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master && x.BillNumber.HasValue && x.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ShipmentType).GetValueOrDefault() == CusInBondBill.OceanBillType);
					if (oceanBills.Count() == 1)
					{
						var oceanBill = oceanBills.FirstOrDefault();
						var issuerCode = oceanBill.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.IssuerCode).GetValueOrDefault();
						var billNumber = oceanBill.BillNumber.GetValueOrDefault();
						var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
						billSubQuery.AddToFilter(CusInBondBillSchema.B0_IssuerCode, issuerCode);
						billSubQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, billNumber);
						headerQuery.AddSubQuery(billSubQuery, JoinCondition.And);
						existingMatched = factory.Load<CusInBondHeader>(headerQuery);
						multipleMatchedReason = "ocean bill details, Issue Code: " + issuerCode + ", Bill Number: " + billNumber;
					}
				}
			}

			if ((existingMatched == null || existingMatched.Length == 0) && InBondNumbers.Length > 0)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCode);
				var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, InBondNumbers);
				moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
				headerQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
				headerQuery.OrderBy = CusInBondHeaderSchema.BH_SystemCreateTimeUtc.Name;
				existingMatched = factory.Load<CusInBondHeader>(headerQuery);
				multipleMatchedReason = "InBond Number (" + string.Join(", ", InBondNumbers.Select(x => "'" + x + "'")) + ")";
			}

			if (existingMatched != null && existingMatched.Length == 1)
			{
				result = existingMatched[0];
			}

			return result;
		}
		CusInBondHeader[] existingMatched;
		ZString multipleMatchedReason;

		ZString GetTransitDirection()
		{
			var result = ZString.Empty;
			var transitDirection = dataObject.MessageType.GetCodeAsUpperCase();
			if (transitDirection == DirectionTypeList.Codes.MVOCC || transitDirection == DirectionTypeList.Codes.NVOCC)
			{
				result = transitDirection;
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusInBondHeader targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty)
			{
				var transitDirectionInMessage = GetTransitDirection();
				if (transitDirectionInMessage.IsEmpty)
				{
					result = Res.GetString("af47e432-b9c9-4886-ae18-9cfda9f07c81", "Message type is invalid for AMS job, should be either 'N' or 'M'.");
				}
				else if (targetBO != null && targetBO.IsInDatabase)
				{
					var transitDirectionInHeader = targetBO.BH_TransitDirection;
					if (transitDirectionInMessage != transitDirectionInHeader)
					{
						result = Res.GetString("cb06e5a9-1495-4a11-b1d5-e0725d2a4832", "Message type in UXML and in AMS job - {0} doesn't match.", targetBO.BH_JobReference);
					}
				}
				else if (existingMatched != null && existingMatched.Length > 1 && !multipleMatchedReason.IsEmpty)
				{
					result = Res.GetString("a9da11fc-964f-4f8e-9de7-75521e78b0af", "Multiple jobs were matched with {0}", multipleMatchedReason);
				}
			}
			return result;
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override MutexID InBondMutexID
		{
			get { return MutexIDs.AMSJobBeingCreatedForConsol; }
		}

		protected override void FillInBondSpecificData(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters, CusInBondHeader headerBO)
		{
			base.FillInBondSpecificData(headerRow, delaySetters, headerBO);
			SetValue(headerRow, CusInBondHeaderSchema.BH_TransitDirection, dataObject.MessageType, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_RL_NKImportLoadPort, dataObject.PortOfLoading, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_RL_NKPortUnlading, dataObject.PortOfDischarge, delaySetters);
		}

		protected override void FillDataFromAddInfos(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillDataFromAddInfos(headerRow, delaySetters);
			if (dataObject.AddInfoCollection != null)
			{
				SetValue(headerRow, CusInBondHeaderSchema.BH_CarrierSCAC, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.CarrierSCAC), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_PortUnladingDCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.PortOfDischargeScheduleD), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_FIRMS, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.FIRMS), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_UniqueVoyageIdentifier, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.UniqueVoyageIdentifier), delaySetters);

				var isPaperlessMIBParticipant = dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.IsPaperlessMIBParticipant).GetValueOrDefault();
				if (isPaperlessMIBParticipant == YesNoList.Codes.Yes)
				{
					SetValue(headerRow, CusInBondHeaderSchema.BH_HeaderType, ModeTypeList.Codes.PaperlessMIBParticipant, delaySetters);
				}

				var isOutboundCargo = dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.IsOutboundCargo).GetValueOrDefault();
				if (isOutboundCargo == YesNoList.Codes.Yes)
				{
					SetValue(headerRow, CusInBondHeaderSchema.BH_ExportFlag, ExportTypeList.Codes.OutboundCargo, delaySetters);
				}
			}
		}

		protected override void FillDates(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillDates(headerRow, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_FirstExportDate, GetDate(DateType.Departure, true), delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_ETA, GetDate(DateType.Arrival, true), delaySetters);
		}

		ZDateTime? GetDate(DateType dateType, ZBool isEstimate)
		{
			ZDateTime? result = null;
			Date dateObject = null;
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				dateObject = dataObject.DateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dateType && x.IsEstimate.GetValueOrDefault() == isEstimate);
			}

			if (dateObject != null)
			{
				result = dateObject.Value;
			}

			return result;
		}

		protected override IEnumerable<ZString> GetHeaderSettingOrder(CusInBondHeader header)
		{
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_TransitDirection);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_OverrideFreightDefaults);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_GB);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportTransportMode);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_CarrierSCAC);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_HeaderType);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ExportFlag);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_RL_NKImportLoadPort);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_FirstExportDate);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_RL_NKPortUnlading);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_PortUnladingDCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ETA);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_FIRMS);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportConveyanceName);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_VoyageNumber);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_LloydsNumber);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportConveyanceCountry);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_UniqueVoyageIdentifier);
		}

		protected override DataTransfer.Universal.CusInBondBillDataObjectReader<CusInBondBill> InBondBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DataTransfer.Universal.InBondDataObjectReaderHelper helper, CusInBondHeader headerBO)
		{
			return new CusInBondBillDataObjectReader(dataObject, additionalBillDataObject, logger, factory, Helper, headerBO);
		}

		protected override DataTransfer.Universal.CusInBondMoveHeaderDataObjectReader<CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondMoveHeaderDataObjectReader(InBondMoveHeader inBondMoveHeaderDataObject, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, CusInBondHeader headerBO)
		{
			return new CusInBondMoveHeaderDataObjectReader(dataObject, inBondMoveHeaderDataObject, logger, Helper, headerBO);
		}

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper GetInBondDataObjectReaderHelperCore()
		{
			return new InBondDataObjectReaderHelper(factory);
		}
	}
}

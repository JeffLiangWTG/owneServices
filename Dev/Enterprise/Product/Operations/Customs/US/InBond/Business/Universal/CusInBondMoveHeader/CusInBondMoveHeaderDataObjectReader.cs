using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using InBondTransportModeCodes = Enterprise.Customs.US.Business.InBondTransportModeCodes;
using ValueSetter = Enterprise.UniversalDataBuss.DataObjects.Core.ValueSetter;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondMoveHeaderDataObjectReader : DataTransfer.Universal.CusInBondMoveHeaderDataObjectReader<CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondMoveHeaderDataObjectReader(InBondMoveHeader moveHeaderDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, CusInBondHeader header)
			: base(moveHeaderDataObject, logger, helper, header)
		{
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		internal void PopulateMainData(CusInBondMoveHeader moveHeaderBO)
		{
			PopulateBusinessObject(moveHeaderBO);
		}

		protected override void FillInBondSpecificData(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters, CusInBondMoveHeader moveHeaderBO)
		{
			base.FillInBondSpecificData(moveHeaderRow, delaySetters, moveHeaderBO);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_MonetaryValue, dataObject.MonetaryValue, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_RL_NKForeignDestPort, dataObject.ForeignDestinationPortUNLOCO, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_Via, dataObject.LastForeignPortScheduleK, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_AdditionalText, dataObject.AdditionalText, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_Seals, dataObject.Seals, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_PortOfPresentationCode, dataObject.PortOfPresentationScheduleD, delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ExportTransportMode, GetExportTransportMode(), delaySetters);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_MoveToFTZ, dataObject.MoveToFTZ, delaySetters);
			FillAdditionalReferences(moveHeaderRow, delaySetters);
		}

		void FillAdditionalReferences(IColumnIndexer moveHeaderRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var pedimentoNumberData = dataObject.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.MovementHeader.AdditionalReferences.PedimentoNumber);
				if (pedimentoNumberData != null)
				{
					var pedimentoNumberDataValue = pedimentoNumberData.ReferenceNumber ?? ZString.Empty;
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_PedimentoNumber, pedimentoNumberDataValue, delaySetters);
				}
				var goNumberData = dataObject.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.MovementHeader.AdditionalReferences.GONumber);
				if (goNumberData != null)
				{
					var goNumberDataValue = goNumberData.ReferenceNumber ?? ZString.Empty;
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_GONumber, goNumberDataValue, delaySetters);
				}
			}
		}

		ZString? GetExportTransportMode()
		{
			ZString? result = null;

			if (dataObject.ExportTransportMode != null)
			{
				switch (dataObject.ExportTransportMode.GetCodeAsUpperCase())
				{
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Air:
						result = InBondTransportModeCodes.Codes.AirNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
						result = InBondTransportModeCodes.Codes.RailNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck:
						result = InBondTransportModeCodes.Codes.TruckNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
						var isContainerized = dataObject.ExportContainerMode != null && dataObject.ExportContainerMode.GetCodeAsUpperCase() == Enterprise.Customs.US.Business.ContainerModeList.Codes.Containerized;
						result = isContainerized ? InBondTransportModeCodes.Codes.VesselContainer : InBondTransportModeCodes.Codes.VesselNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations:
						result = InBondTransportModeCodes.Codes.FixedTransportInstallations;
						break;
				}
			}
			return result;
		}

		protected override IEnumerable<ZString> GetMoveHeaderSettingOrder(CusInBondMoveHeader moveHeader)
		{
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_InBondEntryType);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_BTAIndicator);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_OA_InBondCarrier);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_InBondCarrierID);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_InBondCarrierSCAC);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_DestinationPortCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_ForeignDestPortKCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_RL_NKForeignDestPort);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_MonetaryValue);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_ArrivalDate);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_ExportDate);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_ExportLadenOn);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_ExportTransportMode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_OA_TOLCarrier);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_TOLCarrierID);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_TOLCarrierCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_TOLDate);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_TOLCityName);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_TOLStateCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_EntryDate);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_PortOfPresentationCode);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_Via);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_AdditionalText);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_Seals);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_GS_NKCusAgent);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_PedimentoNumber);
			yield return ColumnValueSetter.GetKey(moveHeader.PK, CusInBondMoveHeaderSchema.BM_GONumber);
		}

		protected override DataTransfer.Universal.CusInBondMoveDetailDataObjectReader<CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondMoveDetailDataObjectReader(InBondMoveDetail moveDetailData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNumber)
		{
			return new CusInBondMoveDetailDataObjectReader(moveDetailData, logger, Helper, moveHeaderPK, billPK, inBondNumber);
		}
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondMoveHeaderDataObjectWriter : DataObjectWriter<CusInBondMoveHeader, InBondMoveHeader>
	{
		public CusInBondMoveHeaderDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData)
			: base(writeManager)
		{
			this.writerHelper = Argument.NotNull(helper, "helper");
			this.headerData = Argument.NotNull(headerData, "headerData");
		}
		readonly InBondDataObjectWriterHelper writerHelper;
		readonly Shipment headerData;

		protected InBondDataObjectWriterHelper Helper
		{
			get { return writerHelper; }
		}

		protected override InBondMoveHeader PopulateDataObject(CusInBondMoveHeader moveHeaderBO)
		{
			var moveHeaderData = new InBondMoveHeader(writeManager.WriterStrategy);
			moveHeaderData.EntryType = ListHelper.GetWithDescription<CodeDescriptionPair9Char>(moveHeaderBO.BM_InBondEntryType, moveHeaderBO.Lookups.EntryTypeList);
			moveHeaderData.CustomsStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(moveHeaderBO.BM_CustomsStatus, moveHeaderBO.Lookups.MessageStatusList);
			moveHeaderData.BioterrorismActIndicator = moveHeaderBO.BM_BTAIndicator;
			moveHeaderData.InBondCarrierID = moveHeaderBO.BM_InBondCarrierID;
			moveHeaderData.InBondCarrierSCAC = moveHeaderBO.BM_InBondCarrierSCAC;
			moveHeaderData.DestinationPortScheduleD = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(moveHeaderBO.BM_DestinationPortCode, moveHeaderBO.Lookups.RegionDistrictPorts);
			moveHeaderData.ForeignDestinationPortScheduleK = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(moveHeaderBO.BM_ForeignDestPortKCode, moveHeaderBO.Lookups.ForeignPorts);
			moveHeaderData.ExportVesselName = moveHeaderBO.BM_ExportLadenOn;
			moveHeaderData.CustomsAgent = Staff.New(moveHeaderBO.CusAgent);
			moveHeaderData.DateCollection = PopulateDates(moveHeaderBO, moveHeaderData);
			moveHeaderData.InBondMoveDetailCollection = ProcessCollection(GetRelatedMoveDetails(moveHeaderBO, Helper), InBondMoveDetailDataObjectWriter(writeManager, Helper, headerData));
			moveHeaderData.EntryNumberCollection = PopulateEntryNumbers(moveHeaderBO, moveHeaderData);
			PopulateOrganizations(moveHeaderBO, moveHeaderData);
			PopulateTransferOfLiabilityData(moveHeaderBO, moveHeaderData);
			PopulateInBondSpecificData(moveHeaderBO, moveHeaderData);
			return moveHeaderData;
		}

		protected virtual void PopulateOrganizations(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			moveHeaderData.AddOrgAddress(writeManager, moveHeaderBO.InBondCarrier, Constants.CusInBond.AddressTypes.InBondCarrier);
			moveHeaderData.AddOrgAddress(writeManager, moveHeaderBO.TOLCarrier, Constants.CusInBond.AddressTypes.TransferOfLiabilityCarrier);
		}

		protected virtual void PopulateTransferOfLiabilityData(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			moveHeaderData.TransferOfLiabilityCarrierCode = moveHeaderBO.BM_TOLCarrierCode;
			moveHeaderData.TransferOfLiabilityCarrierID = moveHeaderBO.BM_TOLCarrierID;
			moveHeaderData.TransferOfLiabilityCityName = moveHeaderBO.BM_TOLCityName;
			moveHeaderData.TransferOfLiabilityStateCode = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(moveHeaderBO.BM_TOLStateCode, moveHeaderBO.Lookups.USStatesList);
		}

		protected virtual List<EntryNumber> PopulateEntryNumbers(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			var entryNumberCollection = moveHeaderData.EntryNumberCollection ?? new List<EntryNumber>();
			entryNumberCollection.Add(new EntryNumber()
			{
				Type = new EntryType() { Code = Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, Description = Constants.CusInBond.MovementHeader.NumberTypes.InBondNumberDescription },
				Number = moveHeaderBO.InBondNumber
			});
			return entryNumberCollection;
		}

		protected virtual List<Date> PopulateDates(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			var dateCollection = moveHeaderData.DateCollection ?? new List<Date>();
			dateCollection.Add(DateType.EntryDate, ZBool.True, moveHeaderBO.BM_EntryDate);
			dateCollection.Add(DateType.Arrival, ZBool.True, moveHeaderBO.BM_ArrivalDate);
			dateCollection.Add(DateType.Departure, ZBool.True, moveHeaderBO.BM_ExportDate);
			dateCollection.Add(DateType.TransferOfLiability, ZBool.True, moveHeaderBO.BM_TOLDate);
			return dateCollection;
		}

		protected virtual void PopulateInBondSpecificData(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
		}

		protected virtual IEnumerable<CusInBondMoveDetail> GetRelatedMoveDetails(CusInBondMoveHeader moveHeaderBO, InBondDataObjectWriterHelper headerHelper)
		{
			return headerHelper.Load<CusInBondMoveDetail>(moveHeaderBO.MovementDetails.CompleteFilter);
		}

		protected virtual CusInBondMoveDetailDataObjectWriter InBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerShipment)
		{
			return new CusInBondMoveDetailDataObjectWriter(writeManager, helper, headerShipment);
		}
	}
}

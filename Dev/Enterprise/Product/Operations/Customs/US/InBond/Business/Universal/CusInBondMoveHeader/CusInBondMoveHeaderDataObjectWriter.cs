using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondMoveHeaderDataObjectWriter : DataTransfer.Universal.CusInBondMoveHeaderDataObjectWriter
	{
		public CusInBondMoveHeaderDataObjectWriter(IDataWritingManager manager, Shipment headerData, InBondDataObjectWriterHelper helper, bool includeWarehouseData = false)
			: base(manager, helper, headerData)
		{
			this.includeWarehouseData = includeWarehouseData;
		}
		readonly bool includeWarehouseData;

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(US.Business.CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			base.PopulateInBondSpecificData(moveHeaderBO, moveHeaderData);
			var inBondMoveHeaderBO = (CusInBondMoveHeader)moveHeaderBO;
			moveHeaderData.MonetaryValue = inBondMoveHeaderBO.BM_MonetaryValue;
			moveHeaderData.ForeignDestinationPortUNLOCO = ListHelper.GetWithName(inBondMoveHeaderBO.BM_RL_NKForeignDestPort, moveHeaderBO.Factory.GetRefUNLOCOList());
			moveHeaderData.LastForeignPortScheduleK = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(inBondMoveHeaderBO.BM_Via, inBondMoveHeaderBO.Lookups.ForeignPorts);
			moveHeaderData.AdditionalText = inBondMoveHeaderBO.BM_AdditionalText;
			moveHeaderData.Seals = inBondMoveHeaderBO.BM_Seals;
			moveHeaderData.PortOfPresentationScheduleD = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(inBondMoveHeaderBO.BM_PortOfPresentationCode, inBondMoveHeaderBO.Lookups.RegionDistrictPorts);
			moveHeaderData.MoveToFTZ = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(inBondMoveHeaderBO.BM_MoveToFTZ, inBondMoveHeaderBO.Lookups.YesNoList);
			PopulateExportTransportModeAndContainerMode(inBondMoveHeaderBO, moveHeaderData);
			PopulateAdditonalReferences(inBondMoveHeaderBO, moveHeaderData);
		}

		void PopulateExportTransportModeAndContainerMode(CusInBondMoveHeader headerMoveBO, InBondMoveHeader moveHeaderData)
		{
			var transportCode = headerMoveBO.BM_ExportTransportMode;
			if (!transportCode.IsEmpty)
			{
				var containerCode = ZString.Empty;

				switch (transportCode)
				{
					case InBondTransportModeCodes.Codes.AirNonContainer:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Air;
						break;
					case InBondTransportModeCodes.Codes.RailNonContainer:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail;
						break;
					case InBondTransportModeCodes.Codes.TruckNonContainer:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck;
						break;
					case InBondTransportModeCodes.Codes.VesselContainer:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
						containerCode = Enterprise.Customs.US.Business.ContainerModeList.Codes.Containerized;
						break;
					case InBondTransportModeCodes.Codes.VesselNonContainer:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
						break;
					case InBondTransportModeCodes.Codes.FixedTransportInstallations:
						transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations;
						break;
				}

				moveHeaderData.ExportTransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportCode, Helper.TransportTypeList);
				moveHeaderData.ExportContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerCode, Helper.ContainerModeList);
			}
		}

		protected override IEnumerable<US.Business.CusInBondMoveDetail> GetRelatedMoveDetails(US.Business.CusInBondMoveHeader moveHeaderBO, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			return base.GetRelatedMoveDetails(moveHeaderBO, headerHelper).OfType<CusInBondMoveDetail>().OrderBy(x => GetOrderValue(x));
		}

		ZString GetOrderValue(CusInBondMoveDetail moveDetail)
		{
			var bill = moveDetail.Bill;
			return bill == null ? ZString.Empty : bill.BillUniqueCode;
		}

		void PopulateAdditonalReferences(CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			moveHeaderData.AdditionalReferenceCollection = new DataObjectList<AdditionalReference>(new[]
			{
				new AdditionalReference()
				{
					Type = new EntryType() { Code = Constants.MovementHeader.AdditionalReferences.GONumber, Description = Constants.MovementHeader.AdditionalReferences.GONumberDescription },
					ReferenceNumber = moveHeaderBO.BM_GONumber
				},
				new AdditionalReference()
				{
					Type = new EntryType() { Code = Constants.MovementHeader.AdditionalReferences.PedimentoNumber, Description = Constants.MovementHeader.AdditionalReferences.PedimentoNumberDescription },
					ReferenceNumber = moveHeaderBO.BM_PedimentoNumber
				}
			});
		}

		protected override DataTransfer.Universal.CusInBondMoveDetailDataObjectWriter InBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper, Shipment headerData)
		{
			return new CusInBondMoveDetailDataObjectWriter(writeManager, Helper, headerData, includeWarehouseData);
		}
	}
}

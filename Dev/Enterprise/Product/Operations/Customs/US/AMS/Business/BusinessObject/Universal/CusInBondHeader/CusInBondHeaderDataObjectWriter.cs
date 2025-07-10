using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondHeaderDataObjectWriter : DataTransfer.Universal.CusInBondHeaderDataObjectWriter
	{
		public CusInBondHeaderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.USAMS;
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondHeader headerBO, Shipment headerData, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			base.PopulateInBondSpecificData(headerBO, headerData, headerHelper);
			var amsHeaderBO = (CusInBondHeader)headerBO;
			var amsHeadHelper = (InBondDataObjectWriterHelper)headerHelper;
			headerData.MessageType = ListHelper.GetWithDescription<CodeDescriptionPair>(amsHeaderBO.BH_TransitDirection, amsHeadHelper.DirectionTypes);
			headerData.PortOfLoading = ListHelper.GetWithName(amsHeaderBO.BH_RL_NKImportLoadPort, amsHeaderBO.Lookups.ImportLoadPorts);
			headerData.PortOfDischarge = ListHelper.GetWithName(amsHeaderBO.BH_RL_NKPortUnlading, amsHeaderBO.Lookups.PortUnladings);
			headerData.VesselName = amsHeaderBO.BH_ImportConveyanceName;
			headerData.VesselCountryOfRegistration = ListHelper.GetWithName<Country>(amsHeaderBO.BH_ImportConveyanceCountry, amsHeaderBO.Lookups.Countries);
			headerHelper.PopulateDispositions(amsHeaderBO, headerData);
			headerData.SetAdditionalBillCollection(() => ProcessCollection(headerBO.Bills, new CusInBondBillDataObjectWriter(writeManager, amsHeadHelper)));
			PopulateTransportMode(amsHeaderBO, headerData, amsHeadHelper);
			PopulatePortArrivalDates(amsHeaderBO, headerData);
		}

		protected override List<AddInfo> PopulateAddInfosData(Customs.Business.CusInBondHeader headerBO, Shipment headerData)
		{
			var addInfoCollection = base.PopulateAddInfosData(headerBO, headerData) ?? new List<AddInfo>();
			var amsHeaderBO = (CusInBondHeader)headerBO;
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.CarrierSCAC, Value = amsHeaderBO.BH_CarrierSCAC });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.IsPaperlessMIBParticipant, Value = amsHeaderBO.BH_IsPaperlessMIBParticipant ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.IsOutboundCargo, Value = amsHeaderBO.BH_IsOutboundCargo ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.PortOfDischargeScheduleD, Value = amsHeaderBO.BH_PortUnladingDCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.FIRMS, Value = amsHeaderBO.BH_FIRMS });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Header.AddInfo.UniqueVoyageIdentifier, Value = amsHeaderBO.BH_UniqueVoyageIdentifier });
			return addInfoCollection;
		}

		protected override List<Date> PopulateDatesData(Customs.Business.CusInBondHeader headerBO, Shipment headerData)
		{
			var dateCollection = base.PopulateDatesData(headerBO, headerData) ?? new List<Date>();
			var amsHeaderBO = (CusInBondHeader)headerBO;
			dateCollection.Add(DateType.Departure, ZBool.True, amsHeaderBO.BH_FirstExportDate);
			dateCollection.Add(DateType.Arrival, ZBool.True, amsHeaderBO.BH_ETA);
			dateCollection.Add(DateType.Arrival, ZBool.False, amsHeaderBO.NVOCCActualArrivalDate);
			return dateCollection;
		}

		void PopulateTransportMode(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper headerHelper)
		{
			var transportModeCode = headerBO.BH_ImportTransportMode;
			if (!transportModeCode.IsEmpty)
			{
				var containerCode = ZString.Empty;

				switch (transportModeCode)
				{
					case InBondTransportModeCodes.Codes.VesselNonContainer:
						transportModeCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
						break;
					case InBondTransportModeCodes.Codes.VesselContainer:
						transportModeCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
						containerCode = Enterprise.Customs.US.Business.ContainerModeList.Codes.Containerized;
						break;
					case InBondTransportModeCodes.Codes.RailNonContainer:
						transportModeCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail;
						break;
				}

				headerData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportModeCode, headerHelper.TransportTypeList);
				headerData.CustomsContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerCode, headerHelper.ContainerModeList);
			}
		}

		void PopulatePortArrivalDates(CusInBondHeader headerBO, Shipment headerData)
		{
			headerData.SetAddInfoGroupCollection(() =>
			{
				var addInfoGroupCollection = headerData.AddInfoGroupCollection ?? new List<UniversalCustoms.AddInfoGroup>();
				foreach (PortArrivalDetail arrivalDetail in headerBO.PortArrivalDetails)
				{
					headerData.AddInfoGroupCollection.Add(new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = Constants.PortArrivalDate.GroupTypeCode, Description = Constants.PortArrivalDate.GroupTypeDescription },
						AddInfoCollection = new List<AddInfo>()
						{
							new AddInfo() { Key = Constants.PortArrivalDate.AddInfo.PortCode, Value = arrivalDetail.PortCode },
							new AddInfo() { Key = Constants.PortArrivalDate.AddInfo.ArrivalDate, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(arrivalDetail.ActualArrivalDate) }
						}
					});
				}

				return addInfoGroupCollection;
			});
		}

		protected override DataTransfer.Universal.CusInBondBillDataObjectWriter InBondBillDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondBillDataObjectWriter(writeManager, (InBondDataObjectWriterHelper)helper);
		}

		protected override DataTransfer.Universal.InBondDataObjectWriterHelper GetWriterHelperCore(Customs.Business.CusInBondHeader headerBO)
		{
			return new InBondDataObjectWriterHelper((CusInBondHeader)headerBO);
		}

		protected override DataTransfer.Universal.CusInBondMoveHeaderDataObjectWriter InBondMoveHeaderDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper, Shipment headerData)
		{
			return new CusInBondMoveHeaderDataObjectWriter(writeManager, (InBondDataObjectWriterHelper)helper, headerData);
		}

		protected override IEnumerable<Customs.Business.CusInBondMoveHeader> GetRelatedMoveHeaders(Customs.Business.CusInBondHeader headerBO, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			var moveHeadersCollection = base.GetRelatedMoveHeaders(headerBO, headerHelper).ToList();
			var amsHeaderBO = (CusInBondHeader)headerBO;
			moveHeadersCollection.AddRange(headerHelper.Load<CusInBondMoveHeader>(amsHeaderBO.PTTMovements.CompleteFilter));
			moveHeadersCollection.AddRange(headerHelper.Load<CusInBondMoveHeader>(amsHeaderBO.InBondMovementHeaders.CompleteFilter));
			return moveHeadersCollection;
		}
	}
}

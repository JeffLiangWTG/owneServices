using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using InBondTransportModeCodes = Enterprise.Customs.US.Business.InBondTransportModeCodes;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondHeaderDataObjectWriter : DataTransfer.Universal.CusInBondHeaderDataObjectWriter
	{
		public CusInBondHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondHeader headerBO, Shipment headerData, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			base.PopulateInBondSpecificData(headerBO, headerData, headerHelper);
			var inBondHeaderBO = (CusInBondHeader)headerBO;
			var inBondHeaderHelper = (InBondDataObjectWriterHelper)headerHelper;
			PopulateTransportModeAndContainerMode(inBondHeaderBO, headerData, inBondHeaderHelper);
			headerData.VesselName = inBondHeaderBO.BH_ImportConveyanceName;
			headerData.VesselCountryOfRegistration = ListHelper.GetWithName<Country>(inBondHeaderBO.BH_ImportConveyanceCountry, inBondHeaderBO.Lookups.Countries);
			var refUNLOCOList = headerBO.Factory.GetRefUNLOCOList();
			headerData.PortOfLoading = ListHelper.GetWithName(inBondHeaderBO.BH_Calc_ImportLoadPortUNLOCO, refUNLOCOList);
			headerData.PortOfDischarge = ListHelper.GetWithName(inBondHeaderBO.BH_Calc_PortUnladingUNLOCO, refUNLOCOList);
			PopulateImporterAndSupplier(inBondHeaderBO, headerData);
			PopulateTransportLegs(inBondHeaderBO, headerData);
			headerData.SetContainerCollection(() => new DataObjectList<Container>());
			headerData.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			if (!inBondHeaderBO.IsAir)
			{
				headerData.CommercialInfo = new UniversalXml.Customs.CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalXml.Customs.CommercialInvoiceHeader>(new[]
					{
						new UniversalXml.Customs.CommercialInvoiceHeader(writeManager.WriterStrategy)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<UniversalXml.Customs.CommercialInvoiceLine>()))
					})
				};
			}
		}

		protected override IEnumerable<Customs.Business.CusInBondBill> GetRelatedBills(Customs.Business.CusInBondHeader headerBO, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			return base.GetRelatedBills(headerBO, headerHelper).OfType<CusInBondBill>().OrderBy(x => x.BillUniqueCode);
		}

		protected override IEnumerable<Customs.Business.CusInBondMoveHeader> GetRelatedMoveHeaders(Customs.Business.CusInBondHeader headerBO, DataTransfer.Universal.InBondDataObjectWriterHelper headerHelper)
		{
			return base.GetRelatedMoveHeaders(headerBO, headerHelper).OfType<CusInBondMoveHeader>().OrderBy(x => x.InBondNumber);
		}

		internal void PopulateMainData(CusInBondHeader headerBO, Shipment headerData, CusInBondMoveHeader warehouseMovement)
		{
			var writerHelper = (InBondDataObjectWriterHelper)WriterHelper(headerBO);
			PopulateMainData(headerBO, headerData, writerHelper);

			if (warehouseMovement != null)
			{
				PopulateWarehouseMainData(headerBO, headerData, writerHelper, warehouseMovement);
			}
		}

		void PopulateWarehouseMainData(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper writerHelper, CusInBondMoveHeader warehouseMovement)
		{
			PopulateWarehouseAdditionalBillData(headerBO, headerData, writerHelper, warehouseMovement);
			PopulateWarehouseMoveHeaderData(headerBO, headerData, writerHelper, warehouseMovement);
		}

		void PopulateWarehouseAdditionalBillData(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper headerHelper, CusInBondMoveHeader warehouseMovement)
		{
			var query = headerBO.Bills.CompleteFilter;
			query.AddToFilter(CusInBondBillSchema.PK, headerHelper.Load<CusInBondMoveDetail>(warehouseMovement.MovementDetails.CompleteFilter).Select(x => x.B9_B0));
			var bills = headerHelper.Load<CusInBondBill>(query).OrderBy(x => x.BillUniqueCode);
			headerData.SetAdditionalBillCollection(() => ProcessCollection(bills, new CusInBondBillDataObjectWriter(writeManager, headerHelper)));
		}

		void PopulateWarehouseMoveHeaderData(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper headerHelper, CusInBondMoveHeader warehouseMovement)
		{
			headerData.MessageType = new CodeDescriptionPair() { Code = CusInBondApplicationCodeList.Codes.InBond, Description = CusInBondApplicationCodeList.Descriptions.InBond };
			var warehouseAddress = warehouseMovement.WarehouseAddress;
			if (warehouseAddress != null)
			{
				headerData.AddOrgAddress(writeManager, warehouseAddress, DocAddressType.CustomsWarehouseAddress);
			}
			headerData.SetInBondMoveHeaderCollection(() => ProcessCollection(new[] { warehouseMovement }, new CusInBondMoveHeaderDataObjectWriter(writeManager, headerData, headerHelper, true)));
		}

		void PopulateTransportLegs(CusInBondHeader headerBO, Shipment headerData)
		{
			headerData.SetTransportLegCollection(() => new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete });
			var mainLeg = new TransportLeg(writeManager.WriterStrategy)
			{
				LegOrder = 1,
				TransportMode = GetTransportMode(headerBO.BH_ImportTransportMode),
				VesselName = headerData.VesselName,
				VesselLloydsIMO = headerData.LloydsIMO,
				VoyageFlightNo = headerData.VoyageFlightNo,
				PortOfLoading = headerData.PortOfLoading,
				ActualDeparture = headerBO.BH_SailingDate,
				PortOfDischarge = headerData.PortOfDischarge,
				EstimatedArrival = headerBO.BH_ETA
			};
			if ((!headerBO.BH_RN_NKFirstExportCountry.IsEmpty && headerBO.BH_RN_NKFirstExportCountry != headerBO.BH_Calc_ImportLoadPortUNLOCO.Left(2)) || (!headerBO.BH_FirstExportDate.IsEmpty && headerBO.BH_FirstExportDate != headerBO.BH_SailingDate))
			{
				mainLeg.LegOrder = 2;
				var preCarriageLeg = new TransportLeg(writeManager.WriterStrategy)
				{
					LegOrder = 1,
					ActualDeparture = headerBO.BH_FirstExportDate
				};
				var firstExportCountry = headerBO.FirstExportCountry;
				if (firstExportCountry != null)
				{
					preCarriageLeg.PortOfLoading = new UNLOCO() { Code = firstExportCountry.RN_Code, Name = firstExportCountry.RN_DescMultilingual };
				}
				headerData.TransportLegCollection?.Add(preCarriageLeg);
			}
			headerData.TransportLegCollection?.Add(mainLeg);
		}

		TransportMode? GetTransportMode(ZString code)
		{
			TransportMode? result = null;
			switch (code)
			{
				case InBondTransportModeCodes.Codes.VesselContainer:
				case InBondTransportModeCodes.Codes.VesselNonContainer:
					result = TransportMode.Sea;
					break;
				case InBondTransportModeCodes.Codes.RailNonContainer:
					result = TransportMode.Rail;
					break;
				case InBondTransportModeCodes.Codes.TruckNonContainer:
					result = TransportMode.Road;
					break;
				case InBondTransportModeCodes.Codes.AirNonContainer:
					result = TransportMode.Air;
					break;
			}
			return result;
		}

		void PopulateImporterAndSupplier(CusInBondHeader headerBO, Shipment headerData)
		{
			var importerAddress = headerBO.Importer;
			if (importerAddress != null)
			{
				headerData.AddOrgAddress(writeManager, importerAddress, DocAddressType.ImporterDocumentaryAddress);
			}
			var supplier = headerBO.Supplier;
			if (supplier != null)
			{
				headerData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);
			}
		}

		void PopulateTransportModeAndContainerMode(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper headerHelper)
		{
			var transportCode = headerBO.BH_ImportTransportMode;
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

				headerData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportCode, headerHelper.TransportTypeList);
				headerData.CustomsContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerCode, headerHelper.ContainerModeList);
			}
		}

		protected override List<Date> PopulateDatesData(Customs.Business.CusInBondHeader headerBO, Shipment headerData)
		{
			var dateCollection = base.PopulateDatesData(headerBO, headerData) ?? new List<Date>();
			dateCollection.Add(DateType.Arrival, ZBool.False, headerBO.BH_ETA);
			dateCollection.Add(DateType.Departure, ZBool.True, headerBO.BH_SailingDate);
			return dateCollection;
		}

		protected override List<AddInfo> PopulateAddInfosData(Customs.Business.CusInBondHeader headerBO, Shipment headerData)
		{
			var list = base.PopulateAddInfosData(headerBO, headerData) ?? new List<AddInfo>();
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.FTZMove,
				Value = headerBO.BH_FTZMove ? Constants.AddInfo.True : Constants.AddInfo.False
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.UI_NKCarrierSCAC,
				Value = headerBO.BH_CarrierSCAC
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.SchDArrival,
				Value = headerBO.BH_PortUnladingDCode
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.US_NKLocationOfGoods,
				Value = headerBO.BH_FIRMS
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.InBondMode,
				Value = headerBO.BH_HeaderType
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Header.AddInfo.SchDLoading,
				Value = headerBO.BH_ImportLoadPortKCode
			});
			return list;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.InBond;
		}

		protected override DataTransfer.Universal.CusInBondBillDataObjectWriter InBondBillDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondBillDataObjectWriter(writeManager, (InBondDataObjectWriterHelper)helper);
		}

		protected override DataTransfer.Universal.CusInBondMoveHeaderDataObjectWriter InBondMoveHeaderDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper, Shipment headerData)
		{
			return new CusInBondMoveHeaderDataObjectWriter(writeManager, headerData, (InBondDataObjectWriterHelper)helper);
		}

		protected override DataTransfer.Universal.InBondDataObjectWriterHelper GetWriterHelperCore(Customs.Business.CusInBondHeader headerBO)
		{
			return new InBondDataObjectWriterHelper((CusInBondHeader)headerBO);
		}
	}
}

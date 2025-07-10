using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.ZA
{
	public sealed class CargoDuesDataObjectWriter : DataObjectWriter<CargoDues, UniversalShipment>
	{
		public CargoDuesDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CargoDues cargoDues)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);
			uxmlShipment.DataContext = cargoDues.CreateUXmlDataContext();

			uxmlShipment.WayBillNumber = cargoDues.WayBillNumber;
			uxmlShipment.ContainerMode = cargoDues.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.TotalNoOfPacks = cargoDues.TotalNumberOfPacks;

			PopulateAddresses(cargoDues, uxmlShipment);
			PopulatePorts(cargoDues, uxmlShipment);
			PopulateTransportLegs(cargoDues, uxmlShipment);
			PopulateGoodsDetails(cargoDues, uxmlShipment);
			PopulateContainers(cargoDues, uxmlShipment);
			PopulateDates(cargoDues, uxmlShipment);
			PopulateAddInfos(cargoDues, uxmlShipment);
			
			return uxmlShipment;
		}

		#region Addresses

		void PopulateAddresses(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!cargoDues.Agent.IsEmpty())
				{
					addresses.Add(cargoDues.Agent.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy));
					addresses.Add(cargoDues.Agent.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!cargoDues.ShippingLine.IsEmpty())
				{
					addresses.Add(cargoDues.ShippingLine.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy));
				}

				if (!cargoDues.ArrivalCTO.IsEmpty())
				{
					addresses.Add(cargoDues.ArrivalCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy));
				}

				if (!cargoDues.DepartureCTO.IsEmpty())
				{
					addresses.Add(cargoDues.ArrivalCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy));
				}

				if (!cargoDues.CustomsContainerTerminalOperator.IsEmpty())
				{
					addresses.Add(cargoDues.ArrivalCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.CustomsContainerTerminalOperatorAddress), writeManager.WriterStrategy));
				}

				if (!cargoDues.CurrentUser.IsEmpty())
				{
					addresses.Add(cargoDues.CurrentUser.ToUXmlOrganizationAddress(CargoDuesDataObjectWriterConstants.CargoDuesAddressType.CurrentUser, writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region Ports

		void PopulatePorts(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			uxmlShipment.PortOfLoading = cargoDues.PortOfLoading.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = cargoDues.PortOfDischarge.ToUXmlUnloco();
			uxmlShipment.PlaceOfReceipt = cargoDues.PlaceOfReceipt.ToUXmlUnloco();
			uxmlShipment.PlaceOfDelivery = cargoDues.PlaceOfDelivery.ToUXmlUnloco();
		}

		#endregion

		#region Transports

		void PopulateTransportLegs(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			if (cargoDues.Transports?.Any() ?? false)
			{
				uxmlShipment.SetTransportLegCollection(() =>
				{
					var result = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };

					foreach (Transport transport in cargoDues.Transports)
					{
						var transportDO = transport.ToUXmlTransportLeg(writeManager.WriterStrategy);

						if (!transport.ArrivalAt.IsEmpty())
						{
							transportDO.ArrivalCTO = transport.ArrivalAt.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy);
						}

						if (!transport.DepartureFrom.IsEmpty())
						{
							transportDO.DepartureCTO = transport.DepartureFrom.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy);
						}

						result.Add(transportDO);
					}

					return result;
				});
			}
		}

		#endregion

		#region Goods Details

		void PopulateGoodsDetails(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			if (cargoDues.IsContainerised || cargoDues.ShipmentPackingInfos == null)
			{
				return;
			}

			uxmlShipment.SetSubShipmentCollection(() =>
			{
				var subShipments = new DataObjectList<UniversalShipment>();

				foreach (var shipmentDO in cargoDues.ShipmentPackingInfos)
				{
					var uxmlSubShipment = new UniversalShipment(writeManager.WriterStrategy);
					uxmlSubShipment.TotalWeight = shipmentDO.TotalWeight?.Value;
					uxmlSubShipment.TotalWeightUnit = new UnitOfWeight
					{
						Code = shipmentDO.TotalWeight?.Unit?.Code,
						Description = shipmentDO.TotalWeight?.Unit?.Description
					};
					uxmlSubShipment.OuterPacks = shipmentDO.OuterPacks;
					uxmlSubShipment.OuterPacksPackageType = shipmentDO.PackType.ToUXmlPackType();

					PopulateGoodsDetailPackingLines(shipmentDO, uxmlSubShipment);
					PopulateGoodsDetailAddresses(shipmentDO, uxmlSubShipment);

					subShipments.Add(uxmlSubShipment);
				}

				return subShipments.Any() ? subShipments : null;
			});
		}

		void PopulateGoodsDetailPackingLines(ShipmentPackingInfo shipmentPackingInfo, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetPackingLineCollection(() =>
			{
				var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();
				foreach (var goodsInfo in shipmentPackingInfo.GoodsInfoCollection)
				{
					var packingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine(writeManager.WriterStrategy)
					{
						MarksAndNos = goodsInfo.MarksAndNos,
						PackQty = new ZLong(goodsInfo.NumberOfPacks),
						PackType = new PackageType()
						{
							Code = goodsInfo.PackType
						},
						GoodsDescription = goodsInfo.GoodsDescription,
						Weight = goodsInfo.GrossMass.Value,
						WeightUnit = goodsInfo.GrossMass.Unit.ToUXmlUnitOfWeight()
					};

					uxmlPackingLines.Add(packingLine);
				}

				return uxmlPackingLines.Any() ? uxmlPackingLines : null;
			});
		}

		void PopulateGoodsDetailAddresses(ShipmentPackingInfo shipmentPackingInfo, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!shipmentPackingInfo.Consignee.IsEmpty())
				{
					addresses.Add(shipmentPackingInfo.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!shipmentPackingInfo.Consignor.IsEmpty())
				{
					addresses.Add(shipmentPackingInfo.Consignor.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region Containers

		void PopulateContainers(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			if (!cargoDues.IsContainerised)
			{
				return;
			}

			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			foreach (var container in cargoDues.Containers)
			{
				uxmlContainers.Add(container.ToUXmlContainer(writeManager.WriterStrategy));
			}

			if (uxmlContainers.Any())
			{
				uxmlShipment.SetContainerCollection(() => uxmlContainers);
			}
		}

		#endregion

		#region Dates

		void PopulateDates(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();

				AddDate(dates, cargoDues.Eta, DateType.Arrival, true);
				AddDate(dates, cargoDues.Etd, DateType.Departure, true);

				return dates.Any() ? dates : null;
			});
		}

		void AddDate(List<Date> dates, ZDateTime dateTimeToBeAdded, DateType dateType, bool isEstimate = false)
		{
			if (!dateTimeToBeAdded.IsEmpty)
			{
				dates.Add(Date.New(dateType, isEstimate, dateTimeToBeAdded));
			}
		}

		#endregion

		#region AddInfos

		void PopulateAddInfos(CargoDues cargoDues, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.CarrierCode, cargoDues.CarrierCode);

				if (!cargoDues.ServicePort.IsEmpty())
				{
					addInfos.AddRange(cargoDues.ServicePort.ToUXmlAddInfos(nameof(cargoDues.ServicePort)));
				}

				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.ContainerOperator, cargoDues.ContainerOperator);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.Terminal, cargoDues.Terminal);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.TNPAArrivalNumber, cargoDues.TNPAArrivalNumber);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.TNPAOrderNumber, cargoDues.TNPAOrderNumber);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.TNPAQuotationNumber, cargoDues.TNPAQuotationNumber);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.TNPAAccountNumber, cargoDues.TNPAAccountNumber);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.RadioCallSign, cargoDues.RadioCallSign);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.SubTotal, cargoDues.SubTotal.ToString());
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.VAT, cargoDues.VAT.ToString());
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.TotalR, cargoDues.TotalR.ToString());
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.CancellingOrder, cargoDues.CancellingOrder.ToString());
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.IsTranship, cargoDues.IsTranship.ToString());
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.ServicePortCode, cargoDues.ServicePort?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, CargoDuesDataObjectWriterConstants.AddInfoKeys.ServicePortName, cargoDues.ServicePort?.Name ?? ZString.Empty);

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addInfos, ZString key, ZString value)
		{
			if (!key.IsEmpty
				&& !value.IsEmpty
				&& addInfos.All(x => x.Key.HasValue && x.Key.Value != key))
			{
				var addInfo = AddInfo.New(key, value);
				addInfos.Add(addInfo);
			}
		}

		#endregion
	}
}

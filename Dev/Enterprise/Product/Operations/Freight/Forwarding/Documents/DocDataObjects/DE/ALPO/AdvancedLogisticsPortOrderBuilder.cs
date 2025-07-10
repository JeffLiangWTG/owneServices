using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	sealed class AdvancedLogisticsPortOrderBuilder
	{
		public AdvancedLogisticsPortOrderBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		public AdvancedLogisticsPortOrder Build()
		{
			var advancedLogisticsPortOrder = new AdvancedLogisticsPortOrder(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			PopulateHeaderInformation(advancedLogisticsPortOrder);
			PopulateParties(advancedLogisticsPortOrder);
			PopulateContainerAndGoodsDetails(advancedLogisticsPortOrder);
			PopulateShipments(advancedLogisticsPortOrder);

			AddContainerValidation(advancedLogisticsPortOrder);
			AddPackingLineValidation(advancedLogisticsPortOrder);
			AddErrorPlaceHolderValidations(advancedLogisticsPortOrder);

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			return advancedLogisticsPortOrder;
		}

		#region PopulateHeaderInformation
		void PopulateHeaderInformation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			advancedLogisticsPortOrder.ConsolNumber = consol.JK_UniqueConsignRef;
			advancedLogisticsPortOrder.Direction = AdvancedLogisticsPortOrderHelper.IsALPOExport(consol) ? Constants.FreightShipmentDirection.Description.Export : Constants.FreightShipmentDirection.Description.Import;

			var fclModes = new[] {
				Constants.ContainerModes.FCL,
				Constants.ContainerModes.Groupage,
				Constants.ContainerModes.BuyersConsol,
				Constants.ContainerModes.ShippersConsol,
				Constants.ContainerModes.Other
			};

			advancedLogisticsPortOrder.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};

			advancedLogisticsPortOrder.OperationalPort = Unloco.Create(context, AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));
			advancedLogisticsPortOrder.PortOfOrigin = Unloco.Create(context, AdvancedLogisticsPortOrderHelper.GetOrigin(consol));
			advancedLogisticsPortOrder.PortOfDestination = Unloco.Create(context, AdvancedLogisticsPortOrderHelper.GetDestination(consol));
			var transport = AdvancedLogisticsPortOrderHelper.GetMainTransport(consol);
			if (transport != null)
			{
				advancedLogisticsPortOrder.SisNumber = advancedLogisticsPortOrder.Direction.EqualsIgnoringCase(Constants.FreightShipmentDirection.Description.Import.ToString()) ? transport.JW_ArrivalPortRouteId : transport.JW_DeparturePortRouteId;
			}

			PopulateReferences(advancedLogisticsPortOrder);
			PopulateMainTransportInformation(advancedLogisticsPortOrder);
			PopulatePreCarriageInformation(advancedLogisticsPortOrder);

			AddHeaderValidation(advancedLogisticsPortOrder);
		}

		void PopulateReferences(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			advancedLogisticsPortOrder.BillOfLading = consol.JK_MasterBillNum;
			advancedLogisticsPortOrder.CarrierBookingReference = consol.JK_BookingReference;
			advancedLogisticsPortOrder.FreightForwarderReference = consol.JK_UniqueConsignRef;
			advancedLogisticsPortOrder.MarksAndNumbers = consol.ShipmentCount != 1 ? ZString.Empty : consol.Shipments[0].JS_MarksAndNumbers;
			PopulateAlpoReferenceAndUserId(advancedLogisticsPortOrder);
		}

		void PopulateAlpoReferenceAndUserId(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var consolCustomNumbers = consol.Numbers.Where(v => v is CusEntryNumber).Cast<CusEntryNumber>().ToList();
			advancedLogisticsPortOrder.ALPOReference = consolCustomNumbers.FirstOrDefault(number => number.CE_EntryType.EqualsIgnoringCase(GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber))?.CE_EntryNum ?? ZString.Empty;

			var currentUser = GlbStaff.CurrentUser;
			var certificate = currentUser.Certificates.GetFirstCertificate(Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH);
			if (certificate != null)
			{
				advancedLogisticsPortOrder.ALPOUserID = certificate.XZ_RefNumber;
			}
		}

		void PopulateMainTransportInformation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var mainTransport = AdvancedLogisticsPortOrderHelper.GetMainTransport(consol);
			if (mainTransport != null)
			{
				advancedLogisticsPortOrder.PortOfLoading = Unloco.Create(context, mainTransport.LoadPort);
				advancedLogisticsPortOrder.PortOfDischarge = Unloco.Create(context, mainTransport.DiscPort);
				advancedLogisticsPortOrder.Vessel = Transport.Create(context, mainTransport).Vessel;
				advancedLogisticsPortOrder.VoyageFlightNo = mainTransport?.JW_VoyageFlight ?? consol.JK_JX_JV_VoyageFlight;
				advancedLogisticsPortOrder.ETA = mainTransport.JW_ETA;
				advancedLogisticsPortOrder.ETD = mainTransport.JW_ETD;
			}
			else
			{
				advancedLogisticsPortOrder.PortOfLoading = Unloco.Create(context, consol.LoadPort);
				advancedLogisticsPortOrder.PortOfDischarge = Unloco.Create(context, consol.DischargePort);
			}
		}

		void PopulatePreCarriageInformation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			Freight.Business.Transport transport = null;
			if (advancedLogisticsPortOrder.Direction.EqualsIgnoringCase(Constants.FreightShipmentDirection.Description.Import.ToString()))
			{
				var operationPortSeaLeg = consol.Transports.Cast<Freight.Business.Transport>().FirstOrDefault(t => t.JW_TransportMode == Constants.TransportModes.Sea && t.JW_RL_NKDiscPort == advancedLogisticsPortOrder.OperationalPort.Code);
				if (operationPortSeaLeg != null)
				{
					transport = consol.Transports.Cast<Freight.Business.Transport>().FirstOrDefault(t => t.JW_LegOrder > operationPortSeaLeg.JW_LegOrder);
				}
			}
			else
			{
				var operationPortSeaLeg = consol.Transports.Cast<Freight.Business.Transport>().FirstOrDefault(t => t.JW_TransportMode == Constants.TransportModes.Sea && t.JW_RL_NKLoadPort == advancedLogisticsPortOrder.OperationalPort.Code);
				if (operationPortSeaLeg != null)
				{
					transport = consol.Transports.Cast<Freight.Business.Transport>().LastOrDefault(t => t.JW_LegOrder < operationPortSeaLeg.JW_LegOrder);
				}
			}

			advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding =
				new CodeDescription(context.TransportModes)
				{
					Code = transport?.JW_TransportMode ?? Constants.TransportModes.Road
				};

			if (transport != null)
			{
				PopulatePreCarriageOrOnForwardingID(advancedLogisticsPortOrder, transport);
			}
		}

		void PopulatePreCarriageOrOnForwardingID(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, Freight.Business.Transport transport)
		{
			switch (transport.JW_TransportMode)
			{
				case Constants.TransportModes.Road:
				case Constants.TransportModes.Rail:
					advancedLogisticsPortOrder.PreCarriageOrOnForwardingID = transport.JW_VoyageFlight;
					break;
				case Constants.TransportModes.Sea:
				case Constants.TransportModes.InlandWaterwayTransport:
					advancedLogisticsPortOrder.PreCarriageOrOnForwardingID = transport.JW_Vessel;
					break;
			}
		}

		void AddHeaderValidation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var operationalPortErrorMessage = Res.GetString("0c792f2a-e4ba-434f-a873-db4ed76783e4", "Operational port can only be 'DEBRE', 'DEBRV', 'DECUX', 'DEWVN' or 'DEHAM'.");
			var portOfLoadingErrorMessage = Res.GetString("7d8968bf-a482-4336-a233-dcb22c1b5da9", "Port of Loading is required.");
			var portOfDischargeErrorMessage = Res.GetString("7fae5320-8a8b-44d3-aca2-9cdcebe59741", "Port of Discharge is required.");
			var vesselNameErrorMessage = Res.GetString("c43e46a3-de89-4fd0-8c1d-cfe368541630", "Vessel Name is required.");
			var transportETDErrorMessage = Res.GetString("b110f1cd-c9da-4ef6-817a-56b856c12792", "ETD is required.");
			var alpoUserIdErrorMessage = Res.GetString("2AC7EDC9-F858-4E14-9757-E9D2D8E5BA7F", "ALPO User Id is required. Specify DBH user code of logged on user (Human Resources > Certificates and ID Numbers tab in Staff and Resources)");

			var preCarriageOrOnForwarding = string.Empty;

			if (advancedLogisticsPortOrder.Direction.EqualsIgnoringCase(Constants.FreightShipmentDirection.Description.Import.ToString()))
			{
				preCarriageOrOnForwarding = Res.GetString("4651fa97-fe2e-4650-8b70-f0495c29886c", "On-Forwarding");
			}
			else
			{
				preCarriageOrOnForwarding = Res.GetString("0371722d-5df5-48d1-9384-65286556b177", "Pre-Carriage");
			}

			var precarriageIDErrorMessage = Res.GetString("d3ac6a8b-1095-4477-82ec-e00a38ddd440", "{0} ID is required.", preCarriageOrOnForwarding);

			((Unloco)advancedLogisticsPortOrder.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(operationalPortErrorMessage);
			((Vessel)advancedLogisticsPortOrder.Vessel)?.NameInfo.AddMessageErrorIfEmpty(vesselNameErrorMessage);
			advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo.AddMessageError(
				() => IDIsMandatory(advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding?.Code) && advancedLogisticsPortOrder.PreCarriageOrOnForwardingID.IsEmpty, precarriageIDErrorMessage);

			advancedLogisticsPortOrder.ALPOUserIDInfo?.AddMessageErrorIfEmpty(alpoUserIdErrorMessage);

			((Unloco)advancedLogisticsPortOrder.PortOfLoading)?.CodeInfo.AddMessageError(() => advancedLogisticsPortOrder.SisNumber.IsEmpty && string.IsNullOrEmpty(((Unloco)advancedLogisticsPortOrder.PortOfLoading)?.Code ?? string.Empty), portOfLoadingErrorMessage);
			((Unloco)advancedLogisticsPortOrder.PortOfDischarge)?.CodeInfo.AddMessageError(() => advancedLogisticsPortOrder.SisNumber.IsEmpty && string.IsNullOrEmpty(((Unloco)advancedLogisticsPortOrder.PortOfDischarge)?.Code ?? string.Empty), portOfDischargeErrorMessage);
			advancedLogisticsPortOrder.ETDInfo.AddMessageError(() => (advancedLogisticsPortOrder.SisNumber.IsEmpty && advancedLogisticsPortOrder.ETD.IsEmpty), transportETDErrorMessage);

			advancedLogisticsPortOrder.SisNumberInfo.ValueChanged += (s, e) =>
			{
				((Unloco)advancedLogisticsPortOrder.PortOfLoading)?.ValidateAllIncludingChildren();
				((Unloco)advancedLogisticsPortOrder.PortOfDischarge)?.ValidateAllIncludingChildren();
				advancedLogisticsPortOrder.Validate(nameof(advancedLogisticsPortOrder.ETD));
			};
		}

		void AddErrorPlaceHolderValidations(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			if (!PackingLinesPresent(advancedLogisticsPortOrder))
			{
				advancedLogisticsPortOrder.ErrorPlaceHolderInfo.AddMessageError(() => true, Res.GetString("1B60E1E6-A8EF-4A52-8193-45CEF7BACD41", "No Packing Lines found! Advanced Logistics Port Order cannot be created!"));
			}

			var fclModes = new[] {
					Constants.ContainerModes.FCL,
					Constants.ContainerModes.LCL,
					Constants.ContainerModes.BuyersConsol,
					Constants.ContainerModes.Groupage
			};

			advancedLogisticsPortOrder.ErrorPlaceHolderInfo.AddMessageError(() => fclModes.Contains<string>(consol.JK_ConsolMode) && !advancedLogisticsPortOrder.Containers.Any(), Res.GetString("9992E1E3-D569-48B6-A8BE-A3BC58E00764", "Container is required"));
		}

		bool PackingLinesPresent(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			return advancedLogisticsPortOrder.Shipments.Any(c => c.PackingLines != null && c.PackingLines.Any());
		}

		bool IDIsMandatory(string transportMode)
		{
			var transportModeList = new string[] { Constants.TransportModes.Sea, Constants.TransportModes.Rail, Constants.TransportModes.InlandWaterwayTransport };
			return transportModeList.Contains(transportMode);
		}

		#endregion PopulateHeaderInformation

		#region Parties

		void PopulateParties(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			advancedLogisticsPortOrder.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			var direction = advancedLogisticsPortOrder.Direction;
			var operationalPort = AdvancedLogisticsPortOrderHelper.GetALPOPort(consol);
			if (operationalPort != null)
			{
				advancedLogisticsPortOrder.CarrierCode = consol.ShippingLineAddress.GetPortCarrierCode(operationalPort.Code, context);
			}

			if (IsImport(direction))
			{
				advancedLogisticsPortOrder.Forwarder = consol.ReceivingForwarderWithContact != null
					? AddressBuilder.Create(context, consol.ReceivingForwarderWithContact)
					: AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);

				advancedLogisticsPortOrder.EoriNumber = consol.ReceivingForwarderAddress.GetEoriNumber(context, Constants.CountryCodes.Germany);
				advancedLogisticsPortOrder.EoriBranchSuffix = consol.ReceivingForwarderAddress.GetEoriBranchSuffix(context, Constants.CountryCodes.Germany);
			}
			else
			{
				advancedLogisticsPortOrder.Forwarder = consol.SendingForwarderWithContact != null
					? AddressBuilder.Create(context, consol.SendingForwarderWithContact)
					: AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);

				advancedLogisticsPortOrder.EoriNumber = consol.SendingForwarderAddress.GetEoriNumber(context, Constants.CountryCodes.Germany);
				advancedLogisticsPortOrder.EoriBranchSuffix = consol.SendingForwarderAddress.GetEoriBranchSuffix(context, Constants.CountryCodes.Germany);
			}

			advancedLogisticsPortOrder.SendingParty = AddressBuilder.CreateForCurrentUser(context);

			if (IsImport(direction))
			{
				advancedLogisticsPortOrder.CTO = AddressBuilder.Create(context, consol.ArrivalCTOAddress);
				advancedLogisticsPortOrder.CTOWarehouseCode = consol.ArrivalCTOAddress.GetWarehouseCode(context);
			}
			else
			{
				advancedLogisticsPortOrder.CTO = AddressBuilder.Create(context, consol.DepartureCTOAddress);
				advancedLogisticsPortOrder.CTOWarehouseCode = consol.DepartureCTOAddress.GetWarehouseCode(context);
			}
		}

		#endregion Parties

		#region Goods and Equipment Details

		void PopulateContainerAndGoodsDetails(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var containers = new List<Container>();

			var containerBuilder = new ContainerBuilder();
			var packingLineBuilder = new PackingLineBuilder();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				if (containerBizObj.JC_ContainerMode == Constants.ContainerModes.FCL
					|| containerBizObj.JC_ContainerMode == Constants.ContainerModes.LCL
					|| containerBizObj.JC_ContainerMode == Constants.ContainerModes.BuyersConsol
					|| containerBizObj.JC_ContainerMode == Constants.ContainerModes.ShippersConsol
					|| containerBizObj.JC_ContainerMode == Constants.ContainerModes.Groupage)
				{
					var packingLines = new List<PackingLine>();

					foreach (var packlineBizObj in containerBizObj.PackLines.OfType<ForwardingPackLine>())
					{
						var packingLine = PopolatePackingLine(packlineBizObj, packingLineBuilder, advancedLogisticsPortOrder.Direction);

						packingLines.Add(packingLine);
					}

					var container = containerBuilder.Build(containerBizObj, context, packingLines.ToArray());

					containers.Add(container);
				}
			}
			advancedLogisticsPortOrder.Containers = new ReadOnlyCollection<Container>(containers);
		}

		void SetDefaultValuesForAESCustomsData(List<PackingLine> packingLines, ZString direction)
		{
			var import = IsImport(direction);
			var packLineGroups = packingLines.GroupBy(pl => new { pl.DocumentNoLabel, ReferenceNumber = import ? pl.ImportReferenceNumber : pl.ExportReferenceNumber, pl.ItemNumber });

			foreach (var packLines in packLineGroups)
			{
				if (packLines.Count() > 1)
				{
					var sortedPackLines = packLines.OrderBy(x => x.ContainerNumber).ThenBy(x => x.ItemNumber).ThenBy(x => x.ShipmentID).ToList();

					var counter = 1;
					foreach (var packLine in sortedPackLines)
					{
						packLine.CargoItem = packLine.ItemNumber.ToString();
						packLine.PackageNumber = packLine.PackageNumberOriginalValue = counter.ToString();
						packLine.Complete = false;
						packLine.Shortage = false;
						counter++;
					}

					sortedPackLines.Last().Complete = true;
				}
				else
				{
					var packLine = packLines.First();
					packLine.CargoItem = packingLines.Any(pl => pl.Identifier != packLine.Identifier && pl.DocumentNoLabel == packLine.DocumentNoLabel && (import ? pl.ImportReferenceNumber == packLine.ImportReferenceNumber : pl.ExportReferenceNumber == packLine.ExportReferenceNumber))
						? packLine.ItemNumber.ToString()
						: ZString.Empty;
					packLine.PackageNumber = packLine.PackageNumberOriginalValue = ZString.Empty;
					packLine.Complete = true;
					packLine.Shortage = false;
				}
			}
		}

		void PopulateShipments(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var packingLineBuilder = new PackingLineBuilder();

			var packLineDOs = advancedLogisticsPortOrder.Containers.Count == 0 ?
				consol.UnAllocatedPackLines.Cast<PackLine>().Select(x => PopolatePackingLine(x, packingLineBuilder, advancedLogisticsPortOrder.Direction)).ToList() :
				advancedLogisticsPortOrder.Containers.SelectMany(x => x.PackingLines).ToList();

			SetDefaultValuesForAESCustomsData(packLineDOs, advancedLogisticsPortOrder.Direction);

			var shipmentBOs = consol.Shipments.Cast<ForwardingShipment>().Where(x => x.CanHaveOwnPackLines);
			var shipments = new List<Shipment>();
			var shipmentBuilder = new ShipmentBuilder(context);

			foreach (var group in packLineDOs.GroupBy(x => x.ShipmentID))
			{
				var shipmentBO = shipmentBOs.First(x => x.JS_UniqueConsignRef == group.Key);
				var shipmentDO = shipmentBuilder.Build(shipmentBO, (shipment) => group.ToList());

				shipmentDO.ConsignorEoriNumber = shipmentBO.ConsignorDocumentaryAddress.RealAddress.GetEoriNumber(context, Constants.CountryCodes.Germany);
				shipmentDO.ConsignorEoriBranchSuffix = shipmentBO.ConsignorDocumentaryAddress.RealAddress.GetEoriBranchSuffix(context, Constants.CountryCodes.Germany);

				shipments.Add(shipmentDO);
			}

			advancedLogisticsPortOrder.Shipments = new ReadOnlyCollection<Shipment>(shipments);
		}

		PackingLine PopolatePackingLine(PackLine packlineBizObj, PackingLineBuilder packingLineBuilder, ZString direction)
		{
			var packingLine = packingLineBuilder.Build(packlineBizObj, consol: consol);

			if (packlineBizObj.Shipment is ForwardingShipment forwardingShipmentBO)
			{
				DefaultPackingLineEntryType(forwardingShipmentBO, packingLine, direction);

				if (IsImport(direction) && packingLine.ImportReferenceNumber.IsEmpty)
				{
					packingLine.ImportReferenceNumber = forwardingShipmentBO.CustomsEntryNumber;
				}

				if (IsExport(direction) && packingLine.ExportReferenceNumber.IsEmpty)
				{
					packingLine.ExportReferenceNumber = forwardingShipmentBO.CustomsEntryNumber;
				}
			}

			var localDEDBHCode = packlineBizObj.Commodity
									?.RefCommodityCodeMaps
									?.FirstOrDefault(map => map.LC_RN_NKCountry == Constants.CountryCodes.Germany && map.LC_LocalCodeProvider == DELocalCommodityCodeProviderList.Codes.DE_DBH)
									?.LC_LocalCode
									?? ZString.Empty;

			if (!localDEDBHCode.IsEmpty)
			{
				packingLine.Commodity = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = localDEDBHCode
				};
			}

			FallbackDangerousGoodsDetailsToPackingLine(packingLine);
			DefaultDangerousGoods(packingLine);

			return packingLine;
		}

		void DefaultPackingLineEntryType(ForwardingShipment shipment, PackingLine packingLine, string direction)
		{
			var entryTypeCode = string.Empty;
			var entryTypeStatus = EntryTypeStatus.ALL;

			if (IsImport(direction))
			{
				entryTypeCode = EntryTypes.Codes.EntryTypes_NA;
				entryTypeStatus = EntryTypeStatus.ReadOnly_DefaultToNA;
			}
			else if (shipment.CustomsEntryNumberType == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryTypeCode = EntryTypes.Codes.EntryTypes_AES;
				entryTypeStatus = EntryTypeStatus.ReadOnly_DefaultToAES;
			}
			else if (shipment.CustomsEntryNumberType == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryTypeCode = EntryTypes.Codes.EntryTypes_AE1;
				entryTypeStatus = EntryTypeStatus.ReadOnly_DefaultToAE1;
			}
			else if (IsShipmentWeightAbove1000kg(shipment) || IsShipmentGoodsValueAbove1000EUR(shipment))
			{
				entryTypeCode = EntryTypes.Codes.EntryTypes_AES;
				entryTypeStatus = EntryTypeStatus.DefaultToAES;
			}

			packingLine.EntryTypeStatus = entryTypeStatus.ToString();
			packingLine.EntryType = new CodeDescription(Lookups.GetEntryTypesList(entryTypeStatus))
			{
				Code = entryTypeCode
			};

			var isVisibleInitialState = packingLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AES || packingLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1;

			((CodeDescription)packingLine.EntryType).CodeInfo.ValueChanged += (s, e) =>
			{
				var isVisibleCurrentState = packingLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AES || packingLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1;
				packingLine.DocumentNoLabel = GetDocumentNumberLabel(packingLine.EntryType.Code);
				packingLine.IsVisible_Complete = isVisibleCurrentState;
				packingLine.IsVisible_Shortage = isVisibleCurrentState;
				packingLine.IsVisible_CargoItem = isVisibleCurrentState;
				packingLine.IsVisible_PackageNumber = isVisibleCurrentState;
			};

			packingLine.DocumentNoLabel = GetDocumentNumberLabel(packingLine.EntryType.Code);
			packingLine.IsVisible_CargoItem = isVisibleInitialState;
			packingLine.IsVisible_PackageNumber = isVisibleInitialState;
			packingLine.IsVisible_Complete = isVisibleInitialState;
			packingLine.IsVisible_Shortage = isVisibleInitialState;
		}

		bool IsShipmentWeightAbove1000kg(ForwardingShipment shipment) => Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Kilograms) > 1000;

		bool IsShipmentGoodsValueAbove1000EUR(ForwardingShipment shipment) => shipment.JS_RX_NKGoodsValueCurr == Constants.CurrencyCodes.EuropeanUnion && shipment.JS_GoodsValue > 1000;

		bool IsExport(ZString direction) => direction.EqualsIgnoringCase(Constants.FreightShipmentDirection.Description.Export.ToString());

		bool IsImport(ZString direction) => direction.EqualsIgnoringCase(Constants.FreightShipmentDirection.Description.Import.ToString());

		ZString GetDocumentNumberLabel(ZString entryTypeCode)
		{
			switch (entryTypeCode)
			{
				case EntryTypes.Codes.EntryTypes_AES:
					return "MRN";
				case EntryTypes.Codes.EntryTypes_AE1:
					return "LRN";
				default:
					return (NoResString)"Document Number";
			}
		}

		void FallbackDangerousGoodsDetailsToPackingLine(PackingLine packingLine)
		{
			if (packingLine.DangerousGoods.Count == 1)
			{
				var dangerousGood = packingLine.DangerousGoods.FirstOrDefault();
				if (dangerousGood != null)
				{
					if (dangerousGood.PackageType?.Code.IsEmpty ?? true)
					{
						dangerousGood.PackageType = packingLine.PackageType;
					}

					if (dangerousGood.Quantity.IsEmpty)
					{
						dangerousGood.Quantity = packingLine.Quantity;
					}

					if ((dangerousGood.Weight?.Value.IsEmpty ?? true) || (dangerousGood.Weight?.Unit?.Code.IsEmpty ?? true))
					{
						dangerousGood.Weight = packingLine.Weight;
					}

					if ((dangerousGood.Volume?.Value.IsEmpty ?? true) || (dangerousGood.Volume?.Unit?.Code.IsEmpty ?? true))
					{
						dangerousGood.Volume = packingLine.Volume;
					}
				}
			}
		}

		void DefaultDangerousGoods(PackingLine packingLine)
		{
			foreach (var dangerousGood in packingLine.DangerousGoods)
			{
				dangerousGood.NetExplosiveWeight = new Measurement
				{
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Constants.Weight.Kilograms
					}
				};
			}
		}

		void AddContainerValidation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			foreach (var container in advancedLogisticsPortOrder.Containers)
			{
				AddContainerValidation(container);
			}
		}

		void AddContainerValidation(Container container)
		{
			var isoTypeErrorMessage = Res.GetString("05a5414b-7b34-46b5-af50-06d719fd4b55", "Make sure an ISO type has been set on the container type");
			container.Type.ISOCodeInfo.AddMessageErrorIfEmpty(isoTypeErrorMessage);
		}

		void AddPackingLineValidation(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var packingLines = advancedLogisticsPortOrder.Shipments.SelectMany(x => x.PackingLines);
			var direction = advancedLogisticsPortOrder.Direction;
			var shipments = advancedLogisticsPortOrder.Shipments;
			var shipmentBOs = consol.Shipments.Cast<ForwardingShipment>().Where(x => x.CanHaveOwnPackLines);

			if (packingLines != null)
			{
				foreach (var packLine in packingLines)
				{
					((CodeDescription)packLine.Commodity).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("E38FCCF0-6027-4CBE-87B6-C30958B7EFC4", "Commodity Code is mandatory"));

					((CodeDescription)packLine.EntryType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("AFAF680A-DA12-49B5-AE4E-CE54923CB527", "Entry Type required."));

					if (IsExport(direction))
					{
						packLine.ExportReferenceNumberInfo.AddMessageError(() => packLine.ExportReferenceNumber.IsEmpty && packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AES,
							Res.GetString("F8976D89-9712-4C35-94A0-538DCA6A33DE", "MRN Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)"));

						packLine.ExportReferenceNumberInfo.AddMessageError(() => packLine.ExportReferenceNumber.IsEmpty && packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1,
							Res.GetString("2BAA4C4E-9C66-4D72-AD4D-14E5658B5290", "LRN Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)"));

						packLine.ExportReferenceNumberInfo.AddMessageError(() => packLine.ExportReferenceNumber.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_5555D ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_4444N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_2222G
							), Res.GetString("8C5813CD-F38D-447D-91E0-BC26554371EE", "Document Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)"));
						packLine.AddValidationDependencies(packLine.ExportReferenceNumberInfo, ((CodeDescription)packLine.EntryType).CodeInfo);

						var shipment = shipments.First(x => x.ShipmentID == packLine.ShipmentID);

						advancedLogisticsPortOrder.EoriNumber.ValueInfo.AddMessageError(() => packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1 && !IsValidShipmentWithEORAndEBS(advancedLogisticsPortOrder, shipment),
							Res.GetString("B4EC8BC2-4355-4DAF-BFFD-727DCA953347", "In case of AE1, EORI Number of Sending Agent or Consignor needs to be provided. (See Sending Agent > Config > Registration Number \"EOR\")"));

						advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo.AddMessageError(() => packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1 && !IsValidShipmentWithEORAndEBS(advancedLogisticsPortOrder, shipment),
							Res.GetString("00956906-8E78-4350-BB94-85196C45C66C", "In case of AE1, EORI Branch Suffix of Sending Agent or Consignor needs to be provided. (See Sending Agent > Config > Registration Number \"EBS\")"));

						((RegistrationNumber)shipment.ConsignorEoriNumber).ValueInfo.AddMessageError(() => packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1 && !IsValidShipmentWithEORAndEBS(advancedLogisticsPortOrder, shipment),
							Res.GetString("48BF3489-EE3E-4389-9812-54D6CC898BF9", "In case of AE1, EORI Number of Sending Agent or Consignor needs to be provided. (See Shipment > Consignor > Config > Registration Number \"EOR\")"));

						((RegistrationNumber)shipment.ConsignorEoriBranchSuffix).ValueInfo.AddMessageError(() => packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1 && !IsValidShipmentWithEORAndEBS(advancedLogisticsPortOrder, shipment),
							Res.GetString("1F2BAE5E-2B6B-4492-A442-56CBCBA8C0FF", "In case of AE1, EORI Branch Suffix of Sending Agent or Consignor needs to be provided. (See Shipment > Consignor > Config > Registration Number \"EBS\")"));

						((Unloco)shipment.Origin).CodeInfo.AddMessageError(() => shipment.Origin.Code.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_5555D ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_2222G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000N
							), Res.GetString("40311CE7-44F0-4A69-8D43-587A6781AB6B", "Origin required."));

						((Unloco)shipment.Destination).CodeInfo.AddMessageError(() => shipment.Destination.Code.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_5555D ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_4444N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_2222G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000N
							), Res.GetString("E76D903C-35F1-4E08-89C2-057CF4D4D07A", "Destination required."));

						packLine.GoodsDescriptionInfo.AddMessageError(() => packLine.GoodsDescription.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_M ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_5555D ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_4444N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_3333N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_9999N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_2222G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000N
							), Res.GetString("4ACB1673-3EB1-4718-A2F4-D73AC83AF2D6", "Goods Description required."));

						((HarmonizedCode)packLine.HarmonizedCode).CodeInfo.AddMessageError(() => packLine.HarmonizedCode.Code.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_4444N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000N
							), Res.GetString("DA5BB295-1B8D-4A4F-B719-976A032EEE00", "Harmonized Code required."));

						((Address)shipment.Consignor).CompanyNameInfo.AddMessageError(() => shipment.Consignor.CompanyName.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_4444N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_2222G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1111N ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_1000N
							), Res.GetString("E23F3A75-C477-4DF5-A957-77603F02771B", "Consignor required."));

						((Address)shipment.Consignee).CompanyNameInfo.AddMessageError(() => shipment.Consignee.CompanyName.IsEmpty &&
							(
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_5555D ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777G ||
								packLine.EntryType.Code == EntryTypes.Codes.EntryTypes_7777N
							), Res.GetString("A9CD8250-2549-4398-95D5-970E5C2958E8", "Consignee required."));

						var shipmentBO = shipmentBOs.First(x => x.JS_UniqueConsignRef == packLine.ShipmentID);

						((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() => packLine.EntryTypeStatus == nameof(EntryTypeStatus.ReadOnly_DefaultToAES) && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AES && shipmentBO.CustomsEntryNumberType == CusEntryNumberTypes.Standard.MovementReferenceNumber,
							Res.GetString("6C710A9D-6B27-46D3-824E-A9B59463267B", "Entry Type should be AES when an MRN Number has been provided in Shipment > Entry Details"));

						((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() => packLine.EntryTypeStatus == nameof(EntryTypeStatus.ReadOnly_DefaultToAE1) && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AE1 && shipmentBO.CustomsEntryNumberType == CusEntryNumberTypes.Standard.LocalReferenceNumber,
							Res.GetString("B73D064C-17E6-41EC-B31A-E4FA07F21A57", "Entry Type should be AE1 when an LRN Number has been provided in Shipment > Entry Details"));

						((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() => packLine.EntryTypeStatus == nameof(EntryTypeStatus.DefaultToAES) && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AE1 && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AES && IsShipmentGoodsValueAbove1000EUR(shipmentBO),
							Res.GetString("337BFC64-CA16-4423-89EF-BBBCE679DEB8", "Invalid entry Type, as the goods value of this shipment is above € 1000,00"));

						((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() => packLine.EntryTypeStatus == nameof(EntryTypeStatus.DefaultToAES) && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AE1 && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AES && IsShipmentWeightAbove1000kg(shipmentBO),
							Res.GetString("FE9940B3-DD06-4F56-854F-E5AB34AEC07E", "Invalid entry Type, as the total weight of this shipment is above 1000 kg"));

						((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() =>
							!packLine.EntryType.Code.IsEmpty && !packLine.EntryType.Description.IsEmpty
							&& packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AES && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AE1
							&& FindMatchedPackingLine(packingLines, packLine, direction),
							Res.GetString("3F12A170-9E9F-4769-A9D1-3E5E63495061", "It is not allowed to set different Entry types for the same document number!"));

						AddPackingLineValidationDependenciess(advancedLogisticsPortOrder, packLine, packingLines, shipment);
					}

					((CodeDescription)packLine.EntryType).CodeInfo.AddMessageError(() => !packLine.EntryType.Code.IsEmpty && packLine.EntryType.Description.IsEmpty, Res.GetString("8B2CA3B3-E10B-4B82-82BC-703D3869761F", "Invalid Entry Type."));

					var shortageValidationRule = new Func<PackingLine, bool>(packLine =>
							!packLine.Complete && !packLine.Shortage
							&& GetPackingLinesWithSameExportOrImportReferenceNumber(advancedLogisticsPortOrder, packLine, direction)
								.Where(p => p.DocumentNoLabel == packLine.DocumentNoLabel && p.CargoItem == packLine.CargoItem)
								.All(p => !p.Complete && !p.Shortage));

					packLine.ShortageInfo.AddMessageError(
						() => packLine.DocumentNoLabel == "MRN" && shortageValidationRule(packLine),
						Res.GetString("2479d4f5-7ea3-4f39-88a1-3ad5d4cf3395", "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete."));

					packLine.ShortageInfo.AddMessageError(
						() => packLine.DocumentNoLabel == "LRN" && shortageValidationRule(packLine),
						Res.GetString("309d8e1b-68b7-452c-a98f-6c52b05b5b73", "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete."));

					packLine.ShortageInfo.AddMessageError(
						() => packLine.DocumentNoLabel == new ZString((NoResString)"Document Number") && shortageValidationRule(packLine),
						Res.GetString("18b34dde-483a-4cae-ba3b-9825e570fcea", "At least one packline needs to be marked as complete or have a Shortage in case Document Number is not marked as complete."));

					packLine.AddValidationDependencies(packLine.ShortageInfo, GetValidationDependendListForShortage(GetPackingLinesWithSameExportOrImportReferenceNumber(advancedLogisticsPortOrder, packLine, direction)));
					packLine.AddValidationDependencies(packLine.ShortageInfo, packLine.CompleteInfo, ((CodeDescription)packLine.EntryType).CodeInfo, packLine.CargoItemInfo);

					packLine.CargoItemInfo.AddMessageError(
						() => packLine.CargoItem == "0"
						|| (packLine.CargoItem.IsEmpty
							&& GetPackingLinesWithSameExportOrImportReferenceNumber(advancedLogisticsPortOrder, packLine, direction).Any(p => p.DocumentNoLabel == packLine.DocumentNoLabel)),
						Res.GetString("6569e245-86a5-42dd-b2d9-2393e71c2840", "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No."));

					packLine.CargoItemInfo.AddMessageError(
						() => !packLine.CargoItem.IsNumbersOnlyOrEmpty
						, Res.GetString("858e4ce7-c336-4092-8af8-4dd5398515bf", "Cargo Item no. should be numeric or blank."));

					packLine.AddValidationDependencies(packLine.CargoItemInfo
						, GetPackingLinesWithSameExportOrImportReferenceNumber(advancedLogisticsPortOrder, packLine, direction).Select(p => ((CodeDescription)p.EntryType)?.CodeInfo).WhereNotNull().ToArray());
					packLine.AddValidationDependencies(packLine.CargoItemInfo, ((CodeDescription)packLine.EntryType).CodeInfo);

					packLine.ShortageInfo.ValueChanged += (s, e) =>
					{
						if (packLine.Shortage)
						{
							packLine.Complete = false;

							if (packLine.CargoItem.IsEmpty)
							{
								packLine.CargoItem = packLine.ItemNumber.ToString();
							}

							if (packLine.PackageNumber.IsEmpty)
							{
								packLine.PackageNumber = packLine.PackageNumberOriginalValue.IsEmpty ? "1" : packLine.PackageNumberOriginalValue;
							}
						}
					};

					AddDangerousGoodsValidation(packLine);
				}
			}
		}

		IEnumerable<PackingLine> GetPackingLinesWithSameExportOrImportReferenceNumber(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, PackingLine packLine, ZString direction)
		{
			return advancedLogisticsPortOrder
				.Shipments
				.SelectMany(x => x.PackingLines)
				.Where(p => p.PK != packLine.PK && (IsExport(direction) ? p.ExportReferenceNumber == packLine.ExportReferenceNumber : p.ImportReferenceNumber == packLine.ImportReferenceNumber));
		}

		ZPropertyInfo[] GetValidationDependendListForShortage(IEnumerable<PackingLine> packingLinesWithSameReference)
		{
			var propertyList = packingLinesWithSameReference.Select(p => p.ShortageInfo).ToList();
			propertyList.AddRange(packingLinesWithSameReference.Select(p => p.CompleteInfo).ToList());
			propertyList.AddRange(packingLinesWithSameReference.Select(p => ((CodeDescription)p.EntryType)?.CodeInfo).WhereNotNull().ToList());
			propertyList.AddRange(packingLinesWithSameReference.Select(p => p.CargoItemInfo).ToList());

			return propertyList.ToArray();
		}

		bool IsValidShipmentWithEORAndEBS(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, Shipment subShipmnet)
		{
			return !advancedLogisticsPortOrder.EoriBranchSuffix.Value.IsEmpty || advancedLogisticsPortOrder.EoriNumber.Value.IsEmpty && !subShipmnet.ConsignorEoriBranchSuffix.Value.IsEmpty;
		}

		void AddPackingLineValidationDependenciess(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, PackingLine packLine, IEnumerable<PackingLine> packingLines, Shipment shipment)
		{
			((Unloco)shipment.Origin).AddValidationDependencies(((Unloco)shipment.Origin).CodeInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
			((Unloco)shipment.Destination).AddValidationDependencies(((Unloco)shipment.Destination).CodeInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
			packLine.AddValidationDependencies(packLine.GoodsDescriptionInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
			((HarmonizedCode)packLine.HarmonizedCode).AddValidationDependencies(((HarmonizedCode)packLine.HarmonizedCode).CodeInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
			((Address)shipment.Consignor).AddValidationDependencies(((Address)shipment.Consignor).CompanyNameInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
			((Address)shipment.Consignee).AddValidationDependencies(((Address)shipment.Consignee).CompanyNameInfo, ((CodeDescription)packLine.EntryType).CodeInfo);

			packingLines.ForEach(dependencyValidationPackingLine =>
			{
				if (dependencyValidationPackingLine.PK != packLine.PK)
				{
					((CodeDescription)dependencyValidationPackingLine.EntryType).AddValidationDependencies(((CodeDescription)dependencyValidationPackingLine.EntryType).CodeInfo, ((CodeDescription)packLine.EntryType).CodeInfo);
				}
			});

			advancedLogisticsPortOrder.EoriNumber.AddValidationDependencies(advancedLogisticsPortOrder.EoriNumber.ValueInfo, (((CodeDescription)packLine.EntryType).CodeInfo));
			advancedLogisticsPortOrder.EoriBranchSuffix.AddValidationDependencies(advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo, (((CodeDescription)packLine.EntryType).CodeInfo));
			((RegistrationNumber)shipment.ConsignorEoriNumber).AddValidationDependencies(((RegistrationNumber)shipment.ConsignorEoriNumber).ValueInfo, (((CodeDescription)packLine.EntryType).CodeInfo));
			((RegistrationNumber)shipment.ConsignorEoriBranchSuffix).AddValidationDependencies(((RegistrationNumber)shipment.ConsignorEoriBranchSuffix).ValueInfo, (((CodeDescription)packLine.EntryType).CodeInfo));
		}

		bool FindMatchedPackingLine(IEnumerable<PackingLine> packLines, PackingLine currentPackLine, string direction)
		{
			var referenceNumber = IsExport(direction) ? currentPackLine.ExportReferenceNumber : currentPackLine.ImportReferenceNumber;

			if (!referenceNumber.IsEmpty)
			{
				foreach (var packLine in packLines)
				{
					if (packLine.PK != currentPackLine.PK)
					{
						var refNumber = IsExport(direction) ? packLine.ExportReferenceNumber : packLine.ImportReferenceNumber;

						if (refNumber == referenceNumber && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AES && packLine.EntryType.Code != EntryTypes.Codes.EntryTypes_AE1 && currentPackLine.EntryType.Code != packLine.EntryType.Code)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		void AddDangerousGoodsValidation(PackingLine packLine)
		{
			foreach (var dangerousGood in packLine.DangerousGoods)
			{
				dangerousGood.NetExplosiveWeight.ValueInfo.AddMessageError(() => dangerousGood.NetExplosiveWeight.Value.IsEmpty && dangerousGood.IMOClass.StartsWith("1")
					, Res.GetString("a402e3cd-0cb4-4426-91a9-288276d21476", "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances."));
				dangerousGood.NetExplosiveWeight.ValueInfo.AddMessageError(() => dangerousGood.NetExplosiveWeight.Value < 0
					, Res.GetString("c2fc0535-c342-47c1-9dfc-12de7bfdbbef", "Net Explosive Weight can't be negative."));
				dangerousGood.NetExplosiveWeight.ValueInfo.AddMessageError(() => string.IsNullOrEmpty(dangerousGood.Weight?.Unit?.Code)
					|| dangerousGood.NetExplosiveWeight.Value > Constants.Weight.Convert(dangerousGood.Weight.Value, dangerousGood.Weight.Unit.Code, Constants.Weight.Kilograms)
					, Res.GetString("415c26d1-3928-450a-a759-3a8fa245fc94", "Net Explosive Weight can't be higher than DG gross weight."));
			}
		}

		public AdvancedLogisticsPortOrderLookups Lookups
		{
			get
			{
				return lookups ?? (lookups = new AdvancedLogisticsPortOrderLookups());
			}
		}

		AdvancedLogisticsPortOrderLookups lookups;

		#endregion Goods and Equipment Details
	}
}

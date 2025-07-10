using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA.CargoDuesHelper;
using static Enterprise.MasterFiles.Business.OrgCusCode;
using EventConstants = CargoWise.EventReference.Constants;
using TransportBizoT = Enterprise.Freight.Business.Transport;
using TransportDataObject = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Transport;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects
{
	public class CargoDuesBrokerageDocDataBuilder
	{
		public CargoDuesBrokerageDocDataBuilder(JobDeclaration declaration, IDocDataObjectParameters parameters)
		{
			declaration.Transports.Sort(MovementLegComparer.PortsAndDatesBased(declaration.Transports));
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.parameters = parameters;
			context = ObjectFactory.Get<IContext>("IContext", declaration.Factory);
		}

		readonly IDocDataObjectParameters parameters;
		readonly JobDeclaration declaration;
		readonly IContext context;

		IEnumerable<TransportBizoT> SeaTransports { get => declaration.Transports.OfType<TransportBizoT>().Where(transport => transport.TransportMode == TransportModes.Sea); }

		string GetBrokerageDirection(string documentTitle)
		{
			switch (documentTitle)
			{
				case DeclarationDocumentConstants.DocumentNames.LoadCoastwise:
					return DeclarationDocumentConstants.BrokerageDirections.LoadCoastwise;
				case DeclarationDocumentConstants.DocumentNames.CargoDuesImport:
					return DeclarationDocumentConstants.BrokerageDirections.CargoDuesImport;
				case DeclarationDocumentConstants.DocumentNames.CargoDuesExport:
					return DeclarationDocumentConstants.BrokerageDirections.CargoDuesExport;
				case DeclarationDocumentConstants.DocumentNames.DischargeCoastwise:
					return DeclarationDocumentConstants.BrokerageDirections.DischargeCoastwise;
			}

			throw new ArgumentException("Unexpected documentTitle");
		}

		public CargoDues Build()
		{
			var cargoDuesBrokerage = new CargoDues(nameof(DataContextType.CustomsDeclaration), declaration.JE_DeclarationReference, DataContext.CargoDuesBrokerage);
			var documentTitle = parameters?.DocumentTitle ?? DeclarationDocumentConstants.DocumentNames.CargoDuesImport;

			cargoDuesBrokerage.Direction = GetBrokerageDirection(documentTitle);
			cargoDuesBrokerage.ClientRef = declaration.JE_DeclarationReference;
			cargoDuesBrokerage.IsExportDocument = documentTitle.In(new[] { DeclarationDocumentConstants.DocumentNames.CargoDuesExport, DeclarationDocumentConstants.DocumentNames.LoadCoastwise });
			cargoDuesBrokerage.WayBillNumber = declaration.JE_MasterBill;

			PopulateCategories(cargoDuesBrokerage);
			PopulateSimpleMappings(cargoDuesBrokerage);
			PopulatePortsInformation(cargoDuesBrokerage);
			PopulateTransportLegs(cargoDuesBrokerage);
			PopulateContainerOperator(cargoDuesBrokerage);
			PopulateTNPAOrderNumber(cargoDuesBrokerage);
			PopulateContainers(cargoDuesBrokerage);
			PopulateEtaEtd(cargoDuesBrokerage);
			PopulateGoodsInfos(cargoDuesBrokerage);
			PopulateTNPAInformation(cargoDuesBrokerage);

			SetupCargoDuesSection(cargoDuesBrokerage);

			AddValidationSimpleMappings(cargoDuesBrokerage);
			AddValidationEtaEtd(cargoDuesBrokerage);
			AddValidationVesselAndOnCarrierInformation(cargoDuesBrokerage);
			AddValidationContainerOperator(cargoDuesBrokerage);
			AddValidationTNPANumbers(cargoDuesBrokerage);
			AddValidationsForContainer(cargoDuesBrokerage);
			AddValidationGoods(cargoDuesBrokerage);
			AddValidationWayBillNumber(cargoDuesBrokerage);
			AddValidationCargoDuesWarningPlaceHolder(cargoDuesBrokerage);

			cargoDuesBrokerage.ValidateAllIncludingChildren();

			return cargoDuesBrokerage;
		}

		void AddValidationSimpleMappings(CargoDues wrapper)
		{
			wrapper.CarrierCodeInfo.AddMessageErrorIfEmpty("Carrier Code is required.");
			wrapper.IMONumberInfo.AddMessageErrorIfEmpty("IMO Number is required.");
			wrapper.RadioCallSignInfo.AddMessageErrorIfEmpty("Radio Call Sign is required.");
			wrapper.PlaceOfReceipt.Country.NameInfo.AddMessageErrorIfEmpty("Country/Region of Origin is required.");
			wrapper.PlaceOfDelivery.Country.NameInfo.AddMessageErrorIfEmpty("Country/Region of Destination is required.");
		}

		void AddValidationEtaEtd(CargoDues wrapper)
		{
			wrapper.EtdInfo.AddMessageErrorIfEmpty("ETD/ETA is required.");
			wrapper.EtaInfo.AddMessageErrorIfEmpty("ETD/ETA is required.");
		}

		void AddValidationVesselAndOnCarrierInformation(CargoDues wrapper)
		{
			wrapper.TNPAArrivalNumberInfo.AddMessageErrorIfEmpty("TNPA Arrival Number is required.");
			wrapper.Transports?.Main?.Vessel?.NameInfo.AddMessageErrorIfEmpty("Vessel Name is required.");
			wrapper.Transports?.Main?.VoyageFlightNumberInfo.AddMessageErrorIfEmpty("Voyage number is required.");
			wrapper.TerminalInfo.AddMessageErrorIfEmpty("TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization > Config > Registration Numbers/Codes as TNP code for country/region ZA.");
			wrapper.ServicePort.NameInfo.AddMessageErrorIfEmpty("Service Port is required.");
			wrapper.PortOfDischarge.NameInfo.AddMessageErrorIfEmpty("Port of Discharge is required.");
			wrapper.PortOfLoading.NameInfo.AddMessageErrorIfEmpty("Port of Loading is required.");
			wrapper.ClientRefInfo.AddMessageErrorIfEmpty("Client Reference is required.");
		}

		void AddValidationContainerOperator(CargoDues wrapper)
		{
			wrapper.ContainerOperatorInfo.AddMessageErrorIfEmpty("Container Operator/Vessels Agent is required.");
		}

		void AddValidationTNPANumbers(CargoDues wrapper)
		{
			wrapper.TNPAAccountNumberInfo.AddMessageErrorIfEmpty("TNPA Account Number is required.");
		}

		void AddValidationsForContainer(CargoDues cargoDues)
		{
			if (!cargoDues.IsContainerised)
			{
				return;
			}

			foreach (var container in cargoDues.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty("Container Number is required.");
				container.Type?.CodeInfo.AddMessageErrorIfEmpty("Container Type is required.");
			}

			cargoDues.CargoDuesWarningPlaceHolderInfo.AddMessageError(() => cargoDues.Containers != null && cargoDues.Containers.Count == 0, "Container Number is required.");
			cargoDues.CargoDuesWarningPlaceHolderInfo.AddMessageError(() => cargoDues.Containers != null && cargoDues.Containers.Count == 0, "Marks and Numbers required.");
		}

		void AddValidationGoods(CargoDues cargoDues)
		{
			if (cargoDues.IsContainerised && cargoDues.ShipmentPackingInfos == null)
			{
				return;
			}

			foreach (var goodsInfo in cargoDues.ShipmentPackingInfos.SelectMany(s => s.GoodsInfoCollection))
			{
				goodsInfo.MarksAndNosInfo.AddMessageErrorIfEmpty("Marks and Numbers required.");
			}

			cargoDues.CargoDuesWarningPlaceHolderInfo.AddMessageError(() => cargoDues.ShipmentPackingInfos.Count == 0, "Marks and Numbers required.");
		}

		void AddValidationWayBillNumber(CargoDues wrapper)
		{
			wrapper.WayBillNumberInfo.AddMessageErrorIfEmpty("Bill of Lading/Mates Receipt is required.");
		}

		void AddValidationCargoDuesWarningPlaceHolder(CargoDues wrapper)
		{
			wrapper.CargoDuesSectionTitleInfo.AddWarning(() => true, "This section is for information only and is not included in Cargo Dues EDI message.");
		}

		void PopulateContainers(CargoDues cargoDuesBrokerage)
		{
			if (!cargoDuesBrokerage.IsContainerised)
			{
				return;
			}

			var containerBuilder = new ContainerBuilder();
			var containerDOs = new List<Container>();

			foreach (CusContainer cusContainerBO in declaration.CusContainers)
			{
				var jobContainer = cusContainerBO.JobContainer;
				if (jobContainer != null)
				{
					var containerDO = containerBuilder.Build(cusContainerBO.JobContainer, context);
					containerDO.IsEmpty = cusContainerBO.JobContainer.JC_IsEmptyContainer;

					containerDO.Number = cusContainerBO.CO_ContainerNumber;
					containerDO.Type = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
					{
						Code = cusContainerBO.Container?.RC_Code ?? ZString.Empty,
						ISOCode = cusContainerBO.Container?.RC_ISOType ?? ZString.Empty,
						Type = new CodeDescription(cusContainerBO?.Container?.Lookups?.ContainerTypes ?? new CodeDescriptionPairList())
						{
							Code = cusContainerBO.Container?.RC_ContainerType ?? ZString.Empty
						}
					};
					containerDO.GrossWeight = new Measurement
					{
						Value = Weight.Convert(cusContainerBO.CO_Weight, cusContainerBO.CO_WeightUQ, Weight.Kilograms),
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = Weight.Kilograms
						}
					};

					containerDOs.Add(containerDO);
				}
			}

			cargoDuesBrokerage.Containers = containerDOs
			.OrderBy(container => container.Number)
			.ToArray();

			cargoDuesBrokerage.Containers.ForEach(container =>
			{
				container.PackCountInfo.ValueChanged += (s, e) =>
				{
					cargoDuesBrokerage.TotalNumberOfPacks = cargoDuesBrokerage.Containers.Sum(c => c.PackCount);
				};
			});
		}

		void PopulateGoodsInfos(CargoDues cargoDuesBrokerage)
		{
			if (cargoDuesBrokerage.IsContainerised)
			{
				return;
			}

			cargoDuesBrokerage.ShipmentPackingInfos = new List<ShipmentPackingInfo>
			{
				new ShipmentPackingInfo(ZGuid.Empty)
				{
					GoodsInfoCollection = new List<GoodsInfo>
					{
						new GoodsInfo(ZGuid.Empty)
						{
							MarksAndNos = declaration.JE_MarksAndNumbersShort,
							NumberOfPacks = declaration.JE_TotalNoOfPacks,
							PackType = declaration.JE_TotalNoOfPacksPackType,
							GoodsDescription = declaration.JE_GoodsDescription,
							GrossMass =  new Measurement
							{
								Value = Weight.Convert(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit, Weight.Kilograms),
								Unit = new CodeDescription(context.WeightUnits)
								{
									Code = Weight.Kilograms
								}
							}
						}
					}
				}
			};

			cargoDuesBrokerage.TotalNumberOfPacks = declaration.JE_TotalNoOfPacks;
		}

		void PopulateCategories(CargoDues cargoDuesBrokerage)
		{
			var arrivalCountryCode = declaration.PortOfArrival?.Country.Code ?? ZString.Empty;
			var loadingCountryCode = declaration.PortOfLoading?.Country.Code ?? ZString.Empty;
			var isArrivalInZa = arrivalCountryCode == CountryCodes.SouthAfrica;
			var isLoadInZa = loadingCountryCode == CountryCodes.SouthAfrica;

			cargoDuesBrokerage.IsContainerised = declaration.ContainerMode == ContainerModes.Containerised;
			cargoDuesBrokerage.IsDeepSea = !isArrivalInZa || !isLoadInZa;
			cargoDuesBrokerage.IsCoastwise = isArrivalInZa && isLoadInZa;

			cargoDuesBrokerage.IsTranship = CheckIsTranship();
			cargoDuesBrokerage.IsBulk = declaration.ContainerMode == ContainerModes.Bulk;
			cargoDuesBrokerage.IsBreakBulk = declaration.ContainerMode == ContainerModes.BreakBulk;

			cargoDuesBrokerage.ContainerMode = new CodeDescription(declaration.Lookups.CargoIdTypeList)
			{
				Code = declaration.ContainerMode
			};
		}

		void PopulateTNPAInformation(CargoDues cargoDuesBrokerage)
		{
			var tnpaAccountNumber = PortMessagingRegistry.Instance.TNPAAccountNumber.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)
					.OfType<TNPAAccountNumber>()
					.FirstOrDefault(x => x.Port == cargoDuesBrokerage.ServicePort.Code);
			if (tnpaAccountNumber != null)
			{
				cargoDuesBrokerage.TNPAAccountNumber = cargoDuesBrokerage.IsCoastwise ? tnpaAccountNumber.CoastwiseNumber : cargoDuesBrokerage.IsExportDocument ? tnpaAccountNumber.ExportNumber : tnpaAccountNumber.ImportNumber;
			}
			else
			{
				cargoDuesBrokerage.TNPAAccountNumber = ZString.Empty;
			}
		}

		bool CheckIsTranship()
		{
			var preTransport = SeaTransports.FirstOrDefault();
			foreach (var transport in SeaTransports)
			{
				if (preTransport.PK != transport.PK && (preTransport.DiscPort?.RL_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.SouthAfrica && (transport.LoadPort?.RL_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.SouthAfrica)
				{
					return true;
				}
				preTransport = transport;
			}

			return false;
		}

		void PopulateSimpleMappings(CargoDues cargoDuesBrokerage)
		{
			cargoDuesBrokerage.Agent = AddressBuilder.Create(context, declaration.AgentOverride?.MainAddress);
			cargoDuesBrokerage.CustomsContainerTerminalOperator = AddressBuilder.Create(context, declaration.ContainerTerminalOperatorDocAddress);
			cargoDuesBrokerage.ShippingLine = AddressBuilder.Create(context, declaration.ShippingLine?.MainAddress);
			cargoDuesBrokerage.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			cargoDuesBrokerage.IMONumber = declaration.Vessel?.RV_LloydsNumber ?? ZString.Empty;

			cargoDuesBrokerage.CarrierCode = declaration.ShippingLine?.CustomsCodes?.GetOrgCusCode(OrgCusCode.SouthAfricaCodeTypes.TNP, declaration.Country)?.OK_CustomsRegNo ?? ZString.Empty;
			if (string.IsNullOrEmpty(cargoDuesBrokerage.CarrierCode))
			{
				cargoDuesBrokerage.CarrierCode = declaration.ShippingLine?.CustomsCodes?.GetOrgCusCode(OrgCusCode.CodeTypes.CarrierCode, declaration.Country)?.OK_CustomsRegNo ?? ZString.Empty;
			}

			cargoDuesBrokerage.RadioCallSign = declaration.JE_RadioCallSign;
		}

		void PopulatePortsInformation(CargoDues cargoDuesBrokerage)
		{
			cargoDuesBrokerage.PortOfLoading = Unloco.Create(context, SeaTransports.FirstOrDefault()?.LoadPort);
			cargoDuesBrokerage.PortOfDischarge = Unloco.Create(context, SeaTransports.LastOrDefault()?.DiscPort);
			cargoDuesBrokerage.PlaceOfReceipt = Unloco.Create(context, declaration.Origin);
			cargoDuesBrokerage.PlaceOfDelivery = Unloco.Create(context, declaration.FinalDestination);
		}

		void PopulateTransportLegs(CargoDues cargoDuesBrokerage)
		{
			var zaDiscSeaTransports = SeaTransports.Where(transport => transport?.DiscPort?.RL_RN_NKCountryCode.EqualsIgnoringCase(CountryCodes.SouthAfrica) ?? false);
			var zaLoadSeaTransports = SeaTransports.Where(transport => transport?.LoadPort?.RL_RN_NKCountryCode.EqualsIgnoringCase(CountryCodes.SouthAfrica) ?? false);
			var leg = cargoDuesBrokerage.IsExportDocument ? zaLoadSeaTransports?.FirstOrDefault() : zaDiscSeaTransports?.LastOrDefault();

			cargoDuesBrokerage.TNPAArrivalNumber = cargoDuesBrokerage.IsExportDocument ?
							leg?.Sailing?.Origin?.JA_DepartReference ?? ZString.Empty :
							leg?.Sailing?.Destination?.JB_ArrivalReference ?? ZString.Empty;

			cargoDuesBrokerage.ServicePort = cargoDuesBrokerage.IsExportDocument ?
							Unloco.Create(context, leg?.LoadPort) :
							Unloco.Create(context, leg?.DiscPort);

			if (leg != null)
			{
				var mainLeg = TransportDataObject.Create(context, leg);
				if (cargoDuesBrokerage.IsExportDocument)
				{
					var onForwardingTransportLegBO = SeaTransports.FirstOrDefault(transport => transport.LoadPort == leg?.DiscPort);

					if (onForwardingTransportLegBO != null)
					{
						var onForwardingTransport = TransportDataObject.Create(context, onForwardingTransportLegBO);
						mainLeg.LegOrder = 1;
						onForwardingTransport.LegOrder = 2;

						cargoDuesBrokerage.Transports = Transports.Create(new[] { mainLeg, onForwardingTransport });
						cargoDuesBrokerage.Transports.Main = mainLeg;
						cargoDuesBrokerage.Transports.OnForwarding = onForwardingTransport;
					}
					else
					{
						cargoDuesBrokerage.Transports = Transports.Create(new[] { mainLeg });
						cargoDuesBrokerage.Transports.Main = mainLeg;
						mainLeg.LegOrder = 1;
					}
				}
				else
				{
					var preTransportLegBO = SeaTransports.FirstOrDefault(transport => transport.DiscPort == leg?.LoadPort);

					if (preTransportLegBO != null)
					{
						var preCarriageTransport = TransportDataObject.Create(context, preTransportLegBO);
						preCarriageTransport.LegOrder = 1;
						mainLeg.LegOrder = 2;

						cargoDuesBrokerage.Transports = Transports.Create(new[] { preCarriageTransport, mainLeg });
						cargoDuesBrokerage.Transports.PreCarriage = preCarriageTransport;
						cargoDuesBrokerage.Transports.Main = mainLeg;
					}
					else
					{
						cargoDuesBrokerage.Transports = Transports.Create(new[] { mainLeg });
						cargoDuesBrokerage.Transports.Main = mainLeg;
						mainLeg.LegOrder = 1;
					}
				}
			}

			PopulateTerminal(cargoDuesBrokerage, leg);
		}

		void PopulateTerminal(CargoDues cargoDuesBrokerage, TransportBizoT leg)
		{
			var relevantArrivalLocation = cargoDuesBrokerage.IsExportDocument ? leg?.DepartureLocation : leg?.ArrivalLocation;
			var terminal = relevantArrivalLocation?.Header?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_RN_NKCodeCountry == declaration.CountryCode && cusCode.OK_CodeType == SouthAfricaCodeTypes.TNP)?.OK_CustomsRegNo ?? ZString.Empty;
			if (terminal == ZString.Empty)
			{
				terminal = declaration.ContainerTerminalOperatorDocAddress?.Organisation?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_RN_NKCodeCountry == declaration.CountryCode && cusCode.OK_CodeType == SouthAfricaCodeTypes.TNP)?.OK_CustomsRegNo ?? ZString.Empty;
			}
			cargoDuesBrokerage.Terminal = terminal;
		}

		void PopulateEtaEtd(CargoDues cargoDuesBrokerage)
		{
			cargoDuesBrokerage.Etd = declaration.JE_DateAtOrigin;
			cargoDuesBrokerage.Eta = declaration.JE_DateAtFinalDestination;
		}

		void PopulateContainerOperator(CargoDues cargoDuesBrokerage)
		{
			var carrierTNP = declaration.ShippingLine?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_RN_NKCodeCountry == declaration.CountryCode && cusCode.OK_CodeType == SouthAfricaCodeTypes.TNP)?.OK_CustomsRegNo ?? ZString.Empty;
			var carrierCCC = declaration.ShippingLine?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_RN_NKCodeCountry == declaration.CountryCode && cusCode.OK_CodeType == CodeTypes.CarrierCode)?.OK_CustomsRegNo ?? ZString.Empty;

			cargoDuesBrokerage.ContainerOperator = !string.IsNullOrEmpty(carrierTNP) ? carrierTNP : carrierCCC;
		}

		void PopulateTNPAOrderNumber(CargoDues cargoDuesBrokerage)
		{
			var lastMessageWithRfn = parameters?.LogProvider?
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(l =>
				{
					var messageType = l.Parameters?.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType);
					var department = l.Parameters?.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Department);
					var referenceNum = l.Parameters?.GetValueSafe(EventConstants.EventReferenceParameters.Codes.ReferenceNumber);
					Func<string, string, bool> equalIgnoreCase = (s1, s2) => string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
					var isRfn = equalIgnoreCase(l.Event.SE_Code, Events.MessageAcceptedCode)
							 && equalIgnoreCase(messageType, parameters?.DocumentTitle ?? ZString.Empty)
							 && equalIgnoreCase(department, "TNPA")
							 && !string.IsNullOrEmpty(referenceNum);
					return isRfn;
				}).OrderBy(l => l.SL_EventTime).OrderBy(l => l.SL_PostedTimeUtc).LastOrDefault();
			cargoDuesBrokerage.TNPAOrderNumber = lastMessageWithRfn?.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.ReferenceNumber);
		}

		void SetupCargoDuesSection(CargoDues cargoDuesBrokerage)
		{
			cargoDuesBrokerage.CargoDuesSectionTitle = "Cargo Dues";

			cargoDuesBrokerage.DuesCollectionElement1 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement2 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement3 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement4 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement5 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement6 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement7 = new DuesCollectionRow();
			cargoDuesBrokerage.DuesCollectionElement8 = new DuesCollectionRow();

			var relatedEDIMessage = GetMessageAcceptedUniversalEventLogsInDescendingOrder(declaration.Logs, parameters?.DocumentTitle)?.FirstOrDefault()?.RelatedEDIMessage;
			if (relatedEDIMessage != null)
			{
				var universalEvent = relatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				var contextCollection = universalEvent?.ContextCollection;

				if (contextCollection != null)
				{
					SetDuesCollectionElement(1, cargoDuesBrokerage.DuesCollectionElement1, contextCollection);
					SetDuesCollectionElement(2, cargoDuesBrokerage.DuesCollectionElement2, contextCollection);
					SetDuesCollectionElement(3, cargoDuesBrokerage.DuesCollectionElement3, contextCollection);
					SetDuesCollectionElement(4, cargoDuesBrokerage.DuesCollectionElement4, contextCollection);
					SetDuesCollectionElement(5, cargoDuesBrokerage.DuesCollectionElement5, contextCollection);
					SetDuesCollectionElement(6, cargoDuesBrokerage.DuesCollectionElement6, contextCollection);
					SetDuesCollectionElement(7, cargoDuesBrokerage.DuesCollectionElement7, contextCollection);
					SetDuesCollectionElement(8, cargoDuesBrokerage.DuesCollectionElement8, contextCollection);

					var subTotal = contextCollection.FirstOrDefault(c => c.Type == ContextCollectionTypes.TotalCargoDuesAmount);
					cargoDuesBrokerage.SubTotal = RoundToTwoDecimals(ParseStringToDecimal(subTotal != null ? subTotal.Value.ToString() : string.Empty));
					cargoDuesBrokerage.VAT = RoundToTwoDecimals((decimal)cargoDuesBrokerage.SubTotal * GetTaxRateForCountryOfCurrentCompany(context.Factory));
					cargoDuesBrokerage.TotalR = RoundToTwoDecimals(cargoDuesBrokerage.SubTotal + cargoDuesBrokerage.VAT);
				}
			}
		}
	}
}

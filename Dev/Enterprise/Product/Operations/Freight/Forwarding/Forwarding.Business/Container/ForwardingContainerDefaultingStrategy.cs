using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class ForwardingContainerDefaultingStrategy : IContainerDefaultingStrategy
	{
		public ForwardingContainerDefaultingStrategy(ForwardingContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			this.container = container;
		}

		readonly ForwardingContainer container;

		public OrgAddress CalculateReleaseContainerYard()
		{
			if (container.Consol is not ForwardingConsol consol
				|| consol.ShippingLine is not OrgHeader carrier
				|| container.Container is not RefContainer refContainer
				|| refContainer.RC_StorageClass.IsEmpty)
			{
				return null;
			}

			return carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress(consol.JK_RL_NKLoadPort, refContainer);
		}

		public OrgAddress CalculateReturnContanierYard()
		{
			if (container.Consol is not ForwardingConsol consol
				|| consol.ShippingLine is not OrgHeader carrier
				|| container.Container is not RefContainer refContainer
				|| refContainer.RC_StorageClass.IsEmpty)
			{
				return null;
			}

			return carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress(consol.JK_RL_NKDischargePort, refContainer);
		}

		public (ZDateTime returnedBy, ZDateTime availableDate, ZString availableDateName) CalculateRequiredBy()
		{
			if (!IsContainerized)
			{
				return (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);
			}

			var branch = Declaration == null ? GlbBranch.CurrentBranch : (Declaration as IBranchProvider)?.Branch;
			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = CarrierForDetentionCalculation,
				OriginPort = PortOfLoadingForDetentionCalculation,
				DetentionPort = PortOfDischargeForDetentionCalculation,
				ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
				Direction = ContainerDetentionDirection.Import,
				ProcessType = ContainerPenaltyProcessType.Import,
				Container = container,
				Company = branch?.Company,
				Shipments = GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Import, null),
				GetClientsFunction = GetClients
			};

			var penaltyMatchers = ContainerPenaltyMatchers.PriorityOrderedPenaltyMatchers;
			IContainerPenaltyMatchResult matchedResult = null;
			foreach (IContainerPenaltyMatcher matcher in penaltyMatchers)
			{
				matchedResult = matcher.MatchDetention(filter) ?? matcher.MatchMDD(filter);
				if (matchedResult != null)
				{
					break;
				}
			}

			var (availableDate, availableDateName) = ContainerPenaltyDateHelper.GetAvailableDateForDetention(container, matchedResult?.FreeDayType
				?? ContainerDetentionFreeDayType.CTOAvailable);

			var returnByDate = ZDateTime.Empty;
			if (availableDate.IsValid && matchedResult != null)
			{
				ZByte freeDaysToExclude = 0;
				var penalty = container.ImportPenalties.FindContainerPenalty(matchedResult.PenaltyType, matchedResult.CreditorType, null);
				if (penalty?.FreeDayExclusion is IContainerPenaltyDayExclusion freeDayExclusion && !freeDayExclusion.IsEmpty())
				{
					freeDaysToExclude = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(availableDate, matchedResult.FreeDays, freeDayExclusion, penalty.Location);
				}

				returnByDate = availableDate.AddDays(Math.Max(matchedResult.FreeDays - 1 + freeDaysToExclude, 0));
			}

			return (returnByDate, availableDate, availableDateName);
		}

		public (ZDateTime storageStart, ZDateTime availableDate, ZString availableDateName) CalculateStorageStart(ZString direction)
		{
			if (!IsContainerized)
			{
				return (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);
			}

			var detention = GetMatchedImportStorage(direction, ContainerPenaltyCreditorType.Codes.CTO, false)
				?? GetMatchedImportStorage(direction, ContainerPenaltyCreditorType.Codes.Carrier, false);

			if (detention == null && container.GetArrivalCTOStorageStartDate(shouldIgnoreField: !container.JC_OverrideFCLAvailableStorage).IsEmpty)
			{
				detention = GetMatchedImportStorage(direction, ContainerPenaltyCreditorType.Codes.CTO, true);
			}

			var freeDayType = detention?.FreeDayType;
			if (string.IsNullOrEmpty(freeDayType))
			{
				freeDayType = direction == ContainerDetentionDirection.Import
					? ContainerDetentionFreeDayType.CTOAvailable
					: ContainerDetentionFreeDayType.FCLLoad;
			}

			var (availableDate, availableDateName) = ContainerPenaltyDateHelper.GetAvailableDateForDetention(container, freeDayType);
			if (detention != null && availableDate.IsValid)
			{
				var penalty = container.ImportPenalties.FindContainerPenalty(detention.PenaltyType, detention.CreditorType, null);
				ZByte freeDaysToExclude = 0;
				if (penalty?.FreeDayExclusion is IContainerPenaltyDayExclusion freeDayExclusion && !freeDayExclusion.IsEmpty())
				{
					freeDaysToExclude = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(availableDate, detention.FreeDays, freeDayExclusion, penalty.Location);
				}

				var freeDays = detention?.FreeDays ?? ZByte.Zero;
				return (availableDate.AddDays(freeDays + freeDaysToExclude), availableDate, availableDateName);
			}

			return (container.GetArrivalCTOStorageStartDate(shouldIgnoreField: !container.JC_OverrideFCLAvailableStorage), availableDate, availableDateName);
		}

		public ZDateTime CalculateStorageStartAvailableDate(ZString direction, ZString creditorType)
		{
			if (!IsContainerized)
			{
				return ZDateTime.Empty;
			}

			var (availableDate, _) = CalculateAvailableDateForStorage(direction, creditorType, ContainerPenaltyProcessType.Import, null);

			return availableDate.IsValid
				? availableDate
				: ZDateTime.Empty;
		}

		public ContainerPenaltyDate CalculateAvailableDateForStorage(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null)
		{
			var detention = CalculateMatchedStorage(direction, creditorType, processType, shipment);
			var freeDayType = detention?.FreeDayType;
			if (!freeDayType.HasValue || freeDayType.Value.IsEmpty)
			{
				freeDayType = direction == ContainerDetentionDirection.Import
					? ContainerDetentionFreeDayType.CTOAvailable
					: ContainerDetentionFreeDayType.FCLLoad;
			}

			return ContainerPenaltyDateHelper.GetAvailableDateForDetention(container, freeDayType);
		}

		public ContainerPenaltyDate CalculateAvailableDateForDetention(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			var detention = CalculateMatchedDetention(detentionPort, direction, processType, shipment);
			var freeDayType = detention?.FreeDayType;
			if (!freeDayType.HasValue || freeDayType.Value.IsEmpty)
			{
				freeDayType = direction == ContainerDetentionDirection.Import
					? ContainerDetentionFreeDayType.CTOAvailable
					: ContainerDetentionFreeDayType.WharfGateIn;
			}

			return ContainerPenaltyDateHelper.GetAvailableDateForDetention(container, freeDayType);
		}

		public ContainerPenaltyDate CalculateAvailableDateForMDD(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			var detention = CalculateMatchedMDD(detentionPort, direction, processType, shipment);
			var freeDayType = detention?.FreeDayType;
			if (!freeDayType.HasValue || freeDayType.Value.IsEmpty)
			{
				freeDayType = direction == ContainerDetentionDirection.Import
					? ContainerDetentionFreeDayType.CTOAvailable
					: ContainerDetentionFreeDayType.WharfGateIn;
			}

			return ContainerPenaltyDateHelper.GetAvailableDateForDetention(container, freeDayType);
		}

		public IContainerPenaltyMatchResult GetMatchedStoragePenalty(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null)
		{
			if (!IsContainerized)
			{
				return null;
			}

			return CalculateMatchedStorage(direction, creditorType, processType, shipment);
		}

		public IContainerPenaltyMatchResult GetMatchedMDDPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			if (!IsContainerized)
			{
				return null;
			}

			return CalculateMatchedMDD(detentionPort, direction, processType, shipment);
		}

		#region Implementation

		OrgHeader CarrierForDetentionCalculation
		{
			get
			{
				return container.Consol?.ShippingLine ?? (OrgHeader)(Declaration?["ShippingLine"]);
			}
		}

		OrgHeader CarrierForExportDetentionCalculation
		{
			get
			{
				OrgHeader result = null;
				if (container.Consol != null)
				{
					var transports = container.Consol.Transports;
					if (transports.Count > 1 && transports.DepartureTransport?.Carrier != null)
					{
						result = transports.DepartureTransport?.Carrier;
					}
					else if (container.Consol.ShippingLine != null)
					{
						result = container.Consol.ShippingLine;
					}
				}
				else if (Declaration != null && Declaration["ShippingLine"] != null)
				{
					result = (OrgHeader)Declaration["ShippingLine"];
				}
				return result;
			}
		}

		OrgHeader CarrierForImportDetentionCalculation
		{
			get
			{
				OrgHeader result = null;
				if (container.Consol != null)
				{
					var transports = container.Consol.Transports;
					if (transports?.Count > 1 && transports.ArrivalTransport?.Carrier != null)
					{
						result = transports.ArrivalTransport?.Carrier;
					}
					else if (container.Consol.ShippingLine != null)
					{
						result = container.Consol.ShippingLine;
					}
				}
				else if (Declaration != null && Declaration["ShippingLine"] != null)
				{
					result = (OrgHeader)Declaration["ShippingLine"];
				}
				return result;
			}
		}

		IReadOnlyCollection<IOrgHeader> GetClients(ICommonShipment shipment, ZString direction)
		{
			var clients = new List<IOrgHeader>();
			var commonShipment = shipment as CommonShipment;

			if (commonShipment != null)
			{
				if (commonShipment.ControllingCustomer != null)
				{
					clients.Add(commonShipment.ControllingCustomer);
				}

				if (commonShipment.ShipmentJobHeader?.LocalCharges != null)
				{
					clients.Add(commonShipment.ShipmentJobHeader?.LocalCharges);
				}

				if (direction == ContainerDetentionDirection.Import && commonShipment.Consignee != null)
				{
					clients.Add(commonShipment.Consignee);
				}

				if (direction == ContainerDetentionDirection.Export && commonShipment.Consignor != null)
				{
					clients.Add(commonShipment.Consignor);
				}
			}

			var declarationClient = (OrgHeader)(Declaration != null ? Declaration[direction == ContainerDetentionDirection.Export ? (NoResString)"Exporter" : (NoResString)"Importer"] : null);
			if (declarationClient != null)
			{
				clients.Add(declarationClient);
			}

			return clients;
		}

		ZString PortOfLoadingForDetentionCalculation
		{
			get
			{
				return container.Consol?.JK_RL_NKLoadPort
					?? (Declaration != null
						? (ZString)Declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading]
						: ZString.Empty);
			}
		}

		ZString PortOfDischargeForDetentionCalculation
		{
			get
			{
				return container.Consol?.JK_RL_NKDischargePort
					?? (Declaration != null
						? (ZString)Declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival]
						: ZString.Empty);
			}
		}

		OrgHeader ArrivalCTOForDetentionCalculation
		{
			get
			{
				if (container.Consol?.ArrivalCTOAddress?.Header is OrgHeader arrivalCTO)
				{
					return arrivalCTO;
				}
				else if (Declaration != null
					&& ((ZBool)Declaration["IsImport"]))
				{
					return ((JobDocAddress)Declaration["ContainerTerminalOperatorDocAddress"])?.Address?.Header;
				}

				return null;
			}
		}

		OrgHeader DepartureCTOForDetentionCalculation
		{
			get
			{
				if (container.Consol?.DepartureCTOAddress?.Header is OrgHeader departureCTO)
				{
					return departureCTO;
				}
				else if (Declaration != null && ((ZBool)Declaration["IsExport"]))
				{
					return ((JobDocAddress)Declaration["ContainerTerminalOperatorDocAddress"])?.Address?.Header;
				}

				return null;
			}
		}

		bool IsContainerized
		{
			get
			{
				return container.ContainerParent switch
				{
					ForwardingConsol consol => Core.Constants.ContainerModes.IsContainerised(consol.JK_ConsolMode),
					IBaseJobDeclaration declaration => Core.Constants.ContainerModes.IsContainerised(declaration.JE_ContainerMode),
					_ => true
				};
			}
		}

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = new CachedProperty<BusinessObject>(container.Factory, () =>
					{
						BusinessObject cusContainer = (BusinessObject)container.Factory.LoadTop1<IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, container.PK));

						if (cusContainer != null)
						{
							ZGuid decPK = (ZGuid)cusContainer[CusContainerSchema.CO_JE];
							return (BusinessObject)container.Factory.Load<IBaseJobDeclaration>(decPK);
						}
						else
						{
							return null;
						}
					});
				}
				return declaration.Value;
			}
		}
		CachedProperty<BusinessObject> declaration;

		#endregion

		#region private

		IContainerPenaltyMatchResult CalculateMatchedMDD(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment)
		{
			if (!IsContainerized || !SupportsPenalties(processType, shipment))
			{
				return null;
			}

			var filters = GetMDDFilters(detentionPort, direction, processType, shipment);
			IContainerPenaltyMatchResult result = null;

			foreach (var filter in filters)
			{
				result = PenaltyMatcherFactory.MatchMDD(filter, IsConsolProcessType(filter.ProcessType));
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		IContainerPenaltyMatchResult CalculateMatchedStorage(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment)
		{
			if (!IsContainerized || !SupportsPenalties(processType, shipment))
			{
				return null;
			}

			var filters = GetStorageFilters(direction, creditorType, processType, shipment);
			IContainerPenaltyMatchResult result = null;

			foreach (var filter in filters)
			{
				result = PenaltyMatcherFactory.MatchStorage(filter, IsConsolProcessType(filter.ProcessType));
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		IContainerPenaltyMatchResult GetMatchedImportStorage(ZString direction, ZString creditorType, bool useRegistryFallback)
		{
			var processType = ContainerPenaltyProcessType.Import;
			var filters = GetStorageFilters(direction, creditorType, processType, null);

			foreach (var filter in filters)
			{
				var result = PenaltyMatcherFactory.MatchStorage(filter, useRegistryFallback);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public IContainerPenaltyMatchResult GetMatchedDetentionPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment)
		{
			if (!IsContainerized)
			{
				return null;
			}

			return CalculateMatchedDetention(detentionPort, direction, processType, shipment);
		}

		IContainerPenaltyMatchResult CalculateMatchedDetention(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment)
		{
			if (!IsContainerized || !SupportsPenalties(processType, shipment))
			{
				return null;
			}

			var filters = GetDetentionFilters(detentionPort, direction, processType, shipment);
			IContainerPenaltyMatchResult result = null;

			foreach (var filter in filters)
			{
				result = PenaltyMatcherFactory.MatchDetention(filter, IsConsolProcessType(filter.ProcessType));
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		#region CalculateMatchedPenalties

		public IEnumerable<IContainerPenaltyMatchResult> CalculateMatchedPenalties(ZString processType, CommonShipment shipment = null)
		{
			if (!IsContainerized)
			{
				return Enumerable.Empty<IContainerPenaltyMatchResult>();
			}

			ZString detentionPort = ZString.Empty;
			ZString creditorType = ZString.Empty;
			var direction = ContainerPenalty.GetDirection(processType);

			if (processType == ContainerPenaltyProcessType.Import
				|| processType == ContainerPenaltyProcessType.Delivery)
			{
				detentionPort = container.ContainerParent?.DischargePort?.Code ?? ZString.Empty;
				creditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			}
			else if (processType == ContainerPenaltyProcessType.Export
				|| processType == ContainerPenaltyProcessType.Pickup)
			{
				detentionPort = container.ContainerParent?.LoadPort?.Code ?? ZString.Empty;
				creditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			}

			return PenaltyMatcherFactory.MatchPenalties(GetDetentionFilters(detentionPort, direction, processType, shipment),
				GetStorageFilters(direction, creditorType, processType, shipment),
				GetMDDFilters(detentionPort, direction, processType, shipment));
		}

		IEnumerable<ContainerPenaltyMatchFilter> GetDetentionFilters(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment)
		{
			var branch = Declaration == null ? GlbBranch.CurrentBranch : (Declaration as IBranchProvider)?.Branch;

			if (processType == ContainerPenaltyProcessType.Delivery)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Import,
					ProcessType = processType,
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else if (processType == ContainerPenaltyProcessType.Pickup)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Export,
					Container = container,
					ProcessType = processType,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}

			if (direction == ContainerDetentionDirection.Import)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Import,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Export,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
		}

		IEnumerable<ContainerPenaltyMatchFilter> GetStorageFilters(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment)
		{
			var branch = Declaration == null ? GlbBranch.CurrentBranch : (Declaration as IBranchProvider)?.Branch;

			if (processType == ContainerPenaltyProcessType.Delivery)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					CTO = ArrivalCTOForDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = PortOfDischargeForDetentionCalculation,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = direction,
					CreditorType = creditorType,
					ProcessType = processType,
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else if (processType == ContainerPenaltyProcessType.Pickup)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					CTO = DepartureCTOForDetentionCalculation,
					OriginPort = ZString.Empty,
					DetentionPort = PortOfLoadingForDetentionCalculation,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = direction,
					CreditorType = creditorType,
					ProcessType = processType,
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}

			if (direction == ContainerDetentionDirection.Import)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					CTO = ArrivalCTOForDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = PortOfDischargeForDetentionCalculation,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = direction,
					CreditorType = creditorType,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					CTO = DepartureCTOForDetentionCalculation,
					OriginPort = ZString.Empty,
					DetentionPort = PortOfLoadingForDetentionCalculation,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = direction,
					CreditorType = creditorType,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
		}

		IEnumerable<ContainerPenaltyMatchFilter> GetMDDFilters(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment)
		{
			var branch = Declaration == null ? GlbBranch.CurrentBranch : (Declaration as IBranchProvider)?.Branch;

			if (processType == ContainerPenaltyProcessType.Delivery)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Import,
					ProcessType = processType,
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else if (processType == ContainerPenaltyProcessType.Pickup)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Export,
					Container = container,
					ProcessType = processType,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}

			if (direction == ContainerDetentionDirection.Import)
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForImportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Import,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
			else
			{
				yield return new ContainerPenaltyMatchFilter
				{
					Carrier = CarrierForExportDetentionCalculation,
					OriginPort = PortOfLoadingForDetentionCalculation,
					DetentionPort = detentionPort,
					ContainerClass = container.Container?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Export,
					ProcessType = GetConsolProcessType(processType),
					Container = container,
					Company = branch?.Company,
					Shipments = GetRelatedShipmentsForPenaltyDefaulting(processType, shipment),
					GetClientsFunction = GetClients,
				};
			}
		}

		#endregion

		List<ICommonShipment> GetRelatedShipmentsForPenaltyDefaulting(ZString processType, CommonShipment shipment)
		{
			if (shipment != null && !SupportsPenalties(processType, shipment))
			{
				return new List<ICommonShipment>();
			}
			else if (shipment != null)
			{
				return new List<ICommonShipment>() { shipment };
			}

			return container.GetRelatedShipmentsForPenaltyDefaulting(processType).Cast<ICommonShipment>().ToList();
		}

		static bool SupportsPenalties(ZString processType, CommonShipment shipment)
		{
			if (shipment == null)
			{
				return true;
			}

			if (processType == ContainerPenaltyProcessType.Delivery)
			{
				return shipment.SupportsDeliveryPenalties;
			}
			else if (processType == ContainerPenaltyProcessType.Pickup)
			{
				return shipment.SupportsPickupPenalties;
			}

			return true;
		}

		static ZString GetConsolProcessType(ZString processType)
		{
			var result = ZString.Empty;

			if (processType == ContainerPenaltyProcessType.Import
				|| processType == ContainerPenaltyProcessType.Delivery)
			{
				result = Core.Constants.ContainerPenaltyProcessType.Import;
			}
			else if (processType == ContainerPenaltyProcessType.Export
				|| processType == ContainerPenaltyProcessType.Pickup)
			{
				result = Core.Constants.ContainerPenaltyProcessType.Export;
			}

			return result;
		}

		static bool IsConsolProcessType(ZString processType)
		{
			return processType == ContainerPenaltyProcessType.Import || processType == ContainerPenaltyProcessType.Export;
		}

		IContainerPenaltyMatcherFactory PenaltyMatcherFactory => penaltyMatcherFactory ??= new ContainerPenaltyMatcherFactory();
		IContainerPenaltyMatcherFactory penaltyMatcherFactory;

		#endregion
	}
}

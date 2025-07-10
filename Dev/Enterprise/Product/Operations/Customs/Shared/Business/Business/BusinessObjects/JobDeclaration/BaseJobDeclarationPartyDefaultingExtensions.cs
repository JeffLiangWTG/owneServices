using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class BaseJobDeclarationPartyDefaultingExtensions
	{
		public static void DefaultMasterBillFromAirLine(this BaseJobDeclaration jobDeclaration)
		{
			if (jobDeclaration != null && jobDeclaration.IsAir && jobDeclaration.JE_MasterBill.Length <= 3)
			{
				var twoCharCode = jobDeclaration.JE_VoyageFlightNo.SubstringSafe(0, 2);
				if (twoCharCode.Length == 2)
				{
					var airLine = RefAirline.LoadFromAirline2LetterCode(jobDeclaration.Factory, twoCharCode);
					if (airLine != null)
					{
						jobDeclaration.JE_MasterBill = airLine.RM_EagleAddedAirlinePrefixOrAccountingCode;
					}
				}
			}
		}

		public static void DefaultCarrierFromAirLine(this BaseJobDeclaration jobDeclaration)
		{
			if (jobDeclaration != null && jobDeclaration.JE_OH_ShippingLine.IsEmpty && jobDeclaration.IsAir)
			{
				var twoCharCode = jobDeclaration.JE_VoyageFlightNo.SubstringSafe(0, 2);
				if (twoCharCode.Length == 2)
				{
					var airLineQuery = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, twoCharCode);
					airLineQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);
					var airLines = jobDeclaration.Factory.Load<RefAirline>(airLineQuery);
					var threeCharCode = jobDeclaration.JE_MasterBill.SubstringSafe(0, 3);
					var airLinesMatchThreeCharCode = threeCharCode.Length == 3 ? airLines.FirstOrDefault(a => a.RM_EagleAddedAirlinePrefixOrAccountingCode == threeCharCode) : null;
					var org = airLinesMatchThreeCharCode?.GetCorrespondingCarrierOrganisation() ?? airLines.FirstOrDefault()?.GetCorrespondingCarrierOrganisation();
					if (org != null)
					{
						jobDeclaration.JE_OH_ShippingLine = org.PK;
					}
				}
			}
		}

		public static void DefaultWarehouseDocAddress(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.WarehouseDocAddress.IsEmpty)
			{
				OrgHeader org = null;
				if (isExport)
				{
					if (jobDeclaration.Supplier is OrgHeader supplier && RelatedPartyDefaultingTypeCodeEnableForExport(CustomsDataRegistry.Instance.RelatedPartyDefaultingBondedWarehouse.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						org = GetRelatedParty(supplier, RelatedPartyTypeList.Codes.Warehouse, RelatedPartyDirectionList.Codes.Pickup);
					}
				}
				else if (isImport && jobDeclaration.Importer is OrgHeader importer && RelatedPartyDefaultingTypeCodeEnableForImport(CustomsDataRegistry.Instance.RelatedPartyDefaultingBondedWarehouse.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
				{
					org = GetRelatedParty(importer, RelatedPartyTypeList.Codes.Warehouse, RelatedPartyDirectionList.Codes.Delivery);
				}

				if (org != null)
				{
					jobDeclaration.WarehouseDocAddress.OrganisationPK = org.PK;
				}
			}
		}

		public static void DefaultExternalBroker(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.JE_OH_ExternalBroker.IsEmpty)
			{
				OrgHeader org = null;
				if (isExport)
				{
					if (jobDeclaration.Supplier is OrgHeader supplier && RelatedPartyDefaultingTypeCodeEnableForExport(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						org = GetRelatedParty(supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, jobDeclaration.JE_TransportMode, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKOrigin);
					}
				}
				else if (isImport && jobDeclaration.Importer is OrgHeader importer && RelatedPartyDefaultingTypeCodeEnableForImport(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
				{
					org = GetRelatedParty(importer, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, jobDeclaration.JE_TransportMode, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKFinalDestination);
				}

				if (org != null && !org.IsProxyOrgOfAnyCompany())
				{
					jobDeclaration.JE_OH_ExternalBroker = org.PK;
				}
			}
		}

		public static void DefaultForwarder(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.JE_OH_Forwarder.IsEmpty)
			{
				OrgHeader org = null;
				if (isExport)
				{
					if (jobDeclaration.Supplier is OrgHeader supplier && RelatedPartyDefaultingTypeCodeEnableForExport(CustomsDataRegistry.Instance.RelatedPartyDefaultingForwarder.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						org = GetRelatedPartyForForwarder(jobDeclaration, supplier, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKOrigin);
					}
				}
				else if (isImport && jobDeclaration.Importer is OrgHeader importer && RelatedPartyDefaultingTypeCodeEnableForImport(CustomsDataRegistry.Instance.RelatedPartyDefaultingForwarder.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
				{
					org = GetRelatedPartyForForwarder(jobDeclaration, importer, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKFinalDestination);
				}

				if (org != null)
				{
					jobDeclaration.JE_OH_Forwarder = org.PK;
				}
			}
		}

		public static void DefaultCTO(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && !jobDeclaration.JE_OH_ShippingLine.IsEmpty && jobDeclaration.ContainerTerminalOperatorDocAddress.IsEmpty)
			{
				OrgCarrierAppointedAgentPorts port = null;

				if (isExport)
				{
					if (RelatedPartyDefaultingTypeCodeEnableForExport(
						CustomsDataRegistry.Instance.RelatedPartyDefaultingCTO.GetFallBackValueAtAllLevels(
							jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						port = GetCTOForExport(jobDeclaration);
					}
				}
				else if (isImport)
				{
					if (RelatedPartyDefaultingTypeCodeEnableForImport(
						CustomsDataRegistry.Instance.RelatedPartyDefaultingCTO.GetFallBackValueAtAllLevels(
							jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						port = GetCTOForImport(jobDeclaration);
					}
				}

				if (port != null)
				{
					jobDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = port.O5_OA_AgentOfficeAddress;
				}
			}
		}

		static OrgCarrierAppointedAgentPorts GetCTOForExport(BaseJobDeclaration jobDeclaration)
		{
			OrgCarrierAppointedAgentPorts port = null;
			var carrier = jobDeclaration.ShippingLine;
			if (carrier != null)
			{
				if (jobDeclaration.IsAir)
				{
					port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_AirCTO, jobDeclaration.JE_RL_NKOrigin);
				}
				else if (jobDeclaration.IsSea)
				{
					port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_Stevedore, jobDeclaration.JE_RL_NKOrigin, true);
				}
			}

			return port;
		}

		static OrgCarrierAppointedAgentPorts GetCTOForImport(BaseJobDeclaration jobDeclaration)
		{
			OrgCarrierAppointedAgentPorts port = null;
			var carrier = jobDeclaration.ShippingLine;
			if (carrier != null)
			{
				if (jobDeclaration.IsAir)
				{
					port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_AirCTO, jobDeclaration.JE_RL_NKFinalDestination);
				}
				else if (jobDeclaration.IsSea)
				{
					port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_Stevedore, jobDeclaration.JE_RL_NKFinalDestination, true);
				}
			}

			return port;
		}

		public static void DefaultDepot(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.DepotDocAddress is JobDocAddress depotDocAddress && !depotDocAddress.E2_AddressOverride && depotDocAddress.OrganisationPK.IsEmpty)
			{
				OrgHeader org = null;
				if (isExport)
				{
					if (jobDeclaration.Supplier is OrgHeader supplier && RelatedPartyDefaultingTypeCodeEnableForExport(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						org = GetRelatedParty(supplier, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, jobDeclaration.JE_TransportMode, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKOrigin);
					}
				}
				else if (isImport && jobDeclaration.Importer is OrgHeader importer && RelatedPartyDefaultingTypeCodeEnableForImport(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
				{
					org = GetRelatedParty(importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, jobDeclaration.JE_TransportMode, jobDeclaration.JE_ContainerMode, jobDeclaration.JE_RL_NKFinalDestination);
				}

				if (org != null)
				{
					depotDocAddress.OrganisationPK = org.PK;
				}
			}
		}

		public static void DefaultContainerYard(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.IsSea && !jobDeclaration.JE_OH_ShippingLine.IsEmpty && jobDeclaration.ContainerYardDocAddress.IsEmpty)
			{
				var carrier = jobDeclaration.ShippingLine;
				OrgCarrierAppointedAgentPorts port = null;
				if (carrier != null)
				{
					if (isImport)
					{
						if (RelatedPartyDefaultingTypeCodeEnableForImport(CustomsDataRegistry.Instance.RelatedPartyDefaultingContainerYard.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
						{
							port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_ContainerYardPark, jobDeclaration.JE_RL_NKFinalDestination);
						}
					}
					else if (isExport && RelatedPartyDefaultingTypeCodeEnableForExport(CustomsDataRegistry.Instance.RelatedPartyDefaultingContainerYard.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty)))
					{
						port = GetOrgCarrierAppointedAgentPorts(carrier.CarrierAppointedAgentPorts_ContainerYardPark, jobDeclaration.JE_RL_NKOrigin);
					}
				}

				if (port != null)
				{
					jobDeclaration.ContainerYardDocAddress.E2_OA_Address = port.O5_OA_AgentOfficeAddress;
				}
			}
		}

		public static void DefaultControllingCustomer(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.JE_OH_ControllingCustomer.IsEmpty && FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.Value)
			{
				var precedenceRule = FreightDataRegistry.Instance.DefaultControllingCustomerRule.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty);
				var orgTypesPrecedence = precedenceRule.SelectedItems;
				foreach (EntityPrecedenceRuleItem orgTypeRuleItem in orgTypesPrecedence)
				{
					var org = GetOrganizationByOrgType(jobDeclaration, RelatedPartyTypeList.Codes.ControllingCustomer, orgTypeRuleItem.Code, isExport, isImport);

					if (org != null)
					{
						jobDeclaration.JE_OH_ControllingCustomer = org.PK;
						break;
					}
				}
			}
		}

		public static void DefaultControllingAgent(this BaseJobDeclaration jobDeclaration, bool isExport, bool isImport)
		{
			if (jobDeclaration != null && jobDeclaration.JE_OH_ControllingAgent.IsEmpty && FreightDataRegistry.Instance.DefaultShipmentControllingAgent.Value)
			{
				var precedenceRule = FreightDataRegistry.Instance.DefaultControllingAgentRule.GetFallBackValueAtAllLevels(jobDeclaration.RegistryCompanyPK, jobDeclaration.RegistryBranchPK, Guid.Empty);
				var orgTypesPrecedence = precedenceRule.SelectedItems;
				foreach (EntityPrecedenceRuleItem orgTypeRuleItem in orgTypesPrecedence)
				{
					var org = GetOrganizationByOrgType(jobDeclaration, RelatedPartyTypeList.Codes.ControllingAgent, orgTypeRuleItem.Code, isExport, isImport);

					if (org != null)
					{
						jobDeclaration.JE_OH_ControllingAgent = org.PK;
						break;
					}
				}
			}
		}

		static bool RelatedPartyDefaultingTypeCodeEnableForExport(string code) => code == RelatedPartyDefaultingTypeList.Codes.Both || code == RelatedPartyDefaultingTypeList.Codes.Export;

		static bool RelatedPartyDefaultingTypeCodeEnableForImport(string code) => code == RelatedPartyDefaultingTypeList.Codes.Both || code == RelatedPartyDefaultingTypeList.Codes.Import;

		static OrgHeader GetRelatedParty(OrgHeader org, string controllingPartyType, string direction, string transportMode = "", string containerMode = "", string location = "")
		{
			OrgRelatedParty relatedParty = null;
			if (!org.IsNull && !controllingPartyType.IsNullOrEmpty() && !direction.IsNullOrEmpty())
			{
				relatedParty = org.AllRelatedParties.GetRelatedParty(controllingPartyType, direction, transportMode, containerMode, location);
			}

			return relatedParty?.RelatedParty;
		}

		static OrgHeader GetRelatedPartyForForwarder(BaseJobDeclaration jobDeclaration, OrgHeader org, string controllingPartyType, string direction, string containerMode, string location)
		{
			var countryCode = ((ZString)location).Left(2);
			var relatedPartiesFiltered = org?.AllRelatedParties.Cast<OrgRelatedParty>()
				.Where(x => x.PR_PartyType == controllingPartyType
							&& MatchFreightDirection(x)
							&& MatchTransportMode(x)
							&& MatchFreightContainerMode(x)
							&& MatchCompanyLevel(x)
							&& MatchLocation(x)
							&& x.RelatedParty != null)
				.Select(x => new
				{
					RelatedParty = x.RelatedParty,
					AppointedAgentPortsFiltered = x.RelatedParty.AppointedAgentPorts.Cast<OrgAppointedAgentPorts>().Where(MatchAgentDirection).ToArray()
				}).ToArray();
			var filtered = relatedPartiesFiltered?.FirstOrDefault(x => x.AppointedAgentPortsFiltered.Any(ports => ports.O5_PortOrCountry == location && MatchAgentStatusPublished(ports)))
								?? relatedPartiesFiltered.FirstOrDefault(x => x.AppointedAgentPortsFiltered.Any(ports => ports.O5_PortOrCountry == countryCode && MatchAgentStatusPublished(ports)))
								?? relatedPartiesFiltered.FirstOrDefault(x => x.AppointedAgentPortsFiltered.Any(ports => ports.O5_PortOrCountry == location && MatchAgentStatusAppointed(ports)))
								?? relatedPartiesFiltered.FirstOrDefault(x => x.AppointedAgentPortsFiltered.Any(ports => ports.O5_PortOrCountry == countryCode && MatchAgentStatusAppointed(ports)));
			return filtered?.RelatedParty;

			bool MatchFreightDirection(OrgRelatedParty relatedParty)
			{
				var freightDirection = relatedParty.PR_FreightDirection;
				return freightDirection == direction || freightDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery;
			}

			bool MatchTransportMode(OrgRelatedParty relatedParty)
			{
				var freightTransportMode = relatedParty.PR_FreightTransportMode;
				var transportMode = jobDeclaration.IsAir
					? Constants.TransportModes.Air
					: jobDeclaration.IsSea
						? Constants.TransportModes.Sea
						: jobDeclaration.IsRoad
							? Constants.TransportModes.Road
							: jobDeclaration.IsRail
								? Constants.TransportModes.Rail
								: string.Empty;
				return freightTransportMode == transportMode || freightTransportMode == Constants.TransportModes.All;
			}

			bool MatchFreightContainerMode(OrgRelatedParty relatedParty)
			{
				var freightContainerMode = relatedParty.PR_FreightContainerMode;
				return freightContainerMode.IsEmpty || freightContainerMode == containerMode;
			}

			bool MatchLocation(OrgRelatedParty relatedParty)
			{
				var partyLocation = relatedParty.PR_Location;
				return partyLocation.IsEmpty || partyLocation == location || partyLocation == countryCode;
			}

			bool MatchCompanyLevel(OrgRelatedParty relatedParty)
			{
				return !OrgRelatedPartyCompanySpecificCollection.ShouldBeCompanySpecific(controllingPartyType) || relatedParty.PR_GC == jobDeclaration.JE_GC;
			}

			bool MatchAgentStatusPublished(OrgAppointedAgentPorts ports) => (jobDeclaration.IsAir && ports.O5_AirAgentStatus == AgentStatusList.Codes.Published)
				|| (jobDeclaration.IsSea && ports.O5_SeaAgentStatus == AgentStatusList.Codes.Published)
				|| (jobDeclaration.IsRail && ports.O5_RailAgentStatus == AgentStatusList.Codes.Published)
				|| (jobDeclaration.IsRoad && ports.O5_RoadAgentStatus == AgentStatusList.Codes.Published);

			bool MatchAgentStatusAppointed(OrgAppointedAgentPorts ports) => (jobDeclaration.IsAir && ports.O5_AirAgentStatus == AgentStatusList.Codes.Appointed)
				|| (jobDeclaration.IsSea && ports.O5_SeaAgentStatus == AgentStatusList.Codes.Appointed)
				|| (jobDeclaration.IsRail && ports.O5_RailAgentStatus == AgentStatusList.Codes.Appointed)
				|| (jobDeclaration.IsRoad && ports.O5_RoadAgentStatus == AgentStatusList.Codes.Appointed);

			bool MatchAgentDirection(OrgAppointedAgentPorts ports)
			{
				var portsAgentDirection = ports.O5_AgentDirection;
				var agentDirection = direction == RelatedPartyDirectionList.Codes.Delivery
					? AgentDirectionList.Codes.Import
					: AgentDirectionList.Codes.Export;
				return portsAgentDirection == AgentDirectionList.Codes.Both || portsAgentDirection == agentDirection;
			}
		}

		static OrgCarrierAppointedAgentPorts GetOrgCarrierAppointedAgentPorts(OrgCarrierAppointedAgentPortsDependentCollection agentPortsCollection, ZString matchPortOrCountry, bool isMatchType = false)
			=> agentPortsCollection.Cast<OrgCarrierAppointedAgentPorts>().FirstOrDefault(agentPorts => (!isMatchType || (agentPorts.O5_TerminalType == StevedoreTerminalType.Codes.ContainerTerminal || agentPorts.O5_TerminalType == StevedoreTerminalType.Codes.BulkTerminal)) && agentPorts.O5_PortOrCountry == matchPortOrCountry);

		static OrgHeader GetOrganizationByOrgType(BaseJobDeclaration jobDeclaration, string controllingPartyType, string relatedPartyType, bool isExport, bool isImport)
		{
			OrgHeader organization = null;

			if (controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent)
			{
				if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.ControllingCustomer && jobDeclaration.ControllingCustomer is OrgHeader controllingCustomer)
				{
					organization = GetRelatedParty(controllingCustomer, controllingPartyType, RelatedPartyDirectionList.Codes.Sales);
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.BillToParty)
				{
					organization = GetControllingCustomerAgentFromBillToParty(jobDeclaration, controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.BookingParty)
				{
					organization = GetControllingCustomerAgentFromBookingParty(jobDeclaration, controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.ConsignorConsignee)
				{
					organization = GetControllingCustomerAgentFromConsignorOrConsignee(jobDeclaration, controllingPartyType, isExport, isImport);
				}
			}
			else if (controllingPartyType == RelatedPartyTypeList.Codes.ControllingCustomer)
			{
				if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.BookingParty)
				{
					organization = GetControllingCustomerAgentFromBookingParty(jobDeclaration, controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.BillToParty)
				{
					organization = GetControllingCustomerAgentFromBillToParty(jobDeclaration, controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.ConsignorConsignee)
				{
					organization = GetControllingCustomerAgentFromConsignorOrConsignee(jobDeclaration, controllingPartyType, isExport, isImport);
				}
			}

			return organization;
		}

		static bool IsPrepaid(BaseJobDeclaration jobDeclaration)
			=> jobDeclaration.RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight) == Constants.PaymentType.Prepaid;

		static OrgHeader GetControllingCustomerAgentFromBillToParty(BaseJobDeclaration jobDeclaration, string controllingPartyType)
		{
			var job = jobDeclaration.Job;
			var factory = jobDeclaration.Factory;
			OrgHeader controllingOrg = null;
			if (job != null)
			{
				var freightChargeCode = factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(jobDeclaration.RegistryCompanyPK));

				if (freightChargeCode != null)
				{
					ZGuid getDebtorPK(IJobNumber jobParent) => job.GetDebtorPK(freightChargeCode, jobParent.JobNumber);
					var debtorPk = getDebtorPK(jobDeclaration);

					var billToParty = factory.Load<OrgHeader>(debtorPk);

					if (billToParty != null && !billToParty.OH_IsBroker && !billToParty.OH_IsForwarder)
					{
						var direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
							? RelatedPartyDirectionList.Codes.Sales
							: (IsPrepaid(jobDeclaration) ? RelatedPartyDirectionList.Codes.Pickup : RelatedPartyDirectionList.Codes.Delivery);

						controllingOrg = GetRelatedParty(billToParty, controllingPartyType, direction);
					}
				}
			}

			return controllingOrg;
		}

		static OrgHeader GetControllingCustomerAgentFromBookingParty(BaseJobDeclaration jobDeclaration, string controllingPartyType)
		{
			var shipment = jobDeclaration.Shipment;
			var factory = jobDeclaration.Factory;
			OrgHeader controllingOrg = null;
			if (shipment != null && shipment.JS_IsBooking && !shipment.JS_IsForwardRegistered)
			{
				var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
				var quotedBooking = (IQuotedBooking)quotedBookingBuilder.Load(factory, shipment.PK);
				var bookingParty = factory.Load<OrgHeader>(quotedBooking.ClientPK);

				if (bookingParty != null)
				{
					var direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
						? RelatedPartyDirectionList.Codes.Sales
						: RelatedPartyDirectionList.Codes.Pickup;
					controllingOrg = GetRelatedParty(bookingParty, controllingPartyType, direction);
				}
			}

			return controllingOrg;
		}

		static OrgHeader GetControllingCustomerAgentFromConsignorOrConsignee(BaseJobDeclaration jobDeclaration, string controllingPartyType, bool isExport, bool isImport)
		{
			OrgHeader org = isImport ? jobDeclaration.Importer : (isExport ? jobDeclaration.Supplier : null);
			OrgHeader controllingOrg = null;
			if (org != null)
			{
				var direction = isImport ? RelatedPartyDirectionList.Codes.Pickup : (isExport ? RelatedPartyDirectionList.Codes.Delivery : string.Empty);
				if (!direction.IsNullOrEmpty())
				{
					var ictOrg = GetRelatedParty(org, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, direction);

					direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
						? RelatedPartyDirectionList.Codes.Sales
						: direction;

					if (ictOrg != null && !ictOrg.OH_IsBroker && !ictOrg.OH_IsForwarder)
					{
						controllingOrg = GetRelatedParty(ictOrg, controllingPartyType, direction);
					}

					controllingOrg = controllingOrg ?? GetRelatedParty(org, controllingPartyType, direction);
				}
			}

			return controllingOrg;
		}
	}
}

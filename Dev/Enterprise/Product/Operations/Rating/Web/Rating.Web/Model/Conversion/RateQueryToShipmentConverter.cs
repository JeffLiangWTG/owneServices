using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	internal static class RateQueryToShipmentConverter
	{
		public static CommonShipment CreateSingleConsolidatedShipment(RateQueryBusinessObject rateQueryBO, bool setAllPossibleInformations)
		{
			var shipment = rateQueryBO.Factory.New<ForwardingShipment>();
			/*
				We had to create a ForwardingShipment (rather than CommonShipment) because
				when we try to create a new consol for that, there would be development error
				indicating that CommonConsol should not be constructed in production code.
				CommonCosol.cs -> CheckIsSubclass() method.
			*/
			var consol = shipment.Consols.AddNew();

			var rateQueryParties = rateQueryBO.GetResolvedRateParties();

			// Important! Assign the Consignor / Consignee before Origin /
			// Destination, as these assignments will default Origin /
			// Destination to the Organisation's nominated closest port
			//
			// See eg: CommonShipment.SetOriginToConsignorLocation
			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.CNR, out var cnr))
			{
				shipment.ConsignorPK = cnr.PK;
			}

			// Likewise: CommonShipment.SetDestinationToConsigneeLocation
			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.CNE, out var cne))
			{
				shipment.ConsigneePK = cne.PK;
			}

			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.CCUS, out var ccus))
			{
				shipment.ControllingCustomerNameOrPK = ccus.PK.ToString();
			}

			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.LC, out var lc))
			{
				if (shipment.JobHeader == null)
				{
					new JobHeader.Loader(shipment).TryCreate();
				}

				shipment.JobHeader.LocalChargesPK = lc.PK;
			}

			shipment.JS_RL_NKOrigin = rateQueryBO.Origin?.Code ?? ZString.Empty;
			shipment.JS_RL_NKDestination = rateQueryBO.Destination?.Code ?? ZString.Empty;
			shipment.JS_INCO = rateQueryBO.RateQuery.Incoterm;
			shipment.JS_TransportMode = rateQueryBO.TransportMode;

			consol.JK_RL_NKLoadPort = rateQueryBO.Origin?.Code ?? ZString.Empty;
			consol.JK_RL_NKDischargePort = rateQueryBO.Destination?.Code ?? ZString.Empty;
			consol.JK_PrepaidCollect = rateQueryBO.RateQuery.CarrierPayTerm;

			shipment.JS_OH_ImportBroker = rateQueryParties.TryGetValue(OrganisationRole.Roles.IB, out var importBroker) ? importBroker.PK : ZGuid.Empty;
			shipment.JS_OH_ExportBroker = rateQueryParties.TryGetValue(OrganisationRole.Roles.EB, out var exportBroker) ? exportBroker.PK : ZGuid.Empty;
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = rateQueryParties.TryGetValue(OrganisationRole.Roles.PTC, out var pickupCartageCo) ? pickupCartageCo.MainAddress.PK : ZGuid.Empty;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = rateQueryParties.TryGetValue(OrganisationRole.Roles.DTC, out var deliveryCartageCo) ? deliveryCartageCo.MainAddress.PK : ZGuid.Empty;
			shipment.JS_OH_DeliveryAgent = rateQueryParties.TryGetValue(OrganisationRole.Roles.DA, out var deliveryAgent) ? deliveryAgent.PK : ZGuid.Empty;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = rateQueryParties.TryGetValue(OrganisationRole.Roles.PA, out var pickupAgent) ? pickupAgent.PK : ZGuid.Empty;
			shipment.JS_OA_ImportReleaseDepot = rateQueryParties.TryGetValue(OrganisationRole.Roles.ICFS, out var importCFS) ? importCFS.MainAddress.PK : ZGuid.Empty;
			shipment.JS_OA_ExportReceivingDepot = rateQueryParties.TryGetValue(OrganisationRole.Roles.ECFS, out var exportCFS) ? exportCFS.MainAddress.PK : ZGuid.Empty;

			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.CA, out var controllingAgent))
			{
				var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
				controllingAgentAddress.OrganisationPK = controllingAgent.PK;
			}

			consol.Transports.MostInterestingTransport.CreditorPK = rateQueryParties.TryGetValue(OrganisationRole.Roles.COR, out var creditorOnRoute) ? creditorOnRoute.PK : ZGuid.Empty;
			if (rateQueryBO.Carrier != null)
			{
				consol.SetDefaultShippingLineAddress(rateQueryBO.Carrier);
			}

			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.SAG, out var sendingAgent))
			{
				consol.SetDefaultSendingForwarderAddress(sendingAgent);
			}

			if (rateQueryParties.TryGetValue(OrganisationRole.Roles.RAG, out var receivingAgent))
			{
				consol.SetDefaultReceivingForwarderAddress(receivingAgent);
			}

			consol.JK_OA_CreditorAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.CCR, out var creditor) ? creditor.MainAddress.PK : ZGuid.Empty;

			consol.Transports.MostInterestingTransport.JW_OA_ArrivalLocation = rateQueryParties.TryGetValue(OrganisationRole.Roles.ACTR, out var arrivalCTOOnRoute) ? arrivalCTOOnRoute.MainAddress.PK : ZGuid.Empty;
			consol.JK_OA_ArrivalCTOAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.ACTO, out var arrivalCTO) ? arrivalCTO.GetAddressWithFallback(AddressType.DLV).PK : ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.ACFS, out var arrivalCFS) ? arrivalCFS.GetAddressWithFallback(AddressType.DLV).PK : ZGuid.Empty;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.ACFT, out var arrivalCFSTransport) ? arrivalCFSTransport.MainAddress.PK : ZGuid.Empty;

			consol.Transports.MostInterestingTransport.JW_OA_DepartureLocation = rateQueryParties.TryGetValue(OrganisationRole.Roles.DCTR, out var departureCTOOnRoute) ? departureCTOOnRoute.MainAddress.PK : ZGuid.Empty;
			consol.JK_OA_DepartureCTOAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.DCTO, out var departureCTO) ? departureCTO.GetAddressWithFallback(AddressType.PIC).PK : ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.DCFS, out var departureCFS) ? departureCFS.GetAddressWithFallback(AddressType.PIC).PK : ZGuid.Empty;
			consol.JK_OA_DeparturePackCFSTransportAddress = rateQueryParties.TryGetValue(OrganisationRole.Roles.DCFT, out var departureCFSTransport) ? departureCFSTransport.MainAddress.PK : ZGuid.Empty;

			if (setAllPossibleInformations)
			{
				SetConsolContainersAndShipmentPackLines(shipment, rateQueryBO);
				SetShipmentJobServices(shipment, rateQueryBO);
				SetShipmentCustomFields(shipment, rateQueryBO);
			}

			return shipment;
		}

		static void SetConsolContainersAndShipmentPackLines(ForwardingShipment shipment, RateQueryBusinessObject rateQueryBO)
		{
			var consol = shipment.Consols.Cast<ForwardingConsol>().Single();
			consol.Containers.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var rateQuery = rateQueryBO.RateQuery;

			foreach (var container in rateQuery.JobInfo?.Containers ?? Array.Empty<JobContainer>())
			{
				ForwardingContainer consolContainer = null;

				if (rateQuery.IsContainerized && rateQueryBO.ContainerTypes.TryGetValue(container?.ContainerTypeCWCode, out var refContainer))
				{
					consolContainer = consol.Containers.AddNew();
					consolContainer.JC_RC = refContainer.PK;
					consolContainer.JC_ContainerCount = Convert.ToInt16(container?.Unit ?? 0);
					consolContainer.JC_ContainerNum = container?.Number;
					if (!string.IsNullOrEmpty(container?.Commodity) && rateQueryBO.Commodities.TryGetValue(container.Commodity, out var refCommodityCode))
					{
						consolContainer.JC_RH_NKContainerCommodityCode = refCommodityCode.RH_Code;
					}
				}

				foreach (var packLine in container?.PackLines ?? Array.Empty<JobPackLine>())
				{
					var shipmentPackline = shipment.OuterPackLines.AddNew();

					if (!string.IsNullOrEmpty(packLine?.Commodity) && rateQueryBO.Commodities.TryGetValue(packLine.Commodity, out var refCommodityCode))
					{
						shipmentPackline.JL_RH_NKCommodityCode = refCommodityCode.RH_Code;
					}

					shipmentPackline.JL_F3_NKPackType = packLine?.PackageType;
					shipmentPackline.JL_PackageCount = packLine.Unit;
					shipmentPackline.JL_ActualWeight = packLine?.Weight ?? 0;
					shipmentPackline.JL_ActualWeightUQ = packLine?.WeightUnit;
					shipmentPackline.JL_ActualVolume = packLine?.Volume ?? 0;
					shipmentPackline.JL_ActualVolumeUQ = packLine?.VolumeUnit;

					if (consolContainer != null)
					{
						shipmentPackline.SetContainer(consol, consolContainer);
					}

					if (!string.IsNullOrEmpty(packLine?.DGSubstance) || !string.IsNullOrEmpty(packLine?.DGClass))
					{
						var dg = shipmentPackline.UNDGs.AddNew();
						if (!string.IsNullOrEmpty(packLine?.DGSubstance))
						{
							var substance = GetUNDGSubstance(rateQueryBO, packLine?.DGSubstance);
							if (substance != null)
							{
								dg.DI_DG = substance.PK;
							}
						}
						else
						{
							dg.DI_IMOClass = packLine.DGClass;
						}
					}
				}
			}
		}

		static UNDGSubstance GetUNDGSubstance(RateQueryBusinessObject rateQueryBO, string dgsubstanceCode)
		{
			var query = new ZQuery(UNDGSubstanceSchema.DG_Code, dgsubstanceCode);
			return rateQueryBO.Factory.Load<UNDGSubstance>(query).FirstOrDefault();
		}

		static void SetShipmentJobServices(ForwardingShipment shipment, RateQueryBusinessObject rateQueryBO)
		{
			shipment.DocsAndCartage.Services.RemoveAndDeleteAll();
			var rateQuery = rateQueryBO.RateQuery;

			foreach (var jobService in rateQuery.JobInfo?.JobServices ?? Array.Empty<JobService>())
			{
				if (jobService != null)
				{
					var shipmentJobService = shipment.DocsAndCartage.Services.AddNew();
					shipmentJobService.ES_ServiceCode = jobService.Type;
					shipmentJobService.ES_Duration = new TimeSpan(jobService.Duration / 60, jobService.Duration % 60, 0);
					shipmentJobService.ES_MeasurementBasis = jobService.MeasurementBasis;

					if (jobService.Booked.HasValue)
					{
						shipmentJobService.ES_Booked = jobService.Booked.Value.ToLocalTime().DateTime;
					}

					if (jobService.Completed.HasValue)
					{
						shipmentJobService.ES_Completed = jobService.Completed.Value.ToLocalTime().DateTime;
					}

					var contractor = jobService.Contractor.ResolveOrganisation(rateQueryBO.Factory);
					if (contractor != null)
					{
						shipmentJobService.ES_OH_Contractor = contractor.PK;
					}

					if (jobService.Location?.Value != null)
					{
						var cw1Location = LocationHelper.GetCachedLocationFromString(jobService.Location.Value, rateQueryBO.Factory);
						var serviceLocationAddress = rateQueryBO.Factory.New<OrgAddress>();
						serviceLocationAddress.OA_RN_NKCountryCode = cw1Location.Country.Code;
						serviceLocationAddress.OA_RL_NKRelatedPortCode = cw1Location.IsUNLOCO() ? cw1Location.Code : ZString.Empty;

						shipmentJobService.ES_OA_Location = serviceLocationAddress.PK;
					}

					shipmentJobService.ES_ServiceCount = jobService.Count;
					shipmentJobService.ES_ServiceRate = jobService.Rate;
					shipmentJobService.ES_RX_NKServiceRateCurrency = jobService.RateCurrency;
				}
			}
		}

		static void SetShipmentCustomFields(ForwardingShipment shipment, RateQueryBusinessObject rateQueryBO)
		{
			var rateQueryShipmentCustomFields = ImmutableDictionary<string, string>.Empty as IDictionary<string, string>;
			rateQueryShipmentCustomFields = rateQueryBO.CustomFields ?? rateQueryShipmentCustomFields;

			if (rateQueryShipmentCustomFields.Count == 0)
			{
				return;
			}

			var shipmentCustomBO = shipment.GetCustomBusinessObject(createWhenAbsent: true);

			foreach (var rateQueryShipmentCustomField in rateQueryShipmentCustomFields)
			{
				var name = rateQueryShipmentCustomField.Key;
				var shipmentCustomFieldAccessor = shipmentCustomBO.GetCustomFieldAccessor(name, null);

				if (shipmentCustomFieldAccessor != null)
				{
					try
					{
						var mappedValue = ZDataType.ObjectToZType(shipmentCustomFieldAccessor.MetaData.Type, rateQueryShipmentCustomField.Value);
						shipmentCustomFieldAccessor.SetValue(mappedValue);
					}
					catch (Exception ex)
					{
						var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(RateQueryBusinessObject.CustomFields)}[{name}]";
						var propKey = $"{nameof(RateQuery)}.{nameof(RateQuery.JobInfo)}.{nameof(RateQuery.JobInfo.CustomFields)}[{name}]";

						if (ex is ZTypeValueException)
						{
							ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, $"Provided {propKey} ({rateQueryShipmentCustomField.Value}) is not assignable to this field: data type conversion failed.");
						}
						else
						{
							ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, $"Provided {propKey} ({rateQueryShipmentCustomField.Value}) could not be assigned to this field: unexpected error.");
						}
					}
				}
				else
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(RateQueryBusinessObject.CustomFields)}[{name}]";
					var propKey = $"{nameof(RateQuery)}.{nameof(RateQuery.JobInfo)}.{nameof(RateQuery.JobInfo.CustomFields)}[{name}]";

					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, $"Provided {propKey} ({rateQueryShipmentCustomField.Value}) is not assignable to this field: custom field {name} is not defined.");
				}
			}
		}
	}
}

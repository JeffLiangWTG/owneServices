using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public static class ShipmentRatingExtensions
	{
		#region JobServices

		public static JobServicesCollection GetJobServices(this CommonShipment shipment)
		{
			var result = new JobServicesCollection();

			if (shipment == null)
			{
				return result;
			}

			var docsAndCartage = shipment.DocsAndCartage;
			var pickUpOrg = docsAndCartage.PickupCartageCo;
			var deliveryOrg = docsAndCartage.DeliveryCartageCo;
			var unit = JobServiceInfo.Constants.Codes.Hour;
			var isShipmentPenaltyApplicable = shipment.HasSeaTransportModeConsol();

			result.Add(GetOriginLaborServiceInfo(docsAndCartage, pickUpOrg, unit));
			result.Add(GetDestinationLaborServiceInfo(docsAndCartage, deliveryOrg, unit));

			result.Add(GetOriginDemurrageServiceInfo(docsAndCartage, pickUpOrg, unit));
			result.Add(GetDestinationDemurrageServiceInfo(docsAndCartage, deliveryOrg, unit));

			if (!shipment.IsShipmentPickupPenaltyApplicable(isShipmentPenaltyApplicable))
			{
				result.Add(GetOriginDetentionServiceInfo(shipment));
			}
			else
			{
				shipment.PickupPenalties.AddPenaltiesServices(result, ChargeCodeGroupList.Codes.Origin);
			}

			if (!shipment.IsShipmentDeliveryPenaltyApplicable(isShipmentPenaltyApplicable))
			{
				result.Add(GetDestinationStorageServiceInfo(docsAndCartage, shipment.ImportReleaseDepot, shipment.IsAir));
				result.Add(GetDestinationDetentionServiceInfo(shipment));
			}
			else
			{
				shipment.DeliveryPenalties.AddPenaltiesServices(result, ChargeCodeGroupList.Codes.Destination);
			}

			result.Add(GetOriginBeyondCartageInfo(shipment, pickUpOrg));
			result.Add(GetDestinationBeyondCartageInfo(shipment, deliveryOrg));

			result.AddRange(shipment.GetOverweightServices());

			return result;
		}

		#region Add Service Infos from Penalties

		static void AddPenaltiesServices(this ShipmentContainerPenaltyCollection shipmentContainerPenalties, JobServicesCollection jobServiceInfos, ZString chargeCodeGroup)
		{
			foreach (CodeDescriptionPair service in FreightRatingHelper.HiddenContainerServices)
			{
				var penalties = shipmentContainerPenalties.Where(x => x.JobServiceCode == service.Code);

				if (penalties.Any())
				{
					penalties.ForEach(x => jobServiceInfos.Add(x.GetJobServiceInfoByShipmentContainerPenalty(chargeCodeGroup)));
				}
				else
				{
					jobServiceInfos.Add(new JobServiceInfo(false, chargeCodeGroup, service.Code, service.Description, useTotalCostAndIgnoreRate: true) { });
				}
			}
		}

		static JobServiceInfo GetJobServiceInfoByShipmentContainerPenalty(this ShipmentContainerPenalty penalty, ZString chargeCodeGroup)
		{
			var jobServiceCode = penalty.JobServiceCode;
			var duration = !penalty.CPY_Duration.IsEmpty && penalty.CPY_Duration.IsValid ? penalty.CPY_Duration.ToTimeSpan() : new TimeSpan();
			var isEnabled = duration.TotalHours > 0 && !jobServiceCode.IsEmpty && !penalty.CPY_JC_Container.IsEmpty;

			var jobServiceInfo = new JobServiceInfo(isEnabled,
				chargeCodeGroup,
				jobServiceCode,
				penalty.PenaltyTypeDescription,
				duration: duration,
				contractor: penalty.Creditor,
				rate: penalty.CPY_PerUnitCost,
				unit: penalty.JobServiceTimeUnit,
				totalCost: penalty.CPY_TotalCost,
				currencyCode: penalty.CPY_RX_NKCurrency,
				useTotalCostAndIgnoreRate: true
			);

			jobServiceInfo.IsContractorCreditor = penalty.Creditor != null;

			if (isEnabled)
			{
				FreightRatingHelper.PopulateServiceContainerInfo(jobServiceInfo, penalty.Container);
			}

			return jobServiceInfo;
		}

		#endregion

		#region Get Service Infos from Docs and Cartage

		static JobServiceInfo GetOriginDemurrageServiceInfo(JobDocsAndCartage jdc, OrgHeader contractor, ZString unit)
		{
			var isEnabled = jdc.JP_PickupTruckWaitTime.IsValid;

			var rate = jdc.JP_PickupTruckWaitCharge;
			var time = isEnabled ? jdc.JP_PickupTruckWaitTime.ToTimeSpan() : new TimeSpan(0);
			var description = Res.GetString("cf05415e-c5f0-4565-86d8-3f2be796c3b0", "Pickup Demurrage");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal, description, null, time, contractor, rate, unit);
		}

		static JobServiceInfo GetOriginLaborServiceInfo(JobDocsAndCartage jdc, OrgHeader contractor, ZString unit)
		{
			var isEnabled = jdc.JP_PickupLabourTime.IsValid;

			var rate = jdc.JP_PickupLabourCharge;
			var time = isEnabled ? jdc.JP_PickupLabourTime.ToTimeSpan() : new TimeSpan(0);
			var description = Res.GetString("90a17193-8157-44e7-a698-cb1599c9dd9c", "Pickup Labor");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor, description, null, time, contractor, rate, unit);
		}

		static JobServiceInfo GetOriginDetentionServiceInfo(CommonShipment shipment)
		{
			var cartageDetentionDays = shipment.DocsAndCartage.JP_FCLPickupDetentionDays;
			var cartageDetentionCharge = shipment.DocsAndCartage.JP_FCLPickupDetentionCharge;

			return new JobServiceInfo(
				cartageDetentionDays > 0,
				ChargeCodeGroupList.Codes.Origin,
				ChargeCodeSubGroupList.ContainerDetention,
				Res.GetString("b2837829-dc95-4e4a-847f-5a8e7e21d08d", "Detention"),
				duration: TimeSpan.FromDays(cartageDetentionDays),
				rate: cartageDetentionCharge,
				unit: JobServiceInfo.Constants.Codes.Day);
		}

		static JobServiceInfo GetDestinationDemurrageServiceInfo(JobDocsAndCartage jdc, OrgHeader contractor, ZString unit)
		{
			var isEnabled = jdc.JP_DeliveryTruckWaitTime.IsValid;

			var rate = jdc.JP_DeliveryTruckWaitCharge;
			var time = isEnabled ? jdc.JP_DeliveryTruckWaitTime.ToTimeSpan() : new TimeSpan(0);
			var description = Res.GetString("353c3f35-edf0-44d4-95e7-64b82c5992ae", "Delivery Demurrage");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal, description, null, time, contractor, rate, unit);
		}

		static JobServiceInfo GetDestinationLaborServiceInfo(JobDocsAndCartage jdc, OrgHeader contractor, ZString unit)
		{
			var isEnabled = jdc.JP_DeliveryLabourTime.IsValid;

			var rate = jdc.JP_DeliveryLabourCharge;
			var time = isEnabled ? jdc.JP_DeliveryLabourTime.ToTimeSpan() : new TimeSpan(0);
			var description = Res.GetString("f1fb2baa-54fb-4342-8505-765000c21a72", "Delivery Labor");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor, description, null, time, contractor, rate, unit);
		}

		static JobServiceInfo GetDestinationStorageServiceInfo(JobDocsAndCartage jdc, OrgAddress importAddress, bool useHours)
		{
			var rate = jdc.JP_LCLAirStorageCharge;
			var isEnabled = jdc.JP_LCLAirStorageDaysOrHours > 0;
			var description = Res.GetString("a22d3b3b-3100-4e76-a8f3-1d1c91274e45", "Storage");
			var contractor = importAddress != null ? importAddress.Header : null;

			var duration = useHours ? TimeSpan.FromHours(jdc.JP_LCLAirStorageDaysOrHours) : TimeSpan.FromDays(jdc.JP_LCLAirStorageDaysOrHours);
			var unit = useHours ? JobServiceInfo.Constants.Codes.Hour : JobServiceInfo.Constants.Codes.Day;

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, description, null, duration, contractor, rate, unit);
		}

		static JobServiceInfo GetDestinationDetentionServiceInfo(CommonShipment shipment)
		{
			var cartageDetentionDays = shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays;
			var cartageDetentionCharge = shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge;

			return new JobServiceInfo(
				cartageDetentionDays > 0,
				ChargeCodeGroupList.Codes.Destination,
				ChargeCodeSubGroupList.ContainerDetention,
				Res.GetString("b2837829-dc95-4e4a-847f-5a8e7e21d08d", "Detention"),
				duration: TimeSpan.FromDays(cartageDetentionDays),
				rate: cartageDetentionCharge,
				unit: JobServiceInfo.Constants.Codes.Day);
		}

		#endregion

		#region Get Overweight Service Infos

		public static JobServicesCollection GetOverweightServices(this CommonShipment shipment)
		{
			var result = new JobServicesCollection();

			if (shipment == null)
			{
				return result;
			}

			#region OverweightPenalty

			var description = Res.GetString("0955e1e7-69c2-4f0b-accb-c295a7af9da9", "Overweight Penalty");

			var count = shipment.JS_ActualChargeable != 0m
				? (1 - shipment.JS_DocumentedChargeable / shipment.JS_ActualChargeable) * 100
				: 0m;

			bool isEnabled = count > 0m;

			result.Add(new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.OverweightPenalty, description, count));
			result.Add(new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.OverweightPenalty, description, count));

			#endregion

			#region OverweightSurcharge

			description = Res.GetString("8a111960-3bd5-4392-bc1f-e43269a91930", "Overweight Surcharge");

			count = shipment.JS_ActualChargeable - shipment.JS_DocumentedChargeable;

			isEnabled = count > 0m;

			result.Add(new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.OverweightSurcharge, description, count));
			result.Add(new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.OverweightSurcharge, description, count));

			#endregion

			return result;
		}

		#endregion

		#region Get Service Charge Group

		public static string GetServiceChargeGroup(this CommonShipment shipment, JobServiceInfo info)
		{
			Argument.NotNull(info, "info");

			ZDateTime jobServicesCutOffDate;
			string servicesChargeGroup;
			Transport firstTransport = shipment.TransportsIncludingRelated.DepartureTransport;

			if (firstTransport == null)
			{
				servicesChargeGroup = ChargeCodeGroupList.Codes.Origin;
				jobServicesCutOffDate = shipment.JS_E_DEP;
			}
			else if (!firstTransport.JW_ATD.IsEmpty)
			{
				servicesChargeGroup = ChargeCodeGroupList.Codes.Destination;
				jobServicesCutOffDate = firstTransport.JW_ATD;
			}
			else
			{
				jobServicesCutOffDate = firstTransport.JW_ETD.IsEmpty ? shipment.JS_E_DEP : firstTransport.JW_ETD;
				servicesChargeGroup = ChargeCodeGroupList.Codes.Origin;
			}

			if (!info.IsEnabled)
			{
				return servicesChargeGroup;
			}

			if (!info.LocationCountryCode.IsEmpty && !shipment.IsDomestic())
			{
				if (shipment.Origin != null && info.LocationCountryCode == shipment.Origin.RL_RN_NKCountryCode)
				{
					return ChargeCodeGroupList.Codes.Origin;
				}

				if (shipment.Destination != null && info.LocationCountryCode == shipment.Destination.RL_RN_NKCountryCode)
				{
					return ChargeCodeGroupList.Codes.Destination;
				}
			}

			if (jobServicesCutOffDate.IsEmpty)
			{
				return servicesChargeGroup;
			}
			else
			{
				return info.CompletedDate <= jobServicesCutOffDate ? ChargeCodeGroupList.Codes.Origin : ChargeCodeGroupList.Codes.Destination;
			}
		}

		#endregion

		#region Get 'Is Beyond' Postcode Transport Zones Service Infos

		static JobServiceInfo GetOriginBeyondCartageInfo(this CommonShipment shipment, IOrgHeader transportProvider)
		{
			var isEnabled = IsBeyondCartage(shipment, shipment.Origin, shipment.ConsignorPickupAddress, transportProvider);
			var description = Res.GetString("a4f3140f-9ef5-4cd4-9b6b-9d2d8e0293a0", "Pickup Port Transport Beyond");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode, description);
		}

		static JobServiceInfo GetDestinationBeyondCartageInfo(this CommonShipment shipment, IOrgHeader transportProvider)
		{
			var isEnabled = IsBeyondCartage(shipment, shipment.Destination, shipment.ConsigneeDeliveryAddress, transportProvider);
			var description = Res.GetString("2db2d6bb-81a1-46f2-b7c3-5f468fdf3dea", "Delivery Port Transport Beyond");

			return new JobServiceInfo(isEnabled, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode, description);
		}

		static ZBool IsBeyondCartage(this CommonShipment shipment, RefUNLOCO unloco, JobDocAddress address, IOrgHeader transportProvider)
		{
			ZBool result = false;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Canada)
			{
				var portCode = unloco != null ? unloco.Code : ZString.Empty;
				RefDomesticCartageZone aciZone = RefDomesticCartageZone.GetZone(shipment.Factory, address.E2_Postcode, portCode);
				if (aciZone != null)
				{
					result = aciZone.F1_IsBeyond;
				}
			}
			else
			{
				result = shipment.RateTransportZoneHelper.IsBeyond(shipment.Factory, transportProvider, unloco, address.E2_RN_NKCountryCode, address.E2_Postcode, address.E2_City);
			}

			return result;
		}

		#endregion

		#endregion

		#region AutoRatingStatusInfo

		public static AutoRatingStatusInfo GetStatusInformation(this CommonShipment shipment)
		{
			ZString message = ZString.Empty;
			ZBool canExecute = true;

			if (shipment.JS_PackingMode == Constants.ContainerModes.FCL || shipment.JS_PackingMode == Constants.ContainerModes.ULD)
			{
				if (shipment.Consols.Count == 0)
				{
					canExecute = false;
					message = Res.GetString("fcb8ef60-ab93-477b-ade5-a6dd1694a25d", "A Consol must be attached to this Shipment before you can do {0} Autorating.", shipment.JS_PackingMode);
				}
				else if (UnallocatedContainers(shipment).Count > 0)
				{
					message = Res.GetString("51d47c41-d21d-4b74-9d45-646162a16812", "All containers must be allocated to a Shipment before Autorating can occur.\r\nThe following containers are not allocated to any Shipment:") + "\r\n\r\n";

					foreach (CommonContainer container in UnallocatedContainers(shipment))
					{
						if (!container.JC_ContainerNum.IsEmpty)
						{
							message += "  " + Res.GetString("67775522-459d-43fa-9631-18a5d56fcfa4", "- Container {0}", container.JC_ContainerNum) + "\r\n";
						}
						else
						{
							message += "  " + Res.GetString("1604f032-166e-4416-b02c-cd0a2b6e6398", "- {0} unnumbered containers", container.JC_ContainerCount) + "\r\n";
						}
					}

					if (IsOnlyShipmentInConsol(shipment) && (!shipment.IsMasterShipmentRepresentingAllChildShipments))
					{
						message += "\r\n" + Res.GetString("76d24a26-7393-4642-9e7a-7f406bac9baf", "Since this is the only Shipment on the Consol, all Containers will be allocated to this Shipment. Is this correct?");
						if (shipment.OnOnlyShipmentInConsol(message).Cancel)
						{
							canExecute = false;
							message = Res.GetString("503484ec-eaf7-4bf6-8fc7-d87fe660fbea", "You chose to not allocate the unallocated containers to this Shipment. Please allocate them before Autorating.");
						}
						else
						{
							shipment.AllocateUnallocatedContainersToThisShipment();
							message = ZString.Empty;
						}
					}
					else
					{
						canExecute = false;
						message += "\r\n" + Res.GetString("b753672b-3b3f-48f1-b1d3-a54e53bbe30a", "Please allocate them before Autorating.");
					}
				}
			}

			if (!shipment.JS_JS_ColoadMasterShipment.IsEmpty && !shipment.JS_JS_ColoadMasterShipment.IsValid)
			{
				canExecute = false;
				message += "\r\n" + Res.GetString("b753672b-3b3f-48f1-b1d3-a54e53bbe30b", "Please enter a valid value for {0}.", shipment.JS_JS_ColoadMasterShipmentForBindingInfo.HumanReadableName);
			}

			return new AutoRatingStatusInfo(canExecute, message);
		}

		static void AllocateUnallocatedContainersToThisShipment(this CommonShipment shipment)
		{
			foreach (CommonContainer container in UnallocatedContainers(shipment))
			{
				PackLine line = shipment.OuterPackLines.AddNew();
				line.JL_JC = container.PK;
			}
		}

		public static List<CommonContainer> UnallocatedContainers(this CommonShipment shipment)
		{
			var results = new List<CommonContainer>();
			var firstOrCorrectConsol = shipment.GetFirstOrCorrectConsol();

			if (firstOrCorrectConsol != null)
			{
				results.AddRange(firstOrCorrectConsol
										.Containers
										.Cast<CommonContainer>()
										.Where(x => x.PackLines.Count == 0));
			}

			return results;
		}

		public static bool IsOnlyShipmentInConsol(this CommonShipment shipment)
		{
			var firstOrCorrectConsol = shipment.GetFirstOrCorrectConsol();
			return firstOrCorrectConsol != null && firstOrCorrectConsol.Shipments.Count == 1 && firstOrCorrectConsol.Shipments[0] == shipment;
		}

		#endregion

		public static CommonShipment GetConsolLeadShipment(this CommonShipment shipment)
		{
			if (shipment.IsBuyersConsolLead || shipment.IsShippersConsolLead)
			{
				return shipment;
			}
			else if (shipment.CoLoadMasterShipment != null
				&& (shipment.CoLoadMasterShipment.IsBuyersConsolLead
				|| shipment.CoLoadMasterShipment.IsShippersConsolLead))
			{
				return shipment.CoLoadMasterShipment;
			}

			return null;
		}

		public static bool ApportionConsol(this CommonShipment parent)
		{
			if (parent.Job == null)
			{
				return false;
			}

			if (parent.PackingMode == Constants.ContainerModes.ShippersConsol)
			{
				return parent.Job.LocalCharges != null
					&& (parent.Job.LocalCharges.CompanyData.EffectiveShippersConsolInvoicingStyle == Constants.ConsolInvoicingStyles.Apportion
						|| parent.Job.LocalCharges.CompanyData.EffectiveShippersConsolInvoicingStyle == Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);
			}
			else if (parent.PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				// In case if cross trade shipments, Local Client (LocalCharges in BusinessObject) in the UI is renamed to Prepaid Bill To Party,
				// and Overseas Agent (AgentCollect in BusinessObject) in the UI is renamed to Collect Bill To Party. Since in case of BCN shipment,
				// the buyer always pays, then we need to check the Collect Bill To Party for the effective invoicing style.
				// So, basically, in case of Cross Trade shipments, Local Client and Overseas Agent are swapped.
				//
				// I had to use smelly CanCrossTradeDebtorDefaultingBeApplied flag as it is the one recommended to use by accounting team, and it is used by the UI
				// to change naming of Local Client and Overseas Agent.
				var billingParty = parent.Job.CanCrossTradeDebtorDefaultingBeApplied ? parent.Job.AgentCollect : parent.Job.LocalCharges;

				return billingParty != null
					&& (billingParty.CompanyData.EffectiveBuyersConsolInvoicingStyle == Constants.ConsolInvoicingStyles.Apportion
						|| billingParty.CompanyData.EffectiveBuyersConsolInvoicingStyle == Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);
			}
			else
			{
				return false;
			}
		}

		public static CommonConsol GetFirstOrCorrectConsol(this CommonShipment shipment)
		{
			if (shipment.Consols == null)
			{
				return null;
			}

			if (shipment.Consols.Count == 1)
			{
				return shipment.Consols[0];
			}

			var possibleGatewayConsols = shipment.Consols
				.Cast<CommonConsol>()
				.Where(c => c.JK_IsForwarding && c.LoadPort != null && c.LoadPort.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				.ToArray();

			foreach (var consol in possibleGatewayConsols)
			{
				if (consol.IsGatewayBillingEnabled())
				{
					return consol;
				}
			}

			return shipment.FindCorrectConsol();
		}

		public static bool IsConsolLeadShipment(this IBusiness bizO) =>
			bizO is CommonShipment shipment &&
			(shipment.JS_PackingMode == Constants.ContainerModes.BuyersConsol
			|| shipment.JS_PackingMode == Constants.ContainerModes.ShippersConsol);
	}
}

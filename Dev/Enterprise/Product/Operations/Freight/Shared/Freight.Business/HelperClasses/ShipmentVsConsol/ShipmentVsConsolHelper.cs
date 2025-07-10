using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public static class ShipmentVsConsolHelper
	{
		public static bool AllowAttachingDirectShipmentsToAgentConsols(IEnumerable<CommonShipment> directShipments, IEnumerable<CommonConsol> agentConsols)
		{
			var shipmentsList = directShipments.ToList();
			var consolsList = agentConsols.ToList();

			if (shipmentsList.Any(s => !s.IsDirectShipment)
				|| consolsList.Any(c => !c.IsAgent))
			{
				return false;
			}

			foreach (var consol in consolsList)
			{
				if (!shipmentsList.All(shipment => IsPreCarriageForDirectShipment(consol, shipment) || IsOnForwardingForDirectShipment(consol, shipment)))
				{
					return false;
				}
			}

			return true;
		}

		public static bool IsPreCarriageForDirectShipment(CommonConsol consol, CommonShipment shipment)
		{
			var preCarriageETA = consol.Transports.ArrivalTransport?.JW_ETA ?? ZDateTime.Invalid;
			var departureDirectShipmentETD = shipment?.DepartureConsol?.Transports.DepartureTransport?.JW_ETD ?? ZDateTime.Invalid;
			return consol.IsAgent
				   && (consol.IsRoad || consol.IsRail || consol.IsDomesticSeaOrAir())
				   && consol.JK_RL_NKDischargePort == (shipment?.DepartureConsol?.JK_RL_NKLoadPort ?? ZString.Empty)
				   && preCarriageETA < departureDirectShipmentETD;
		}

		public static bool IsOnForwardingForDirectShipment(CommonConsol consol, CommonShipment shipment)
		{
			var onForwardingETD = consol.Transports.DepartureTransport?.JW_ETD ?? ZDateTime.Invalid;
			var arrivalDirectShipmentETA = shipment.ArrivalConsol?.Transports.ArrivalTransport?.JW_ETA ?? ZDateTime.Invalid;
			return consol.IsAgent
				   && (consol.IsRoad || consol.IsRail || consol.IsDomesticSeaOrAir())
				   && consol.JK_RL_NKLoadPort == (shipment.ArrivalConsol?.JK_RL_NKDischargePort ?? ZString.Empty)
				   && arrivalDirectShipmentETA < onForwardingETD;
		}

		/// <summary>
		/// This delegate designed to popup a login form from GUI to be injected to a parent job (consol/shipment) in Business
		/// </summary>
		/// <param name="message">The message to be shown on the login form</param>
		/// <param name="permission">The security checkpoint to be checked</param>
		/// <returns></returns>
		public delegate bool RequestPermissionByImpersonation(string message, SecurityCheckpoint permission);

		/// <summary>
		/// This class designed to manage all the logic of Shipment/Consol attachment validations.
		/// </summary>
		public class ExclusiveGatewayServiceChecker
		{
			public string CheckUserPermissionToAttachShipmentWithDifferentServiceLevel
			(
				AttachDetachAction userAction,
				CommonShipment shipment,
				CommonConsol consol
			)
			{
				if (userAction != AttachDetachAction.New && userAction != AttachDetachAction.Attach)
				{
					return string.Empty;
				}

				var handlingTypes = new List<ZString>()
				{
					AgentStatusList.Codes.GatewayAgent,
					AgentStatusList.Codes.GatewayAgentWithTariff,
				};

				OrgHeader referenceOrg = null;
				if (consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid && handlingTypes.Contains(consol.JK_SendingForwarderHandlingType))
				{
					referenceOrg = consol.SendingForwarder;
				}
				if (consol.JK_PrepaidCollect == Core.Constants.PaymentType.Collect && handlingTypes.Contains(consol.JK_ReceivingForwarderHandlingType))
				{
					referenceOrg = consol.ReceivingForwarder;
				}

				var consolGatewayServiceLevel = consol.JK_RS_NKGatewayServiceLevel;
				if (consolGatewayServiceLevel.IsEmpty || referenceOrg == null)
				{
					return string.Empty;
				}

				var shipmentGatewayServiceLevel = shipment.JS_RS_NKGatewayServiceLevel;
				if (shipmentGatewayServiceLevel.IsEmpty)
				{
					return GetMandatoryErrorMessage(shipment.HumanReadableName, consol.HumanReadableName, consolGatewayServiceLevel);
				}

				var supportedShipmentServiceLevels = FindSupportedShipmentServiceLevels(referenceOrg, consolGatewayServiceLevel);
				if (supportedShipmentServiceLevels == null || supportedShipmentServiceLevels.Contains(shipmentGatewayServiceLevel))
				{
					return string.Empty;
				}

				IsAllowed = IsAllowed || Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed;

				if (IsAllowed)
				{
					return string.Empty;
				}
				else if (!Requested)
				{
					var requestPermission = consol.RequestPermissionByImpersonation ?? shipment.RequestPermissionByImpersonation;
					if (requestPermission != null)
					{
						Requested = true;

						IsAllowed = requestPermission(
							GetImpersonationMessage(consolGatewayServiceLevel),
							Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel);

						if (IsAllowed)
						{
							return string.Empty;
						}
					}
				}

				return GetErrorMessage(shipmentGatewayServiceLevel, consolGatewayServiceLevel, referenceOrg.OH_Code);
			}

			ZString[] FindSupportedShipmentServiceLevels(OrgHeader org, ZString consolGatewayServiceLevel)
			{
				var exclusiveGatewayServices =
					org.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>()
					.SelectMany(x => x.ExclusiveGatewayServices).Cast<OrgExclusiveGatewayService>();

				if (exclusiveGatewayServices.Any())
				{
					return exclusiveGatewayServices
						.Where(x => x.O7_RS_NKGatewayService == consolGatewayServiceLevel)
						.Select(x => x.O7_RS_NKShipmentServiceLevel)
						.ToArray();
				}

				return null;
			}

			string GetMandatoryErrorMessage(string shipmentCode, string consolCode, string consolGatewayServiceLevel) =>
				ResString.GetMultilingualString(
					"72FE76DB-ACD9-43F5-99F3-F2C63E0AD841",
					"Gateway Service Level for {0} cannot be blank. Because it's attached to {1} with Gateway Service Level '{2}'.",
					string.IsNullOrWhiteSpace(shipmentCode) ? (NoResString)"Shipment" : shipmentCode,
					string.IsNullOrWhiteSpace(consolCode) ? (NoResString)"a Consol" : consolCode,
					consolGatewayServiceLevel);

			string GetImpersonationMessage(string consolGatewayServiceLevel) =>
				ResString.GetMultilingualString(
					"01964B40-595B-4364-801B-459E2176F774",
					"To attach a Shipment with Gateway Service Level not supported by '{0}' chosen on the Consol, a user with relevant security access must login. Please enter username and password details below.",
					consolGatewayServiceLevel);

			string GetErrorMessage(string shipmentServiceLevel, string consolGatewayServiceLevel, string orgCode) =>
				ResString.GetMultilingualString(
					"A015235F-4B41-42A2-97FC-8FD4EA9DB071",
					"You don't have permission to attach a shipment with Gateway Service Level '{0}' to a consol with Gateway Service Level '{1}'. Please contact the administrator to either set up this relation for the organization '{2}' or give you permission for this operation.",
					shipmentServiceLevel,
					consolGatewayServiceLevel,
					orgCode);

			bool Requested { get; set; }
			bool IsAllowed { get; set; }
		}
	}
}

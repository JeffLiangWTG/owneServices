using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentGatewayValidation : JobShipmentGatewayValidation
	{
		public ShipmentGatewayValidation(ShipmentGateway parent)
			: base(parent)
		{
		}

		#region JSG_OA_ForwarderAddress

		protected override void CheckJSG_OA_ForwarderAddress()
		{
			base.CheckJSG_OA_ForwarderAddress();

			MandatoryValidation.CheckEntered(Parent.JSG_OA_ForwarderAddressInfo);
			ValidateDuplicateAddress();
			ValidateAddressIsValidGateway();
			ValidateConsistencyWithConsolGatewayAgents();
		}

		void ValidateDuplicateAddress()
		{
			var shipment = Parent.Shipment;
			if (shipment != null)
			{
				if (shipment.Gateways.Count(x => x.JSG_OA_ForwarderAddress == Parent.JSG_OA_ForwarderAddress) > 1)
				{
					Parent.JSG_OA_ForwarderAddressInfo.AddError(Res.GetString("99da8d9f-832b-41a7-ad55-c9ff85f6f475", "The Gateway must be unique."));
				}
			}
		}

		void ValidateAddressIsValidGateway()
		{
			var forwarder = Parent.Forwarder;
			if (forwarder != null)
			{
				if (!forwarder.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>()
					.Any(x => x.O5_OA_AgentOfficeAddress == Parent.JSG_OA_ForwarderAddress && IsValidGatewayForAnyPortAndTransportMode(x)))
				{
					Parent.JSG_OA_ForwarderAddressInfo.AddError(Res.GetString("aacf85cd-4d75-445d-8253-998c47a87bf0", @"The address is not a valid Gateway.

Gateway addresses can be configured in Organization -> Fwd/Agent -> Details -> Gateway Agent."));
				}
			}
		}

		void ValidateConsistencyWithConsolGatewayAgents()
		{
			var shipment = Parent.Shipment;
			if (shipment != null && shipment.Consols.Count > 0)
			{
				var consolsGatewayAgentsList = new List<ZGuid>();
				var consolsGatewayAgentsPairs = new List<KeyValuePair<ZGuid, ZGuid>>();
				var sortedConsols = shipment.Consols.Cast<ForwardingConsol>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
				var hasChecked = false;
				var orderErrorMessage = Res.GetString("0BC7EF5B-F133-4553-AF5C-5F4554E7DE81", "Gateways must be listed in their correct order from origin to destination. This Gateway is out of order; please check the order of attached Consols and their Sending and Receiving Gateway Agents.");

				foreach (var consol in sortedConsols)
				{
					var consolSendingForwarder = consol.SendingForwarderAddress?.PK ?? ZGuid.Empty;
					var consolReceivingForwarder = consol.ReceivingForwarderAddress?.PK ?? ZGuid.Empty;

					var consolHasSendingForwarder = !consol.JK_SendingForwarderHandlingType.IsEmpty && !consolSendingForwarder.IsEmpty;
					var consolHasReceivingForwarder = !consol.JK_ReceivingForwarderHandlingType.IsEmpty && !consolReceivingForwarder.IsEmpty;

					if (consolHasSendingForwarder)
					{
						if (Parent.JSG_OA_ForwarderAddress == consolSendingForwarder)
						{
							CheckConsolGatewayOrder(consolsGatewayAgentsList, orderErrorMessage);
							hasChecked = true;
							break;
						}

						if (!consolsGatewayAgentsList.Contains(consolSendingForwarder))
						{
							consolsGatewayAgentsList.Add(consolSendingForwarder);
						}
					}

					if (consolHasReceivingForwarder)
					{
						if (Parent.JSG_OA_ForwarderAddress == consolReceivingForwarder)
						{
							CheckConsolGatewayOrder(consolsGatewayAgentsList, orderErrorMessage);
							hasChecked = true;
							break;
						}

						if (!consolsGatewayAgentsList.Contains(consolReceivingForwarder))
						{
							consolsGatewayAgentsList.Add(consolReceivingForwarder);
						}
					}

					if (consolHasSendingForwarder && consolHasReceivingForwarder)
					{
						var pairGateway = new KeyValuePair<ZGuid, ZGuid>(consolSendingForwarder, consolReceivingForwarder);
						if (!consolsGatewayAgentsPairs.Contains(pairGateway))
						{
							consolsGatewayAgentsPairs.Add(pairGateway);
						}
					}
				}

				if (!hasChecked && !consolsGatewayAgentsList.Contains(Parent.JSG_OA_ForwarderAddress))
				{
					Parent.JSG_OA_ForwarderAddressInfo.AddWarning(Res.GetString("FC93633D-E086-4261-BE15-AF2C8AA9B83B", "This Gateway does not match any of the Gateway Agents on attached Consol/s."));

					if (consolsGatewayAgentsPairs.Any(x => Parent.JSG_Sequence > GetGatewaySequence(x.Key) && Parent.JSG_Sequence < GetGatewaySequence(x.Value) || Parent.JSG_Sequence > GetGatewaySequence(x.Value) && Parent.JSG_Sequence < GetGatewaySequence(x.Key)))
					{
						Parent.JSG_OA_ForwarderAddressInfo.AddError(orderErrorMessage);
					}
				}
			}
		}

		void CheckConsolGatewayOrder(List<ZGuid> consolsGatewayAgentsList, string orderErrorMessage)
		{
			if (consolsGatewayAgentsList.Any(x => Parent.JSG_Sequence <= GetGatewaySequence(x)))
			{
				Parent.JSG_OA_ForwarderAddressInfo.AddError(orderErrorMessage);
			}
		}

		int GetGatewaySequence(ZGuid forwarderAddress)
		{
			var gatewayAddress = Gateways.FirstOrDefault(x => x.JSG_OA_ForwarderAddress == forwarderAddress);

			return gatewayAddress != null ? gatewayAddress.JSG_Sequence : 0;
		}

		ShipmentGatewayCollection Gateways
		{
			get
			{
				if (gateways == null)
				{
					gateways = new ShipmentGatewayCollection(Parent.Shipment);
				}
				return gateways;
			}
		}
		ShipmentGatewayCollection gateways;

		bool IsValidGatewayForAnyPortAndTransportMode(OrgAppointedAgentPorts appointedAgentPorts)
		{
			var validAgentStatuses = new ZString[] { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff };

			return validAgentStatuses.Contains(appointedAgentPorts.O5_AirAgentStatus)
				|| validAgentStatuses.Contains(appointedAgentPorts.O5_SeaAgentStatus)
				|| validAgentStatuses.Contains(appointedAgentPorts.O5_RoadAgentStatus)
				|| validAgentStatuses.Contains(appointedAgentPorts.O5_RailAgentStatus);
		}

		#endregion

		#region ForwarderPK

		public void ValidateForwarderPK()
		{
			ValidateCalculatedProperty(Parent.ForwarderPKInfo);
		}

		protected void CheckForwarderPK()
		{
			var agentDescription = Res.GetString("4c4c9f98-658b-4da4-8938-5d393065614b", "Gateway Agent");
			MandatoryValidation.CheckEntered(Parent.ForwarderPKInfo, agentDescription);
			TypeValidation.CheckValidGuid(Parent.ForwarderPKInfo, agentDescription);
		}

		#endregion

		#region Implementation

		protected new ShipmentGateway Parent => (ShipmentGateway)base.Parent;

		#endregion
	}
}

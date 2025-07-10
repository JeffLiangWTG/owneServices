using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Business
{
	public class CommonConsolValidation : JobConsolValidation
	{
		public CommonConsolValidation(CommonConsol parent)
			: base(parent)
		{
		}

		#region JK_RL_NKLoadPort

		protected override void CheckJK_RL_NKLoadPort()
		{
			base.CheckJK_RL_NKLoadPort();
			MandatoryValidation.CheckEntered(Parent.JK_RL_NKLoadPortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_RL_NKLoadPortInfo, Parent.RefUNLOCO_List);

			if (ShouldValidateConsolValuesMatchFirstShipment && Parent.JK_RL_NKLoadPort != Parent.Shipments[0].JS_RL_NKOrigin)
			{
				Parent.JK_RL_NKLoadPortInfo.AddWarning(Res.GetString("ea084e44-434a-483b-8517-10f0e815aa82", "Loading port does not match Shipment Origin."));
			}
		}

		#endregion

		#region JK_RL_NKDischargePort

		protected override void CheckJK_RL_NKDischargePort()
		{
			base.CheckJK_RL_NKDischargePort();
			MandatoryValidation.CheckEntered(Parent.JK_RL_NKDischargePortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_RL_NKDischargePortInfo, Parent.RefUNLOCO_List);

			if (ShouldValidateConsolValuesMatchFirstShipment && Parent.JK_RL_NKDischargePort != Parent.Shipments[0].JS_RL_NKDestination)
			{
				Parent.JK_RL_NKDischargePortInfo.AddWarning(Res.GetString("b9d06f53-4de8-4182-82f7-6a1783c58bbe", "Discharge Port does not match Shipment Destination."));
			}
		}

		#endregion

		#region JK_TransportMode

		protected override void CheckJK_TransportMode()
		{
			base.CheckJK_TransportMode();
			MandatoryValidation.CheckEntered(Parent.JK_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_TransportModeInfo, Parent.JK_TransportMode_List);

			if (ShouldValidateConsolValuesMatchFirstShipment && Parent.JK_TransportMode != Parent.Shipments[0].JS_TransportMode)
			{
				Parent.JK_TransportModeInfo.AddWarning(Res.GetString("ec00e0cc-9d61-42fa-842d-3c4e855a7719", "Transport Mode does not match Shipment."));
			}
		}

		bool ShouldValidateConsolValuesMatchFirstShipment
		{
			get
			{
				return Parent.IsDirect && Parent.Shipments.Count > 0;
			}
		}

		#endregion

		#region JK_AgentType

		protected override void CheckJK_AgentType()
		{
			base.CheckJK_AgentType();

			var propertyInfo = Parent.JK_AgentTypeInfo;
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo, Parent.JK_AgentType_List);

			var shipments = Parent.Shipments.ToArray<CommonShipment>();

			const int numberOfShipmentsToShowInTheMessage = 3;

			if (Parent.IsDirect)
			{
				if (shipments.Length > 1 && !AreAllShipmentsValidForDirectConsol(Parent.Shipments))
				{
					propertyInfo.AddError(Res.GetString("01371ef1-1d1b-4065-b0ca-7f59054768c2", "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments."));
				}
				else if (shipments.Any(s => !s.IsDirectShipment))
				{
					propertyInfo.AddError(Res.GetString("f943a703-1bf2-4498-afbc-9e0281ca827c", "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master."));
				}
				else
				{
					var shipmentsAttachedToNonDirectConsol = shipments
						.Where(shipment => shipment.Consols.Cast<CommonConsol>()
							.Any(consol => !consol.IsDirect
										   && !ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(consol, shipment)
										   && !ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(consol, shipment)))
						.Take(numberOfShipmentsToShowInTheMessage + 1)
						.ToArray();

					if (shipmentsAttachedToNonDirectConsol.Length > 0)
					{
						(var shipmentsToString, var shouldAddOthersPhrase) = ShipmentsListToString(shipmentsAttachedToNonDirectConsol, numberOfShipmentsToShowInTheMessage);

						if (shouldAddOthersPhrase)
						{
							propertyInfo.AddError(Res.GetString("c1debd4b-ca9d-4516-a8d4-cd87de6d78cf",
								"Consol cannot be marked as Direct, as these and other shipment(s) are already attached to at least one other non-Direct Consol: {0}.",
								shipmentsToString));
						}
						else
						{
							propertyInfo.AddError(Res.GetString("3fface55-6a99-497f-80af-b220f27ebca6",
								"Consol cannot be marked as Direct, as these shipment(s) is/are already attached to at least one other non-Direct Consol: {0}.",
								shipmentsToString));
						}
					}
				}
			}
			else
			{
				var shipmentsAttachedToDirectConsol = shipments.Where(shipment => shipment.IsDirectShipment).ToArray();

				if (shipmentsAttachedToDirectConsol.Length > 0)
				{
					(var shipmentsToString, var shouldAddOthersPhrase) =
						ShipmentsListToString(shipmentsAttachedToDirectConsol, numberOfShipmentsToShowInTheMessage);

					var message = shouldAddOthersPhrase
						? Res.GetString("03809860-0095-43e4-b27f-281684a6dba7",
							"The following shipment(s) and other shipment(s) are already attached to at least one other Direct Consol: {0}. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).",
							shipmentsToString)
						: Res.GetString("2df5876b-5998-4f68-85c5-003013a2027d",
							"The following shipment(s) is/are already attached to at least one other Direct Consol: {0}. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).",
							shipmentsToString);

					if (ShipmentVsConsolHelper.AllowAttachingDirectShipmentsToAgentConsols(shipmentsAttachedToDirectConsol, new[] { Parent }))
					{
						propertyInfo.AddWarning(message);
					}
					else
					{
						propertyInfo.AddError(message);
					}
				}
			}
		}

		(string shipmentsToString, bool shouldAddOthersPhrase) ShipmentsListToString(CommonShipment[] shipmentsAttachedToConsol, int numberOfShipmentsToTake)
		{
			var shouldAddOthersPhrase = false;
			var selectedShipmentsToShow = shipmentsAttachedToConsol;

			if (shipmentsAttachedToConsol.Length > numberOfShipmentsToTake)
			{
				selectedShipmentsToShow = shipmentsAttachedToConsol.Take(numberOfShipmentsToTake).ToArray();
				shouldAddOthersPhrase = true;
			}

			var shipmentsToString = ZString.Join(", ", selectedShipmentsToShow.Select(shipment => new ZString(shipment.JS_UniqueConsignRef)).ToArray());

			return (shipmentsToString, shouldAddOthersPhrase);
		}

		bool AreAllShipmentsValidForDirectConsol(ConsolShipmentCollection shipments)
		{
			var masterLeadShipment = shipments.Cast<CommonShipment>().FirstOrDefault(s => s.IsAssemblyMaster);
			if (masterLeadShipment != null)
			{
				return masterLeadShipment.JS_JS_ColoadMasterShipment.IsEmpty
						&& shipments.Cast<CommonShipment>().All(s => s.IsStandardHouse
															&& s.JS_JS_ColoadMasterShipment == masterLeadShipment.PK
															|| s == masterLeadShipment);
			}

			return false;
		}

		public SecurityCheckpoint GetConsolTypeSecurityCheckpoint()
		{
			switch (Parent.JK_AgentType)
			{
				case Constants.AgentType.Direct:
					return Env.Security.MaintainConsolTypeDirect;
				case Constants.AgentType.CoLoad:
					return Env.Security.MaintainConsolTypeCoLoad;
				case Constants.AgentType.Agent:
					return Env.Security.MaintainConsolTypeAgent;
				case Constants.AgentType.Charter:
					return Env.Security.MaintainConsolTypeCharter;
				case Constants.AgentType.Other:
					return Env.Security.MaintainConsolTypeOther;
				case Constants.AgentType.AWBCoload:
					return Env.Security.MaintainConsolTypeAWBCoload;
				case Constants.AgentType.AWBMaster:
					return Env.Security.MaintainConsolTypeAWBMaster;
				case Constants.AgentType.Courier:
					return Env.Security.MaintainConsolTypeCourier;
				default:
					return Env.Security.None;
			}
		}

		#endregion

		#region JK_PrepaidCollect

		protected override void CheckJK_PrepaidCollect()
		{
			base.CheckJK_PrepaidCollect();
			ListValidation.ErrorIfInvalidCode(Parent.JK_PrepaidCollectInfo, Parent.JK_PrepaidCollect_List);

			if (ShouldValidateConsolValuesMatchFirstShipment && Parent.JK_PrepaidCollect != Parent.Shipments[0].JS_PaymentTerm)
			{
				Parent.JK_PrepaidCollectInfo.AddWarning(Res.GetString("18ddb593-a86e-45ed-9ef0-575dbae0c6b8", "Payment Type does not match Shipment."));
			}
		}

		#endregion

		#region JK_RL_NKFirstForeignPort

		protected override void CheckJK_RL_NKFirstForeignPort()
		{
			base.CheckJK_RL_NKFirstForeignPort();
			ListValidation.ErrorIfInvalidCode(Parent.JK_RL_NKFirstForeignPortInfo, Parent.RefUNLOCO_List);
		}

		#endregion

		#region JK_RL_NKPortOfFirstArrival

		protected override void CheckJK_RL_NKPortOfFirstArrival()
		{
			base.CheckJK_RL_NKPortOfFirstArrival();
			ListValidation.ErrorIfInvalidCode(Parent.JK_RL_NKPortOfFirstArrivalInfo, Parent.RefUNLOCO_List);
		}

		#endregion

		#region CoLoadPrintOptions

		protected override void CheckJK_PrintOptionForColoadsOnManifest()
		{
			base.CheckJK_PrintOptionForColoadsOnManifest();
			MandatoryValidation.CheckEntered(Parent.JK_PrintOptionForColoadsOnManifestInfo);
			if (!Parent.JK_PrintOptionForColoadsOnManifestInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.JK_PrintOptionForColoadsOnManifestInfo, Parent.JK_PrintOptionForColoads_List);
			}
		}

		protected override void CheckJK_PrintOptionForColoadsOnOtherDocs()
		{
			base.CheckJK_PrintOptionForColoadsOnOtherDocs();
			MandatoryValidation.CheckEntered(Parent.JK_PrintOptionForColoadsOnOtherDocsInfo);
			if (!Parent.JK_PrintOptionForColoadsOnOtherDocsInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.JK_PrintOptionForColoadsOnOtherDocsInfo, Parent.JK_PrintOptionForColoads_List);
			}
		}

		#endregion

		#region JK_ConsolMode

		protected override void CheckJK_ConsolMode()
		{
			base.CheckJK_ConsolMode();

			MandatoryValidation.CheckEntered(Parent.JK_ConsolModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_ConsolModeInfo, Parent.JK_ConsolMode_List);

			if (!Parent.JK_ConsolModeInfo.HasErrors())
			{
				foreach (CommonContainer container in Parent.Containers)
				{
					container.Validation.ValidateJC_ContainerMode();
				}
			}

			if (ShouldValidateConsolValuesMatchFirstShipment && Parent.JK_ConsolMode != Parent.Shipments[0].JS_PackingMode)
			{
				Parent.JK_ConsolModeInfo.AddWarning(Res.GetString("409f5942-2dc0-421e-8479-9f0bd423859e", "Container Mode does not match Shipment."));
			}
		}

		#endregion

		#region JK_OA_SendingForwarderAddress

		protected override void CheckJK_OA_SendingForwarderAddress()
		{
			base.CheckJK_OA_SendingForwarderAddress();

			if (!Parent.JK_OA_SendingForwarderAddress.IsEmpty && !Parent.JK_OA_ReceivingForwarderAddress.IsEmpty && Parent.ReceivingForwarderPK.Equals(Parent.SendingForwarderPK))
			{
				Parent.JK_OA_SendingForwarderAddressInfo.AddError(Res.GetString("86d31fdf-1850-467e-9420-c48a12148bdc", "The Sending and Receiving Forwarder organizations cannot be the same."));
			}

			if (Parent.JK_OA_SendingForwarderAddressInfo.HasChanges || !Parent.IsInDatabase)
			{
				if (SendingForwarderHasBeenChanged() || !Parent.IsInDatabase)
				{
					string consolRelatedSendingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(Parent.Shipments.Cast<CommonShipment>(), new[] { Parent });
					if (!string.IsNullOrEmpty(consolRelatedSendingAgentsWarning))
					{
						Parent.JK_OA_SendingForwarderAddressInfo.AddWarning(consolRelatedSendingAgentsWarning);
					}
				}
			}

			if (!FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.Value
				&& Parent.JK_SendingForwarderHandlingType.IsEmpty)
			{
				CheckForwarderIsAppointedAgentForRelatedPort(Parent.JK_OA_SendingForwarderAddressInfo, Parent.SendingForwarder, Parent.JK_RL_NKLoadPortInfo, true);
			}
		}

		public bool SendingForwarderHasBeenChanged()
		{
			return ReceivingSendingForwarderHasBeenChanged(Parent.JK_OA_SendingForwarderAddressInfo, Parent.SendingForwarderPK);
		}

		#endregion

		#region JK_OA_ReceivingForwarderAddress

		protected override void CheckJK_OA_ReceivingForwarderAddress()
		{
			base.CheckJK_OA_ReceivingForwarderAddress();
			if (!Parent.JK_OA_ReceivingForwarderAddress.IsEmpty && !Parent.JK_OA_SendingForwarderAddress.IsEmpty && Parent.ReceivingForwarderPK.Equals(Parent.SendingForwarderPK))
			{
				Parent.JK_OA_ReceivingForwarderAddressInfo.AddError(Res.GetString("86d31fdf-1850-467e-9420-c48a12148bdc", "The Sending and Receiving Forwarder organizations cannot be the same."));
			}

			if (Parent.JK_OA_ReceivingForwarderAddressInfo.HasChanges || !Parent.IsInDatabase)
			{
				if (ReceivingForwarderHasBeenChanged() || !Parent.IsInDatabase)
				{
					var consolRelatedReceivingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(Parent.Shipments.Cast<CommonShipment>(), new[] { Parent });
					if (!string.IsNullOrEmpty(consolRelatedReceivingAgentsWarning))
					{
						Parent.JK_OA_ReceivingForwarderAddressInfo.AddWarning(consolRelatedReceivingAgentsWarning);
					}
				}
			}

			if (!FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.Value
				&& Parent.JK_ReceivingForwarderHandlingType.IsEmpty)
			{
				CheckForwarderIsAppointedAgentForRelatedPort(Parent.JK_OA_ReceivingForwarderAddressInfo, Parent.ReceivingForwarder, Parent.JK_RL_NKDischargePortInfo, false);
			}
		}

		public bool ReceivingForwarderHasBeenChanged()
		{
			return ReceivingSendingForwarderHasBeenChanged(Parent.JK_OA_ReceivingForwarderAddressInfo, Parent.ReceivingForwarderPK);
		}

		bool ReceivingSendingForwarderHasBeenChanged(ZPropertyInfo forwarderAddressInfo, ZGuid forwarderPK)
		{
			if (!Parent.IsInDatabase && !forwarderPK.IsEmpty)
			{
				return true;
			}

			var originalAddressPK = (ZGuid)forwarderAddressInfo.OriginalValue;
			var originalAddress = Parent.Factory.Load<OrgAddress>(originalAddressPK);
			var originalOrgHeader = originalAddress != null ? originalAddress.OA_OH : ZGuid.Empty;

			return originalOrgHeader != forwarderPK;
		}

		#endregion

		#region Check Has Relevant Appointed Agent

		void CheckForwarderIsAppointedAgentForRelatedPort(ZPropertyInfo forwardingAgentInfo, OrgHeader relatedOrg, ZPropertyInfo portInfo, bool isDirectionExport)
		{
			if (relatedOrg != null)
			{
				var port = portInfo.Value.ToString();
				var expectedDirection = new Dictionary<ZString, bool>();
				expectedDirection[AgentDirectionList.Codes.Both] = true;
				expectedDirection[AgentDirectionList.Codes.Export] = isDirectionExport;
				expectedDirection[AgentDirectionList.Codes.Import] = !isDirectionExport;

				var relatedAgentPorts = relatedOrg.AppointedAgentPorts.Concat(relatedOrg.AppointedGatewayAgentPorts);
				var relatedOrgSuitableForHandlingPort = from appointedAgentPort in relatedAgentPorts.Cast<OrgAppointedAgentPorts>()
														where port.StartsWith(appointedAgentPort.O5_PortOrCountry) && expectedDirection[appointedAgentPort.O5_AgentDirection] && IsHandlingOrGatewayAgent(appointedAgentPort)
														select appointedAgentPort;

				if (!portInfo.HasErrors() && !relatedOrgSuitableForHandlingPort.Any())
				{
					var appointedAgentError = Res.GetString("429a77c4-fb3f-4a61-b6e6-ff8b5b911217", "This organization is not valid for handling {0} consols in {1}. Please choose a new organization or use F3 to amend this organization’s Freight Handling Details on the Fwd/Agent tab.", Parent.JK_TransportMode, port);
					forwardingAgentInfo.AddError(appointedAgentError);
				}
			}
		}

		bool IsHandlingOrGatewayAgent(OrgAppointedAgentPorts appointedAgentPort)
		{
			switch (Parent.JK_TransportMode)
			{
				case (Constants.TransportModes.Sea):
					return appointedAgentPort.O5_IsHandlesSeaAgent || appointedAgentPort.O5_IsGatewaySeaAgent;
				case (Constants.TransportModes.Air):
					return appointedAgentPort.O5_IsHandlesAirAgent || appointedAgentPort.O5_IsGatewayAirAgent;
				case (Constants.TransportModes.Road):
					return appointedAgentPort.O5_IsHandlesRoadAgent || appointedAgentPort.O5_IsGatewayRoadAgent;
				case (Constants.TransportModes.Rail):
					return appointedAgentPort.O5_IsHandlesRailAgent || appointedAgentPort.O5_IsGatewayRailAgent;
				default:
					return false;
			}
		}

		#endregion

		#region JK_RL_NKLastForeignPort

		protected override void CheckJK_RL_NKLastForeignPort()
		{
			base.CheckJK_RL_NKLastForeignPort();

			ListValidation.ErrorIfInvalidCode(Parent.JK_RL_NKLastForeignPortInfo, Parent.RefUNLOCO_List);

			if (!Parent.JK_RL_NKLastForeignPortInfo.HasErrors() && !Parent.JK_RL_NKLastForeignPortInfo.ReadOnly)
			{
				var isBasedOnCountryUS = Constants.CountryCodes.IsUsaOrTerritory(Parent.JK_RL_NKDischargePort.SubstringSafe(0, 2));
				if (!isBasedOnCountryUS)
				{
					foreach (Transport transport in Parent.Transports)
					{
						if (!transport.IsDomestic && transport.JW_RL_NKDiscPort.SubstringSafe(0, 2) == Constants.CountryCodes.UnitedStates)
						{
							isBasedOnCountryUS = true;
							break;
						}
					}
				}

				var lastForeignPortWillBeDefaultedOnSaving = Parent.SeaImportLinkedTransportLegHasLastForeignPort
					&& !Parent.JK_RL_NKLastForeignPortInfo.ReadOnly && Parent.JK_RL_NKLastForeignPort.IsEmpty
					&& !Parent.JK_DateLastForeignPortInfo.ReadOnly && Parent.JK_DateLastForeignPort.IsEmpty;

				if (isBasedOnCountryUS && !lastForeignPortWillBeDefaultedOnSaving)
				{
					MandatoryValidation.CheckEntered(Parent.JK_RL_NKLastForeignPortInfo);
				}
			}
		}

		#endregion

		#region JK_RS_NKGatewayServiceLevel

		protected override void CheckJK_RS_NKGatewayServiceLevel()
		{
			base.CheckJK_RS_NKGatewayServiceLevel();

			ListValidation.ErrorIfInvalidCode(Parent.JK_RS_NKGatewayServiceLevelInfo, Parent.Lookups.GatewayServiceLevels);

			foreach (CommonShipment shipment in Parent.Shipments)
			{
				var errorMessage = Parent.ExclusiveGatewayServiceChecker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(
					AttachDetachAction.Attach,
					shipment,
					Parent);

				if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					Parent.JK_RS_NKGatewayServiceLevelInfo.AddError(errorMessage);
					break;
				}
			}
		}

		#endregion

		#region JK_ConsolChargeable

		protected override void CheckJK_ConsolChargeable()
		{
			base.CheckJK_ConsolChargeable();

			if (!Parent.JK_ConsolChargeableInfo.HasErrors())
			{
				if (Parent.JK_ConsolChargeable < 0)
				{
					Parent.JK_ConsolChargeableInfo.AddError(Res.GetString("38f17b9c-df01-4b0c-8e12-b78936a135d2", "Consol Chargeable quantity should not be less than zero."));
				}
			}
		}

		#endregion

		#region JK_CorrectedConsolWeight

		protected override void CheckJK_CorrectedConsolWeightIsValidZDecimal()
		{
			if (Parent.JK_OverrideConsolChargeable)
			{
				base.CheckJK_CorrectedConsolWeightIsValidZDecimal();
			}
		}

		#endregion

		#region JK_CorrectedConsolWeightUnit

		protected override void CheckJK_CorrectedConsolWeightUnit()
		{
			base.CheckJK_CorrectedConsolWeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.JK_CorrectedConsolWeightUnitInfo);
		}

		#endregion

		#region JK_CorrectedConsolVolumeUnit

		protected override void CheckJK_CorrectedConsolVolumeUnit()
		{
			base.CheckJK_CorrectedConsolVolumeUnit();
			ListValidation.ErrorIfInvalidCode(Parent.JK_CorrectedConsolVolumeUnitInfo);
		}

		#endregion

		#region JK_OA_ShippingLineAddress

		protected override void CheckJK_OA_ShippingLineAddress()
		{
			base.CheckJK_OA_ShippingLineAddress();

			if (!Parent.JK_OA_ShippingLineAddress.IsEmpty && !Parent.Transports.Cast<Transport>().Any(transport => transport.CarrierPK == Parent.ShippingLinePK))
			{
				Parent.JK_OA_ShippingLineAddressInfo.AddWarning(Res.GetString("dbdc396b-9084-4a0d-b197-e472df24591e", "The carrier does not match a carrier on any of the routing legs."));
			}
		}

		#endregion

		#region ValidateJK_ScreeningStatus

		protected override void CheckJK_ScreeningStatus()
		{
			base.CheckJK_ScreeningStatus();
			MandatoryValidation.CheckEntered(Parent.JK_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}

		#endregion

		#region Calculated

		#region IsDomesticFreight

		public void ValidateIsDomesticFreight()
		{
			ValidateCalculatedProperty(Parent.IsDomesticFreightInfo);
		}

		protected void CheckIsDomesticFreight()
		{
			if (!Parent.JK_RL_NKLoadPort.IsEmpty && !Parent.JK_RL_NKDischargePort.IsEmpty)
			{
				if (Parent.IsDomesticFreight && !Parent.IsDomestic())
				{
					Parent.IsDomesticFreightInfo.AddError(Res.GetString("23f4f027-8da7-4299-b07f-4e366930baec", "You have marked this consol as Domestic Freight, but the load and discharge ports are not in the same country/region."));
				}
			}
		}

		#endregion

		#region JK_CRN

		public void ValidateJK_CRN()
		{
			ValidateCalculatedProperty(Parent.JK_CRNInfo);
		}

		protected virtual void CheckJK_CRN()
		{
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsDomesticFreight();
			ValidateJK_CRN();
		}

		#endregion

		#region Implementation

		public new CommonConsol Parent
		{
			get { return (CommonConsol)base.Parent; }
		}

		#endregion
	}
}

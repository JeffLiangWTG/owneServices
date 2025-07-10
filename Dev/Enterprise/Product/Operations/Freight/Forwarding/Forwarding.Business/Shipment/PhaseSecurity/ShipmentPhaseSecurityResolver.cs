using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentPhaseSecurityResolver : PhaseSecurityResolver
	{
		public ShipmentPhaseSecurityResolver(ForwardingShipment master)
			: base(master)
		{
			Master = master;
		}

		readonly ForwardingShipment Master;

		#region Phase Code

		protected override ZString PhaseCode
		{
			get { return !Master.IsDeleted ? Master.JS_Phase : ZString.Empty; }
		}

		#endregion

		#region Get Phase Security

		protected override IPhaseSecurity GetPhaseSecurity()
		{
			return ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.Value;
		}

		protected override bool IsPhaseSecurityApplicableCore
		{
			get
			{
				if (Master is ForwardingModuleShipment)
				{
					return false;
				}

				return base.IsPhaseSecurityApplicableCore;
			}
		}

		#endregion

		#region GetMandatoryValidationError

		protected override ZString GetMandatoryValidationError(ZPropertyInfo propertyInfo)
		{
			return Res.GetString("f86cf2a5-7a0f-48c1-8277-376558cfc444", "{0} has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Shipment -> Phases for details.", propertyInfo.HumanReadableName);
		}

		#endregion

		#region Resolve UNLOCO

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		protected override IEnumerable<ZString> ResolveUNLOCOs(ZString locationCode)
		{
			List<ZString> result = new List<ZString>();

			if (locationCode == PhaseConstants.Locations.Shipment.OriginPort)
			{
				result.Add(Master.JS_RL_NKOrigin);
			}
			else if (locationCode == PhaseConstants.Locations.Shipment.OriginCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JS_RL_NKOrigin));
			}
			else if (locationCode == PhaseConstants.Locations.Shipment.DestinationPort)
			{
				result.Add(Master.JS_RL_NKDestination);
			}
			else if (locationCode == PhaseConstants.Locations.Shipment.DestinationCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JS_RL_NKDestination));
			}
			else if (locationCode == PhaseConstants.Locations.Shipment.TransitCountry)
			{
				ZString originCountry = GetCountryFromUNLOCO(Master.JS_RL_NKOrigin);
				ZString destinationCountry = GetCountryFromUNLOCO(Master.JS_RL_NKDestination);
				Transport[] transportsIncludingRelated = GetTransportsWithoutTouchingShipmentConsolCollection();

				result.AddRange(GetTransitCountries(originCountry, destinationCountry, transportsIncludingRelated));
			}
			else
			{
				ForwardingConsol firstConsol = null;
				ForwardingConsol lastConsol = null;

				ForwardingConsol[] allConsols = GetConsolsWithoutTouchingShipmentConsolCollection();
				MovementLegComparer.SortMovementLegsByPorts(allConsols);
				if (allConsols.Length >= 1)
				{
					firstConsol = allConsols.First();
					lastConsol = allConsols.Last();
				}

				if (locationCode == PhaseConstants.Locations.Shipment.LoadPort && firstConsol != null)
				{
					result.Add(firstConsol.JK_RL_NKLoadPort);
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort && firstConsol != null)
				{
					result.Add(firstConsol.JK_RL_NKLoadPort);

					var sendingAgentPortCode = GetAgentUNLOCOIfApplicable(firstConsol.SendingForwarderAddress);
					if (!sendingAgentPortCode.IsEmpty)
					{
						result.Add(sendingAgentPortCode);
					}
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.LoadCountry && firstConsol != null)
				{
					result.Add(GetCountryFromUNLOCO(firstConsol.JK_RL_NKLoadPort));
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry && firstConsol != null)
				{
					result.Add(GetCountryFromUNLOCO(firstConsol.JK_RL_NKLoadPort));

					var sendingAgentCountryCode = GetAgentCountryIfApplicable(firstConsol.SendingForwarderAddress);
					if (!sendingAgentCountryCode.IsEmpty)
					{
						result.Add(sendingAgentCountryCode);
					}
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.DischargePort && lastConsol != null)
				{
					result.Add(lastConsol.JK_RL_NKDischargePort);
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort && lastConsol != null)
				{
					result.Add(lastConsol.JK_RL_NKDischargePort);

					var receivingAgentPortCode = GetAgentUNLOCOIfApplicable(lastConsol.ReceivingForwarderAddress);
					if (!receivingAgentPortCode.IsEmpty)
					{
						result.Add(receivingAgentPortCode);
					}
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.DischargeCountry && lastConsol != null)
				{
					result.Add(GetCountryFromUNLOCO(lastConsol.JK_RL_NKDischargePort));
				}
				else if (locationCode == PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry && lastConsol != null)
				{
					result.Add(GetCountryFromUNLOCO(lastConsol.JK_RL_NKDischargePort));

					var receivingAgentCountryCode = GetAgentCountryIfApplicable(lastConsol.ReceivingForwarderAddress);
					if (!receivingAgentCountryCode.IsEmpty)
					{
						result.Add(receivingAgentCountryCode);
					}
				}
			}

			return result.Distinct();
		}

		ForwardingConsol[] GetConsolsWithoutTouchingShipmentConsolCollection()
		{
			// NOTE: Consol collection on the shipment must NOT be accessed here OR stack overflow exception will happen
			// Explanation: ManyToManyBizoCollection on it's creation will call RegisterEditableChildObject for PivotCollection
			// Explanation: RegisterEditableChildObject will try to resolve phase security => stack overflow

			var consolShipmentPivotQuery = new ZQuery(JobConShipLinkSchema.JN_JS, Master.PK);
			var consolShipmentPivots = Master.Factory.Load<JobConShipLink>(consolShipmentPivotQuery);

			var consolsQuery = new ZQuery(JobConsolSchema.PK, consolShipmentPivots.Select(pivot => pivot.JN_JK));
			var consols = Master.Factory.Load<ForwardingConsol>(consolsQuery);

			return consols;
		}

		Transport[] GetTransportsWithoutTouchingShipmentConsolCollection()
		{
			ForwardingConsol[] consols = GetConsolsWithoutTouchingShipmentConsolCollection();

			List<ZGuid> transportParents = new List<ZGuid>();
			transportParents.Add(Master.PK);
			transportParents.AddRange(consols.Select(consol => consol.PK));

			Transport[] transportsIncludingRelated = Master.Factory.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, transportParents));
			// TODO: Every instance of a transport should have its ParentType set per comment in Transport.get_Parent.
			// Some other operational actions may refer to its property and ParentType check will throw exception with reason "ParentType not set".

			return transportsIncludingRelated;
		}

		#endregion
	}
}

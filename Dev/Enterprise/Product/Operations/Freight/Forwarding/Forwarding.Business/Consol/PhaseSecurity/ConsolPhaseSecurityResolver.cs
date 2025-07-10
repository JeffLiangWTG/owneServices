using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolPhaseSecurityResolver : PhaseSecurityResolver
	{
		public ConsolPhaseSecurityResolver(ForwardingConsol master)
			: base(master)
		{
			Master = master;
		}

		readonly ForwardingConsol Master;

		#region Phase Code

		protected override ZString PhaseCode
		{
			get { return !Master.IsDeleted ? Master.JK_Phase : ZString.Empty; }
		}

		#endregion

		#region Get Phase Security

		protected override IPhaseSecurity GetPhaseSecurity()
		{
			return ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.Value;
		}

		protected override bool IsPhaseSecurityApplicableCore
		{
			get
			{
				if (Master is ForwardingModuleConsol)
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
			return Res.GetString("0d9a5d1c-e153-4297-b7f5-d8cab556cebd", "{0} has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Consolidations -> Phases for details.", propertyInfo.HumanReadableName);
		}

		#endregion

		#region Resolve UNLOCO

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		protected override IEnumerable<ZString> ResolveUNLOCOs(ZString locationCode)
		{
			List<ZString> result = new List<ZString>();

			if (locationCode == PhaseConstants.Locations.Consol.FirstLoadPort)
			{
				result.Add(Master.JK_RL_NKLoadPort);
			}
			else if (locationCode == PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort)
			{
				result.Add(Master.JK_RL_NKLoadPort);

				var sendingAgentPortCode = GetAgentUNLOCOIfApplicable(Master.SendingForwarderAddress);
				if (!sendingAgentPortCode.IsEmpty)
				{
					result.Add(sendingAgentPortCode);
				}
			}
			else if (locationCode == PhaseConstants.Locations.Consol.FirstLoadCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JK_RL_NKLoadPort));
			}
			else if (locationCode == PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JK_RL_NKLoadPort));

				var sendingAgentCountryCode = GetAgentCountryIfApplicable(Master.SendingForwarderAddress);
				if (!sendingAgentCountryCode.IsEmpty)
				{
					result.Add(sendingAgentCountryCode);
				}
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LastDischargePort)
			{
				result.Add(Master.JK_RL_NKDischargePort);
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort)
			{
				result.Add(Master.JK_RL_NKDischargePort);

				var receivingAgentCode = GetAgentUNLOCOIfApplicable(Master.ReceivingForwarderAddress);
				if (!receivingAgentCode.IsEmpty)
				{
					result.Add(receivingAgentCode);
				}
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LastDischargeCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JK_RL_NKDischargePort));
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry)
			{
				result.Add(GetCountryFromUNLOCO(Master.JK_RL_NKDischargePort));

				var receivingAgentCountryCode = GetAgentCountryIfApplicable(Master.ReceivingForwarderAddress);
				if (!receivingAgentCountryCode.IsEmpty)
				{
					result.Add(receivingAgentCountryCode);
				}
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LoadPort && Master.Transports.MostInterestingTransport != null)
			{
				result.Add(Master.Transports.MostInterestingTransport.JW_RL_NKLoadPort);
			}
			else if (locationCode == PhaseConstants.Locations.Consol.LoadCountry && Master.Transports.MostInterestingTransport != null)
			{
				result.Add(GetCountryFromUNLOCO(Master.Transports.MostInterestingTransport.JW_RL_NKLoadPort));
			}
			else if (locationCode == PhaseConstants.Locations.Consol.DischargePort && Master.Transports.MostInterestingTransport != null)
			{
				result.Add(Master.Transports.MostInterestingTransport.JW_RL_NKDiscPort);
			}
			else if (locationCode == PhaseConstants.Locations.Consol.DischargeCountry && Master.Transports.MostInterestingTransport != null)
			{
				result.Add(GetCountryFromUNLOCO(Master.Transports.MostInterestingTransport.JW_RL_NKDiscPort));
			}
			else if (locationCode == PhaseConstants.Locations.Consol.TransitCountry)
			{
				ZString loadCountry = GetCountryFromUNLOCO(Master.JK_RL_NKLoadPort);
				ZString dischargeCountry = GetCountryFromUNLOCO(Master.JK_RL_NKDischargePort);
				result.AddRange(GetTransitCountries(loadCountry, dischargeCountry, Master.Transports.Cast<Transport>()));
			}

			return result.Distinct();
		}

		#endregion
	}
}

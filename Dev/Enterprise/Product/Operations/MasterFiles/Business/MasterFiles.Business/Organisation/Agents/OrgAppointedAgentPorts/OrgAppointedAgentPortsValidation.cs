using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAppointedAgentPortsValidation : AutoOrgAppointedAgentPortsValidation
	{
		public OrgAppointedAgentPortsValidation(AutoOrgAppointedAgentPorts parent)
			: base(parent)
		{
		}

		protected override void CheckO5_PortOrCountry()
		{
			base.CheckO5_PortOrCountry();

			MandatoryValidation.CheckEntered(Parent.O5_PortOrCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O5_PortOrCountryInfo);
		}

		#region Implementation

		public new OrgAppointedAgentPorts Parent
		{
			get { return (OrgAppointedAgentPorts)base.Parent; }
		}

		protected override void CheckO5_OA_AgentOfficeAddress()
		{
			base.CheckO5_OA_AgentOfficeAddress();

			if (Parent.AgentOfficeAddress != null && !Parent.AgentOfficeAddress.OA_IsActive)
			{
				Parent.O5_OA_AgentOfficeAddressInfo.AddError(Res.GetString("CB21D73D-EF1D-4689-8B0E-FD17129B83F1", "Only active address can be set as Agent Office Address"));
			}
		}

		#endregion

		#region Duplicate Port Validation

		protected ZBool IsDuplicatePort(OrgAppointedAgentPorts[] ports)
		{
			if (ports.Length > 1)
			{
				return true;
			}
			else if (ports.Length == 1)
			{
				if (ports[0].O5_AgentDirection != Parent.O5_AgentDirection
					&& ports[0].O5_AgentDirection != AgentDirectionList.Codes.Both
					&& Parent.O5_AgentDirection != AgentDirectionList.Codes.Both)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}

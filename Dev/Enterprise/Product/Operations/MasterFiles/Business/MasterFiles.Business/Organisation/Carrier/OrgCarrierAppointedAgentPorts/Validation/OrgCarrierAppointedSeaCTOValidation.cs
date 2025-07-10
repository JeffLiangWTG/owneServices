using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedSeaCTOValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedSeaCTOValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		#region Validation overrides

		protected override void CheckO5_TerminalType()
		{
			base.CheckO5_TerminalType();

			MandatoryValidation.CheckEntered(Parent.O5_TerminalTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O5_TerminalTypeInfo);
			ValidateAll();
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsSeaCTO)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("0fbb1abf-72e5-4af0-903e-26d95d5055d5", "Selected Organization should be Sea CTO/Stevedore"));
			}
		}

		protected override bool IsDirectionMandatory => true;

		#endregion

		protected override void CheckForDuplicatePortsAndAddresses()
		{
			var query = GetDuplicateCarrierPortsQuery();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_TerminalType, Parent.O5_TerminalType);

			var ports = Parent.Factory.Load<OrgCarrierAppointedAgentPorts>(query);

			if (IsDuplicatePort(ports))
			{
				var message = Res.GetString("e63c5742-d415-4ff8-b1e7-5f8ddf5f4e1b", "{0} already has a Sea CTO/Stevedore with this address and {1} terminal type.", Parent.O5_PortOrCountry, Parent.O5_TerminalType);
				Parent.O5_OA_AgentOfficeAddressInfo.AddError(message);
			}
		}

		protected override void CheckForMultipleAddressesForSameCriteria()
		{
			var query = GetMultipleAddressesForSameCriteriaQuery();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_TerminalType, Parent.O5_TerminalType);

			if (Parent.Factory.Exists(typeof(OrgCarrierAppointedAgentPorts), query))
			{
				var message = Res.GetString("1536FD4E-F49C-46BE-BEC2-F172EAD19EA9", "There is more than one record with port {0}, direction {1} and terminal type {2}.", Parent.O5_PortOrCountry, Parent.O5_AgentDirection, Parent.O5_TerminalType);
				Parent.O5_AgentDirectionInfo.AddWarning(message);
			}
		}
	}
}

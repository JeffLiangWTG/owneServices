using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgentPortsValidation : OrgAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedAgentPortsValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		public new OrgCarrierAppointedAgentPorts Parent
		{
			get { return (OrgCarrierAppointedAgentPorts)base.Parent; }
		}

		#region OrganisationPK

		public void ValidateOrganisationPK()
		{
			ValidateCalculatedProperty(Parent.OrganisationPKInfo);
		}

		protected virtual void CheckOrganisationPK()
		{
			MandatoryValidation.CheckEntered(Parent.OrganisationPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.OrganisationPKInfo);
		}

		#endregion

		#region Validation overrides

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOrganisationPK();
		}

		protected override void CheckO5_OA_AgentOfficeAddress()
		{
			base.CheckO5_OA_AgentOfficeAddress();
			CheckForDuplicatePortsAndAddresses();
		}

		#endregion

		#region Duplicate Port Entries

		protected virtual string DuplicateRelatedPartyErrorMessage => Res.GetString("d86e422d-aafa-4e3f-9ffd-e2cbd2ca0ff7", "{0} already has a related party for this address.", Parent.O5_PortOrCountry);

		protected virtual void CheckForDuplicatePortsAndAddresses()
		{
			var ports = Parent.Factory.Load<OrgCarrierAppointedAgentPorts>(GetDuplicateCarrierPortsQuery());

			if (IsDuplicatePort(ports))
			{
				Parent.O5_OA_AgentOfficeAddressInfo.AddError(DuplicateRelatedPartyErrorMessage);
			}
		}

		protected ZQuery GetDuplicateCarrierPortsQuery()
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OH, Parent.O5_OH);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, Parent.O5_PortOrCountry);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress, Parent.O5_OA_AgentOfficeAddress);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, Parent.O5_SeaAirCarrierOrForwarderType);

			return filter;
		}

		#endregion

		#region Direction

		protected virtual bool IsDirectionMandatory => false;

		protected override void CheckO5_AgentDirection()
		{
			base.CheckO5_AgentDirection();

			if (IsDirectionMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.O5_AgentDirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.O5_AgentDirectionInfo);
			}

			CheckForMultipleAddressesForSameCriteria();
		}

		#endregion

		#region Multiple Entries with Same Port

		protected virtual void CheckForMultipleAddressesForSameCriteria()
		{
			if (Parent.Factory.Exists(typeof(OrgCarrierAppointedAgentPorts), GetMultipleAddressesForSameCriteriaQuery()))
			{
				var message = Res.GetString("7A06F54F-FBAE-428C-8151-04F65020BB41", "There is more than one record with port {0} and direction {1}.", Parent.O5_PortOrCountry, Parent.O5_AgentDirection);
				Parent.O5_AgentDirectionInfo.AddWarning(message);
			}
		}

		protected ZQuery GetMultipleAddressesForSameCriteriaQuery()
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OH, Parent.O5_OH);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, Parent.O5_PortOrCountry);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, Parent.O5_SeaAirCarrierOrForwarderType);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_AgentDirection, Parent.O5_AgentDirection);

			return filter;
		}

		#endregion
	}
}

using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationPartyConfigValidation : AutoEDICommunicationPartyConfigValidation
	{
		public EDICommunicationPartyConfigValidation(AutoEDICommunicationPartyConfig parent)
			: base(parent)
		{
		}

		public new EDICommunicationPartyConfig Parent => (EDICommunicationPartyConfig)base.Parent;

		protected override void CheckECC_Endpoint()
		{
			if (Parent.Party.ECP_IsActive
				&& Parent.Party.OutboundConfig.ECC_IsActive
				&& Parent.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound)
			{
				if (!UrlValidation.IsValidUrl(Parent.ECC_Endpoint) && !Parent.ECC_EndpointInfo.HasErrors())
				{
					if (Parent.Auth.ECA_Certificate.IsEmpty && !Parent.Auth.ECA_RenewalEncodedPrivateKey.IsEmpty)
					{
						return;
					}

					Parent.ECC_EndpointInfo.AddError(Res.GetString("02093877-0be0-45d7-8262-d169fd19c0dd", "Not a valid URI"));
				}
			}
		}

		protected override void CheckECC_GB_Branch()
		{
			var applicationDescriptor = ObjectFactory.Get<IEDIClientApplicationDescriptors>().GetValue(Parent.Party.ECP_ApplicationCode);
			var requiresBranch = applicationDescriptor != null && applicationDescriptor.AccessTypes.Contains(AccessRequirement.RequiresBranch);
			if (Parent.Party.ECP_IsActive
				&& Parent.Party.InboundConfig.ECC_IsActive
				&& Parent.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound
				&& Parent.ECC_GB_Branch.IsEmpty
				&& requiresBranch
				)
			{
				Parent.ECC_GB_BranchInfo.AddError(Res.GetString("b058374b-0ec3-4fae-848e-aaba08378295", "'Please enter a value.'"));
			}
		}

		protected override void CheckECC_GE_Department()
		{
			var applicationDescriptor = ObjectFactory.Get<IEDIClientApplicationDescriptors>().GetValue(Parent.Party.ECP_ApplicationCode);
			var requiresDepartment = applicationDescriptor != null && applicationDescriptor.AccessTypes.Contains(AccessRequirement.RequiresDepartment);
			if (Parent.Party.ECP_IsActive
				&& Parent.Party.InboundConfig.ECC_IsActive
				&& Parent.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound
				&& Parent.ECC_GE_Department.IsEmpty
				&& requiresDepartment
				)
			{
				Parent.ECC_GE_DepartmentInfo.AddError(Res.GetString("46100e73-cb1f-417d-85dd-109212517b9d", "'Please enter a value.'"));
			}
		}
	}
}

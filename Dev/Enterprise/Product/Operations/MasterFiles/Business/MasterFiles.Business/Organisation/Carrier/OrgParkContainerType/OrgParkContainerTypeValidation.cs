using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgParkContainerTypeValidation : AutoOrgParkContainerTypeValidation
	{
		public OrgParkContainerTypeValidation(AutoOrgParkContainerType parent) : base(parent)
		{
		}

		new OrgParkContainerType Parent
		{
			get { return (OrgParkContainerType)base.Parent; }
		}

		#region PT_ContainerStorageClass

		protected override void CheckPT_ContainerStorageClass()
		{
			base.CheckPT_ContainerStorageClass();
			MandatoryValidation.CheckEntered(Parent.PT_ContainerStorageClassInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PT_ContainerStorageClassInfo);
			CheckDuplicateContainerClass();
		}

		void CheckDuplicateContainerClass()
		{
			var carrierAppointedAgentPorts = Parent.AppointedAgentPorts as OrgCarrierAppointedAgentPorts;
			if (carrierAppointedAgentPorts != null)
			{
				if (carrierAppointedAgentPorts.ContainerTypes.Any(x => x.PT_ContainerStorageClass == Parent.PT_ContainerStorageClass && x.PK != Parent.PK))
				{
					Parent.PT_ContainerStorageClassInfo.AddError(Res.GetString("c29bc5ee-f695-40be-a141-b8312ab8231c", "Container class should be unique."));
				}
			}
		}

		#endregion

		protected override void CheckPT_RX_NKCYWorkOrderApprovalLimitCurrency()
		{
			base.CheckPT_RX_NKCYWorkOrderApprovalLimitCurrency();

			if (Parent.PT_CYWorkOrderApprovalLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);
			}
		}

		protected override void CheckPT_CYWorkOrderApprovalNumber()
		{
			base.CheckPT_CYWorkOrderApprovalNumber();

			if (Parent.PT_CYWorkOrderApprovalLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PT_CYWorkOrderApprovalNumberInfo);
			}
		}

		protected override void CheckPT_OC_CYWorkOrderApprovedBy()
		{
			base.CheckPT_OC_CYWorkOrderApprovedBy();

			if (Parent.PT_CYWorkOrderApprovalLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PT_OC_CYWorkOrderApprovedByInfo);
				ValidateContactIsValid();
			}
		}

		void ValidateContactIsValid()
		{
			if (!Parent.PT_OC_CYWorkOrderApprovedBy.IsEmpty)
			{
				OrgHeader orgCarrier = Parent.AppointedAgentPorts?.Header;
				if (orgCarrier != null)
				{
					OrgContact orgContact = (OrgContact)orgCarrier.Contacts.FindByPK(Parent.PT_OC_CYWorkOrderApprovedBy);
					if (orgContact == null || !orgContact.OC_IsActive)
					{
						Parent.PT_OC_CYWorkOrderApprovedByInfo.AddError(Res.GetString("4cd4703f-afb9-4b23-b77a-64dd0246eb1d", "Select an active contact belonging to the organization."));
					}
				}
			}
		}
	}
}

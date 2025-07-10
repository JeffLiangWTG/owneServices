using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAllocationValidation : OrgContactAttributeValidation
	{
		public OrgContactAllocationValidation(AutoOrgContactAttribute parent)
			: base(parent)
		{
		}

		protected override void CheckPC_Type()
		{
			ListValidation.ErrorIfInvalidCode(Parent.PC_TypeInfo);
			if (!Parent.PC_TypeInfo.HasNotifications())
			{
				var contactAllocationQuery = new ZQuery(OrgContactAttributeSchema.PC_OC, Parent.PC_OC);
				contactAllocationQuery.AddToFilter(OrgContactAttributeSchema.PC_IsAllocatedContact, true);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PC_TypeInfo, Parent.Factory.Load<OrgContactAllocation>(contactAllocationQuery), Res.GetString("b12e97c0-4f93-433a-a711-7cb2083cfd4f", "The Allocation has been duplicated and must be unique."));
				CheckIsUniqueAllocationAcrossOrganization(Parent.PC_TypeInfo, Parent.Contact);
			}
		}

		void CheckIsUniqueAllocationAcrossOrganization(ZPropertyInfo fieldInfo, OrgContact validationContact)
		{
			var fieldValue = fieldInfo.Value.ToString();
			if (!OrgConstants.ContactAllocationType.ShouldAllowMultiple(fieldValue))
			{
				var orgHeader = validationContact.ParentOrg;

				foreach (OrgContact contact in orgHeader.Contacts)
				{
					if (contact != validationContact)
					{
						foreach (OrgContactAllocation contactAllocation in contact.Allocations)
						{
							var allocationType = contactAllocation.PC_Type;

							if (allocationType == fieldValue)
							{
								string errorMessage = Res.GetString("13AD084B-8008-4B19-8CD6-78A728451CFA", "{0} contact person is already currently allocated to {1}. You will need to remove that allocation if you wish {2} to be the allocated contact for {0}.", contactAllocation.PC_Type, contact.OC_ContactName, validationContact.OC_ContactName);
								fieldInfo.AddError(string.Format(errorMessage));
								break;
							}
						}
					}
				}
			}
		}
	}
}

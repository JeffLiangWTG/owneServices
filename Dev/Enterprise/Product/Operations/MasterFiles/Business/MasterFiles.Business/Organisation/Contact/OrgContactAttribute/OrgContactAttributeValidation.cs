using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAttributeValidation : AutoOrgContactAttributeValidation
	{
		public OrgContactAttributeValidation(AutoOrgContactAttribute parent) : base(parent)
		{
		}

		#region PC_Type

		protected override void CheckPC_Type()
		{
			base.CheckPC_Type();

			ListValidation.ErrorIfInvalidCode(Parent.PC_TypeInfo);
			if (!Parent.PC_TypeInfo.HasNotifications())
			{
				MandatoryValidation.CheckEntered(Parent.PC_TypeInfo);
				var contactAttributeQuery = new ZQuery(OrgContactAttributeSchema.PC_OC, Parent.PC_OC);
				contactAttributeQuery.AddToFilter(OrgContactAttributeSchema.PC_IsAllocatedContact, false);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PC_TypeInfo, Parent.Factory.Load<OrgContactAttribute>(contactAttributeQuery));
			}
		}

		#endregion

		#region PC_URL

		protected override void CheckPC_URL()
		{
			base.CheckPC_URL();
			if (!Parent.PC_URLInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.PC_URLInfo);
			}
		}

		#endregion
	}
}

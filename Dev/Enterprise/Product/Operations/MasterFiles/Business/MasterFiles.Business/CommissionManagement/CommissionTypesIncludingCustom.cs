using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionTypesIncludingCustom : CommissionTypes
	{
		public CommissionTypesIncludingCustom()
		{
			foreach (ICodeDescriptionBool customType in OrganisationsDataRegistry.Instance.CustomCommissionTypes.Value)
			{
				AddPairIfNotExist(customType.Code, customType.Description);
			}
		}
	}
}


namespace Enterprise.MasterFiles.Business
{
	class PLOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.WarehouseControlledPremisesID:
					return true;
				default:
					return false;
			}
		}
	}
}

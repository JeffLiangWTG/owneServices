using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class CusAuthorisationHeaderValidation : Customs.Business.CusAuthorisationHeaderValidation
	{
		public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent) : base(parent)
		{
		}

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();
			if (Parent.CPH_Type == CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration
				|| Parent.CPH_Type == CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration)
			{
				MandatoryValidation.CheckEntered(Parent.CPH_OH_PermitHolderInfo);
			}
		}

		protected override void CheckCPH_OA_AppliesTo()
		{
			base.CheckCPH_OA_AppliesTo();
			if (Parent.CPH_Type == CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration
				|| Parent.CPH_Type == CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration)
			{
				CheckAuthorizationIsUnique(Parent.CPH_OA_AppliesToInfo);
			}
		}

		void CheckAuthorizationIsUnique(ZPropertyInfo targetPropertyInfo)
		{
			if (!Parent.CPH_OH_PermitHolder.IsEmpty && !Parent.CPH_OA_AppliesTo.IsEmpty)
			{
				var matchingAuthorizations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Parent.Factory, Parent.CPH_RN_NKCountryCode, new[] { Parent.CPH_Type }, ZDate.Today, new[] { Parent.CPH_OH_PermitHolder }, new[] { Parent.CPH_OA_AppliesTo });
				if (matchingAuthorizations.Length > 1)
				{
					targetPropertyInfo.AddError(Res.GetString("5BD81FD3-4C0E-45A8-9989-B3164E10BE1D", "An Authorization of Type {0} already exists for this Company.", Parent.CPH_Type));
				}
			}
		}
	}
}

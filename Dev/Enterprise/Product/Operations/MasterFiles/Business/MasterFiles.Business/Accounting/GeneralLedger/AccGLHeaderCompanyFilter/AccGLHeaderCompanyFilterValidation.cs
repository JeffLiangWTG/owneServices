//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLHeaderCompanyFilterValidation
//
//    This class should be used for overriding validation in AutoAccGLHeaderCompanyFilterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class AccGLHeaderCompanyFilterValidation : AutoAccGLHeaderCompanyFilterValidation
	{
		public AccGLHeaderCompanyFilterValidation(AutoAccGLHeaderCompanyFilter parent) : base(parent)
		{
		}

		protected new AccGLHeaderCompanyFilter Parent
		{
			get { return base.Parent as AccGLHeaderCompanyFilter; }
		}

		protected override void CheckACF_GC_Company()
		{
			var glHeader = Parent.Header;
			if (!glHeader.AG_IsGlobal)
			{
				if (glHeader.CompanyFilters.Count > 0)
				{
					if (glHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.PK != Parent.PK && x.ACF_GC_Company == Parent.ACF_GC_Company))
					{
						Parent.ACF_GC_CompanyInfo.AddError(Res.GetString("0e704c3b-4b8b-4d84-842e-fb3953608adf", "This company filter is already entered"));
					}

					var companyPks = glHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Select(x => x.ACF_GC_Company).ToArray();
					var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, companyPks);
					query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
					var companyPksNotInCompanyFilterList = Parent.Factory.Load<GlbCompany>(query).Select(x => x.PK).ToArray();
					var companyShouldSet = new HashSet<string>();

					var chargeCodes = Parent.GetChargeCodeUseGLAccountWithCompanyNotSet(companyPksNotInCompanyFilterList);
					if (chargeCodes.Any())
					{
						chargeCodes.ForEach(x => companyShouldSet.Add(x.Company.GC_Code));
					}

					var bankAccounts = Parent.GetBankAccountUseGLAccountWithCompanyNotSet(companyPksNotInCompanyFilterList);
					if (bankAccounts.Any())
					{
						bankAccounts.ForEach(x => companyShouldSet.Add(x.Company.GC_Code));
					}

					if (companyShouldSet.Any())
					{
						Parent.ACF_GC_CompanyInfo.AddError(Res.GetString("AABD13F3-5AD2-4F3B-860F-4AF05816A0CF", "This GL Account has been used in Charge Codes configuration and/or Bank Accounts configuration in the following companies: {0}", string.Join(", ", companyShouldSet.OrderBy(x => x))));
					}
				}
				glHeader.Validation.ValidateAG_IsGlobal();
			}
		}
	}
}

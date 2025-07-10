using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class BranchHelper
	{
		public static GlbBranch NewCompanyAndBranchWith(
			this BusinessObjectFactory factory,
			string companyCode = "ZXY",
			string branchCode = "XYZ",
			string countryCode = Core.Constants.CountryCodes.Australia
			)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = countryCode;

			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = branchCode;
			branch.GB_GC = company.PK;

			return branch;
		}

		public static (GlbCompany company, IReadOnlyList<GlbBranch> branches) NewCompanyWithBranches(this BusinessObjectFactory factory,
			string companyCountry,
			string[] branchCountries)
			=> NewCompanyWithBranches(factory,
				companyCode: companyCountry, companyCountry: companyCountry,
				branchCountries.Select((x, i) => (i.ToString("D3"), x)).ToArray());

		public static (GlbCompany company, IReadOnlyList<GlbBranch> branches) NewCompanyWithBranches(this BusinessObjectFactory factory,
			string companyCode, string companyCountry,
			(string branchCode, string branchCountry)[] branchInfos)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = companyCountry;

			var branches = new List<GlbBranch>();
			foreach ((string branchCode, string branchCountry) in branchInfos)
			{
				var branch = company.Branches.AddNew();
				branch.GB_Code = branchCode;
				branch.GB_RL_NKHomePort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, branchCountry)).Code;
				branches.Add(branch);
			}

			return (company, branches);
		}

		public static T WithExternalPassword<T>(
			this GlbBranch branch,
			string userId = "",
			string passwordStatus = "",
			string statusReason = ""
		) where T : GlbExternalPassword
		{
			var extPassword = branch.Factory.NewWithValidTestData<T>();
			extPassword.GP_GB = branch.PK;
			extPassword.GP_UserID = userId;
			extPassword.GP_PasswordStatus = passwordStatus;
			extPassword.GP_StatusReason = statusReason;
			return extPassword;
		}
	}
}

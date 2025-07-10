using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	static class TemplateApplicationUserContextProvider
	{
		internal static IEnumerable<IUserContext> GetTemplateUserContexts(IWorkflowProvider workFlowProvider, BusinessObjectFactory factory)
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentUserLogin = GlbStaff.CurrentUser.GS_LoginName;
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			var currrentDepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();

			var companies = new[] { currentCompanyPK };
			var informationProvider = workFlowProvider.GetWorkflowInformationProvider();

			if (informationProvider != null && informationProvider.Companies != null)
			{
				var providerCompanies = informationProvider.Companies.ToArray();
				if (providerCompanies.Length > 0)
				{
					companies = providerCompanies;
				}
			}

			foreach (var company in factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, companies)))
			{
				var branchPK = company.PK == currentCompanyPK ? currentBranchPK : company.FirstActiveBranch?.PK.ToGuid();
				if (branchPK.HasValue)
				{
					yield return new UserContext(currentUserLogin, branchPK.Value, currrentDepartmentPK);
				}
			}
		}
	}
}

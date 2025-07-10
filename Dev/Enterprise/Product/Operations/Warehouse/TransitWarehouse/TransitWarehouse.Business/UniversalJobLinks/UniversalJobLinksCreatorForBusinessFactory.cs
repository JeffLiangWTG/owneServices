using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	class UniversalJobLinksCreatorForBusinessFactory : IUniversalJobLinkCreator
	{
		public UniversalJobLinksCreatorForBusinessFactory(BusinessObjectFactory businessObjectFactory, BusinessObject parentBO, DataContextType dataContextType)
		{
			BusinessObjectFactory = Argument.NotNull(businessObjectFactory, "businessObjectFactory");
			TargetDataContext = Argument.NotNull(dataContextType, "dataContextType");
			ParentBO = Argument.NotNull(parentBO, "parentBO");
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			if (registrationKey != null)
			{
				EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}"); // programmatic constant
				CompanyCode = Env.CurrentCompany.Code;
				ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}"); // programmatic constant
			}
		}

		readonly BusinessObjectFactory BusinessObjectFactory;
		readonly BusinessObject ParentBO;
		readonly DataContextType TargetDataContext;
		readonly string EnterpriseCode;
		readonly string CompanyCode;
		readonly string ServerCode;

		#region ExistingLinks

		IEnumerable<StmUniversalJobLink> ExistingLinks => BusinessObjectFactory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, ParentBO.PK));

		#endregion

		#region CreateJobLink

		public void CreateUniversalJobLink(BusinessObject targetBO)
		{
			if (IsValidCode(EnterpriseCode)
				&& IsValidCode(ServerCode)
				&& IsValidCode(CompanyCode)
				&& targetBO != null)
			{
				CreateJobLink(targetBO);
			}
		}

		bool IsValidCode(string code) => code.Length == 3;

		public void CreateJobLink(BusinessObject targetBO)
		{
			var job = targetBO as IRelatedJob;
			if (job != null)
			{
				var universalJobLink = ExistingLinks.SingleOrDefault(s => s.UCL_SourceKey == job.JobNumber && s.UCL_SourceType == TargetDataContext.ToString()) ??
				BusinessObjectFactory.New<StmUniversalJobLink>();
				SetupJobLink(universalJobLink, targetBO);
			}
		}

		void SetupJobLink(StmUniversalJobLink universalJobLink, BusinessObject targetBO)
		{
			universalJobLink.UCL_EnterpriseCode = EnterpriseCode;
			universalJobLink.UCL_ServerCode = ServerCode;
			universalJobLink.UCL_CompanyCode = CompanyCode;
			universalJobLink.UCL_ParentID = ParentBO.PK;
			universalJobLink.UCL_ParentTableCode = ParentBO.TablePrefix;
			universalJobLink.UCL_SourceType = TargetDataContext.ToString();
			universalJobLink.UCL_SourceKey = ((IRelatedJob)targetBO).JobNumber;
			universalJobLink.UCL_OH_Owner = ZGuid.Empty;
		}

		#endregion
	}
}

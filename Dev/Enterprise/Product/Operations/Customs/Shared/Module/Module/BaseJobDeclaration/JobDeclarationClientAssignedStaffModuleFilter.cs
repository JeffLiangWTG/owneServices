using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationClientAssignedStaffModuleFilter : OrgClientAssignedStaffModuleFilter
	{
		public JobDeclarationClientAssignedStaffModuleFilter(ZString description)
			: base(description, null, ClientTypes)
		{
		}

		static CodeDescriptionPairList ClientTypes
		{
			get { return new JobDeclarationClientTypesList(); }
		}

		protected override ClientTypesList GetNewClientTypesList()
		{
			return new JobDeclarationClientTypesList();
		}

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var orgFilter = GetClientAssignedStaffFilter();
				return GetClientAssignedStaffQueryForDeclaration(orgFilter);
			}
		}

		ZQuery GetClientAssignedStaffQueryForDeclaration(ZDBOnlySubQuery orgFilter)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			if (ClientType == JobDeclarationClientTypesList.Codes.Importer)
			{
				query.AddSubQuery(JobDeclarationSchema.JE_OH_Importer, orgFilter, JoinCondition.And);
			}
			else if (ClientType == JobDeclarationClientTypesList.Codes.Supplier)
			{
				query.AddSubQuery(JobDeclarationSchema.JE_OH_Supplier, orgFilter, JoinCondition.And);
			}
			else
			{
				query.AddSubQuery(orgFilter, JoinCondition.And);
				query.AddSubQuery(JobDeclarationSchema.JE_JS, orgFilter, JoinCondition.Or);
			}

			return query;
		}

		protected override ZDBOnlySubQuery GetClientAssignedStaffFilter()
		{
			ZDBOnlySubQuery orgQuery;

			if (ClientType == JobDeclarationClientTypesList.Codes.LocalClient)
			{
				var orgAddressQuery = GetOrgAddressQuery();
				orgQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				orgQuery.AddSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressQuery, JoinCondition.And);
			}
			else
			{
				orgQuery = GetClientAssignedStaffSubQuery();
				if (ClientType == JobDeclarationClientTypesList.Codes.Importer)
				{
					AddControllingBranchSubQuery(orgQuery, JobDeclarationSchema.JE_OH_Importer);
				}
				else
				{
					AddControllingBranchSubQuery(orgQuery, JobDeclarationSchema.JE_OH_Supplier);
				}
			}

			return orgQuery;
		}
	}
}

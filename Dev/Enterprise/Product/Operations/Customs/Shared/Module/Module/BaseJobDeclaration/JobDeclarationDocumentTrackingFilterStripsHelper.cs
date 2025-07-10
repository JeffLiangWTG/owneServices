using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Module.DocumentTrackingFilterStripsHelper;

namespace Enterprise.Customs.Module
{
	class JobDeclarationDocumentTrackingFilterStripsHelper : DocumentTrackingFilterStripsHelper
	{
		protected override ModuleFilterSubGroup GetNewSubGroup() => new JobDeclarationDocumentsFilterCommonSubGroup(BusinessObjectType);
	}

	class JobDeclarationDocumentsFilterCommonSubGroup : DocumentsFilterCommonSubGroup
	{
		public JobDeclarationDocumentsFilterCommonSubGroup(Type businessObjectType)
			: base(businessObjectType)
		{ }

		protected override void AddSubQuery(ZDBOnlyQuery query, ZDBOnlySubQuery subQueryForForwardingDocsAndCartage)
		{
			base.AddSubQuery(query, subQueryForForwardingDocsAndCartage);
			query.AddSubQuery(JobDeclarationSchema.JE_JS, subQueryForForwardingDocsAndCartage, JoinCondition.Or);
		}
	}
}

using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgManagementRelatedParty : OrgRelatedParty
	{
		public OrgManagementRelatedParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			((ILightValidationInternals)this).IsValid = true;
		}

		#region Load

		public static ZQuery GetRelatedManagementParentsQuery(OrgHeader organisation)
		{
			var result = new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			result.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, organisation.PK);
			result.AddToFilter(GetRelatedManagementCompanyFilter());

			// It was possible to set a relation to self; exclude them if it's still somehow possible to do so.
			result.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, SQLComparisonOperator.NotEqual, organisation.PK);
			return result;
		}

		public static ZQuery GetRelatedManagementSubsidiariesQuery(OrgHeader organisation)
		{
			var result = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, organisation.PK);
			result.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			result.AddToFilter(GetRelatedManagementCompanyFilter());

			// It was possible to set a relation to self; exclude them if it's still somehow possible to do so.
			result.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, organisation.PK);
			return result;
		}

		static ZQuery GetRelatedManagementCompanyFilter()
		{
			var companyFilter = new ZQuery(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
			companyFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);

			return companyFilter;
		}

		#endregion

		#region Properties

		#region ParentOrganisation

		public OrgHeader ParentOrganisation
		{
			get { return Factory.Load<OrgHeader>(PR_OH_RelatedParty); }
			set { PR_OH_RelatedParty = value.PK; }
		}

		#endregion

		#region SubsidiaryOrganisation

		public OrgHeader SubsidiaryOrganisation
		{
			get { return Factory.Load<OrgHeader>(PR_OH_Parent); }
			set { PR_OH_Parent = value.PK; }
		}

		#endregion

		#endregion
	}
}

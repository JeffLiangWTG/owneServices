//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCompetitorValidation
//
//    This class should be used for overriding validation in AutoOrgCompetitorValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompetitorValidation : AutoOrgCompetitorValidation
	{
		public OrgCompetitorValidation(AutoOrgCompetitor parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCompanyLevel();
		}

		void ValidateIsInCollectionAlready()
		{
			var errorMessage = (NoResString)"There is already a competitor with the same type, organization and company level. You can only specify a single competitor organization for this combination.";
			Parent.RemoveRowError(errorMessage);

			var query = new ZQuery(OrgCompetitorSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(OrgCompetitorSchema.OCP_OH_Parent, Parent.OCP_OH_Parent);
			query.AddToFilter(OrgCompetitorSchema.OCP_Type, Parent.OCP_Type);
			query.AddToFilter(OrgCompetitorSchema.OCP_OH_Competitor, Parent.OCP_OH_Competitor);

			if (Parent.Factory.Load<OrgCompetitor>(query).Any(competitor => competitor.OCP_GC_Company == Parent.OCP_GC_Company))
			{
				Parent.AddRowError(errorMessage);
			}
		}

		public void ValidateCompanyLevel()
		{
			ValidateCalculatedProperty(Competitor.CompanyLevelInfo);
		}

		protected virtual void CheckCompanyLevel()
		{
			ValidateIsInCollectionAlready();
		}

		protected override void CheckOCP_OH_Competitor()
		{
			if (!Parent.OCP_OH_Competitor.IsEmpty && Parent.Lookups.ActiveCompetitorTypes.ContainsCode(Parent.OCP_Type))
			{
				var filter = new ZQuery(OrgHeaderSchema.PK, Parent.OCP_OH_Competitor);
				filter.AddToFilter(Parent.Lookups.Organisations.CompleteFilter);

				if (!Parent.Factory.Exists(typeof(OrgHeader), filter))
				{
					Parent.OCP_OH_CompetitorInfo.AddError(Res.GetString("07C3B71E-3A4D-4D39-A950-F6EBEC77F3DB", "Organization is invalid for the selected Competitor Type."));
				}
			}
			ValidateIsInCollectionAlready();
		}

		protected override void CheckOCP_Type()
		{
			var competitorTypeIsActive = Parent.Lookups.ActiveCompetitorTypes.ContainsCode(Parent.OCP_Type);
			if (competitorTypeIsActive)
			{
				ValidateIsInCollectionAlready();
				return;
			}

			var competitorTypeIsInvalid = !Parent.Lookups.AllCompetitorTypes.ContainsCode(Parent.OCP_Type);
			var message = competitorTypeIsInvalid
				? Res.GetString("07F8544D-AD90-40DB-8208-4B6CCE6236D9", "This competitor type is invalid. Please use a valid competitor type.")
				: Res.GetString("C4B9800E-0981-4574-BC5F-52D4AAB2B61D", "This competitor type is disabled. Please use a valid competitor type.");

			if (!Parent.IsInDatabase || Parent.HasChanges)
			{
				Parent.OCP_TypeInfo.AddError(message);
			}
			else
			{
				Parent.OCP_TypeInfo.AddWarning(message);
			}
		}

		#region Implementation

		OrgCompetitor Competitor
		{
			get { return (OrgCompetitor)base.Parent; }
		}

		#endregion
	}
}

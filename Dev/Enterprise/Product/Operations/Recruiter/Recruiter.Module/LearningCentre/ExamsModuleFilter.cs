using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class ExamsModuleFilter : ModuleGuidForeignCollectionFilter
	{
		public ExamsModuleFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.LearningCentreCampaign, ExamSettingSchema.EXS_G0, GlbCompanyCampaignSchema.PK, list, typeof(ExamSetting))
		{
		}

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			query.AddSubQuery(ExamSettingSchema.EXS_G0, subQuery, JoinCondition.And);
		}
	}
}

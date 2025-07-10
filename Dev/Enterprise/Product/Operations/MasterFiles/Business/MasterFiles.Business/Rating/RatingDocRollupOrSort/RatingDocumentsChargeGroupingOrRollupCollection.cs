using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDocumentsChargeGroupingOrRollupCollection : DependentBusinessObjectCollection<RatingDocumentsChargeGroupingOrRollup, OrgCompanyData>
	{
		public RatingDocumentsChargeGroupingOrRollupCollection(OrgCompanyData companyData)
			: base(companyData)
		{
		}

		protected override string FkColumnName => RatingDocumentsChargeGroupingOrRollupSchema.RCG_OB_CompanyData.Name;
	}
}

using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class CompanyTariffProcessTaskCollection : RatingHeaderProcessTaskCollection<CompanyTariff, CompanyTariffProcessTask>
	{
		public CompanyTariffProcessTaskCollection(CompanyTariff companyTariff)
			: base(companyTariff)
		{
		}

		protected override ProcessTaskCollection GetNewCollectionCore(CompanyTariff parent)
		{
			return new CompanyTariffProcessTaskCollection(parent);
		}
	}
}



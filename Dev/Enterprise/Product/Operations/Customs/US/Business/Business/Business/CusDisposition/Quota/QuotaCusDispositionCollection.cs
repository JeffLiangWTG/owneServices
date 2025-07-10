using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class QuotaCusDispositionCollection : CusDispositionCollection
	{
		public QuotaCusDispositionCollection(ICusDispositionParent cusDispositionParent) : base(cusDispositionParent)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var additionalFilter = new ZQuery();
			additionalFilter.AddToFilter(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USQuotaLineStatus);
			result.AddToFilter(additionalFilter);
			return result;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class BizObjectRateLineTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();

			var collection = new RateLinesCollection(rateEntry);
			return collection.AddNew();
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name.Equals(RateLine.Schema.TL_TI))
			{
				using (info.BizObj.MarkAsInDeletion())
				{
					base.TestBizObjectField(info);
				}
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}
	}
}

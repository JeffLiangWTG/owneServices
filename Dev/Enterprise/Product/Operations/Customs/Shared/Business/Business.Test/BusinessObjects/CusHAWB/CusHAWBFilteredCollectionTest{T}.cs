using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusHAWBFilteredCollectionTest<T> : SubsetBusinessObjectCollectionTestCase<T, CusHAWB> where T : CusHAWBFilteredCollection
	{
		CusMAWB mawb;
		protected CusMAWB MAWB
		{
			get
			{
				return mawb ?? (mawb = GetNewMAWB());
			}
		}

		protected virtual CusMAWB GetNewMAWB()
		{
			return Factory.New<CusMAWB>();
		}

		protected override T GetCollectionToTest()
		{
			return (T)MAWB.FilteredChildBills;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusHAWB>();
		}
	}
}

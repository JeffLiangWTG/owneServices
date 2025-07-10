using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusEntryLineFeeCollectionTest<TFee, TLine> : BusinessObjectCollectionTestCase
			where TFee : CusEntryLineFee
			where TLine : CusEntryLine
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(Factory.New<CusEntryLine>(), Factory);
		}
	}
}

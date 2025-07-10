using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ATFCategoryCodeListTest : TestCaseWithFactory
	{
		public void TestCoutFELRequired()
		{
			var list = new ATFCategoryCodeList();
			int count = list.Cast<object>().Count(item => ATFCategoryCodeList.IsFELRequired(item.ToString()));
			AssertEquals(8, count);
		}

		public void TestCoutFFLRequired()
		{
			var list = new ATFCategoryCodeList();
			int count = list.Cast<object>().Count(item => ATFCategoryCodeList.IsFFLRequired(item.ToString()));
			AssertEquals(45, count);
		}

		public void TestCountPermitNotRequired()
		{
			var list = new ATFCategoryCodeList();
			int count = list.Cast<object>().Count(item => !ATFCategoryCodeList.IsPermitRequired(item.ToString()));
			AssertEquals(5, count);
		}

		public void TestCountAECANotRequired()
		{
			var list = new ATFCategoryCodeList();
			int count = list.Cast<object>().Count(item => !ATFCategoryCodeList.IsAECARequired(item.ToString()));
			AssertEquals(16, count);
		}
	}
}

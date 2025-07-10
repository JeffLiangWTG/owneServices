using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(PortArrivalDetail))]
	public class PortArrivalDetailTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2016, 8, 1)]
		public void TestProperties()
		{
			var arrivalDetail = new PortArrivalDetail(Factory);
			arrivalDetail.PortCode = "1101";
			arrivalDetail.ActualArrivalDate = ZDateTime.Today;
			AssertEquals("1101", arrivalDetail.PortCode);
			AssertEquals(ZDateTime.Today, arrivalDetail.ActualArrivalDate);
		}
	}
}

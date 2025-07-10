using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCodeDataCollection<DummyCusCodeData>))]
	sealed class GenericCusCodeDataCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusCodeDataCollection<DummyCusCodeData>(Factory.New<CusEntryHeader>(), GuaranteeCusCodeDataTypeList.Codes.GRN);
		}
	}
}

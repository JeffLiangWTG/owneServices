using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusPerson))]
	public abstract class CusPersonAbstractTest<TCusPersonCountry> : EnterpriseBusinessObjectTestCase
		where TCusPersonCountry : CusPersonCountry
	{
		public virtual void TestCountriesType()
		{
			var person = (CusPerson)GetNewBusinessObject();

			AssertEquals("CusPersonCountryCollection type", typeof(CusPersonCountryCollection<TCusPersonCountry>), person.Countries.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}

	[TestedType(typeof(CusPerson))]
	class CusPersonTest : CusPersonAbstractTest<CusPersonCountry>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var person = Factory.NewWithValidTestData<CusPerson>();
			return person;
		}
	}
}

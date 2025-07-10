using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusPersonCountryCollectionTest<TCusPersonCountryCollection, TCusPersonCountry> : BusinessObjectCollectionTestCase
		where TCusPersonCountryCollection : CusPersonCountryCollection<TCusPersonCountry>
		where TCusPersonCountry : AutoCusPersonCountry
	{
		protected override Type GetExpectedCollectionType() => typeof(TCusPersonCountryCollection);
	}

	[TestedType(typeof(CusPersonCountryCollection<CusPersonCountry>))]
	class CusPersonCountryCollectionTest : CusPersonCountryCollectionTest<CusPersonCountryCollection<CusPersonCountry>, CusPersonCountry>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var person = Factory.NewWithValidTestData<CusPerson>();
			return new CusPersonCountryCollection<CusPersonCountry>(person);
		}
	}
}

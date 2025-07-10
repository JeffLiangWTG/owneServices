using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<CusPersonCountry>
	{
		public void TestICusPerson()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.ICusPerson>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPerson>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var person = (CusPerson)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(person.Header);
		}

		public void TestCountries()
		{
			var person = (CusPerson)GetNewBusinessObject();
			AssertType<ASYCUDA.Business.CusPersonCountryCollection<CusPersonCountry>>(person.Countries);
		}

		public void TestCreateNewCusPersonCountryCollection()
		{
			var person = Factory.New<CusPersonForTest>();
			AssertType<ASYCUDA.Business.CusPersonCountryCollection<CusPersonCountry>>(person.CreateNewCusPersonCountryCollection());
		}

		public void TestGetPersonCountryTypeCore()
		{
			var person = Factory.New<CusPersonForTest>();
			AssertEquals(typeof(CusPersonCountry), person.GetPersonCountryTypeCore());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			return person;
		}

		sealed class CusPersonForTest : CusPerson
		{
			public CusPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal new Customs.Business.ICusPersonCountryCollection<Customs.Business.AutoCusPersonCountry> CreateNewCusPersonCountryCollection() => base.CreateNewCusPersonCountryCollection();

			internal new Type GetPersonCountryTypeCore() => base.GetPersonCountryTypeCore();
		}
	}
}

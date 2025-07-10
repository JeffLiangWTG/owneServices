using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	public class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<CusPersonCountry>
	{
		public void TestICusPerson()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRManifest.ICusPerson>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPerson>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var person = (CusPerson)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(person.Header);
		}

		public void TestGetPersonCountryTypeCore()
		{
			var person = Factory.New<CusPersonForTest>();
			AssertEquals(typeof(CusPersonCountry), person.GetPersonCountryTypeCore());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			return person;
		}

		class CusPersonForTest : CusPerson
		{
			public CusPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Type GetPersonCountryTypeCore() => base.GetPersonCountryTypeCore();
		}
		#endregion
	}
}

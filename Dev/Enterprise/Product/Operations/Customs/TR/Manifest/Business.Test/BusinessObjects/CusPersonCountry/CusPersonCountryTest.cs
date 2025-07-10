using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(CusPersonCountry))]
	public class CusPersonCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusPersonCountry()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRManifest.ICusPersonCountry>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPersonCountry>(bizObj.PK).GetType());
		}

		public void TestParentPerson()
		{
			var personCountry = (CusPersonCountry)GetNewBusinessObject();
			AssertType<CusPerson>(personCountry.ParentPerson);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			var country = person.Countries.AddNew();
			country.CPC_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			country.CPC_Type = "OCC";
			country.CPC_Value = "H";
			return country;
		}
		#endregion
	}
}

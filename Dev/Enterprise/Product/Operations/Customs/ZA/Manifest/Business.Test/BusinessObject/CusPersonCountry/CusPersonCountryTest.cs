using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(CusPersonCountry))]
	class CusPersonCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusPersonCountry()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.ICusPersonCountry>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPersonCountry>(bizObj.PK).GetType());
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
			person.OccupationInZA = "A";
			person.ReasonForMovementInZA = "A";
			person.TravelDocumentTypeInZA = "A";
			person.TravellerTypeInZA = "A";
			return person.Countries[0];
		}
	}
}

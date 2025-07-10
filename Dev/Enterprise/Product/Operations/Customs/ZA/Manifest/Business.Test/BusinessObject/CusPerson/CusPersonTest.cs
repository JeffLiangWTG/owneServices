using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<CusPersonCountry>
	{
		public void TestICusPerson()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.ICusPerson>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPerson>(bizObj.PK).GetType());
		}

		public void TestRemovePersonCountryIfEmpty()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var persons = manifest.Persons;
			var person = persons.AddNew();
			person.ReasonForMovementInZA = ZaReasonForMovement.Codes.Transit;
			AssertNotNull(person.Countries.OfType<CusPersonCountry>().FirstOrDefault(c => c.CPC_RN_NKCountry == Core.Constants.CountryCodes.SouthAfrica && c.CPC_Type == ZaDataTypes.Codes.ReasonForMovement));
			person.ReasonForMovementInZA = ZString.Empty;
			AssertNull(person.Countries.OfType<CusPersonCountry>().FirstOrDefault(c => c.CPC_RN_NKCountry == Core.Constants.CountryCodes.SouthAfrica && c.CPC_Type == ZaDataTypes.Codes.ReasonForMovement));
		}

		public void TestFetchStrategy()
		{
			var person = Factory.New<CusPerson>();
			AssertType<CusPersonFetchStrategy>(person.FetchStrategy);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			person.OccupationInZA = "A";
			person.ReasonForMovementInZA = "A";
			person.TravelDocumentTypeInZA = "A";
			person.TravellerTypeInZA = "A";
			return person;
		}
	}
}

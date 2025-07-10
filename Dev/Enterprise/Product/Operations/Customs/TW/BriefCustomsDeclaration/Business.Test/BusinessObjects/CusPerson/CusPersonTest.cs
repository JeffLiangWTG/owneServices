using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<ASYCUDA.Business.CusPersonCountry>
	{
		public void TestPersonDescription()
		{
			var person = (CusPerson)GetNewBusinessObject();
			var glbPerson = person.Person;

			glbPerson.PER_DriversLicenseNumber = "DLN001";
			glbPerson.PER_Passport = "PSP001";
			AssertEquals("DLN001", person.PersonDescription);

			glbPerson.PER_DriversLicenseNumber = ZString.Empty;
			AssertEquals("PSP001", person.PersonDescription);

			glbPerson.PER_Passport = ZString.Empty;
			AssertEquals(ZString.Empty, person.PersonDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var header = Factory.New<AsycudaManifestHeader>();
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			return person;
		}
	}
}

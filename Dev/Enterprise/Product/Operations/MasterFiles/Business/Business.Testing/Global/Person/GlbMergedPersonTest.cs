using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbMergedPerson))]
	public class GlbMergedPersonTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var randomGuid = ZGuid.NewZGuid();
			var mergedPerson = Factory.New<GlbMergedPerson>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Name For Test";
			mergedPerson.GMP_PER_Person = person.PK;
			mergedPerson.GMP_MergedPerson = randomGuid;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedMergedPerson = newFactory.Load<GlbMergedPerson>(mergedPerson.PK);
			AssertEquals(loadedMergedPerson.GMP_MergedPerson, randomGuid);
			AssertEquals(loadedMergedPerson.Person.PK, person.PK);
			AssertEquals(loadedMergedPerson.Person.PER_FullName, "Name For Test");
		}
	}
}

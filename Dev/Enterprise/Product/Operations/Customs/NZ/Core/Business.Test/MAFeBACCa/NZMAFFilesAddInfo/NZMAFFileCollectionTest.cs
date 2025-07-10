namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(NZMAFFileCollection))]
	public class NZMAFFileCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var maf = TestDataBuilder.GetMAFMessaging(Factory.NewWithValidTestData<JobDeclaration>());
			return new NZMAFFileCollection(maf);
		}
	}
}

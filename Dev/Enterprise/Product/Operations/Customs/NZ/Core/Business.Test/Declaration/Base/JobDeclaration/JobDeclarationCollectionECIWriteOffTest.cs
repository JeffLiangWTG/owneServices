using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationCollectionECIWriteOff))]
	public class JobDeclarationCollectionECIWriteOffTest : JobDeclarationCollectionTest
	{
		public void TestAllowNew()
		{
			var collection = new JobDeclarationCollectionECIWriteOff(Factory, GlbCompany.CurrentCompany.PK);
			Assert(!collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollectionECIWriteOff(Factory, GlbCompany.CurrentCompany.PK);
	}
}

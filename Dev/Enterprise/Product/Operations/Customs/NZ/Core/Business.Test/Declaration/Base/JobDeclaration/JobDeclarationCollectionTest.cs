using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(JobDeclarationCollection))]
	public class JobDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}

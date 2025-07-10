using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	sealed class JobDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobDeclaration>();

		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}

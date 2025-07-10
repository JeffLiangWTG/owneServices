using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconDeclaration))]
	sealed class ReconDeclarationCustomFieldsTest : TestICustomFieldProvider
	{
		protected override BusinessObject GetBizo() => new ReconDeclaration(Factory.New<JobDeclaration>());
	}
}

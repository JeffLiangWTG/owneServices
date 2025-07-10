using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoReconOriginalDeclaration))]
	sealed class AddInfoReconOriginalDeclarationBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddInfoReconOriginalDeclaration(Factory.New<JobDeclaration>().JE_AddInfoInfo);
	}
}

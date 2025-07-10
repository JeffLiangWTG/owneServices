using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobDeclarationDeepCloneStrategy))]
sealed class JobDeclarationDeepCloneStrategyTest : Customs.Business.Testing.JobDeclarationDeepCloneStrategyAbstractTest<JobDeclaration>
{
	public void TestCloneJE_GoodsNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsNumber = "111";
		var clonedDec = GetJobDeclarationClone(declaration);
		AssertEquals(nameof(JobDeclaration.JE_GoodsNumber), declaration.JE_GoodsNumber, clonedDec.JE_GoodsNumber);
	}

	public void TestCloneJE_Position()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_Position = "222";
		var clonedDec = GetJobDeclarationClone(declaration);
		AssertEquals(nameof(JobDeclaration.JE_Position), declaration.JE_Position, clonedDec.JE_Position);
	}

	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(JobDeclaration declarationToClone, Customs.Business.CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
	{
		return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
	}

	JobDeclaration GetJobDeclarationClone(JobDeclaration declaration) => (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
}

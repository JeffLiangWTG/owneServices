using System;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
sealed class CusEntryInstructionCollectionTest : Customs.Business.Testing.CusEntryInstructionCollectionTest
{
	public void TestSetDefaultsForNewChild_Arguments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var collection = new CusEntryInstructionCollectionForTesting(declaration);

		AssertArgumentExceptionThrown<ArgumentNullException>("child", () => collection.SetDefaultsForNewChildExposed(null));
	}

	public void TestSetDefaultsForNewChild_ShouldCallSetDefaultsForNewChildOnInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var collection = new CusEntryInstructionCollectionForTesting(declaration);
		var instructionMock = Factory.NewMoq<CusEntryInstruction>();

		AssertNoExceptionThrown(() => collection.SetDefaultsForNewChildExposed(instructionMock.Object));
		instructionMock.Verify(x => x.SetDefaultsForNewChild(It.IsAny<JobDeclaration>()), Times.Once);
	}

	class CusEntryInstructionCollectionForTesting(JobDeclaration declaration) : CusEntryInstructionCollection(declaration)
	{
		public void SetDefaultsForNewChildExposed(BusinessObject child) => base.SetDefaultsForNewChild(child);
	}
}

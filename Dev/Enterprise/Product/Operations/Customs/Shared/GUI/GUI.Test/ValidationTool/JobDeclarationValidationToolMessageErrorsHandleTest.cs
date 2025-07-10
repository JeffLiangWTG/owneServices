using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing;

sealed class JobDeclarationValidationToolMessageErrorsHandleTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationValidationToolMessageErrorsHandle(null));
	}

	public void TestHandle()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		var handle = new JobDeclarationValidationToolMessageErrorsHandle(declaration);
		AssertEquals(true, handle.Handle());
	}
}

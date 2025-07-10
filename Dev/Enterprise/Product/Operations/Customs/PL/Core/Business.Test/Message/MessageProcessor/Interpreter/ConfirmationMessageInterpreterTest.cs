using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(ConfirmationMessageInterpreter<CusEntryHeader>))]
sealed class ConfirmationMessageInterpreterTest : ConfirmationMessageInterpreterTestBase<CusEntryHeader>
{
	protected override CusEntryHeader CreateAttachedObject()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		return jobDeclaration.CustomsEntryHeaders.AddNew();
	}
}

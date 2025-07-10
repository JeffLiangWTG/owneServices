using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationValidation))]
	class JobDeclarationValidationBaseOnlyTest : JobDeclarationValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationValidation GetValidation() => new JobDeclarationValidation(jobDeclaration);
	}
}

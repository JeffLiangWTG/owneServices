using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationValidation))]
	class JobDeclarationValidationBaseOnlyTest : JobDeclarationValidationAbstractTest<JobDeclarationValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationValidation GetValidation() => new JobDeclarationValidation(jobDeclaration);
	}
}

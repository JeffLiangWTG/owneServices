using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobDeclarationValidation))]
	class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobDeclarationValidation GetValidation() => new ImportJobDeclarationValidation(jobDeclaration);
	}
}

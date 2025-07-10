using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobDeclarationValidation))]
	class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(jobDeclaration);
	}
}

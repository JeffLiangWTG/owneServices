using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobDeclarationValidation))]
	sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ExportJobDeclarationValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(jobDeclaration);
	}
}

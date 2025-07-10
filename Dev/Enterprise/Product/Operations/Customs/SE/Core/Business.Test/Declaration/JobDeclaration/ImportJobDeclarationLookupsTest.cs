using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobDeclarationLookups))]
	sealed class ImportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobDeclarationLookups GetLookups() => new ImportJobDeclarationLookups(jobDeclaration);
	}
}

using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobDeclarationLookups))]
	class ImportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest<ImportJobDeclarationLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override ImportJobDeclarationLookups GetLookups() => new ImportJobDeclarationLookups(jobDeclaration);
	}
}

using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobDeclarationLookups))]
	sealed class ExportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobDeclarationLookups GetLookups() => new ExportJobDeclarationLookups(jobDeclaration);
	}
}

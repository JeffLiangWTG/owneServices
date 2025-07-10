using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobDeclarationLookups))]
	class ImportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest
	{
		public void TestEntryStyleList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(EntryStyleList.Codes.IMPEU, EntryStyleList.Descriptions.IMPEU),
				new CodeDescriptionPair(EntryStyleList.Codes.IMPIM, EntryStyleList.Descriptions.IMPIM),
				new CodeDescriptionPair(EntryStyleList.Codes.IMPAN, EntryStyleList.Descriptions.IMPAN),
			}, lookups.EntryStyleList);
		}

		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobDeclarationLookups GetLookups() => new ImportJobDeclarationLookups(jobDeclaration);
	}
}

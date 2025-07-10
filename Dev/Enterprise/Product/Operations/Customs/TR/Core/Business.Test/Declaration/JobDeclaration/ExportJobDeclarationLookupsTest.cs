using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobDeclarationLookups))]
	class ExportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest
	{
		public void TestEntryStyleList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(EntryStyleList.Codes.EXPEU, EntryStyleList.Descriptions.EXPEU),
				new CodeDescriptionPair(EntryStyleList.Codes.EXPEX, EntryStyleList.Descriptions.EXPEX),
				new CodeDescriptionPair(EntryStyleList.Codes.EXPTR, EntryStyleList.Descriptions.EXPTR),
			}, lookups.EntryStyleList);
		}

		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobDeclarationLookups GetLookups() => new ExportJobDeclarationLookups(jobDeclaration);
	}
}

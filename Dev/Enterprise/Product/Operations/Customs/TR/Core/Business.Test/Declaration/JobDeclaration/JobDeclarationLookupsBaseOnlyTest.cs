using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationLookups))]
	class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsAbstractTest
	{
		public void TestEntryStyleList()
		{
			AssertContainsExactElementsInAnyOrder(new EntryStyleList(), lookups.EntryStyleList);
		}

		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationLookups GetLookups() => new JobDeclarationLookups(jobDeclaration);
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWMessageCollection))]
	sealed class TWMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TWMessage>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TWMessageCollection(EntryHeader);
		}

		[ExpectNoExceptions]
		public void TestEntryHeaderMessageCollection()
		{
			NUnit.Framework.Assert.That(EntryHeader.Messages.GetType(), NUnit.Framework.Is.EqualTo(typeof(TWMessageCollection)));
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Declaration.CustomsEntryHeaders.AddNew());
		CusEntryHeader entryHeader;
	}
}

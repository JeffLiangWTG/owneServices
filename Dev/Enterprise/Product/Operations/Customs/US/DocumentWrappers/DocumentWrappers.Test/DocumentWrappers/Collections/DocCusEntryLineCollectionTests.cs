using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLineCollection))]
	sealed class DocCusEntryLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineCollection>
	{
		public void TestAllowNew()
		{
			DocCusEntryLineCollection collection = new DocCusEntryLineCollection(Factory);
			AssertEquals("AllowNew", false, collection.AllowNew);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			return DocCusEntryLine.New(cusEntryLine, Factory);
		}

		protected override DocCusEntryLineCollection GetCollectionToTest()
		{
			return new DocCusEntryLineCollection(Factory);
		}
	}
}


using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs._CustomsTemplate_.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLineCollection))]
	sealed class DocCusEntryLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineCollection>
	{
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

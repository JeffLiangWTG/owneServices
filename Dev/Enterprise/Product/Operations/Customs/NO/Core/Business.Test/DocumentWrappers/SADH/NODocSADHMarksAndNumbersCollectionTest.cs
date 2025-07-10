using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHMarksAndNumbersCollection))]
sealed class NODocSADHMarksAndNumbersCollectionTest : DocBaseWrapperCollectionTest<NODocSADHMarksAndNumbersCollection>
{
	protected override NODocSADHMarksAndNumbersCollection GetNewDocumentWrapperCollection() => new NODocSADHMarksAndNumbersCollection(Factory);

	protected override object GetNewObjectToWrap() => MarksCollection[0];

	public NODocSADHMarksAndNumbersCollection MarksCollection => marksCollection ??= CreateNewCollection();
	NODocSADHMarksAndNumbersCollection marksCollection;

	NODocSADHMarksAndNumbersCollection CreateNewCollection()
	{
		var collection = new NODocSADHMarksAndNumbersCollection(Factory);
		var wrapper = new NODocSADHMarksAndNumbers("Test1", Factory);
		collection.Add(wrapper);
		return collection;
	}

	public void TestAddIfValueIsDistinct() => CombineAssertions(() =>
	{
		var collection = new NODocSADHMarksAndNumbersCollection(Factory);

		collection.AddIfValueIsDistinct("XXXX");
		AssertEquals("One entry", 1, collection.Count);

		collection.AddIfValueIsDistinct("YYYY");
		AssertEquals("Two entries", 2, collection.Count);

		collection.AddIfValueIsDistinct("XXXX");
		AssertEquals("Still two entries", 2, collection.Count);
	});
}


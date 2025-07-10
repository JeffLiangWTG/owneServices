using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHLineCollection))]
sealed class NODocSADHLineCollectionTest : DocBaseWrapperCollectionTest<NODocSADHLineCollection>
{
	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
	{
		var line = NODocSADHLine.New(lineCollection.AddNew(), Factory);
		collection.Add(line);
		return line;
	}

	protected override NODocSADHLineCollection GetNewDocumentWrapperCollection()
	{
		return new NODocSADHLineCollection(LineCollection, Factory);
	}

	ICusEntryLineCollection<CusEntryLine> LineCollection
	{
		get
		{
			if (lineCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();
				lineCollection = entryHeader.MergedLines;
			}

			return lineCollection;
		}
	}
	ICusEntryLineCollection<CusEntryLine> lineCollection;
}

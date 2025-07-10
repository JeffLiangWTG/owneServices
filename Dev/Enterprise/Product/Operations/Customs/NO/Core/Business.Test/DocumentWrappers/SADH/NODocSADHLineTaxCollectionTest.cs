using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHLineTaxCollection))]
sealed class NODocSADHLineTaxCollectionTest : DocBaseWrapperCollectionTest<NODocSADHLineTaxCollection>
{
	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var result = NODocSADHLineTax.New(FeeCollection.AddNew(), Factory);
		collection.Add(result);
		return result;
	}

	protected override NODocSADHLineTaxCollection GetNewDocumentWrapperCollection()
	{
		return new NODocSADHLineTaxCollection(FeeCollection, Factory);
	}

	public void TestTotalChargeAmount()
	{
		var fee1 = FeeCollection.AddNew();
		fee1.CF_ChargeAmount = 10.00m;

		var fee2 = FeeCollection.AddNew();
		fee2.CF_ChargeAmount = 20.00m;

		var wrapper = GetNewDocumentWrapperCollection();
		AssertEquals(30, wrapper.TotalChargeAmount);
	}

	CusEntryLineFeeCollection FeeCollection
	{
		get
		{
			if (feeCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.AllEntryLines.AddNew();
				feeCollection = entryLine.Fees;
			}

			return feeCollection;
		}
	}
	CusEntryLineFeeCollection feeCollection;
}

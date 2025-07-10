using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CommonCusReferenceCollectionTest : TestCaseWithFactory
	{
		public void TestEmptyType()
		{
			AssertExceptionThrown<ArgumentException>(() => new CommonCusReferenceCollectionForTest(null, ZString.Empty));
		}

		public void TestAddNew()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var collection = new CommonCusReferenceCollectionForTest(entryInstruction, "XX");
			var reference = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Type", "XX", reference.CFR_Type);
				AssertEquals("CFR_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, reference.CFR_ParentTableCode);
				AssertEquals("CFR_ParentID", entryInstruction.PK, reference.CFR_ParentID);
			});
		}

		class CommonCusReferenceCollectionForTest : CommonCusReferenceCollection<CommonCusReferenceForTest>
		{
			public CommonCusReferenceCollectionForTest(BusinessObject parent, ZString type) : base(parent, type)
			{
			}
		}
	}
}

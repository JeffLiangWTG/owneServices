using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionDocumentCollection))]
	sealed class CusEntryInstructionDocumentCollectionTest : CusCodeDataCollectionTest<CusEntryInstructionDocument>
	{
		protected override CusCodeDataCollection<CusEntryInstructionDocument> GetCusCodeDataCollection()
		{
			return new CusEntryInstructionDocumentCollection(Factory.New<CusEntryInstruction>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusEntryInstructionDocument>();
			var parent = Factory.New<CusEntryInstruction>();
			result.CY_ParentID = parent.PK;
			result.CY_ParentTableCode = parent.TablePrefix;
			return result;
		}

		[ExpectNoExceptions]
		public void TestDocumentNumbersAsString()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var collection = cusEntryInstruction.DocumentNumbers;
			collection.DocumentNumbersAsString = "A,B,C,D";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison));

			collection.DocumentNumbersAsString = "A,,C";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo(ZString.Empty));
			collection.DocumentNumbersAsString = "1111111111111111111111111111111111111111111111111111111111111";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.Not.EqualTo("1111111111111111111111111111111111111111111111111111111111111").Using(CustomComparers.TypeComparison));

			collection.DocumentNumbersAsString = "";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestSetCY_OrderWhenItemChanges()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var collection = cusEntryInstruction.DocumentNumbers;
			collection.DocumentNumbersAsString = "A,B,C";
			CombineAssertions(() =>
			{
				var item1 = collection[1];
				NUnit.Framework.Assert.That(collection[0].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1));
				NUnit.Framework.Assert.That(collection[1].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2));
				NUnit.Framework.Assert.That(collection[2].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)3));
				collection.Remove(item1);
				NUnit.Framework.Assert.That(collection[0].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1));
				NUnit.Framework.Assert.That(collection[1].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2));
				NUnit.Framework.Assert.That(collection[1].CY_Data, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison));
				var newItem = collection.AddNew();
				newItem.CY_Data = "D";
				NUnit.Framework.Assert.That(collection[2].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)3));
				NUnit.Framework.Assert.That(collection[2].CY_Data, NUnit.Framework.Is.EqualTo("D").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestAllowNewCore()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var collection = cusEntryInstruction.DocumentNumbers;
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			_ = collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			_ = collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			_ = collection.AddNew();
			NUnit.Framework.Assert.That(!collection.AllowNew, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestAllowSort()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var collection = cusEntryInstruction.DocumentNumbers as IBindingList;
			NUnit.Framework.Assert.That(!collection.SupportsSorting, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestLoadInOrder()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();

			var document3 = CreateDocument(3);
			var document2 = CreateDocument(2);
			var document1 = CreateDocument(1);

			var collection = new CusEntryInstructionDocumentCollection(cusEntryInstruction);
			collection.Load();
			NUnit.Framework.Assert.That(collection, NUnit.Framework.Is.EqualTo(new[] { document1, document2, document3 }));

			CusEntryInstructionDocument CreateDocument(ZShort order)
			{
				var document = Factory.New<CusEntryInstructionDocument>();
				document.Parent = cusEntryInstruction;
				document.CY_Order = order;
				return document;
			}
		}
	}
}

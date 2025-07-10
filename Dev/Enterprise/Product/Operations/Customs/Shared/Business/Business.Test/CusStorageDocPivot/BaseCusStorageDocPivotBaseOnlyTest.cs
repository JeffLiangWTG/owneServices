using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotForTest))]
	sealed class BaseCusStorageDocPivotBaseOnlyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocument()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = Factory.New<CusEntryInstructionAsTypeSupporter>();
			instruction.CEI_JE = declaration.PK;
			var pivot = Factory.New<CusStorageDocPivotForTest>();
			pivot.Parent = instruction;
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals(eDoc, pivot.Document);
		}

		public void TestFileName()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = Factory.New<CusEntryInstructionAsTypeSupporter>();
			instruction.CEI_JE = declaration.PK;
			var pivot = Factory.New<CusStorageDocPivotForTest>();
			pivot.Parent = instruction;
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals("Invoice.pdf", pivot.FileName);
		}

		public void TestUniqueIndexFailureHandler()
		{
			var pivot = (BaseCusStorageDocPivot)GetNewBusinessObjectForDeleteTest(Factory);
			var handler = GetUniqueIndexFailureHandler(pivot);
			CombineAssertions(() =>
			{
				AssertType<CusStorageDocPivotUniqueIndexFailureHandler>("Correct Handler Type", handler);
				AssertSame("Cached", handler, GetUniqueIndexFailureHandler(pivot));
			});
		}

		public void TestParent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			var pivot = Factory.New<CusStorageDocPivotForTest>();
			pivot.CSD_ParentTableCode = invoice.TablePrefix;
			pivot.CSD_ParentID = invoice.PK;

			CombineAssertions(() =>
			{
				AssertEquals("parent: invoice", invoice, pivot.Parent);
				pivot.Parent = invoice2;
				AssertEquals("CSD_ParentTableCode", invoice2.TablePrefix, pivot.CSD_ParentTableCode);
				AssertEquals("CSD_ParentID", invoice2.PK, pivot.CSD_ParentID);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() =>
			GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var pivot = Factory.New<CusStorageDocPivotForTest>();
			pivot.CSD_ParentTableCode = invoice.TablePrefix;
			pivot.CSD_ParentID = invoice.PK;
			pivot.CSD_DocType = "TY1";

			return pivot;
		}

		static IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(BaseCusStorageDocPivot pivot)
		{
			return ((IEnumerable<IUniqueIndexFailureHandler>)typeof(BaseCusStorageDocPivot).GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pivot, null)).Single();
		}

		class CusStorageDocPivotForTest : BaseCusStorageDocPivot
		{
			public CusStorageDocPivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(BaseJobComInvoiceHeader));
		}
	}
}

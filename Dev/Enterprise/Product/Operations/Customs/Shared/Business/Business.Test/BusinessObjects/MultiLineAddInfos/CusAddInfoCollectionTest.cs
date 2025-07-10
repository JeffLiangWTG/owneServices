using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(CusAddInfoCollection<AddInfoWithTypeCode>))]
	sealed class CusAddInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParent()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var collection = new CusAddInfoCollection<AddInfoWithTypeCode, BaseJobComInvoiceLine>(invoiceLine);
			collection.Load();
			var addInfoRow = collection.AddNew();
			AssertEquals("addInfoRow.Parent", invoiceLine, addInfoRow.Parent);
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var reloadedInvoiceLine = secondFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			var reloadedCollection = new CusAddInfoCollection<AddInfoWithTypeCode, BaseJobComInvoiceLine>(reloadedInvoiceLine);
			reloadedCollection.Load();
			AssertEquals("Precondition: reloadedCollection.Count", 1, reloadedCollection.Count);
			AssertEquals("reloadedCollection[0].Parent", reloadedInvoiceLine, reloadedCollection[0].Parent);
		}

		public void TestOnlyLoadsElementsOfTheRightType()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var collection = new CusAddInfoCollection<AddInfoWithTypeCode>(invoiceLine);
			collection.Load();
			var addInfoRowMatching = collection.AddNew();
			AssertEquals("addInfoRowMatching.B7_ParentID", invoiceLine.PK, addInfoRowMatching.B7_ParentID);
			AssertEquals("addInfoRowMatching.B7_ParentTableCode", "JI", addInfoRowMatching.B7_ParentTableCode);
			AssertEquals("addInfoRowMatching.B7_Type", CusAddInfoTypeAttribute.Codes.TypeCodeForTesting, addInfoRowMatching.B7_Type);

			var addInfoRowNonMatching = collection.AddNew();
			AssertEquals("addInfoRowNonMatching.B7_ParentID", invoiceLine.PK, addInfoRowNonMatching.B7_ParentID);
			AssertEquals("addInfoRowNonMatching.B7_ParentTableCode", "JI", addInfoRowNonMatching.B7_ParentTableCode);
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals("", ErrorReporter.LastMessageReported);

			collection = new CusAddInfoCollection<AddInfoWithTypeCode>(invoiceLine);
			collection.Load();
			AssertEquals("collection.Count", 2, collection.Count);
			AssertCollectionContains(addInfoRowMatching, collection);
		}

		public void TestCanHaveAJobComInvoiceLineAsAParent()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var collection = new CusAddInfoCollection<AddInfoWithTypeCode>(invoiceLine);
			collection.Load();
			var addInfoRow = collection.AddNew();
			AssertEquals(invoiceLine.PK, addInfoRow.B7_ParentID);
			AssertEquals("JI", addInfoRow.B7_ParentTableCode);
		}

		public void TestCanHaveAJobDeclarationAsAParent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var collection = new CusAddInfoCollection<AddInfoWithTypeCode>(declaration);
			collection.Load();
			var addInfoRow = collection.AddNew();
			AssertEquals(declaration.PK, addInfoRow.B7_ParentID);
			AssertEquals("JE", addInfoRow.B7_ParentTableCode);
		}

		public void TestGetInners()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			var collection = new CusAddInfoCollection<AddInfoWithTypeCode>(declaration);
			collection.Load();
			CusAddInfo<AddInfoWithTypeCode> addInfoRow = collection.AddNew();
			CusAddInfo<AddInfoWithTypeCode> addInfoRow2 = collection.AddNew();
			List<BusinessObject> list = collection.GetInners();
			Assert(list.Contains(addInfoRow2.Data) && list.Contains(addInfoRow.Data));
		}

		[ExpectNoExceptions]
		public void TestWrongConstructorBlows()
		{
			NUnit.Framework.Assert.That(delegate
			{
				new CusAddInfoCollection<AddInfoWithWrongConstructor>(Factory.New<CusEntryHeader>());
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentException)));
		}

		[ExpectNoExceptions]
		public void TestMissingTypeCodeAttributeBlows()
		{
			NUnit.Framework.Assert.That(delegate
			{
				new CusAddInfoCollection<Business.Testing.TestAddInfo>(Factory.New<CusEntryLine>());
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentException)));
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusAddInfoCollection<AddInfoWithTypeCode>(Factory.New<BaseJobComInvoiceHeader>());
		}

		#endregion
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusCodeDataTest : TestCaseWithFactory
	{
		public void TestDescription()
		{
			var mockCusCode = Factory.NewMoq<DummyCusCodeData>();
			var cusCode = mockCusCode.Object;
			var mockCusCodeLookups = new Mock<CusCodeDataLookups>(cusCode);
			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("ZZ1", "ZZ1 Blah");
			mockCusCodeLookups.Setup(m => m.CY_CodeList).Returns(pairList).Verifiable();
			mockCusCode.Protected().Setup<CusCodeDataLookups>("GetNewLookups").Returns(mockCusCodeLookups.Object).Verifiable();
			cusCode.CY_Code = "ZZZ";
			AssertEquals("", cusCode.Description);
			cusCode.CY_Code = "ZZ1";
			AssertEquals("ZZ1 Blah", cusCode.Description);
			cusCode.CY_Code = "";
			AssertEquals("", cusCode.Description);
			mockCusCode.Verify();
			mockCusCodeLookups.Verify();
		}

		[ExpectNoExceptions]
		public void TestMarkParentAsNeedingValidation()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var dec = mockDec.Object;
			var mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var invoice = mockInvoice.Object;
			dec.Invoices.Add(invoice);
			var cusCode = Factory.New<DummyCusCodeData>();
			cusCode.Parent = dec;

			mockDec.Invocations.Clear();
			mockInvoice.Invocations.Clear();
			cusCode.MarkParentAsNeedingValidation();
			mockDec.Protected().Verify("MarkAsNeedingValidationCore", Times.AtLeastOnce());
			mockInvoice.Protected().Verify("MarkAsNeedingValidationCore", Times.Never());
		}

		[ExpectNoExceptions]
		public void TestMarkParentAsNeedingValidationIncludingChildren()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var dec = mockDec.Object;
			var mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var invoice = mockInvoice.Object;
			dec.Invoices.Add(invoice);
			var cusCode = Factory.New<DummyCusCodeData>();
			cusCode.Parent = dec;

			mockDec.Invocations.Clear();
			mockInvoice.Invocations.Clear();
			cusCode.MarkParentAsNeedingValidationIncludingChildren();
			mockDec.Protected().Verify("MarkAsNeedingValidationCore", Times.AtLeastOnce());
			mockInvoice.Protected().Verify("MarkAsNeedingValidationCore", Times.AtLeastOnce());
		}

		public void TestParent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			DummyCusCodeData cusCode = Factory.New<DummyCusCodeData>();
			AssertNull("Parent", cusCode.Parent);
			AssertEquals("CY_ParentID", ZGuid.Empty, cusCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", ZString.Empty, cusCode.CY_ParentTableCode);

			cusCode.Parent = declaration;
			AssertEquals("Parent", declaration, cusCode.Parent);
			AssertEquals("CY_ParentID", declaration.PK, cusCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", "JE", cusCode.CY_ParentTableCode);

			cusCode.Parent = null;
			cusCode.CY_ParentID = declaration.PK;
			cusCode.CY_ParentTableCode = "JE";
			AssertEquals("Parent", declaration, cusCode.Parent);
			AssertEquals("CY_ParentID", declaration.PK, cusCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", "JE", cusCode.CY_ParentTableCode);

			cusCode.Parent = invoiceLine;
			AssertEquals("Parent", invoiceLine, cusCode.Parent);
			AssertEquals("CY_ParentID", invoiceLine.PK, cusCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", "JI", cusCode.CY_ParentTableCode);

			cusCode.Parent = null;
			cusCode.CY_ParentID = invoiceLine.PK;
			cusCode.CY_ParentTableCode = "JI";
			AssertEquals("Parent", invoiceLine, cusCode.Parent);
			AssertEquals("CY_ParentID", invoiceLine.PK, cusCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", "JI", cusCode.CY_ParentTableCode);

			cusCode.CY_ParentID = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Enterprise.Customs.Business.Testing.DummyCusCodeData.CY_ParentID must be specified. The parent table code is JI", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNoDBHitOnStmDocDataOverrideWhenDelete()
		{
			var factory = Factory.CreateNewFactory();
			var cusCodeData = factory.New<DummyCusCodeData>();
			cusCodeData.CY_ParentTableCode = "JI";
			cusCodeData.CY_ParentID = ZGuid.NewZGuid();
			factory.Save();
			cusCodeData.Delete();
			var expectedDBHits = new Dictionary<string, int> { { "StmDocDataOverride", 0 } };
			AssertDbHits(expectedDBHits, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}
	}
}

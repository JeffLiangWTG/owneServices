using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NPBOCusGuaranteeLineTransaction))]
	sealed class NPBOCusGuaranteeLineTransactionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NPBOCusGuaranteeLineTransaction(null));
		}

		public void TestLookups()
		{
			var npbo = (NPBOCusGuaranteeLineTransaction)GetNewBusinessObject();
			AssertType<NPBOCusGuaranteeLineTransactionLookups>(npbo.Lookups);
		}

		public void TestFieldsReadOnlyIfTypeIsEmpty()
		{
			var npbo = (NPBOCusGuaranteeLineTransaction)GetNewBusinessObject();
			npbo.CPL_TransactionType = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("CPL_TransactionType", false, npbo.CPL_TransactionTypeInfo.ReadOnly);
				AssertEquals("CPL_TransactionDate", true, npbo.CPL_TransactionDateInfo.ReadOnly);
				AssertEquals("CPL_Reference", true, npbo.CPL_ReferenceInfo.ReadOnly);
				AssertEquals("CPL_TranValue", true, npbo.CPL_TranValueInfo.ReadOnly);
				AssertEquals("CPL_Comment", true, npbo.CPL_CommentInfo.ReadOnly);
			});
		}

		public void TestFieldsEditableIfTypeIsEntered()
		{
			var npbo = (NPBOCusGuaranteeLineTransaction)GetNewBusinessObject();
			npbo.CPL_TransactionType = "abc";
			CombineAssertions(() =>
			{
				AssertEquals("CPL_TransactionType", false, npbo.CPL_TransactionTypeInfo.ReadOnly);
				AssertEquals("CPL_TransactionDate", false, npbo.CPL_TransactionDateInfo.ReadOnly);
				AssertEquals("CPL_Reference", false, npbo.CPL_ReferenceInfo.ReadOnly);
				AssertEquals("CPL_TranValue", false, npbo.CPL_TranValueInfo.ReadOnly);
				AssertEquals("CPL_Comment", false, npbo.CPL_CommentInfo.ReadOnly);
			});
		}

		public void TestDefaultDate()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			var newLineTransaction = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			newLineTransaction.CPL_TranValue = 100m;
			AssertEquals("Default date should be today when setting the line transaction value", ZDate.Today, newLineTransaction.CPL_TransactionDate.Date);
		}

		public void TestDefaultTransactionType_NoLineTransactionExists()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			newTransactionLine.CPL_TranValue = 100m;
			AssertEquals("Default transaction should be of type OBL when no line transaction exists", PermitTransactionTypeList.Codes.OBL, newTransactionLine.CPL_TransactionType);
		}

		public void TestDefaultTransactionType_LineTransactionExists()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			newTransactionLine.CPL_TranValue = 100m;
			AssertEquals("Default transaction should be of type ADJ when a line transaction already exists", PermitTransactionTypeList.Codes.ADJ, newTransactionLine.CPL_TransactionType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NPBOCusGuaranteeLineTransaction(Factory.New<BaseCusGuaranteeHeader>());
		}
	}
}

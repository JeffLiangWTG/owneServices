using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GovernmentInvoice))]
	sealed class GovernmentInvoiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClearComplianceBookWhenComplianceNumberOrComplainceSubTypeHasChange()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 25;
			sequence.XD_Prefix = "AAA";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GC = sequence.XD_GC_Company;
			invoice.AH_GB = sequence.XD_GB_BranchOwner;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_ComplianceSubType = "TCR";
			invoice.AH_TransactionReference = "Test 123";
			invoice.AH_TransactionNum = "1111";
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_PostDate = ZDateTime.Now;

			Factory.Save();

			invoice.AH_XD_ComplianceBook = sequence.PK;
			Factory.Save();

			AssertEquals("ComplianceBook is stored in AH_XD_ComplianceBook.", sequence.PK, invoice.AH_XD_ComplianceBook);

			invoice.AH_TransactionReference = "Changed 123";
			Factory.Save();

			AssertEquals("ComplianceBook is cleared after modifying AH_TransactionReference.", ZGuid.Empty, invoice.AH_XD_ComplianceBook);

			invoice.AH_XD_ComplianceBook = sequence.PK;
			Factory.Save();

			AssertEquals("ComplianceBook is stored in AH_XD_ComplianceBook.", sequence.PK, invoice.AH_XD_ComplianceBook);

			invoice.AH_ComplianceSubType = "TXI";
			Factory.Save();

			AssertEquals("ComplianceBook is cleared after modifying AH_ComplianceSubType.", ZGuid.Empty, invoice.AH_XD_ComplianceBook);
		}

		public void TestGovtTaxInvoiceDisplay_TransactionReference()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Peru;
			GovernmentInvoice header = Factory.NewWithValidTestData<GovernmentInvoice>();
			header.AH_TransactionReference = "123.001";
			header.AH_ComplianceSubType = "TXI";
			Factory.Save();

			header.AH_TransactionReference = "abc-12345";
			AssertEquals("abc-12345", header.AH_TransactionReference);
		}

		public void TestGovtTaxInvoiceDisplay_ComplianceSubtype()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Peru;
			GovernmentInvoice header = Factory.NewWithValidTestData<GovernmentInvoice>();
			header.AH_TransactionReference = "123.001";
			header.AH_ComplianceSubType = "TXI";
			Factory.Save();

			header.AH_ComplianceSubType = "TCR";
			AssertEquals("TCR", header.AH_ComplianceSubType);
		}

		public void TestLogTransactionReferenceChanged()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Peru;
			GovernmentInvoice header = Factory.NewWithValidTestData<GovernmentInvoice>();
			header.AH_TransactionReference = "123.001";
			header.AH_ComplianceSubType = "TXI";
			header.AH_InvoiceDate = new ZDate(2016, 3, 15);
			header.AH_ComplianceDocumentDate = new ZDate(2016, 3, 16);

			header.AH_TransactionReference = "xyz-001";
			header.AH_ComplianceDocumentDate = new ZDate(2016, 3, 18);
			AssertEquals("change transaction reference should NOT be logged as it's not in database yet", 0, header.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Compliance Number updated in Compliance Update Form. Old value: '123.001', New value: 'xyz-001'")).Length);
			AssertEquals("change compliance document date should NOT be logged as it's not in database yet", 0, header.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Compliance Document Date updated in Compliance Update Form. Old value: '16-Mar-16', New value: '18-Mar-16'")).Length);
			Factory.Save();

			header.AH_TransactionReference = "abc-12345";
			header.AH_ComplianceDocumentDate = new ZDate(2016, 03, 20);
			Factory.Save();
			AssertEquals("change transaction reference should be logged as it's in database now", 1, header.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Compliance Number updated in Compliance Update Form. Old value: 'xyz-001', New value: 'abc-12345'")).Length);
			AssertEquals("change compliance document date should be logged as it's in database now", 1, header.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Compliance Document Date updated in Compliance Update Form. Old value: '18-Mar-16', New value: '20-Mar-16'")).Length);
		}

		public void TestGovtTaxInvoiceSave_TriggerActioned()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Peru;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = header.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			ProcessTask trigger = Factory.NewWithValidTestData<ProcessTask>();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.P9_ParentID = header.PK;
			trigger.P9_ParentTableCode = "AH";
			trigger.P9_Description = "Test Trigger";

			// Working around the fact we can't create the process task type we want
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent90Code;

			Factory.Save();
			var factory2 = Factory.CreateNewFactory();
			var trigger2 = factory2.Load<ProcessTask>(new ZQuery()).First(x => x.PK == trigger.PK);
			trigger2.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;

			factory2.Save();

			AssertEquals("Trigger is open status.", ProcessTaskStatusCodeList.Codes.Open, trigger2.P9_Status);
			Assert("Trigger isn't updated before update compliance number.", trigger2.P9_ActualDate.IsEmpty);

			GovernmentInvoice header1 = factory2.Load<GovernmentInvoice>(header.PK);
			header1.AH_TransactionReference = "123.001";
			header1.AH_ComplianceSubType = "TXI";
			factory2.Save();

			AssertEquals("Trigger is closed status.", ProcessTaskStatusCodeList.Codes.Closed, trigger2.P9_Status);
			Assert("Trigger is updated after update compliance number.", trigger2.P9_ActualDate.IsValid);
		}

		public void TestComplianceSubTypeIsReadOnlyForPortugalWhenComplianceNumberIsEntered()
		{
			var header = Factory.NewWithValidTestData<GovernmentInvoice>();
			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.AH_ComplianceSubType = "TXI";
			Assert(!header.AH_ComplianceSubTypeInfo.ReadOnly);

			Factory.Save();
			Assert("Should still be editable", !header.AH_ComplianceSubTypeInfo.ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				Assert("Pre-condition", header.Company.Country.SupportDocumentSigning);
				Assert("ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);

				header.AH_TransactionReference = "FAC 001/000001";
				Assert("ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);

				header.AH_ComplianceSubType = ZString.Empty;
				Assert("Still ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);

				header.AH_TransactionReference = ZString.Empty;
				Assert("Not ReadOnly", !header.AH_ComplianceSubTypeInfo.ReadOnly);

				header.AH_ComplianceSubType = "TXI";
				Assert("ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);

				header.AH_TransactionReference = "FAC 001/000001";
				Assert("ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);

				Factory.Save();
				Assert("ReadOnly", header.AH_ComplianceSubTypeInfo.ReadOnly);
			}

			Assert(!header.Company.Country.SupportDocumentSigning);
			Assert(!header.AH_ComplianceSubTypeInfo.ReadOnly);
		}

		public void TestTransactionReferenceIsReadOnlyForPortugal()
		{
			var header = Factory.NewWithValidTestData<GovernmentInvoice>();
			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.AH_ComplianceSubType = "TXI";
			header.AH_TransactionReference = "FAC 001/000001";
			Assert(!header.AH_TransactionReferenceInfo.ReadOnly);

			Factory.Save();
			Assert("Should still be editable", !header.AH_TransactionReferenceInfo.ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				Assert("Pre-condition", header.Company.Country.SupportDocumentSigning);
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);

				header.AH_ComplianceSubType = ZString.Empty;
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);

				header.AH_TransactionReference = ZString.Empty;
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);

				header.AH_ComplianceSubType = "TXI";
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);

				header.AH_TransactionReference = "FAC 001/000002";
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);

				Factory.Save();
				Assert("ReadOnly", header.AH_TransactionReferenceInfo.ReadOnly);
			}

			Assert(!header.Company.Country.SupportDocumentSigning);
			Assert(!header.AH_TransactionReferenceInfo.ReadOnly);
		}

		#region E-Invoicing Related Tests

		public void TestEvaluateEInvoicingEligibility_IsCalledOnSave()
		{
			var eInvoicingProxyFactoryMock = new Mock<IEInvoicingTransactionProxyFactory>();

			var transactionMock = new Mock<IEInvoicingTransaction>();
			transactionMock.Setup(x => x.CurrentStatus).Returns(ZString.Empty);
			eInvoicingProxyFactoryMock.Setup(c => c.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(transactionMock.Object);

			using (ObjectFactory.Substitute(eInvoicingProxyFactoryMock.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
				invoice.AH_ComplianceSubType = "ABC";
				invoice.AH_TransactionReference = "ZXC9999";
				Factory.Save();
				transactionMock.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Never(), "EInvoicing eligibility is not evaluated until after bizo is in database");

				invoice.AH_ComplianceSubType = "DEF";
				invoice.AH_TransactionReference = "ZXC9999";
				Factory.Save();
				transactionMock.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Once(), "EInvoicing eligibility must be evaluated");
				Assert("Assertions done via mocks", true);

				// Note: we cannot assert pivots or status in MasterFiles as the e-Invoicing bizos are in Accounting.Business.
				// Further assertions are made in implementation of IEInvoicingTransaction
			}
		}

		#endregion
	}
}

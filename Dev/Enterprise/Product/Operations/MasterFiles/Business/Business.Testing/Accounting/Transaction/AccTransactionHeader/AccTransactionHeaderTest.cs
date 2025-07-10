using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionHeaderTest : TestCaseWithFactory
	{
		#region Transaction Number Fields Max Length Compatibility Test

		public void TestTransactionNumberFieldsMaxLengthCompatibility()
		{
			var jR_APInvoiceNumNode = new TreeNode<SchemaStringColumn>(JobChargeSchema.JR_APInvoiceNum);

			var aH_TransactionNumNode = jR_APInvoiceNumNode.AddChild(AccTransactionHeaderSchema.AH_TransactionNum);
			aH_TransactionNumNode.AddChild(AccTransactionHeaderSchema.AH_OriginalTransactionNum);
			aH_TransactionNumNode.AddChild(JobConsolCostSchema.E6_InvoiceNum);
			aH_TransactionNumNode.AddChild(NettingPayableLineReferenceSchema.NP1_Reference);
			aH_TransactionNumNode.AddChild(NettingPayableTransactionSchema.NPT_Reference);
			aH_TransactionNumNode.AddChild(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference);
			aH_TransactionNumNode.AddChild(NettingPayableTransactionRefSchema.NPR_Reference);
			aH_TransactionNumNode.AddChild(NettingReceivableLineReferenceSchema.NR1_Reference);
			aH_TransactionNumNode.AddChild(NettingReceivableTransactionSchema.NRT_Reference);
			aH_TransactionNumNode.AddChild(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference);
			aH_TransactionNumNode.AddChild(NettingReceivableTransactionRefSchema.NRR_Reference);
			aH_TransactionNumNode.AddChild(ViewMatchGroupSchema.MG_TransactionNum);
			aH_TransactionNumNode.AddChild(GenericTransactionSchema.VT_TransactionNo);

			var aH_ChequeOrReferenceNode = aH_TransactionNumNode.AddChild(AccTransactionHeaderSchema.AH_ChequeOrReference);
			aH_ChequeOrReferenceNode.AddChild(JobChargeSchema.JR_ChequeNo);
			aH_ChequeOrReferenceNode.AddChild(JobConsolCostSchema.E6_CostReference);
			aH_ChequeOrReferenceNode.AddChild(AccPaymentApprovalSchema.AV_ChequeOrReference);
			aH_ChequeOrReferenceNode.AddChild(JobChargeSchema.JR_CostReference);
			aH_ChequeOrReferenceNode.AddChild(AccStatementSchema.AS_ChequeOrReference);
			aH_ChequeOrReferenceNode.AddChild(JobConsolCostSchema.E6_ChequeOrReference);

			AssertMaxLength(jR_APInvoiceNumNode);
			AssertMaxLength(aH_TransactionNumNode);
			AssertMaxLength(aH_ChequeOrReferenceNode);
		}

		void AssertMaxLength(TreeNode<SchemaStringColumn> node)
		{
			var expectedMaxLength = node.GetTreeNodeItem().MaxLength;
			var listOfAffectedFields = new ZStringBuilder();
			foreach (var child in node.GetAllChildren())
			{
				if (child.GetTreeNodeItem().MaxLength < expectedMaxLength)
				{
					listOfAffectedFields.Append(child.GetTreeNodeItem().Name);
				}
			}

			var devMessage = string.Format("{0} max length is {1}. Following fields can be set from {0}, but their max length is smaller than {0} max length\r\n{2}",
				node.GetTreeNodeItem().Name, expectedMaxLength, listOfAffectedFields.ToStringWithNewLineBetweenAppends());
			Assert(devMessage, listOfAffectedFields.IsEmpty);
		}

		class TreeNode<T>
		{
			readonly HashSet<TreeNode<T>> Children;
			T Item { get; set; }

			public TreeNode(T item)
			{
				Item = item;
				Children = new HashSet<TreeNode<T>>();
			}

			public TreeNode<T> AddChild(T item)
			{
				TreeNode<T> nodeItem = new TreeNode<T>(item);
				Children.Add(nodeItem);
				return nodeItem;
			}

			public HashSet<TreeNode<T>> GetAllChildren()
			{
				var result = new HashSet<TreeNode<T>>();
				foreach (var child in Children)
				{
					result.Add(child);
					result.UnionWith(child.GetAllChildren());
				}
				return result;
			}

			public T GetTreeNodeItem()
			{
				return Item;
			}
		}

		#endregion

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccTransactionHeaderSchema.PK,
				AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccTransactionHeaderSchema.PK.TableSchema.SqlSchemaName, AccTransactionHeaderSchema.PK.TableName, columns);
		}

		public void TestAH_ConsolidatedInvoiceRef_Setter()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			Assert(header.AH_JobNumber.IsEmpty);
			Assert(header.AH_ConsolidatedInvoiceRef.IsEmpty);

			header.AH_ConsolidatedInvoiceRef = "S00001030/B";
			AssertEquals("S00001030/B", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("S00001030", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = "S00001030/AF";
			AssertEquals("S00001030/AF", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("S00001030", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = "S00001033";
			AssertEquals("S00001033", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("S00001033", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = string.Empty;
			Assert(header.AH_JobNumber.IsEmpty);
			Assert(header.AH_ConsolidatedInvoiceRef.IsEmpty);

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			Assert(header.AH_JobNumber.IsEmpty);
			Assert(header.AH_ConsolidatedInvoiceRef.IsEmpty);

			header.AH_ConsolidatedInvoiceRef = "S00001030/B";
			AssertEquals("S00001030/B", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = "S00001033";
			AssertEquals("S00001033", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = string.Empty;
			Assert(header.AH_JobNumber.IsEmpty);
			Assert(header.AH_ConsolidatedInvoiceRef.IsEmpty);
		}

		public void TestAH_ConsolidatedInvoiceRef_Setter_MaxLength()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			AssertExceptionThrown<MaxLengthExceededException>($@"The maximum length of 'AH_JobNumber' has been exceeded.
The maximum length of AH_JobNumber is 35 characters, but 37 were entered. New value: AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA.",
				() => header.AH_ConsolidatedInvoiceRef = new String('A', 37));
			ErrorReporter.Clear();

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertNoExceptionThrown(() => header.AH_ConsolidatedInvoiceRef = new String('A', 37));
			ErrorReporter.Clear();
		}

		public void TestAH_ConsolidatedInvoiceRef_Setter_WhenJobNumberContainsSlash()
		{
			var cartage = Factory.New<ICommonCartage>();
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			cartage.JJ_ConsignmentID = "S0001000/A";
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = "JJ";
			job.JH_JobNum = "S0001000/A";

			AssertEquals("job number has a slash", "S0001000/A", job.JH_JobNum);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_JH = job.PK;

			Assert(header.AH_JobNumber.IsEmpty);
			Assert(header.AH_ConsolidatedInvoiceRef.IsEmpty);

			header.AH_ConsolidatedInvoiceRef = "S0001000/A";
			AssertEquals("S0001000/A", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("S0001000/A", header.AH_JobNumber);

			header.AH_ConsolidatedInvoiceRef = "S0001000/A/A";
			AssertEquals("S0001000/A/A", header.AH_ConsolidatedInvoiceRef);
			AssertEquals("S0001000/A", header.AH_JobNumber);
		}

		public void TestSavingWithCriticalValidationException_1()
		{
			var header = GetHeaderToSave(Factory);

			header.AH_InvoiceAmount = 35;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception kk)
			{
				Fail("Should be a TransactionOnSavingCheckException" + kk.Message);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSavingWithCriticalValidationException_2()
		{
			var header = GetHeaderToSave(Factory);

			header.AH_InvoiceAmount = 25;
			header.AH_GSTAmount = 15;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception)
			{
				Fail("Should be an OnSavingCriticalCheckException");
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSavingWithCriticalValidationException_3()
		{
			AccTransactionMatchLink matchLink;
			var header = GetHeaderToSave(Factory, out matchLink);

			header.AH_GSTAmount = 5;
			matchLink.AP_Amount = 20;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception)
			{
				Fail("Should be an OnSavingCriticalCheckException");
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSavingWithCriticalValidationException_4()
		{
			AccTransactionMatchLink matchLink;
			var header = GetHeaderToSave(Factory, out matchLink);

			matchLink.AP_Amount = 10;
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_Ledger = "AR";
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_InvoiceAmount = 35;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception)
			{
				Fail("Should be an OnSavingCriticalCheckException");
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSavingWithCriticalValidationException_5()
		{
			var header = GetHeaderToSave(Factory);
			header.AH_InvoiceAmount = 25;
			header.AH_GSTAmount = 15;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception)
			{
				Fail("Should be a OnSavingCriticalCheckException");
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSavingWithCriticalValidationException_6()
		{
			AccTransactionMatchLink matchLink;

			var header = GetHeaderToSave(Factory, out matchLink);
			header.AH_GSTAmount = 5;
			matchLink.AP_Amount = 20;
			try
			{
				header.Factory.Save();
				Fail("Should throw an exeption");
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
			}
			catch (Exception)
			{
				Fail("Should be an OnSavingCriticalCheckException");
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSaving()
		{
			AccTransactionMatchLink matchLink;

			var header = GetHeaderToSave(Factory, out matchLink);
			matchLink.AP_Amount = 10;
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_Ledger = "CB";
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_InvoiceAmount = 35;
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_InvoiceAmount = 25;
			header.AH_GSTAmount = 15;
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			header.AH_GSTAmount = 5;
			matchLink.AP_Amount = 20;
			header.OnSaving();
			Assert(header.IsSavedByFactory);

			matchLink.AP_Amount = 10;
			header.OnSaving();
			Assert(header.IsSavedByFactory);
		}

		AccTransactionHeader GetHeaderToSave(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink matchLink;
			return GetHeaderToSave(factory, out matchLink);
		}

		AccTransactionHeader GetHeaderToSave(BusinessObjectFactory factory, out AccTransactionMatchLink matchLink)
		{
			AccTransactionHeader result = factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionLines line = factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = result.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			AccPaymentApproval paymentApproval = factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_AH = result.PK;
			IJobConsolCost cost = (IJobConsolCost)factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IJobConsolCost)));
			((BusinessObject)cost)[JobConsolCostSchema.E6_AH_APInvoice] = result.PK;
			result.AH_OutstandingAmount = 20;
			result.AH_Ledger = "AP";
			result.AH_InvoiceAmount = 25;
			result.AH_GSTAmount = 5;
			matchLink = factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_AH = result.PK;
			matchLink.AP_Amount = 10;
			result.OnSaving();
			Assert(result.IsSavedByFactory);

			return result;
		}

		public void TestAH_InvoiceTermDescriptionCalc()
		{
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_InvoiceTerm = "AAA";
			AssertEquals("AH_InvoiceTerm description for non valid term is empty", "", header.AH_InvoiceTermDescriptionCalc);

			header.AH_InvoiceTerm = Constants.InvoiceTerms.FromPeriodEnd;
			AssertEquals("From End of Period", header.AH_InvoiceTermDescriptionCalc);

			header.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals("Cash On Delivery", header.AH_InvoiceTermDescriptionCalc);
		}

		public void TestAH_IsDisbursementCalc()
		{
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			var headerAsEInvoicingLite = (IEInvoicingEligibilityLiteTransaction)header;

			header.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("It's not disbursement transaction", false, header.AH_IsDisbursementCalc);
			AssertEquals("It's not disbursement transaction (IEInvoicingEligibilityLiteTransaction)", false, headerAsEInvoicingLite.IsDisbursementInvoice());

			foreach (string invoiceType in new[] { InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTypesList.Codes.DisbursementInForeignCurrency, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching })
			{
				header.AH_TransactionCategory = invoiceType;
				Assert(string.Format("{0} is a disbursement transaction", invoiceType), header.AH_IsDisbursementCalc);
				Assert(string.Format("{0} is a disbursement transaction (IEInvoicingEligibilityLiteTransaction)", invoiceType), headerAsEInvoicingLite.IsDisbursementInvoice());
			}
		}

		public void TestHaveConstructorStackTrace()
		{
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			IHaveConstructorStackTrace hasTrace = header;
			AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);

			AssertNull("Should be no ConstructorStackTrace by default", hasTrace.ConstructorStackTrace);

			StackTrace trace = new StackTrace();
			hasTrace.ConstructorStackTrace = trace;

			AssertEquals("Should be assigned StackTrace", trace, hasTrace.ConstructorStackTrace);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				hasTrace = Factory.New<AccTransactionHeader>();
				AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
				AssertNotNull("Should have ConstructorStackTrace", hasTrace.ConstructorStackTrace);
				AssertContains("Trace should be as expected", trace.ToString(), hasTrace.ConstructorStackTrace.ToString());
			}
		}

		public void TestDisbursementInvoiceTypes()
		{
			AssertNotNull(AccTransactionHeader.DisbursementInvoiceTypes);
			AssertEquals("Length", 4, AccTransactionHeader.DisbursementInvoiceTypes.Length);
			Assert("Contains DisbursementInForeignCurrency", AccTransactionHeader.DisbursementInvoiceTypes.Contains(InvoiceTypesList.Codes.DisbursementInForeignCurrency));
			Assert("Contains DisbursementInvoice", AccTransactionHeader.DisbursementInvoiceTypes.Contains(InvoiceTypesList.Codes.DisbursementInvoice));
			Assert("Contains DisbursementInForeignCurrency_Batching", AccTransactionHeader.DisbursementInvoiceTypes.Contains(InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching));
			Assert("Contains DisbursementInvoice_Batching", AccTransactionHeader.DisbursementInvoiceTypes.Contains(InvoiceTypesList.Codes.DisbursementInvoice_Batching));
		}

		public void TestIsDisbursementInvoiceType()
		{
			AssertEquals("Null", false, AccTransactionHeader.IsDisbursementInvoiceType(null));
			AssertEquals("Empty string", false, AccTransactionHeader.IsDisbursementInvoiceType(string.Empty));
			AssertEquals("Random string", false, AccTransactionHeader.IsDisbursementInvoiceType("!@#"));

			foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
			{
				if (AccTransactionHeader.DisbursementInvoiceTypes.Contains(invoiceType.Code))
				{
					AssertEquals(invoiceType.Code + " should be disbursement invoice type", true, AccTransactionHeader.IsDisbursementInvoiceType(invoiceType.Code));
				}
				else
				{
					AssertEquals(invoiceType.Code + " should not be disbursement invoice type", false, AccTransactionHeader.IsDisbursementInvoiceType(invoiceType.Code));
				}
			}
		}

		public void TestIsIndonesianGovtComplianceInvoice()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			header.AH_ComplianceSubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_GSTAmount = 10m;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				AssertEquals("IsIndonesianGovtComplianceInvoice", true, header.IsIndonesianGovtComplianceInvoice);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("IsIndonesianGovtComplianceInvoice", false, header.IsIndonesianGovtComplianceInvoice);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				header.AH_GSTAmount = 0;
				AssertEquals("IsIndonesianGovtComplianceInvoice", false, header.IsIndonesianGovtComplianceInvoice);

				header.AH_GSTAmount = 10m;
				header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
				AssertEquals("IsIndonesianGovtComplianceInvoice", false, header.IsIndonesianGovtComplianceInvoice);

				header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
				AssertEquals("IsIndonesianGovtComplianceInvoice", false, header.IsIndonesianGovtComplianceInvoice);

				header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				header.AH_ComplianceSubType = "";
				AssertEquals("IsIndonesianGovtComplianceInvoice", false, header.IsIndonesianGovtComplianceInvoice);

				header.AH_ComplianceSubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("IsIndonesianGovtComplianceInvoice", true, header.IsIndonesianGovtComplianceInvoice);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestIsGovtTaxInvoice()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			AssertEquals("IsGovtTaxInvoice", false, header.IsGovtTaxInvoice);

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				orgHeader.OH_RL_NKClosestPort = "CNBJS";
				header.AH_OH = orgHeader.PK;
				AssertEquals("IsGovtTaxInvoice", false, header.IsGovtTaxInvoice);

				header.AH_OH = ZGuid.Empty;
				AssertEquals("IsGovtTaxInvoice", false, header.IsGovtTaxInvoice);

				header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				header.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
				AssertEquals("IsGovtTaxInvoice", true, header.IsGovtTaxInvoice);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				AssertEquals("IsGovtTaxInvoice", false, header.IsGovtTaxInvoice);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
				header.AH_GSTAmount = 10m;
				header.AH_ComplianceSubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("IsGovtTaxInvoice", true, header.IsGovtTaxInvoice);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestAdditionalCompanyName()
		{
			var expectedAdditionalCompanyName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsDebtor = true;
				orgHeader.Addresses.RemoveAndDeleteAll();

				var orgAddress = orgHeader.Addresses.AddNew();
				orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
				orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
				orgAddress.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
				orgAddress.CompanyName = expectedAdditionalCompanyName;
				orgAddress.AddressCapability.SetCapabilityEnabled("APM");
				orgAddress.AddressCapability.SetIsMainAddress("APM");
				orgAddress.AddressCapability.SetCapabilityEnabled("ARM");
				orgAddress.AddressCapability.SetIsMainAddress("ARM");

				Factory.Save();

				var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsPayable;
				transactionHeader.AH_OH = orgHeader.PK;

				AssertEquals("The value of transactionHeader.AdditionalCompanyName should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, transactionHeader.AdditionalCompanyName);

				var transactionHeader1 = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionHeader1.AH_OH = orgHeader.PK;

				AssertEquals("The value of transactionHeader1.AdditionalCompanyName should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, transactionHeader1.AdditionalCompanyName);
			}
		}

		public void TestZQueryWithAH_ConsolidatedInvoiceRefUseIsNonBlankFilteredIndexParticipantIsUsed()
		{
			Assert("AccTransactionHeader.AH_ConsolidatedInvoiceRef IsNonBlankFilteredIndexParticipant must be false to use the index NR_RX__AH_ConsolidatedInvoiceRef_AH_GC", !AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.IsNonBlankFilteredIndexParticipant);

			using (Db.Connection.TrackExecutedCommands())
			{
				var query = new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK).AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "BANANNAS");
				var result = Factory.Load<AccTransactionHeader>(query);

				var commands = Db.Connection.ExecutedCommands.Where(t => t.Contains("FROM dbo.AccTransactionHeader")).ToArray();
				AssertEquals(1, commands.Length);
				AssertNotContains("ZQuery should not add AH_ConsolidatedInvoiceRef <> '' filter in the query", "AH_ConsolidatedInvoiceRef <> ''", commands[0]);
			}
		}

		public void TestZQueryWithLikeStatementOnAH_ConsolidatedInvoiceRefAddBetweenClauseToFixPerformanceIssue()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				var uniqueRef = "S00001254";
				var query = new ZQuery();
				var jobUniqueRefQuery = new ZQuery();
				jobUniqueRefQuery.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, uniqueRef);
				jobUniqueRefQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, uniqueRef + "/");
				query.AddToFilter(jobUniqueRefQuery);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK);

				var result = Factory.Load<AccTransactionHeader>(query);

				var commands = Db.Connection.ExecutedCommands.Where(t => t.Contains("FROM dbo.AccTransactionHeader")).ToArray();
				AssertEquals(1, commands.Length);
				AssertContains("ZQuery should automatically add in the query a between clause on AH_ConsolidatedInvoiceRef", "AH_ConsolidatedInvoiceRef >=", commands[0]);
				AssertContains("ZQuery should automatically add in the query a between clause on AH_ConsolidatedInvoiceRef", "AH_ConsolidatedInvoiceRef <=", commands[0]);
			}
		}

		public void TestNR_UX__AH_GC_AH_Ledger_AH_TransactionType_AH_TransactionNum_AH_TransactionCount()
		{
			bool isExceptionThrown = false;

			DateTime date = new DateTime(2008, 1, 1);
			OrgHeader creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			try
			{
				TestHelper.PostTransaction("AR", "INV", "00001001", 100, 10, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor1.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
				TestHelper.PostTransaction("AR", "INV", "00001001", 200, 20, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor2.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
			}
			catch (SqlException ex)
			{
				isExceptionThrown = ex.Number == 2601 && ex.Message.Contains("Cannot insert duplicate key row in object 'dbo.AccTransactionHeader' with unique index 'NR_UX__AH_GC_AH_Ledger_AH_TransactionType_AH_TransactionNum_AH_TransactionCount'");
			}
			Assert(isExceptionThrown);

			AssertNoExceptionThrown(delegate
			{
				TestHelper.PostTransaction("AP", "INV", "00001001", 100, 10, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor1.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
				TestHelper.PostTransaction("AP", "INV", "00001001", 200, 20, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor2.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
			});

			AssertNoExceptionThrown(delegate
			{
				TestHelper.PostTransaction("UA", "INV", "00001001", 100, 10, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor1.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
				TestHelper.PostTransaction("UA", "INV", "00001001", 200, 20, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor2.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
			});

			AssertNoExceptionThrown(delegate
			{
				TestHelper.PostTransaction("IN", "INV", "00001001", 100, 10, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor1.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
				TestHelper.PostTransaction("IN", "INV", "00001001", 200, 20, 0, Env.CurrentBranch.PK, date, Guid.Empty, false, "AUD", "", creditor2.PK.ToGuid(), date, Env.CurrentDepartment.PK, ZDateTime.Empty, 1);
			});
		}

		public void TestResetAddressContactOnOrgChange()
		{
			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_OH = ZGuid.NewZGuid();
			var expectedAddress = ZGuid.NewZGuid();
			var expectedContact = ZGuid.NewZGuid();
			transaction.AH_OA_InvoiceAddressOverride = expectedAddress;
			transaction.AH_OC_InvoiceContactOverride = expectedContact;

			AssertEquals("Precondition: AH_OA_InvoiceAddressOverride", expectedAddress, transaction.AH_OA_InvoiceAddressOverride);
			AssertEquals("Precondition: AH_OC_InvoiceContactOverride", expectedContact, transaction.AH_OC_InvoiceContactOverride);

			transaction.AH_OH = transaction.AH_OH;
			AssertEquals("Don't change AH_OA_InvoiceAddressOverride", expectedAddress, transaction.AH_OA_InvoiceAddressOverride);
			AssertEquals("Don't change AH_OC_InvoiceContactOverride", expectedContact, transaction.AH_OC_InvoiceContactOverride);

			transaction.AH_OH = ZGuid.NewZGuid();
			AssertEquals("Reset AH_OA_InvoiceAddressOverride", ZGuid.Empty, transaction.AH_OA_InvoiceAddressOverride);
			AssertEquals("Reset AH_OC_InvoiceContactOverride", ZGuid.Empty, transaction.AH_OC_InvoiceContactOverride);
		}

		public void TestIsCreatedByENett()
		{
			var header = GetHeaderToSave(Factory);
			AssertEquals("IsCreatedByENett default", false, header.IsCreatedByENett);
			header.IsCreatedByENett = true;
			AssertEquals("IsCreatedByENett set True", true, header.IsCreatedByENett);
			header.IsCreatedByENett = false;
			AssertEquals("IsCreatedByENett set False", false, header.IsCreatedByENett);
		}

		public void TestIsAlreadyReversedByOtherUser()
		{
			//Creating a Transaction Header 
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			Factory.Save();
			Assert("Transaction is not reversed", !header.IsAlreadyReversedByOtherUser());

			//Loading into another Factory and trying to Reverse
			var dbCommand = Db.Connection.Command(string.Format("UPDATE {0} SET {2} = @IsCancelled, {3} = GETUTCDATE(), {4} = 'TST' WHERE {1} = @PK", AccTransactionHeaderSchema.Constants.TableName, AccTransactionHeaderSchema.Constants.PK, AccTransactionHeaderSchema.Constants.AH_IsCancelled, AccTransactionHeaderSchema.Constants.AH_SystemLastEditTimeUtc, AccTransactionHeaderSchema.Constants.AH_SystemLastEditUser));
			dbCommand.AddParameter("@PK", SqlDbType.UniqueIdentifier, header.PK.ToGuid());
			dbCommand.AddParameter("@IsCancelled", SqlDbType.Bit, true);
			dbCommand.ExecuteNonQuery();

			Assert("Transaction is reversed", header.IsAlreadyReversedByOtherUser());
		}

		public void TestLedgerAndTransactionTypeConcurrency()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var headerInFactory1 = factory1.NewWithValidTestData<AccTransactionHeader>();
			headerInFactory1.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			headerInFactory1.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			factory1.Save();

			var headerInFactory2 = factory2.Load<AccTransactionHeader>(headerInFactory1.PK);
			headerInFactory2.AH_Ledger = LedgerTypes.AccountsPayable;
			headerInFactory2.AH_TransactionType = TransactionTypes.Invoice;
			factory2.Save();

			headerInFactory1.AH_Ledger = LedgerTypes.AccountsReceivable;
			headerInFactory1.AH_TransactionType = TransactionTypes.CreditNote;
			AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
		}

		public void TestReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			var actualMessage = service.GetInfo(header.PK, CriticalValidationInfoCollectorServiceKeyType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges);
			AssertContains("ReversedTransactionsShouldNotHaveLinesLinkedToCharges: There is no data collected for this PK.", actualMessage);

			header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			Factory.Save();

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			actualMessage = service.GetInfo(header.PK, CriticalValidationInfoCollectorServiceKeyType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges);
			AssertContains("set_AH_Ledger", actualMessage);

			service.ClearServiceCache();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			actualMessage = service.GetInfo(header.PK, CriticalValidationInfoCollectorServiceKeyType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges);
			AssertContains("ReversedTransactionsShouldNotHaveLinesLinkedToCharges: There is no data collected for this PK.", actualMessage);
		}

		public void TestIsAPTransactionConvertedFromIncompleteTransaction()
		{
			var headerINI = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerINI.AH_Ledger = LedgerTypes.IncompleteTransactions;
			headerINI.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			var headerINC = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerINC.AH_Ledger = LedgerTypes.IncompleteTransactions;
			headerINC.AH_TransactionType = TransactionTypes.IncompleteCreditNote;

			var headerINA = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerINA.AH_Ledger = LedgerTypes.IncompleteTransactions;
			headerINA.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;
			Factory.Save();

			headerINI.AH_Ledger = LedgerTypes.AccountsPayable;
			headerINI.AH_TransactionType = TransactionTypes.Invoice;
			Assert(headerINI.IsAPTransactionConvertedFromIncompleteTransaction);
			headerINI.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerINI.IsAPTransactionConvertedFromIncompleteTransaction);

			headerINC.AH_Ledger = LedgerTypes.AccountsPayable;
			headerINC.AH_TransactionType = TransactionTypes.CreditNote;
			Assert(headerINC.IsAPTransactionConvertedFromIncompleteTransaction);
			headerINC.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerINC.IsAPTransactionConvertedFromIncompleteTransaction);

			headerINA.AH_Ledger = LedgerTypes.AccountsPayable;
			headerINA.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Assert(headerINA.IsAPTransactionConvertedFromIncompleteTransaction);
			headerINA.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerINA.IsAPTransactionConvertedFromIncompleteTransaction);
		}

		public void TestIsAPTransactionConvertedFromUnapprovedTransaction()
		{
			var headerUAI = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerUAI.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			headerUAI.AH_TransactionType = TransactionTypes.UAInvoice;

			var headerUAC = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerUAC.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			headerUAC.AH_TransactionType = TransactionTypes.UACreditNote;
			Factory.Save();

			headerUAI.AH_Ledger = LedgerTypes.AccountsPayable;
			headerUAI.AH_TransactionType = TransactionTypes.Invoice;
			Assert(headerUAI.IsAPTransactionConvertedFromUnapprovedTransaction);
			headerUAI.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerUAI.IsAPTransactionConvertedFromUnapprovedTransaction);

			headerUAC.AH_Ledger = LedgerTypes.AccountsPayable;
			headerUAC.AH_TransactionType = TransactionTypes.CreditNote;
			Assert(headerUAC.IsAPTransactionConvertedFromUnapprovedTransaction);
			headerUAC.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerUAC.IsAPTransactionConvertedFromUnapprovedTransaction);
		}

		public void TestIsAPTransactionConvertedFromTransactionPendingAllocation()
		{
			var headerIPA = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerIPA.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			headerIPA.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;

			var headerCPA = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerCPA.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			headerCPA.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			Factory.Save();

			headerIPA.AH_Ledger = LedgerTypes.AccountsPayable;
			headerIPA.AH_TransactionType = TransactionTypes.Invoice;
			Assert(headerIPA.IsAPTransactionConvertedFromTransactionPendingAllocation);
			headerIPA.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerIPA.IsAPTransactionConvertedFromTransactionPendingAllocation);

			headerCPA.AH_Ledger = LedgerTypes.AccountsPayable;
			headerCPA.AH_TransactionType = TransactionTypes.CreditNote;
			Assert(headerCPA.IsAPTransactionConvertedFromTransactionPendingAllocation);
			headerCPA.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!headerCPA.IsAPTransactionConvertedFromTransactionPendingAllocation);
		}

		public void TestAH_LocalTotal()
		{
			var command = Db.Connection.Command($"SELECT definition FROM sys.computed_columns WHERE name = 'AH_LocalTotal'");
			var expression = (string)command.ExecuteScalar();
			AssertEquals("If AH_LocalTotal db column expression is changed, please change AH_LocalTotal implementation in this class to be the same as in SQL", AccTransactionHeader.AH_LocalTotalSQLFormula, expression);

			var invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_InvoiceAmount = 100;
			AssertEquals(100m, invoice.AH_LocalTotal);

			invoice.AH_GSTAmount = 20;
			AssertEquals(120m, invoice.AH_LocalTotal);

			invoice.AH_LocalTaxAmountOtherTaxes = 3;
			AssertEquals(123m, invoice.AH_LocalTotal);

			AssertEquals("Decimal places", 2, CargoWise.ComponentModel.MetaData.GetDecimalPlaces(invoice, invoice.AH_LocalTotalInfo.PropertyDescriptor));

			invoice.Company.GC_RX_NKLocalCurrency = CurrencyCodes.Japan;
			AssertEquals("Decimal places", 0, CargoWise.ComponentModel.MetaData.GetDecimalPlaces(invoice, invoice.AH_LocalTotalInfo.PropertyDescriptor));
		}

		public void TestZDecimalsHaveCorrectDecimalPlaces_WhenTransactionCurrencyIsNull()
		{
			var invoice = Factory.New<AccTransactionHeader>();
			AssertNull("Precondition", invoice.TransactionCurrency);

			var localList = new List<string>
			{
				nameof(invoice.AH_OutstandingAmount),
				nameof(invoice.AH_LocalTotal),
				nameof(invoice.AH_GSTAmount),
				nameof(invoice.AH_InvoiceAmount),
				nameof(invoice.AH_WithholdingTax),
				nameof(invoice.AH_LocalTaxAmountOtherTaxes),
			};

			var osList = new List<string>
			{
				nameof(invoice.AH_OSOutstandingAmount),
				nameof(invoice.AH_OSTotal),
				nameof(invoice.AH_OSTaxAmountOtherTaxes),
			};

			var tester = new DecimalPlacesAttributeTester(invoice);
			tester.CheckCompanyLocalCurrency(localList, nameof(invoice.LocalCurrencyDecimals));
			tester.CheckCompanyLocalCurrency(osList, nameof(invoice.OSCurrencyDecimals));
		}

		public void TestZDecimalsHaveCorrectDecimalPlaces_WhenTransactionCurrencyIsNotNull()
		{
			var invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			AssertNotNull("Precondition", invoice.TransactionCurrency);

			var localList = new List<string>
			{
				nameof(invoice.AH_OutstandingAmount),
				nameof(invoice.AH_LocalTotal),
				nameof(invoice.AH_GSTAmount),
				nameof(invoice.AH_InvoiceAmount),
				nameof(invoice.AH_WithholdingTax),
				nameof(invoice.AH_LocalTaxAmountOtherTaxes),
			};

			var osList = new List<string>
			{
				nameof(invoice.AH_OSOutstandingAmount),
				nameof(invoice.AH_OSTotal),
				nameof(invoice.AH_OSTaxAmountOtherTaxes),
			};

			var tester = new DecimalPlacesAttributeTester(invoice);
			tester.CheckLocalCurrency(localList, nameof(invoice.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(invoice.OSCurrencyDecimals), nameof(invoice.AH_RX_NKTransactionCurrency), invoice);
		}

		public void TestComplianceSubTypeFilterByLedger()
		{
			ComplianceSubTypeList mockedComplianceSubTypeList = new ComplianceSubTypeList();
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXV", () => (NoResString)"Purchase Tax Voucher", () => "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", () => ""));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TCD", () => (NoResString)"Debit Note", () => "Nota de D\u00e9bito", () => "", LedgerOfUse.ALL));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXI", () => (NoResString)"Tax Invoice", () => "Factura", () => "", LedgerOfUse.AR));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("XCL", () => (NoResString)"Reimbursement/Disbursement/Excluded Supply", () => "Liquidaci\u00f3n de Reembolso", () => "", LedgerOfUse.AP));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(x => x.GetComplianceSubTypes()).Returns(mockedComplianceSubTypeList);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);

			var tXVDescription = new CodeDescriptionPair("TXV", "Purchase Tax Voucher");
			var tCDDescription = new CodeDescriptionPair("TCD", "Debit Note");
			var tXIDescription = new CodeDescriptionPair("TXI", "Tax Invoice");
			var xCLDescription = new CodeDescriptionPair("XCL", "Reimbursement/Disbursement/Excluded Supply");
			var tXVLocalDescription = new CodeDescriptionPair("TXV", "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios");
			var tCDLocalDescription = new CodeDescriptionPair("TCD", "Nota de D\u00e9bito");
			var tXILocalDescription = new CodeDescriptionPair("TXI", "Factura");
			var xCLLocalDescription = new CodeDescriptionPair("XCL", "Liquidaci\u00f3n de Reembolso");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = LedgerTypes.AccountsReceivable;
				AssertContainsExactElementsInAnyOrder(new[] { tXIDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXILocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_Ledger = LedgerTypes.AccountsPayable;
				AssertContainsExactElementsInAnyOrder(new[] { xCLDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_Ledger = LedgerTypes.IncompleteTransactions;
				AssertContainsExactElementsInAnyOrder(new[] { xCLDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_Ledger = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new[] { tXIDescription, xCLDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXILocalDescription, xCLLocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);
			}
		}

		public void TestCanUpdateTransactionNumber()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();

			var header1 = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Payment);
			AssertEquals(false, header1.CanUpdateTransactionNumber());

			var header2 = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			AssertEquals(true, header2.CanUpdateTransactionNumber());

			var header3 = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
			AssertEquals(true, header3.CanUpdateTransactionNumber());

			var header4 = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote);
			AssertEquals(true, header4.CanUpdateTransactionNumber());

			var header5 = creator.CreateTransaction(Factory, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);
			header5.AH_IsCancelled = false;
			Factory.Save();
			AssertEquals(false, header5.CanUpdateTransactionNumber());
			header5.AH_IsCancelled = true;
			AssertEquals(true, header5.CanUpdateTransactionNumber());

			var header6 = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertEquals(false, header6.CanUpdateTransactionNumber());
			header6.SetContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim);
			AssertEquals(true, header6.CanUpdateTransactionNumber());
		}

		#region IEInvoicingEligibilityLiteTransaction

		public void TestIEInvoicingEligibilityLiteTransactionMembers()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var asEligibility = (IEInvoicingEligibilityLiteTransaction)transaction;

			AssertEquals("GC_RN_NKCountryCode / CountryCode", transaction.Company.GC_RN_NKCountryCode, asEligibility.CountryCode);
			AssertEquals("AH_Ledger / Ledger", transaction.AH_Ledger, asEligibility.Ledger);
			AssertEquals("AH_TransactionType / TransactionType", transaction.AH_TransactionType, asEligibility.TransactionType);
			AssertEquals("AH_ComplianceSubType / ComplianceSubType", transaction.AH_ComplianceSubType, asEligibility.ComplianceSubType);
			AssertEquals("AH_TransactionReference / ComplianceNumber", transaction.AH_TransactionReference, asEligibility.ComplianceNumber);
			AssertEquals("AH_TransactionCategory / TransactionCategory", transaction.AH_TransactionCategory, asEligibility.TransactionCategory);
			AssertEquals("AH_PlaceOfSupply / PlaceOfSupply", transaction.AH_PlaceOfSupply, asEligibility.PlaceOfSupply);
			AssertEquals("AH_GC / CompanyPK", transaction.AH_GC, asEligibility.CompanyPK);
			AssertEquals("AH_GB / BranchPK", transaction.AH_GB, asEligibility.BranchPK);
			AssertEquals("AH_InvoiceAmount / InvoiceAmount", transaction.AH_InvoiceAmount, asEligibility.InvoiceAmount);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				transaction.AH_Ledger = "AR";
				transaction.AH_TransactionType = "INV";
				transaction.AH_ComplianceSubType = "TXI";
				transaction.AH_TransactionReference = "ABC12345678";
				transaction.AH_TransactionCategory = "DBT";
				transaction.AH_PlaceOfSupply = "ALX";
				AssertEquals("GC_RN_NKCountryCode / CountryCode", "US", asEligibility.CountryCode);
				AssertEquals("AH_Ledger / Ledger", "AR", asEligibility.Ledger);
				AssertEquals("AH_TransactionType / TransactionType", "INV", asEligibility.TransactionType);
				AssertEquals("AH_ComplianceSubType / ComplianceSubType", "TXI", asEligibility.ComplianceSubType);
				AssertEquals("AH_TransactionReference / ComplianceNumber", "ABC12345678", asEligibility.ComplianceNumber);
				AssertEquals("AH_TransactionCategory / TransactionCategory", "DBT", asEligibility.TransactionCategory);
				AssertEquals("AH_PlaceOfSupply / PlaceOfSupply", "ALX", asEligibility.PlaceOfSupply);
			}

			transaction.AH_GC = ZGuid.Empty;
			transaction.AH_GB = ZGuid.Empty;
			transaction.AH_Ledger = "AP";
			transaction.AH_TransactionType = "CRD";
			transaction.AH_ComplianceSubType = "";
			transaction.AH_TransactionReference = "";
			transaction.AH_TransactionCategory = "";
			transaction.AH_PlaceOfSupply = "";
			transaction.AH_InvoiceAmount = 0m;
			AssertEquals("GC_RN_NKCountryCode / CountryCode", "", asEligibility.CountryCode);
			AssertEquals("AH_Ledger / Ledger", "AP", asEligibility.Ledger);
			AssertEquals("AH_TransactionType / TransactionType", "CRD", asEligibility.TransactionType);
			AssertEquals("AH_ComplianceSubType / ComplianceSubType", "", asEligibility.ComplianceSubType);
			AssertEquals("AH_TransactionReference / ComplianceNumber", "", asEligibility.ComplianceNumber);
			AssertEquals("AH_TransactionCategory / TransactionCategory", "", asEligibility.TransactionCategory);
			AssertEquals("AH_PlaceOfSupply / PlaceOfSupply", "", asEligibility.PlaceOfSupply);
			AssertEquals("AH_GC / CompanyPK", ZGuid.Empty, asEligibility.CompanyPK);
			AssertEquals("AH_GB / BranchPK", ZGuid.Empty, asEligibility.BranchPK);
			AssertEquals("AH_InvoiceAmount / InvoiceAmount", 0m, asEligibility.InvoiceAmount);

			var orgHdr = Factory.NewWithValidTestData<OrgHeader>();
			orgHdr.CustomsCodes.AddNew("GST", "GST12345678", "AU");

			transaction.AH_OH = orgHdr.PK;
			AssertEquals("AH_OH / OrgHeader RegistrationCode Count", 1, asEligibility.OrgHeader.RegistrationCodes.Count);
			Assert("AH_OH / OrgHeader Has GST for AU", asEligibility.OrgHeader.HasAnyRegistrationCode("GST", "AU"));

			transaction.AH_OH = ZGuid.Empty;
			AssertNotNull("AH_OH / OrgHeader", asEligibility.OrgHeader);
			AssertNotNull("AH_OH / OrgHeader RegistrationCodes", asEligibility.OrgHeader.RegistrationCodes);
			AssertEquals("AH_OH / OrgHeader RegistrationCode Count", 0, asEligibility.OrgHeader.RegistrationCodes.Count);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = orgHdr.PK;
			transaction.AH_GC = company.PK;
			AssertEquals("AH_GC / CompanyOrgProxy RegistrationCode Count", 1, asEligibility.CompanyOrgProxy.RegistrationCodes.Count);
			Assert("AH_GC / CompanyOrgProxy Has GST for AU", asEligibility.CompanyOrgProxy.HasAnyRegistrationCode("GST", "AU"));
			AssertEquals("AH_GC / CompanyPK", company.PK, asEligibility.CompanyPK);

			transaction.AH_GC = ZGuid.Empty;
			AssertNotNull("AH_GC / CompanyOrgProxy", asEligibility.CompanyOrgProxy);
			AssertNotNull("AH_GC / CompanyOrgProxy RegistrationCodes", asEligibility.CompanyOrgProxy.RegistrationCodes);
			AssertEquals("AH_GC / CompanyOrgProxy RegistrationCode Count", 0, asEligibility.CompanyOrgProxy.RegistrationCodes.Count);
			AssertEquals("AH_GC / CompanyPK", ZGuid.Empty, asEligibility.CompanyPK);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = orgHdr.PK;
			transaction.AH_GB = branch.PK;
			AssertEquals("AH_GB / BranchOrgProxy RegistrationCode Count", 1, asEligibility.BranchOrgProxy.RegistrationCodes.Count);
			Assert("AH_GB / BranchOrgProxy Has GST for AU", asEligibility.BranchOrgProxy.HasAnyRegistrationCode("GST", "AU"));
			AssertEquals("AH_GB / BranchPK", branch.PK, asEligibility.BranchPK);

			transaction.AH_GB = ZGuid.Empty;
			AssertNotNull("AH_GB / BranchOrgProxy", asEligibility.BranchOrgProxy);
			AssertNotNull("AH_GB / BranchOrgProxy RegistrationCodes", asEligibility.BranchOrgProxy.RegistrationCodes);
			AssertEquals("AH_GB / BranchOrgProxy RegistrationCode Count", 0, asEligibility.BranchOrgProxy.RegistrationCodes.Count);
			AssertEquals("AH_GB / BranchPK", ZGuid.Empty, asEligibility.BranchPK);

			transaction.AH_InvoiceAmount = 1000m;
			AssertEquals("AH_InvoiceAmount / InvoiceAmount", 1000m, asEligibility.InvoiceAmount);

			transaction.AH_InvoiceAmount = 0m;
			AssertEquals("AH_InvoiceAmount / InvoiceAmount", 0m, asEligibility.InvoiceAmount);
		}

		public void TestMakeOSOutstandingAmountApplicable()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();

			AssertEquals("Pre-condition", false, transaction.AH_IsOSOutstandingAmountApplicable);
			AssertEquals(0m, transaction.AH_OSOutstandingAmount);

			transaction.MakeOSOutstandingAmountApplicable(190m);

			AssertEquals(true, transaction.AH_IsOSOutstandingAmountApplicable);
			AssertEquals(190m, transaction.AH_OSOutstandingAmount);
		}

		#endregion

		public void TestComplianceSubTypeFilterByTransactionType()
		{
			ComplianceSubTypeList mockedComplianceSubTypeList = new ComplianceSubTypeList();
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXV", () => (NoResString)"Purchase Tax Voucher", () => "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", () => ""));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TCD", () => (NoResString)"Debit Note", () => "Nota de D\u00e9bito", () => "", transactionType: TransactionTypeOfUse.ALL));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXI", () => (NoResString)"Tax Invoice", () => "Factura", () => "", transactionType: TransactionTypeOfUse.INV));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("XCL", () => (NoResString)"Reimbursement/Disbursement/Excluded Supply", () => "Liquidaci\u00f3n de Reembolso", () => "", transactionType: TransactionTypeOfUse.CRD));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(x => x.GetComplianceSubTypes()).Returns(mockedComplianceSubTypeList);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);

			var tXVDescription = new CodeDescriptionPair("TXV", "Purchase Tax Voucher");
			var tCDDescription = new CodeDescriptionPair("TCD", "Debit Note");
			var tXIDescription = new CodeDescriptionPair("TXI", "Tax Invoice");
			var xCLDescription = new CodeDescriptionPair("XCL", "Reimbursement/Disbursement/Excluded Supply");
			var tXVLocalDescription = new CodeDescriptionPair("TXV", "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios");
			var tCDLocalDescription = new CodeDescriptionPair("TCD", "Nota de D\u00e9bito");
			var tXILocalDescription = new CodeDescriptionPair("TXI", "Factura");
			var xCLLocalDescription = new CodeDescriptionPair("XCL", "Liquidaci\u00f3n de Reembolso");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_TransactionType = TransactionTypes.Invoice;
				AssertContainsExactElementsInAnyOrder(new[] { tXIDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXILocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_TransactionType = TransactionTypes.CreditNote;
				AssertContainsExactElementsInAnyOrder(new[] { xCLDescription, tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_TransactionType = TransactionTypes.AdjustmentNote;
				AssertContainsExactElementsInAnyOrder(new[] { tXVDescription, tCDDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXVLocalDescription, tCDLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);

				header.AH_TransactionType = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new[] { tXVDescription, tCDDescription, tXIDescription, xCLDescription }, header.ComplianceSubTypeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXVLocalDescription, tCDLocalDescription, tXILocalDescription, xCLLocalDescription }, header.ComplianceSubTypeInLocalLanguageList);
			}
		}

		public void TestDisableWorkflowSettingPropertiesAfterOnSavingAttribute()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			var componentType = header.GetType();
			var ignoreBoAttributes = componentType.GetCustomAttributes(typeof(DisableWorkflowSettingPropertiesAfterOnSavingAttribute), true);
			AssertEquals(1, ignoreBoAttributes.Length);
			AssertNotNull("DisableWorkflowSettingPropertiesAfterOnSaving Attribute", ignoreBoAttributes[0] as DisableWorkflowSettingPropertiesAfterOnSavingAttribute);
		}

		public void TestCreator()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "XXX";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_SystemCreateUser = "XXX";
			AssertEquals(testUser, header.Creator);
		}

		#region GetComplianceAllocationMethodAR()

		public void TestGetComplianceAllocationMethodAR_MatchesOnBranchLevel()
		{
			var currentBranchPk = Env.CurrentBranchPK;
			var otherBranchPk = GlbCompany.CurrentCompany.Branches.First(b => b.PK != currentBranchPk).PK;
			var thirdBranchPk = GlbCompany.CurrentCompany.Branches.First(b => b.PK != currentBranchPk && b.PK != otherBranchPk).PK;

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var nfeAndBranch = collection.AddNew();
			nfeAndBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			nfeAndBranch.BranchPK = Env.CurrentBranchPK;
			nfeAndBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var nfeAndBranch2 = collection.AddNew();
			nfeAndBranch2.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			nfeAndBranch2.BranchPK = otherBranchPk;
			nfeAndBranch2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var nfeNoBranch = collection.AddNew();
			nfeNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			nfeNoBranch.BranchPK = ZGuid.Empty;
			nfeNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate;

			AssertEquals("Precondition: default allocation method is Print",
				AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
			);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
						Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
						ComplianceDocumentNumberAllocationRuleTypes.LBD.Code))
				{
					var transactionNfe = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionNfe.AH_Ledger = LedgerTypes.AccountsReceivable;
					transactionNfe.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
					var allocationMethod = transactionNfe.GetComplianceAllocationMethodAR();
					AssertEquals("Allocation method should match on sub-type and current login branch", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

					var transactionNfeAndBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionNfeAndBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
					transactionNfeAndBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
					transactionNfeAndBranch.AH_GB = otherBranchPk;
					allocationMethod = transactionNfeAndBranch.GetComplianceAllocationMethodAR();
					AssertEquals("Allocation method should match on sub-type and current login branch (LBD registry)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

					allocationMethod = transactionNfeAndBranch.GetComplianceAllocationMethodARThisTransaction();
					AssertEquals("Allocation method should match on sub-type and current login branch (this transaction)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);
				}

				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
						Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
						ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
				{
					var transactionNfe = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionNfe.AH_Ledger = LedgerTypes.AccountsReceivable;
					transactionNfe.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
					var allocationMethod = transactionNfe.GetComplianceAllocationMethodAR();
					AssertEquals("Allocation method should match on sub-type and current login branch", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

					var transactionNfeAndBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionNfeAndBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
					transactionNfeAndBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
					transactionNfeAndBranch.AH_GB = otherBranchPk;
					allocationMethod = transactionNfeAndBranch.GetComplianceAllocationMethodAR();
					AssertEquals("Allocation method should match on sub-type and transaction branch (HBD registry)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);

					var transactionNfeAnotherBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionNfeAnotherBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
					transactionNfeAnotherBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
					transactionNfeAnotherBranch.AH_GB = thirdBranchPk;
					allocationMethod = transactionNfeAnotherBranch.GetComplianceAllocationMethodAR();
					AssertEquals("Allocation method should match on sub-type as transaction branch (HBD registry) does not match any rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, allocationMethod);
				}
			}
		}

		public void TestGetComplianceAllocationMethodAR_MatchesOnSubTypeLevel()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var nfeAndBranch = collection.AddNew();
			nfeAndBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			nfeAndBranch.BranchPK = ZGuid.Empty;
			nfeAndBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var nfsNoBranch = collection.AddNew();
			nfsNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsNoBranch.BranchPK = ZGuid.Empty;
			nfsNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			AssertEquals("Precondition: default allocation method is Print",
				AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
			);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				var transactionNfeAndBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionNfeAndBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionNfeAndBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
				var allocationMethod = transactionNfeAndBranch.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should match on sub-type and branch", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

				var transactionNfsNoBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionNfsNoBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionNfsNoBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				allocationMethod = transactionNfsNoBranch.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should match on sub-type only", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);

				var transactionXndNoBranch = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionXndNoBranch.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionXndNoBranch.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
				allocationMethod = transactionXndNoBranch.GetComplianceAllocationMethodAR();
				AssertEquals("When no rule matches, should fall back to previous registry", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, allocationMethod);

				var transactionWithoutSubtype = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionWithoutSubtype.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionWithoutSubtype.AH_ComplianceSubType = ZString.Empty;
				allocationMethod = transactionWithoutSubtype.GetComplianceAllocationMethodAR();
				AssertEquals("When sub-type is empty, should fall back to previous registry", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, allocationMethod);
			}
		}

		public void TestGetComplianceAllocationMethodAR_MatchesUsingSubTypeDependencyRegistry()
		{
			var dependencyCollection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.Value;
			// Regular dependency XNC > XND
			var dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;

			// Dependency chain NFS > NFE > CNS
			dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var xndAndNoBranch = collection.AddNew();
			xndAndNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			xndAndNoBranch.BranchPK = ZGuid.Empty;
			xndAndNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var cnsAndNoBranch = collection.AddNew();
			cnsAndNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			cnsAndNoBranch.BranchPK = ZGuid.Empty;
			cnsAndNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			AssertEquals("Precondition: default allocation method is Print",
				AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
			);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dependencyCollection))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				var transactionCne = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionCne.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionCne.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
				var allocationMethod = transactionCne.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should use default when no override rules apply", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, allocationMethod);

				var transactionXnd = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionXnd.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionXnd.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
				allocationMethod = transactionXnd.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should use override rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

				var transactionXnc = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionXnc.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionXnc.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
				allocationMethod = transactionXnc.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should traverse dependency chain to determine override rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, allocationMethod);

				var transactionCns = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionCns.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionCns.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
				allocationMethod = transactionCns.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should use override rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);

				var transactionNfe = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionNfe.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionNfe.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
				allocationMethod = transactionNfe.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should traverse partial dependency chain to determine override rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);

				var transactionNfs = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionNfs.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionNfs.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				allocationMethod = transactionNfs.GetComplianceAllocationMethodAR();
				AssertEquals("Allocation method should traverse entire dependency chain to determine override rule", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, allocationMethod);
			}
		}

		#endregion

		MasterFilesTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new MasterFilesTestHelper(Factory)); }
		}

		MasterFilesTestHelper testHelper;
	}
}

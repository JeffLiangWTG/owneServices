using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceHeader))]
	public class AccDraftInvoiceHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPostedTransactionHeader()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals("Pre-condition", AccDraftInvoiceHeaderStatus.Draft, draftInvoice.AIH_Status);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.Invoice;
			draftInvoice.AIH_AH_PostedTransactionHeader = header.PK;

			AssertEquals("The draft invoice should be marked as processed once linked to a posted invoice.",
				AccDraftInvoiceHeaderStatus.Processed, draftInvoice.AIH_Status);

			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;

			AssertEquals("The draft invoice should be approved for posting if un-posted.",
				AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);
		}

		public void TestAIH_Status_STUEventIsAddedAfterChange()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var mockStatusChangeHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockStatusChangeHandler.Object);
			mockStatusChangeHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Analyzing)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });

			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Analyzing;
			Factory.Save();

			var logs = draftInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("Status Updated | FROM: DFT | TO: ANL", logs.First().SL_Reference);
		}

		public void TestAIH_Status_STUEventIsAddedAfterWorkflowTriggeredChange()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var mockStatusChangeHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockStatusChangeHandler.Object);
			mockStatusChangeHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Analyzing)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });

			var trigger = draftInvoice.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "AIH_Status";
			action.PQ_FieldValue = AccDraftInvoiceHeaderStatus.Analyzing;

			Factory.Save();

			draftInvoice.Logs.AddNew(Events.CustomisableEvent01);
			AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);

			Factory.Save();

			var logs = draftInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("Status Updated | FROM: DFT | TO: ANL", logs.First().SL_Reference);
		}

		public void TestPostingStatus_CAN()
		{
			AssertPostingStatus(header =>
			{
				header.AH_IsCancelled = true;
			}, "CAN");
		}

		public void TestPostingStatus_PIC()
		{
			AssertPostingStatus(header =>
			{
				header.AH_Ledger = LedgerTypes.IncompleteTransactions;
				header.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			}, "PIC");
		}

		public void TestPostingStatus_PUA()
		{
			AssertPostingStatus(header =>
			{
				header.AH_Ledger = LedgerTypes.IncompleteTransactions;
				header.AH_TransactionType = TransactionTypes.IncompleteInvoice;

				var approvalRequest = Factory.NewWithValidTestData<GenApprovalRequest>();
				approvalRequest.XP_ParentID = header.PK;
			}, "PUA");
		}

		public void TestPostingStatus_PST()
		{
			AssertPostingStatus(header =>
			{
				header.AH_TransactionType = TransactionTypes.Invoice;
			}, "PST");
		}

		public void TestPostingStatus_PCM()
		{
			AssertPostingStatus(header =>
			{
				header.AH_TransactionType = TransactionTypes.Invoice;
				header.AH_InvoiceAmount = 100m;
				header.AH_OutstandingAmount = 100m;

				var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				transactionLine.AL_AH = header.PK;
				transactionLine.AL_LineAmount = 100m;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

				var accQueryClaim = (AccQueryClaim)Factory.New<IARAccQueryClaim>();
				accQueryClaim.AY_OH_Debtor = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				accQueryClaim.AY_QueryClaimAmount = 10m;
				accQueryClaim.AY_QueryClaimReference = "REF";
				accQueryClaim.AY_ShortDescriptionOfClaim = "SHORTDESC";
				accQueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
				accQueryClaim.Details = "DETAILS";
				accQueryClaim.AY_AH = header.PK;
			}, "PCM");
		}

		public void TestPostingStatus_PAI()
		{
			AssertPostingStatus(header =>
			{
				header.AH_TransactionType = TransactionTypes.Invoice;
				header.AH_FullyPaidDate = ZDateTime.Now;
			}, "PAI");
		}

		void AssertPostingStatus(Action<AccTransactionHeader> updateTransactionHeader, string expectedPostingStatus)
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			updateTransactionHeader(header);
			draftInvoice.AIH_AH_PostedTransactionHeader = header.PK;

			Factory.Save();

			AssertEquals(expectedPostingStatus, draftInvoice.PostingStatus);
		}

		public void TestPostingStatus_Reload()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			draftInvoice.AIH_AH_PostedTransactionHeader = header.PK;

			Factory.Save();

			AssertEquals("PIC", draftInvoice.PostingStatus);

			var approvalRequest = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest.XP_ParentID = header.PK;

			Factory.Save();

			draftInvoice.Reload();
			AssertEquals("PUA", draftInvoice.PostingStatus);
		}

		public void TestCreditorName()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			AssertEquals(ZString.Empty, draftInvoice.CreditorName);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_FullName = "TEST";
			draftInvoice.AIH_OH_Creditor = creditor.PK;

			Factory.Save();
			AssertEquals("TEST", draftInvoice.CreditorName);
		}

		public void TestOriginalTransactionDescription()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			AssertEquals(ZString.Empty, draftInvoice.OriginalTransactionDescription);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Desc = "Test desc";
			draftInvoice.AIH_AH_OriginalTransaction = header.PK;

			Factory.Save();
			AssertEquals("Test desc", draftInvoice.OriginalTransactionDescription);
		}

		public void TestJobClusters()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor1.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var jobCluster1 = Factory.NewWithValidTestData<AccDraftInvoiceJobCluster>();
			jobCluster1.AIC_AIH_Header = draftInvoice.PK;
			jobCluster1.AIC_Amount = 0m;
			jobCluster1.AIC_RX_NKCurrency = "AUD";
			jobCluster1.AIC_GC_Company = GlbCompany.CurrentCompany.PK;

			var jobCluster2 = Factory.NewWithValidTestData<AccDraftInvoiceJobCluster>();
			jobCluster2.AIC_AIH_Header = draftInvoice.PK;
			jobCluster2.AIC_Amount = 50m;
			jobCluster2.AIC_RX_NKCurrency = "AUD";
			jobCluster2.AIC_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertEquals(2, draftInvoice.JobClusters.Count);
			draftInvoice.JobClusters.Select(x => x.PK).ContainsSameElementsInAnyOrder(new List<ZGuid> { jobCluster1.PK, jobCluster2.PK });
		}

		public void TestExchangeRates()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor1.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var draftExRate1 = Factory.NewWithValidTestData<AccDraftInvoiceExRate>();
			draftExRate1.AIE_AIH_Header = draftInvoice.PK;

			AssertEquals(1, draftInvoice.ExchangeRates.Count);

			var draftExRate2 = Factory.NewWithValidTestData<AccDraftInvoiceExRate>();
			draftExRate2.AIE_AIH_Header = draftInvoice.PK;

			draftInvoice.ExchangeRates.Reload(true);
			AssertEquals(2, draftInvoice.ExchangeRates.Count);

			draftInvoice.ExchangeRates.Select(x => x.PK).ContainsSameElementsInAnyOrder(new List<ZGuid> { draftExRate1.PK, draftExRate2.PK });
		}

		public void TestDocManagerSupportImplementation()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			AssertEquals(DocManagerCodes.AccountingDraftInvoiceHeader, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		public void TestCodeProperty()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var codeProperty = header.GetAttribute<CodePropertyAttribute>();
			AssertEquals(nameof(header.AIH_InternalReference), codeProperty.PropertyName);
		}

		public void TestIsInLocalCurrency()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";
			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_GC_Company = company.PK;
			header.AIH_RX_NKTransactionCurrency = "AUD";
			Assert(header.IsInLocalCurrency);

			header.AIH_RX_NKTransactionCurrency = "NZD";
			Assert(!header.IsInLocalCurrency);
		}

		public void TestOSCurrencyDecimals()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			AssertEquals(header.LocalCurrencyDecimals, header.OSCurrencyDecimals);

			header.AIH_RX_NKTransactionCurrency = "AUD";
			AssertEquals(2, header.OSCurrencyDecimals);

			header.AIH_RX_NKTransactionCurrency = "KRW";
			AssertEquals(0, header.OSCurrencyDecimals);
		}

		public void TestLocalCurrencyDecimals()
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();

			header.Company.LocalCurrency.RX_SubUnitRatio = 0;
			AssertEquals("Decimals should be 0", 0, header.LocalCurrencyDecimals);

			header.Company.LocalCurrency.RX_SubUnitRatio = 100;
			AssertEquals("Decimals should be 2", 2, header.LocalCurrencyDecimals);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionHeader()
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();

			var osList = new List<string>
				{
					nameof(header.AIH_ExpectedOSTotalAmount),
					nameof(header.AIH_ExpectedOSTaxAmount),
					nameof(header.AIH_ExpectedOSExTaxAmount),
				};

			var tester = new DecimalPlacesAttributeTester(header, header.Company);
			tester.CheckNonLocalCurrency(osList, nameof(header.OSCurrencyDecimals), nameof(header.AIH_RX_NKTransactionCurrency), header);
		}

		[TestDate(2024, 5, 2, 18, 0, 0)]
		public void TestLocalTimes()
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			header.AIH_SystemCreateTimeUtc = ZDateTime.Now;
			header.AIH_SystemLastEditTimeUtc = ZDateTime.Now;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				AssertEquals(new ZDateTime(2024, 5, 3, 2, 0, 0), header.SystemCreateTimeLocal);
				AssertEquals(new ZDateTime(2024, 5, 3, 2, 0, 0), header.SystemLastEditTimeLocal);
			}
		}

		public void TestDraftInvoiceAsIEDocsParsingSupport_UtilityData()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XYZ";
			company.GC_RX_NKLocalCurrency = "AUD";

			var branch = company.Branches.AddNew();
			Factory.Save();

			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_GC_Company = company.PK;
			header.AIH_GB_Branch = branch.PK;
			header.AIH_GE_Department = GlbDepartment.CurrentDepartment.PK;
			var parsingSupport = header as IEDocsParsingSupport;
			AssertNotNull(parsingSupport);

			var jobNumberFormaterForSystemLevel = new BillOfLadingNumberCustomisation();
			RegistryItemSetForTest.Instance.DummyNumberCustomisation1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobNumberFormaterForSystemLevel);
			RegistryItemSetForTest.Instance.DummyNumberCustomisation2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobNumberFormaterForSystemLevel);

			var jobNumberFormaterForCompanyLevel = new BillOfLadingNumberCustomisation();
			RegistryItemSetForTest.Instance.DummyNumberCustomisation1.SetValue(header.AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, jobNumberFormaterForCompanyLevel);
			((IRegistryItemInternals)RegistryItemSetForTest.Instance.DummyNumberCustomisation2).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);

			var jobNumberFormaterForBranchLevel = new BillOfLadingNumberCustomisation();
			RegistryItemSetForTest.Instance.DummyNumberCustomisation1.SetValue(Guid.Empty, header.AIH_GB_Branch.ToGuid(), Guid.Empty, jobNumberFormaterForBranchLevel);
			RegistryItemSetForTest.Instance.DummyNumberCustomisation2.SetValue(Guid.Empty, header.AIH_GB_Branch.ToGuid(), Guid.Empty, jobNumberFormaterForBranchLevel);

			var jobNumberFormaterForDepartmentLevel = new BillOfLadingNumberCustomisation();
			RegistryItemSetForTest.Instance.DummyNumberCustomisation1.SetValue(Guid.Empty, header.AIH_GB_Branch.ToGuid(), header.AIH_GE_Department.ToGuid(), jobNumberFormaterForDepartmentLevel);
			RegistryItemSetForTest.Instance.DummyNumberCustomisation2.SetValue(Guid.Empty, header.AIH_GB_Branch.ToGuid(), header.AIH_GE_Department.ToGuid(), jobNumberFormaterForDepartmentLevel);

			CombineAssertions("PreCondiftions", () => {
				AssertEquals("DummyNumberCustomisation1 Fallback"
					, true
					, jobNumberFormaterForCompanyLevel.Equals(RegistryItemSetForTest.Instance.DummyNumberCustomisation1.GetFallBackValueAtAllLevels(header.AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)));

				AssertEquals("DummyNumberCustomisation2 Fallback"
					, true
					, jobNumberFormaterForSystemLevel.Equals(RegistryItemSetForTest.Instance.DummyNumberCustomisation2.GetFallBackValueAtAllLevels(header.AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)));
			});

			var mockJobNumberHelper = new Mock<IJobNumberHelper>(MockBehavior.Strict);
			mockJobNumberHelper
				.Setup(x => x.GetJobNumberCustomisationRegistries())
				.Returns(new[] {
					("A", RegistryItemSetForTest.Instance.DummyNumberCustomisation1)
					, ("B", RegistryItemSetForTest.Instance.DummyNumberCustomisation2)
				});
			mockJobNumberHelper
				.Setup(x => x.GetJobNumberRegEx("A", jobNumberFormaterForCompanyLevel))
				.Returns("RegEx_DummyNumberCustomisation1_FallbackToCompanyLevel_IgnoreBranchAndDepartmentLevel");
			mockJobNumberHelper
				.Setup(x => x.GetJobNumberRegEx("B", jobNumberFormaterForSystemLevel))
				.Returns("RegEx_DummyNumberCustomisation2_FallbackToSystemLevel_IgnoreBranchAndDepartmentLevel");
			mockJobNumberHelper
				.Setup(x => x.GetGenericJobTypeRegEx())
				.Returns("RegEx_DummyResultForGenericJobTypes");

			using (ObjectFactory.Substitute(mockJobNumberHelper.Object))
			{
				AssertEquals(
					JsonConvert.SerializeObject(new
					{
						CompanyCode = "XYZ",
						JobRegexList = new[] {
							"RegEx_DummyNumberCustomisation1_FallbackToCompanyLevel_IgnoreBranchAndDepartmentLevel"
							, "RegEx_DummyNumberCustomisation2_FallbackToSystemLevel_IgnoreBranchAndDepartmentLevel"
							, "RegEx_DummyResultForGenericJobTypes"
						}
					})
					, parsingSupport.UtilityData
				);
			}

			mockJobNumberHelper.Verify(x => x.GetJobNumberCustomisationRegistries(), Times.Exactly(1));
			mockJobNumberHelper.Verify(x => x.GetJobNumberRegEx(It.IsAny<string>(), It.IsAny<BillOfLadingNumberCustomisation>()), Times.Exactly(2));
			mockJobNumberHelper.Verify(x => x.GetGenericJobTypeRegEx(), Times.Exactly(1));
		}

		public void TestDraftInvoiceAsIEDocsParsingSupport_DenySendForParsing()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "XYZ";
			company.GC_RX_NKLocalCurrency = "AUD";
			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_GC_Company = company.PK;

			var parsingSupport = header as IEDocsParsingSupport;
			AssertNotNull(parsingSupport);

			string[] invoiceStatus = {
				Core.Constants.AccDraftInvoiceHeaderStatus.Analyzing,
				Core.Constants.AccDraftInvoiceHeaderStatus.Discarded,
				Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting,
				Core.Constants.AccDraftInvoiceHeaderStatus.AwaitingApproval,
				Core.Constants.AccDraftInvoiceHeaderStatus.Draft,
				Core.Constants.AccDraftInvoiceHeaderStatus.Processed,
				Core.Constants.AccDraftInvoiceHeaderStatus.InDispute
			};
			foreach (var status in invoiceStatus)
			{
				header.AIH_Status = status;
				if (header.AIH_Status == Core.Constants.AccDraftInvoiceHeaderStatus.Analyzing)
				{
					Assert(!parsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
				}
				else
				{
					Assert(parsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
				}
			}
		}

		public void TestConversation()
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var jobConversation = JobConversation.GetConversation(header);
			AssertNull(jobConversation);
			Factory.Save();

			var provider = (IConversationProvider)header;
			var conversation = provider.eConversation;
			var expectedConversation = JobConversation.GetConversation(header);

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestConversation_NotSaved()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertNull(provider.eConversation);
		}

		public void TestParentModule()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertNull(provider.ParentModule);
		}

		public void TestParentController()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_TransactionType = TransactionTypes.Invoice;
			var provider = (IConversationProvider)header;

			AssertEquals(ControllerIDs.APInvoiceFromDraftInvoice, provider.ParentController);

			header.AIH_TransactionType = TransactionTypes.CreditNote;

			AssertEquals(ControllerIDs.APCreditNoteFromDraftInvoice, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			Assert(provider.SendEmailNotificationsOnSave);
		}

		public void TestEmailSubjectContentOverride()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertNull(provider.EmailSubjectContentOverride);
		}

		public void TestFromAddressOverride()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertNull(provider.FromAddressOverride);
		}

		public void TestNotificationEmailTemplateOverride()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationProvider)header;

			AssertNull(provider.NotificationEmailTemplateOverride);
		}

		public void TestShouldUseThisProviderForHyperlink()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationParentHyperlinkProvider)header;

			Assert(provider.ShouldUseThisProviderForHyperlink(null));
		}

		public void TestGetHyperlinkToConversationParent()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals/"))
			{
				var header = Factory.New<AccDraftInvoiceHeader>();
				var provider = (IConversationParentHyperlinkProvider)header;

				AssertEquals($"https://glowdev/Portals/PAY/Desktop#/formFlow/ddecb92840d649b59bd10c9a2303f3a2/{header.PK}", provider.GetHyperlinkToConversationParent());
			}

			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"))
			{
				var header = Factory.New<AccDraftInvoiceHeader>();
				var provider = (IConversationParentHyperlinkProvider)header;

				AssertEquals($"https://glowdev/Portals/PAY/Desktop#/formFlow/ddecb92840d649b59bd10c9a2303f3a2/{header.PK}", provider.GetHyperlinkToConversationParent());
			}
		}

		public void TestHumanReadableName()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_TransactionType = TransactionTypes.Invoice;
			header.AIH_InternalReference = "INV123";

			AssertEquals("Draft Invoice INV123", header.HumanReadableName);

			header.AIH_TransactionNumber = "123";

			AssertEquals("Draft Invoice 123", header.HumanReadableName);

			header.AIH_TransactionType = TransactionTypes.CreditNote;

			AssertEquals("Draft Credit Note 123", header.HumanReadableName);

			header.AIH_InternalReference = "CN123";
			header.AIH_TransactionNumber = string.Empty;

			AssertEquals("Draft Credit Note CN123", header.HumanReadableName);
		}

		public void TestOpenInPortal()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			AssertEquals("Open in Portal", draftInvoice.OpenInPortal);
		}

		public override void TestBizObjectFields()
		{
			var mocklifeCycleHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Processed)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.ApprovedForPosting)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Draft)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			ObjectFactory.Substitute(mocklifeCycleHandler.Object);
			base.TestBizObjectFields();
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name == nameof(AccDraftInvoiceHeader.AIH_Status))
			{
				var bizO = info.BizObj;
				bizO[info.Name] = AccDraftInvoiceHeaderStatus.Draft;
				AssertEquals(AccDraftInvoiceHeaderStatus.Draft, bizO[info.Name]);
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			var mocklifeCycleHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Processed)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.ApprovedForPosting)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Draft)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Analyzing)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			ObjectFactory.Substitute(mocklifeCycleHandler.Object);
			base.TestSettingValueCallsRefreshBinding();
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			if (info.Name == nameof(AccDraftInvoiceHeader.AIH_Status) && !CachedValueForSettingValueCallsRefreshBindingTestCore.ContainsKey(info.Name))
			{
				CachedValueForSettingValueCallsRefreshBindingTestCore.Add(info.Name, (ZString)AccDraftInvoiceHeaderStatus.Analyzing);
			}
			else
			{
				base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
			}
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore => cachedValueForSettingValueCallsRefreshBindingTestCore ?? (cachedValueForSettingValueCallsRefreshBindingTestCore = new Dictionary<string, IZType>());
		Dictionary<string, IZType> cachedValueForSettingValueCallsRefreshBindingTestCore;

		#region Status change related tests

		public void TestThereIsAHandlerForAllPossibleStatusesAndCorrectHandlerIsCalled()
		{
			bool checkUpdateSuccessful = false;
			var newStatus = "";
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();

			var mocklifeCycleHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.ApprovedForPosting)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.ApprovedForPosting == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Discarded)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.Discarded == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Draft)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.Draft == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.InDispute)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.InDispute == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Processed)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.Processed == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.AwaitingApproval)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.AwaitingApproval == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.InReview)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.InReview == newStatus && checkUpdateSuccessful });
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.Analyzing)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = AccDraftInvoiceHeaderStatus.Analyzing == newStatus && checkUpdateSuccessful });
			ObjectFactory.Substitute(mocklifeCycleHandler.Object);

			var statuses = typeof(AccDraftInvoiceHeaderStatus).GetAllPublicConstantValues();

			foreach (var updateSuccessful in new[] { true, false })
			{
				checkUpdateSuccessful = updateSuccessful;
				foreach (var status in statuses)
				{
					AssertNoExceptionThrown(() =>
						{
							draftInvoice.AIH_Status = newStatus = status;
							var oldStatus = draftInvoice.AIH_Status;
							var message = FormattableString.Invariant($"Update status to {status}");
							if (updateSuccessful)
							{
								AssertEquals(message, status, draftInvoice.AIH_Status);
							}
							else
							{
								AssertEquals(message, oldStatus, draftInvoice.AIH_Status);
							}
						});
				}
			}
		}

		public void TestCorrectEventIsFiredWhenAttemptingToChangeStatus()
		{
			bool checkUpdateSuccessful = false;
			bool correctEventCalled = false;
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.StatusUpdateSuccessful += new Action<(AccDraftInvoiceHeader draftInvoice, string oldStatus)>((e) => correctEventCalled = checkUpdateSuccessful);
			draftInvoice.StatusUpdateFailed += new Action<(AccDraftInvoiceHeader draftInvoice, string[] errors)>((e) => correctEventCalled = !checkUpdateSuccessful);

			var mocklifeCycleHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mocklifeCycleHandler.Setup(m => m.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), AccDraftInvoiceHeaderStatus.ApprovedForPosting)).Returns(() => new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = checkUpdateSuccessful });
			ObjectFactory.Substitute(mocklifeCycleHandler.Object);

			var statuses = typeof(AccDraftInvoiceHeaderStatus).GetAllPublicConstantValues();

			foreach (var updateSuccessful in new[] { true, false })
			{
				checkUpdateSuccessful = updateSuccessful;

				draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.ApprovedForPosting;
				Assert(correctEventCalled);
			}
		}

		#endregion

		#region ISupportErrorLogging

		public void TestISupportErrorLoggingImplementation()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var errorLoggingSupporter = draftInvoice as ISupportAccProcessLogging;
			AssertEquals(draftInvoice.PK, errorLoggingSupporter.ParentId);
			Assert(errorLoggingSupporter.ShouldLog);
			AssertExceptionThrown<NotImplementedException>(() => _ = errorLoggingSupporter.Logs);
			AssertType<AccDraftInvoiceProcessingErrorLogger>(errorLoggingSupporter.Logger);
		}

		#endregion

		#region IConversationEmailBehaviorProvider

		public void TestShouldExcludeSender()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationEmailBehaviorProvider)header;

			AssertEquals(provider.ShouldExcludeSender, true);
		}

		public void TestShouldSendEmailFromSender()
		{
			var header = Factory.New<AccDraftInvoiceHeader>();
			var provider = (IConversationEmailBehaviorProvider)header;

			AssertEquals(provider.ShouldSendEmailFromSender, true);
		}

		#endregion

		class RegistryItemSetForTest : RegistryItemSet
		{
			public static RegistryItemSetForTest Instance
			{
				get { return fInstance ?? (fInstance = new RegistryItemSetForTest()); }
			}

			[ThreadStatic] static RegistryItemSetForTest fInstance;

			public BillCustomisationRegistryItem DummyNumberCustomisation1
			{
				get
				{
					return GetItem("DummyNumberCustomisation1", delegate
					{
						BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
						dataType.FountainPrefix = null;
						dataType.GeneratedNumberName = (NoResString)"Shipment Number";
						dataType.SequenceNumberName = (NoResString)"Shipment";
						dataType.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;

						return new BillCustomisationRegistryItem(
							"DummyNumberCustomisation1",
							(NoResString)"Test Category",
							(NoResString)"Mandatory Registry With Fallbacks",
							(NoResString)"Enable the prerequisite registry",
							RegistryStorageFlags.All,
							dataType
						);
					});
				}
			}

			public BillCustomisationRegistryItem DummyNumberCustomisation2
			{
				get
				{
					return GetItem("DummyNumberCustomisation2", delegate
					{
						BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
						dataType.FountainPrefix = null;
						dataType.GeneratedNumberName = (NoResString)"Shipment Number";
						dataType.SequenceNumberName = (NoResString)"Shipment";
						dataType.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;

						return new BillCustomisationRegistryItem(
							"DummyNumberCustomisation2",
							(NoResString)"Test Category",
							(NoResString)"Mandatory Registry With Fallbacks",
							(NoResString)"Enable the prerequisite registry",
							RegistryStorageFlags.All,
							dataType
						);
					});
				}
			}

			public override bool IsForProductivityWise => false;
		}
	}
}

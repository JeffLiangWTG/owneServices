using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VisaQuotaADDCVDQuerySendingActionCollection))]
	sealed class VisaQuotaQueryActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VisaQuotaADDCVDQuerySendingActionCollection>
	{
		public void TestDoNotTickEvenIfThereIsOneElement()
		{
			AssertEquals("PreCondition:One element", 1, Actions.Count);
			AssertEquals(false, Actions[0].US_SendMessage);
		}

		public void TestPopulateElementsAndSendMessages()
		{
			//same details as the first invoice line
			JobComInvoiceLine firstLine = Declaration.InvoiceLines[0];
			JobComInvoiceLine line = Declaration.InvoiceLines.AddNew();
			line.JI_Tariff = firstLine.JI_Tariff;
			line.US_UC_NKCountryOfOrigin = firstLine.US_UC_NKCountryOfOrigin;
			Declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two entry lines", 2, Declaration.CustomsEntryHeaders[0].MergedLines.Count);
			VisaQuotaADDCVDQuerySendingActionCollection actions = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.Quota, Declaration);
			AssertEquals("one action line", 1, actions.Count);
			AssertEquals("US_TariffNumber", USCTariff.FDAPriorNoticeRequiredTariff, actions[0].US_TariffNumber);
			AssertEquals("US_UC_NKOrigin", "AU", actions[0].US_UC_NKOrigin);
			AssertEquals("US_SecondaryTariffNumber", "", actions[0].US_SecondaryTariffNumber);
			actions.SendMessagesWithoutSaving();
			AssertEquals("no message generated", 0, Declaration.Messages.Count);
			actions[0].US_SendMessage = true;
			actions.SendMessagesWithoutSaving();
			AssertEquals("one message generated", 1, Declaration.Messages.Count);
			//ADD/CVD
			Declaration.Messages.RemoveAndDeleteAll();
			line.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			line.US_UC_NKCountryOfOrigin = "IT";
			actions = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.ADDCVD, Declaration);
			AssertEquals("2 lines", 3, actions.Count);
			AssertEquals("US_TariffNumber", USCTariff.FDAPriorNoticeRequiredTariff, actions[0].US_TariffNumber);
			AssertEquals("US_TariffNumber", USCTariff.AGOABenefitsApplicable, actions[1].US_TariffNumber);
			AssertEquals("US_TariffNumber", USCTariff.FDAPriorNoticeRequiredTariff, actions[2].US_TariffNumber);
			AssertEquals("US_UC_NKOrigin", "IT", actions[1].US_UC_NKOrigin);
			AssertEquals("US_SecondaryTariffNumber", "", actions[0].US_SecondaryTariffNumber);
			AssertEquals("US_SecondaryTariffNumber", "", actions[1].US_SecondaryTariffNumber);
			AssertEquals("US_SecondaryTariffNumber", "", actions[2].US_SecondaryTariffNumber);
			actions.SendMessagesWithoutSaving();
			AssertEquals("no message generated", 1, Declaration.Messages.Count);
			AssertEquals("ADD/CVD Query message has been generated", ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery, Declaration.Messages[0].EM_MessageType);
		}

		protected override VisaQuotaADDCVDQuerySendingActionCollection GetCollectionToTest()
		{
			Actions.RemoveAll();
			return Actions;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new VisaQuotaADDCVDQuerySendingAction("1", "", "AU", Actions, QueryTypeForQuotaVisaADDCVD.Quota);

		VisaQuotaADDCVDQuerySendingActionCollection actions;
		VisaQuotaADDCVDQuerySendingActionCollection Actions => actions ?? (actions = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.Quota, Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					DeclarationTestHelper.SetEntryFilerCode("XJ5");
					DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.ImportEntryNumber = "ENT32432";
					declaration.Invoices.AddNew();
					JobComInvoiceLine line = declaration.InvoiceLines.AddNew();
					line.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
					line.US_UC_NKCountryOfOrigin = "AU";
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}

				return declaration;
			}
		}
	}
}

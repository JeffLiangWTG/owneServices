using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VisaQuotaADDCVDQuerySendingAction))]
	sealed class VisaQuotaADDCVDQuerySendingActionTest : XmlSerializableNonPersistentBusinessObjectTest<VisaQuotaADDCVDQuerySendingAction>
	{
		public override void TestSchemaPropertiesHaveFields()
		{
			Assert("We need not save this sending action, so it is allowd has one Property hasn't a corresponding field.", true);
		}

		public void TestSendQueryMessagesWithoutSaving()
		{
			AssertEquals("PreCondition:One element", 1, Actions.Count);
			Actions[0].SendQueryMessagesWithoutSaving();
			AssertEquals("One message generated", 1, Declaration.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)Declaration.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryQuota, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.QuotaVisaQuery, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Message constructed", @"B018888XJ5UI                                               <<MSGNO PLACEHOLDER>>
U10804504040          AU                                                        
Y  8888XJ5UI00001", message.EM_FormattedMessageText);
		}

		public void TestMessageDescription()
		{
			VisaQuotaADDCVDQuerySendingAction action = Actions[0];
			AssertEquals("Tariff Number: 0804.50.4040, Country Of Origin: AU", action.US_MessageDescription);
			JobComInvoiceLine secondaryTariff = Declaration.InvoiceLines[0].AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			VisaQuotaADDCVDQuerySendingActionCollection actions = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.Quota, Declaration);
			AssertEquals("PreCondition:One element", 1, actions.Count);
			AssertEquals("Tariff Number: 0804.50.4040, Secondary Tariff Number: 3004.50.5040, Country Of Origin: AU", actions[0].US_MessageDescription);
		}

		public void TestCorrectQueryTypeIsUsed()
		{
			VisaQuotaADDCVDQuerySendingActionCollection actionCollection = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.Visa, Declaration);
			VisaQuotaADDCVDQuerySendingAction action = actionCollection[0];
			AssertEquals("Request should be for Visa details", QueryTypeForQuotaVisaADDCVD.Visa, action.queryType);
		}

		protected override BusinessObject GetNewBusinessObject() => Actions[0];

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
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
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

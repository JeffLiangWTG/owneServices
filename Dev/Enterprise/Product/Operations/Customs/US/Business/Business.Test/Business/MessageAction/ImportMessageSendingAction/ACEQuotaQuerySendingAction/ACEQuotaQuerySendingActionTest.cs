using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEQuotaQuerySendingAction))]
	sealed class ACEQuotaQuerySendingActionTest : XmlSerializableNonPersistentBusinessObjectTest<ACEQuotaQuerySendingAction>
	{
		public override void TestSchemaPropertiesHaveFields()
		{
			Assert("We need not save this sending action, so it is allowd has one Property hasn't a corresponding field.", true);
		}

		public void TestSendQueryMessagesWithoutSaving()
		{
			AssertEquals("PreCondition:One element", 1, Actions.Count);
			var action = Actions[0];
			action.SendQueryMessagesWithoutSaving();
			AssertEquals("Tariff Number: 0804.50.4040, Country Of Origin: AU", action.US_MessageDescription);
			AssertEquals("One message generated", 1, Declaration.Messages.Count);
			var message = (MQEDIMessage)Declaration.Messages[0];
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.QuotaQuery, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.QuotaVisaQuery, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Message constructed", @"B  8888XJ5QA                                               <<MSGNO PLACEHOLDER>>
Q1R0804504040          AU                                                       
Y  8888XJ5QA", message.EM_FormattedMessageText);
			var secondaryTariff = Declaration.InvoiceLines[0].AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ACEQuotaQuerySendingActionCollection(Declaration);
			AssertEquals("PreCondition:One element", 1, actions.Count);
			AssertEquals("Tariff Number: 0804.50.4040, Secondary Tariff Number: 3004.50.5040, Country Of Origin: AU", actions[0].US_MessageDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => Actions[0];

		ACEQuotaQuerySendingActionCollection actions;
		ACEQuotaQuerySendingActionCollection Actions => actions ?? (actions = new ACEQuotaQuerySendingActionCollection(Declaration));

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
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					var invoice = declaration.Invoices.AddNew();
					var line = invoice.InvoiceLines.AddNew();
					line.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
					line.US_UC_NKCountryOfOrigin = "AU";
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}

				return declaration;
			}
		}
	}
}

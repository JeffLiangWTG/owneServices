using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationMessageSendingNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestIncludeNotificationsFromObject()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var instruction1 = declaration.CustomsEntryInstructions.AddNew();
				instruction1.CEI_Style = "XXXX";
				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader1 = declaration.Invoices.AddNew();
				invoiceHeader1.JZ_IncoTerm = "XXX";
				var invoiceHeader2 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction1.PK;
				invoiceLine1.JI_Tariff = "0000000000";
				var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction2.PK;
				var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = instruction1.PK;
				var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader2.CH_CEI_Instruction = instruction2.PK;
				invoiceLine1.JI_CL = entryHeader1.MergedLines.AddNew().PK;
				invoiceLine2.JI_CL = entryHeader2.MergedLines.AddNew().PK;

				var parent = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				declaration.LoadChildEditableObjects();
				declaration.RunPreSaveValidation();
				var collector = new JobDeclarationMessageSendingNotificationCollector(declaration, new CusEntryHeader[] { entryHeader1, entryHeader2 });
				var messageErrors = collector.GetMessageErrors();
				Assert("CPC error for instruction 1", messageErrors.Contains("CPC: The code you have selected is not in the list."));
				Assert("CPC error for instruction 2", messageErrors.Contains("CPC: You have not entered a CPC."));
				Assert("Tariff error for invoice 1", messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
				Assert("Tariff error for invoice 2", messageErrors.Contains("Incoterm: Please enter an Incoterm."));
				Assert("Tariff error for invoice line 1", messageErrors.Contains("Tariff: You have not entered a valid code."));
				Assert("Tariff error for invoice line 2", messageErrors.Contains("Tariff: Tariff may not be empty"));
				Assert("Agent error for JobDeclaration", messageErrors.Any(x => x.Message.StartsWith("Branch Code: USC Registration Number for CN is required for this branch/company's Organization Proxy.")));

				collector = new JobDeclarationMessageSendingNotificationCollector(declaration, new CusEntryHeader[] { entryHeader2 });
				messageErrors = collector.GetMessageErrors();
				Assert("CPC error for instruction 1", !messageErrors.Contains("CPC: The code you have selected is not in the list."));
				Assert("CPC error for instruction 2", messageErrors.Contains("CPC: You have not entered a CPC."));
				Assert("Tariff error for invoice 1", !messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
				Assert("Tariff error for invoice 2", messageErrors.Contains("Incoterm: Please enter an Incoterm."));
				Assert("Tariff error for invoice line 1", !messageErrors.Contains("Tariff: You have not entered a valid code."));
				Assert("Tariff error for invoice line 2", messageErrors.Contains("Tariff: Tariff may not be empty"));
				Assert("Agent error for JobDeclaration", messageErrors.Any(x => x.Message.StartsWith("Branch Code: USC Registration Number for CN is required for this branch/company's Organization Proxy.")));

				collector = new JobDeclarationMessageSendingNotificationCollector(declaration, Array.Empty<CusEntryHeader>());
				messageErrors = collector.GetMessageErrors();
				Assert("CPC error for instruction 1", !messageErrors.Contains("CPC: The code you have selected is not in the list."));
				Assert("CPC error for instruction 2", !messageErrors.Contains("CPC: You have not entered a CPC."));
				Assert("Tariff error for invoice 1", !messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
				Assert("Tariff error for invoice 2", !messageErrors.Contains("Incoterm: Please enter an Incoterm."));
				Assert("Tariff error for invoice line 1", !messageErrors.Contains("Tariff: You have not entered a valid code."));
				Assert("Tariff error for invoice line 2", !messageErrors.Contains("Tariff: Tariff may not be empty"));

				collector = new JobDeclarationMessageSendingNotificationCollector(declaration, new CusEntryHeader[] { entryHeader1 });
				messageErrors = collector.GetMessageErrors();
				Assert("CPC error for instruction 1", messageErrors.Contains("CPC: The code you have selected is not in the list."));
				Assert("CPC error for instruction 2", !messageErrors.Contains("CPC: You have not entered a CPC."));
				Assert("Tariff error for invoice 1", messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
				Assert("Tariff error for invoice 2", !messageErrors.Contains("Incoterm: Please enter an Incoterm."));
				Assert("Tariff error for invoice line 1", messageErrors.Contains("Tariff: You have not entered a valid code."));
				Assert("Tariff error for invoice line 2", !messageErrors.Contains("Tariff: Tariff may not be empty"));
				Assert("Agent error for JobDeclaration", messageErrors.Any(x => x.Message.StartsWith("Branch Code: USC Registration Number for CN is required for this branch/company's Organization Proxy.")));
			}
		}

		public void TestMessageErrorsAreCollectedCorrectly()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_CustomsUnitQty = "";
				invoiceLine2.JI_CustomsQuantity = 10m;
				Assert("PreCondition", invoiceLine2.HasMessageErrors);
				var messageErrors = invoiceLine2.JI_CustomsQuantityInfo.GetMessageErrors().ToUniqueMessageListString();

				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entry1.MergedLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				entry1.CH_CEI_Instruction = entryInstruction1.PK;
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_CEI_Instruction = entryInstruction2.PK;
				var entryLine2 = entry2.MergedLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;

				var parent = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				AssertEquals("PreCondition", 2, parent.SendingObjectsCollection.Count);

				parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First(x => x.Header == entry1).ShouldSend = true;
				AssertNotContains(messageErrors, parent.BizObjValidationMessageErrors);

				parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First(x => x.Header == entry2).ShouldSend = true;
				AssertContains(messageErrors, parent.BizObjValidationMessageErrors);
			}
		}
	}
}

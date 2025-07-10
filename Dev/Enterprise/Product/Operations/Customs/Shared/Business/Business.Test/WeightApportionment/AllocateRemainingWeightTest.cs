using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AllocateRemainingWeightTest : TestCaseWithFactory
	{
		public void TestAllocateByPrice()
		{
			AssertAllocateRemainingWeight((declaration, notificationCollector) => { new AllocateRemainingWeightWrapper(declaration).ApportionRemaining(notificationCollector, AllocateRemainingWeightWay.ByPrice); }, (line, value) => { line.JI_LinePrice = value; });
		}

		public void TestAllocateByInvoiceQuantity()
		{
			AssertAllocateRemainingWeight((declaration, notificationCollector) => { new AllocateRemainingWeightWrapper(declaration).ApportionRemaining(notificationCollector, AllocateRemainingWeightWay.ByQuantity); }, (line, value) => { line.JI_InvoiceQuantity = value; });
		}

		void AssertAllocateRemainingWeight(Action<BaseJobDeclaration, IMessageNotificationCollector> allocateAction, Action<BaseJobComInvoiceLine, ZDecimal> setInvoiceLineValueAction)
		{
			var notificationCollector = new MessageNotificationCollector_ForTest();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_NetWeight = 1000M;
			invoiceHeader.JZ_Weight = 10000M;
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			declaration.JE_AutoWeightApportion = false;
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceUQ = "PCS";
			line1.JI_NetWeight = 600M;
			line1.JI_Weight = 6000M;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_InvoiceUQ = "PCS";
			line2.JI_NetWeight = 0M;
			line2.JI_Weight = 0M;
			var line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_InvoiceUQ = "PCS";
			line3.JI_NetWeight = 0M;
			line3.JI_Weight = 0M;
			notificationCollector.ShowWarning(null, null);
			allocateAction.Invoke(declaration, notificationCollector);
			CombineAssertions(() =>
			{
				AssertNotContains("Please untick the 'Auto Apportion Weight' menu item before allocating remaining weight.", notificationCollector.LastMessage);
				AssertEquals("line1.JI_Weight", 6000M, line1.JI_Weight);
				AssertEquals("line2.JI_Weight", 0M, line2.JI_Weight);
				AssertEquals("line3.JI_Weight", 0M, line3.JI_Weight);

				AssertEquals("line1.JI_NetWeight", 600M, line1.JI_NetWeight);
				AssertEquals("line2.JI_NetWeight", 0M, line2.JI_NetWeight);
				AssertEquals("line3.JI_NetWeight", 0M, line3.JI_NetWeight);
			});
			setInvoiceLineValueAction.Invoke(line1, 1M);
			setInvoiceLineValueAction.Invoke(line2, 1M);
			setInvoiceLineValueAction.Invoke(line3, 2M);
			line1.JI_NetWeight = 6000M;
			line1.JI_Weight = 60000M;
			notificationCollector.ShowWarning(null, null);
			allocateAction.Invoke(declaration, notificationCollector);
			CombineAssertions(() =>
			{
				AssertContains("The remaining net weight cannot be allocated because the sum of the entered invoice line net weight is larger than or equal to the entered invoice header (Invoice Number: INV1) net weight.", notificationCollector.LastMessage);
				AssertContains("The remaining gross weight cannot be allocated because the sum of the entered invoice line gross weight is larger than or equal to the entered invoice header (Invoice Number: INV1) gross weight.", notificationCollector.LastMessage);
				AssertEquals("line1.JI_Weight", 60000M, line1.JI_Weight);
				AssertEquals("line2.JI_Weight", 0M, line2.JI_Weight);
				AssertEquals("line3.JI_Weight", 0M, line3.JI_Weight);

				AssertEquals("line1.JI_NetWeight", 6000M, line1.JI_NetWeight);
				AssertEquals("line2.JI_NetWeight", 0M, line2.JI_NetWeight);
				AssertEquals("line3.JI_NetWeight", 0M, line3.JI_NetWeight);
			});

			line1.JI_NetWeight = 600M;
			line1.JI_Weight = 6000M;
			notificationCollector.ShowWarning(null, null);
			allocateAction.Invoke(declaration, notificationCollector);
			CombineAssertions(() =>
			{
				AssertNull(notificationCollector.LastMessage);
				AssertEquals("line1.JI_Weight", 6000M, line1.JI_Weight);
				AssertEquals("line2.JI_Weight", 1333.333M, line2.JI_Weight);
				AssertEquals("line3.JI_Weight", 2666.667M, line3.JI_Weight);

				AssertEquals("line1.JI_NetWeight", 600M, line1.JI_NetWeight);
				AssertEquals("line2.JI_NetWeight", 133.333M, line2.JI_NetWeight);
				AssertEquals("line3.JI_NetWeight", 266.667M, line3.JI_NetWeight);
			});

			notificationCollector.ShowWarning(null, null);
			allocateAction.Invoke(declaration, notificationCollector);
			CombineAssertions(() =>
			{
				AssertContains("The remaining net weight cannot be allocated because all invoice lines under the invoice (Invoice Number: INV1) have net weight.", notificationCollector.LastMessage);
				AssertContains("The remaining gross weight cannot be allocated because all invoice lines under the invoice (Invoice Number: INV1) have gross weight.", notificationCollector.LastMessage);
				AssertEquals("line1.JI_Weight", 6000M, line1.JI_Weight);
				AssertEquals("line2.JI_Weight", 1333.333M, line2.JI_Weight);
				AssertEquals("line3.JI_Weight", 2666.667M, line3.JI_Weight);

				AssertEquals("line1.JI_NetWeight", 600M, line1.JI_NetWeight);
				AssertEquals("line2.JI_NetWeight", 133.333M, line2.JI_NetWeight);
				AssertEquals("line3.JI_NetWeight", 266.667M, line3.JI_NetWeight);
			});

			line1.JI_NetWeight = 0M;
			line1.JI_Weight = 0M;
			line2.JI_NetWeight = 0M;
			line2.JI_Weight = 0M;
			line3.JI_NetWeight = 0M;
			line3.JI_Weight = 0M;
			setInvoiceLineValueAction.Invoke(line1, 4M);
			setInvoiceLineValueAction.Invoke(line2, 3M);
			setInvoiceLineValueAction.Invoke(line3, 2M);

			notificationCollector.ShowWarning(null, null);
			allocateAction.Invoke(declaration, notificationCollector);
			CombineAssertions(() =>
			{
				AssertNull(notificationCollector.LastMessage);
				AssertEquals("line1.JI_Weight", 4444.445M, line1.JI_Weight);
				AssertEquals("line2.JI_Weight", 3333.333M, line2.JI_Weight);
				AssertEquals("line3.JI_Weight", 2222.222M, line3.JI_Weight);

				AssertEquals("line1.JI_NetWeight", 444.445M, line1.JI_NetWeight);
				AssertEquals("line2.JI_NetWeight", 333.333M, line2.JI_NetWeight);
				AssertEquals("line3.JI_NetWeight", 222.222M, line3.JI_NetWeight);
			});
		}
	}
}

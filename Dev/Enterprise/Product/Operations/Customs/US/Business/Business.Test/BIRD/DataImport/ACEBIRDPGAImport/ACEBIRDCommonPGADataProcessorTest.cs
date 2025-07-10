using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class ACEBIRDCommonPGADataProcessorTest : TestCaseWithFactory
	{
		public void TestEndToEndProcessDeclaredPGA()
		{
			var message = GetEDIMessageForEndToEndTest();
			ImportMessageBlocks(message);
			AssertEndToEndTestResult();
			AssertNoExceptionThrownWhenProcessDuplicateMessageBlocks(message);
		}

		public abstract void TestEndToEndProcessDisclaimedPGA();

		protected abstract MQEDIMessage GetEDIMessageForEndToEndTest();

		protected abstract void AssertEndToEndTestResult();

		protected void ImportMessageBlocks(MQEDIMessage message)
		{
			var notifications = new NotificationCollection();
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			var messageText = message.EM_FormattedMessageText + "                                                                    ";
			generator.Deserialise(messageText.Replace("\r\n", ""));

			new ACEBIRDDeclarationDataAdapter().DoImport(DeclarationImported, generator, notifications);
		}

		protected ISEAdditionalData GetAction(JobDeclaration declaration)
		{
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			return ACEEntrySummaryMessageSendingOption.New(action);
		}

		protected JobDeclaration DeclarationImported
		{
			get
			{
				if (declarationImported == null)
				{
					declarationImported = Factory.New<JobDeclaration>();
					declarationImported.JE_MessageType = JobMessageTypeList.Codes.Import;
					declarationImported.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declarationImported.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declarationImported.US_EnableENS = true;
					declarationImported.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}

				return declarationImported;
			}
		}
		JobDeclaration declarationImported;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;

		void AssertNoExceptionThrownWhenProcessDuplicateMessageBlocks(MQEDIMessage message)
		{
			var notifications = new NotificationCollection();
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			var messageText = (ZString)(message.EM_FormattedMessageText + "                                                                    ").Replace("\r\n", "");
			var messageBlocksBeforePGA = new List<ZString>();
			var pgaMessageBlocks = new Dictionary<ZString, List<ZString>>();
			var messageBlocksAfterPGA = new List<ZString>();
			var messageBlockTextToProcesse = ZString.Empty;
			var previousProcessedPG01Block = ZString.Empty;
			var hasPGABlocksDetected = false;

			while (!(messageBlockTextToProcesse = messageText.Left(80)).IsEmpty)
			{
				if (messageBlockTextToProcesse.StartsWith("PG"))
				{
					hasPGABlocksDetected = true;

					if (messageBlockTextToProcesse.StartsWith("PG01"))
					{
						pgaMessageBlocks[messageBlockTextToProcesse] = new List<ZString>();
						previousProcessedPG01Block = messageBlockTextToProcesse;
					}
					else
					{
						pgaMessageBlocks[previousProcessedPG01Block].Add(messageBlockTextToProcesse);
					}
				}
				else
				{
					if (hasPGABlocksDetected)
					{
						messageBlocksAfterPGA.Add(messageBlockTextToProcesse);
					}
					else
					{
						messageBlocksBeforePGA.Add(messageBlockTextToProcesse);
					}
				}

				messageText = messageText.SubstringSafe(80);
			}

			var stringBuilder = new ZStringBuilder();
			foreach (var block in messageBlocksBeforePGA)
			{
				stringBuilder.Append(block);
			}

			foreach (var block in pgaMessageBlocks)
			{
				stringBuilder.Append(block.Key);

				for (var i = 0; i < 2; i++)
				{
					foreach (var childBlock in block.Value)
					{
						stringBuilder.Append(childBlock);
					}
				}
			}

			foreach (var block in messageBlocksAfterPGA)
			{
				stringBuilder.Append(block);
			}

			generator.Deserialise(stringBuilder.ToString());

			AssertNoExceptionThrown(() =>
			{
				new ACEBIRDDeclarationDataAdapter().DoImport(DeclarationImported, generator, notifications);
			});
		}
	}
}

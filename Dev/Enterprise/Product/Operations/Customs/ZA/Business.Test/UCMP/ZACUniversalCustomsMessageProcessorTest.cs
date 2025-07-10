using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP
{
	[TestedType(typeof(ZACUniversalCustomsMessageProcessor))]
	public sealed class ZACUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<ZACUniversalCustomsMessageProcessor>
	{
		// This test only checks if UCMP can pick up ZAC messages.
		// For the full test coverage of individual message processor functionality, see tests in ZA.Business.MessageProcessor.Testing.

		public void Test_CONTRLMessage()
		{
			var (message, _, _, _) = TestMessageFactory.Get_CONTRL_WithLinkedCusResEntryHeaderOutgoingMessage(Factory);
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void Test_CUSCARMessage()
		{
			var (message, _) = TestMessageFactory.Get_CUSCAR_WithLinkedAsycudaManifestHeader(Factory);
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void Test_CUSRESMessage()
		{
			var message = TestMessageFactory.GetIncomingCUSRESEDIMessage(Factory, "TODO");
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message, expectDiscarded: true);
		}

		public void Test_CUSRES_REQDOCMessage()
		{
			var (message, _, _) = TestMessageFactory.Get_CUSRES_REQDOC(Factory, "1234567890");
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void Test_GENRALMessage()
		{
			var message = TestMessageFactory.Get_GENRAL(Factory);
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void Test_STATACMessage()
		{
			var message = TestMessageFactory.Get_STATAC(Factory);
			AssertZACUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		void AssertZACUniversalCustomsMessageProcessorCanHandleMessage(EDIMessage message, bool expectDiscarded = false)
		{
			using (EnableUCMP())
			{
				var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
				var processor = new ZACUniversalCustomsMessageProcessor();
				AssertEquals("Pre-condition: message queued", EDIMessage.Status.Queued, message.EM_Status.ToString());

				var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, logger);
				var branch = processor.GetBranch(message, logger, linkedBusinessObjectMetaData.ReturnValue.BranchPk);
				var serializationKeysResult = processor.GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData.ReturnValue);
				if (string.IsNullOrEmpty(serializationKeysResult.DiscardReason))
				{
					message.EM_Status = EDIMessage.Status.PreProcessedOK;
					message.EM_LinkTable = linkedBusinessObjectMetaData.ReturnValue.LinkTableName;
					message.EM_LinkUniqueID = linkedBusinessObjectMetaData.ReturnValue.LinkUniqueID;

					processor.ProcessMessage(message, logger, default);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;
				}

				if (expectDiscarded)
				{
					AssertEquals(EDIMessage.Status.Discarded, message.EM_Status.ToString());
				}
				else
				{
					AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status.ToString());
				}
			}

			static IDisposable EnableUCMP() => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true);
		}

		protected override string ApplicationCode => EDIMessage.ApplicationCodes.SouthAfricanCustoms;
	}
}

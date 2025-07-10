using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageProcessorProviderTest : TestCase
	{
		public void TestDifferentProcessorIsHandedBackFromSameProvider()
		{
			MessageBlock messageBlock = new ZZZB();
			var provider = new MessageProcessorProvider();
			IProcessor processor1 = provider.GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting1, messageBlock);
			IProcessor processor2 = provider.GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting1, messageBlock);
			AssertNotEquals(processor1, processor2);
		}

		public void TestProcessTopLevelMessageBlock()
		{
			MessageProcessorProvider provider = new MessageProcessorProvider();
			AssertEquals(typeof(ProcessorTestClass), provider.GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting1, new ZZZB()).GetType());
			AssertEquals(typeof(HookedMessageProcessorFactoryForTesting.HookedProcessor), provider.GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting2, new ZZZC2()).GetType());
		}

		public void TestProcessTopLevelMessageBlockWithBadApplicationIdentifier()
		{
			MessageBlock messageBlock = new ZZZB();
			AssertNull(new MessageProcessorProvider().GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, "XX", messageBlock));
		}

		public void TestProcessSubLevelMessageBlock()
		{
			MessageBlock messageBlock = new ZZZC();
			AssertNull(new MessageProcessorProvider().GetProcessor(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting2, messageBlock));
		}

		public void TestProcessorTypeForACEErrorBlock()
		{
			var provider = new MessageProcessorProvider();
			var messageTypesToExclude = new HashSet<string>
			{
				ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleUpdateTransactionResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation
			};
			var errorBlock = new AABIX0();
			foreach (ZArchitecture.Core.CodeDescriptionPair messageTypePair in new ACEApplicationIdentifierCodeList())
			{
				if (!messageTypesToExclude.Contains(messageTypePair.Code))
				{
					//Error block can happen any message type
					var processor = provider.GetProcessor(CBPEDIInterchange.ApplicationCodes.USCustomsImport, messageTypePair.Code, errorBlock);
					AssertNotNull(messageTypePair.Code, processor);
				}
			}
		}
	}
}

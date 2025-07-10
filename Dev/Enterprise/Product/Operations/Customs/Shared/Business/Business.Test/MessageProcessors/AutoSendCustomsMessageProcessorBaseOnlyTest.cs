using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;

namespace Enterprise.Customs.Business.MessageProcessors.Testing
{
	sealed class AutoSendCustomsMessageProcessorBaseOnlyTest : AutoSendCustomsMessageProcessorTest
	{
		public void TestCallToFactorySave()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			declaration.CustomsEntryHeaders.AddNew();
			IProcessor processor = new AutoSendCustomsMessageProcessorForTest(declaration)
			{
				SendCustomsMessageForTest = () =>
				{
					declaration.Factory.Save();
				}
			};

			processor.Process(Mock.Of<INotifications>());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Factory.Save() should not be called by message processor.", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSaveWithConcurrencyConflict()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			newEntryHeader.CH_Status = "XXX";
			newFactory.Save();

			IProcessor processor = new AutoSendCustomsMessageProcessorForTest(declaration)
			{
				SendCustomsMessageForTest = () =>
				{
					entryHeader.CH_Status = "YYY";
				}
			};
			var notifications = Mock.Of<INotifications>();

			processor.Process(notifications);

			var latestEntryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("XXX", latestEntryHeader.CH_Status);
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(CusEntryHeader entry)
		{
			CombineAssertions(() =>
			{
				AssertEquals("One entry should be generated.", 1, entry.Declaration.CustomsEntryHeaders.Count);

				var message = entry.Messages[0];
				AssertNotNull(message);
				AssertEquals("EM_ApplicationReference should have JE_CustomsProfile", "AAA", message.EM_ApplicationReference);
			});
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration) => new AutoSendCustomsMessageProcessorForTest(declaration);

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsProfile = "AAA";
		}

		protected override CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration) => declaration.CustomsEntryHeaders.FirstOrDefault();

		protected override void SetEntryClearedStatus(CusEntryHeader entry)
		{
			entry.EntryNumber = "123";
			entry.CH_EntryStatus = ZString.Empty;
		}

		protected override ZString ExpectedMessageDescription => nameof(AutoSendCustomsMessageProcessorForTest);

		sealed class AutoSendCustomsMessageProcessorForTest : AutoSendCustomsMessageProcessor
		{
			public AutoSendCustomsMessageProcessorForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			protected override ZString MessageDescription => nameof(AutoSendCustomsMessageProcessorForTest);
			protected override IEnumerable<CusEntryHeader> GetEntryHeadersToSendCore() => Declaration.CustomsEntryHeaders;

			public Action SendCustomsMessageForTest { get; set; } 

			protected override ZBool SendCustomsMessageCore(INotifications notifications, CusEntryHeader entryHeader)
			{
				if (SendCustomsMessageForTest is null)
				{
					var message = entryHeader.Messages.AddNew();
					message.MessageNumberStrategy = new TestMessageNumberStrategy();
					message.EM_ApplicationReference = Declaration.JE_CustomsProfile;
				}
				else
				{
					SendCustomsMessageForTest?.Invoke();
				}

				notifications.AddWarning("please check whether the job is waiting for response from customs");
				return true;
			}

			sealed class TestMessageNumberStrategy : IMessageNumberStrategy
			{
				string IMessageNumberStrategy.GetMessageReferenceNumber()
				{
					return ZDateTime.Now.Ticks.ToString();
				}
			}
		}
	}
}

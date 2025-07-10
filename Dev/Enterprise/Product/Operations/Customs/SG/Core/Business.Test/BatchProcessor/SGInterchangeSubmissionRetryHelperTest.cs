using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class SGInterchangeSubmissionRetryHelperTest : TestCaseWithFactory
	{
		public void TestIsNextRetryLastRetry()
		{
			using (SGCustomsDataRegistry.Instance.SubmissionRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var interchange = TestInterchange;

				CombineAssertions(() =>
				{
					AssertEquals("Initial Retry count", 0, interchange.EI_RetryCount);
					AssertEquals(false, Helper.IsNextRetryLastRetry(interchange));

					interchange.EI_RetryCount = 3;
					Assert("Next Retry is the last retry", Helper.IsNextRetryLastRetry(interchange));
				});
			}
		}

		public void TestMaxRetriesExceeded()
		{
			using (SGCustomsDataRegistry.Instance.SubmissionRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var interchange = TestInterchange;

				CombineAssertions(() =>
				{
					AssertEquals("Initial Retry count", 0, interchange.EI_RetryCount);
					AssertEquals(false, Helper.MaxRetriesExceeded(interchange));

					interchange.EI_RetryCount = 4;
					Assert("Retries exceeded", Helper.MaxRetriesExceeded(interchange));
				});
			}
		}

		[TestDate(2022, 5, 1, 3, 0, 0)]
		public void TestAddDelayToLinkedEDIMessage()
		{
			using (SGCustomsDataRegistry.Instance.SubmissionFinalRetryDelay.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15))
			{
				var interchange = TestInterchange;
				var message = Factory.New<EDIMessage>();
				message.EM_EI = interchange.PK;
				message.EM_HeldUntilDate = ZDateTime.Now;

				Helper.AddDelayToLinkedEDIMessage(interchange);
				var expectedHeldUntilTime = new ZDateTime(2022, 5, 1, 3, 15, 0);
				AssertEquals(expectedHeldUntilTime, message.EM_HeldUntilDate);
			}
		}

		[TestDate(2022, 5, 1, 3, 0, 0)]
		public void TestIsLinkedMessageDelayed()
		{
			var interchange = TestInterchange;
			var message = Factory.New<EDIMessage>();
			message.EM_EI = interchange.PK;

			CombineAssertions(() =>
			{
				message.EM_HeldUntilDate = new DateTime(2022, 5, 1, 2, 45, 0);
				AssertEquals("Linked message not delayed", false, Helper.IsLinkedMessageDelayed(interchange));

				message.EM_HeldUntilDate = new DateTime(2022, 5, 1, 3, 15, 0);
				Assert("Linked message is delayed", Helper.IsLinkedMessageDelayed(interchange));
			});
		}

		public void TestOnMaxRetriesExceeded()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			var interchange = TestInterchange;
			var entry = Factory.New<CusEntryHeader>();
			entry.SG_PreviousEntryStatus = Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent;
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			var message = entry.Messages.AddNew();
			message.EM_EI = interchange.PK;

			CombineAssertions(() =>
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				AssertEquals("Current count", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

				Helper.OnMaxRetriesExceeded(interchange);
				AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
				AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
				AssertEquals(Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, entry.CH_Status);
				AssertEquals("One email should have been sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			});
		}

		SGInterchangeSubmissionRetryHelper Helper => helper ?? (helper = new SGInterchangeSubmissionRetryHelper());
		SGInterchangeSubmissionRetryHelper helper;

		EDIInterchange TestInterchange
		{
			get
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeTradenet4;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_InterchangeNum = "1";
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;

				return interchange;
			}
		}
	}
}

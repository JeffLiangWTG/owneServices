#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterData.Business.Tests
{
	public class RecalculateWorkerForOrgHeaderTest : TestCaseWithFactory
	{
		bool checkInvokeRecalculated;
		bool checkInvokeRecalculating;
		bool checkInvokeRecalculateStart;

		public void TestRegenerateForEventHandler()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "GB333";

			if (SynchronizationContext.Current == null)
			{
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			}

			var recalculateWorker = new PatternMatchingRecalculator<OrgHeader>(orgHeader);
			recalculateWorker.Recalculated += (sender, e) =>
			{
				checkInvokeRecalculated = true;
			};
			recalculateWorker.Recalculating += (sender, e) =>
			{
				checkInvokeRecalculating = true;
			};
			recalculateWorker.RecalculateStart += (sender, e) =>
			{
				checkInvokeRecalculateStart = true;
			};

			recalculateWorker.Regenerate(new DeduplicationOrgHeader(orgHeader));
			Assert("Recalculated handler should be invoked.", checkInvokeRecalculated);
			Assert("Recalculating handler should be invoked.", checkInvokeRecalculating);
			Assert("RecalculatStart handler should be invoked.", checkInvokeRecalculateStart);
		}

		public void TestRegenerateForFunctionCall()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "GB333";

			orgHeader.MainAddress.Address1 = "72 ORiordan Street";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			var needHashValue = orgHeader.MainAddress.OA_Address1 + orgHeader.MainAddress.OA_Address2 + orgHeader.MainAddress.OA_City + orgHeader.MainAddress.OA_PostCode + orgHeader.MainAddress.OA_State;
			var firstHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());

			Factory.Save();
			var recalculateWorker = new PatternMatchingRecalculator<OrgHeader>(orgHeader);
			recalculateWorker.Regenerate(new DeduplicationOrgHeader(orgHeader));
			var query = new ZQuery(PatternMatchingAddressSchema.PMA_OH, orgHeader.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, OrgAddressSchema.Constants.Prefix);
			var result = Factory.Load<PatternMatchingAddress>(query);
			Assert("Expected:correct amount of address patterns were created", result.Length == 1);

			var updateAddress = Factory.Load<OrgAddress>(orgHeader.MainAddress.PK);
			updateAddress.OA_Address2 = "Bell Street CC2";
			updateAddress.OA_RN_NKCountryCode = "CN";
			Factory.Save();

			recalculateWorker.Regenerate(new DeduplicationOrgHeader(orgHeader));
			needHashValue = orgHeader.MainAddress.OA_Address1 + orgHeader.MainAddress.OA_Address2 + orgHeader.MainAddress.OA_City + orgHeader.MainAddress.OA_PostCode + orgHeader.MainAddress.OA_State;
			var secondHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());
			Assert("Expected:hash value must be changed ", secondHashValue != firstHashValue);
			query = new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, orgHeader.MainAddress.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, OrgAddressSchema.Constants.Prefix);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_HashedValue, secondHashValue);
			var mat = Factory.Load<PatternMatchingAddress>(query).FirstOrDefault();
			AssertNotNull("Expected:hash value were correct ", mat);
		}

		public void TestNoExceptionThrowWhenRegenerateAsync()
		{
			ErrorReporter.Clear();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "GB333";

			orgHeader.MainAddress.Address1 = "72 ORiordan Street";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			Factory.Save();

			var recalculateWorker = new PatternMatchingRecalculatorForTest<OrgHeader>(orgHeader);
			AssertNoExceptionThrown(() =>
			{
				orgHeader.Factory.ThreadSentry.RelinquishThreadOwnership();
				Task.Run(() =>
				{
					orgHeader.Factory.ThreadSentry.TakeThreadOwnership();
				}).Wait();

				recalculateWorker.RegenerateCoreForTest();
			});
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestRegenerate_SendsDebugger_Information()
		{
			var participantMock = new Mock<IDeduplicationDebuggerParticipant>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "GB333";

			participantMock.Setup(x => x.DebuggerName).Returns("PatternRecalculator");
			participantMock.Setup(x => x.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.IsAny<object>()));

			orgHeader.MainAddress.Address1 = "72 ORiordan Street";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			var needHashValue = orgHeader.MainAddress.OA_Address1 + orgHeader.MainAddress.OA_Address2 + orgHeader.MainAddress.OA_City + orgHeader.MainAddress.OA_PostCode + orgHeader.MainAddress.OA_State;
			var firstHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());

			Factory.Save();
			var recalculateWorker = new PatternMatchingRecalculator<OrgHeader>(orgHeader, participantMock.Object);
			recalculateWorker.Regenerate(new DeduplicationOrgHeader(orgHeader));
			var query = new ZQuery(PatternMatchingAddressSchema.PMA_OH, orgHeader.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, OrgAddressSchema.Constants.Prefix);
			var result = Factory.Load<PatternMatchingAddress>(query);
			Assert("Expected:correct amount of address patterns were created", result.Length == 1);

			var updateAddress = Factory.Load<OrgAddress>(orgHeader.MainAddress.PK);
			updateAddress.OA_Address2 = "Bell Street CC2";
			updateAddress.OA_RN_NKCountryCode = "CN";
			Factory.Save();

			recalculateWorker.Regenerate(new DeduplicationOrgHeader(orgHeader));

			AssertEquals(4, recalculateWorker.DebuggerMessages.Count);
			AssertNotNull(DeduplicationUtils.DebuggerHubInstance.FindParticipant("PatternRecalculator"));
			participantMock.Verify(mock => mock.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.IsAny<object>()), Times.Exactly(2));
			participantMock.Verify(mock => mock.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.Is<object>(u => (u as IOrgHeader) != null)), Times.Exactly(2));
		}
	}

	public class RecalculateWorkerForPersonTest : TestCaseWithFactory
	{
		bool checkInvokeRecalculated;
		bool checkInvokeRecalculating;
		bool checkInvokeRecalculateStart;

		public void TestRegenerateForEventHandler()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";

			if (SynchronizationContext.Current == null)
			{
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			}

			var recalculateWorker = new PatternMatchingRecalculator<GlbPerson>(person);
			recalculateWorker.Recalculated += (sender, e) =>
			{
				checkInvokeRecalculated = true;
			};
			recalculateWorker.Recalculating += (sender, e) =>
			{
				checkInvokeRecalculating = true;
			};
			recalculateWorker.RecalculateStart += (sender, e) =>
			{
				checkInvokeRecalculateStart = true;
			};

			recalculateWorker.Regenerate((DeduplicationGlbPerson)person.CreateIGlbPerson());
			Assert("Recalculated handler should be invoked.", checkInvokeRecalculated);
			Assert("Recalculating handler should be invoked.", checkInvokeRecalculating);
			Assert("RecalculatStart handler should be invoked.", checkInvokeRecalculateStart);
		}

		public void TestRegenerateForFunctionCall()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.Address1 = "123 Fake St";
			person.Address2 = "72 ORiordan Street";
			var needHashValue = person.Address1 + person.Address2 + person.City + person.Postcode + person.State;
			var firstHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());

			Factory.Save();
			var recalculateWorker = new PatternMatchingRecalculator<GlbPerson>(person);
			recalculateWorker.Regenerate((DeduplicationGlbPerson)person.CreateIGlbPerson());
			var query = new ZQuery(PatternMatchingAddressSchema.PMA_PER, person.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, GlbPersonSchema.Constants.Prefix);
			var result = Factory.Load<PatternMatchingAddress>(query);
			AssertEquals("Expected:correct amount of address patterns were created", 1, result.Length);

			person.Address2 = "Bell Street CC2";
			Factory.Save();
			recalculateWorker.Regenerate((DeduplicationGlbPerson)person.CreateIGlbPerson());
			needHashValue = person.Address1 + person.Address2 + person.City + person.Postcode + person.State;
			var secondHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());
			Assert("Expected:hash value must be changed ", secondHashValue != firstHashValue);
			query = new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, person.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, person.TablePrefix);
			var mat = Factory.Load<PatternMatchingAddress>(query).FirstOrDefault();
			AssertEquals("Expected:hash value were correct ", mat.PMA_HashedValue, secondHashValue);
		}

		public void TestNoExceptionThrowWhenRegenerateAsync()
		{
			ErrorReporter.Clear();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.Address1 = "123 Fake St";
			person.Address2 = "72 ORiordan Street";

			Factory.Save();

			var recalculateWorker = new PatternMatchingRecalculatorForTest<GlbPerson>(person);

			recalculateWorker.SetTargetGlowBizO((DeduplicationGlbPerson)person.CreateIGlbPerson());
			AssertNoExceptionThrown(() =>
			{
				person.Factory.ThreadSentry.RelinquishThreadOwnership();
				Task.Run(() =>
				{
					person.Factory.ThreadSentry.TakeThreadOwnership();
				}).Wait();

				recalculateWorker.RegenerateCoreForTest();
			});
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestRegenerate_SendsDebugger_Information()
		{
			var participantMock = new Mock<IDeduplicationDebuggerParticipant>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.Address1 = "123 Fake St";
			person.PER_HomePhone = "0255555556";
			person.PER_EmailAddress = "fakeaddress@wisetechglobal.com";
			person.PER_DriversLicenseNumber = "555555556";

			participantMock.Setup(x => x.DebuggerName).Returns("PatternRecalculator");
			participantMock.Setup(x => x.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.IsAny<object>()));

			var needHashValue = person.Address1 + person.Address2 + person.City + person.Postcode + person.State;
			var firstHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue.ToUpperInvariant());

			Factory.Save();
			var recalculateWorker = new PatternMatchingRecalculator<GlbPerson>(person, participantMock.Object);
			recalculateWorker.Regenerate((DeduplicationGlbPerson)person.CreateIGlbPerson());
			var query = new ZQuery(PatternMatchingAddressSchema.PMA_PER, person.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, GlbPersonSchema.Constants.Prefix);
			var result = Factory.Load<PatternMatchingAddress>(query);
			AssertEquals("Expected:correct amount of address patterns were created", 1, result.Length);

			person.Address2 = "Bell Street CC2";
			Factory.Save();

			recalculateWorker.Regenerate((DeduplicationGlbPerson)person.CreateIGlbPerson());

			AssertEquals(10, recalculateWorker.DebuggerMessages.Count);
			AssertNotNull(DeduplicationUtils.DebuggerHubInstance.FindParticipant("PatternRecalculator"));
			participantMock.Verify(mock => mock.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.IsAny<object>()), Times.Exactly(2));
			participantMock.Verify(mock => mock.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationDebuggerMaster>>(), "Recalcaulate", It.IsAny<TimeSpan>(), It.Is<object>(u => (u as IGlbPerson) != null)), Times.Exactly(2));
		}
	}

	class PatternMatchingRecalculatorForTest<TBizo> : PatternMatchingRecalculator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		public PatternMatchingRecalculatorForTest(TBizo targetObject) : base(targetObject)
		{
		}

		public void SetTargetGlowBizO(object glowBizO)
		{
			targetGlowBizO = glowBizO;
		}

		public void RegenerateCoreForTest()
		{
			RegenerateCore();
		}
	}
}

#endif

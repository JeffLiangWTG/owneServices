using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationLockConfigExtensionTest : TestCaseWithFactory
	{
		public void TestGetEventLockInfoFromConfigThatMatchesLog()
		{
			var matchingLog = new Mock<IQueuedLog>();
			matchingLog.Setup(c => c.SJ_SE_NKEvent).Returns(AutoEvents.ArrivalCode);
			matchingLog.Setup(c => c.SJ_Reference).Returns("REF");
			matchingLog.Setup(c => c.SJ_ParentTableCode).Returns(JobDeclarationSchema.Constants.Prefix);

			var nonMatchingLog = new Mock<IQueuedLog>();
			nonMatchingLog.Setup(c => c.SJ_SE_NKEvent).Returns(AutoEvents.DepartureCode);
			nonMatchingLog.Setup(c => c.SJ_Reference).Returns("REF");
			nonMatchingLog.Setup(c => c.SJ_ParentTableCode).Returns(JobDeclarationSchema.Constants.Prefix);

			var queuedLogs = new[] { matchingLog.Object, nonMatchingLog.Object };

			CombineAssertions(() =>
			{
				DeclarationLockConfig config = null;
				AssertNull("Config Null: no matching Log", config.GetEventLockInfoFromConfigWhichMatchesLog(queuedLogs));

				config = new DeclarationLockConfig(null, Factory);
				var matchingEventInfo = config.EventInfos.AddNew();
				matchingEventInfo.EventType = AutoEvents.ArrivalCode;
				matchingEventInfo.EventReference = "REF";
				matchingEventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;
				matchingEventInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
				var nonMatchingEventInfo = config.EventInfos.AddNew();
				nonMatchingEventInfo.EventType = AutoEvents.ArrivalCode;
				nonMatchingEventInfo.EventReference = "REF2";
				nonMatchingEventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;
				nonMatchingEventInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
				AssertSame("Config has EventLockInfo with matching Log", matchingEventInfo, config.GetEventLockInfoFromConfigWhichMatchesLog(queuedLogs));
			});
		}

		public void TestMatchesLog()
		{
			CombineAssertions(() =>
			{
				var lockInfo = new DeclarationEventLockInfo { EventType = AutoEvents.ArrivalCode, EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration, EventReference = "REF", EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC };
				AssertEquals("MatchesLog", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Non matching EventType", expected: false, lockInfo.MatchesLog(AutoEvents.DepartureCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Non matching EventSource", expected: false, lockInfo.MatchesLog(AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Non matching EventReference", expected: false, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF1", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Non matching EntryType", expected: false, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.CommercialAccountingDeclaration));
			});
		}

		public void TestMatchesLog_IsMatchedEventSource()
		{
			var lockInfo = new DeclarationEventLockInfo { EventType = AutoEvents.ArrivalCode, EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration, EventReference = "REF", EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC };

			CombineAssertions(() =>
			{
				AssertEquals("Matches JE", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Matches JobDeclaration", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.TableName, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;
				AssertEquals("Matches CH", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, CusEntryHeaderSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
				AssertEquals("Matches CusEntryHeader", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, CusEntryHeaderSchema.Constants.TableName, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
				AssertEquals("Matches BH", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, CusInBondHeaderSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				AssertEquals("Matches CusInBondHeader", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, CusInBondHeaderSchema.Constants.TableName, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
			});
		}

		public void TestMatchesLog_IsMatchedEventReference()
		{
			CombineAssertions(() =>
			{
				var lockInfo = new DeclarationEventLockInfo { EventType = AutoEvents.ArrivalCode, EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration, EventReference = "*", EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC };
				AssertEquals("Matches *", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EventReference = "REF";
				AssertEquals("Matches REF", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EventReference = "R*";
				AssertEquals("Matches R*", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
			});
		}

		public void TestMatchesLog_IsMatchedEntryType()
		{
			CombineAssertions(() =>
			{
				var lockInfo = new DeclarationEventLockInfo { EventType = AutoEvents.ArrivalCode, EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration, EventReference = "*", EntryType = ZString.Empty };
				AssertEquals("Matches empty string", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
				AssertEquals("Matches All", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));

				lockInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC;
				AssertEquals("Matches B3C", expected: true, lockInfo.MatchesLog(AutoEvents.ArrivalCode, JobDeclarationSchema.Constants.Prefix, "REF", Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC));
			});
		}

		public void TestFind()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);
			var config1 = collection.AddNew();
			config1.DeclarationType = "IMP";

			var evnetInfo1 = config1.EventInfos.AddNew();
			evnetInfo1.EventType = AutoEvents.ArrivalCode;
			evnetInfo1.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			var config2 = collection.AddNew();
			config2.DeclarationType = "IMP";

			var evnetInfo2 = config2.EventInfos.AddNew();
			evnetInfo2.EventType = AutoEvents.DepartureCode;
			evnetInfo2.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;
			evnetInfo2.EventReference = string.Empty;

			var config = collection.Find("IMP");
			AssertEquals("Should find the first config with IMP.", config1, config);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Logs.AddNew(AutoEvents.Departure);

			config = collection.Find("IMP", declaration.Logs);
			AssertEquals("Should find the second config at it matched the event info.", config2, config);
		}
	}
}

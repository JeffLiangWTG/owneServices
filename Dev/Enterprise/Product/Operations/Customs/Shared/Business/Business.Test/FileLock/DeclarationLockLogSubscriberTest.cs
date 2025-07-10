using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationLockLogSubscriber))]
	sealed class DeclarationLockLogSubscriberTest : LogSubscriberTest<DeclarationLockLogSubscriber>
	{
		public void TestProcess_EventSource()
		{
			var expectedReference = "Declaration Lock Log Subscriber";
			var now = ZDateTime.Now;

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			importDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var exportEntryHeader = exportDeclaration.ActiveEntryHeaders.AddNew();
			exportEntryHeader.FillWithValidTestData();
			var importEntryHeader = importDeclaration.ActiveEntryHeaders.AddNew();
			importEntryHeader.FillWithValidTestData();
			((ICustomsFileParent)exportDeclaration).DeclarationTypeInfo.SetValueFromString("EXP");
			((ICustomsFileParent)importDeclaration).DeclarationTypeInfo.SetValueFromString("IMP");

			Factory.Save();

			var configs = new DeclarationLockConfigCollection(null, Factory);
			var exportConfig = AddConfig(configs, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);
			var importCconfig = AddConfig(configs, "IMP", AutoEvents.DepartureCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);
			var newFactory = NewFactory();
			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix, SJ_ParentID = exportDeclaration.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix, SJ_ParentID = importDeclaration.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeader.PK, SJ_SE_NKEvent = AutoEvents.DepartureCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = importEntryHeader.PK, SJ_SE_NKEvent = AutoEvents.DepartureCode, SJ_EventTime = now },
			};

			exportDeclaration = newFactory.Load<BaseJobDeclaration>(exportDeclaration.PK);
			importDeclaration = newFactory.Load<BaseJobDeclaration>(importDeclaration.PK);
			exportEntryHeader = newFactory.Load<CusEntryHeader>(exportEntryHeader.PK);
			importEntryHeader = newFactory.Load<CusEntryHeader>(importEntryHeader.PK);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
			{
				new DeclarationLockLogSubscriberForTest().ProcessLogs(logs);
				CombineAssertions(() =>
				{
					var lockLog = exportDeclaration.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the exportDeclaration matches the exportConfig.", lockLog);
					AssertEquals("Should add it from DeclarationLockLogSubscriber.", expectedReference, lockLog.SL_Reference);

					lockLog = importDeclaration.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the importDeclaration doesn't match the importConfig.", lockLog);

					lockLog = exportEntryHeader.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the exportEntryHeader doesn't match the exportConfig.", lockLog);

					lockLog = importEntryHeader.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the importEntryHeader doesn't match the importConfig.", lockLog);

					AssertEquals("ExportDeclaration locked, as all EntryHeaders have Lock-Event", expected: true, ((ICustomsFileParent)exportDeclaration).IsLocked);
					AssertEquals("ImportDeclaration not locked, as not all EntryHeaders have Lock-Event", expected: false, ((ICustomsFileParent)importDeclaration).IsLocked);
				});
			}
		}

		public void TestProcess_DeclarationLockMode_All()
		{
			var expectedReference = "Declaration Lock Log Subscriber";
			var now = ZDateTime.Now;

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var exportEntryHeader1 = exportDeclaration.ActiveEntryHeaders.AddNew();
			exportEntryHeader1.FillWithValidTestData();
			var exportEntryHeader2 = exportDeclaration.ActiveEntryHeaders.AddNew();
			exportEntryHeader2.FillWithValidTestData();
			((ICustomsFileParent)exportDeclaration).DeclarationTypeInfo.SetValueFromString("EXP");

			Factory.Save();

			var configs = new DeclarationLockConfigCollection(null, Factory);
			var exportConfig = AddConfig(configs, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader);
			var newFactory = NewFactory();
			var logs = new IQueuedLog[]	{ new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeader1.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now } };

			exportDeclaration = newFactory.Load<BaseJobDeclaration>(exportDeclaration.PK);
			exportEntryHeader1 = newFactory.Load<CusEntryHeader>(exportEntryHeader1.PK);
			exportEntryHeader2 = newFactory.Load<CusEntryHeader>(exportEntryHeader2.PK);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
			{
				var declarationLogSubscriber = new DeclarationLockLogSubscriberForTest();
				CombineAssertions(() =>
				{
					declarationLogSubscriber.ProcessLogs(logs);
					var lockLog = exportDeclaration.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the exportDeclaration doesn't match the exportConfig.", lockLog);

					lockLog = exportEntryHeader1.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the exportEntryHeader1 matches the exportConfig.", lockLog);
					AssertEquals("Should add it from DeclarationLockLogSubscriber.", expectedReference, lockLog.SL_Reference);

					lockLog = exportEntryHeader2.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the exportEntryHeader2 doesn't match the exportConfig.", lockLog);
					AssertEquals("Declaration not locked, as not all EntryHeaders have Lock-Event", expected: false, ((ICustomsFileParent)exportDeclaration).IsLocked);

					logs = new IQueuedLog[] { new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeader2.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now } };
					declarationLogSubscriber.ProcessLogs(logs);
					lockLog = exportEntryHeader2.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the exportEntryHeader2 matches the exportConfig.", lockLog);
					AssertEquals("Should add it from DeclarationLockLogSubscriber.", expectedReference, lockLog.SL_Reference);
					AssertEquals("Declaration locked, as all EntryHeaders have Lock-Event", expected: true, ((ICustomsFileParent)exportDeclaration).IsLocked);
				});
			}
		}

		public void TestProcess_DeclarationLockMode_Any()
		{
			var now = ZDateTime.Now;

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var exportEntryHeader1 = exportDeclaration.ActiveEntryHeaders.AddNew();
			exportEntryHeader1.FillWithValidTestData();
			var exportEntryHeader2 = exportDeclaration.ActiveEntryHeaders.AddNew();
			exportEntryHeader2.FillWithValidTestData();
			((ICustomsFileParent)exportDeclaration).DeclarationTypeInfo.SetValueFromString("EXP");

			Factory.Save();

			var configs = new DeclarationLockConfigCollection(null, Factory);
			var exportConfig = AddConfig(configs, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, lockMode: Core.Constants.Customs.DeclarationLockModes.Codes.Any);
			var newFactory = NewFactory();
			var logs = new IQueuedLog[]	{ new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeader2.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now } };

			exportDeclaration = newFactory.Load<BaseJobDeclaration>(exportDeclaration.PK);
			exportEntryHeader1 = newFactory.Load<CusEntryHeader>(exportEntryHeader1.PK);
			exportEntryHeader2 = newFactory.Load<CusEntryHeader>(exportEntryHeader2.PK);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
			{
				CombineAssertions(() =>
				{
					new DeclarationLockLogSubscriberForTest().ProcessLogs(logs);
					var lockLog = exportEntryHeader1.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the exportEntryHeader1 doesn't match the exportConfig.", lockLog);

					lockLog = exportEntryHeader2.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the exportEntryHeader2 matches the exportConfig.", lockLog);
					Assert("Declaration locked, as any EntryHeaders has Lock-Event", ((ICustomsFileParent)exportDeclaration).IsLocked);
				});
			}
		}

		public void TestProcess_EntryType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var now = ZDateTime.Now;

				var exportDeclarationCAD = Factory.NewWithValidTestData<BaseJobDeclaration>();
				exportDeclarationCAD.JE_GB = GlbBranch.CurrentBranch.PK;
				var exportEntryHeaderCAD = exportDeclarationCAD.ActiveEntryHeaders.AddNew();
				exportEntryHeaderCAD.FillWithValidTestData();
				var exportDeclarationBAC = Factory.NewWithValidTestData<BaseJobDeclaration>();
				exportDeclarationBAC.JE_GB = GlbBranch.CurrentBranch.PK;
				var exportEntryHeaderBAC = exportDeclarationBAC.ActiveEntryHeaders.AddNew();
				exportEntryHeaderBAC.FillWithValidTestData();
				((ICustomsFileParent)exportDeclarationCAD).DeclarationTypeInfo.SetValueFromString("EXP");
				((ICustomsFileParent)exportDeclarationBAC).DeclarationTypeInfo.SetValueFromString("EXP");
				exportEntryHeaderCAD.CH_MessageType = Core.Constants.Customs.EntryHeaderTypes.Codes.CommercialAccountingDeclaration;
				exportEntryHeaderBAC.CH_MessageType = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC;

				Factory.Save();

				var configs = new DeclarationLockConfigCollection(null, Factory);
				var exportConfig = AddConfig(configs, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, entryType: Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC);
				var newFactory = NewFactory();
				var logs = new IQueuedLog[]
				{
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeaderCAD.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix, SJ_ParentID = exportEntryHeaderBAC.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now },
				};

				exportDeclarationCAD = newFactory.Load<BaseJobDeclaration>(exportDeclarationCAD.PK);
				exportDeclarationBAC = newFactory.Load<BaseJobDeclaration>(exportDeclarationBAC.PK);
				exportEntryHeaderCAD = newFactory.Load<CusEntryHeader>(exportEntryHeaderCAD.PK);
				exportEntryHeaderBAC = newFactory.Load<CusEntryHeader>(exportEntryHeaderBAC.PK);

				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
				{
					var declarationLogSubscriber = new DeclarationLockLogSubscriberForTest();
					CombineAssertions(() =>
					{
						declarationLogSubscriber.ProcessLogs(logs);
						var lockLog = exportEntryHeaderCAD.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
						AssertNull("Should be null as the exportEntryHeaderCAD doesn't match the exportConfig.", lockLog);
						AssertEquals("exportDeclarationCAD not locked, as non of EntryHeaders have Lock-Event", expected: false, ((ICustomsFileParent)exportDeclarationCAD).IsLocked);

						lockLog = exportEntryHeaderBAC.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
						AssertNotNull("Should not be null as the exportEntryHeaderBAC matches the exportConfig.", lockLog);
						AssertEquals("exportDeclarationBAC locked, as all EntryHeaders have Lock-Event", expected: true, ((ICustomsFileParent)exportDeclarationBAC).IsLocked);
					});
				}
			}
		}

		public void TestEventTypes()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_IsActive = true;

			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			branch1.GB_IsActive = true;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_IsActive = true;

			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();
			branch2.GB_IsActive = true;

			Factory.Save();

			var configs1 = new DeclarationLockConfigCollection(null, Factory);
			AddConfig(configs1, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);
			AddConfig(configs1, "IMP", AutoEvents.DepartureCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);

			var configs2 = new DeclarationLockConfigCollection(null, Factory);
			AddConfig(configs2, "EXP", AutoEvents.ArrivalCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);
			AddConfig(configs2, "IMP", AutoEvents.ContainerPackCode, Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, configs1))
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, configs2))
			{
				var subscriber = new DeclarationLockLogSubscriberForTest();

				var expectedEventTypes = new[] { AutoEvents.ArrivalCode, AutoEvents.DepartureCode, AutoEvents.ContainerPackCode };
				var actualEventTypes = subscriber.EventTypes;

				AssertContainsExactElementsInAnyOrder(expectedEventTypes, actualEventTypes);
			}
		}

		DeclarationLockConfig AddConfig(DeclarationLockConfigCollection configs, string declarationType, string eventType, string eventSource, string lockMode = Core.Constants.Customs.DeclarationLockModes.Codes.All, string entryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All)
		{
			var config = configs.AddNew();
			config.DeclarationType = declarationType;
			config.LockMode = lockMode;

			var eventInfo = config.EventInfos.AddNew();
			eventInfo.EventType = eventType;
			eventInfo.EventSource = eventSource;
			eventInfo.EventReference = DeclarationEventLockInfo.DefaultMatchCharacter;
			using (eventInfo.GetValidationSuspender())
			{
				eventInfo.EntryType = entryType;
			}

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			return config;
		}

		public override void TestStmJobQueueIsNotSubscribedForEvents()
		{
			Assert(true);
		}

		#region Implement

		[Serializable]
		sealed class DeclarationLockLogSubscriberForTest : DeclarationLockLogSubscriber
		{
			public void ProcessLogs(IQueuedLog[] queuedLogs)
			{
				base.ProcessLogQueueItems(queuedLogs);
			}
		}

		#endregion
	}
}

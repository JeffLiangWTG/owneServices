using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CustomsStatusLogSubscriber))]
	sealed class CustomsStatusLogSubscriberTest : LogSubscriberTest<CustomsStatusLogSubscriber>
	{
		public void TestDontQueryStmALog()
		{
			var now = ZDateTimeOffset.Now;

			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();

			var outturn = outturnHeader.Outturns.AddNew();
			outturn.FillWithValidTestData();

			var log = outturn.Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now);

			Factory.Save();

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory)
				{
					SJ_ALogReference = log.PK,
					SJ_Reference = log.SL_Reference,
					SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix,
					SJ_ParentID = outturn.PK,
					SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode,
					SJ_EventTime = now.ToZDateTime()
				},
			};

			using (newFactory.AddDisposableService())
			{
				var subscriber = new CustomsStatusLogSubscriberForTest();
				subscriber.ProcessLogs(logs);

				newFactory.Save();

				var tableSelect = newFactory.TableSelects.FirstOrDefault(c => c.TableName == StmALogSchema.Constants.TableName);
				Assert("We need to be very careful whenever we read from dbo.StmALog as the table can have a huge data rows, so it's better to do not query any StmALog data in this log subscriber.", tableSelect.Value == 0);
			}
		}

		public void TestFactorySaveShouldNotBeCalledByLogSubscribers()
		{
			var now = ZDateTimeOffset.Now;

			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();

			var outturn = outturnHeader.Outturns.AddNew();
			outturn.FillWithValidTestData();

			var log = outturn.Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now);

			Factory.Save();

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory)
				{
					SJ_ALogReference = log.PK,
					SJ_Reference = log.SL_Reference,
					SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix,
					SJ_ParentID = outturn.PK,
					SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode,
					SJ_EventTime = now.ToZDateTime()
				},
			};

			using (new FactorySaveAlerter(() => "FactorySaveShouldNotBeCalledByLogSubscribers", "Factory.Save() should not be called by Log Subscribers"))
			{
				AssertNoExceptionThrown("Factory.Save() should not be called by Log Subscribers", () =>
				{
					using (newFactory.AddDisposableService())
					{
						var subscriber = new CustomsStatusLogSubscriberForTest();
						subscriber.ProcessLogs(logs);
					}
				});
			}
		}

		public void TestPorcessLogsWithHouseBill()
		{
			var now = ZDateTimeOffset.Now;

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MAWB = "MB20071400";

			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			var hawb3 = mawb.ChildBills.AddNew();
			var hawb4 = mawb.ChildBills.AddNew();

			hawb1.CS_HAWB = "HB20071401";
			hawb2.CS_HAWB = "HB20071402";
			hawb3.CS_HAWB = "HB20071403";
			hawb4.CS_HAWB = "HB20071404";

			hawb1.FillWithValidTestData();
			hawb2.FillWithValidTestData();
			hawb3.FillWithValidTestData();
			hawb4.FillWithValidTestData();

			var log1 = hawb1.Logs.AddNew(Events.CustomsEntryStatus, "|PCS", now);
			var log2 = hawb2.Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS|TYP=CLR", now);
			var log3 = hawb3.Logs.AddNew(Events.CustomsCleared, "|SER=PCS", now);

			Factory.Save();

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = CusHAWBSchema.Constants.Prefix, SJ_ParentID = hawb1.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() },
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = CusHAWBSchema.Constants.Prefix, SJ_ParentID = hawb2.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() },
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log3.PK, SJ_Reference = log3.SL_Reference, SJ_ParentTableCode = CusHAWBSchema.Constants.Prefix, SJ_ParentID = hawb3.PK, SJ_SE_NKEvent = AutoEvents.CustomsClearedCode, SJ_EventTime = now.ToZDateTime() },

				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusHAWBSchema.Constants.Prefix, SJ_ParentID = ZGuid.Empty, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusHAWBSchema.Constants.Prefix, SJ_ParentID = hawb4.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusMAWBSchema.Constants.Prefix, SJ_ParentID = mawb.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() },
			};

			using (newFactory.AddDisposableService())
			{
				var subscriber = new CustomsStatusLogSubscriberForTest();
				subscriber.ProcessLogs(logs);

				newFactory.Save();
			}

			CombineAssertions(() =>
			{
				AssertHasSentUEvent(hawb1, false, "Should not publish universal event on HAWB1 as the log doesnt contain 'SER=PCS'.");
				AssertHasSentUEvent(hawb3, false, "Should not publish universal event on HAWB3 as the event type is not CES.");
				AssertHasSentUEvent(hawb4, false, "Should not publish universal event on HAWB4 as it doesnt contain any CES logs.");

				AssertHasSentUEvent(hawb2, true, "Should publish universal event on HAWB2 as the event type is CES and the log contains 'SER=PCS'.");
			});

			var hawb2InNewFactory = newFactory.Load<CusHAWB>(hawb2.PK);
			var dataExportFailureLog = hawb2InNewFactory.Logs.Find((l) => l.SL_SE_NKEvent == Events.DataExportFailureCode).Single();

			AssertNotNull("Should create a Data Export Failure Log as there is no matched consignment data.", dataExportFailureLog);
			AssertEquals("Should log the failure reason and source customs status.", "|RES=Warning - No Module found a Business Entity to link this Universal Event to.|STA=CLR|TYP=CES", dataExportFailureLog.SL_Reference);
		}

		public void TestPorcessLogsWithSCAHouse()
		{
			var now = ZDateTime.Now;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				var oceanBill = Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();
				((BusinessObject)oceanBill).FillWithValidTestData();

				var houseBill1 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
				var houseBill2 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
				var houseBill3 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
				var houseBill4 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();

				houseBill1.FillWithValidTestData();
				houseBill2.FillWithValidTestData();
				houseBill3.FillWithValidTestData();
				houseBill4.FillWithValidTestData();

				houseBill1[CusSCAHouseSchema.CA_CB] = oceanBill.PK;
				houseBill2[CusSCAHouseSchema.CA_CB] = oceanBill.PK;
				houseBill3[CusSCAHouseSchema.CA_CB] = oceanBill.PK;
				houseBill4[CusSCAHouseSchema.CA_CB] = oceanBill.PK;

				var log1 = ((IStmALogParent)houseBill1).Logs.AddNew(Events.CustomsEntryStatus, "|PCS", now.ToOffset());
				var log2 = ((IStmALogParent)houseBill2).Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now.ToOffset());
				var log3 = ((IStmALogParent)houseBill3).Logs.AddNew(Events.CustomsCleared, "|SER=PCS", now.ToOffset());

				Factory.Save();

				var newFactory = NewFactory();

				var logs = new IQueuedLog[]
				{
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = houseBill1.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = houseBill2.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log3.PK, SJ_Reference = log3.SL_Reference, SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = houseBill3.PK, SJ_SE_NKEvent = AutoEvents.CustomsClearedCode, SJ_EventTime = now },

					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = ZGuid.Empty, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = houseBill4.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusSCAHouseSchema.Constants.Prefix, SJ_ParentID = oceanBill.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
				};

				using (newFactory.AddDisposableService())
				{
					var subscriber = new CustomsStatusLogSubscriberForTest();
					subscriber.ProcessLogs(logs);

					newFactory.Save();
				}

				CombineAssertions(() =>
				{
					AssertHasSentUEvent((IStmALogParent)houseBill1, false, "Should not publish universal event on houseBill1 as the log doesnt contain 'SER=PCS'.");
					AssertHasSentUEvent((IStmALogParent)houseBill3, false, "Should not publish universal event on houseBill3 as the event type is not CES.");
					AssertHasSentUEvent((IStmALogParent)houseBill4, false, "Should not publish universal event on houseBill4 as it doesnt contain any CES logs.");

					AssertHasSentUEvent((IStmALogParent)houseBill2, true, "Should publish universal event on houseBill2 as the event type is CES and the log contains 'SER=PCS'.");
				});
			}
		}

		public void TestProcessLogsWithAsycudaBill()
		{
			var now = ZDateTime.Now;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
				header.FillWithValidTestData();

				var asycudaBill1 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				var asycudaBill2 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				var asycudaBill3 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				asycudaBill1[AsycudaBillSchema.ABL_AMA] = header.PK;
				asycudaBill2[AsycudaBillSchema.ABL_AMA] = header.PK;
				asycudaBill3[AsycudaBillSchema.ABL_AMA] = header.PK;

				asycudaBill1.FillWithValidTestData();
				asycudaBill2.FillWithValidTestData();
				asycudaBill3.FillWithValidTestData();

				var log1 = ((IStmALogParent)asycudaBill1).Logs.AddNew(Events.CustomsEntryStatus, "|PCS", now.ToOffset());
				var log2 = ((IStmALogParent)asycudaBill2).Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now.ToOffset());

				Factory.Save();

				var newFactory = NewFactory();

				var logs = new IQueuedLog[]
				{
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill1.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill2.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill3.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
				};

				using (newFactory.AddDisposableService())
				{
					var subscriber = new CustomsStatusLogSubscriberForTest();
					subscriber.ProcessLogs(logs);

					newFactory.Save();
				}

				CombineAssertions(() =>
				{
					AssertHasSentUEvent((IStmALogParent)asycudaBill1, false, "Should not publish universal event on asycudaBill1 as the log doesnt contain 'SER=PCS'.");
					AssertHasSentUEvent((IStmALogParent)asycudaBill3, false, "Should not publish universal event on asycudaBill4 as it doesnt contain any CES logs.");

					AssertHasSentUEvent((IStmALogParent)asycudaBill2, true, "Should publish universal event on asycudaBill2 as the event type is CES and the log contains 'SER=PCS'.");
				});
			}
		}

		public void TestPorcessLogsWithOutturn()
		{
			var now = ZDateTime.Now;

			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();

			var outturn1 = outturnHeader.Outturns.AddNew();
			var outturn2 = outturnHeader.Outturns.AddNew();
			var outturn3 = outturnHeader.Outturns.AddNew();

			outturn1.FillWithValidTestData();
			outturn2.FillWithValidTestData();
			outturn3.FillWithValidTestData();

			var log1 = outturn1.Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now.ToOffset());
			var log2 = outturn2.Logs.AddNew(Events.CustomsCleared, string.Empty, now.ToOffset());

			Factory.Save();

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = outturn1.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = outturn2.PK, SJ_SE_NKEvent = AutoEvents.CustomsClearedCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = ZGuid.Empty, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = outturn3.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = outturnHeader.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
			};

			using (newFactory.AddDisposableService())
			{
				var subscriber = new CustomsStatusLogSubscriberForTest();
				subscriber.ProcessLogs(logs);

				newFactory.Save();
			}

			CombineAssertions(() =>
			{
				AssertHasSentUEvent(outturn1, true, "Should publish universal event on outturn1 as the event type is CES.");

				AssertHasSentUEvent(outturn2, false, "Should not publish universal event on outturn2 as the event type is not CES.");
				AssertHasSentUEvent(outturn3, false, "Should not publish universal event on outturn3 as there is no original log.");
			});
		}

		public void TestProcessLogsWithCusUSLVClearance()
		{
			var now = ZDateTime.Now;

			var clearance1 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVClearance>();
			var clearance2 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVClearance>();
			var clearance3 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVClearance>();
			clearance1.FillWithValidTestData();

			var consignment1 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVConsignment>();
			var consignment2 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVConsignment>();
			var consignment3 = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVConsignment>();
			consignment1[CusUSLVConsignmentSchema.ULB_ULH] = clearance1.PK;
			consignment2[CusUSLVConsignmentSchema.ULB_ULH] = clearance2.PK;
			consignment3[CusUSLVConsignmentSchema.ULB_ULH] = clearance3.PK;

			consignment1.FillWithValidTestData();
			consignment2.FillWithValidTestData();
			consignment3.FillWithValidTestData();

			var log1 = ((IStmALogParent)consignment1).Logs.AddNew(Events.MessageStatusChange, "|PCS", now.ToOffset());
			var log2 = ((IStmALogParent)consignment2).Logs.AddNew(Events.MessageStatusChange, "|CRF=SV971002057|MST=Cargo Release|NEW=REL|RFN=PI3151202078|SER=PCS|TYP=SO", now.ToOffset());

			Factory.Save();

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = CusUSLVClearanceSchema.Constants.Prefix, SJ_ParentID = clearance1.PK, SJ_SE_NKEvent = AutoEvents.MessageStatusChangeCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = CusUSLVClearanceSchema.Constants.Prefix, SJ_ParentID = clearance2.PK, SJ_SE_NKEvent = AutoEvents.MessageStatusChangeCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusUSLVClearanceSchema.Constants.Prefix, SJ_ParentID = clearance3.PK, SJ_SE_NKEvent = AutoEvents.MessageStatusChangeCode, SJ_EventTime = now },
			};

			using (newFactory.AddDisposableService())
			{
				var subscriber = new CustomsStatusLogSubscriberForTest();
				subscriber.ProcessLogs(logs);

				newFactory.Save();
			}

			CombineAssertions(() =>
			{
				AssertHasSentUEvent((IStmALogParent)clearance1, false, "Should not publish universal event on clearance1 as the log doesnt contain '|SER=PCS'.");
				AssertHasSentUEvent((IStmALogParent)clearance3, false, "Should not publish universal event on clearance3 as it doesnt contain any MSC logs.");

				AssertHasSentUEvent((IStmALogParent)clearance2, true, "Should publish universal event on clearance2 as the event type is MSC and the log contains '|SER=PCS'.");
			});
		}

		public void TestProcessLogs_FilterQueuedLogs()
		{
			var now = ZDateTime.Now;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
				header.FillWithValidTestData();

				var asycudaBill1 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				var asycudaBill2 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				var asycudaBill3 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaBill>();
				asycudaBill1[AsycudaBillSchema.ABL_AMA] = header.PK;
				asycudaBill2[AsycudaBillSchema.ABL_AMA] = header.PK;
				asycudaBill3[AsycudaBillSchema.ABL_AMA] = header.PK;

				asycudaBill1.FillWithValidTestData();
				asycudaBill2.FillWithValidTestData();
				asycudaBill3.FillWithValidTestData();

				var log1 = ((IStmALogParent)asycudaBill1).Logs.AddNew(Events.CustomsEntryStatus, "|PCS", now.ToOffset());
				var log2 = ((IStmALogParent)asycudaBill2).Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS", now.ToOffset());
				var log3 = ((IStmALogParent)asycudaBill1).Logs.AddNew(Events.MessageStatusChange, "REJ", now.ToOffset());
				var log4 = ((IStmALogParent)asycudaBill2).Logs.AddNew(Events.MessageStatusChange, "INV", now.ToOffset());

				Factory.Save();

				var newFactory = NewFactory();

				var logs = new IQueuedLog[]
				{
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log1.PK, SJ_Reference = log1.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill1.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log2.PK, SJ_Reference = log2.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill2.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill3.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log3.PK, SJ_Reference = log3.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill1.PK, SJ_SE_NKEvent = AutoEvents.MessageStatusChangeCode, SJ_EventTime = now },
					new QueuedLogForTesting(newFactory) { SJ_ALogReference = log4.PK, SJ_Reference = log4.SL_Reference, SJ_ParentTableCode = AsycudaBillSchema.Constants.Prefix, SJ_ParentID = asycudaBill2.PK, SJ_SE_NKEvent = AutoEvents.MessageStatusChangeCode, SJ_EventTime = now },
				};

				using (newFactory.AddDisposableService())
				{
					var subscriber = new CustomsStatusLogSubscriberForTest();
					subscriber.ProcessLogs(logs);

					newFactory.Save();
				}

				CombineAssertions(() =>
				{
					AssertHasSentUEvent((IStmALogParent)asycudaBill1, false, "Should not publish universal event on asycudaBill1 as the log doesnt contain 'SER=PCS'.");

					var asycudaBill2DexLogs = AssertHasSentUEvent((IStmALogParent)asycudaBill2, true, "Should publish universal event on asycudaBill2 as the event type is CES and the log contains 'SER=PCS'.");
					AssertEquals("Only CES events that meet the filter generate DEX events, MSC events will not generate DEX events.", 1, asycudaBill2DexLogs.Count());

					AssertHasSentUEvent((IStmALogParent)asycudaBill3, false, "Should not publish universal event on asycudaBill4 as it doesnt contain any CES logs.");
				});
			}
		}

		IEnumerable<StmALog> AssertHasSentUEvent(IStmALogParent logParent, bool expectedValue, string message)
		{
			var dexLogs = logParent.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.DataExportCode);
			var exportLog = dexLogs.FirstOrDefault();

			AssertEquals(message, expectedValue, exportLog != null);

			if (exportLog != null)
			{
				var query = new ZQuery();
				query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
				query.AddToFilter(GenPivotSchema.XX_Relation1ID, exportLog.PK);
				query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

				var pivot = NewFactory().LoadTop1<GenPivot>(query);
				AssertNotNull(message, pivot);

				var ediMessage = Factory.Load<EDIMessage>(pivot.XX_Relation2ID);
				AssertEquals("Should create an UDM message.", ApplicationCodeList.Codes.UniversalDataMessaging, ediMessage.EM_ApplicationCode);
				AssertEquals("The message type should be internal for processing directly.", ReceiveTransmitList.Codes.Internal, ediMessage.EM_ReceiveTransmit);
			}

			return dexLogs;
		}

		[ExpectNoExceptions]
		public void TestProcessWithEmptyLogs()
		{
			var logs = Array.Empty<IQueuedLog>();

			var subscriber = new CustomsStatusLogSubscriberForTest();
			subscriber.ProcessLogs(logs);
		}

		public override void TestStmJobQueueIsNotSubscribedForEvents()
		{
			Assert(true);
		}

		#region Implement

		[Serializable]
		sealed class CustomsStatusLogSubscriberForTest : CustomsStatusLogSubscriber
		{
			public void ProcessLogs(IQueuedLog[] queuedLogs)
			{
				var allPrefix = TableNames.Select(c => EnterpriseSchema.GetTableSchema(c)?.PK.ColumnPrefix ?? string.Empty);
				var filterLogs = queuedLogs.Where(c => allPrefix.Any(d => d == c.SJ_ParentTableCode) && EventTypes.Any(d => d == c.SJ_SE_NKEvent)).ToArray();

				base.ProcessLogQueueItems(filterLogs);
			}
		}

		#endregion
	}
}

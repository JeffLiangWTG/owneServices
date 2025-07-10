using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	public abstract class HVLVReferenceValidationServiceTaskTest<T> : ServiceTaskTestCase<HVLVReferenceValidationServiceTask> where T : EnterpriseBusinessObject
	{
		public void TestValidatorDoesNotRun_WhenAutoGenerateIDsRegistrySettingIsEnabled()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var record1 = CreateNewRecordWithId("ABC");
				MarkAsValidatedForUniqueness(record1);
				Factory.Save();

				var task = CreateAndRunTask();
				var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

				AssertEquals("There are no logs as the service task shouldn't run.", logs.Count, 0);
			}
		}

		public void TestValidatorRuns_WhenAutoGenerateIDsRegistrySettingIsDisabled_AndConsignmentsOrItemsExistToValidateForUniqueness()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var record1 = CreateNewRecordWithId("ABC");
				MarkAsValidatedForUniqueness(record1);
				var record2 = CreateNewRecordWithId("ABC");
				Factory.Save();

				var task = CreateAndRunTask();
				var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

				record1.Reload();
				record2.Reload();

				CombineAssertions("The records are modified because the task runs (preconditions met)", () =>
				{
					AssertEquals("ABC", GetId(record1));
					AssertNotEquals("ABC", GetId(record2));
					Assert(IsValidatedForUniqueness(record1));
					Assert(IsValidatedForUniqueness(record2));
					Assert("There are logs that confirm that the service task has run.", logs.Count > 0);
				});
			}
		}

		public void TestTaskGoesOnWithNullHVH_OA_BillToParty()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				const int clusterKeyForTest = 9999;
				const string idForTest = "FakeIdForTest";
				CreateNewRecordWithId(idForTest);
				var record = CreateNewRecordWithId(idForTest);
				Factory.Save();

				SetClusterKey(record, clusterKeyForTest);
				AssertNull($"No Booking Headers with ClusterKey ({clusterKeyForTest})", Factory.LoadTop1<HVLVBookingHeader>(new ZQuery(HVLVBookingHeaderSchema.HVH_ClusterKey, clusterKeyForTest)));
				Factory.Save();

				Assert("It's not validated yet", !IsValidatedForUniqueness(record));

				HVLVReferenceValidationServiceTask task;
				AssertNoExceptionThrown("Service task should not fail", () => { task = CreateAndRunTask(); });

				record.Reload();
				AssertNotEquals("ConsignmentId has been re-assigned", idForTest, GetId(record));
			}
		}

		public void TestDuplicateId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var record1 = CreateNewRecordWithId("ABC");
				MarkAsValidatedForUniqueness(record1);
				var record2 = CreateNewRecordWithId("ABC");
				Factory.Save();

				var task = CreateAndRunTask();

				var expectedNewId = FormatId(1);
				var expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} {record2.PK} ID updated from 'ABC' to '{expectedNewId}'$",
					$"^Debug\\|1 {TableName} row validated$"
				};
				AssertLogsRegex(task, expectedLogRegexes);

				record1.Reload();
				record2.Reload();

				AssertEquals("ABC", GetId(record1));
				AssertEquals(expectedNewId, GetId(record2));

				Assert(IsValidatedForUniqueness(record1));
				Assert(IsValidatedForUniqueness(record2));

				AssertNoCIDEvent(record1);
				AssertHasCIDEvent(record2, "ABC", expectedNewId);
			}
		}

		public void TestDuplicateIdsInSameBatch()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var record1 = CreateNewRecordWithId("ABC");
				var record2 = CreateNewRecordWithId("ABC");
				Factory.Save();

				var task = CreateAndRunTask();

				var expectedNewId = FormatId(1);
				var expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} (({record1.PK})|({record2.PK})) ID updated from 'ABC' to '{expectedNewId}'$",
					$"^Debug\\|2 {TableName} rows validated$"
				};
				AssertLogsRegex(task, expectedLogRegexes);

				record1.Reload();
				record2.Reload();

				AssertContainsExactElementsInAnyOrder(new[] { "ABC", expectedNewId }, new[] { GetId(record1), GetId(record2) });
				Assert(IsValidatedForUniqueness(record1));
				Assert(IsValidatedForUniqueness(record2));
			}
		}

		public void TestNoDuplicates()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var record1 = CreateNewRecordWithId("ABC");
				var record2 = CreateNewRecordWithId("XYZ");
				Factory.Save();

				var task = CreateAndRunTask();

				var expectedLogRegexes = new[]
				{
					$"^Debug\\|2 {TableName} rows validated$"
				};
				AssertLogsRegex(task, expectedLogRegexes);

				record1.Reload();
				record2.Reload();

				AssertEquals("ABC", GetId(record1));
				AssertEquals("XYZ", GetId(record2));

				Assert(IsValidatedForUniqueness(record1));
				Assert(IsValidatedForUniqueness(record2));
			}
		}

		public void TestNextNumbersToBeGeneratedOccupied()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var validatedRecords = new[] { 3, 4, 5 }.Select(x => CreateNewRecordWithId(FormatId(x))).ToList();
				validatedRecords.AddRange(Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString())));

				foreach (var record in validatedRecords)
				{
					MarkAsValidatedForUniqueness(record);
				}

				var newRecords = Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString())).ToArray();
				Factory.Save();

				var expectedNewIds = new[] { 1, 2, 3, 4 }.Select(FormatId).ToArray();
				var anyRecordPkAndIdRegex = string.Join("|", newRecords.Select(x => $"({x.PK} ID updated from '{GetId(x)}')"));
				var expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[0]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[1]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[2]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[3]}'$",
					$"^Debug\\|2 {TableName} rows validated$"
				};

				var task = CreateAndRunTask();
				AssertLogsRegex(task, expectedLogRegexes);

				foreach (var record in newRecords)
				{
					record.Reload();
				}

				expectedNewIds = new[] { 6, 7 }.Select(FormatId).ToArray();
				anyRecordPkAndIdRegex = string.Join("|", newRecords.Where(x => !IsValidatedForUniqueness(x)).Select(x => $"({x.PK} ID updated from '{GetId(x)}')"));
				expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[0]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[1]}'$",
					$"^Debug\\|2 {TableName} rows validated$"
				};

				task = CreateAndRunTask();
				AssertLogsRegex(task, expectedLogRegexes);

				foreach (var record in newRecords)
				{
					record.Reload();
					Assert(IsValidatedForUniqueness(record));
				}

				var expectedIds = new[] { 1, 2, 6, 7 }.Select(FormatId);
				AssertContainsExactElementsInAnyOrder(expectedIds, newRecords.Select(GetId));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestNextNumbersToBeGeneratedOccupied_SSCCFountain()
		{
			var factory = new BusinessObjectFactory();
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				const string gs1Prefix = "1234567";
				HVLVTestHelper.SetGS1FountainOnOrgProxy(new BusinessObjectFactory(), gs1Prefix);

				var validatedRecords = new[] { 3, 4, 5 }.Select(x => CreateNewRecordWithId(GetSSCCNumber(x, gs1Prefix), factory)).ToList();
				validatedRecords.AddRange(Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString(), factory)));

				foreach (var record in validatedRecords)
				{
					MarkAsValidatedForUniqueness(record);
				}

				var newRecords = Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString(), factory)).ToArray();

				foreach (var record in newRecords)
				{
					// To ensure they are processed in the same batch
					SetShipperOrgAddressPk(record, GetShipperOrgAddressPk(newRecords[0]));
				}

				factory.Save();

				var expectedNewIds = new[] { 1, 2, 3, 4 }.Select(x => GetSSCCNumber(x, gs1Prefix)).ToArray();
				var anyRecordPkAndIdRegex = string.Join("|", newRecords.Select(x => $"({x.PK} ID updated from '{GetId(x)}')"));
				var expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[0]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[1]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[2]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[3]}'$",
					$"^Debug\\|2 {TableName} rows validated$"
				};

				var task = CreateAndRunTask();
				AssertLogsRegex(task, expectedLogRegexes);

				foreach (var record in newRecords)
				{
					record.Reload();
				}

				expectedNewIds = new[] { 6, 7 }.Select(x => GetSSCCNumber(x, gs1Prefix)).ToArray();
				anyRecordPkAndIdRegex = string.Join("|", newRecords.Where(x => !IsValidatedForUniqueness(x)).Select(x => $"({x.PK} ID updated from '{GetId(x)}')"));
				expectedLogRegexes = new[]
				{
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[0]}'$",
					$"^Debug\\|{TableName} ({anyRecordPkAndIdRegex}) to '{expectedNewIds[1]}'$",
					$"^Debug\\|2 {TableName} rows validated$"
				};

				task = CreateAndRunTask();
				AssertLogsRegex(task, expectedLogRegexes);

				foreach (var record in newRecords)
				{
					record.Reload();
					Assert(IsValidatedForUniqueness(record));
				}

				var expectedIds = new[] { 1, 2, 6, 7 }.Select(x => GetSSCCNumber(x, gs1Prefix));
				AssertContainsExactElementsInAnyOrder(expectedIds, newRecords.Select(GetId));
			}
		}

		public void TestInformationCountShouldBeZero()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var validatedRecords = new[] { 3, 4, 5 }.Select(x => CreateNewRecordWithId(FormatId(x))).ToList();
				validatedRecords.AddRange(Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString())));

				foreach (var record in validatedRecords)
				{
					MarkAsValidatedForUniqueness(record);
				}
				var newRecords = Enumerable.Range(0, 4).Select(x => CreateNewRecordWithId(x.ToString())).ToArray();
				Factory.Save();

				var task = CreateAndRunTask();
				var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
				var informationLogCount = logs.Where(l => l.Split('|')[0] == "Information").Count();

				AssertEquals("Information Log count should be 0", 0, informationLogCount);
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Precondition: expected single attribute", 1, hostedServiceAttributes.Length);

			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
				Assert("IsMandatory", hostedServiceAttribute.IsMandatory);
				Assert("ActiveByDefault", hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestActiveByDefault()
		{
			var task = new HVLVReferenceValidationServiceTask();
			InitialiseTaskSchedule(task, out var schedule);

			Assert("Service task is active by default", ((StmServiceTask)schedule).SST_Active);
		}

		void AssertHasCIDEvent(T bizo, string oldValue, string newValue)
		{
			var cidEvent = bizo.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ChangeOfIdentifierCode).FirstOrDefault();
			AssertNotNull(cidEvent);
			Assert(cidEvent.IsInDatabase);
			AssertEquals(oldValue, cidEvent.Parameters[Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals(newValue, cidEvent.Parameters[Constants.EventReferenceParameters.Codes.New]);
		}

		void AssertNoCIDEvent(T bizo) => Assert(!bizo.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ChangeOfIdentifierCode).Any());

		void AssertLogsRegex(HVLVReferenceValidationServiceTask task, string[] expectedLogRegexes)
		{
			var actualLogsToMatch = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var actualLogsMatched = new List<string>();
			var expectedLogRegexesMatched = new bool[expectedLogRegexes.Length];

			foreach (var i in Enumerable.Range(0, expectedLogRegexes.Length))
			{
				var matchedActualLog = actualLogsToMatch.FirstOrDefault(x => Regex.IsMatch(x, expectedLogRegexes[i]));

				if (matchedActualLog != null)
				{
					actualLogsMatched.Add(matchedActualLog);
					actualLogsToMatch.Remove(matchedActualLog);
					expectedLogRegexesMatched[i] = true;
				}
			}

			CombineAssertions(() =>
			{
				var expectedButNotMatched = expectedLogRegexes.Where((x, i) => !expectedLogRegexesMatched[i]).ToArray();

				if (expectedButNotMatched.Any())
				{
					Fail($@"The following expected log regexes did not match:

{string.Join(System.Environment.NewLine, expectedButNotMatched)}");
				}

				if (actualLogsToMatch.Any())
				{
					Fail($@"The following actual logs were not matched:

{string.Join(System.Environment.NewLine, actualLogsToMatch)}");
				}
			});
		}

		protected virtual HVLVReferenceValidationServiceTask CreateAndRunTask()
		{
			var task = new HVLVReferenceValidationServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			return task;
		}

		string GetSSCCNumber(int num, string prefix)
		{
			var numberWithoutCheckDigit = SSCCBarCodeChecker.GetSSCCWithoutCheckDigit(num, prefix);
			return numberWithoutCheckDigit + SSCCBarCodeChecker.GetCheckDigit(numberWithoutCheckDigit);
		}

		protected abstract void SetClusterKey(T record, int key);

		protected abstract int GetClusterKey(T record);

		protected abstract T CreateNewRecordWithId(string id, BusinessObjectFactory factory = null);

		// Calling NewWithValidTestData on thousands of records is too slow, because each creates new data for all related records when in reality they will share a lot of this data.
		protected abstract T CreateNewRecordWithIdForBatch(string id);

		protected abstract void MarkAsValidatedForUniqueness(T record);

		protected abstract bool IsValidatedForUniqueness(T record);

		protected abstract string GetId(T record);

		protected abstract string FormatId(int num);

		protected abstract string TableName { get; }

		protected abstract ZGuid GetShipperOrgAddressPk(T record);

		protected abstract void SetShipperOrgAddressPk(T record, ZGuid value);

		protected static string Format(long number, string prefix, int formatDigits)
		{
			return prefix + number.ToString(CultureInfo.InvariantCulture).PadLeft(formatDigits, '0');
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						HVLVConsignmentSchema.Constants.TableName,
						"HVLV Consignment Uniqueness check",
						HVLVConsignmentSchema.Constants.HVC_IsValidatedForUniqueness + "=N"),

					new TaskNudgeInformationForTest(
						HVLVItemSchema.Constants.TableName,
						"HVLV Item Uniqueness check",
						HVLVItemSchema.Constants.HVI_IsValidatedForUniqueness + "=N"),
				};
			}
		}
	}

	[TestedType(typeof(HVLVConsignment))]
	public class ConsignmentHVLVReferenceValidateServiceTaskTest : HVLVReferenceValidationServiceTaskTest<HVLVConsignment>
	{
		protected override HVLVConsignment CreateNewRecordWithId(string id, BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;
			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = string.Empty;
			consignment.HVC_WaybillNumber = id;

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.Add(consignment);

			return consignment;
		}

		protected override HVLVConsignment CreateNewRecordWithIdForBatch(string id)
		{
			var consignment = Header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = id;
			return consignment;
		}

		HVLVBookingHeader Header => header ?? (header = Factory.NewWithValidTestData<HVLVBookingHeader>());
		HVLVBookingHeader header;

		protected override void MarkAsValidatedForUniqueness(HVLVConsignment record) => record.HVC_IsValidatedForUniqueness = true;

		protected override bool IsValidatedForUniqueness(HVLVConsignment record) => record.HVC_IsValidatedForUniqueness;

		protected override string GetId(HVLVConsignment record) => record.HVC_ConsignmentId;

		protected override string FormatId(int num) => Format(num, "HVC", 15);

		protected override string TableName => HVLVConsignmentSchema.Constants.TableName;

		protected override ZGuid GetShipperOrgAddressPk(HVLVConsignment record) => record.BookingHeader.HVH_OA_BillToParty;

		protected override void SetShipperOrgAddressPk(HVLVConsignment record, ZGuid value) => record.BookingHeader.HVH_OA_BillToParty = value;

		protected override void SetClusterKey(HVLVConsignment record, int key)
		{
			record.HVC_ClusterKey = key;
		}

		protected override int GetClusterKey(HVLVConsignment record) => record.HVC_ClusterKey;
	}

	[TestedType(typeof(HVLVItem))]
	public class ItemHVLVReferenceValidateServiceTaskTest : HVLVReferenceValidationServiceTaskTest<HVLVItem>
	{
		protected override HVLVItem CreateNewRecordWithId(string id, BusinessObjectFactory factory)
		{
			factory = factory ?? Factory;
			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_CurrentBarcode = id;
			item.Consignment.HVC_IsValidatedForUniqueness = true;

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.Add(item.Consignment);

			return item;
		}

		protected override HVLVItem CreateNewRecordWithIdForBatch(string id)
		{
			var consignment = Header.Consignments.AddNew();

			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = id;
			return item;
		}

		HVLVBookingHeader Header => header ?? (header = Factory.NewWithValidTestData<HVLVBookingHeader>());
		HVLVBookingHeader header;

		protected override HVLVReferenceValidationServiceTask CreateAndRunTask()
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			foreach (var consignment in Factory.Load<HVLVConsignment>(query))
			{
				consignment.HVC_IsValidatedForUniqueness = true;
			}

			Factory.Save();

			return base.CreateAndRunTask();
		}

		protected override void MarkAsValidatedForUniqueness(HVLVItem record) => record.HVI_IsValidatedForUniqueness = true;

		protected override bool IsValidatedForUniqueness(HVLVItem record) => record.HVI_IsValidatedForUniqueness;

		protected override string GetId(HVLVItem record) => record.HVI_ItemId;

		protected override string FormatId(int num) => Format(num, "HVI", 15);

		protected override string TableName => HVLVItemSchema.Constants.TableName;

		protected override ZGuid GetShipperOrgAddressPk(HVLVItem record) => record.Consignment.BookingHeader.HVH_OA_BillToParty;

		protected override void SetShipperOrgAddressPk(HVLVItem record, ZGuid value) => record.Consignment.BookingHeader.HVH_OA_BillToParty = value;

		protected override void SetClusterKey(HVLVItem record, int key)
		{
			record.HVI_ClusterKey = key;
		}

		protected override int GetClusterKey(HVLVItem record) => record.HVI_ClusterKey;
	}
}

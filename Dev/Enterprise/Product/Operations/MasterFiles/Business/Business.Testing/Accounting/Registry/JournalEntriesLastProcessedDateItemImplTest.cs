using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Registry
{
	[TestedType(typeof(JournalEntriesLastProcessedDateItemImpl))]
	sealed class JournalEntriesLastProcessedDateItemImplTest : StronglyTypedRegistryItemTestCase<DateTime>
	{
		public void TestCollectGLPData()
		{
			CreateTestPeriodsForEntireYear(2023);
			CreateTestPeriodsForEntireYear(2024);

			var item = GetNewRegistryItem();
			item.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 1, 1));

			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Wrong number of rows in EDIMessage table.", 0, ediMessages.Length);

			item.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);
			var jObject = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail)).First();
			AssertEquals("GLP", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureCode", StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("Accounting", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("Module", StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("Completed Journal Entries Backlog Processing", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureDescription", StringComparison.InvariantCulture))?.Value.ToString());
		}

		void CreateTestPeriodsForEntireYear(int year)
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		protected override StronglyTypedRegistryItem<DateTime, DateTime> GetNewRegistryItem()
		{
			return new JournalEntriesLastProcessedDateItemImpl(
					"JournalEntriesLastProcessedDate",
					AccountingMasterFilesRegistry.Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
					(NoResString)"Journal Entries Last Processed Date",
					(NoResString)"When all journal entries queue records have been processed by 'GLP - General Ledger Data Backlog Process Service Task', the current 'Generate Journal Entries - Start Date' registry value will be saved into this registry.",
					RegistryStorageFlags.Company,
					RegistryOptions.IsReadOnly);
		}

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Workflow.Business.Test
{
	sealed class AuditStmALogDeciderTest : TestCaseWithFactory
	{
		public void TestIsAutoLoggedFallbackFromRegistry()
		{
			// Arrange
			var testAuditStmALogDecider = new AuditStmALogDeciderForTesting();
			var dummyBizo1 = Factory.NewWithValidTestData<DummyBizOWithAutoLogs>();
			var testCollection = new EnableAddEditAndDeleteLogsItemCollection
			{
				new EnableAddEditAndDeleteLogsItem() { Table = dummyBizo1.TableName, EnableADDLogs = true, EnableEDTLogs = false, EnableDELLogs = false }
			};

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				// Act
				testAuditStmALogDecider.AddToAutoLoggedDefaultConfigurationValues(dummyBizo1.TableName, false, false, false);
				// Assert
				Assert("dummyBizo1 should be autologged as it is in the dictionary with ADD set to true in the registry.", dummyBizo1.IsAutoAdminBusinessObjectLoggerEnabled);
			}
		}

		public void TestIsAutoLoggedFallbackFromOverridenFallbackLevel()
		{
			// Arrange
			var dummyBizo1 = Factory.NewWithValidTestData<DummyBizOWithAutoLogs>();
			var dummyBizo2 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			// Act
			// Assert
			Assert("dummyBizo1 should be autologged due to the fallback being true.", dummyBizo1.IsAutoAdminBusinessObjectLoggerEnabled);
			Assert("dummyBizo2 should not be autologged due to the fallback being false.", !(dummyBizo2.IsAutoAdminBusinessObjectLoggerEnabled));
		}

		public void TestIsAutoLoggedForAddEdtAndDelEvents()
		{
			// Arrange
			var testAuditStmALogDecider = new AuditStmALogDeciderForTesting();
			var dummyBizo1 = Factory.NewWithValidTestData<DummyEnterpriseBizo>();
			var testCollection = new EnableAddEditAndDeleteLogsItemCollection()
			{
				new EnableAddEditAndDeleteLogsItem() { Table = dummyBizo1.TableName, EnableADDLogs = false, EnableEDTLogs = false, EnableDELLogs = true }
			};

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				// Act
				testAuditStmALogDecider.ClearAutoLoggedDefaultConfigurationValues();
				testAuditStmALogDecider.AddToAutoLoggedDefaultConfigurationValues(dummyBizo1.TableName, false, false, true);
				testAuditStmALogDecider.TryGetAutoLogConfigurations(dummyBizo1, out var config);

				// Assert
				Assert("dummyBizo1 ADD should not be autologged as it is set to false in the registry.", !config.IsEnabledForADD);
				Assert("dummyBizo1 EDT should not be autologged as it is set to false in the registry.", !config.IsEnabledForEDT);
				Assert("tedummyBizo1stItem1 DEL should be autologged as it is set to true in the registry.", config.IsEnabledForDEL);
			}
		}

		public void TestAuditLogConfigNonPersisted_StateInRegistry()
		{
			var dummy = Factory.New<DummyLoggedConfigurable>();
			dummy.OverridenAutoLogState = EnterpriseBusinessObject.AutologState.NotLogged;
			var testAuditDecider = new AuditStmALogDecider();
			var testCollection = new EnableAddEditAndDeleteLogsItemCollection()
			{
				new EnableAddEditAndDeleteLogsItem() { Table = dummy.TableName, EnableADDLogs = false, EnableEDTLogs = true, EnableDELLogs = true }
			};

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				Assert("Should be non persisted as its in registry as false for ADD", testAuditDecider.AuditLogConfigNonPersistedForEventCode(dummy, AutoEvents.AddedARecordToTheSystemCode));
				Assert("Should be persisted as its in registry as true for EDT", !testAuditDecider.AuditLogConfigNonPersistedForEventCode(dummy, AutoEvents.EditedARecordCode));
			}
		}

		public void TestAuditLogConfigNonPersisted_StateAutoLogged()
		{
			var dummy = Factory.New<DummyLoggedConfigurable>();
			dummy.OverridenAutoLogState = EnterpriseBusinessObject.AutologState.AutoLogged;
			var auditDecider = new AuditStmALogDecider();
			Assert("Should be persisted as its not in registry + IsAutoLogged", !auditDecider.AuditLogConfigNonPersistedForEventCode(dummy, AutoEvents.AddedARecordToTheSystemCode));
		}

		public void TestAuditLogConfigNonPersisted_StateAutoLoggedToQueueOnly()
		{
			var dummy = Factory.New<DummyLoggedConfigurable>();
			dummy.OverridenAutoLogState = EnterpriseBusinessObject.AutologState.AutoLoggedToQueueOnly;
			var auditDecider = new AuditStmALogDecider();
			Assert("Should be non persisted as its AutoLoggedToQueueOnly", auditDecider.AuditLogConfigNonPersistedForEventCode(dummy, AutoEvents.AddedARecordToTheSystemCode));
		}

		public void TestLoggerEnabled_StateInRegistry()
		{
			var dummy = Factory.New<DummyLoggedConfigurable>();
			var testAuditDecider = new AuditStmALogDecider();
			var testCollection = new EnableAddEditAndDeleteLogsItemCollection()
			{
				new EnableAddEditAndDeleteLogsItem() { Table = dummy.TableName, EnableADDLogs = false, EnableEDTLogs = true, EnableDELLogs = true }
			};
			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				Assert("Should be enabled as its in registry", testAuditDecider.IsAutoAdminBusinessObjectLoggerEnabled(dummy, EnterpriseBusinessObject.AutologState.NotLogged));
			}
		}

		public void TestLoggerEnabled_AllAutoLogStates()
		{
			var dummy = Factory.New<DummyLoggedConfigurable>();
			var testAuditDecider = new AuditStmALogDecider();
			Assert("Should be enabled as its AutoLogged", testAuditDecider.IsAutoAdminBusinessObjectLoggerEnabled(dummy, EnterpriseBusinessObject.AutologState.AutoLogged));
			Assert("Should be enabled as its AutoLoggedToQueueOnly", testAuditDecider.IsAutoAdminBusinessObjectLoggerEnabled(dummy, EnterpriseBusinessObject.AutologState.AutoLoggedToQueueOnly));
			Assert("Should not be enabled as its NotLogged", !testAuditDecider.IsAutoAdminBusinessObjectLoggerEnabled(dummy, EnterpriseBusinessObject.AutologState.NotLogged));
		}

		class AuditStmALogDeciderForTesting : AuditStmALogDecider
		{
			public override IDictionary<string, (bool, bool, bool)> AutoLoggedTablesDefaultConfigurationValues => autoLoggedTablesDefaultConfigurationValues;

			readonly Dictionary<string, (bool, bool, bool)> autoLoggedTablesDefaultConfigurationValues = new Dictionary<string, (bool, bool, bool)>();

			public void AddToAutoLoggedDefaultConfigurationValues(string tableName, bool isAdd, bool isEdt, bool isDel)
			{
				autoLoggedTablesDefaultConfigurationValues.Add(tableName, (isAdd, isEdt, isDel));
			}

			public void ClearAutoLoggedDefaultConfigurationValues() => autoLoggedTablesDefaultConfigurationValues.Clear();
		}

		public void TestAuditStmALogDeciderReturnsCorrectDefaultValues()
		{
			AssertContainsExactElementsInAnyOrder((new AuditStmALogDecider()).AutoLoggedTablesDefaultConfigurationValues, AutoLoggedTablesDefaultConfigValues.Instance.GetDefaultConfigurationValues());
		}

		class DummyLoggedConfigurable : DummyLogged
		{
			public DummyLoggedConfigurable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public AutologState OverridenAutoLogState { get; set; }

			protected override AutologState AutoLoggingState => OverridenAutoLogState;
		}
	}
}

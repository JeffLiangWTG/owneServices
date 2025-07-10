using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	public class ProcessFieldChangeRuleConfigurationTest : TestCaseWithFactory
	{
		public void TestDependancyInjection()
		{
			var configurations = ObjectFactory.Get<Hashtable>("ProcessFieldChangeRuleConfigurations");

			AssertGreaterThan(configurations.Count, 0);

			var descriptors = Factory.GetCachedValue<WorkflowDescriptorList>();
			foreach (DictionaryEntry entry in configurations)
			{
				AssertEquals("The configuration key must match the workflow descriptor type", true, descriptors.ContainsCode(entry.Key));

				var handle = (ObjectHandle)entry.Value;
				var config = (IProcessFieldChangeRuleConfiguration)handle.GetObject();

				AssertNotNull("Schemas to include must not be null", config.Schemas);
				AssertNotNull("BlacklistedColumns must not be null", config.BlacklistedColumns);
			}
		}

		public void TestColumnsFromMultipleSchemas()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			var configManager = new ProcessFieldChangeRuleConfigurationManager();
			var fields = configManager.GetFields(WorkflowDescriptors.DummyWorkflowDescriptorCode);

			AssertEquals(true, fields.ContainsCode(JobShipmentSchema.JS_Phase.Name));
			AssertEquals(true, fields.ContainsCode(DummyBizoSchema.Z0_Date.Name));
			AssertEquals(true, fields.ContainsCode(StmALogSchema.SL_Reference.Name));
		}

		public void TestBlacklistedColumns()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			var configManager = new ProcessFieldChangeRuleConfigurationManager();
			var fields = configManager.GetFields(WorkflowDescriptors.DummyWorkflowDescriptorCode);

			AssertEquals(true, fields.ContainsCode(JobShipmentSchema.JS_Phase.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.PK.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemCreateTimeUtc.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemCreateUser.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemCreateBranch.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemCreateDepartment.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemLastEditTimeUtc.Name));
			AssertEquals(false, fields.ContainsCode(JobShipmentSchema.JS_SystemLastEditUser.Name));
			AssertEquals(false, fields.ContainsCode(DummyBizoSchema.Z0_Code.Name));
		}

		public void TestColumnDisplayNames()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			var configManager = new ProcessFieldChangeRuleConfigurationManager();

			var fieldAndTableName = configManager.GetFieldAndTableNames(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Byte.Name);
			var tableName = configManager.GetTables(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Byte.Name);
			var fieldName = configManager.GetFields(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Byte.Name);

			AssertEquals("Expected format: 'Tablename - Column Name', needs to be like this for the GUI", "DummyBizo - Z0_Byte", fieldAndTableName);
			AssertEquals("TableName", "DummyBizo", tableName);
			AssertEquals("ColumnName", "Z0_Byte", fieldName);

			fieldAndTableName = configManager.GetFieldAndTableNames(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode).GetDescriptionFromCode(JobShipmentSchema.JS_Phase.Name);
			tableName = configManager.GetTables(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode).GetDescriptionFromCode(JobShipmentSchema.JS_Phase.Name);
			fieldName = configManager.GetFields(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode).GetDescriptionFromCode(JobShipmentSchema.JS_Phase.Name);

			AssertEquals("Expected format: 'Tablename - Column Name', needs to be like this for the GUI", "JobShipment - Phase", fieldAndTableName);
			AssertEquals("TableName", "JobShipment", tableName);
			AssertEquals("ColumnName taken from resourse strings <Key>JobShipment|JS_Phase</Key>", "Phase", fieldName);
		}

		public void TestCustomColumnDisplayNames()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			var configManager = new ProcessFieldChangeRuleConfigurationManager();

			var fieldAndTableName = configManager.GetFieldAndTableNames(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Date.Name);
			var tableName = configManager.GetTables(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Date.Name);
			var fieldName = configManager.GetFields(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_Date.Name);

			AssertEquals("Expected format: 'Tablename - Column Name', needs to be like this for the GUI", "DummyBizo - My Date Column", fieldAndTableName);
			AssertEquals("TableName", "DummyBizo", tableName);
			AssertEquals("ColumnName", "My Date Column", fieldName);

			fieldAndTableName = configManager.GetFieldAndTableNames(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_AnotherDate.Name);
			tableName = configManager.GetTables(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_AnotherDate.Name);
			fieldName = configManager.GetFields(WorkflowDescriptors.DummyWorkflowDescriptorCode).GetDescriptionFromCode(DummyBizoSchema.Z0_AnotherDate.Name);
			AssertEquals("Expected format: 'Tablename - Column Name', needs to be like this for the GUI", "DummyBizo - Another - Date", fieldAndTableName);
			AssertEquals("TableName", "DummyBizo", tableName);
			AssertEquals("ColumnName, should still work with extra - character", "Another - Date", fieldName);
		}

		public void TestUseSchemaFromWorkflowDescriptorByDefault()
		{
			var configurations = ObjectFactory.Get<Hashtable>("ProcessFieldChangeRuleConfigurations");
			AssertEquals("Precondition: Must not have configuration for IProcessFieldChangeRuleConfiguration",
				false, configurations.ContainsKey(WorkflowDescriptors.AccComplianceReportCode));

			var configManager = new ProcessFieldChangeRuleConfigurationManager();
			var fields = configManager.GetFields(WorkflowDescriptors.AccComplianceReportCode);

			AssertEquals(true, fields.ContainsCode(AccComplianceReportSchema.ACR_Description.Name));
			AssertEquals(true, fields.ContainsCode(AccComplianceReportSchema.ACR_ReportType.Name));
		}
	}
}

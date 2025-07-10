using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleConfigurationManager : IProcessFieldChangeRuleConfigurationManager
	{
		ZString workflowType;
		CodeDescriptionPairList fieldNames;
		CodeDescriptionPairList tableNames;
		CodeDescriptionPairList fieldAndTableNames;
		readonly Hashtable configurations;
		readonly Dictionary<ZString, IProcessFieldChangeRuleConfiguration> configurationInstances;

		public ProcessFieldChangeRuleConfigurationManager()
		{
			configurations = ObjectFactory.Get<Hashtable>("ProcessFieldChangeRuleConfigurations");
			configurationInstances = new Dictionary<ZString, IProcessFieldChangeRuleConfiguration>();
		}

		public CodeDescriptionPairList GetFields(ZString workflowType)
		{
			UpdateCacheIfRequired(workflowType);
			return fieldNames;
		}

		public CodeDescriptionPairList GetTables(ZString workflowType)
		{
			UpdateCacheIfRequired(workflowType);
			return tableNames;
		}

		public CodeDescriptionPairList GetFieldAndTableNames(ZString workflowType)
		{
			UpdateCacheIfRequired(workflowType);
			return fieldAndTableNames;
		}

		void UpdateCacheIfRequired(ZString workflowType)
		{
			if (!workflowType.IsEmpty && workflowType.EqualsIgnoringCase(this.workflowType))
			{
				return;
			}

			fieldNames = new CodeDescriptionPairList();
			tableNames = new CodeDescriptionPairList();
			fieldAndTableNames = new CodeDescriptionPairList();

			if (!workflowType.IsEmpty)
			{
				var config = GetConfiguration(workflowType);
				foreach (var schema in config.Schemas)
				{
					foreach (var column in schema.All)
					{
						if (!IsExcludedColumn(config, column))
						{
							var fieldName = config.GetFieldColumnDescription(column);
							fieldNames.AddPair(column.Name, fieldName);
							tableNames.AddPair(column.Name, column.TableName);
							fieldAndTableNames.AddPair(column.Name, column.TableName + " - " + fieldName);
						}
					}
				}
			}
			this.workflowType = workflowType;
		}

		bool IsExcludedColumn(IProcessFieldChangeRuleConfiguration config, SchemaColumn column)
		{
			return config.BlacklistedColumns.Contains(column.Name)
				|| column.IsPKColumn
				|| column.Name.EndsWith("_IsValid", StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith("_AddInfo", StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemCreateTimeUtc, StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemCreateUser, StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemCreateBranch, StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemCreateDepartment, StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemLastEditTimeUtc, StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith(AuditDetailsColumns.SystemLastEditUser, StringComparison.OrdinalIgnoreCase);
		}

		IProcessFieldChangeRuleConfiguration GetConfiguration(ZString workflowType)
		{
#if DEBUG
			AddConfigurationForTest();
#endif

			var key = workflowType.ToString();
			if (configurations.ContainsKey(key))
			{
				if (configurationInstances.TryGetValue(key, out IProcessFieldChangeRuleConfiguration value))
				{
					return value;
				}
				else
				{
					var handle = (ObjectHandle)configurations[key];
					var config = (IProcessFieldChangeRuleConfiguration)handle.GetObject();
					configurationInstances[key] = config;
					return config;
				}
			}

			return new DefaultProcessFieldChangeRuleConfiguration(key);
		}

#if DEBUG
		void AddConfigurationForTest()
		{
			var instance = ConfigurationForTest.Value.Item2;
			if (instance != null)
			{
				var key = ConfigurationForTest.Value.Item1;
				configurations[key] = null;
				configurationInstances[key] = instance;
			}
		}

		[ThreadSafe]
		public static Overridable<(ZString, IProcessFieldChangeRuleConfiguration)> ConfigurationForTest { get; } = new Overridable<(ZString, IProcessFieldChangeRuleConfiguration)>((ZString.Empty, null));
#endif
	}
}

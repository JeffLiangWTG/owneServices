using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	public class AuditStmALogDecider : IAuditStmALogDecider
	{
		public AuditStmALogDecider()
		{
		}

		public bool TryGetAutoLogConfigurations(IBusiness bizo, out (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL) autoLogConfig)
		{
			var registryConfig = ObjectFactory.Get<ISystemDataRegistry>().AuditLogsEnabledFor(bizo.TableName);
			if (registryConfig.HasValue)
			{
				autoLogConfig = registryConfig.Value;
				return true;
			}
			else
			{
				autoLogConfig = default;
				return false;
			}
		}

		public bool AuditLogConfigNonPersistedForEventCode(IBusiness bO, string eventCode)
		{
			if (TryGetAutoLogConfigurations(bO, out var auditLogConfig))
			{
				if (eventCode == AutoEvents.AddedARecordToTheSystemCode)
				{
					return !auditLogConfig.IsEnabledForADD;
				}
				else if (eventCode == AutoEvents.EditedARecordCode)
				{
					return !auditLogConfig.IsEnabledForEDT;
				}
				else if (eventCode == AutoEvents.DeletedARecordInTheSystemCode)
				{
					return !auditLogConfig.IsEnabledForDEL;
				}
			}
			else if (bO is IAutoAdminLogTarget logTarget && (!logTarget.IsAutoAdminBusinessObjectLoggerEnabled || logTarget.IsAutoLogged))
			{
				return false;
			}
			return true;
		}

		public bool IsAutoAdminBusinessObjectLoggerEnabled(BusinessObject bO, EnterpriseBusinessObject.AutologState logState)
		{
			return TryGetAutoLogConfigurations(bO, out _) || logState != EnterpriseBusinessObject.AutologState.NotLogged;
		}

		public virtual IDictionary<string, (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)> AutoLoggedTablesDefaultConfigurationValues
		{
			get
			{
				return AutoLoggedTablesDefaultConfigValues.Instance.GetDefaultConfigurationValues();
			}
		}
	}
}


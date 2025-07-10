using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class SingleActionPerTransactionExtensions
	{
		public static void RunSingleActionPerTransaction(this BusinessObject bizo, string actionIdUniqueOnFactoryLevel, Action actionToRun)
		{
			Argument.NotNull(bizo, "bizo");
			Argument.NotNull(actionIdUniqueOnFactoryLevel, "actionIdUniqueOnFactoryLevel");
			Argument.NotNull(actionToRun, "action");

			if (!SingleActionPerTransactionProvider.IsEventAdded(actionIdUniqueOnFactoryLevel, bizo))
			{
				actionToRun();
				SingleActionPerTransactionProvider.AddEvent(actionIdUniqueOnFactoryLevel, bizo);
			}
		}

		public static void AddNewSinglePerTransaction(this Logs logs, Event eventType, string reference = null)
		{
			Argument.NotNull(logs, "logs");
			Argument.NotNull(eventType, "eventType");

			var logsParent = logs.Parent;
			var uniqueActionID = eventType.Code;
			if (!SingleActionPerTransactionProvider.IsEventAdded(uniqueActionID, logsParent))
			{
				if (string.IsNullOrEmpty(reference))
				{
					logs.AddNew(eventType);
				}
				else
				{
					logs.AddNew(eventType, new ZString(reference).Left(StmALog.Schema.SL_ReferenceMaxLength));
				}
				SingleActionPerTransactionProvider.AddEvent(uniqueActionID, logsParent);
			}
		}

		class SingleActionPerTransactionProvider : IService
		{
			public SingleActionPerTransactionProvider(BusinessObjectFactory factory)
			{
				factory.Saved += factory_Saved;
			}

			public static bool IsEventAdded(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent)
			{
				bool result = false;
				var service = GetInstance(logsParent.Factory);
				if (service != null)
				{
					result = service.IsEventInDictionary(actionIdUniqueOnFactoryLevel, logsParent);
				}

				return result;
			}

			public static void AddEvent(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent)
			{
				var service = CreateOrGetInstance(logsParent.Factory);
				service.AddEventToDictionary(actionIdUniqueOnFactoryLevel, logsParent);
			}

			static SingleActionPerTransactionProvider GetInstance(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<SingleActionPerTransactionProvider>();
			}

			static SingleActionPerTransactionProvider CreateOrGetInstance(BusinessObjectFactory factory)
			{
				var service = GetInstance(factory);
				if (service == null)
				{
					service = new SingleActionPerTransactionProvider(factory);
					factory.ServiceContainer.AddService(service);
				}

				return service;
			}

			static void RemoveInstance(BusinessObjectFactory factory)
			{
				factory.ServiceContainer.RemoveService<SingleActionPerTransactionProvider>();
			}

			bool IsEventInDictionary(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent)
			{
				bool result = false;
				HashSet<ZGuid> bizoPKs;
				if (bizoPKsByActionID.TryGetValue(actionIdUniqueOnFactoryLevel, out bizoPKs))
				{
					result = bizoPKs.Contains(logsParent.PK);
				}

				return result;
			}

			void AddEventToDictionary(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent)
			{
				HashSet<ZGuid> bizoPKs;
				if (!bizoPKsByActionID.TryGetValue(actionIdUniqueOnFactoryLevel, out bizoPKs))
				{
					bizoPKs = new HashSet<ZGuid>();
					bizoPKsByActionID.Add(actionIdUniqueOnFactoryLevel, bizoPKs);
				}
				if (!bizoPKs.Add(logsParent.PK))
				{
					throw new InvalidOperationException(string.Format("Action with ID '{0}' for business object {1} with PK {2}, is already added", actionIdUniqueOnFactoryLevel, logsParent.HumanReadableName, logsParent.PK));
				}
			}

			void factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					RemoveInstance(factory);
					factory.Saved -= factory_Saved;
				}
			}

			readonly Dictionary<string, HashSet<ZGuid>> bizoPKsByActionID = new Dictionary<string, HashSet<ZGuid>>();
		}
	}
}

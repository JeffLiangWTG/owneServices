using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ISingleActionPerTransactionOnDifferentLevels
	{
		void RunSingleActionPerTransactionOnDifferentLevels(BusinessObject bizo, string actionIdUniqueOnFactoryLevel, Action actionToRun, int invoiceProcessingLevel);
	}

	public class SingleActionPerTransactionOnDifferentLevels : ISingleActionPerTransactionOnDifferentLevels
	{
		void ISingleActionPerTransactionOnDifferentLevels.RunSingleActionPerTransactionOnDifferentLevels(BusinessObject bizo, string actionIdUniqueOnFactoryLevel, Action actionToRun, int invoiceProcessingLevel)
		{
			Argument.NotNull(bizo, nameof(bizo));
			Argument.NotNullOrEmpty(actionIdUniqueOnFactoryLevel, nameof(actionIdUniqueOnFactoryLevel));
			Argument.NotNull(actionToRun, nameof(actionToRun));

			int prevLevel = SingleActionPerTransactionOnDifferentLevelsProvider.GetOrAddInvoiceProcessingLevel(actionIdUniqueOnFactoryLevel, bizo, invoiceProcessingLevel);
			if (prevLevel == invoiceProcessingLevel)
			{
				actionToRun();
			}
		}

		class SingleActionPerTransactionOnDifferentLevelsProvider : IService
		{
			SingleActionPerTransactionOnDifferentLevelsProvider(BusinessObjectFactory factory)
			{
				factory.Saved += factory_Saved;
			}

			public static int GetOrAddInvoiceProcessingLevel(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent, int invoiceProcessingLevel)
			{
				var service = GetInstance(logsParent.Factory);
				if (service == null)
				{
					service = new SingleActionPerTransactionOnDifferentLevelsProvider(logsParent.Factory);
					logsParent.Factory.ServiceContainer.AddService(service);
				}
				return service.GetOrAddInvoiceProcessingLevelToDictionary(actionIdUniqueOnFactoryLevel, logsParent, invoiceProcessingLevel);
			}

			static SingleActionPerTransactionOnDifferentLevelsProvider GetInstance(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<SingleActionPerTransactionOnDifferentLevelsProvider>();
			}

			static void RemoveInstance(BusinessObjectFactory factory)
			{
				factory.ServiceContainer.RemoveService<SingleActionPerTransactionOnDifferentLevelsProvider>();
			}

			int GetOrAddInvoiceProcessingLevelToDictionary(string actionIdUniqueOnFactoryLevel, BusinessObject logsParent, int invoiceProcessingLevel)
			{
				var bizoPK = logsParent.PK;
				Dictionary<ZGuid, int> dictionaryBizoPKsByLevelResult;
				int previousLevel;

				if (!actionIDDictionaryWithBizoPKsByLevel.TryGetValue(actionIdUniqueOnFactoryLevel, out dictionaryBizoPKsByLevelResult))// || !actionIDDictionaryWithBizoPKsByLevel[actionIdUniqueOnFactoryLevel].ContainsKey(logsParent.PK))
				{
					dictionaryBizoPKsByLevelResult = new Dictionary<ZGuid, int>();
					actionIDDictionaryWithBizoPKsByLevel.Add(actionIdUniqueOnFactoryLevel, dictionaryBizoPKsByLevelResult);
				}
				if (!dictionaryBizoPKsByLevelResult.TryGetValue(bizoPK, out previousLevel))
				{
					previousLevel = invoiceProcessingLevel;
					dictionaryBizoPKsByLevelResult.Add(bizoPK, previousLevel);
				}

				return previousLevel;
			}

			void factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					RemoveInstance(factory);
					factory.Saved -= factory_Saved;
				}
			}

			readonly Dictionary<string, Dictionary<ZGuid, int>> actionIDDictionaryWithBizoPKsByLevel = new Dictionary<string, Dictionary<ZGuid, int>>();
		}
	}
}

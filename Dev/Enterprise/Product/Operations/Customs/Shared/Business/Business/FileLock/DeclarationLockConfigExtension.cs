using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class DeclarationLockConfigExtension
	{
		public static DeclarationEventLockInfo GetEventLockInfoFromConfigWhichMatchesLog(this DeclarationLockConfig config, IEnumerable<IQueuedLog> logs, string entryType = "")
		{
			DeclarationEventLockInfo matchingEventLockInfo = null;
			if (config != null)
			{
				var eventInfos = config.EventInfos.Cast<DeclarationEventLockInfo>().ToArray();
				foreach (var log in logs)
				{
					matchingEventLockInfo = eventInfos.FirstOrDefault(x => x.MatchesLog(log.SJ_SE_NKEvent, log.SJ_ParentTableCode, log.SJ_Reference, entryType));
					if (matchingEventLockInfo != null)
					{
						break;
					}
				}
			}
			return matchingEventLockInfo;
		}

		public static bool MatchesLog(this DeclarationEventLockInfo eventLockInfo, ZString eventType, ZString parentTableReference, ZString reference, string entryType = "")
			=> eventLockInfo.EventType == eventType
				&& IsMatchedEntryType(eventLockInfo.EntryType, entryType)
				&& IsMatchedEventSource(eventLockInfo.EventSource, parentTableReference)
				&& IsMatchedEventReference(eventLockInfo.EventReference, reference);

		static bool IsMatchedEntryType(string sourceEntryType, string targetEntryType)
		{
			return string.IsNullOrWhiteSpace(sourceEntryType)
				|| sourceEntryType.Equals(Core.Constants.Customs.EntryHeaderTypes.Codes.All, StringComparison.InvariantCultureIgnoreCase)
				|| sourceEntryType.Equals(targetEntryType, StringComparison.InvariantCultureIgnoreCase);
		}

		static bool IsMatchedEventSource(ZString eventSource, ZString tableReference)
		{
			switch (eventSource.ToUpperInvariant())
			{
				case Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration:
					return tableReference == JobDeclarationSchema.Constants.TableName || tableReference == JobDeclarationSchema.Constants.Prefix;

				case Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader:
					return tableReference == CusEntryHeaderSchema.Constants.TableName || tableReference == CusEntryHeaderSchema.Constants.Prefix;

				case Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader:
					return tableReference == CusInBondHeaderSchema.Constants.TableName || tableReference == CusInBondHeaderSchema.Constants.Prefix || tableReference == CusInBondMoveHeaderSchema.Constants.TableName || tableReference == CusInBondMoveHeaderSchema.Constants.Prefix;
			}

			return false;
		}

		static bool IsMatchedEventReference(ZString sourceReference, ZString targetReference)
		{
			if (sourceReference == DeclarationEventLockInfo.DefaultMatchCharacter || sourceReference == targetReference)
			{
				return true;
			}

			var targetReferenceForMatch = BaseStmALog.GetReferenceForBinding(targetReference);

			if (sourceReference == targetReferenceForMatch)
			{
				return true;
			}

			if (!string.IsNullOrWhiteSpace(sourceReference) && !string.IsNullOrWhiteSpace(targetReferenceForMatch))
			{
				var regex = TriggerConditionRegexProvider.GetEventReferenceWithWildcardsRegex(sourceReference);
				return regex.IsMatch(targetReferenceForMatch);
			}

			return false;
		}

		public static DeclarationLockConfig Find(this DeclarationLockConfigCollection configs, ZString declarationType)
		{
			return configs != null && !string.IsNullOrWhiteSpace(declarationType)
				? configs.Cast<DeclarationLockConfig>().FirstOrDefault(c => c.DeclarationType.EqualsIgnoringCase(declarationType))
				: null;
		}

		public static DeclarationLockConfig Find(this DeclarationLockConfigCollection configs, ZString declarationType, Logs logs)
		{
			if (configs != null && logs != null && !string.IsNullOrWhiteSpace(declarationType))
			{
				foreach (var config in configs.Cast<DeclarationLockConfig>().Where(c => c.DeclarationType.EqualsIgnoringCase(declarationType)))
				{
					var eventInfos = config.EventInfos.Cast<DeclarationEventLockInfo>().ToArray();
					if (eventInfos.Length == 0
						|| logs.Find(c => !c.IsCancelled &&
							eventInfos.Any(x => x.MatchesLog(c.SL_SE_NKEvent, c.SL_Table, c.SL_Reference))).Any())
					{
						return config;
					}
				}
			}

			return null;
		}
	}
}

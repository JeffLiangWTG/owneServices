using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	public class CriticalValidationInfoCollectorService : IService
	{
		CriticalValidationInfoCollectorService()
		{
		}

		#region Fields

		readonly Dictionary<CriticalValidationInfoCollectorServiceKeyType, (int totalSize, CollectionFrequency lastCollectionFrequencyUsed)> collectorKeyUsageInfo = new Dictionary<CriticalValidationInfoCollectorServiceKeyType, (int totalSize, CollectionFrequency lastCollectionFrequencyUsed)>();
		int totalCollectorSize;
		readonly HashSet<CollectionFrequency> totalCollectorSizeReachedInfo = new HashSet<CollectionFrequency>();
		readonly HashSet<CriticalValidationInfoCollectorServiceKeyType> keysWhereCollectorSizeReached = new HashSet<CriticalValidationInfoCollectorServiceKeyType>();
		const int maximumCollectorSizeForOneKey = 1024 * 1024; // 1mb
		const int maximumCollectorSize = 10 * maximumCollectorSizeForOneKey; // 10mb
		const int maximumCollectorSizeForOneKeyOnSecondReportCollection = maximumCollectorSizeForOneKey * 10; //more memory for one key
		const int maximumCollectorSizeOnSecondReportCollection = maximumCollectorSizeForOneKeyOnSecondReportCollection * 5; //less keys collected
		readonly Dictionary<ZGuid, Dictionary<CriticalValidationInfoCollectorServiceKeyType, string>> collectorInfoDic = new Dictionary<ZGuid, Dictionary<CriticalValidationInfoCollectorServiceKeyType, string>>();
		readonly Dictionary<ZGuid, Dictionary<CriticalValidationInfoCollectorServiceKeyType, string>> collectorInfoDic_NeverCleared = new Dictionary<ZGuid, Dictionary<CriticalValidationInfoCollectorServiceKeyType, string>>();
		const string maxBufferSizeMarker = "<FULL_BUFFER>";

		#endregion

		#region Methods

		public void AddInfoWhenAllowed(ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, Func<string> generateDebugInfoWhenRequired, CollectionFrequency collectionFrequency = CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession, bool useNeverClearedInfo = false)
		{
			TryAddInfo(pk, key, generateDebugInfoWhenRequired, collectionFrequency, (existingInfo, debugInfoStringToAdd) => existingInfo + System.Environment.NewLine + debugInfoStringToAdd, useNeverClearedInfo);
		}

		public void AddLastInfoWhenAllowed(ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, Func<string> generateDebugInfoWhenRequired, CollectionFrequency collectionFrequency = CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
		{
			TryAddInfo(pk, key, generateDebugInfoWhenRequired, collectionFrequency, (existingInfo, debugInfoStringToAdd) => debugInfoStringToAdd);
		}

		public void AddInfoOnceWhenAllowed(ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, Func<string> generateDebugInfoWhenRequired, CollectionFrequency collectionFrequency = CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
		{
			if (!collectorInfoDic.TryGetValue(pk, out var collectorThisPK) || !collectorThisPK.ContainsKey(key))
			{
				TryAddInfo(pk, key, generateDebugInfoWhenRequired, collectionFrequency, null);
			}
		}

		public string GetInfo(ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, bool useNeverClearedInfo = false)
		{
			var seqNumberMessage = (NoResString)"It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			string result = GetInfoPreffix(key);

			KeysRequestedInThisSession.Keys.Add(key);

			var dictionary = useNeverClearedInfo ? collectorInfoDic_NeverCleared : collectorInfoDic;

			if (dictionary.TryGetValue(pk, out var collectInfoForOnePK))
			{
				if (collectInfoForOnePK != null && collectInfoForOnePK.TryGetValue(key, out var info))
				{
					if (info == maxBufferSizeMarker)
					{
						result += (NoResString)" Maximum buffer size is reached.";
					}
					else
					{
						result += System.Environment.NewLine + info;
					}
				}
				else
				{
					result += (NoResString)" There is no data collected for this key. " + seqNumberMessage;
					AddNoDataWarnings();
				}
			}
			else
			{
				result += (NoResString)" There is no data collected for this PK. " + seqNumberMessage;
				AddNoDataWarnings();
			}

			return result;

			void AddNoDataWarnings()
			{
				if (keysWhereCollectorSizeReached.Contains(key))
				{
					result += System.Environment.NewLine + (NoResString)"Attention. Data may not be collected because maximum buffer size was reached for this key.";
				}
				if (totalCollectorSizeReachedInfo.Any())
				{
					result += System.Environment.NewLine + $"Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: {new ZStringBuilder(totalCollectorSizeReachedInfo.Select(x => x.ToString())).ToStringWithDelimiterBetweenAppends(", ")}.";
				}
			}
		}

		internal static string GetInfoPreffix(CriticalValidationInfoCollectorServiceKeyType key) => System.Environment.NewLine + key.ToString() + ":";

		public void ClearServiceCache()
		{
			totalCollectorSize = 0;
			totalCollectorSizeReachedInfo.Clear();
			collectorKeyUsageInfo.Clear();
			collectorInfoDic.Clear();
		}

		public static CriticalValidationInfoCollectorService GetService(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<CriticalValidationInfoCollectorService>();
		}

		public static CriticalValidationInfoCollectorService GetOrCreateService(BusinessObjectFactory factory)
		{
			var result = GetService(factory);
			if (result == null)
			{
				result = new CriticalValidationInfoCollectorService();
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		#endregion

		#region Implementation

		void TryAddInfo(ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, Func<string> generateDebugInfoWhenRequired, CollectionFrequency collectionFrequency, Func<string, string, string> getInfoToAddByExistingInfo, bool useNeverClearedInfo = false)
		{
			bool shouldCollectOnSecondReport = collectionFrequency == CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession ||
											  (collectionFrequency == CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport && !Globals.IsTest);

			if (shouldCollectOnSecondReport && !KeysRequestedInThisSession.Keys.Contains(key))
			{
				return;
			}

			int collectorSizeForKey = 0;
			if (collectorKeyUsageInfo.TryGetValue(key, out var keyUsageInfo))
			{
				collectorSizeForKey = keyUsageInfo.totalSize;

				if (keyUsageInfo.lastCollectionFrequencyUsed != collectionFrequency)
				{
					ErrorReporter.ReportOnce(Invariant($"CriticalValidationInfoCollectorService key '{key}' must not be used with different collectionFrequency."), Invariant($"Key '{key}' was used with keys: {keyUsageInfo.lastCollectionFrequencyUsed} and {collectionFrequency}."));

					return;
				}
			}

			var maxCollectorSizeByCollectionFrequency = shouldCollectOnSecondReport ? maximumCollectorSizeOnSecondReportCollection : maximumCollectorSize;
			var maxCollectorSizeForOneKeyByCollectionFrequency = shouldCollectOnSecondReport ? maximumCollectorSizeForOneKeyOnSecondReportCollection : maximumCollectorSizeForOneKey;

			var dictionary = useNeverClearedInfo ? collectorInfoDic_NeverCleared : collectorInfoDic;

			if (CheckIsMaxBufferSizeReachedAndSetMarker(0))
			{
				return;
			}

			string debugInfoString;
			if (generateDebugInfoWhenRequired != null && !string.IsNullOrEmpty(debugInfoString = generateDebugInfoWhenRequired()))
			{
				var strBytesLength = System.Text.Encoding.Unicode.GetBytes(debugInfoString).Length;

				if (!CheckIsMaxBufferSizeReachedAndSetMarker(strBytesLength))
				{
					var collectorThisPK = dictionary.GetOrAdd(pk, () => new Dictionary<CriticalValidationInfoCollectorServiceKeyType, string>());
					if (!collectorThisPK.ContainsKey(key))
					{
						collectorThisPK.Add(key, debugInfoString);
					}
					else if (getInfoToAddByExistingInfo != null)
					{
						var existingInfo = collectorThisPK[key];
						collectorThisPK[key] = getInfoToAddByExistingInfo(existingInfo, debugInfoString);
					}
				}

				totalCollectorSize += strBytesLength;
				if (totalCollectorSize <= maxCollectorSizeByCollectionFrequency)
				{
					collectorKeyUsageInfo[key] = (collectorSizeForKey + strBytesLength, collectionFrequency);
				}
			}

			bool CheckIsMaxBufferSizeReachedAndSetMarker(int strBytesLength)
			{
				var isMaxSizeReached = false;
				if (totalCollectorSize + strBytesLength > maxCollectorSizeByCollectionFrequency)
				{
					totalCollectorSizeReachedInfo.Add(collectionFrequency);
					isMaxSizeReached = true;
				}
				else if (collectorSizeForKey + strBytesLength > maxCollectorSizeForOneKeyByCollectionFrequency)
				{
					keysWhereCollectorSizeReached.Add(key);
					isMaxSizeReached = true;
				}

				if (isMaxSizeReached)
				{
					var collectorThisPK = dictionary.GetValueSafe(pk);
					if (collectorThisPK != null && collectorThisPK.ContainsKey(key))
					{
						collectorThisPK[key] = maxBufferSizeMarker;
					}
				}

				return isMaxSizeReached;
			}
		}

#if DEBUG
		public static void ClearKeysRequestedInThisSession_ForTestOnly() => KeysRequestedInThisSession.ClearKeys_ForTestOnly();
#endif

		public enum CollectionFrequency
		{
			CollectOnlyAfterErrorReportForCurrentUserSession,
			CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE,
			CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport
		}

		static class KeysRequestedInThisSession
		{
			public static HashSet<CriticalValidationInfoCollectorServiceKeyType> Keys => keys.Value ?? (keys.Value = new HashSet<CriticalValidationInfoCollectorServiceKeyType>());

#if DEBUG
			public static void ClearKeys_ForTestOnly() => keys.Value = null;
#endif
			static readonly ThreadLocalOverridable<HashSet<CriticalValidationInfoCollectorServiceKeyType>> keys = new ThreadLocalOverridable<HashSet<CriticalValidationInfoCollectorServiceKeyType>>();
		}

		#endregion
	}

	public static class CriticalValidationInfoCollectorServiceExtensions
	{
		public static string GetInfoSafe(this CriticalValidationInfoCollectorService service, ZGuid pk, CriticalValidationInfoCollectorServiceKeyType key, bool useNeverClearedInfo = false)
		{
			if (service == null)
			{
				return CriticalValidationInfoCollectorService.GetInfoPreffix(key) + (NoResString)" There was no attempt to collect any data.";
			}

			return service.GetInfo(pk, key, useNeverClearedInfo);
		}
	}
}

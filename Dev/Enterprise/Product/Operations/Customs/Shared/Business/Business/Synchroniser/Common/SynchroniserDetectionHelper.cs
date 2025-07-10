#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public static class SynchroniserDetectionHelper
	{
		public static void SetupDetection(ZPropertyInfo info)
		{
			var wrapped = info as ZWrappedPropertyInfo;
			var keyInfo = wrapped == null ? info : wrapped.InnerInfo;
			if (detectionInfos != null && !detectionInfos.Contains(keyInfo))
			{
				detectionInfos.Add(keyInfo);
				keyInfo.ValueChanged += detectionInfo_ValueChanged;
			}
		}

		public static void SetupDetection(BusinessObjectCollectionSynchroniser synchroniser)
		{
			if (businessObjectCollectionSynchroniserDetails != null && !businessObjectCollectionSynchroniserDetails.Contains(synchroniser))
			{
				businessObjectCollectionSynchroniserDetails.Add(synchroniser);
			}
		}

		public static void MarkHasData(BusinessObjectCollectionSynchroniser synchroniser)
		{
			if (businessObjectCollectionSynchroniserDetails != null)
			{
				businessObjectCollectionSynchroniserDetails.Remove(synchroniser);
			}
		}

		static void detectionInfo_ValueChanged(object sender, EventArgs e)
		{
			var ev = e as ValueChangedEventArgs;
			if (ev != null)
			{
				var info = ev.Info;
				if (info != null && detectionInfos != null)
				{
					detectionInfos.Remove(info);
					info.ValueChanged -= detectionInfo_ValueChanged;
				}
			}
		}

		public static IDisposable SetupEnableDetectionForTesting()
		{
			if (detectionInfos == null)
			{
				detectionInfos = new List<ZPropertyInfo>();
			}
			if (businessObjectCollectionSynchroniserDetails == null)
			{
				businessObjectCollectionSynchroniserDetails = new List<BusinessObjectCollectionSynchroniser>();
			}
			return new DisposableAction(() =>
			{
				if (detectionInfos != null)
				{
					foreach (var info in detectionInfos)
					{
						info.ValueChanged -= detectionInfo_ValueChanged;
					}
					detectionInfos.Clear();
					detectionInfos = null;
				}
				if (businessObjectCollectionSynchroniserDetails != null)
				{
					businessObjectCollectionSynchroniserDetails.Clear();
					businessObjectCollectionSynchroniserDetails = null;
				}
			});
		}

		[ThreadStatic]
		static List<ZPropertyInfo> detectionInfos;

		[ThreadStatic]
		static List<BusinessObjectCollectionSynchroniser> businessObjectCollectionSynchroniserDetails;

		public static ZString GetInfosNotChanged(Func<ZPropertyInfo, bool> shouldIgnoreInfo)
		{
			var result = new ZStringBuilder();
			if (detectionInfos != null)
			{
				foreach (var info in detectionInfos)
				{
					if (!shouldIgnoreInfo(info))
					{
						var bizObj = info.BizObj;
						result.Append(string.Format("{0}.{2} ({1})", bizObj.GetType().FullName, GetBizObjDetail(bizObj), info.Name));
					}
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		static string GetBizObjDetail(BusinessObject bizObj)
		{
			var docAddress = bizObj as JobDocAddress;
			if (docAddress != null)
			{
				var parent = docAddress.Parent;
				return string.Format("{0} child of {1}", docAddress.AddressCaption, parent == null ? "NULL" : parent.HumanReadableName.ToString());
			}
			else
			{
				return bizObj.HumanReadableName;
			}
		}

		public static ZString GetSynchroniserWithData()
		{
			var result = new ZStringBuilder();
			if (businessObjectCollectionSynchroniserDetails != null)
			{
				foreach (var synchroniser in businessObjectCollectionSynchroniserDetails)
				{
					result.Append(string.Format("{0} should have at least one element to synchronise", synchroniser.GetType().FullName));
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}
	}
}
#endif

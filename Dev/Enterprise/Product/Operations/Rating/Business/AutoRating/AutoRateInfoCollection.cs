using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Rating.Business
{
	public class AutoRateInfoCollection : List<AutoRateInfo>
	{
		public BusinessObjectFactory Factory;
		public AutoRateInfoCollection(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region Cancel AutoRating

		public AutoRatingCancellation Cancellation { get; private set; }

		public bool UserCancelledAutoRating
		{
			get { return Cancellation != null; }
		}

		public void CancelAutoRating(AutoRatingCancellation reason)
		{
			Cancellation = reason;
		}

		#endregion

		#region Sum Up Same Charges

		public void SumUpSameCharges()
		{
			var rateInfosToKeep = new List<AutoRateInfo>(Count);
			var ratesToKeepCurrentHashGroup = new List<AutoRateInfo>();
			var autoRateInfoComparer = new AutoRateInfoComparer();

			var hasAtMostOneItem = !this.Skip(1).Any();
			if (hasAtMostOneItem)
			{
				return;
			}

			/** 
			 * Group them according to the hash so we dont have to compare
			 * amongst obviously different ones.
			 *
			 * Then within each group, we sort and compare.
			 */
			foreach (var autoRateInfoHashGroup in this.GroupBy(x => x.HashCodeForMerging()))
			{
				ratesToKeepCurrentHashGroup.Clear();

				foreach (var currentRateInfo in autoRateInfoHashGroup.OrderBy(x => x, autoRateInfoComparer))
				{
					if (currentRateInfo.ChargeCode != null)
					{
						var existingRateIndex = FindExistingInfoIndex(ratesToKeepCurrentHashGroup, currentRateInfo);

						if (existingRateIndex >= 0)
						{
							var existingRateInfo = ratesToKeepCurrentHashGroup[existingRateIndex];
							AppendToExisting(currentRateInfo, existingRateInfo);

							// Skip the currentRateInfo because it has been absorbed
							// into the existingRateinfo.
							continue;
						}
					}
					ratesToKeepCurrentHashGroup.Add(currentRateInfo);
				}
				rateInfosToKeep.AddRange(ratesToKeepCurrentHashGroup);
			}

			Clear();
			AddRange(rateInfosToKeep);
			Sort(autoRateInfoComparer);
		}

		void AppendToExisting(AutoRateInfo sourceRateInfo, AutoRateInfo destinationRateInfo)
		{
			destinationRateInfo.AddMergedInfo(sourceRateInfo);
			destinationRateInfo.AddAttributes(sourceRateInfo);
			destinationRateInfo.AddAmount(sourceRateInfo);

			if (!sourceRateInfo.CalculationLogs.IsEmpty)
			{
				destinationRateInfo.CalculationLogs.Logs.AddRange(sourceRateInfo.CalculationLogs.Logs);
			}

			destinationRateInfo.AppendUniqueInvoiceLineDescriptions(sourceRateInfo);
		}

#if DEBUG
		internal int findExistingInfoIndexHitCount;
#endif
		int FindExistingInfoIndex(List<AutoRateInfo> existingCharges, AutoRateInfo info)
		{
#if DEBUG
			findExistingInfoIndexHitCount++;
#endif
			for (var i = 0; i < existingCharges.Count; i++)
			{
				if (existingCharges[i].CanBeMergedWith(info))
				{
					return i;
				}
#if DEBUG
				findExistingInfoIndexHitCount++;
#endif
			}
			return -1;
		}

		#endregion

		#region MethodFromBusinessObjectCollection

		public void CheckAndAddRange(IEnumerable<AutoRateInfo> autoRateInfos)
		{
			foreach (var info in autoRateInfos)
			{
				if (!info.ChargeCode.AC_GC.IsEmpty && info.ChargeCode.AC_GC != Env.CurrentCompanyPK)
				{
					ErrorReporter.ReportOnce("AutoRateInfoWithWrongCompany", "AutoRating should not be mapping other Company's Charge Codes which is causing Issue 00850403. Please report to the Rating Team.");
				}

				Add(info);
			}
		}

		public void RemoveAll()
		{
			if (Count > 0)
			{
				for (int i = this.Count - 1; i >= 0; i--)
				{
					Remove(this[i]);
				}
			}
		}

		public void RemoveAndDeleteAll()
		{
			RemoveAll();
		}

		#endregion
	}
}


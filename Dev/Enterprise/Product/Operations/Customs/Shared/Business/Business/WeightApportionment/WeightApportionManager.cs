using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class WeightApportionManager
	{
		public void ApportionAll(IWeightHolder weightHolder, IWeightApportionee uncommittedApportionee)
		{
			if (IsWeightApportionmentDeffered)
			{
				DeferredApportionments.Enqueue((weightHolder, uncommittedApportionee));
			}
			else if (!isApportionmentInProgress)
			{
				try
				{
					isApportionmentInProgress = true;
					IWeightApportionee[] allApportionees = GetAllApportionees(weightHolder, uncommittedApportionee);

					ApportionAllCore(weightHolder, allApportionees);
					IWeightHolder[] childWeightHolders = weightHolder.WeightHolders;
					if (childWeightHolders != null)
					{
						foreach (var childWeightHolder in childWeightHolders)
						{
							ApportionAllCore(childWeightHolder, childWeightHolder.AllApportionees);
						}
					}
				}
				finally
				{
					isApportionmentInProgress = false;
				}
			}
		}

		IWeightApportionee[] GetAllApportionees(IWeightHolder weightHolder, IWeightApportionee uncommittedApportionee)
		{
			var result = new List<IWeightApportionee>(weightHolder.AllApportionees);
			if (uncommittedApportionee != null && !result.Contains(uncommittedApportionee))
			{
				result.Add(uncommittedApportionee);
			}
			return result.ToArray();
		}

		void ApportionAllCore(IWeightHolder weightHolder, IWeightApportionee[] allApportionees)
		{
			var totalWeight = weightHolder.TotalWeight;
			var totalNetWeight = weightHolder.TotalNetWeight;
			var oldTotalAllApportioneesWeight = allApportionees.Sum(x => Core.Constants.Weight.ConvertSafe(x.Weight, x.WeightUQ, totalWeight.Unit));
			var oldTotalAllApportioneesNetWeight = allApportionees.Sum(x => Core.Constants.Weight.ConvertSafe(x.NetWeight, x.NetWeightUQ, totalNetWeight.Unit));

			if (!totalWeight.Amount.IsEmpty && !totalWeight.Unit.IsEmpty)
			{
				if (allApportionees.Length > 1)
				{
					var totalAmount = GetTotalAmount(allApportionees);
					foreach (var apportionee in allApportionees)
					{
						var weight = GetApportionWeightByAmount(totalWeight, totalAmount, apportionee.Amount, apportionee.WeightUQ.IsEmpty ? totalWeight.Unit : apportionee.WeightUQ);
						apportionee.WeightUQ = weight.Unit;
						apportionee.Weight = SetMinimumReapportionedWeight(apportionee, weight);
					}
				}
				else if (allApportionees.Length == 1)
				{
					var apportionee = allApportionees[0];
					apportionee.Weight = totalWeight.Amount;
					apportionee.WeightUQ = totalWeight.Unit;
				}
				ApportionNetWeightViaScaling(allApportionees, totalWeight, totalNetWeight);
			}
			else
			{
				ApportionNetWeightByAmount(allApportionees, totalNetWeight);
			}

			var allApportioneesCount = allApportionees.Length;
			ReconcileWeightIfRequired(allApportioneesCount > 1, allApportionees, totalWeight, oldTotalAllApportioneesWeight, x => Core.Constants.Weight.ConvertSafe(x.Weight, x.WeightUQ, totalWeight.Unit), x => x.Amount, (apportionee, balance) => apportionee.Weight += Core.Constants.Weight.ConvertSafe(balance, totalWeight.Unit, apportionee.WeightUQ));
			ReconcileWeightIfRequired(allApportioneesCount > 1 && allApportionees.All(c => c.NeedToApportionNetWeight), allApportionees, totalNetWeight, oldTotalAllApportioneesNetWeight, x => Core.Constants.Weight.ConvertSafe(x.NetWeight, x.NetWeightUQ, totalNetWeight.Unit), x => Core.Constants.Weight.ConvertSafe(x.NetWeight, x.NetWeightUQ, totalNetWeight.Unit), (apportionee, balance) => apportionee.NetWeight += Core.Constants.Weight.ConvertSafe(balance, totalNetWeight.Unit, apportionee.NetWeightUQ));
		}

		void ReconcileWeightIfRequired(bool requiredReconcileWeight, IWeightApportionee[] allApportionees, ZWeight totalWeight, ZDecimal oldTotalAllApportioneesWeight,
			Func<IWeightApportionee, decimal> sumSelector,
			Func<IWeightApportionee, ZDecimal> specifiedApportioneeSelector,
			Action<IWeightApportionee, ZDecimal> reconcileAction)
		{
			if (requiredReconcileWeight)
			{
				int weightDecimalPlaces = 3;
				var totalAllApportioneesWeight = ((ZDecimal)allApportionees.Sum(sumSelector)).Round(weightDecimalPlaces);
				var hasChanged = oldTotalAllApportioneesWeight.Round(weightDecimalPlaces) != totalAllApportioneesWeight;
				var balance = totalWeight.Amount - totalAllApportioneesWeight;
				if (hasChanged && balance != 0)
				{
					var apportionee = allApportionees.OrderByDescending(specifiedApportioneeSelector).First();
					reconcileAction.Invoke(apportionee, balance);
				}
			}
		}

		void ApportionNetWeightViaScaling(IWeightApportionee[] allApportionees, ZWeight totalWeight, ZWeight totalNetWeight)
		{
			if (!totalNetWeight.Amount.IsEmpty && totalWeight.IsValid && totalNetWeight.IsValid)
			{
				var totalNetWeightInTotalWeightUnit = totalWeight.ConvertTo(totalNetWeight.Unit);
				if (totalNetWeightInTotalWeightUnit != 0)
				{
					var ratio = totalNetWeight.Amount / totalNetWeightInTotalWeightUnit;
					foreach (var apportionee in allApportionees)
					{
						if (apportionee.NeedToApportionNetWeight)
						{
							var netWeight = new ZWeight(ZDecimal.Zero, totalNetWeight.Unit);
							netWeight += new ZWeight(apportionee.Weight, apportionee.WeightUQ) * ratio;
							apportionee.NetWeight = netWeight.Amount.Round(3);
							apportionee.NetWeightUQ = netWeight.Unit;
						}
					}
				}
			}
		}

		void ApportionNetWeightByAmount(IWeightApportionee[] allApportionees, ZWeight totalNetWeight)
		{
			if (!totalNetWeight.Amount.IsEmpty && !totalNetWeight.Unit.IsEmpty)
			{
				var totalAmount = GetTotalAmount(allApportionees);
				foreach (var apportionee in allApportionees)
				{
					if (apportionee.NeedToApportionNetWeight)
					{
						var netWeight = GetApportionWeightByAmount(totalNetWeight, totalAmount, apportionee.Amount, apportionee.NetWeightUQ.IsEmpty ? totalNetWeight.Unit : apportionee.NetWeightUQ);
						apportionee.NetWeightUQ = netWeight.Unit;
						apportionee.NetWeight = SetMinimumReapportionedWeight(apportionee, netWeight);
					}
				}
			}
		}

		ZWeight GetApportionWeightByAmount(ZWeight totalWeight, ZDecimal totalAmount, ZDecimal amount, ZString unit)
		{
			var weight = new ZWeight(ZDecimal.Zero, unit);
			if (totalAmount > 0)
			{
				weight += totalWeight / totalAmount * amount;
			}
			return weight;
		}

		ZDecimal SetMinimumReapportionedWeight(IWeightApportionee apportionee, ZWeight weight)
		{
			var weightRounded = weight.Amount.Round(3);
			if (apportionee.MinimumReapportionedLineWeight == ZDecimal.Zero)
			{
				return weightRounded;
			}
			else
			{
				if (weightRounded < apportionee.MinimumReapportionedLineWeight)
				{
					return apportionee.MinimumReapportionedLineWeight;
				}
				else
				{
					return weightRounded;
				}
			}
		}

		ZDecimal GetTotalAmount(IWeightApportionee[] allApportionees)
		{
			var result = ZDecimal.Zero;
			foreach (var apportionee in allApportionees)
			{
				result += apportionee.Amount;
			}
			return result;
		}

		bool isApportionmentInProgress;

		internal bool IsWeightApportionmentDeffered => DeferWeightApportionmentSemaphore.IsSuspended;

		internal IDisposable DeferWeightApportionment() =>
			new DisposableList(new IDisposable[]
			{
				new SemaphoreManager(DeferWeightApportionmentSemaphore),
				new DisposableAction(() =>
				{
					if (!IsWeightApportionmentDeffered)
					{
						while (DeferredApportionments.Count > 0)
						{
							var (weightHolder, uncommittedApportionee) = DeferredApportionments.Dequeue();
							ApportionAll(weightHolder, uncommittedApportionee);
						}
					}
				})
			});

		Semaphore DeferWeightApportionmentSemaphore => deferWeightApportionmentSemaphore ?? (deferWeightApportionmentSemaphore = new Semaphore());
		Semaphore deferWeightApportionmentSemaphore;

		Queue<(IWeightHolder WeightHolder, IWeightApportionee UncommittedApportionee)> DeferredApportionments => deferredApportionments ?? (deferredApportionments = new Queue<(IWeightHolder WeightHolder, IWeightApportionee UncommittedApportionee)>());
		Queue<(IWeightHolder WeightHolder, IWeightApportionee UncommittedApportionee)> deferredApportionments;
	}
}

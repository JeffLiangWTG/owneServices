using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class AllocationProductFact : ProductFact, IAllocationProductFact
	{
		public AllocationProductFact(
			OrgSupplierPart part,
			IOrgPartRelation relation,
			bool isDynamic,
			bool hasPickFaces,
			string styleCode,
			string classificationCode,
			string colorCode,
			string sizeCode)
			: base(part, relation, styleCode, classificationCode, colorCode, sizeCode)
		{
			IsDynamic = isDynamic;
			HasPickFaces = hasPickFaces;
			Part = part;
			IsUsingExpiryDate = relation.OU_UseExpiryDate;
			IsUsingSpecifiedSerialNumbers = relation.OU_UseSerialNumber && !relation.OU_IsSerialNumberReleaseCaptured && relation.OU_PickMode.EqualsIgnoringCase(WhsPickMode.Codes.AttributeSpecified);
			HasPalletDefined = part.OP_StockKeepingUnitPerPallet > 0;

			SKUTable = new ConversionsToSKUTableForAllocation(part);
			AllocationQuantityCache = new Dictionary<(decimal, decimal, UOMAllocationModes), decimal>();
		}

		public bool IsDynamic { get; }

		public bool HasPickFaces { get; }

		public bool HasPalletDefined { get; }

		public bool IsUsingExpiryDate { get; }

		public bool IsUsingSpecifiedSerialNumbers { get; }

		public decimal GetQuantityThatCanBeAllocated(decimal inventoryQuantity, decimal orderedQuantity, UOMAllocationModes allocationModes)
		{
			var key = (inventoryQuantity, orderedQuantity, allocationModes);
			if (!AllocationQuantityCache.TryGetValue(key, out var quantity))
			{
				AllocationQuantityCache[key] = quantity = GetQuantityThatCanBeAllocatedCore(inventoryQuantity, orderedQuantity, allocationModes);
			}

			return quantity;
		}

		decimal GetQuantityThatCanBeAllocatedCore(decimal inventoryQuantity, decimal orderedQuantity, UOMAllocationModes allocationModes)
		{
			var qtyToAllocate = Math.Min(inventoryQuantity, orderedQuantity);
			var integerComponent = Math.Floor(qtyToAllocate);
			var fractionalComponent = qtyToAllocate % 1.0m;

			qtyToAllocate = allocationModes.HasFlag(UOMAllocationModes.CanBreakUOMs)
				? GetQuantity(integerComponent, SKUTable, allocationModes, allowBreaking: true)
				: GetQuantity(integerComponent, BiggestPackTypeGrouper.GetGroups(Part, Math.Floor(inventoryQuantity), SKUTable), allocationModes, allowBreaking: false);

			if (allocationModes.HasFlag(UOMAllocationModes.AllocateSplitCases))
			{
				qtyToAllocate += fractionalComponent;
			}

			return qtyToAllocate;
		}

		decimal GetQuantity(
			decimal quantityToAllocate,
			IEnumerable<IPackTypeConversion> conversions,
			UOMAllocationModes allocationModes,
			bool allowBreaking)
		{
			var foundPreciseConversion = false;
			var packs = allowBreaking ? new HashSet<int>() : (ICollection<int>)new List<int>();

			var greedyRemaining = quantityToAllocate;
			var greedySum = 0m;

			var sumOfSkuQtyOnPacks = 0m;

			// At a high level, we have four schemes to find the quantity:
			// 1. A pack quantity is directly divisble by the allocation quantity (e.g. the SKU was included).
			// 2. We only have one valid pack type, use that.
			// 3. We can satisfy the full quantity by greedily allocating packs in descending order.
			// 4. We have to test combinations of pack types if greedy allocation appears to fail. Use a knapsack variant to test these.
			// Step 4 alone can solve this problem but can be performance and memory intensive, so we  check for the earlier cases while preparing inputs.
			foreach (var conversion in conversions
				.Where(c => !c.UOMType.IsEmpty && c.QtySKU <= quantityToAllocate && allocationModes.HasFlag(GetAllocationMode(c.UOMType)))
				.OrderByDescending(c => c.QtySKU))
			{
				if (quantityToAllocate % conversion.QtySKU == 0
					&& (allowBreaking || (quantityToAllocate / conversion.QtySKU <= conversion.PackQty)))
				{
					foundPreciseConversion = true;
					break;
				}

				for (int i = 0; i < conversion.PackQty; i++)
				{
					packs.Add((int)conversion.QtySKU);
				}

				var greedyTotalPacksPossible = Math.Floor(greedyRemaining / conversion.QtySKU);
				greedyTotalPacksPossible = allowBreaking ? greedyTotalPacksPossible : Math.Min(conversion.PackQty, greedyTotalPacksPossible);

				var greedyAllocateQty = Math.Floor(greedyTotalPacksPossible * conversion.QtySKU);
				greedySum += greedyAllocateQty;
				greedyRemaining -= greedyAllocateQty;

				sumOfSkuQtyOnPacks += conversion.QtySKU * conversion.PackQty;

				if (greedyRemaining == 0)
				{
					break;
				}
			}

			if (!foundPreciseConversion && greedyRemaining > 0m)
			{
				// If there are no precise conversions and greedy allocation doesn't work, we must test combinations.
				// But first check that there is more than one valid conversion.
				var packsArray = packs.ToArray();
				if (packsArray.Length == 0)
				{
					quantityToAllocate = 0;
				}
				else if (packsArray.Length == 1)
				{
					var packQty = allowBreaking ? Math.Floor(quantityToAllocate / packsArray[0]) : 1m;
					quantityToAllocate = packQty * packsArray[0];
				}
				else
				{
					// If we can't break packs, upperbound quantity to allocate by the sum of pack qty.
					// We should do this early to facilitate scaling, as we're more likely to find a good common divisor with the pack qty sum.
					if (!allowBreaking && sumOfSkuQtyOnPacks < quantityToAllocate)
					{
						quantityToAllocate = sumOfSkuQtyOnPacks;
					}

					// Take the GCD, this improves performance of knapsack and allows us to support beyond 2.147b quantity in some cases.
					var gcd = packsArray.Select(p => (decimal)p).Append(quantityToAllocate).Aggregate(GreatestCommonDivisor);
					var scaledPacksDescending = packsArray.Select(p => (int)(p / gcd)).OrderByDescending(p => p).ToArray();

					var scaledQuantity = quantityToAllocate / gcd;
					var scaledQuantityAsInt = scaledQuantity > int.MaxValue ? -1 : (int)scaledQuantity;

					if (scaledQuantityAsInt == -1 ||
						WouldRequireTooMuchMemoryToRunKnapsack(scaledQuantityAsInt, scaledPacksDescending.Length))
					{
						quantityToAllocate = greedySum; // Greedy allocation should suffice at these upper limits...
					}
					else
					{
						quantityToAllocate = gcd *
							(allowBreaking
							? GetQuantityUsingKnapsackWithRepetition(scaledQuantityAsInt, scaledPacksDescending)
							: GetQuantityUsing01Knapsack(scaledQuantityAsInt, scaledPacksDescending));
					}
				}
			}

			return quantityToAllocate;

			decimal GreatestCommonDivisor(decimal aDecimal, decimal bDecimal)
			{
				var a = (long)aDecimal;
				var b = (long)bDecimal;

				// See https://stackoverflow.com/a/41766138/12855332, this is Euclid's algorithm without recursion
				while (a != 0 && b != 0)
				{
					if (a > b)
					{
						a %= b;
					}
					else
					{
						b %= a;
					}
				}

				return a | b;
			}

			int GetQuantityUsingKnapsackWithRepetition(int capacity, int[] conversionArrayDescending)
			{
				// This algorithm is a textbook knapsack problem (with repetition) using dynamic programming.
				// We need to do this to ensure we test all combinations of pack types, but we try to short circuit doing this when possible.

				/*
				 * The knapsack algorithm is a mathematically recursive algorithm that uses an iterative implementation (dynamic programming).
				 * At a high level, Dynamic Programming tries to solve problems by splitting them down into smaller subproblem.
				 * 
				 * In the repetition variant, we have a capacity B (e.g. 20) and a set of packs X (e.g. [3, 5, 7]).
				 * We define subproblems using a table T as follows: T[i] = the maximum value we can pack up to a capacity of i.
				 * The optimum solution can then be found by doing T[20]. Dynamic programming allows lookback to avoid repeated computations via the subproblems.
				 * 
				 * The recursive formula (recurrence relation) is:
				 * T[i] = MAX_j(X[j] + T[i - j]) for j in len(X) where i - X[j] >= 0
				 *      = 0 (if none of i - X[j] >= 0)
				 * 
				 * Note that it looks at a previous value in T, and each value in T also considered nibbling away a pack -- which recurses all the way down.
				 * We avoid recursion by actually looping from 1 to B, and looking back at previous values.
				 * 
				 * Consider T[20]:
				 * T[0] = 0, ..., T[3] = 3, T[4] = 3, ..., T[5] = 5, T[6] = 5, T[7] = 7, T[8] = 8 etc. -- note that T[8] = 3 + T[5]
				 * T[20] = 3 + T[17] OR 5 + T[15] OR 7 + T[13], doesn't matter. If we needed to know the packs involved, we'd add an extra array.
				*/

				var table = new int[capacity + 1];

				for (int i = conversionArrayDescending.Last(); i <= capacity; i++)
				{
					var max = 0;
					foreach (var conversionQty in conversionArrayDescending)
					{
						var newCapacity = i - conversionQty;
						if (newCapacity >= 0)
						{
							max = Math.Max(max, conversionQty + table[newCapacity]);
						}
					}

					table[i] = max;
				}

				return table[capacity];
			}

			int GetQuantityUsing01Knapsack(int capacity, int[] conversionArrayDescending)
			{
				// This algorithm is a textbook knapsack problem (the 0-1 variant) using dynamic programming.
				// We need to do this to ensure we test all combinations of packs, but we try to short circuit doing this when possible.
				// For better memory usage and efficiency, we use jagged arrays with thresholding.

				/*
				 * If you are new to dynamic programming and knapsack, please first read the comment in GetQuantityUsingKnapsackWithRepetition.
				 * This version is a variant known as the 0-1 knapsack, it means we can only use a pack type once.
				 * To be able to do dynamic programming lookbacks, we must introduce a new variable for the packs.
				 * 
				 * The new subproblem formulation is: T[j, i] = Maximum units that can be made upto a capacity of i using x[0...j] packs.
				 * The recurrence relation is:
				 * T[j, i] = IF i >= x[j]
				 *           THEN max_a(T[j-1, i], X[a] + T[j-1, i - X[a]]) for a in len(j)
				 *           ELSE T[j-1, i]
				 * 
				 * Note that in this variant we can either use a pack (X[a] + T[j-1, i - X[a]]) or not use a pack (T[j-1, i]).
				 * In either case, we then look back in the table at the subproblem results for earlier pack types, possibly having also reduced the capacity.
				 *
				 * Consider B = 20, with X = {3, 5, 7} again:
				 * T[1, 2] = 0, T[1, 3] = 3, ..., T[1, 20] = 3
				 * T[2, 2] = 0, T[2, 3] = 3, T[2, 4] = 3, T[2, 5] = 5, ..., T[2, 7] = 5, T[2, 8] = 8, T[2, 20] = 8
				 * T[3, 2] = 0, T[3, 3] = 3, ..., T[3, 5] = 5, ..., T[3, 7] = 7, T[3, 8] = 8, T[3, 10] = 10, T[3, 12] = 12, T[3, 15] = 15, T[3, 20] = 15
				 * 
				 * We could have greedily allocated 15 from 20, but note a greedy algorithm may have failed to find 8 = 3 + 5 (rather than 7)!
				 * 
				 * As in the repetition variant, this recurses down for both packs and capacity, but we build up solutions from earlier subproblems.
				 */
				var table = new int[conversionArrayDescending.Length][];

				var cumulativePacksQtySum = 0;
				var maxValueEncountered = 0;

				for (int i = 0; i < conversionArrayDescending.Length; i++)
				{
					var conversionQty = conversionArrayDescending[i];
					cumulativePacksQtySum += conversionQty;

					table[i] = new int[cumulativePacksQtySum + 1];
					var capacityToPopulateUpTo = Math.Min(capacity, cumulativePacksQtySum);

					for (int b = conversionQty; b <= capacityToPopulateUpTo; b++)
					{
						var maxQuantity = conversionQty;

						if (i > 0)
						{
							var previousPackMaxQuantities = table[i - 1];
							var previousPackCapacityUpperBound = previousPackMaxQuantities.Length - 1;
							maxQuantity = previousPackMaxQuantities[Math.Min(b, previousPackCapacityUpperBound)];

							var newCapacity = b - conversionQty;
							var previousPacksMaxQuantityAfterSubtractingThisPack = newCapacity > previousPackCapacityUpperBound ? maxQuantity : previousPackMaxQuantities[newCapacity];
							maxQuantity = Math.Max(maxQuantity, conversionQty + previousPacksMaxQuantityAfterSubtractingThisPack);
						}

						table[i][b] = maxQuantity;

						if (maxQuantity > maxValueEncountered)
						{
							maxValueEncountered = maxQuantity;

							if (maxValueEncountered == capacity)
							{
								return maxValueEncountered;
							}
						}
					}
				}

				return maxValueEncountered;
			}

			bool WouldRequireTooMuchMemoryToRunKnapsack(int scaledQuantity, int scaledPacksLength)
			{
				// C# can't support objects greater than 2gb, so short circuit the comprehensive checks if we are running close to this.
				// This is specifically checking if the arrays we would allocate are >= ~1.9gb.
				var oneGbInBytes = (decimal)1e+9;
				var requiredInts = (decimal)(allowBreaking ? scaledQuantity : scaledQuantity * scaledPacksLength);
				return (requiredInts * sizeof(int)) >= (1.9m * oneGbInBytes);
			}
		}

		static UOMAllocationModes GetAllocationMode(string uomType)
		{
			switch (uomType)
			{
				case UOMPackTypesList.Codes.SplitCase:
					return UOMAllocationModes.AllocateSplitCases;

				case UOMPackTypesList.Codes.Case:
					return UOMAllocationModes.AllocateCases;

				case UOMPackTypesList.Codes.Pallet:
					return UOMAllocationModes.AllocatePallets;

				default:
					return UOMAllocationModes.None;
			}
		}

		Dictionary<(decimal InventoryQuantity, decimal OrderedQuantity, UOMAllocationModes Modes), decimal> AllocationQuantityCache { get; }
		ConversionsToSKUTableForAllocation SKUTable { get; }
		OrgSupplierPart Part { get; }
	}
}

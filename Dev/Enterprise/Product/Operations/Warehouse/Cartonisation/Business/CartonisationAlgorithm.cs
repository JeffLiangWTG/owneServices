using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Core;
using Enterprise.Warehouse.Cartonisation.Integration;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	/// <summary>
	/// NOTE: Still needs some refactoring work!
	/// 
	/// The algorithm was developed by Mohammad Hosein Khakbaz in consultation with the product team, we (Product Warehouse Development) are gradually refactoring/improving this code.
	/// Tests have been added and the code has been considerably improved over the first version (wasn't developed under source control).
	/// </summary>
	public class CartonisationAlgorithm : ICartonisation
	{
		//const int TuneParameters = 300; // looks like a parameter to determine how many times we want to run optimisation, but the usage was removed as it was improperly implemented
		public const string DefaultWeightUnitCode = Constants.Weight.Grams;
		public const string DefaultLengthUnitCode = Constants.Length.Centimetres;
		public const string DefaultVolumeUnitCode = Constants.Volume.CubicCentimeters;

		protected virtual IRandom CompositionRandom => compositionRandom.Value;
		readonly Lazy<IRandom> compositionRandom = new Lazy<IRandom>(() => new CartonisationRandom());

		protected virtual IRandom ShufflingListRandom => shufflingListRandom.Value;
		readonly Lazy<IRandom> shufflingListRandom = new Lazy<IRandom>(() => new CartonisationRandom());

		protected virtual int MaxNumberOfParallelProcessing => -1;

		LocalSearchSolutionComparer LocalSolutionComparer => localSolutionComparer ?? (localSolutionComparer = new LocalSearchSolutionComparer());
		LocalSearchSolutionComparer localSolutionComparer;

		LocalSearchSolutionIncludingItemsComparer LocalSolutionIncludingItemsComparer => localSolutionIncludingItemsComparer ?? (localSolutionIncludingItemsComparer = new LocalSearchSolutionIncludingItemsComparer());
		LocalSearchSolutionIncludingItemsComparer localSolutionIncludingItemsComparer;

		public IEnumerable<ICartonWithItems> CartoniseItems(IEnumerable<ICartonisableItem> cartonisableItems, IEnumerable<ICartonDefinition> cartons)
		{
			var itemsConverted = Convert(cartonisableItems);
			var cartonsConverted = Convert(cartons);
			return Cartonisation(itemsConverted, cartonsConverted);
		}

#if DEBUG
		public
#endif
		static IEnumerable<Carton> Convert(IEnumerable<ICartonDefinition> cartons)
		{
			foreach (var carton in cartons)
			{
				yield return new Carton(carton.PK,
					ConvertLength(carton.Height, carton.DimensionUQ),
					ConvertLength(carton.Length, carton.DimensionUQ),
					ConvertLength(carton.Width, carton.DimensionUQ),
					ConvertVolume(carton.Volume, carton.VolumeUQ),
					carton.MaxFillPercent,
					carton.MaxNumberOfUnits,
					ConvertWeight(carton.MaxWeight - carton.EmptyWeight, carton.WeightUQ),
					carton.Cost);
			}
		}

#if DEBUG
		public
#endif
		static IEnumerable<CartonisableItem> Convert(IEnumerable<ICartonisableItem> cartonisableItems)
		{
			var cartonisableItemDefinitionDictionary = new Dictionary<ICartonisableItemDefinition, CartonisableItemDefinition>();

			foreach (var item in cartonisableItems)
			{
				yield return new CartonisableItem(Convert(item.ItemDefinition, cartonisableItemDefinitionDictionary))
				{
					OriginalPK = item.PK,
					PK = item.PK,
					LocationPK = item.LocationPK,
					QTY = item.Quantity
				};
			}
		}

#if DEBUG
		public
#endif
		static CartonisableItemDefinition Convert(ICartonisableItemDefinition item, Dictionary<ICartonisableItemDefinition, CartonisableItemDefinition> cartonisableItemDefinitionDictionary)
		{
			if (!cartonisableItemDefinitionDictionary.TryGetValue(item, out var result))
			{
				cartonisableItemDefinitionDictionary[item] = result = new CartonisableItemDefinition(
					item.PK,
					ConvertLength(item.Height, item.DimensionUQ),
					ConvertLength(item.Length, item.DimensionUQ),
					ConvertLength(item.Width, item.DimensionUQ),
					ConvertVolume(item.Volume, item.VolumeUQ),
					ConvertWeight(item.Weight, item.WeightUQ),
					item.KeepUpright);
			}

			return result;
		}

		IEnumerable<ICartonWithItems> Cartonisation(IEnumerable<CartonisableItem> itemsToCartonise, IEnumerable<Carton> allCartons)
		{
			var itemsToChunk = itemsToCartonise.ToList();

			// The algorithm has a greedy bias due to its use of First Fit. So we sort the cartons to take advantage of this.
			// First by Cost ascending (self explanatory), but then by Volume descending as this also helps greedily minimize cartons.
			// The "Minimize Volume" option in CW1 will set the cost to volume and is therefore unaffected by the volume sort.
			var cartons = allCartons.OrderBy(c => c.Cost).ThenByDescending(c => c.Volume).ToList();

			var validItemsToCartonise = ChunkItemsIntoSmallerPieces(itemsToChunk, cartons);
			var iterationFactor = Math.Min(10, validItemsToCartonise.Count / itemsToChunk.Count);

			var maxNumberOfRequiredCartonsOfEachType = Phase0(cartons, validItemsToCartonise);
			var bestLocalSolutionForPhaseOne = Phase1(cartons, validItemsToCartonise, maxNumberOfRequiredCartonsOfEachType, iterationFactor);
			var bestLocalSolutionForPhaseTwo = Phase2(cartons, validItemsToCartonise, bestLocalSolutionForPhaseOne, maxNumberOfRequiredCartonsOfEachType, iterationFactor);
			var bestLocalSolutionForPhaseThree = Phase3(cartons, validItemsToCartonise, bestLocalSolutionForPhaseTwo);

			return bestLocalSolutionForPhaseThree.Result;
		}

		/// <summary>
		/// This phase calculates an upper bound of required cartons for each size, to narrow later searches.
		/// It does this by running a first fit algorithm across all cartons that fit, limited to a specific size.
		/// </summary>
		IReadOnlyList<int> Phase0(
			IReadOnlyCollection<Carton> cartons,
			IReadOnlyList<CartonisableItem> itemsToCartonise)
		{
			var maxNumberOfRequiredCartonsOfEachType = new List<int>(cartons.Count);

			foreach (var cartonType in cartons)
			{
				var itemsThatCanFitIntoTheCarton = GetItemsThatCanFitIntoTheCarton(itemsToCartonise, cartonType);
				var allItemsCartonised = itemsThatCanFitIntoTheCarton.Count == 0;

				var numberOfNeededCartons = 0;
				while (!allItemsCartonised)
				{
					numberOfNeededCartons++;

					// May need to run this TuneParameters = 300 number of times and get best solution from those, as it was before it was just always taking the first one
					var generatedItemGroups = ShuffleListAndRunFirstFitItemsAlgorithm(cartonType, itemsThatCanFitIntoTheCarton, ShuffleListWithMerge);
					var solution = SolutionConstructor(generatedItemGroups, numberOfNeededCartons, cartonType);

					allItemsCartonised = solution.FilledCartons.SelectMany(line => line.ItemsInList).Count() == itemsThatCanFitIntoTheCarton.Count;
				}

				maxNumberOfRequiredCartonsOfEachType.Add(numberOfNeededCartons);
			}

			return maxNumberOfRequiredCartonsOfEachType;
		}

		/// <summary>
		/// This phase gets an approximate solution by generating and testing candidate solutions.
		/// Candidate solutions are constructed by generating a random tighter upperbound between [0, maxUpperBound], and running a packing algorithm over it.
		/// This approach is greedy biased as the earlier carton types in the generated solution may be sufficient and will therefore be used in more solutions. We sort to exploit this earlier.
		/// </summary>
		LocalSearchSolution Phase1(
			IReadOnlyList<Carton> cartons,
			IReadOnlyCollection<CartonisableItem> itemsToCartonise,
			IReadOnlyCollection<int> maxNumberOfRequiredCartonsOfEachType,
			int iterationFactor)
		{
			var maxAlgorithmIterations = 5000 * iterationFactor;
			var minAlgorithmIterations = 500 * iterationFactor;

			return PhaseCore(cartons, itemsToCartonise, () => CreateCompositionForPhase1(maxNumberOfRequiredCartonsOfEachType), maxAlgorithmIterations, minAlgorithmIterations, ShuffleListWithMerge);

			IReadOnlyList<int> CreateCompositionForPhase1(IReadOnlyCollection<int> upperBounds)
			{
				var composition = new List<int>(upperBounds.Count);
				var percentChanceToSkipCarton = 100 * (1 - (1.0 / upperBounds.Count)); // Chance to use a carton = 1 / numberOfCartons

				foreach (var upperBound in upperBounds)
				{
					// Most cartons will go unused, we add this step to ensure we sample candidate solutions with 0 cartons of a given size.
					var useCarton = CompositionRandom.Next(0, 100) > percentChanceToSkipCarton;

					composition.Add(useCarton ? CompositionRandom.Next(0, upperBound + 1) : 0);
				}

				return composition;
			}
		}

		/// <summary>
		/// This step refines the solution from Phase1 by attempting variations on the best solution, by slightly varying the number of cartons +/- 2 randomly.
		/// </summary>
		LocalSearchSolution Phase2(
			IReadOnlyList<Carton> cartons,
			IReadOnlyCollection<CartonisableItem> itemsToCartonise,
			LocalSearchSolution bestPhase1Solution,
			IReadOnlyList<int> maxNumberOfRequiredCartonsOfEachType,
			int iterationFactor)
		{
			var maxAlgorithmIterations = 300 * iterationFactor;
			var minAlgorithmIterations = 30 * iterationFactor;

			var numberOfRequiredCartonsInCurrentBestSolution = GetCompositionFromBestSolution(cartons, bestPhase1Solution);

			return PhaseCore(cartons, itemsToCartonise, () => CreateCompositionForPhase2(numberOfRequiredCartonsInCurrentBestSolution, maxNumberOfRequiredCartonsOfEachType), maxAlgorithmIterations, minAlgorithmIterations, ShuffleListWithMerge, bestPhase1Solution);

			IReadOnlyList<int> CreateCompositionForPhase2(
				int[] currentSolution,
				IReadOnlyList<int> upperBounds)
			{
				const int PercentChanceToKeepCartonUnchanged = 70;

				var composition = new List<int>(currentSolution.Length);
				for (var i = 0; i < currentSolution.Length; i++)
				{
					var current = currentSolution[i];
					var upperBound = upperBounds[i];

					var varyCarton = CompositionRandom.Next(0, 100) > PercentChanceToKeepCartonUnchanged;
					composition.Add(varyCarton ? CompositionRandom.Next(Math.Max(current - 2, 0), Math.Min(current + 2, upperBound)) : current);
				}

				return composition;
			}
		}

		static int[] GetCompositionFromBestSolution(IReadOnlyList<Carton> cartons, LocalSearchSolution bestPhase1Solution)
		{
			var cartonMap = cartons.Select((carton, index) => new { carton.PK, index }).ToDictionary(o => o.PK, o => o.index);
			var numberOfRequiredCartonsInCurrentBestSolution = new int[cartonMap.Count];

			foreach (var line in bestPhase1Solution.Cartons)
			{
				numberOfRequiredCartonsInCurrentBestSolution[cartonMap[line.Carton.PK]]++;
			}

			return numberOfRequiredCartonsInCurrentBestSolution;
		}

		/// <summary>
		/// This phase fixes the selection of cartons based on Phase2, but attempts to run the first fit algorithm against different arrangements of items.
		/// This will rearrange the packed items within the cartons such that the spread of products and locations is minimized.
		/// </summary>
		LocalSearchSolution Phase3(
			IReadOnlyList<Carton> cartons,
			IReadOnlyCollection<CartonisableItem> itemsToCartonise,
			LocalSearchSolution bestPhase2Solution)
		{
			var numberOfCartons = bestPhase2Solution.Cartons.Count;
			var numberOfProductsAndLocations = itemsToCartonise
				.Select(i => new { i.ItemDefinition.PK, i.LocationPK })
				.Distinct()
				.Count();

			return numberOfCartons == 1 || numberOfProductsAndLocations == 1 ? bestPhase2Solution : Phase3Core();

			LocalSearchSolution Phase3Core()
			{
				var maxAlgorithmIterations = (int)Math.Min(Math.Pow(numberOfProductsAndLocations, 2), 5000);
				var minAlgorithmIterations = Math.Max(numberOfProductsAndLocations, 10);

				var composition = GetCompositionFromBestSolution(cartons, bestPhase2Solution);

				return PhaseCore(
					cartons,
					itemsToCartonise,
					() => composition,
					maxAlgorithmIterations,
					minAlgorithmIterations,
					ShuffleListBasedOnProductAndLocations,
					bestPhase2Solution,
					considerProductAndLocation: true);
			}

			IEnumerable<int> ShuffleListBasedOnProductAndLocations(IReadOnlyCollection<CartonisableItem> items)
			{
				return items
					.Select((item, index) => (item, index))
					.OrderBy(i => i.item.ItemDefinition.PK)
					.ThenBy(i => i.item.LocationPK)
					.ThenBy(i => CompositionRandom.Next(short.MaxValue)) // Shuffle within the item/location
					.GroupBy(i => i.item.ItemDefinition.PK)
					.OrderBy(g => CompositionRandom.Next(short.MaxValue)) // Shuffle Product groups
					.Select(g => g
							.GroupBy(g => g.item.LocationPK)
							.OrderBy(g2 => CompositionRandom.Next(short.MaxValue))) // Shuffle Location groups within the Product group
					.SelectMany(g => g.SelectMany(g2 => g2))
					.Select(i => i.index)
					.ToArray();
			}
		}

		LocalSearchSolution PhaseCore(IReadOnlyList<Carton> cartons,
			IReadOnlyCollection<CartonisableItem> items,
			Func<IReadOnlyList<int>> compositionFunc,
			int maxAlgorithmIterations,
			int minAlgorithmIterations,
			Func<IReadOnlyList<CartonisableItem>, IEnumerable<int>> randomizeItemIndices,
			LocalSearchSolution bestSolution = null,
			bool considerProductAndLocation = false)
		{
			bestSolution ??= new LocalSearchSolution();

			var algorithmIterationCount = 0;
			var repeatedSolutionCount = 0;
			var foundApproximateSolution = false;
			var bestCost = bestSolution.TotalCost;

			var solutionComparer = considerProductAndLocation ? LocalSolutionIncludingItemsComparer : LocalSolutionComparer;

			var lockObject = new object();
			var testedSolutions = new ConcurrentDictionary<string, bool>();

			_ = Parallel.For(
				0,
				maxAlgorithmIterations,
				new ParallelOptions { MaxDegreeOfParallelism = MaxNumberOfParallelProcessing },
				(iter, loopState) =>
				{
					if (!foundApproximateSolution)
					{
						var composition = compositionFunc();
						var localSolution = GetLocalSolution(cartons, composition, items, randomizeItemIndices, solutionComparer);

						if (items.Count == localSolution.TotalPackedItems)
						{
							var diversifyLocalSearchSolution = AttemptToMoveItemsBetweenCartons(localSolution, considerProductAndLocation);

							lock (lockObject)
							{
								var localSolutionCost = diversifyLocalSearchSolution.TotalCost;
								var bestSolutionCost = bestSolution.TotalCost;

								var key = GetLocalSolutionID(diversifyLocalSearchSolution);
								var solutionPreviouslyTested = testedSolutions.GetOrAdd(key, _ =>
								{
									repeatedSolutionCount = 0;

									if (bestSolutionCost == 0
										|| solutionComparer.Compare(diversifyLocalSearchSolution, bestSolution) < 0)
									{
										bestSolution = diversifyLocalSearchSolution;
										algorithmIterationCount = 0;
									}
									else
									{
										algorithmIterationCount++;
									}

									if (algorithmIterationCount >= minAlgorithmIterations)
									{
										foundApproximateSolution = true;
									}

									return true;
								});

								if (solutionPreviouslyTested)
								{
									repeatedSolutionCount++;
									if (repeatedSolutionCount >= minAlgorithmIterations)
									{
										foundApproximateSolution = true;
									}
								}
							}
						}
						else
						{
							algorithmIterationCount++;
						}
					}
					else
					{
						loopState.Stop();
						return;
					}
				});

			return bestSolution;
		}

		string GetLocalSolutionID(LocalSearchSolution solution)
		{
			var lineKeys = new List<string>(solution.Cartons.Count);

			// Create a unique key of solution in the form of "CartonPK/Item1PK/Qty/Item2PK/Qty..."
			foreach (var line in solution.Cartons)
			{
				var items = line.ItemsInList
					.GroupBy(i => i.OriginalPK)
					.Select(group => new { PK = group.Key, Count = group.Sum(i => i.QTY) })
					.OrderBy(i => i.PK);

				var sb = new StringBuilder();
				sb.Append(line.Carton.PK.ToString());

				foreach (var item in items)
				{
					sb.Append($"/{item.PK}/{item.Count}");
				}

				lineKeys.Add(sb.ToString());
			}

			lineKeys.Sort();
			return string.Join("+", lineKeys);
		}

		#region FilterAndChunkItemsIntoBiggestPossiblePieces

#if DEBUG
		public
#endif
		static List<CartonisableItem> ChunkItemsIntoSmallerPieces(IEnumerable<CartonisableItem> itemsToCartonise, IEnumerable<Carton> cartons)
		{
			var maxChunkSize = (int)cartons.Select(b => b.MaxNumberOfUnits).Aggregate(GreatestCommonDivisor);

			var validOrders = new List<CartonisableItem>();
			foreach (var item in itemsToCartonise)
			{
				var smallestMaxQtyPerCarton = GetSmallestMaximumAmountThatCanFitInOneCarton(cartons, item);
				var sizeForChunk = Math.Min(maxChunkSize, smallestMaxQtyPerCarton);

				if (sizeForChunk > 0)
				{
					var numberOfCartons = (int)item.QTY / sizeForChunk;
					for (var i = 0; i < numberOfCartons; i++)
					{
						validOrders.Add(CreateChunkedItem(item, sizeForChunk));
					}

					// Create individual lines for any remaining units
					// This allows the algorithm to try and use individual units to fill cartons
					// which would otherwise remain partially filled with grouped lines
					var leftOver = item.QTY % sizeForChunk;
					for (var i = 0; i < leftOver; i++)
					{
						validOrders.Add(CreateChunkedItem(item, 1m));
					}
				}
			}

			return validOrders;

			// Algorithm taken from Enterprise\Product\Operations\Warehouse\Transactions\Warehouse.Transactions.Facts\Allocation\AllocationProductFact.cs
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

			CartonisableItem CreateChunkedItem(CartonisableItem item, decimal qty)
				=> new CartonisableItem(item.ItemDefinition)
				{
					OriginalPK = item.OriginalPK,
					PK = Guid.NewGuid(),
					LocationPK = item.LocationPK,
					QTY = qty,
				};
		}

		static int GetSmallestMaximumAmountThatCanFitInOneCarton(IEnumerable<Carton> cartons, CartonisableItem item)
		{
			var minQty = 0;
			foreach (var carton in cartons)
			{
				if (carton.CanFitOneSingleItem(item.ItemDefinition))
				{
					var maxQtyForTheCarton = carton.GetMaxUnitsForItem(item.ItemDefinition);
					var maxQtyInt = (int)maxQtyForTheCarton;
					if (maxQtyInt != 0m && (minQty == 0 || maxQtyInt < minQty))
					{
						minQty = maxQtyInt;
					}
				}
				else
				{
					carton.SetZeroMaxUnitsForItem(item.ItemDefinition);
				}
			}

			return minQty;
		}

		#endregion

		LocalSearchSolution GetLocalSolution(
			IReadOnlyList<Carton> cartons,
			IReadOnlyList<int> composition,
			IReadOnlyCollection<CartonisableItem> items,
			Func<IReadOnlyList<CartonisableItem>, IEnumerable<int>> randomizeItemIndices,
			LocalSearchSolutionComparer comparer)
		{
			LocalSearchSolution localSolution = null;
			var numIterations = Math.Min(items.Count + cartons.Count, 25);

			for (var iteration = 0; iteration < numIterations; iteration++)
			{
				var itemsLeft = items.ToHashSet();
				var tempLocalSolution = new LocalSearchSolution();

				var varyCartonOrders = ShufflingListRandom.Next(0, 100) > 50; // Since carton sizes were greedily sorted earlier
				var cartonAndMaximums = cartons.Select((c, i) => new { Carton = c, MaxToUse = composition[i] });
				cartonAndMaximums = varyCartonOrders ? cartonAndMaximums.OrderBy(c => ShufflingListRandom.Next(short.MaxValue)) : cartonAndMaximums;

				foreach (var cartonAndMaximum in cartonAndMaximums)
				{
					var validOrderList = GetItemsThatCanFitIntoTheCarton(itemsLeft, cartonAndMaximum.Carton);
					if (validOrderList.Count > 0)
					{
						var generatedItemGroups = ShuffleListAndRunFirstFitItemsAlgorithm(cartonAndMaximum.Carton, validOrderList, randomizeItemIndices);
						var solution = SolutionConstructor(generatedItemGroups, cartonAndMaximum.MaxToUse, cartonAndMaximum.Carton);

						tempLocalSolution.AddCartons(solution.FilledCartons);
						foreach (var i in solution.FilledCartons.SelectMany(l => l.ItemsInList))
						{
							itemsLeft.Remove(i);
						}

						if (itemsLeft.Count == 0)
						{
							break;
						}
					}
				}

				if (localSolution == null || comparer.Compare(tempLocalSolution, localSolution) < 0)
				{
					localSolution = tempLocalSolution;
					iteration = 0;
				}
			}

			return localSolution ?? new LocalSearchSolution();
		}

		IReadOnlyList<CartonisableItem> GetItemsThatCanFitIntoTheCarton(IReadOnlyCollection<CartonisableItem> items, Carton carton)
		{
			var supportedItems = new List<CartonisableItem>(items.Count);

			foreach (var item in items)
			{
				if (carton.CanFitItems(item.ItemDefinition, item.QTY))
				{
					supportedItems.Add(item);
				}
			}

			return supportedItems;
		}

		List<ItemsGroup> ShuffleListAndRunFirstFitItemsAlgorithm(
			Carton carton,
			IReadOnlyList<CartonisableItem> itemsToCartonise,
			Func<IReadOnlyList<CartonisableItem>, IEnumerable<int>> randomizeItemIndices)
		{
			var shuffledItemsIndices = randomizeItemIndices(itemsToCartonise);
			return FirstFitItemsAlgorithm(carton, itemsToCartonise, shuffledItemsIndices);
		}

		IEnumerable<int> ShuffleListWithMerge(IReadOnlyCollection<CartonisableItem> list) => Enumerable.Range(0, list.Count).OrderBy(i => ShufflingListRandom.Next(short.MaxValue)).ToArray();

#if DEBUG
		public
#endif
		List<ItemsGroup> FirstFitItemsAlgorithm(Carton carton, IReadOnlyList<CartonisableItem> itemsToCartonise, IEnumerable<int> shuffledItemsIndex)
		{
			var result = new List<ItemsGroup>();

			var currentCartonItems = new ItemsGroup();
			var runningWeight = 0m;
			var runningVolume = 0m;
			var runningQty = 0m;

			var currentGroup = new List<CartonisableItem>();
			var groupWeight = 0m;
			var groupVolume = 0m;
			var groupQty = 0m;

			foreach (var index in shuffledItemsIndex)
			{
				PerformFirstFit(itemsToCartonise[index]);
			}
			PerformFirstFit(null); // want to process the last item

			if (currentCartonItems.ItemNodes.Count > 0)
			{
				result.Add(currentCartonItems);
			}

			return result;

			void PerformFirstFit(CartonisableItem item)
			{
				if (item != null && (currentGroup.Count == 0 || currentGroup[0].OriginalPK == item.OriginalPK))
				{
					groupWeight += item.ItemWeight;
					groupVolume += item.ItemVolume;
					groupQty += item.QTY;
					currentGroup.Add(item);
				}
				else
				{
					if (CanFitGroupIntoCurrentCarton())
					{
						runningWeight += groupWeight;
						runningVolume += groupVolume;
						runningQty += groupQty;
						currentCartonItems.ItemNodes.AddRange(currentGroup);
					}
					else if (CanFitGroupIntoSingleCarton())
					{
						result.Add(currentCartonItems);
						currentCartonItems = new ItemsGroup();
						runningWeight = groupWeight;
						runningVolume = groupVolume;
						runningQty = groupQty;
						currentCartonItems.ItemNodes.AddRange(currentGroup);
					}
					else
					{
						foreach (var sameItem in currentGroup)
						{
							runningWeight += sameItem.ItemWeight;
							runningVolume += sameItem.ItemVolume;
							runningQty += sameItem.QTY;

							if (carton.MaxWeight >= runningWeight &&
								carton.VolumeToFill >= runningVolume &&
								carton.MaxNumberOfUnits >= runningQty)
							{
								currentCartonItems.ItemNodes.Add(sameItem);
							}
							else
							{
								result.Add(currentCartonItems);
								currentCartonItems = new ItemsGroup();
								currentCartonItems.ItemNodes.Add(sameItem);
								runningWeight = sameItem.ItemWeight;
								runningVolume = sameItem.ItemVolume;
								runningQty = sameItem.QTY;
							}
						}
					}

					currentGroup = new List<CartonisableItem>();
					if (item != null)
					{
						groupWeight = item.ItemWeight;
						groupVolume = item.ItemVolume;
						groupQty = item.QTY;
						currentGroup.Add(item);
					}
				}
			}

			bool CanFitGroupIntoCurrentCarton() => (groupWeight + runningWeight <= carton.MaxWeight) && (groupVolume + runningVolume <= carton.VolumeToFill) && (groupQty + runningQty <= carton.MaxNumberOfUnits);
			bool CanFitGroupIntoSingleCarton() => (groupWeight <= carton.MaxWeight) && (groupVolume <= carton.VolumeToFill) && (groupQty <= carton.MaxNumberOfUnits);
		}

		Solution SolutionConstructor(List<ItemsGroup> candiateItemGroupList, int numberOfCartons, Carton carton)
		{
			var candidateItemGroups = candiateItemGroupList.Count > numberOfCartons
				? ShuffleList(candiateItemGroupList)
				: candiateItemGroupList;

			var solution = new Solution();
			for (var i = 0; i < candidateItemGroups.Count; i++)
			{
				if (i < numberOfCartons)
				{
					var filledCarton = new CartonItems(carton, candidateItemGroups[i].ItemNodes.ToList());
					solution.FilledCartons.Add(filledCarton);
					solution.AssignedItemGroups.Add(candidateItemGroups[i]);
				}
				else
				{
					solution.UnassignedItemGroups.Add(candidateItemGroups[i]);
				}
			}

			return solution;
		}

		LocalSearchSolution AttemptToMoveItemsBetweenCartons(LocalSearchSolution candidateSolution, bool considerProductAndLocation)
		{
			var solutionComparer = considerProductAndLocation ? LocalSolutionIncludingItemsComparer : LocalSolutionComparer;

			var result = GetNewSolution(candidateSolution.Cartons);
			var betterSolutionFound = true;

			while (betterSolutionFound)
			{
				betterSolutionFound = false;
				var candidateItemAssignments = candidateSolution.Cartons.Select(c => new CartonItemsCacheWrapper(c)).ToArray();
				foreach (var cartonToMoveFrom in candidateItemAssignments)
				{
					// Except in the final phase, the main benefit of moving item between cartons is if we can remove one carton.
					// We could probably check if we can dissolve a carton between other cartons instead of doing it only for cartons with single item type
					if (cartonToMoveFrom.ItemsInList.Count == 1 || considerProductAndLocation)
					{
						var itemToBeMoved = cartonToMoveFrom.ItemsInList.First();
						foreach (var cartonToMoveInto in candidateItemAssignments)
						{
							if (cartonToMoveFrom != cartonToMoveInto && !cartonToMoveInto.ItemsInList.Any(i => i.PK == itemToBeMoved.PK) && ValidateItemFitsInCarton(cartonToMoveInto, itemToBeMoved))
							{
								var newItemAssignment = GenerateItemAssignments(candidateItemAssignments, itemToBeMoved, cartonToMoveInto.CartonWithItems, cartonToMoveFrom.CartonWithItems);
								var newSolution = GetNewSolution(newItemAssignment);
								if (solutionComparer.Compare(newSolution, result) < 0)
								{
									result = newSolution;
									betterSolutionFound = true;
								}
							}
						}
					}
				}

				if (betterSolutionFound)
				{
					candidateSolution = result;
				}
			}

			return result;
		}

		class CartonItemsCacheWrapper
		{
			public CartonItemsCacheWrapper(CartonItems cartonWithItems)
			{
				CartonWithItems = cartonWithItems;
				totalQtyCache = new Lazy<decimal>(() => CartonWithItems.ItemsInList.Sum(i => i.QTY));
				totalVolumeCache = new Lazy<decimal>(() => CartonWithItems.ItemsInList.Sum(i => i.ItemVolume));
				totalWeightCache = new Lazy<decimal>(() => CartonWithItems.ItemsInList.Sum(i => i.ItemWeight));
			}

			readonly Lazy<decimal> totalQtyCache;
			readonly Lazy<decimal> totalVolumeCache;
			readonly Lazy<decimal> totalWeightCache;

			public CartonItems CartonWithItems { get; }
			public List<CartonisableItem> ItemsInList => CartonWithItems.ItemsInList;
			public decimal TotalQty => totalQtyCache.Value;
			public decimal TotalVolume => totalVolumeCache.Value;
			public decimal TotalWeight => totalWeightCache.Value;
		}

		static LocalSearchSolution GetNewSolution(IEnumerable<CartonItems> cartons) => new LocalSearchSolution(cartons);

		bool ValidateItemFitsInCarton(CartonItemsCacheWrapper cartonWithItemsWrapper, CartonisableItem itemToAdd)
		{
			var carton = cartonWithItemsWrapper.CartonWithItems.Carton;
			return (itemToAdd == null || carton.CanFitOneSingleItem(itemToAdd.ItemDefinition)) &&
				carton.MaxNumberOfUnits >= CalculateCartonQty() &&
				carton.VolumeToFill >= CalculateProductVolume() &&
				carton.MaxWeight >= CalculateProductWeight();

			decimal CalculateCartonQty() => cartonWithItemsWrapper.TotalQty + itemToAdd?.QTY ?? 0;
			decimal CalculateProductVolume() => cartonWithItemsWrapper.TotalVolume + itemToAdd?.ItemVolume ?? 0m;
			decimal CalculateProductWeight() => cartonWithItemsWrapper.TotalWeight + itemToAdd?.ItemWeight ?? 0m;
		}

		IEnumerable<CartonItems> GenerateItemAssignments(IEnumerable<CartonItemsCacheWrapper> baseAssignment, CartonisableItem itemToBeMoved, CartonItems cartonToMoveInto, CartonItems cartonToMoveFrom)
		{
			CartonItems cartonToMoveIntoClone = null;
			CartonItems cartonToMoveFromClone = null;
			var result = new List<CartonItems>(baseAssignment.Select(r =>
			{
				var carton = r.CartonWithItems;
				if (carton == cartonToMoveInto)
				{
					cartonToMoveIntoClone = carton.Clone();
					carton = cartonToMoveIntoClone;
				}
				else if (carton == cartonToMoveFrom)
				{
					cartonToMoveFromClone = carton.Clone();
					carton = cartonToMoveFromClone;
				}

				return carton;
			}));

			if (cartonToMoveFromClone != cartonToMoveIntoClone)
			{
				cartonToMoveFromClone.ItemsInList.Remove(itemToBeMoved);
				cartonToMoveIntoClone.ItemsInList.Add(itemToBeMoved);
			}

			return result.Where(c => c.ItemsInList.Count > 0);
		}

		List<T> ShuffleList<T>(IEnumerable<T> list) => list.OrderBy(l => ShufflingListRandom.Next(short.MaxValue)).ToList();

		static decimal ConvertWeight(decimal sourceValue, string sourceUnitCode)
		{
			return sourceUnitCode == DefaultWeightUnitCode ? sourceValue : Constants.Weight.Convert(sourceValue, sourceUnitCode, DefaultWeightUnitCode);
		}

		static decimal ConvertLength(decimal sourceValue, string sourceUnitCode)
		{
			return sourceUnitCode == DefaultLengthUnitCode ? sourceValue : Constants.Length.Convert(sourceValue, sourceUnitCode, DefaultLengthUnitCode);
		}

		static decimal ConvertVolume(decimal sourceValue, string sourceUnitCode)
		{
			return sourceUnitCode == DefaultVolumeUnitCode ? sourceValue : Constants.Volume.Convert(sourceValue, sourceUnitCode, DefaultVolumeUnitCode);
		}
	}
}

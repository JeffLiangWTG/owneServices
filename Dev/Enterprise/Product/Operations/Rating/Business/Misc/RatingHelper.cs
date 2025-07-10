using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class RatingHelper
	{
		#region Max & MaxPlus1

		public static int Max(BusinessObjectCollection collection, ZString property)
		{
			var max = int.MinValue;

			if (!collection.IsLoaded)
			{
				collection.Load();
			}

			var propInfo = collection.TypeOfElements.GetProperty(property);

			foreach (var @object in collection)
			{
				var ob = propInfo.GetValue(@object, null);
				if (!(ob is INumericZType))
				{
					return int.MinValue;
				}

				var valueString = ob.ToString();
				var value = int.Parse(valueString);
				if (value > max)
				{
					max = value;
				}
			}

			return max;
		}

		public static int MaxPlus1(BusinessObjectCollection collection, ZString property)
		{
			var maxPlus1 = Max(collection, property);

			if (maxPlus1 == int.MinValue)
			{
				return 0;
			}

			var propInfo = collection.TypeOfElements.GetProperty(property);

			if (!(propInfo.PropertyType == typeof(ZInt) && maxPlus1 >= int.MaxValue)
				&& !(propInfo.PropertyType == typeof(ZShort) && maxPlus1 >= short.MaxValue)
				&& !(propInfo.PropertyType == typeof(ZByte) && maxPlus1 >= byte.MaxValue))
			{
				maxPlus1++;
			}

			return maxPlus1;
		}

		#endregion

		#region Current Company Filter

		public static GlbCompany GetCompany(IAutoRating autoRatingInfo)
		{
			var autoRatingGlbCompany = autoRatingInfo as IAutoRatingGlbCompany;
			return autoRatingGlbCompany != null ? autoRatingGlbCompany.Company : GlbCompany.CurrentCompany;
		}

		#endregion

		#region Mode / FCL_LCL / Container Filter

		#region Origin / Destination Exclusive

		public static ZQuery GetOriginDestinationModeExclusiveFilter(string mode)
		{
			try
			{
				return GetOriginDestinationModeExclusiveFilter((FreightMode)Enum.Parse(typeof(FreightMode), mode));
			}
			catch (ArgumentException)
			{
				return new ZQuery();
			}
		}

		public static ZQuery GetOriginDestinationModeExclusiveFilter(FreightMode mode)
		{
			return new ZQuery(RateEntrySchema.TI_Mode, GetPossibleModes(mode));
		}

		public static List<string> GetPossibleModes(FreightMode mode)
		{
			var possibleModes = new List<string>();

			if ((mode & FreightMode.BCN) != 0)
			{
				//Mode could be FCL|BCN / LCL|BCN,...
				possibleModes.Add(Core.Constants.RateMode.BCN);
				possibleModes.Add(Core.Constants.RateMode.ALL);
				return possibleModes;
			}

			if ((mode & FreightMode.SCN) != 0)
			{
				possibleModes.Add(Core.Constants.RateMode.SCN);
				possibleModes.Add(Core.Constants.RateMode.ALL);
				return possibleModes;
			}

			if (mode != FreightMode.UKN)
			{
				possibleModes.Add(mode.ToString());
			}

			if ((mode & FreightMode.AIR) != 0)
			{
				if (mode.ToString() != Core.Constants.RateMode.AIR)
				{
					possibleModes.Add(Core.Constants.RateMode.AIR);
				}
			}
			else if ((mode & FreightMode.SEA) != 0)
			{
				if (mode.ToString() != Core.Constants.RateMode.SEA)
				{
					possibleModes.Add(Core.Constants.RateMode.SEA);
				}
			}
			else if ((mode & FreightMode.ROA) != 0)
			{
				if (mode.ToString() != Core.Constants.RateMode.ROA)
				{
					possibleModes.Add(Core.Constants.RateMode.ROA);
				}
			}
			else if ((mode & FreightMode.RAI) != 0)
			{
				if (mode.ToString() != Core.Constants.RateMode.RAI)
				{
					possibleModes.Add(Core.Constants.RateMode.RAI);
				}
			}
			else if ((mode & FreightMode.COU) != 0)
			{
				if (mode.ToString() != Core.Constants.RateMode.COU)
				{
					possibleModes.Add(Core.Constants.RateMode.COU);
				}
			}

			possibleModes.Add(Core.Constants.RateMode.ALL);
			return possibleModes;
		}

		#endregion

		#region Freight Exclusive

		public static ZString GetRateMode(RatingCriteria criteria, string rateCategory)
		{
			if (string.IsNullOrWhiteSpace(rateCategory))
			{
				throw new ArgumentException("Need to know rate category to convert FreightMode to RateMode");
			}

			string result;

			if (RatingConstants.RateCategory.IsFreight(rateCategory))
			{
				result = ConvertToRateModeWhenItIsFreight(criteria.FreightMode);
			}
			else
			{
				result = criteria.FreightMode.ToString();
				if (result.Length != 3 || result == nameof(FreightMode.UKN))
				{
					result = Core.Constants.RateMode.ALL;
				}
			}

			return result;
		}

		public static ZString ConvertToRateModeWhenItIsFreight(FreightMode mode)
		{
			if ((mode & FreightMode.BCN) != 0 || (mode & FreightMode.SCN) != 0)
			{
				return Core.Constants.RateMode.ALL;
			}

			if ((mode & FreightMode.Containerised) != 0)
			{
				if ((mode & FreightMode.AIR) != 0)
				{
					return Core.Constants.RateMode.ULD;
				}

				if ((mode & FreightMode.SEA) != 0)
				{
					return Core.Constants.RateMode.SEA;
				}

				if ((mode & FreightMode.ROA) != 0)
				{
					return Core.Constants.RateMode.ROA;
				}

				if ((mode & FreightMode.RAI) != 0)
				{
					return Core.Constants.RateMode.RAI;
				}
			}
			if ((mode & FreightMode.NonContainerised) != 0 || (mode & FreightMode.COU) != 0)
			{
				return mode.ToString();
			}

			return Core.Constants.RateMode.ALL;
		}

		public static List<ZString> GetFCL_LCLExclusiveCategories(FreightMode mode)
		{
			List<ZString> categories = new List<ZString>();

			if ((mode & FreightMode.AIR) != 0)
			{
				categories.Add(RatingConstants.RateCategory.AIR);
				categories.Add(RatingConstants.RateCategory.CAI);
			}
			else if ((mode & FreightMode.Containerised) != 0)
			{
				categories.AddRange(ContainerisedCategories.Select(x => new ZString(x)));
			}
			else if ((mode & FreightMode.NonContainerised) != 0)
			{
				categories.AddRange(NonContainerisedCategories.Select(x => new ZString(x)));
			}
			else if ((mode & FreightMode.COU) != 0)
			{
				categories.Add(RatingConstants.RateCategory.LCL);
			}

			return categories;
		}

		internal static readonly ReadOnlyCollection<string> ContainerisedCategories = new ReadOnlyCollection<string>(new[]
		{
			RatingConstants.RateCategory.FCL,
			RatingConstants.RateCategory.SCO,
			RatingConstants.RateCategory.CFC
		});

		internal static readonly ReadOnlyCollection<string> NonContainerisedCategories = new ReadOnlyCollection<string>(new[]
		{
			RatingConstants.RateCategory.LCL,
			RatingConstants.RateCategory.SNC,
			RatingConstants.RateCategory.CLC
		});

		//todo: Move all these other filter helpers into the RateEntryQueries class

		public static ZQuery GetFreightModeAndFCL_LCLExclusiveFilter(FreightMode mode)
		{
			var result = new ZQuery(RateEntrySchema.TI_Mode, ConvertToRateModeWhenItIsFreight(mode));

			if ((mode & FreightMode.AIR) != 0)
			{
				result.AddToFilter(RateEntrySchema.TI_RateCategory, new[] { Category.AIR, Category.CAI });
			}
			else if ((mode & FreightMode.Containerised) != 0)
			{
				result.AddToFilter(RateEntrySchema.TI_RateCategory, ContainerisedCategories);
			}
			else if ((mode & FreightMode.NonContainerised) != 0)
			{
				result.AddToFilter(RateEntrySchema.TI_RateCategory, NonContainerisedCategories);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		#endregion

		#region Non-Freight Inclusive

		public static ZQuery GetNonFreightModeInclusiveFilter(string mode)
		{
			var result = new ZQuery();

			if (mode != Core.Constants.RateMode.ALL)
			{
				result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, mode);

				switch (mode)
				{
					case Core.Constants.RateMode.AIR:
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LSE);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.ULD);
						break;

					case Core.Constants.RateMode.SEA:
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LCL);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FCL);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.GRP);
						break;

					case Core.Constants.RateMode.ROA:
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LRO);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FRO);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FTL);
						break;

					case Core.Constants.RateMode.RAI:
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LRA);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FRA);
						result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FWL);
						break;
				}
			}

			return result;
		}

		#endregion

		#region Freight Inclusive

		public static ZQuery GetFreightModeAndFCL_LCLInclusiveFilter(string mode, RateType? rateType)
		{
			var result = new ZQuery();
			switch (mode)
			{
				case Mode.LSE:
				case Mode.ULD:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetAIRCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, mode);
					break;

				case Mode.AIR:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetAIRCategories(rateType));
					break;

				case Mode.LCL:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetLCLCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, mode);
					break;

				case Mode.LRO:
				case Mode.FTL:
				case Mode.LRA:
				case Mode.FWL:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetLCLRailRoadCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, mode);
					break;

				case Mode.FCL:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetFCLCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, Mode.SEA);
					break;

				case Mode.FRO:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetFCLRoadCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, Mode.ROA);
					break;

				case Mode.FRA:
					result.AddToFilter(RateEntrySchema.TI_RateCategory, GetFCLRoadCategories(rateType));
					result.AddToFilter(RateEntrySchema.TI_Mode, Mode.RAI);
					break;

				case Mode.SEA:
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.FCL, rateType), JoinCondition.Or);
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.LCL, rateType), JoinCondition.Or);
					break;

				case Mode.ROA:
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.FRO, rateType), JoinCondition.Or);
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.LRO, rateType), JoinCondition.Or);
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.FTL, rateType), JoinCondition.Or);
					break;

				case Mode.RAI:
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.FRA, rateType), JoinCondition.Or);
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.LRA, rateType), JoinCondition.Or);
					result.AddToFilter(GetFreightModeAndFCL_LCLInclusiveFilter(Mode.FWL, rateType), JoinCondition.Or);
					break;

				case Mode.ALL:
					result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, GetALLCategories(rateType));
					break;

				default:
					result.IsNoResultQuery = true;
					break;
			}

			return result;
		}

		static string[] GetAIRCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.AIR, Category.CAI }
				: rateType.Value == RateType.Customs
					? new[] { Category.CAI }
					: new[] { Category.AIR };

		static string[] GetFCLRoadCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.FCL, Category.CFC }
					: rateType.Value == RateType.Customs
					? new[] { Category.CFC }
					: new[] { Category.FCL };

		static string[] GetLCLCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.LCL, Category.SNC, Category.CLC }
					: rateType.Value == RateType.Customs
					? new[] { Category.CLC }
						: rateType.Value == RateType.Shipping
						? new[] { Category.SNC }
						: new[] { Category.LCL };

		static string[] GetFCLCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.FCL, Category.SCO, Category.CFC }
					: rateType.Value == RateType.Customs
					? new[] { Category.CFC }
						: rateType.Value == RateType.Shipping
						? new[] { Category.SCO }
						: new[] { Category.FCL };

		static string[] GetLCLRailRoadCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.LCL, Category.CLC }
					: rateType.Value == RateType.Customs
					? new[] { Category.CLC }
					: new[] { Category.LCL };

		static string[] GetALLCategories(RateType? rateType)
			=> rateType == null
				? new[] { Category.AIR, Category.LCL, Category.FCL, Category.SCO, Category.SNC, Category.CAI, Category.CLC, Category.CFC }
					: rateType.Value == RateType.Customs
					? new[] { Category.CAI, Category.CLC, Category.CFC }
						: rateType.Value == RateType.Shipping
						? new[] { Category.SCO, Category.SNC }
						: new[] { Category.AIR, Category.LCL, Category.FCL };

		#endregion

		#region Related Freight Rate Lines

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Multiple switch cases are unavoidable")]
		static bool FreightModeAndFCL_LCLFilter(IRateEntry rateEntry, string categoryToMatch, string modeToMatch)
		{
			switch (modeToMatch)
			{
				case Mode.LSE:
				case Mode.ULD:
					return rateEntry.TI_RateCategory.ToString().In(Category.AIR, Category.CAI) && rateEntry.TI_Mode == modeToMatch;

				case Mode.AIR:
					return rateEntry.TI_RateCategory.ToString().In(Category.AIR, Category.CAI);

				case Mode.LCL:
					return rateEntry.TI_RateCategory.ToString().In(Category.LCL, Category.SNC, Category.CLC) && rateEntry.TI_Mode == modeToMatch;

				case Mode.LRO:
				case Mode.FTL:
				case Mode.LRA:
				case Mode.FWL:
					return rateEntry.TI_RateCategory.ToString().In(Category.LCL, Category.CLC) && rateEntry.TI_Mode == modeToMatch;

				case Mode.FCL:
					return rateEntry.TI_RateCategory.ToString().In(Category.FCL, Category.SCO, Category.CFC) && rateEntry.TI_Mode == Mode.SEA;

				case Mode.FRO:
					return rateEntry.TI_RateCategory.ToString().In(Category.FCL, Category.CFC) && rateEntry.TI_Mode == Mode.ROA;

				case Mode.FRA:
					return rateEntry.TI_RateCategory.ToString().In(Category.FCL, Category.CFC) && rateEntry.TI_Mode == Mode.RAI;

				case Core.Constants.RateMode.SEA:
					return FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.FCL)
						|| FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.LCL);

				case Mode.ROA:
					return FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.FRO)
						|| FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.LRO)
						|| FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.FTL);

				case Mode.RAI:
					return FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.FRA)
						|| FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.LRA)
						|| FreightModeAndFCL_LCLFilter(rateEntry, categoryToMatch, Mode.FWL);

				case Mode.ALL:
					return new ZString[]
						{
							Category.AIR,
							Category.LCL,
							Category.FCL,
							Category.SCO,
							Category.SNC,
							Category.CAI,
							Category.CLC,
							Category.CFC,
						}.Contains(rateEntry.TI_RateCategory);

				default:
					return true;
			}
		}

		static bool ContainerFilter(IRateEntry rateEntry, RefContainer containerToMatch)
		{
			return containerToMatch == null || rateEntry.TI_RC == containerToMatch.PK || (rateEntry.TI_MatchContainerRateClass && rateEntry.Container.MatchesClass(containerToMatch));
		}

		static bool ServiceLevelFilter(IRateEntry rateEntry, string serviceLevelToMatch)
		{
			return string.IsNullOrEmpty(serviceLevelToMatch) || rateEntry.TI_RS_NKServiceLevel_NI == serviceLevelToMatch;
		}

		static bool CommodityCodeFilter(IRateEntry rateEntry, string commodityCodeToMatch)
		{
			return string.IsNullOrEmpty(commodityCodeToMatch) || commodityCodeToMatch == "GEN" || rateEntry.TI_RH_NKCommodityCode == commodityCodeToMatch;
		}

		static bool TransportProviderFilter(IRateEntry rateEntry, ZGuid providerPkToMatch)
		{
			return providerPkToMatch.IsEmpty || rateEntry.TI_OH_TransportProvider == providerPkToMatch;
		}

		/// <summary>
		/// Here we try to find all the rate lines which matches the rate line having FRT Calculator
		/// </summary>
		/// <param name="rateLines">All the rate lines applicable to this job</param>
		/// <param name="lineToMatch">the rate line which has FRT calculator</param>
		/// <returns>matched rate lines</returns>
		public static IEnumerable<IRateLine> GetRelatedFreightRateLines(this IEnumerable<IRateLine> rateLines, IRateLine lineToMatch)
		{
			if (lineToMatch != null)
			{
				rateLines = rateLines.Where(r => IsRelatedFreightRateLine(r, lineToMatch));
			}

			return rateLines;
		}

		/// <summary>
		///		Gets all lines among <paramref name="rateLines"/> included in <paramref name="frtLine"/>.
		/// </summary>
		/// <param name="rateLines">
		///		All rate lines list to check with potentially included lines.
		/// </param>
		/// <param name="frtLine">
		///		The line we seach included lines for.
		/// </param>
		/// <returns>matched rate lines</returns>
		public static IEnumerable<IRateLine> GetIncludedLines(this IEnumerable<IRateLine> rateLines, IRateLine frtLine)
		{
			if (frtLine == null)
			{
				return Array.Empty<IRateLine>();
			}

			var includedLines = rateLines.Where(r => IsRelatedFreightRateLine(frtLine, r)).ToArray();
			return includedLines;
		}

		static bool IsRelatedFreightRateLine(IRateLine possibleFreightLine, IRateLine lineToMatch)
		{
			var result = possibleFreightLine.ChargeCode != null
							&& possibleFreightLine.PK != lineToMatch.PK
							&& IsFreightLine(possibleFreightLine, lineToMatch)
							&& HasMatchingRateEntries(possibleFreightLine.ParentRateEntry, lineToMatch.ParentRateEntry);

			return result;
		}

		static bool IsFreightLine(IRateLine possibleFreightLine, IRateLine lineToMatch)
		{
			var calculator = lineToMatch.GetCalculator<FreightInclusiveCalculator>();
			if (calculator == null)
			{
				return false;
			}

			var relatedChargeCode = !calculator.ChargeCode.IsEmpty
				? calculator.ChargeCode
				: Env.Registry.FreightChargeCode;

			var ledgerType = _Rating.Cost
				? LedgerTypes.AccountsPayable
				: LedgerTypes.AccountsReceivable;

			if (relatedChargeCode == possibleFreightLine.TL_AC)
			{
				return true;
			}

			if (possibleFreightLine.ChargeCode.IsGlobal)
			{
				var localChargeCode = possibleFreightLine.ChargeCode.GetLocalChargeCode(ledgerType, null);
				if (localChargeCode != null && localChargeCode.PK == relatedChargeCode)
				{
					return true;
				}
			}
			else
			{
				var globalChargeCode = possibleFreightLine.ChargeCode.GetGlobalChargeCode(ledgerType, null);
				if (globalChargeCode != null && globalChargeCode.PK == relatedChargeCode)
				{
					return true;
				}
			}

			return false;
		}

		static bool HasMatchingRateEntries(IRateEntry possibleFreightEntry, IRateEntry entryToMatch)
		{
			if (possibleFreightEntry == entryToMatch)
			{
				return true;
			}

			var result = possibleFreightEntry != null
							&& entryToMatch != null
							&& FreightModeAndFCL_LCLFilter(possibleFreightEntry, entryToMatch.TI_RateCategory, entryToMatch.TI_Mode)
							&& TransportProviderFilter(possibleFreightEntry, entryToMatch.TI_OH_TransportProvider)
							&& ServiceLevelFilter(possibleFreightEntry, entryToMatch.TI_RS_NKServiceLevel_NI)
							&& ContainerFilter(possibleFreightEntry, entryToMatch.Container)
							&& CommodityCodeFilter(possibleFreightEntry, entryToMatch.TI_RH_NKCommodityCode);

			return result;
		}

		#endregion

		#region Container

		public static bool ContainerMatch(ZGuid containerType, RefContainerCollection containersInSameClass, IRateEntry entry)
		{
			if (entry.TI_RC != containerType)
			{
				if (containersInSameClass != null)
				{
					return entry.TI_MatchContainerRateClass && containersInSameClass.Select(x => x.PK).Contains(entry.TI_RC);
				}

				return false;
			}

			return true;
		}

		public static void AddContainerQuery(ZGuid containerType, RefContainerCollection containersInSameClass, ZQuery mainContainerQuery)
		{
			mainContainerQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, containerType);

			if (containersInSameClass != null)
			{
				var sameClassQuery = new ZQuery();
				sameClassQuery.AddToFilter(RateEntrySchema.TI_MatchContainerRateClass, ZBool.True);
				sameClassQuery.AddToFilter(RateEntrySchema.TI_RC, containersInSameClass.Select(x => x.PK));
				mainContainerQuery.AddToFilter(sameClassQuery, JoinCondition.Or);
			}
		}

		#endregion

		#endregion

		#region RateLineGridHidesConversionFactorColumn

		public static bool RateLineGridHidesConversionFactorColumn(string rateCategory)
		{
			var displayFor = new[]
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,

				// Customs
				RatingConstants.RateCategory.CAI,
				RatingConstants.RateCategory.CLC,
				RatingConstants.RateCategory.COR,
				RatingConstants.RateCategory.CDS,

				RatingConstants.RateCategory.SNC,
				RatingConstants.RateCategory.TBC,
				RatingConstants.RateCategory.TRN
			};

			return !displayFor.Contains(rateCategory);
		}

		#endregion

		public static bool IsLessSpecificThan(this ILocation currentLocation, ILocation otherLocation, ILocation criteriaLocation)
		{
			if (currentLocation == null)
			{
				return otherLocation != null;
			}
			if (otherLocation == null || string.Equals(currentLocation.Code, otherLocation.Code, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			var criteriaUnloco = criteriaLocation?.UNLOCO;
			if (currentLocation is RefZoneHeader currentZone && otherLocation is RefZoneHeader otherZone && criteriaUnloco != null)
			{
				if (otherZone.UNLOCOs.Contains(criteriaUnloco))
				{
					if (currentZone.UNLOCOs.Contains(criteriaUnloco))
					{
						return currentZone.RelatedParty != null && otherZone.RelatedParty != null && currentZone.RelatedParty.OH_IsShippingLine && !otherZone.RelatedParty.OH_IsShippingLine;
					}
					return true;
				}
			}

			var isLessSpecific = currentLocation.CompletelyCovers(otherLocation);
			var isMoreSpecific = otherLocation.CompletelyCovers(currentLocation);

			if (isLessSpecific == isMoreSpecific)
			{
				return LocationHelper.GetLocationType(currentLocation.Code) > LocationHelper.GetLocationType(otherLocation.Code);
			}

			return isLessSpecific;
		}

		public static HashSet<string> GetAllowedNamedAccounts()
		{
			var security = new SecurityCore(GlbStaff.CurrentUser.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Env.CurrentCompanyPK, false);
			if (security.WiseRatesCargoSphereRateSearchCRMSecurity.IgnoreOSMG.IsAllowed)
			{
				return [];
			}

			var allowedOrganisations = GlbStaff.CurrentUser.Groups.Select(g => g.Organisation).SelectMany(o => o).Distinct();

			var namedAccounts = new HashSet<string>() { "" };
			foreach (OrgHeader org in allowedOrganisations)
			{
				AddNamedAccountsFromOrg(org, namedAccounts);
			}
			return namedAccounts;
		}

		static void AddNamedAccountsFromOrg(OrgHeader org, HashSet<string> namedAccounts)
		{
			foreach (OrgCarrierNamedAccount account in org.MappedNamedAccounts)
			{
				namedAccounts.Add(account.ONA_ForeignName);
			}

			if (org.ManagementGrouping.PK != org.PK)
			{
				AddNamedAccountsFromOrg(org.ManagementGrouping, namedAccounts);
			}
		}

		#region FetchHints

		public static void AddFetchHintsToLoadAllRateLineItems(this IEnumerable<IRateLine> lines)
		{
			if (lines.Any())
			{
				var factory = lines.First().Factory;
				factory.AddFetchHint(RateLineItemsSchema.Instance, new ZQuery(RateLineItemsSchema.TM_TL, lines.Select(x => x.PK)));
			}
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Interface for building and querying matches between lines and parts.
	/// Should only be used by AutoRatingCalculatorParameters
	/// </summary>
	public interface IAmountByLine
	{
		/// <summary>
		/// Add a match
		/// </summary>
		void AddAmount(MeasureType measureType, IRateLine line, IRateablePart point);

		/// <summary>
		/// Get all the parts that match the line for all measure types.
		/// Matching logic is more complex that just retrieving those matches that were added using AddAmount.
		/// Base calculator lines are also checked.
		/// Base calculators apply to Company Tariff and Cost based calculators only.
		/// </summary>
		/// <returns>not null</returns>
		IEnumerable<PartWithDimensions> GetPartsForLine(IRateLine line);

		/// <summary>
		/// Get all the parts that match the line and measures.
		/// </summary>
		/// <returns>not null if there were matches, null if there were none</returns>
		List<PartWithDimensions> GetPartsForLineMeasure(MeasureType type, IRateLine line);

		ZDecimal GetTotalMeasureValueForLine(IRateLine line, MeasureType measureType);

		ZDecimal GetTotalMeasureValueForLineWithUnit(IRateLine rateLine, MeasureType measureType, string unit);

		OrgSupplierPart ProductFilter { get; }
		ProductAttributesMeasure ProductAttributesFilter { get; }
		LocationMeasure LocationFilter { get; }
		ZString DocketReferenceFilter { get; }
		ZGuid CartageLegPKFilter { get; }
		ZString? ContainerNumberFilter { get; }
		RefContainer ContainerTypeFilter { get; }
	}

	public class AmountByLineTable : IAmountByLine
	{
		public AmountByLineTable(AutoRatingCalculatorParameters @params)
		{
			parameters = @params;
			innerDictionary = new Dictionary<AmountByLineTableKey, List<PartWithDimensions>>();
			types = new List<MeasureType>();
		}

		readonly Dictionary<AmountByLineTableKey, List<PartWithDimensions>> innerDictionary;
		readonly List<MeasureType> types;
		readonly AutoRatingCalculatorParameters parameters;

		#region Add Amount

		public void AddAmount(MeasureType measureType, IRateLine line, IRateablePart part)
		{
			if (!types.Contains(measureType))
			{
				types.Add(measureType);
			}

			IHasPartDimensions hasDimensions = parameters.Criteria.RateableMeasures.GetPartList(measureType);

			var key = new AmountByLineTableKey(measureType, line);
			List<PartWithDimensions> points;
			if (!innerDictionary.TryGetValue(key, out points))
			{
				points = new List<PartWithDimensions>();
				innerDictionary[key] = points;
			}

			points.Add(new PartWithDimensions(hasDimensions, part));
		}

		#endregion

		#region Get Amount

		public List<PartWithDimensions> GetPartsForLineMeasure(MeasureType type, IRateLine line)
		{
			List<PartWithDimensions> parts;
			if (!TryGetValue(type, line, out parts))
			{
				parts = null;
			}
			return parts;
		}

		bool TryGetValue(MeasureType type, IRateLine line, out List<PartWithDimensions> points)
		{
			// Why is this a linear search instead of a dictionary lookup?
			var typeFilteredDictionary = innerDictionary.Where(pair => pair.Key.MeasureType == type);
			var linePointsCollection = typeFilteredDictionary.Select(pair => new KeyValuePair<IRateLine, List<PartWithDimensions>>(pair.Key.RateLine, pair.Value));
			return TryGetValueCore(linePointsCollection, line, out points);
		}

		bool TryGetValueCore(IEnumerable<KeyValuePair<IRateLine, List<PartWithDimensions>>> linePointsCollection, IRateLine line, out List<PartWithDimensions> points)
		{
			// Linear search of the given list for the first element that matches the given line.
			// The list initially comes from the dictionary keys of (MeasureType, Line).
			// This method can be called recursively though with a new list contructed from the base calculator lines.
			// A strange implementation, that needs explaining.
			foreach (var keyValuePair in linePointsCollection)
			{
				var rateLine = keyValuePair.Key;
				if (rateLine.PK == line.PK)
				{
					points = keyValuePair.Value;
					return true;
				}
			}

			// Build a list of the base lines of the given lines, to search that instead.
			var newList = new List<KeyValuePair<IRateLine, List<PartWithDimensions>>>();
			foreach (var keyValuePair in linePointsCollection)
			{
				var baseRateLine = keyValuePair.Key.Calculator.GetBaseCalculator(parameters).Line;
				if (baseRateLine != keyValuePair.Key)
				{
					newList.Add(new KeyValuePair<IRateLine, List<PartWithDimensions>>(baseRateLine, keyValuePair.Value));
				}
			}

			if (newList.Count > 0)
			{
				return TryGetValueCore(newList, line, out points);
			}

			points = null;
			return false;
		}

		public ZDecimal GetTotalMeasureValueForLine(IRateLine line, MeasureType measureType)
		{
			List<PartWithDimensions> points;
			if (TryGetValue(measureType, line, out points))
			{
				var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				return points.Sum(x => info.GetActualValue(x.Part));
			}
			return 0m;
		}

		public ZDecimal GetTotalMeasureValueForLineWithUnit(IRateLine line, MeasureType measureType, string unit)
		{
			if (TryGetValue(measureType, line, out var partWithDimensionsList))
			{
				var partMeasureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				return partWithDimensionsList.Sum(partWithDimensions => partMeasureInfo.GetActualValueWithUnit(partWithDimensions.Part, unit));
			}
			return 0m;
		}

		#endregion

		public IEnumerable<PartWithDimensions> GetPartsForLine(IRateLine line)
		{
			// In most cases a line has one MeasureType and this method is simple.
			// For the case of two MeasureTypes (like the highest rate calculator)
			// the parts for each MeasureType may come from separate part lists or the same part lists.
			// We need to make sure there are no duplicates.
			List<PartWithDimensions> firstMatch = null;
			HashSet<PartWithDimensions> combinedSet = null;
			foreach (var type in types)
			{
				List<PartWithDimensions> partsForType;
				if (TryGetValue(type, line, out partsForType))
				{
					if (firstMatch == null)
					{
						firstMatch = partsForType;
					}
					else
					{
						if (combinedSet == null)
						{
							combinedSet = new HashSet<PartWithDimensions>(firstMatch);
						}

						foreach (var part in partsForType)
						{
							combinedSet.Add(part);
						}
					}
				}
			}

			return combinedSet ?? firstMatch ?? Enumerable.Empty<PartWithDimensions>();
		}

		#region Filter

		/// <summary>
		/// Private constructor to create a new, immutable instance with a filter from a parent, unfiltered instance.
		/// </summary>
		AmountByLineTable(AutoRatingCalculatorParametersWithFilter calcParams, AmountByLineTable parent, AmountByLineTableFilter filterToApply)
		{
			parameters = calcParams;
			types = parent.types;
			filter = filterToApply;

			var parentDictionary = parent.innerDictionary;
			innerDictionary = new Dictionary<AmountByLineTableKey, List<PartWithDimensions>>(parentDictionary.Count);
			foreach (var kv in parentDictionary)
			{
				var newValue = new List<PartWithDimensions>();
				foreach (var point in kv.Value)
				{
					if (filterToApply.Satisfies(kv.Key.MeasureType, point))
					{
						newValue.Add(point);
					}
				}

				innerDictionary.Add(kv.Key, newValue);
			}
		}

		internal AmountByLineTable CreateWithFilter(AutoRatingCalculatorParametersWithFilter calcParams, AmountByLineTableFilter filterToApply)
			=> new AmountByLineTable(calcParams, this, filterToApply);

		public OrgSupplierPart ProductFilter
		{
			get
			{
				var productPK = filter?.ProductPk;
				return !productPK.HasValue ? null : parameters.Factory.Load<OrgSupplierPart>(productPK.Value);
			}
		}

		public ProductAttributesMeasure ProductAttributesFilter
			=> filter?.ProductAttributes ?? ProductAttributesMeasure.Empty;

		public LocationMeasure LocationFilter
			=> filter?.Location ?? LocationMeasure.Empty;

		public ZString DocketReferenceFilter
			=> filter?.DocketReference;

		public ZGuid CartageLegPKFilter
			=> filter?.CartageLegPk ?? ZGuid.Empty;

		public ZString? ContainerNumberFilter
			=> filter?.ContainerNumber;

		public RefContainer ContainerTypeFilter
		{
			get
			{
				var containerTypePK = filter?.ContainerTypePk;
				return !containerTypePK.HasValue ? null : parameters.Factory.Load<RefContainer>(containerTypePK.Value);
			}
		}

		readonly AmountByLineTableFilter filter;

		#endregion
	}

	#region Key

	class AmountByLineTableKey
	{
		public AmountByLineTableKey(MeasureType type, IRateLine line)
		{
			this.type = type;
			this.line = line;
		}

		public override bool Equals(object obj)
		{
			var key2 = obj as AmountByLineTableKey;
			return key2 != null && key2.line.PK == line.PK && key2.type == type;
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() ^ line.PK.GetHashCode();
		}

		public MeasureType MeasureType
		{
			get { return type; }
		}

		readonly MeasureType type;

		public IRateLine RateLine
		{
			get { return line; }
		}

		readonly IRateLine line;
	}

	#endregion

	#region Filter

	public class AmountByLineTableFilter
	{
		public AmountByLineTableFilter(IPartFilter partFilter, PartWithDimensions keyPart)
		{
			Argument.NotNull(partFilter, nameof(partFilter));
			Argument.NotNull(keyPart, nameof(keyPart));

			this.partFilter = partFilter;
			this.keyPart = keyPart;
		}

		readonly IPartFilter partFilter;
		readonly PartWithDimensions keyPart;

		/// <summary>
		/// The filter value for location. LocationMeasure.Empty if not part of the filter.
		/// Not null.
		/// </summary>
		public LocationMeasure Location => partFilter.GetLocation(keyPart);

		/// <summary>
		/// The filter value for docket. string.Empty if not part of the filter.
		/// Not null.
		/// </summary>
		public string DocketReference => partFilter.GetDocketReference(keyPart);

		/// <summary>
		/// The filter value for ProductPK. HasValue will be false if not part of the filter.
		/// </summary>
		public Guid? ProductPk => partFilter.GetProductPk(keyPart);

		/// <summary>
		/// The filter value for ProductAttributes. ProductAttributes.Empty if not part of the filter.
		/// Not null.
		/// </summary>
		public ProductAttributesMeasure ProductAttributes => partFilter.GetProductAttributes(keyPart);

		/// <summary>
		/// The filter value for CartageLegPK. HasValue will be false if not part of the filter.
		/// </summary>
		public Guid? CartageLegPk => partFilter.GetCartageLegPk(keyPart);

		/// <summary>
		/// The filter value for ContainerTypePk. HasValue will be false if not part of the filter.
		/// </summary>
		public Guid? ContainerTypePk => partFilter.GetContainerTypePk(keyPart);

		/// <summary>
		/// The filter value for ContainerNumber. HasValue will be false if not part of the filter.
		/// </summary>
		public ZString? ContainerNumber => partFilter.GetContainerNumber(keyPart);

		/// <summary>
		/// Check if the part satisfies the filter.
		/// 
		/// This may be redundant now, but it's hard to tell.
		/// The new logic is to get the parts that match the line and group them by the filter attributes, e.g., docket.
		/// Then for each group we create AutoRatingCalculatorParametersWithFilter and populate its AmountByLineTable
		/// from the parent one, by keeping only those parts that Satisfies the filter, i.e., has the same docket.
		/// The alternative would be just to populate it directly with the matching parts, which would seem sensible.
		/// However, this more elaborate logic can populate the new AmountByLineTable with unrelated parts that also have the same docket or have no docket.
		/// The question is, do we need those other parts? We know they don't match the MeasureType of the line being rated.
		/// Is there some some logic somewhere that wants parts from other measure types?
		/// If there is, it would be good to know what it does.
		/// </summary>
		public bool Satisfies(MeasureType measureType, PartWithDimensions point)
		{
			return partFilter.Satisfies(keyPart, point);
		}
	}

	#endregion
}


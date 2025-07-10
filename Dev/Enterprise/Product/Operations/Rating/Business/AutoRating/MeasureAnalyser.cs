using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Encapsulates all logic for analysing Measures, so the rest of rating code doesn't care how it is implemented.
	/// </summary>
	public class MeasureAnalyser
	{
		public MeasureAnalyser(RateableMeasureSet measures)
		{
			Argument.NotNull(measures, nameof(measures));
			Measures = measures;
		}

		public RateableMeasureSet Measures { get; }

		public bool HasMeasureType(MeasureType measureType)
			=> Measures.HasMeasureType(measureType);

		/// <summary>
		/// Alternative name for HasMeasureType for compatibility with MeasureInfoCollection.
		/// </summary>
		public bool ContainsKey(MeasureType measureType) => HasMeasureType(measureType);

		#region Time

		/// <summary>
		/// Return time measure TimeInfo, or null if MeasureType.Time has no TimeInfo or there is no MeasureType.Time.
		/// </summary>
		public TimeInfo GetTime() => Measures.Time;

		#endregion Time

		#region Containers

		/// <summary>
		/// Indicates that a list of containers is present.
		/// The list may be empty.
		/// </summary>
		public bool HasContainerMeasure => Measures.HasMeasureType(MeasureType.ContainerCount);

		/// <summary>
		/// Returns the list of containers, or empty list if there is no container list.
		/// </summary>
		public IEnumerable<IRateableContainer> GetAllContainers()
			=> Measures.GetAllContainers();

		/// <summary>
		/// Returns unique container type PKs (RefContainer.PK) in the list of containers, or empty list if there is no container list.
		/// </summary>
		public IEnumerable<ZGuid> GetContainerTypePKs()
			=> Measures.GetContainerTypePKs();

		/// <summary>
		/// Returns unique container commodity codes, or empty list if there is no container list.
		/// </summary>
		public IEnumerable<string> GetContainerUniqueCommodityCodes()
			=> Measures.GetContainerUniqueCommodities();

		/// <summary>
		/// Return all the container spot rates found in the list of containers,
		/// or empty list if there is no container measure, or it has no spot rates.
		/// </summary>
		public List<ContainerSpotRates> GetContainerSpotRates()
			=> Measures.GetContainerSpotRates().ToList();

		public decimal GetContainerCountForContainerType(ZGuid containerTypePK)
			=> Measures.GetContainerCountForContainerType(NullableHelper.ToNullable(containerTypePK));

		/// <summary>
		/// Return the total TEU for the containers with given container type PK.
		/// Returns zero if there is no container list, or the list does not contain the type PK.
		/// </summary>
		public decimal GetTotalTEUForContainerType(ZGuid containerTypePK)
			=> Measures.GetTotalTEUForContainerType(NullableHelper.ToNullable(containerTypePK));

		/// <summary>
		/// If these measures have LCL containers then:
		/// Create and return a new set of measures that contains the LCL container measures (weight, volume and packages) from these measures
		/// and all other measures types found in the given (second set) of measures (except Chargeable and Containers).
		/// Why use a second set of measures? Who knows. Maybe something to do with using the original values instead of any temporary changes.
		///
		/// These measures are modifed to remove the LCL container amounts, splitting it into two.
		/// </summary>
		public RateableMeasureSet SplitOutLCLMeasuresIfPresent(IRateableMeasureSet otherMeasures)
			=> Measures.SplitOutLCLMeasuresIfPresent(otherMeasures);

		#endregion Containers

		#region RateLine Comparing/Similarity

		internal bool EqualMeasures(FastLine line1, FastLine line2)
		{
			return IsEqualEntryMeasures(line1.ParentRateEntry, line2.ParentRateEntry)
				&& IsEqualLineMeasures(line1.Line, line2.Line);
		}

		static bool IsEqualEntryMeasures(IRateEntry x, IRateEntry y)
			=> x.TI_WW_Warehouse == y.TI_WW_Warehouse
			&& x.TI_RC == y.TI_RC
			&& x.TI_RH_NKCommodityCode == y.TI_RH_NKCommodityCode;

		static bool IsEqualLineMeasures(IRateLine x, IRateLine y)
			=> x.TL_OP_ProductNumber == y.TL_OP_ProductNumber
			&& IsEqualPerContainerValues(x, y);

		static bool IsEqualPerContainerValues(IRateLine line1, IRateLine line2)
		{
			if (line1.TL_WeightVolume != RatingConstants.Units.CN ||
				line2.TL_WeightVolume != RatingConstants.Units.CN)
			{
				return true;
			}
			else
			{
				return line1.TL_IsOnPallets.CompareTo(line2.TL_IsOnPallets) == 0
					&& line1.TL_ContainerOwnership.CompareTo(line2.TL_ContainerOwnership) == 0;
			}
		}

		#endregion

		#region Quantities

		/// <summary>
		/// Return actual quantity for given MeasureType.
		/// Returns zero if MeasureType is not present, i.e., HasMeasureType is false.
		/// </summary>
		public ZDecimal GetActual(MeasureType measureType)
			=> Measures.GetActual(measureType);

		/// <summary>
		/// Return unit for MeasureType.
		/// Returns empty string if MeasureType is not present, i.e., HasMeasureType is false.
		/// </summary>
		public ZString GetUnit(MeasureType measureType)
			=> Measures.GetUnit(measureType);

		/// <summary>
		///  Return the MeasureType as a quantity (amount + unit)
		///  Returns an empty quantity if the MeasureType is not present or has no units.
		/// </summary>
		public Quantity GetQuantity(MeasureType measureType)
			=> Measures.GetQuantity(measureType);

		/// <summary>
		/// Get a length measure in given length units, such as KM.
		/// Returns zero if the MeasureType is not present.
		/// If the MeasureType is present, but the targetUnitCode is not a unit found in <see cref="Core.Constants.Length"/>
		/// then throws ArgumentException.
		/// </summary>
		public decimal GetActualLength(MeasureType measureType, string targetUnitCode)
		{
			var quantity = GetQuantity(measureType);
			return Core.Constants.Length.Convert(quantity.Amount, quantity.Unit, targetUnitCode);
		}

		#endregion Quantities

		#region Commodities

		/// <summary>
		/// Returns all the unique commodity codes across all measure types.
		/// Result will not contain a null string.
		/// It may contain an empty string.
		/// </summary>
		public IEnumerable<string> GetCommodities()
			=> Measures.GetDistinctCommodities();

		public IEnumerable<RefContainerInfoParts> GetJobRefContainerInfo()
			=> Measures.GetDistinctRefContainerInfo();

		#endregion Commodities

		#region Packages

		/// <summary>
		/// Return a dictionary of package type to package count if there are MeasureType.Package,
		/// or null if no such MeasureType.
		/// When loadedPackagesOnly == true, the dictionary still contains packages that are not loaded, and their values are set to 0.
		/// </summary>
		public Dictionary<string, decimal> GetGroupedPackages(bool loadedPackagesOnly)
		{
			return Measures.HasMeasureType(MeasureType.Package)
				? Measures.BuildPackageTypeAndCountMap(loadedPackagesOnly)
				: null;
		}

		/// <summary>
		/// Returns unique packages commodity codes, or empty list if there is no packlines list.
		/// </summary>
		public IEnumerable<string> GetPackageUniqueCommodityCodes()
			=> Measures.GetPackageUniqueCommodities();

		#endregion Packages

		#region Errors

		public IEnumerable<string> GetMeasureErrors(MeasureType measureType)
			=> Measures.GetMeasureErrors(measureType).ToList();

		#endregion

		/// <summary>
		/// Begin a block of temporary changes.
		/// Use the methods on the returned object to make changes.
		/// Disposing the return value will undo all changes.
		/// Can be called multiple times to nest blocks of changes.
		/// For consistency, only the most recent undisposed result can be used to make changes.
		/// Using an outer changer while a nested one is undisposed will throw an exception.
		/// </summary>
		public IMeasureChanger BeginTemporaryChanges()
		{
			var outerChanger = currentChanger;
			currentChanger = new MeasureChanger(this, outerChanger);
			return currentChanger;
		}

		MeasureChanger currentChanger;

		#region Temporary changer implementation

		/// <summary>
		/// Interface to make temporary changes.
		/// Disposing will undo all them all.
		/// </summary>
		public interface IMeasureChanger : IDisposable
		{
			/// <summary>
			/// Set a temporary quantity, such as weight or volume,
			/// Can be called multiple times for the same MeasureType.
			/// Throws an exception if the original measures do not contain the given MeasureType.
			/// </summary>
			void SetTemporaryQuantity(MeasureType measureType, Quantity newQuantity);

			/// <summary>
			/// Change the original list of containers to only consist of those with given type and commodity.
			/// All information for containers that match is retained, such as the container count.
			/// Used to run autorating for a single container and commodity for rate chooser.
			/// Throws an exception if the original measures do not contain containers.
			/// </summary>
			void SetTemporaryContainerTypeAndCommodityFilter(ZGuid filterContainerTypePk, string filterContainerQuality, ZString filterCommodityCode);

			/// <summary>
			/// Change the list of containers to only consist of the given container type PKs and commodities.
			/// No ContainerInfo will be added, so the container count for each combination will be zero.
			/// Used for rate chooser to only load rates for given container types, so the container details don't matter.
			/// Throws an exception if the original measures do not contain containers.
			/// </summary>
			/// <param name="getCommodityCodesForContainerTypePk">function to return the desired commodity codes for a given container type PK.
			/// Will be called for each PK in <paramref name="containerTypePks"/></param>
			void SetTemporaryContainerTypes(IEnumerable<ZGuid> containerTypePks, Func<ZGuid, IEnumerable<ZString>> getCommodityCodesForContainerTypePk);
		}

		class MeasureChanger : IMeasureChanger
		{
			internal MeasureChanger(MeasureAnalyser measures, MeasureChanger outerChanger)
			{
				this.measures = measures;
				this.outerChanger = outerChanger;
			}

			readonly MeasureAnalyser measures;
			readonly MeasureChanger outerChanger;
			bool disposedValue;
			readonly Dictionary<MeasureType, IRateablePartList> savedMeasures = new Dictionary<MeasureType, IRateablePartList>();

			public void SetTemporaryQuantity(MeasureType measureType, Quantity newQuantity)
			{
				SaveOriginalInfo(measureType);
				measures.Measures.SetQuantity(measureType, newQuantity.Amount, newQuantity.Unit);
			}

			public void SetTemporaryContainerTypes(IEnumerable<ZGuid> containerTypePks, Func<ZGuid, IEnumerable<ZString>> getCommodityCodesForContainerTypePk)
			{
				SaveOriginalInfo(MeasureType.ContainerCount);
				var rateableMeasures = measures.Measures;
				rateableMeasures.RemoveContainerList();
				rateableMeasures.CreateContainerList(includeCommodity: true);

				foreach (var pk in containerTypePks)
				{
					var commodityCodes = getCommodityCodesForContainerTypePk(pk);
					foreach (var commodityCode in commodityCodes)
					{
						rateableMeasures.AddContainerGroup(pk, commodityCode, new[] { new MeasureInfo.ContainerInfo() });
					}
				}
			}

			public void SetTemporaryContainerTypeAndCommodityFilter(ZGuid filterContainerTypePk, string filterContainerQuality, ZString filterCommodityCode)
			{
				SaveOriginalInfo(MeasureType.ContainerCount);
				measures.Measures.FilterContainers(NullableHelper.ToNullable(filterContainerTypePk), filterContainerQuality, filterCommodityCode);

				SaveOriginalInfo(MeasureType.Weight);
				measures.Measures.FilterContainerParts(MeasureType.Weight, NullableHelper.ToNullable(filterContainerTypePk), filterCommodityCode);

				SaveOriginalInfo(MeasureType.Volume);
				measures.Measures.FilterContainerParts(MeasureType.Volume, NullableHelper.ToNullable(filterContainerTypePk), filterCommodityCode);
			}

			internal IRateablePartList SaveOriginalInfo(MeasureType measureType)
			{
				if (disposedValue)
				{
					throw new InvalidOperationException("Attempt to use a disposed object");
				}

				if (this != measures.currentChanger)
				{
					throw new InvalidOperationException("Attempt to use an outer IMeasureChanger when there is a inner, nested one active. Probably the inner one wasn't disposed.");
				}

				IRateablePartList originalInfo;
				if (!savedMeasures.ContainsKey(measureType))
				{
					originalInfo = measures.Measures.GetPartList(measureType);
					savedMeasures.Add(measureType, originalInfo);
				}
				else
				{
					originalInfo = savedMeasures[measureType];
				}
				return originalInfo;
			}

			void UndoAllTemporaryChanges()
			{
				if (this != measures.currentChanger)
				{
					throw new InvalidOperationException("Attempt to dispose an outer IMeasureChanger when there is a inner, nested one active. Probably the inner one wasn't disposed.");
				}

				foreach (var pair in savedMeasures)
				{
					measures.Measures.RestorePartList(pair.Key, pair.Value);
				}
				measures.currentChanger = outerChanger;
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						UndoAllTemporaryChanges();
					}

					disposedValue = true;
				}
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}

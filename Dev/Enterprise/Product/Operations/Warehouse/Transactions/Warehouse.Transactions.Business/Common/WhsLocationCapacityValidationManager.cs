using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public partial class WhsLocationCapacityValidationManager<Master, Line> where Master : ILocationCapacityMaster<Line> where Line : ILocationCapacityLine
	{
		public WhsLocationCapacityValidationManager(Master parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		#region Parent   

		readonly Master Parent;

		#endregion

		#region ClearCache

		internal void ClearLocationRequiredCapacity()
		{
			locationCapacityRequiredCache = null;
		}

		#endregion

		#region GetLocationAvailableCapacity

		internal LocationWithCapacity GetLocationAvailableCapacity(WhsLocation location)
		{
			return GetValueFromCache(LocationCapacityAvailableCache, location.PK, () => WhsValidationHelper.GetLocationCapacityInfo(location, Parent.PKToExclude));
		}

		#endregion

		#region GetLocationAvailableCapacity

		internal LocationWithCapacity GetLocationRequiredCapacity(WhsLocation location)
		{
			return GetValueFromCache(LocationCapacityRequiredCache, location.PK, () => CalculateCapacityRequired(location, Parent.GetValidationLines(location)));
		}

		#endregion

		#region GetLocationCapacityMessage

		internal void ValidateForLocationCapacity(ZPropertyInfo info, WhsLocation location, LocationWithCapacity availableCapacityInfo, LocationWithCapacity requiredCapacityInfo)
		{
			var locationCapacityMessage = new MessageWithSeverity(MessageWithSeverity.MessageTypes.None, "");
			var message = ZString.Empty;
			var isWeightDefined = location.WLV_MaxWeight != 0;
			var isVolumeDefined = location.WLV_MaxCubic != 0;
			var isQuantityDefined = location.WLV_MaxQuantity != 0;

			if (isQuantityDefined && requiredCapacityInfo.Quantity.HasValue && availableCapacityInfo.Quantity.HasValue && requiredCapacityInfo.Quantity.Value > availableCapacityInfo.Quantity.Value)
			{
				if (WhsEnvironment.IsRF && Parent.ShowRFMessage)
				{
					message = Res.GetString("11CBF9C3-B50F-4FA1-B960-FA3CED4F9F43", "The Maximum Qty({0}) for the destination location will be exceeded. Please select another location.", location.WLV_MaxQuantity.ToZInt());
				}
				else
				{
					message = Parent.GetLocationQuantityExceededErrorMessage(requiredCapacityInfo.Quantity.Value, availableCapacityInfo.Quantity.Value, location);
				}
				locationCapacityMessage = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Error, message);
			}
			else if (isWeightDefined && requiredCapacityInfo.Weight.HasValue && availableCapacityInfo.Weight.HasValue && requiredCapacityInfo.Weight.Value > availableCapacityInfo.Weight.Value)
			{
				message = Parent.GetLocationWeightExceededErrorMessage(requiredCapacityInfo.Weight.Value, availableCapacityInfo.Weight.Value, location);
				locationCapacityMessage = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Warning, message);
			}
			else if (isVolumeDefined && requiredCapacityInfo.Volume.HasValue && availableCapacityInfo.Volume.HasValue && requiredCapacityInfo.Volume.Value > availableCapacityInfo.Volume.Value)
			{
				message = Parent.GetLocationVolumeExceededErrorMessage(requiredCapacityInfo.Volume.Value, availableCapacityInfo.Volume.Value, location);
				locationCapacityMessage = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Warning, message);
			}

			if (!locationCapacityMessage.IsNone)
			{
				if (locationCapacityMessage.MessageType == MessageWithSeverity.MessageTypes.Error)
				{
					info.AddError(locationCapacityMessage.Message);
				}
				else
				{
					info.AddWarning(locationCapacityMessage.Message);
				}
			}
		}

		#endregion

		#region GetValueFromCache

		static LocationWithCapacity GetValueFromCache(Dictionary<ZGuid, LocationWithCapacity> cache, ZGuid key, Func<LocationWithCapacity> getCapacity)
		{
			LocationWithCapacity capacity;
			if (!cache.TryGetValue(key, out capacity))
			{
				capacity = getCapacity();
				cache.Add(key, capacity);
			}
			return capacity;
		}

		#endregion

		#region CalculateCapacityRequired

		LocationWithCapacity CalculateCapacityRequired(WhsLocation location, IEnumerable<Line> lines)
		{
			var quantity = 0m;
			var weight = 0m;
			var volume = 0m;

			var isWeightDefined = location.WLV_MaxWeight != 0;
			var isVolumeDefined = location.WLV_MaxCubic != 0;
			var isQuantityDefined = location.WLV_MaxQuantity != 0;
			var supplierPartsDictionary = IEnumerableExtensions.DistinctBy(lines, l => l.ProductPK).ToDictionary(l => l.ProductPK, l => location.Factory.Load<OrgSupplierPart>(l.ProductPK));

			if (isQuantityDefined)
			{
				quantity = lines.Sum(l => l.GetQuantity(location));
			}

			if (isWeightDefined)
			{
				var weightUnit = location.WLV_MaxWeightUnit;
				weight = lines.Sum(l => l.GetQuantity(location) * GetConvertedWeight(supplierPartsDictionary[l.ProductPK], weightUnit));
			}

			if (isVolumeDefined)
			{
				var volumeUnit = location.WLV_MaxCubicUnit;
				volume = lines.Sum(l => l.GetQuantity(location) * GetConvertedVolume(supplierPartsDictionary[l.ProductPK], volumeUnit));
			}

			return new LocationWithCapacity(location, weight, volume, quantity);
		}

		static ZDecimal GetConvertedWeight(OrgSupplierPart supplierPart, ZString locationWeightUnit)
		{
			return supplierPart?.UnitConverter.Convert(supplierPart.OP_Weight, supplierPart.OP_WeightUQ, locationWeightUnit) ?? 0;
		}

		static ZDecimal GetConvertedVolume(OrgSupplierPart supplierPart, ZString locationVolumeUnit)
		{
			return supplierPart?.UnitConverter.Convert(supplierPart.OP_Cubic, supplierPart.OP_CubicUQ, locationVolumeUnit) ?? 0;
		}

		#endregion

		#region LocationCapacityAvailableCache

		Dictionary<ZGuid, LocationWithCapacity> LocationCapacityAvailableCache
		{
			get { return locationCapacityAvailableCache ?? (locationCapacityAvailableCache = new Dictionary<ZGuid, LocationWithCapacity>()); }
		}
		Dictionary<ZGuid, LocationWithCapacity> locationCapacityAvailableCache;

		#endregion

		#region LocationCapacityRequiredCache

		Dictionary<ZGuid, LocationWithCapacity> LocationCapacityRequiredCache
		{
			get { return locationCapacityRequiredCache ?? (locationCapacityRequiredCache = new Dictionary<ZGuid, LocationWithCapacity>()); }
		}
		Dictionary<ZGuid, LocationWithCapacity> locationCapacityRequiredCache;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Business
{
	public partial class WhsLocationCapacityValidationManager<Master, Line> where Master : ILocationCapacityMaster<Line> where Line : ILocationCapacityLine
	{
		public void ClearLocationAvailableCapacityForTest()
		{
			locationCapacityAvailableCache = null;
		}
	}

	// test this class in DocketLineValidation and StocktakeLineValidation
}
#endif
#endregion

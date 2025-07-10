using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.ZZRefCarrier)]
	public class ZZRefCarrierCombinedCollection : ActiveBusinessObjectCollection<ZZRefCarrierCombined>
	{
		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory, ZString countryCode)
			: this(factory, countryCode, null)
		{
		}

		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory, ZString countryCode, ZQuery filter)
			: this(factory, AddCountryFilter(countryCode, filter))
		{
			this.countryCode = Argument.NotNullOrEmpty(countryCode, "countryCode");
		}

		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public ZZRefCarrierCombinedCollection(BusinessObjectFactory factory, ZString countryCode, ZString carrierType, ZString transportMode)
			: this(factory, GetLoadingQuery(countryCode, carrierType, transportMode))
		{
			this.countryCode = Argument.NotNullOrEmpty(countryCode, "countryCode");
			this.carrierType = carrierType;
		}

		public static ZZRefCarrierCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString countryCode, ZString carrierType, ZString transportMode)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "ZZRefCarrierCombinedCollection_{0}_{1}_{2}", countryCode, carrierType, transportMode);
			return factory.GetCachedValue(key, () =>
			{
				var result = new ZZRefCarrierCombinedCollection(factory, countryCode, carrierType, transportMode);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.Description, "Property", ZString.Empty, true));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.CarrierType, "Property", carrierType, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.TransportMode, "Property", transportMode, false));
				return result;
			});
		}

		protected override void SetDefaultsForNewElementCore(ZZRefCarrierCombined newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZ4_CountryOrGrouping = countryCode;
			if (IsSetDefaultCarrierType && !carrierType.IsEmpty)
			{
				var carrierTypeAttribute = newElement.Attributes.AddNew();
				carrierTypeAttribute.ZZG_Name = carrierType;
				carrierTypeAttribute.ZZG_Value = carrierType;
			}
		}

		protected readonly ZString countryCode;

		protected readonly ZString carrierType;

		ZBool IsSetDefaultCarrierType => RefCarrierHelper.GetConfig(countryCode)?.IsSetDefaultCarrierType ?? false;

		static ZQuery AddCountryFilter(ZString countryCode, ZQuery filter)
		{
			var result = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, countryCode);
			if (filter != null)
			{
				filter.AddToFilter(result);
			}
			return result;
		}

		public static ZQuery GetLoadingQuery(ZString countryCode, ZString carrierType, ZString transportMode)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCarrierCombined));
			AddCountryFilter(countryCode, result);
			if (!carrierType.IsEmpty)
			{
				var carrierTypeFilter = new ZDBOnlySubQuery(typeof(ZZRefCarrierAttributeCombined), ZZRefCarrierAttributeCombinedSchema.ZZG_ZZ4_CarrierCode);
				carrierTypeFilter.AddToFilter(ZZRefCarrierAttributeCombinedSchema.ZZG_Name, SQLComparisonOperator.Equal, carrierType);
				result.AddSubQuery(carrierTypeFilter, JoinCondition.And);
			}
			if (!transportMode.IsEmpty)
			{
				var subFilter = GetTransportModeSubQuery(transportMode);
				if (subFilter != null)
				{
					result.AddSubQuery(subFilter, JoinCondition.And);
				}
			}
			return result;
		}

		public static ZQuery GetCarrierTypeFilter(ZString carrierType)
		{
			var carrierTypeFilter = new ZDBOnlyQuery(typeof(ZZRefCarrierAttributeCombined));
			carrierTypeFilter.AddToFilter(ZZRefCarrierAttributeCombinedSchema.ZZG_Name, SQLComparisonOperator.Equal, carrierType);
			return carrierTypeFilter;
		}

		public static ZQuery GetTransportModeFilter(ZString transportMode)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCarrierCombined));
			var subFilter = GetTransportModeSubQuery(transportMode);
			if (subFilter != null)
			{
				result.AddSubQuery(subFilter, JoinCondition.And);
			}
			return result;
		}

		static ZDBOnlySubQuery GetTransportModeSubQuery(ZString transportMode)
		{
			ZDBOnlySubQuery transportModeFilter = null;
			if (!transportMode.IsEmpty)
			{
				transportModeFilter = new ZDBOnlySubQuery(typeof(ZZRefCarrierCombined), ZZRefCarrierCombinedSchema.PK);
				var column = ZZRefCarrierCombined.GetTransportModePropertySchemaColumn(transportMode);
				if (column != null)
				{
					transportModeFilter.AddToFilter(column, true);
					var attrTransportModeFilter = new ZDBOnlySubQuery(typeof(ZZRefCarrierAttributeCombined), ZZRefCarrierAttributeCombinedSchema.ZZG_ZZ4_CarrierCode);
					attrTransportModeFilter.AddToFilter(ZZRefCarrierAttributeCombinedSchema.ZZG_Name, SQLComparisonOperator.Equal, transportMode);
					transportModeFilter.AddSubQuery(attrTransportModeFilter, JoinCondition.Or);
				}
			}
			return transportModeFilter;
		}
	}
}

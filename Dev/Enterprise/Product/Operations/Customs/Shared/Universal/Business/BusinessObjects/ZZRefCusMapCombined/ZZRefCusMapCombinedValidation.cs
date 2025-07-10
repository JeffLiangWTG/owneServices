
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusMapCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCusMapCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using ZArchitecture.Schema;

	public class ZZRefCusMapCombinedValidation : AutoZZRefCusMapCombinedValidation
	{
		public ZZRefCusMapCombinedValidation(AutoZZRefCusMapCombined parent) : base(parent)
		{
		}

		protected new ZZRefCusMapCombined Parent
		{
			get { return (ZZRefCusMapCombined)base.Parent; }
		}

		protected override void CheckZZM_EndDateIsValidZDateTimeRange()
		{
			//range validation not desired
		}

		protected override void CheckZZM_StartDateIsValidZDateTimeRange()
		{
			//range validation not desired
		}

		protected override void CheckZZM_ZZP_NKMapType()
		{
			base.CheckZZM_ZZP_NKMapType();
			if (!Parent.ZZM_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZM_ZZP_NKMapTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZM_ZZP_NKMapTypeInfo, Parent.Lookups.MapTypesList);
				ValidateZZM_ZZZ_NKDataGrouping();
			}
		}

		protected override void CheckZZM_ZZZ_NKDataGrouping()
		{
			base.CheckZZM_ZZZ_NKDataGrouping();
			if (!Parent.ZZM_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZM_ZZZ_NKDataGroupingInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZM_ZZZ_NKDataGroupingInfo, Parent.Lookups.CountryOrGroupingList);
				EnsureThatMapTypeAndCountryOrGroupExistsInZZDatabase();
				ValidateZZM_CustomsValue();
				ValidateZZM_CW1orCommercialValue();
			}
		}

		void EnsureThatMapTypeAndCountryOrGroupExistsInZZDatabase()
		{
			if (!Parent.ZZM_ZZP_NKMapType.IsEmpty && !Parent.ZZM_ZZZ_NKDataGrouping.IsEmpty && !Parent.ZZM_ZZZ_NKDataGroupingInfo.HasErrors())
			{
				ValidateZZM_ZZP_NKMapType();
				if (!Parent.ZZM_ZZP_NKMapTypeInfo.HasErrors())
				{
					var query = new ZQuery(RefCusMapSchema.ZZM_ZZP_NKMapType, Parent.ZZM_ZZP_NKMapType);
					query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, Parent.ZZM_ZZZ_NKDataGrouping);
					if (Parent.Factory.LoadTop1<RefCusMap>(query) == null)
					{
						Parent.ZZM_ZZZ_NKDataGroupingInfo.AddError(MapTypeIsNotValidForCountryOrGrouping(Parent.ZZM_ZZP_NKMapType, Parent.ZZM_ZZZ_NKDataGrouping));
					}
				}
			}
		}

		internal static string MapTypeIsNotValidForCountryOrGrouping(ZString mapType, ZString country)
		{
			return Res.GetString("{3304830C-FD20-4C9F-A77C-0BB29065536A}", "This Country/Region or Grouping '{0}' does not support Mapping Type '{1}'.", country, mapType);
		}

		protected override void CheckZZM_CustomsValue()
		{
			base.CheckZZM_CustomsValue();
			if (!Parent.ZZM_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZM_CustomsValueInfo);
				ValidateDuplicateCustomsValue();
			}
		}

		protected override void CheckZZM_CW1orCommercialValue()
		{
			base.CheckZZM_CW1orCommercialValue();
			if (!Parent.ZZM_IsSystem)
			{
				ValidateDuplicateCW1Value();
			}
		}

		protected override void CheckZZM_CW1orCommercialValueIsNotEmpty()
		{
			if (!Parent.ZZM_IsSystem)
			{
				base.CheckZZM_CW1orCommercialValueIsNotEmpty();
			}
		}

		void ValidateDuplicateCustomsValue()
		{
			if (!Parent.ZZM_ZZZ_NKDataGroupingInfo.HasErrors()
				&& !Parent.ZZM_ZZP_NKMapTypeInfo.HasErrors()
				&& !Parent.ZZM_CustomsValueInfo.HasErrors())
			{
				var mapType = Parent.CusMapType;
				if (mapType != null && (mapType.ZZP_Direction == MapDirectionList.Codes.INW || mapType.ZZP_Direction == MapDirectionList.Codes.BTH))
				{
					var query = GetDuplicateValueQuery(ZZRefCusMapCombinedSchema.ZZM_CustomsValue, Parent.ZZM_CustomsValue);
					if (Parent.Factory.LoadTop1<ZZRefCusMapCombined>(query) != null)
					{
						Parent.ZZM_CustomsValueInfo.AddError(DuplicateCustomsValueNotAllowed(Parent.ZZM_ZZP_NKMapType, Parent.ZZM_CustomsValue));
					}
				}
			}
		}

		void ValidateDuplicateCW1Value()
		{
			if (!Parent.ZZM_ZZZ_NKDataGroupingInfo.HasErrors()
				&& !Parent.ZZM_ZZP_NKMapTypeInfo.HasErrors()
				&& !Parent.ZZM_CW1orCommercialValueInfo.HasErrors())
			{
				var mapType = Parent.CusMapType;
				if (mapType != null && (mapType.ZZP_Direction == MapDirectionList.Codes.OUT || mapType.ZZP_Direction == MapDirectionList.Codes.BTH))
				{
					var query = GetDuplicateValueQuery(ZZRefCusMapCombinedSchema.ZZM_CW1orCommercialValue, Parent.ZZM_CW1orCommercialValue);
					if (Parent.Factory.LoadTop1<ZZRefCusMapCombined>(query) != null)
					{
						Parent.ZZM_CW1orCommercialValueInfo.AddError(DuplicateCW1ValueNotAllowed(Parent.ZZM_ZZP_NKMapType, Parent.ZZM_CW1orCommercialValue));
					}
				}
			}
		}

		ZQuery GetDuplicateValueQuery(SchemaStringColumn valueColumn, ZString value)
		{
			var query = new ZQuery(ZZRefCusMapCombinedSchema.ZZM_IsSystem, false);
			query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_ZZP_NKMapType, Parent.ZZM_ZZP_NKMapType);
			query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_ZZZ_NKDataGrouping, Parent.ZZM_ZZZ_NKDataGrouping);
			query.AddToFilter(ZZRefCusMapCombinedSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(valueColumn, value);

			return query;
		}

		internal static string DuplicateCustomsValueNotAllowed(string mapType, string value)
		{
			return Res.GetString("{3878CEAD-6274-4B1E-A154-0831C1C017D9}", "Duplicate Customs Value '{0}' is not allowed for Mapping Type '{1}'.", value, mapType);
		}

		internal static string DuplicateCW1ValueNotAllowed(string mapType, string value)
		{
			return Res.GetString("{6002D938-B906-4ED7-AE47-C97A765C2798}", "Duplicate CW1 or Commercial Value '{0}' is not allowed for Mapping Type '{1}'.", value, mapType);
		}
	}
}

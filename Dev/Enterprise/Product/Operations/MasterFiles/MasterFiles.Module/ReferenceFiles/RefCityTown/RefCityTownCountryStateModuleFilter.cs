using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.ReferenceFiles.RefCityTown
{
	public class RefCityTownCountryStateModuleFilter : CountryStateModuleFilter<ZString>
	{
		#region Construction

		protected RefCityTownCountryStateModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.RW_Code;
		}

		public RefCityTownCountryStateModuleFilter(ZString description, SchemaColumn countryColumn, SchemaColumn stateColumn)
			: base(description, countryColumn, stateColumn)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.RW_Code;
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new RefCityTownCountryStateModuleFilter(category, parentCollection);
		}

		#endregion

		#region Serialization

		protected override void DeserializePropertiesFromXmlForProperty2(XmlReader reader)
		{
			if (reader.Name == "Property2")
			{
				Property2 = reader.ReadElementString("Property2");
			}
		}

		#endregion

		#region CopyPersistantValuesFromFilterForProperty2

		protected override void CopyPersistantValuesFromFilterForProperty2(ModuleFilter filterToCopyFrom)
		{
			ModuleCodeFilter filter = (ModuleCodeFilter)filterToCopyFrom;
			Property2 = filter.Property2;
		}

		#endregion

		#region GetNewValidation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new RefCityTownCountryStateFilterValidation(this);
		}

		public class RefCityTownCountryStateFilterValidation : CountryStateFilterValidation<ZString>
		{
			public RefCityTownCountryStateFilterValidation(CountryStateModuleFilter<ZString> parent)
				: base(parent)
			{
			}

			protected override void ErrorIfInvalid(ZPropertyInfo propertyInfo, IList states)
			{
				ErrorIfInvalidCode(propertyInfo, states);
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			base.FillWithValidTestFilterValueCore();
			Property2 = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}

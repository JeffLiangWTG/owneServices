using System;
using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO
{
	public class RefUNLOCOCountryStateModuleFilter : CountryStateModuleFilter<ZGuid>
	{
		#region Construction

		protected RefUNLOCOCountryStateModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.PK;
		}

		public RefUNLOCOCountryStateModuleFilter(ZString description, SchemaColumn countryCloumn, SchemaColumn stateColumn)
			: base(description, countryCloumn, stateColumn)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.PK;
		}

		#endregion

		protected override void ApplyStatesFilterCore(RefCountryStatesCollection states, ZString countryCode)
		{
			states.AdditionalFilter = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
		}

		#region GetNewCommonModuleFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new RefUNLOCOCountryStateModuleFilter(category, parentCollection);
		}

		#endregion

		#region Serialization

		protected override void DeserializePropertiesFromXmlForProperty2(XmlReader reader)
		{
			if (reader.Name == "Property2")
			{
				Property2 = new Guid(reader.ReadElementString("Property2"));
			}
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilterForProperty2(ModuleFilter filterToCopyFrom)
		{
			ModuleCodeFilter filter = (ModuleCodeFilter)filterToCopyFrom;
			Property2 = new Guid(filter.Property2);
		}

		#endregion

		#region GetNewValidation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new RefUNLOCOCountryStateFilterValidation(this);
		}

		public class RefUNLOCOCountryStateFilterValidation : CountryStateFilterValidation<ZGuid>
		{
			public RefUNLOCOCountryStateFilterValidation(CountryStateModuleFilter<ZGuid> parent)
				: base(parent)
			{
			}
			protected override void ErrorIfInvalid(ZPropertyInfo propertyInfo, IList states)
			{
				ErrorIfInvalidPK(propertyInfo, states);
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			base.FillWithValidTestFilterValueCore();
			Property2 = ZGuid.NewZGuid();
		}

#endif
		#endregion
	}
}

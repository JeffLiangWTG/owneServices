using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCountryStatesFilterBusinessObject : FilterStripBusinessObject
	{
		public RefCountryStatesFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", RefCountryStatesSchema.RW_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCountryStatesFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", RefCountryStatesSchema.RW_Description, typeof(RefCountryStates), ResString.GetMultilingualString("MasterFiles|RefCountryStatesFilter|Description", "Description"));
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Country", RefCountryStatesSchema.RW_RN_NKCountryCode, ModuleIDs.RefCountry, Countries);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCountryStatesFilter|Country", "Country/Region");
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		#endregion

		#endregion

		#region Lookups

		#region Countries

		RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}

				return fCountries;
			}
		}

		RefCountryCollection fCountries;

		#endregion

		#endregion

		public override void SetAdditionalFilterDefaults(CargoWise.Types.ZString code, CargoWise.EntityFramework.IBusinessObjectCollection bizObjCollection)
		{
			base.SetAdditionalFilterDefaults(code, bizObjCollection);

			if (code.IsEmpty)
			{
				SetInitialCodeForSearch("", RefCountryStates.Schema.RW_Code);
				SetInitialCodeForSearch("", RefCountryStates.Schema.RW_RN_NKCountryCode);
			}
		}
	}
}

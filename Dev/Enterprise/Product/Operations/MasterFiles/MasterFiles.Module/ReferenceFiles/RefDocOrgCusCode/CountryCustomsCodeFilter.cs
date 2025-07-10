using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class CountryCustomsCodeFilter : ModuleCodeFilter
	{
		public CountryCustomsCodeFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection findBoxList, IList dropEditList, UpdateDependentList updateDropListDelegate, GetDefaultDependentList getDefaultDropListDelegate)
			: base(description, queryDelegate, findBoxList, dropEditList)
		{
			QueryDelegate = queryDelegate;
			this.updateDropListDelegate = updateDropListDelegate;
			this.getDefaultDropListDelegate = getDefaultDropListDelegate;

			EnsureFindBoxListIsCountryCollection(findBoxList);
		}

		CountryCustomsCodeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		readonly GetDefaultDependentList getDefaultDropListDelegate;
		readonly UpdateDependentList updateDropListDelegate;

		public override ZString Property1
		{
			get => base.Property1;
			set
			{
				base.Property1 = value;
				updateDropListDelegate?.Invoke(List2, value);
			}
		}

		protected override FilterCategory DefaultCategory => FilterCategories.Locations;

		protected override ModuleFilter ShallowCloneCore()
			=> new CountryCustomsCodeFilter(Description, QueryDelegate as GetCodeQuery, (IBusinessObjectCollection)List1, getDefaultDropListDelegate(), updateDropListDelegate, getDefaultDropListDelegate);

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			=> new CountryCustomsCodeFilter(category, parentCollection);

		void EnsureFindBoxListIsCountryCollection(IBusinessObjectCollection countryList)
		{
			Argument.NotNull(countryList, nameof(countryList));

			if (!countryList.GetType().IsAssignableFrom(typeof(RefCountryCollection)))
			{
				throw new ArgumentException("countryList is not a RefCountryCollection.");
			}
		}
	}
}

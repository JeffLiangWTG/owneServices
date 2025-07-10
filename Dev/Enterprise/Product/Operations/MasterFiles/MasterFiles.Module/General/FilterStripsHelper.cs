using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public abstract class FilterStripsHelper : IFilterStripsHelper
	{
		protected FilterStripsHelper()
		{
		}

		protected FilterStripsHelper(Type businessObjectType, BusinessObjectFactory factory) : this()
		{
			BusinessObjectType = businessObjectType;
			Factory = factory;
		}

		public Type BusinessObjectType { get; private set; }

		protected BusinessObjectFactory Factory { get; private set; }

		public abstract bool IsApplicableToBizOTypeIsAssignableFrom();
		protected abstract void AddFilterStrips(ModuleFilterCollection filters);

		protected virtual void AddFilterStripsForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
		}

		public virtual void Initialise(Type businessObjectType, BusinessObjectFactory factory)
		{
			if (BusinessObjectType == null)
			{
				BusinessObjectType = businessObjectType;
			}

			if (Factory == null)
			{
				Factory = factory;
			}
		}

		public void AddFilterStrips(IModuleFilterCollection filters)
		{
			var filterCollection = (ModuleFilterCollection)filters;
			AddFilterStrips(filterCollection);
		}

		public void AddFilterStripsForIndexSearch(IModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			var filterCollection = (ModuleFilterCollection)filters;
			AddFilterStripsForIndexSearch(filterCollection, defaultHiddenIndexSearchFields);
		}

		public void CheckApplicableToBizOTypeIsAssignableFrom()
		{
			if (!IsApplicableToBizOTypeIsAssignableFrom())
			{
				throw new InvalidOperationException(BusinessObjectType.FullName + " does not implement a compatible interface and cannot be used with this helper.");
			}
		}

		public virtual bool CanAddFilters()
		{
			return IsApplicableToBizOTypeIsAssignableFrom();
		}

#if DEBUG
		public abstract string GetAutomaticFilterTestCaseName_ForObjectFactory();
#endif
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Integration
{
	public interface IFilterStripsHelper
	{
		Type BusinessObjectType { get; }

		void Initialise(Type businessObjectType, BusinessObjectFactory factory);
		void AddFilterStrips(IModuleFilterCollection filters);
		void AddFilterStripsForIndexSearch(IModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields);
		bool IsApplicableToBizOTypeIsAssignableFrom();
		bool CanAddFilters();

#if DEBUG
		string GetAutomaticFilterTestCaseName_ForObjectFactory();
#endif
	}
}

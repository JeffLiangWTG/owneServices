using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CountryStatesGlbHolidayModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CountryStatesGlbHoliday; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new CountryStatesGlbHolidayController();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CountryStatesGlbHolidayFilterControl(GridCollection, (CountryStatesGlbHolidayFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CountryStatesGlbHolidayBizoCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CountryStatesGlbHolidayFilterBusinessObject();
		}
		public override bool AllowUniversalCopy => false;
		public override bool SupportsWorkflow => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false; protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			// Not needed for a non-persistent bizo.
			return true;
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CountryStatesGlbHoliday; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			query.AddToFilter(GlbHolidaySchema.GH_RecurrType, CountryStatesGlbHolidayBizo.RecurrTypeDate);
			return PerformSearchResult.Success(factory, query, CountryStatesGlbHolidayBizoCollection.LoadCountryStates(factory, query), permitActiveCollectionUpdates: false);
		}
	}
}

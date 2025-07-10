#define CODE_ANALYSIS
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Module for Statements
	/// </summary>
	public class CusPermitModule : ZFilterGridModule
	{
		public CusPermitModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.Permits; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		public override bool AllowView
		{
			get { return true; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusPermitFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var collection = new CusPermitHeaderCollection(Factory);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitFilterStripBusinessObject.Schema.EndDate, "PropertySearch", new ZString(ModuleDateFilter.Future)));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitFilterStripBusinessObject.Schema.StartDate, "PropertySearch", new ZString(ModuleDateFilter.Past)));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitFilterStripBusinessObject.Schema.Country, "Property", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false));
			return collection;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			var filterBizObj = GetFilterStripBusinessObject();

			var permitCollection = GridCollection as CusPermitHeaderCollection;
			if (permitCollection != null)
			{
				permitCollection.MatchesFilterDelegate = filterBizObj.MatchesFilter;

				var findBoxCollection = permitCollection as PermitFindBoxCollection;
				if (findBoxCollection != null)
				{
					findBoxCollection.ShouldIgnoreAdditionalFilter = () => ModuleDecisionProvider.ShouldIgnoreAdditionalFilter;
				}
			}

			return filterBizObj;
		}

		protected virtual CusPermitFilterStripBusinessObject GetFilterStripBusinessObject()
		{
			return new CusPermitFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Permits);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportBroker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Permits; }
		}

		public override bool SupportsWorkflow
		{
			get { return false; }
		}

		protected override bool ShouldLoadFilterBusinessObjectDefaults
		{
			get { return true; }
		}
	}
}

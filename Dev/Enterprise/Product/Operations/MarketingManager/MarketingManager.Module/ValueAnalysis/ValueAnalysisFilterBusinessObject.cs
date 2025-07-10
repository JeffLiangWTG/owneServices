using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.Module
{
	public class ValueAnalysisFilterBusinessObject : FilterStripBusinessObject
	{
		public string ModuleContext
		{
			get
			{
				var orgModule = ParentModule as ValueAnalysisOrganisationModule;
				if (orgModule != null)
				{
					return orgModule.Context;
				}

				var oppModule = ParentModule as ValueAnalysisOpportunityModule;

				return oppModule != null ? oppModule.Context : null;
			}
			set
			{
				var orgModule = ParentModule as ValueAnalysisOrganisationModule;
				if (orgModule != null)
				{
					orgModule.Context = value;
				}
				else
				{
					var oppModule = ParentModule as ValueAnalysisOpportunityModule;
					if (oppModule != null)
					{
						oppModule.Context = value;
					}
				}
			}
		}

		string ProductCode => ValueAnalysisModuleHelper.GetProductCode(ParentModule?.ID);

		public override bool NullOutParentModuleOnDispose { get; set; }

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var result = base.GetCustomFilterStripsHelpersCore();
			result.Add(new ValueAnalysisFilterStripsHelper(ProductCode, ModuleContext, Factory));
			return result;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}

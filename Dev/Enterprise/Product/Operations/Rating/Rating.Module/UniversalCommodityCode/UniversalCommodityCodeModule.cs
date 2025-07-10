using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class UniversalCommodityCodeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.UniversalCommodityCode;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool AllowDelete => false;
		public override bool AllowEdit => false;
		public override bool AllowNew => false;
		public override bool AllowUniversalCopy => false;
		public override bool AllowView => false;
		public override bool SupportsWorkflow => false;
		protected override bool IsModuleAllowAsync => false;
		protected override bool ShowRecentItemsCore() => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return PerformSearchResult.CustomGridLoad(factory, query);
		}

		protected override void OnCustomGridLoad(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			var list = (UniversalCommodityCodeBizoCollection)collection;
			list.RemoveAndDeleteAll();

			var codeDescriptions = RefCommodityCodeLookups.GetUniversalCommodityGroupMapUnfilteredList().Cast<ICodeDescription>().ToArray();

			var commodityCodeQuery = new ZQuery(RefCommodityCodeSchema.RH_UniversalCommodityGroup, codeDescriptions.Select(x => x.Code)).AddToFilter(RefCommodityCodeSchema.RH_IsActive, true);
			var matchingCommodities = list.Factory.Load<RefCommodityCode>(commodityCodeQuery).GroupBy(x => x.RH_UniversalCommodityGroup).ToDictionary(x => x.Key, x => x.ToList());

			UniversalCommodityCodeBizo newBizo = null;
			foreach (var codeDescription in codeDescriptions)
			{
				if (!matchingCommodities.ContainsKey(codeDescription.Code))
				{
					if (newBizo == null)
					{
						newBizo = list.AddNew();
					}

					newBizo.RH_UniversalCommodityGroup = codeDescription.Code;
					newBizo.RH_UniversalCommodityGroupDescription = codeDescription.Description;
					newBizo.RH_Code = ZString.Empty;
					newBizo.RH_Description = ZString.Empty;

					if (newBizo.MatchesFilter(searchResult.Query))
					{
						newBizo = null;
					}
				}
				else
				{
					var commodities = matchingCommodities[codeDescription.Code];
					foreach (var commodity in commodities)
					{
						if (newBizo == null)
						{
							newBizo = list.AddNew();
						}

						newBizo.RH_UniversalCommodityGroup = codeDescription.Code;
						newBizo.RH_UniversalCommodityGroupDescription = codeDescription.Description;
						newBizo.RH_Code = commodity.RH_Code;
						newBizo.RH_Description = commodity.RH_DescriptionMultilingual;

						if (newBizo.MatchesFilter(searchResult.Query))
						{
							newBizo = null;
						}
					}
				}
			}

			if (newBizo != null)
			{
				list.RemoveAndDelete(newBizo);
			}
		}

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			// Not needed for a non-persistent bizo.
			return true;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UniversalCommodityCodeFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UniversalCommodityCodeFilterControl(GridCollection, (UniversalCommodityCodeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UniversalCommodityCodeBizoCollection(Factory);
		}
	}
}

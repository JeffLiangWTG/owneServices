using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module
{
	public class ManifestBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore() => new ModuleFilterCollection();

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				filter.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.Taiwan);
				filter.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration);
				return filter;
			}
		}
	}
}

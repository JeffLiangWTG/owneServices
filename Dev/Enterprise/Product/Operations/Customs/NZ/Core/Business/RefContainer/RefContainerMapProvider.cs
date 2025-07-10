using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class RefContainerMapProvider : IRefContainerMapProvider
	{
		public UsageRequirement IsUsageNeeded => UsageRequirement.NotRequire;

		public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory)
		{
			return new CodeDescriptionPairList();
		}

		public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage)
		{
			return factory.GetCachedValue("Enterprise.Customs.NZ.Business.RefContainerMapProvider.CodeList", () =>
			{
				var result = new UntranslatableCodeDescriptionPairList("NZ specific descriptions");
				result.AddRangeOverwriteIfExists(new NZContainerCodeList());
				return result;
			});
		}

		public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType) => ZString.Empty;
	}
}

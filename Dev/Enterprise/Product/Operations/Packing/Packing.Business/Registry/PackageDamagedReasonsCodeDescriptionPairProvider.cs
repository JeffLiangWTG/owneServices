using CargoWise.EntityFramework;
using Enterprise.Integration.Packing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public class PackageDamagedReasonsCodeDescriptionPairProvider : IPackageDamagedReasonsCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			return factory.GetCachedValue("PackageDamagedReason", () => PackingRegistry.Instance.DamagedReasons.Value.GetCodeDescriptionPairList());
		}
	}
}

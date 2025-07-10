using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationServiceLevelCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationServiceLevelCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (fServiceLevelList == null)
			{
				fServiceLevelList = new CodeDescriptionPairList();
				FilterBizo.Lookups.ServiceLevelList.ToList().ForEach(x => fServiceLevelList.AddPairIfNotExist(x.RS_Code, x.RS_DescriptionMultilingual));
				fServiceLevelList.SortByDescription();
			}
			return fServiceLevelList;
		}
		CodeDescriptionPairList fServiceLevelList;

		#endregion
	}
}

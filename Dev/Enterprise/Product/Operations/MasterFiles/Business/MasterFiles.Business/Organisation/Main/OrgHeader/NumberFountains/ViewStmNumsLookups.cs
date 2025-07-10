using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ViewStmNumsLookups : AutoViewStmNumsLookups
	{
		public ViewStmNumsLookups(AutoViewStmNums parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList TypeList => new CodeDescriptionPairList();
	}
}

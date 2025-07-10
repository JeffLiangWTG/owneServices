using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public interface IMultiPersonMergerCore
	{
		MultiPersonMergerResult Merge(GlbPerson retained, GlbPerson dissolved);
	}
}

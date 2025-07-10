using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IACECensusWarningQuery : IQueryMessageAttachee
	{
		ZString DistrictPortOfEntry { get; }
		GlbBranch Branch { get; }
	}
}

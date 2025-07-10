using CargoWise.EntityFramework;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IDpsSourceWithParties
	{
		BusinessObject SourceBizO { get; }
	}
}

using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IHrlStaffPolicy
	{
		ZGuid PK { get; }
		ZDateTimeOffset LLS_AutoEffectiveEndDate { get; set; }
		ZDateTimeOffset LLS_EffectiveDate { get; set; }
		ZGuid LLS_GS_Staff { get; set; }
		ZGuid LLS_LLP_Policy { get; set; }
	}
}

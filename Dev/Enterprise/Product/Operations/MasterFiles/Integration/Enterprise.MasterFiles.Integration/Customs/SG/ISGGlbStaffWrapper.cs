namespace Enterprise.MasterFiles.Integration.Customs.SG
{
	public interface ISGGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPassword Tradenetv4Password { get; }

		IGlbExternalPassword SGNationalTradePlatformPassword { get; }
		IGlbExternalPassword AccessPassword { get; }
	}
}

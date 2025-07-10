using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.Customs.GB
{
	public interface IGlbExternalPassword_GB : IGlbExternalPassword
	{
		ZBool IsTokenForAll { get; set; }
		ZBool IsTokenForCDS { get; set; }
		ZBool IsTokenForEMCS { get; set; }
		ZBool IsTokenForGVMS { get; set; }
		ZBool IsTokenForNCTS { get; set; }
	}
}

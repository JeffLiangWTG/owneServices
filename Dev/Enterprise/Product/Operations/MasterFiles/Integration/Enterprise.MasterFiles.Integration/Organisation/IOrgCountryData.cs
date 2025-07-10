using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgCountryData
	{
		ZGuid PK { get; }

		ZString OV_EXApprovedOrMajorExporter { get; set; }
		ZString OV_EXApprovalNumber { get; set; }
		ZString OV_RN_NKClientCountryRelation { get; set; }
		ZString OV_RN_NKIssuingAuthorityCountry { get; set; }
	}
}

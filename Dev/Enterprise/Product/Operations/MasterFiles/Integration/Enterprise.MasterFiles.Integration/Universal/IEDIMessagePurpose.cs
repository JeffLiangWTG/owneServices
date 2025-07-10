using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IEDIMessagePurpose : IBusiness, IAuditDetails
	{
		ZString EMP_Code { get; set; }
		ZString EMP_Description { get; set; }
		ZGuid EMP_ECF_Filter { get; }
		MultilingualString EMP_DescriptionMultilingual { get; }
		ZBool EMP_DisableOrgProxyRecipientOverride { get; set; }
	}
}

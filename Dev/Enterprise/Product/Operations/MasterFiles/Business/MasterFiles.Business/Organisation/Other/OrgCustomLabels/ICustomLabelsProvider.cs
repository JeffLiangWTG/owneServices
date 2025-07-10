using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ICustomLabelsProvider
	{
		CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory);
		ICustomLabelsConfigOrgProvider ConfigOrgProvider { get; }
	}
}

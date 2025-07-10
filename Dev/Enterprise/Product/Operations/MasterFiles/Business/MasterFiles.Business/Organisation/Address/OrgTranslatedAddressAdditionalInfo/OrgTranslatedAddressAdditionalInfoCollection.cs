using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTranslatedAddressAdditionalInfoCollection : ActiveBusinessObjectCollection<OrgTranslatedAddressAdditionalInfo>
	{
		public OrgTranslatedAddressAdditionalInfoCollection(OrgAddressAdditionalInfo addressAdditionalInfo) : base(addressAdditionalInfo)
		{
		}
	}
}

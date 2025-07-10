using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAccountMetaDataCollection : ActiveBusinessObjectCollection<OrgCarrierAccountMetaData>
	{
		public OrgCarrierAccountMetaDataCollection(OrgCarrierAccount carrierAccount)
			: base(carrierAccount.Factory, carrierAccount, null, OrgCarrierAccountMetaDataSchema.OAM_OAN_CarrierAccount)
		{
		}

		public OrgCarrierAccountMetaDataCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

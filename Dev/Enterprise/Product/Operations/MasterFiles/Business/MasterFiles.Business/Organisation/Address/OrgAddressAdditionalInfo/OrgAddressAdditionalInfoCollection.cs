using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressAdditionalInfoCollection : ActiveBusinessObjectCollection<OrgAddressAdditionalInfo>
	{
		public OrgAddressAdditionalInfoCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgAddressAdditionalInfoCollection(OrgAddress orgAddress) : base(orgAddress.Factory, orgAddress, new ZQuery(), OrgAddressAdditionalInfoSchema.OAI_OA_Address)
		{
		}

		public CodeDescriptionPairList GetAsCodeDescriptionPair()
		{
			var pairLists = new CodeDescriptionPairList();

			foreach (var additionalInfo in this)
			{
				pairLists.AddPair(additionalInfo.OAI_AdditionalInfo);
			}

			return pairLists;
		}
	}
}

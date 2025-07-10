using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocUSContacts : DocContacts
	{
		protected DocUSContacts(USContactAddressSource source, BusinessObjectFactory factoryToWrap)
			: base(source, factoryToWrap)
		{
		}

		public static DocUSContacts New(USOrganisation uSOrganisation, BusinessObjectFactory factoryToWrap)
		{
			DocUSContacts result = null;
			if (uSOrganisation != null && uSOrganisation.Organisation != null)
			{
				USContactAddressSource addressSource = USContactAddressSource.New(uSOrganisation, factoryToWrap);
				if (addressSource != null)
				{
					result = new DocUSContacts(addressSource, factoryToWrap);
				}
			}
			return result;
		}
	}
}

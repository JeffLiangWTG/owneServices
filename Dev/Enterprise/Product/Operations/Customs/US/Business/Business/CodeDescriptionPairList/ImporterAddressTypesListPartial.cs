using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class ImporterAddressTypesList
	{
		public static ZBool IsPhysicalRelatedMailingAddressType(ZString mailingAddressTypeCode)
		{
			return mailingAddressTypeCode == Codes._06 || mailingAddressTypeCode == Codes._07 || mailingAddressTypeCode == Codes._08;
		}
	}
}

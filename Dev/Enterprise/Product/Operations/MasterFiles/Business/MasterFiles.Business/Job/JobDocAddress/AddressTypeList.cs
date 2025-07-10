using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class ResidentialCommercialAddressTypeList : CodeDescriptionPairList
	{
		public static class Descriptions
		{
			public static ZString Residential { get { return ResString.GetMultilingualString("AddressTypeList|Residential", "Residential"); } }
			public static ZString Commercial { get { return ResString.GetMultilingualString("AddressTypeList|Commercial", "Commercial"); } }
		}

		/// <summary>
		/// Shows description only in the drop down edit box
		/// </summary>
		public ResidentialCommercialAddressTypeList()
		{
			AddPair(Codes.Residential, Descriptions.Residential);
			AddPair(Codes.Commercial, Descriptions.Commercial);
		}

		public static class Codes
		{
			public const string Residential = "RES";
			public const string Commercial = "COM";
		}
	}
}

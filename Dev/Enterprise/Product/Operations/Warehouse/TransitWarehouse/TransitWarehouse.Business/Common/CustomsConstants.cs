using CargoWise.Definitions.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class CustomsConstants
	{
		public static class AdditionalReference
		{
			public static class EntryType
			{
				public static class Descriptions
				{
					public static MultilingualString ResponsiblePartyID { get { return ResString.GetMultilingualString("Customs|AdditionalReference|EntryType|ResponsiblePartyID", CustomsAdditionalReferenceTypes.EntryType.Codes.ResponsiblePartyID); } } // GetMultilingualString method requires Engligh Text as parameter
					public static MultilingualString PrincipalID { get { return ResString.GetMultilingualString("Customs|AdditionalReference|EntryType|PrincipalID", CustomsAdditionalReferenceTypes.EntryType.Codes.PrincipalID); } } // GetMultilingualString method requires Engligh Text as parameter
					public static MultilingualString ControlledPremiseID { get { return ResString.GetMultilingualString("Customs|AdditionalReference|EntryType|ControlledPremiseID", CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID); } } // GetMultilingualString method requires Engligh Text as parameter
				}
			}
		}
	}
}

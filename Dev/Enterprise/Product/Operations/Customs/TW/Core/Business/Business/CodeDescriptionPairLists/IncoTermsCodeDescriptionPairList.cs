using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class IncoTermsCodeDescriptionPairList : CodeDescriptionPairList
	{
		public static class Descriptions
		{
			public static MultilingualString CostAndFreight { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|CostAndFreight", "Cost and freight"); } }
			public static MultilingualString CostAndInsurance { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|CostAndInsurance", "Cost and insurance"); } }
			public static MultilingualString CostInsuranceAndFreight { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|CostInsuranceAndFreight", "Cost, insurance and freight"); } }
			public static MultilingualString ExWorks { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|ExWorks", "Ex Works"); } }
			public static MultilingualString FreeAlongsideShip { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|FreeAlongsideShip", "Free alongside Ship"); } }
			public static MultilingualString FreeOnBoard { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|FreeOnBoard", "Free On Board"); } }
			public static MultilingualString FreeCarrier { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|FreeCarrier", "Free Carrier"); } }
			public static MultilingualString CarriagePaidTo { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|CarriagePaidTo", "Carriage Paid To"); } }
			public static MultilingualString CarriageAndInsurancePaidTo { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|CarriageAndInsurancePaidTo", "Carriage Insurance Paid To"); } }
			public static MultilingualString DeliveredAtTerminal { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|DeliveredAtTerminal", "Delivered at Terminal"); } }
			public static MultilingualString DeliveredAtPlace { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|DeliveredAtPlace", "Delivered at Place"); } }
			public static MultilingualString DeliveredDutyPaid { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|DeliveredDutyPaid", "Delivered Duty Paid"); } }
			public static MultilingualString DeliveredAtPlaceUnloaded { get { return ResString.GetMultilingualString("IncoTermsCodeDescriptionPairList|DeliveredAtPlaceUnloaded", "Delivered At Place Unloaded"); } }
		}

		public IncoTermsCodeDescriptionPairList()
		{
			AddPair(IncoTerms.CostInsuranceAndFreight, Descriptions.CostInsuranceAndFreight);
			AddPair(IncoTerms.CostAndFreight, Descriptions.CostAndFreight);
			AddPair(IncoTerms.FreeOnBoard, Descriptions.FreeOnBoard);
			AddPair(IncoTerms.CostAndInsurance, Descriptions.CostAndInsurance);
			AddPair(IncoTerms.FreeAlongsideShip, Descriptions.FreeAlongsideShip);
			AddPair(IncoTerms.ExWorks, Descriptions.ExWorks);
			AddPair(IncoTerms.FreeCarrier, Descriptions.FreeCarrier);
			AddPair(IncoTerms.CarriagePaidTo, Descriptions.CarriagePaidTo);
			AddPair(IncoTerms.CarriageAndInsurancePaidTo, Descriptions.CarriageAndInsurancePaidTo);
			AddPair(IncoTerms.DeliveredAtTerminal, Descriptions.DeliveredAtTerminal);
			AddPair(IncoTerms.DeliveredAtPlace, Descriptions.DeliveredAtPlace);
			AddPair(IncoTerms.DeliveredDutyPaid, Descriptions.DeliveredDutyPaid);
			AddPair(IncoTerms.DeliveredAtPlaceUnloaded, Descriptions.DeliveredAtPlaceUnloaded);
		}
	}
}

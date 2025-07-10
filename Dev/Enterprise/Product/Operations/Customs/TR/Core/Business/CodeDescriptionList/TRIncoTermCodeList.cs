using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class TRIncotermCodeList : CodeDescriptionPairList
	{
		public TRIncotermCodeList()
		{
			AddPair(Core.Constants.IncoTerms.CostAndFreight, Core.Constants.IncoTerms.Descriptions.CostAndFreight);
			AddPair(Core.Constants.IncoTerms.CostInsuranceAndFreight, Core.Constants.IncoTerms.Descriptions.CostInsuranceAndFreight);
			AddPair(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, Core.Constants.IncoTerms.Descriptions.CarriageAndInsurancePaidTo);
			AddPair(Core.Constants.IncoTerms.CarriagePaidTo, Core.Constants.IncoTerms.Descriptions.CarriagePaidTo);
			AddPair(Core.Constants.IncoTerms.DeliveredAtPlace, Core.Constants.IncoTerms.Descriptions.DeliveredAtPlace);
			AddPair(Core.Constants.IncoTerms.DeliveredDutyPaid, Core.Constants.IncoTerms.Descriptions.DeliveredDutyPaid);
			AddPair(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, Core.Constants.IncoTerms.Descriptions.DeliveredAtPlaceUnloaded);
			AddPair(Core.Constants.IncoTerms.ExWorks, Core.Constants.IncoTerms.Descriptions.ExWorks);
			AddPair(Core.Constants.IncoTerms.FreeAlongsideShip, Core.Constants.IncoTerms.Descriptions.FreeAlongsideShip);
			AddPair(Core.Constants.IncoTerms.FreeOnBoard, Core.Constants.IncoTerms.Descriptions.FreeOnBoard);
			AddPair(Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.Descriptions.FreeCarrier);
		}
	}
}

using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class TranshipmentRequestLookups : CusUnderbondLookups
	{
		public TranshipmentRequestLookups(TranshipmentRequest parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MovementReasonList => Factory.GetCachedValue<MovementReason>();

		public CodeDescriptionPairList ModeOfMovement
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("ModeOfMovement", delegate
				{
					var result = new TranshipmentRequestModeOfMovement();
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.Sea);

					return result;
				});
			}
		}

		public CodeDescriptionPairList TranshipmentModeOfMovement
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("TranshipmentModeOfMovement", delegate
				{
					var result = new TranshipmentRequestModeOfMovement();
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.SeaCV);
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.SeaOV);
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.Road);
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.Rail);
					result.RemoveCode(TranshipmentRequestModeOfMovement.Codes.Multimodal);

					return result;
				});
			}
		}

		public virtual RefVesselCollection VesselList => new RefVesselCollection(Factory);

		public OrgHeaderCollection TransitDestinationList
		{
			get { return fTransitDestinationList ?? (fTransitDestinationList = new CTOOrDepotOrWarehouseCollection(Factory)); }
		}
		OrgHeaderCollection fTransitDestinationList;

		public OrgHeaderCollection OriginLocationList
		{
			get { return fOriginLocationList ?? (fOriginLocationList = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection fOriginLocationList;

		public MovementStatus MovementStatusList => Factory.GetCachedValue<MovementStatus>();

		public CombinedMovementStatus CombinedMovementStatusList => Factory.GetCachedValue<CombinedMovementStatus>();
	}
}

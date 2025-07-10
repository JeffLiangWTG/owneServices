using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSDepartureMovementHeaderValidation : CusInBondMoveHeaderValidation
	{
		public SPTSDepartureMovementHeaderValidation(SPTSDepartureMovementHeader parent) : base(parent)
		{
		}

		new SPTSDepartureMovementHeader Parent => (SPTSDepartureMovementHeader)base.Parent;

		protected override void CheckBM_InlandTransportMode()
		{
			base.CheckBM_InlandTransportMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InlandTransportModeInfo);

			if (Parent.BM_InlandTransportMode == SPTSTransportModeList.Codes.SEA && (!Parent.Header?.HeaderContainers.Any() ?? false))
			{
				if (Parent.Header.Bills.Cast<SPTSBill>().Any(x => x.B0_ServiceType == Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes))
				{
					Parent.BM_InlandTransportModeInfo.AddMessageError(Res.GetString("07FCD79F-5A24-4391-AF3A-DB7D1AABB2DA", "A container is required"));
				}
			}
		}

		protected override void CheckBM_OA_InBondCarrier()
		{
			base.CheckBM_OA_InBondCarrier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_OA_InBondCarrierInfo);
		}

		protected override void CheckBM_PortOfPresentationCode()
		{
			base.CheckBM_PortOfPresentationCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_PortOfPresentationCodeInfo);
		}

		protected override void CheckBM_DestinationPortCode()
		{
			base.CheckBM_DestinationPortCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_DestinationPortCodeInfo);
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase5Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
	{
		public NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTankerStatus();
		}

		protected override void CheckBM_LocationOfGoodsCode()
		{
			base.CheckBM_LocationOfGoodsCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_LocationOfGoodsCodeInfo, Parent.Lookups.GoodsShippingLocationList);
		}

		public void ValidateTankerStatus()
		{
			ValidateCalculatedProperty(Parent.TankerStatusInfo);
		}

		protected void CheckTankerStatus()
		{
			if (!Parent.TankerStatus.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TankerStatusInfo, Parent.Lookups.TankerStatusList);
			}
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewVesselRoutingPortPairsValidation
//
//    This class should be used for overriding validation in AutoViewVesselRoutingPortPairsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class ViewVesselRoutingPortPairsValidation : AutoViewVesselRoutingPortPairsValidation
	{
		public ViewVesselRoutingPortPairsValidation(AutoViewVesselRoutingPortPairs parent) : base(parent)
		{
		}

		protected override void CheckE9_RL_NKLoadPort()
		{
			base.CheckE9_RL_NKLoadPort();

			if (Parent.E9_RL_NKLoadPort.IsEmpty)
			{
				Parent.E9_RL_NKLoadPortInfo.AddWarning(Res.GetString("8a204e58-579f-4595-b386-69f091c5333c", "Add foreign ports using the controls below"));
			}
		}

		protected override void CheckE9_RL_NKDischargePort()
		{
			base.CheckE9_RL_NKDischargePort();

			if (Parent.E9_RL_NKDischargePort.IsEmpty)
			{
				Parent.E9_RL_NKDischargePortInfo.AddWarning(Res.GetString("baf12e14-e466-48a2-bb43-d3fe5a14db0f", "Add foreign ports using the controls below"));
			}
		}

		protected override void CheckE9_DataProviderIsNotEmpty()
		{
		}

		protected override void CheckE9_LineOperatorIsNotEmpty()
		{
		}

		protected override void CheckE9_LloydsNumberIsNotEmpty()
		{
		}

		protected override void CheckE9_RL_NKDischargePortIsNotEmpty()
		{
		}

		protected override void CheckE9_RL_NKLoadPortIsNotEmpty()
		{
		}

		protected override void CheckE9_VoyageIsNotEmpty()
		{
		}
	}
}

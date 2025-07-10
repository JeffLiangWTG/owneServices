using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderVesselMovementsUrlHelper : IVesselMovementsUrlSupporter
	{
		public OrderVesselMovementsUrlHelper(Order order)
		{
			Order = order ?? throw new ArgumentNullException(nameof(order));
		}

		public Order Order { get; }

		public (VesselMovementsUrlModel model, string errorMessage) GetVesselMovementsUrlModel()
		{
			if (Order.JD_TransportMode != Constants.TransportModes.Sea)
			{
				return (null, Res.GetString("cc7d59de-3587-4dfe-a232-132f899323a3", "Transport Mode must be SEA."));
			}

			if (Order.PlanningVoyageState == PlanningVoyageState.OneVoyage)
			{
				var model = new VesselMovementsUrlModel
				{
					LloydsNumber = Order.DepartureVessel?.RV_LloydsNumber ?? ZString.Empty,
					DepartureTime = !Order.JD_Milestone_A_DEP.IsEmpty ? Order.JD_Milestone_A_DEP : Order.JD_Milestone_E_DEP,
					ArrivalTime = !Order.JD_Milestone_A_ARV.IsEmpty ? Order.JD_Milestone_A_ARV : Order.JD_Milestone_E_ARV,
					CarrierCode = Order.DepartureVessel?.RV_CarrierCode ?? ZString.Empty,
					VoyageNumber = Order.JD_DepartureVoyage,
					ArrivalPortUnloco = Order.Buyer?.OH_RL_NKClosestPort ?? ZString.Empty,
					DeparturePortUnloco = Order.Supplier?.OH_RL_NKClosestPort ?? ZString.Empty,
				};

				return (model, null);
			}

			var now = ZDateTime.Now;
			if (!Order.JD_E_DEP_3.IsEmpty && Order.JD_E_DEP_3 <= now || !Order.JD_E_ARV_2ndIntermediate.IsEmpty && Order.JD_E_ARV_2ndIntermediate <= now)
			{
				var model = new VesselMovementsUrlModel
				{
					LloydsNumber = Order.ArrivalVessel?.RV_LloydsNumber ?? ZString.Empty,
					DepartureTime = Order.JD_E_DEP_3,
					ArrivalTime = !Order.JD_Milestone_A_ARV.IsEmpty ? Order.JD_Milestone_A_ARV : Order.JD_Milestone_E_ARV,
					CarrierCode = Order.ArrivalVessel?.RV_CarrierCode ?? ZString.Empty,
					VoyageNumber = Order.JD_ArrivalVoyage,
					ArrivalPortUnloco = Order.Buyer?.OH_RL_NKClosestPort ?? ZString.Empty,
				};

				return (model, null);
			}
			else if (!Order.JD_E_DEP_2.IsEmpty && Order.JD_E_DEP_2 <= now || !Order.JD_E_ARV_1stIntermediate.IsEmpty && Order.JD_E_ARV_1stIntermediate <= now)
			{
				var model = new VesselMovementsUrlModel
				{
					LloydsNumber = Order.IntermediateVessel?.RV_LloydsNumber ?? ZString.Empty,
					DepartureTime = Order.JD_E_DEP_2,
					ArrivalTime = Order.JD_E_ARV_2ndIntermediate,
					CarrierCode = Order.IntermediateVessel?.RV_CarrierCode ?? ZString.Empty,
					VoyageNumber = Order.JD_IntermediateVoyage,
				};

				return (model, null);
			}
			else
			{
				var model = new VesselMovementsUrlModel
				{
					LloydsNumber = Order.DepartureVessel?.RV_LloydsNumber ?? ZString.Empty,
					DepartureTime = !Order.JD_Milestone_A_DEP.IsEmpty ? Order.JD_Milestone_A_DEP : Order.JD_Milestone_E_DEP,
					ArrivalTime = Order.JD_E_ARV_1stIntermediate,
					CarrierCode = Order.DepartureVessel?.RV_CarrierCode ?? ZString.Empty,
					VoyageNumber = Order.JD_DepartureVoyage,
					DeparturePortUnloco = Order.Supplier?.OH_RL_NKClosestPort ?? ZString.Empty,
				};

				return (model, null);
			}
		}
	}
}

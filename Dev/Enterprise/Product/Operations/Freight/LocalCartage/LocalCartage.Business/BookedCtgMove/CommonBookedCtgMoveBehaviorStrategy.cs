using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonBookedCtgMoveBehaviorStrategy
	{
		public CommonBookedCtgMoveBehaviorStrategy()
		{
		}

		public virtual void CartageBookedMoveLinkCreated(CommonBookedCtgMove bookedMove)
		{
		}

		public virtual void CartageBookedMoveLinkBroken(CommonBookedCtgMove bookedMove)
		{
		}

		public virtual void PickupAddressChanged(CommonBookedCtgMove bookedMove, ZGuid previousValue)
		{
			BookedMoveAddressChanged(bookedMove, previousValue, bookedMove.EW_E2PickupAddressID);
			SetDropMode(bookedMove.PickupFromDocAddress, bookedMove);
		}

		public virtual void WaitPointAddressChanged(CommonBookedCtgMove bookedMove, ZGuid previousValue)
		{
			BookedMoveAddressChanged(bookedMove, previousValue, bookedMove.EW_E2WaitPointAddressID);
			SetDropMode(bookedMove.WaitPointDocAddress, bookedMove);
		}

		public virtual void DeliveryAddressChanged(CommonBookedCtgMove bookedMove, ZGuid previousValue)
		{
			BookedMoveAddressChanged(bookedMove, previousValue, bookedMove.EW_E2DeliveryAddressID);
			SetDropMode(bookedMove.DeliverToDocAddress, bookedMove);
		}

		void SetDropMode(JobDocAddress docAddress, CommonBookedCtgMove bookedMove)
		{
			if (!bookedMove.DefaultingAddresses && docAddress != null && docAddress.DocAddressType == bookedMove.RequestedAddressType)
			{
				var changeToAddressDropMode = false;
				var dropMode = bookedMove.GetAddressDropMode(docAddress);
				if (!bookedMove.EW_DropMode.IsEmpty && !dropMode.IsEmpty && dropMode != bookedMove.EW_DropMode)
				{
					var cartage = bookedMove.Cartage;
					changeToAddressDropMode = cartage == null;

					if (cartage != null)
					{
						var message = Res.GetString("409daad9-7f7e-40ae-954c-a221bc6c4087", "Would you like to set the Booked Move Drop Mode with the {0} Drop Mode '{1}'?", docAddress.AddressCaption, dropMode);
						var caption = Res.GetString("5d018714-8ea9-4049-9334-b857a46626db", "Populate Booked Move Drop Mode");
						var e = new QueryUserYesNoEventArgs(caption, message, false);

						cartage.NotificationsQueryUser(e);
						changeToAddressDropMode = e.Response;
					}
				}

				if (bookedMove.EW_DropMode.IsEmpty || changeToAddressDropMode)
				{
					bookedMove.EW_DropMode = dropMode;
				}
			}
		}

		void BookedMoveAddressChanged(CommonBookedCtgMove bookedMove, ZGuid previousValue, ZGuid newValue)
		{
			if (!previousValue.IsEmpty && !newValue.IsEmpty)
			{
				foreach (CommonCartageLeg leg in bookedMove.CartageLegs)
				{
					if (leg.JU_E2PickupAddressID == previousValue)
					{
						leg.JU_E2PickupAddressID = newValue;
					}

					if (leg.JU_E2WaitPointAddressID == previousValue)
					{
						leg.JU_E2WaitPointAddressID = newValue;
					}

					if (leg.JU_E2DeliveryAddressID == previousValue)
					{
						leg.JU_E2DeliveryAddressID = newValue;
					}
				}
			}
		}
	}
}

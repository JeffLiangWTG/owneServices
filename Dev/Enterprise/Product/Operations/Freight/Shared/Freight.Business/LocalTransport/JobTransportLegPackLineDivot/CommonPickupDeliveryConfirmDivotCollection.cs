using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmDivotCollection : ActiveBusinessObjectCollection<CommonConfirmDivot>, IGoodsCollection
	{
		public CommonPickupDeliveryConfirmDivotCollection(CommonPickupDeliveryConfirm pickupDelivery)
			: base(pickupDelivery.Factory, pickupDelivery, null, JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm)
		{
			this.pickupDelivery = pickupDelivery;
		}

		public CommonPickupDeliveryConfirmDivotCollection(PackLine packLine)
			: base(packLine.Factory, packLine, null, JobTransportLegPackLineDivotSchema.J8_JL)
		{
			this.packLine = packLine;
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly CommonPickupDeliveryConfirm pickupDelivery;

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly PackLine packLine;

		#region Override

		#region Logging for Divots

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Reporting")]
		protected override void OnAdded(CommonConfirmDivot divot)
		{
			if (!DebugLogSuspender.IsSuspended)
			{
				var divotLogMessage = $"{divot.PK}" + "\r\n"
					+ "Confirm: "
					+ CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.GetConfirmLogString(divot.Confirm);
				var debugLogMessage = $"\r\nOnAdded - Divot created: {divotLogMessage}\r\nStackTrace: '{System.Environment.StackTrace}'";
				CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage += debugLogMessage;
			}

			base.OnAdded(divot);
		}

		public Semaphore DebugLogSuspender
		{
			get { return debugLogSuspender ?? (debugLogSuspender = new Semaphore()); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		Semaphore debugLogSuspender;

		#endregion

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void OnLoadedIntoCollectionCore(CommonConfirmDivot loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);

			if (packLine != null)
			{
				loadedObject.PackLineType = packLine.GetType();
			}
			else if (pickupDelivery != null)
			{
				loadedObject.PackLineType = pickupDelivery.PackLineType;
			}
		}

		#endregion
	}
}

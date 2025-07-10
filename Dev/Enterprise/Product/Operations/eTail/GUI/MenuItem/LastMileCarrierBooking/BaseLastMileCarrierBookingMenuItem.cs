using System;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public abstract class BaseLastMileCarrierBookingMenuItem(MultilingualString caption, Func<HVLVConsignment> consignmentGetter)
		: ZMenuItem(caption)
	{
		protected Func<HVLVConsignment> ConsignmentGetter { get; } = consignmentGetter;

		protected HVLVConsignment Consignment
		{
			get
			{
				return ConsignmentGetter();
			}
		}

		protected sealed override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (Consignment.HVC_OH_LastMileCarrier.IsEmpty
				|| Consignment.HVC_OH_LastMileCarrierBookingAgent.IsEmpty
				|| Consignment.HVC_OA_DestinationDepot.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("00a008fe-c92d-46f8-91b7-4b23747e6e82", "Carrier Booking Agent, Last Mile Carrier or Destination Depot have not been configured."));
			}
			else
			{
				MenuAction.Invoke();
			}
		}

		protected abstract Action MenuAction { get; }

		public virtual void UpdateVisibilityAndCaption()
		{
			Visible = Consignment != null && !Consignment.HVC_IsSelfBooked;
		}
	}
}

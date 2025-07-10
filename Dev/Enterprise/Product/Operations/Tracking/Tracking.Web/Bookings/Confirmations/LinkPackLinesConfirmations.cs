using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings
{
	public class LinkPackLinesConfirmations : ZPageConfirmation
	{
		#region Constructors

		public LinkPackLinesConfirmations(ZPage page)
			: base(page)
		{
		}

		#endregion

		#region New

		public new EditBooking Page
		{
			get
			{
				return base.Page as EditBooking;
			}
		}

		#endregion

		#region Overrides

		protected new TrackingBooking DataSource
		{
			get
			{
				return base.DataSource as TrackingBooking;
			}
		}

		protected override ConfirmationTypes ConfirmationType
		{
			get
			{
				return ConfirmationTypes.Warning;
			}
		}

		public override bool AlwaysRegisterConfirmationScript
		{
			get
			{
				return true;
			}
		}

		protected override bool GetRequired()
		{
			return Required;
		}

		internal bool Required
		{
			get { return required; }
			set { required = value; }
		}
		bool required;

		protected override string GetTitle()
		{
			return Res.GetString("0e691a2d-3355-436f-84ad-6d24a0eed139", "Link Order Lines to Pack-lines");
		}

		protected override string GetConfirmationMessage()
		{
			return Res.GetString("c26c5fa5-6afc-44e7-bf78-43478c30afad", "Do you want to link the order lines from the recently attached orders to pack-lines on this booking?");
		}

		protected override string GetUserResponseHolderID()
		{
			return "LinkPacklinesConfirmation";
		}

		#endregion
	}
}

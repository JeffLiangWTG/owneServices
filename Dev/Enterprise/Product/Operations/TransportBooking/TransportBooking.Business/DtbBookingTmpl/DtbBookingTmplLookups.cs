using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingTmplLookups : Common.DtbBookingTmplLookups
	{
		public DtbBookingTmplLookups(DtbBookingTmpl parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Directions
		{
			get { return BindToLists.Directions; }
		}

		public CodeDescriptionPairList RatingFreightModes
		{
			get { return BindToLists.RatingFreightModes.List; }
		}

		public BindToLists BindToLists
		{
			get { return Factory.GetCachedValue("TransportBookings|BindToLists", () => new BindToLists(Factory)); }
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingProcessTaskCollection : ProcessTaskCollection
	{
		public QuotedBookingProcessTaskCollection(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		public new QuotedBookingProcessTask this[int index]
		{
			get { return (QuotedBookingProcessTask)Elements[index]; }
		}

		public new QuotedBookingProcessTask AddNew()
		{
			return (QuotedBookingProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new QuotedBookingProcessTaskCollection(Parent);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, ViewQuotedBookingSchema.Constants.Prefix);
			return result;
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.OriginUNLOCO != null ? Parent.OriginUNLOCO.RL_RN_NKCountryCode : ZString.Empty; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.DestinationUNLOCO != null ? Parent.DestinationUNLOCO.RL_RN_NKCountryCode : ZString.Empty; }
		}

		#endregion

		#region Implementation

		new QuotedBooking Parent
		{
			get { return (QuotedBooking)base.Parent; }
		}

		#endregion
	}
}

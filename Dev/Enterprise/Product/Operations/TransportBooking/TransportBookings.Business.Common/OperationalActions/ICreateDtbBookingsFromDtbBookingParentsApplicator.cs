using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared
{
	public interface ICreateDtbBookingsFromDtbBookingParentsApplicator
	{
		public static class Schema
		{
			public const int DirectionMaxLength = 3;
			public const int BookingTemplateMaxLength = 4;
		}

		[MaxLength(Schema.DirectionMaxLength)]
		[List(nameof(DirectionList))]
		public ZString Direction { get; set; }
		public ZPropertyInfo DirectionInfo { get; }

		[MaxLength(Schema.BookingTemplateMaxLength)]
		[List(nameof(BookingTemplateList))]
		public ZString BookingTemplate { get; set; }
		public ZPropertyInfo BookingTemplateInfo { get; }

		public CodeDescriptionPairList DirectionList { get; }
		public CodeDescriptionPairList BookingTemplateList { get; }
	}
}

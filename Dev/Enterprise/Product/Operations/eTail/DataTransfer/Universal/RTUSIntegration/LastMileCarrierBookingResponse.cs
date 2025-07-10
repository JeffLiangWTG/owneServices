using System.Collections.ObjectModel;
using Enterprise.eTail.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class LastMileCarrierBookingResponse : ILastMileCarrierBookingResponse
	{
		public bool Successful { get; internal set; }
		public ReadOnlyCollection<byte> BinaryData { get; internal set; }
		public string FileType { get; internal set; } = nameof(WTG.RTUS.Interface.FileType.Unknown);
		public string TrackingNumber { get; internal set; } = string.Empty;
		public string ErrorMessage { get; internal set; } = string.Empty;
	}
}

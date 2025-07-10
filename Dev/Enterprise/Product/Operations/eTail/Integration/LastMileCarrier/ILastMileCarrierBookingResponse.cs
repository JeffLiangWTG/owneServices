using System.Collections.ObjectModel;

namespace Enterprise.eTail.Integration
{
	public interface ILastMileCarrierBookingResponse
	{
		bool Successful { get; }
		ReadOnlyCollection<byte> BinaryData { get; }
		string FileType { get; }
		string TrackingNumber { get; }
		string ErrorMessage { get; }
	}
}

using System.Collections.Generic;

namespace Enterprise.eTail.Integration
{
	public interface ILastMileCarrierBookingResponseCollection : IList<ILastMileCarrierBookingResponse>
	{
		bool HasError { get; }
		string ErrorMessage { get; }
	}
}

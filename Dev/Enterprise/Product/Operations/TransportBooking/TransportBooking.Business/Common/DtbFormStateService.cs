using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public enum DtbFormState
	{
		Parent,
		Booking
	}

	/// <summary>
	/// This is used to determine which Form we are on, whether a Parent Form or the Booking
	/// Form. We need to know this when printing Documents (e.g Cartage Advice), so we know 
	/// where to apply Document Events against (i.e against the Booking or Parent).
	/// </summary>
	public class DtbFormStateService
	{
		DtbFormStateService()
		{
			State = DtbFormState.Parent;
		}

		public static void SetState(BusinessObjectFactory factory, DtbFormState state)
		{
			factory.GetCachedValue("DtbFormStateService", () => new DtbFormStateService()).State = state;
		}

		public static DtbFormState GetState(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DtbFormStateService", () => new DtbFormStateService()).State;
		}

		DtbFormState State { get; set; }
	}
}

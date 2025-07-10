using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public enum DtbChildEditableServiceState
	{
		Consolidation,
		Transport,
		None // for example, LWK service task
	}

	/// <summary>
	/// This State is representative of whether the Transport or the Consolidation is the Master Business Object.
	/// This is used to determine whether the Consolidation should be Child Editable or not.
	/// Also used to determine whether auto logs are added on a booking if the booking is not TopLevel (i.e. Consolidation)
	/// </summary>
	public class DtbChildEditableService
	{
		public static DtbChildEditableServiceState GetState(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<DtbChildEditableService>().State;
		}

		public static void SetState(BusinessObjectFactory factory, DtbChildEditableServiceState state)
		{
			factory.GetCachedValue<DtbChildEditableService>().State = state;
		}

		DtbChildEditableServiceState State = DtbChildEditableServiceState.None;
	}
}

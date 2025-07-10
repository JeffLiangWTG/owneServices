using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business.Business
{
	public static class IStmALogExtensions
	{
		public static void AddEventWithReference(this IStmALogProvider logProvider, Event eventType, ZString reference)
		{
			logProvider.Logs.AddNew(eventType, reference);
		}
	}
}

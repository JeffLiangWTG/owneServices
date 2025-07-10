using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	internal sealed class ScheduleUpdateNullQueryProvider : IScheduleUpdateQueryProvider
	{
		public bool ShouldUpdateAgencyShipmentDatesFromATD
		{
			get { return true; }
		}

		public bool ShouldUpdateRelatedShipmentsETD
		{
			get { return true; }
		}

		public bool ShouldUpdateRelatedShipmentsETA
		{
			get { return true; }
		}

		public void ShowInformation(ZString message)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				UnitTestUserNotification.Instance.ShowInformation(message);
			}
#endif
		}

		public bool ShouldSendDelayAlerts(bool hasDelayedImportsVessels, bool hasDelayedExportsVessels)
		{
			return true;
		}

		public ZGuid? ParentPK
		{
			get { return null; }
		}
	}
}

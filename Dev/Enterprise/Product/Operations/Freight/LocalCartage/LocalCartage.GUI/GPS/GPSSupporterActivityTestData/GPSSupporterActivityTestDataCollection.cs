using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class GPSSupporterActivityTestDataCollection : BusinessObjectCollection<GPSSupporterActivityTestData>
	{
		public GPSSupporterActivityTestDataCollection(CommonWorkSheet workSheet)
			: base(workSheet.Factory)
		{
			this.WorkSheet = workSheet;
		}

		public readonly CommonWorkSheet WorkSheet;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (WorkSheet != null)
			{
				var activity = (GPSSupporterActivityTestData)child;
				activity.EN_RQ_Vehicle = WorkSheet.EY_RQ_Truck;
				activity.EN_ActivityID = GetActivityID();
				activity.EN_EventType = GPSConstants.GPSEventTypeList.Codes.Custom;
			}
		}

		ZString GetActivityID()
		{
			var length = 10;
			var trimmedTicks = (ZString)ZDateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).TrimEnd('0');

			return trimmedTicks.SubstringSafe(0, length);
		}
	}
}

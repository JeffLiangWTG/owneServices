using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentJobDatesProvider : JobDatesProvider<CFSShipment>
	{
		public CFSShipmentJobDatesProvider(CFSShipment cfsShipment)
			: base(cfsShipment) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return GetCFSShipmentActualDate();
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetCFSShipmentActualDate();
		}

		ZDateTime GetCFSShipmentActualDate()
		{
			return Parent.JS_A_RCV.IsEmpty ? ZDateTime.Today : Parent.JS_A_RCV;
		}
	}
}

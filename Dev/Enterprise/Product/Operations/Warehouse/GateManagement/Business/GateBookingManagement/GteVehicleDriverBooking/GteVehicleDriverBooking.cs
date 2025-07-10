using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public class GteVehicleDriverBooking : AutoGteVehicleDriverBooking
	{
		public GteVehicleDriverBooking(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public GteBooking Booking => Factory.Load<GteBooking>(GBD_GBK_Booking);

		[RelatedBusinessObject("Booking")]
		public override ZGuid GBD_GBK_Booking { get => base.GBD_GBK_Booking; set => base.GBD_GBK_Booking = value; }

		#endregion
	}
}

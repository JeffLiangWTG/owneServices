using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public class GteVehicleMovementBooking : AutoGteVehicleMovementBooking
	{
		public GteVehicleMovementBooking(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public GteBooking Booking => Factory.Load<GteBooking>(GBV_GBK_Booking);

		[RelatedBusinessObject("Booking")]
		public override ZGuid GBV_GBK_Booking { get => base.GBV_GBK_Booking; set => base.GBV_GBK_Booking = value; }

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GBV_VehicleRegistration = "REGO";
		}

#endif
	}
}

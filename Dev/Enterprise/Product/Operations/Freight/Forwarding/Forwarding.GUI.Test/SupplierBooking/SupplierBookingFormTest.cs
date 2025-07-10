using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SupplierBookingForm))]
	public class SupplierBookingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<JobSupplierBooking>();
			bO.JSB_TransportMode = "AIR";
			bO.JSB_LoadMode = "CY";
			bO.JSB_Status = "PLN";
			bO.JSB_BookingId = "SBK0001";

			factory.Save();

			var result = new SupplierBookingForm(bO);
			result.ControllerID = ControllerIDs.SupplierBooking;
			return result;
		}
	}
}

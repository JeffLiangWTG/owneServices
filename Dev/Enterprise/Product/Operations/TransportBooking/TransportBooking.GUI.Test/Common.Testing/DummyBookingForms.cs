using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

[TestClass]
class DummyBookingFormForControlBinding : ZForm
{
	public DummyBookingFormForControlBinding(DtbBooking booking)
		: base(booking)
	{
	}

	public void AddControlAndShowForm(ZUserControl control)
	{
		DataSourceType = typeof(DtbBooking);
		BindingSource.DataSourceType = typeof(DtbBooking);
		Controls.Add(control);
		BindingSource.SetBindingMember(control, ".");
		Show();
	}
}

using Enterprise.Environment;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.TransportBookings.Module.Testing
{
	class DtbBookingTabPlugInTest : DtbBookingTestCaseWithFactory
	{
		public void TestLicenceCheckPoint()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			using (var form = new DtbBookingPlugInFormWithUserControlTest(dummy, null))
			{
				IPlugInInternals plugIn = form.ExposedPlugIn;
				AssertEquals(Env.Licence.TransportBookings, plugIn.LicenceCheckPoint);
			}
		}

		public void TestUserControl()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			using (var form = new DtbBookingPlugInFormWithUserControlTest(dummy, DtbBookingDirection.PIC))
			{
				form.Show();

				AssertType<ZAutoSizedTabPagePlugIn>(form.ExposedPlugIn.TabPage);
				AssertType<DtbBookingUserControl>(form.ExposedPlugIn.UserControl);

				var businessEntity = form.ExposedPlugIn.BusinessEntity;
				AssertType<DtbBookingParentWrapper>(businessEntity);

				var wrapper = (DtbBookingParentWrapper)businessEntity;
				AssertEquals(dummy, wrapper.Parent);
				AssertEquals(DtbBookingDirection.PIC, wrapper.Direction);
			}
		}

		public void TestUserControl_NoSelection()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			using (var form = new DtbBookingPlugInFormWithUserControlTest(dummy, null))
			{
				form.Show();

				AssertType<DtbBookingUserControl>(form.ExposedPlugIn.UserControl);

				var businessEntity = form.ExposedPlugIn.BusinessEntity;
				AssertType<DtbBookingParentWrapper>(businessEntity);

				// Will Pick Single Direction if Parent only supports one direction
				var wrapper = (DtbBookingParentWrapper)businessEntity;
				AssertEquals(dummy, wrapper.Parent);
				AssertEquals(DtbBookingDirection.PIC, wrapper.Direction);
			}
		}

		public void TestUserControl_NoSelection_MoreThanOneSupportedDirection()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.SupportedDirections = new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };

			using (var form = new DtbBookingPlugInFormWithUserControlTest(dummy, null))
			{
				form.Show();

				AssertType<DtbBookingUserControl>(form.ExposedPlugIn.UserControl);

				var businessEntity = form.ExposedPlugIn.BusinessEntity;
				AssertType<DtbBookingParentWrapper>(businessEntity);

				// Will choose none if Parent supports more one direction and no direction selected
				var wrapper = (DtbBookingParentWrapper)businessEntity;
				AssertEquals(dummy, wrapper.Parent);
				AssertEquals(DtbBookingDirection.None, wrapper.Direction);
			}
		}

		public void TestUserControl_InvalidSelection()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			using (var form = new DtbBookingPlugInFormWithUserControlTest(dummy, DtbBookingDirection.DLV))
			{
				form.Show();

				AssertType<DtbBookingUserControl>(form.ExposedPlugIn.UserControl);

				var businessEntity = form.ExposedPlugIn.BusinessEntity;
				AssertType<DtbBookingParentWrapper>(businessEntity);

				// Will choose None if the selected Direction was not valid
				var wrapper = (DtbBookingParentWrapper)businessEntity;
				AssertEquals(dummy, wrapper.Parent);
				AssertEquals(DtbBookingDirection.None, wrapper.Direction);
			}
		}

		internal class DtbBookingPlugInFormWithUserControlTest : ZForm
		{
			internal DtbBookingPlugInFormWithUserControlTest(DummyWithDtbBooking dummyParent, DtbBookingDirection? selectedDirection)
				: base(dummyParent)
			{
				MainTabControl = new ZTemplateTabControl();
				Controls.Add(MainTabControl);

				if (selectedDirection.HasValue)
				{
					MainTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.DtbBookingTabPlugIn, 0, () => new BookingDirectionSelection(dummyParent, selectedDirection.Value));
				}
				else
				{
					MainTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.DtbBookingTabPlugIn, 0);
				}
			}

			readonly ZTemplateTabControl MainTabControl;

			public DtbBookingTabPlugIn ExposedPlugIn => (DtbBookingTabPlugIn)MainTabControl.PlugIns.GetPlugIn(ControllerIDs.DtbBookingTabPlugIn);
		}
	}
}

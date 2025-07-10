using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(QuotedBookingForm))]
	public class QuotedBookingFormBasherTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "NVOCCModeCheckBox")
			{
				return true;
			}
			else
			{
				return base.ShouldIgnoreMissingBindingMember(control);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				QuotedBooking result = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
				result.Booking.JS_PackingMode = Constants.ContainerModes.OnBoardCourier;
				result.TryLoadOrCreateJob();
				result.Job.JH_GE = Env.CurrentDepartment.PK;
				Factory.Save();
				QuotedBookingForm form = new QuotedBookingForm(result);
				form.ControllerID = ControllerIDs.QuotedBookings;
				return form;
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;
		#endregion
	}
}

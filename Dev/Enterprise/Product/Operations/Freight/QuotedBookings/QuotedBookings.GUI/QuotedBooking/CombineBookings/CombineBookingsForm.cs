using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class CombineBookingsForm : ZChildForm
	{
		public CombineBookingsForm(CombineBookings combine)
			: base(combine)
		{
			InitializeComponent();
		}

		public static void Show(QuotedBooking quotedBooking)
		{
			if (quotedBooking == null)
			{
				throw new ArgumentNullException(nameof(quotedBooking));
			}

			var factoryForDialog = new BusinessObjectFactory();
			var bookingForDialog = factoryForDialog.Load<QuotedBooking>(quotedBooking.PK);

			if (bookingForDialog == null)
			{
				Globals.Message.Show(Res.GetString("be33bf39-f956-42f8-8be3-46a0bd7514fb", "Booking should be in database for Combine function to work."));
				return;
			}

			bookingForDialog.SetReadOnlyIncludingChildren(true);

			var combine = new CombineBookings(bookingForDialog);
			var form = new CombineBookingsForm(combine);

			form.Show();
		}

		#region Implementation

		CombineBookings CombineBookings
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CombineBookings)this.BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return Res.GetString("14b2dcc7-6a12-4128-9b56-bc6d470638f1", "Combine Bookings"); }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("10c0fcfd-417d-4b67-b095-acff9622a2c3", "booking"), Res.GetString("5c195fe6-e37d-4153-b78f-09a6e7eef717", "combine"), Res.GetString("73c42f91-b2fe-4cb6-81b2-ac641f214187", "combined"), includeIgnoreOption);
		}

		bool ValidateAndCombine()
		{
			CombineBookings.RunPreSaveValidation();

			if (CombineBookings.HasErrors || CombineBookings.OtherViewQuotedBookings.Any(x => x.HasRowErrors))
			{
				Globals.Message.Show(Res.GetString("130763bb-2dad-45f1-ab16-b8d92433d681", "Please fix errors before combining."));
				return false;
			}
			else
			{
				return Combine();
			}
		}

		bool Combine()
		{
			DisposePendingUserAction();

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

#if DEBUG
			LastUsedController = controller;
#endif

			var form = (QuotedBookingForm)controller.ShowEditForm(CombineBookings.MasterQuotedBooking);

			if (form != null)
			{
				form.ForceFormCloseOnSaveConcurrencyException = true;
				CombineBookings.Combine(form.BusinessEntity.Factory);

				return true;
			}
			else
			{
				return false;
			}
		}

		void okButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndCombine())
			{
				Close();
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class CombineBookingsForm
	{
		public ZController LastUsedController { get; private set; }
	}
}

#endregion
#endif
#endregion

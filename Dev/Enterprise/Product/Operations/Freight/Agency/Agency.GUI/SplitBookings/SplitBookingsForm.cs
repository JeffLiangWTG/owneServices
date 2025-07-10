using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class SplitBookingsForm : ZChildForm
	{
		public SplitBookingsForm(SplitBookingsHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		public static SplitBookingsForm Show(AgencyBooking booking)
		{
			if (booking == null)
			{
				throw new ArgumentNullException(nameof(booking));
			}

			BusinessObjectFactory factoryForDialog = new BusinessObjectFactory();
			factoryForDialog.RefreshEnabled = false;
			AgencyBooking bookingForDialog = factoryForDialog.Load<AgencyBooking>(booking.PK);
			SplitBookingsHeader header = SplitBookingsHeader.New(bookingForDialog);
			SplitBookingsForm form = new SplitBookingsForm(header);

			HookFormForAutoClose(form);

			form.Show();
			return form;
		}

		static partial void HookFormForAutoClose(SplitBookingsForm form);

		public override string FormVerb
		{
			get { return ""; }
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void okButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				SplitBookingsHeader header = (SplitBookingsHeader)CurrentDataItem;

				string message = Res.GetString("{D4DA8BCB-B52A-4455-BD09-C8306B9BE4B7}", "The booking {0} has been created.", header.NewShipment.JS_UniqueConsignRef);
				string caption = Res.GetString("{A170A702-B763-42cc-95F7-738B61DC54CE}", "Booking Split Successfully");

				Globals.Message.Show(message, caption, MessageBoxButtons.OK, DialogResult.OK);
				Close();
			}
		}
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.GUI
{
	partial class SplitBookingsForm
	{
		static partial void HookFormForAutoClose(SplitBookingsForm form)
		{
			if (Globals.IsTest && !SuppressAutoCloseInTests)
			{
				form.Shown += delegate
				{ form.BeginInvoke(new MethodInvoker(form.Close)); };
			}
		}

		[ThreadStatic]
		internal static bool SuppressAutoCloseInTests;
	}
}

#endregion


#endif
#endregion

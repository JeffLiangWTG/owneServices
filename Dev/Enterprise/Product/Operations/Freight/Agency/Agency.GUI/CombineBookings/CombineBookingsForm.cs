using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class CombineBookingsForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CombineBookingsForm()
		{
			InitializeComponent();
		}

		public CombineBookingsForm(CombineBookings combine)
			: base(combine)
		{
			InitializeComponent();
		}

		public static CombineBookingsForm Show(AgencyBooking booking)
		{
			if (booking == null)
			{
				throw new ArgumentNullException(nameof(booking));
			}

			BusinessObjectFactory factoryForDialog = new BusinessObjectFactory();
			AgencyBooking bookingForDialog = factoryForDialog.Load<AgencyBooking>(booking.PK);
			CombineBookings combine = new CombineBookings(bookingForDialog);
			CombineBookingsForm form = new CombineBookingsForm(combine);

#if DEBUG
			if (Globals.IsTest && !SuppressAutoCloseInTests)
			{
				form.Shown += delegate
				{ form.BeginInvoke(new MethodInvoker(form.Close)); };
			}
#endif

			form.Show();
			return form;
		}

		#region Implementation

		public override string FormHeading
		{
			get { return Res.GetString("49da2fd9-c42a-4712-ba97-d73234363810", "Combine Bookings"); }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("7f3bdfca-1214-4d87-b6a3-0fceb19c2b4d", "booking"), Res.GetString("27639ead-dc98-44c6-bd0d-6d304d918b22", "combine"), Res.GetString("d37a0010-1136-4e8b-9505-9e0a414720ad", "combined"), includeIgnoreOption);
		}

		bool ValidateAndCombine()
		{
			CombineBooking.RunPreSaveValidation();

			if (CombineBooking.HasErrors)
			{
				this.ShowErrorsDialog();
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

			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);

#if DEBUG
			LastUsedController = controller;
#endif

			ZForm form = (ZForm)controller.ShowEditForm(CombineBooking.MasterBooking);

			if (form != null)
			{
				CombineBooking.Combine(form.BusinessEntity.Factory);
				return true;
			}
			else
			{
				return false;
			}
		}

		CombineBookings CombineBooking
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CombineBookings)this.BusinessEntity; }
		}

		#endregion

		#region Events

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

namespace Enterprise.Freight.Agency.GUI
{
	partial class CombineBookingsForm
	{
		[ThreadStatic]
		internal static bool SuppressAutoCloseInTests;

		internal ZController LastUsedController { get; private set; }
	}
}

#endregion


#endif
#endregion

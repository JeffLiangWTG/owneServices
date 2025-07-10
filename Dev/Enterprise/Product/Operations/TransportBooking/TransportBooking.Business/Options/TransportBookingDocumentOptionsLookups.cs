using System;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business.Options
{
	public class TransportBookingDocumentOptionsLookups : ZLookups
	{
		public TransportBookingDocumentOptionsLookups(TransportBookingDocumentOptions parent)
			: base(parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(Res.GetString("216c0732-9982-4a3f-8f4d-7dac40245316", "Parent cannot be null."));
			}
		}

		public DtbBookingTmplCollection BookingTemplates
		{
			get
			{
				BindToLists.BookingTemplatesAdditionalQuery = Parent?.Parent?.TransportBookingTemplateFilters;

				return BindToLists.BookingTemplates;
			}
		}

		public BindToLists BindToLists => bindToLists ?? (bindToLists = new BindToLists(Factory));

		BindToLists bindToLists;

		protected new TransportBookingDocumentOptions Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => (TransportBookingDocumentOptions)base.Parent;
		}
	}
}

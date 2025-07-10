using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionTmplCollection : ActiveBusinessObjectCollection<DtbBookingInstructionTmpl>
	{
		public DtbBookingInstructionTmplCollection(DtbBookingTmpl transportBookingTemplate)
			: base(transportBookingTemplate.Factory, transportBookingTemplate, null, DtbBookingInstructionTmplSchema.K2_KT_BookingTmpl)
		{
			SortBySequence(); // this does not sort until the elements are accessed
		}

		void SortBySequence()
		{
			ApplySort(DtbBookingInstructionTmplSchema.Constants.K2_Sequence, ListSortDirection.Ascending);
		}

		protected override void SetDefaultsForNewElementCore(DtbBookingInstructionTmpl newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.K2_Sequence = Convert.ToSByte(newElement.Template.Instructions.Count + 1);
		}

		protected override bool AllowNew
		{
			get { return !Template.KT_IsSystem; }
		}

		DtbBookingTmpl Template
		{
			get { return (DtbBookingTmpl)Relationship.Master; }
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionTmplCollection : ActiveBusinessObjectCollection<DtbTransportInstructionTmpl>
	{
		protected DtbTransportInstructionTmplCollection(DtbTransportTmpl parent)
			: base(parent.Factory, parent, null, DtbBookingInstructionTmplSchema.K2_KT_BookingTmpl)
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return !Template.KT_IsSystem; }
		}

		#endregion

		#region Template

		DtbTransportTmpl Template
		{
			get { return (DtbTransportTmpl)Relationship.Master; }
		}

		#endregion
	}
}

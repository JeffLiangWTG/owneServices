using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	/// <summary>
	/// This collection does NOT behave like a normal ActiveBusinessObjectCollection,
	/// unless using the constructor with a Consolidation passed in, only consignments
	/// which have been saved will appear in the collection.
	/// </summary>
	[ModuleID(ModuleId.DtbBookingConsignment)]
	public class DtbBookingConsignmentCollection : DtbTransportCollection<DtbBookingConsignment>
	{
		public DtbBookingConsignmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DtbBookingConsignmentCollection(DtbConsignmentConsolidation consolidation)
			: base(consolidation.Factory, consolidation, null, DtbBookingSchema.KM_KB_Booking)
		{
			AllowNewCore = true;
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return AllowNewCore; }
		}

		readonly bool AllowNewCore;

		#endregion

		#region GetParentType

		protected override string GetParentType()
		{
			return TransportConsolidationJobTypes.Codes.Consignment;
		}

		#endregion
	}
}

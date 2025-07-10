using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyBooking)]
	public class AgencyBookingCollection : ActiveBusinessObjectCollection<AgencyBooking>
	{
		public AgencyBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			allowNew = true;
		}

		public AgencyBookingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			allowNew = true;
		}

		public AgencyBookingCollection(BusinessObjectFactory factory, ICollectionRelationship relationship, bool allowNew)
			: base(factory, relationship)
		{
			this.allowNew = allowNew;
		}

		protected override bool AllowNew
		{
			get { return allowNew; }
		}

		readonly bool allowNew;
	}
}

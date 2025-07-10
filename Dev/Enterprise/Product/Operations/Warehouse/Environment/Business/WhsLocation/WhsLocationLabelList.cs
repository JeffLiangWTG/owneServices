using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationLabelList : NonPersistentBusinessObject
	{
		#region Constructors

		public WhsLocationLabelList(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsLocationLabelList(List<WhsLocation> locations, BusinessObjectFactory factory)
			: base(factory)
		{
			this.locations = locations;
		}

		#endregion

		#region Properties

		public List<WhsLocation> Locations
		{
			get
			{
				if (locations == null)
				{
					locations = new List<WhsLocation>();
				}
				return locations;
			}
		}

		#endregion

		#region Implementation

		List<WhsLocation> locations;

		#endregion
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLoader : Integration.ICommonCartageLoader
	{
		public IEnumerable<BusinessObject> GetRelatedCommonCartagesForLocatingEDocs(IDtbBooking booking)
		{
			var commonCartageQuery = new ZQuery(JobCartageSchema.JJ_ParentID, booking.PK.ToGuid());
			var commonCartages = booking.Factory.Load<CommonCartage>(commonCartageQuery);

			return commonCartages;
		}
	}
}

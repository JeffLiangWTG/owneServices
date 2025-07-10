using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICommonCartageLoader
	{
		IEnumerable<BusinessObject> GetRelatedCommonCartagesForLocatingEDocs(IDtbBooking booking);
	}
}

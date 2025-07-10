using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				if (parent is AgencyBooking agencyBooking)
				{
					return GetFromAgencyBooking(agencyBooking, dataContext, parameters);
				}

				if (parent is BillOfLading billOfLading)
				{
					return GetFromBillOfLading(billOfLading, dataContext, parameters);
				}
			}

			return null;
		}

		object GetFromAgencyBooking(AgencyBooking shipment, string dataContext, IDocDataObjectParameters parameters)
		{
			return null;
		}

		object GetFromBillOfLading(BillOfLading billOfLading, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case DataContext.HouseBill:
					return new AgencyHouseBillBuilder(billOfLading, parameters).Build();
			}

			return null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityItinerary : AutoCommodityItinerary, ICusCodeDataTypeSupporter
	{
		public CommodityItinerary(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public CommodityItineraryDataCollection CommodityItineraries
		{
			get
			{
				if (commodityItineraries == null)
				{
					commodityItineraries = new CommodityItineraryDataCollection(this);
					commodityItineraries.Load();
					RegisterEditableChildObject(commodityItineraries);
				}

				return commodityItineraries;
			}
		}
		CommodityItineraryDataCollection commodityItineraries;

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData, typeof(CommodityItineraryData));
			return result;
		}

		#endregion
	}
}

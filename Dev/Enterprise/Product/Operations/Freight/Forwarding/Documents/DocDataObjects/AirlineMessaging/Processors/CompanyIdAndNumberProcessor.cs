using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors;

public class CompanyIdAndNumberProcessor : IUniversalShipmentProcessor
{
	public CompanyIdAndNumberProcessor(ForwardingConsol consol)
	{
		this.consol = consol;
	}
	readonly ForwardingConsol consol;

	public void Process(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment)
	{
		var consolHeader = consol.AWBHeader as ConsolExportAWBHeader;
		if (consolHeader == null || universalShipment == null)
		{
			return;
		}

		var fwbProvider = new FWBMessageDetails(consolHeader);
		var existingShipmentAddInfo = universalShipment.AddInfoCollection;
		var fwbAddInfoList = GetAddInfoList(fwbProvider, existingShipmentAddInfo);

		universalShipment.SetAddInfoCollection(() => fwbAddInfoList);

		if (universalShipment.SubShipmentCollection.IsNullOrEmpty())
		{
			return;
		}

		var consolShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
		foreach (var subShipment in universalShipment.SubShipmentCollection)
		{
			var forwardingShipment = consolShipments.FirstOrDefault(x =>
					string.Equals(x.JS_UniqueConsignRef, subShipment.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key));

			if (forwardingShipment != null)
			{
				var fhlProvider = new FHLMessageDetails(forwardingShipment.AWBHeader);
				var existingSubShipmentAddInfo = subShipment.AddInfoCollection;
				var fhlAddInfoList = GetAddInfoList(fhlProvider, existingSubShipmentAddInfo);

				subShipment.SetAddInfoCollection(() => fhlAddInfoList);
			}
		}
	}

	List<AddInfo> GetAddInfoList(IFBaseMessageDetailsProvider provider, List<AddInfo> existingInfo)
	{
		var infoList = new List<AddInfo>();
		if (existingInfo != null)
		{
			infoList.AddRange(existingInfo);
		}

		InsertAddInfo(infoList, Constants.AddInfoCollectionTypes.ConsignorCompanyIdAndNumber, CompanyIdAndNumberUtils.GetShipperTraderCode(provider));
		InsertAddInfo(infoList, Constants.AddInfoCollectionTypes.ConsigneeCompanyIdAndNumber, CompanyIdAndNumberUtils.GetConsigneeTraderCode(provider));
		InsertAddInfo(infoList, Constants.AddInfoCollectionTypes.AlsoNotifyCompanyIdAndNumber, CompanyIdAndNumberUtils.GetAlsoNotifyTraderCode(provider));

		return infoList;
	}

	void InsertAddInfo(List<AddInfo> infoList, string key, string value)
	{
		if (value.IsNullOrEmpty())
		{
			return;
		}

		foreach (var info in infoList)
		{
			if ((string)info.Key == key)
			{
				info.Value = value;
				return;
			}
		}

		infoList.Add(AddInfo.New(key, value));
	}
}

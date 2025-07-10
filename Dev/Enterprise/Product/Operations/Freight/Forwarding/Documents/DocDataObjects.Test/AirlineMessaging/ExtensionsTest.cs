using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirlineMessaging
{
	public class ExtensionsTest : TestCaseWithFactory
	{
		public void TestConsolToDataObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.IsAWBValuesOverriddenProperty = ZBool.True;

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_ManifestDescriptionOfGoods = "Descriptions";
			awbHeader.EH_ShipperContactName = "MR. Smith";
			awbHeader.EH_ShipperTraderNoType = "VAT";
			awbHeader.EH_ShipperTraderNo = "CHE.123456";

			awbHeader.EH_ConsigneeContactName = "Lady Gaga";
			awbHeader.EH_ConsigneeTraderNoType = "EORI";
			awbHeader.EH_ConsigneeTraderNo = "3145";

			awbHeader.EH_AlsoNotifyContactName = "M Jackson";
			awbHeader.EH_AlsoNotifyTraderNoType = "UST";
			awbHeader.EH_AlsoNotifyTraderNo = "UST12345";

			var dataObject = consol.ToDataObject();
			AssertEquals("Descriptions", dataObject.CarrierDocumentsOverride.AWBHeader.ManifestDescriptionOfGoods);
			AssertEquals("MR. Smith", dataObject.CarrierDocumentsOverride.AWBHeader.Shipper.ContactName);
			AssertEquals("VAT", dataObject.CarrierDocumentsOverride.AWBHeader.Shipper.CompanyIDCode);
			AssertEquals("CHE.123456", dataObject.CarrierDocumentsOverride.AWBHeader.Shipper.CompanyID);

			AssertEquals("Lady Gaga", dataObject.CarrierDocumentsOverride.AWBHeader.Consignee.ContactName);
			AssertEquals("EORI", dataObject.CarrierDocumentsOverride.AWBHeader.Consignee.CompanyIDCode);
			AssertEquals("3145", dataObject.CarrierDocumentsOverride.AWBHeader.Consignee.CompanyID);

			AssertEquals("M Jackson", dataObject.CarrierDocumentsOverride.AWBHeader.AlsoNotify.ContactName);
			AssertEquals("UST", dataObject.CarrierDocumentsOverride.AWBHeader.AlsoNotify.CompanyIDCode);
			AssertEquals("UST12345", dataObject.CarrierDocumentsOverride.AWBHeader.AlsoNotify.CompanyID);

			AssertEquals(dataObject.DataContext.GetType(), typeof(UniversalDataBuss.DataObjects.Universal._2012_11.DataContext));
			var dataContext = dataObject.DataContext as UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
			AssertEquals(dataContext.DataSource.GetType(), typeof(UniversalDataBuss.DataObjects.Universal._2012_11.DataSource));
		}

		public void TestIncludeAddInfo()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolObject = consol.ToDataObject();
			AssertNull(consolObject.AddInfoCollection);

			var addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("SendFWB", "true"),
				new ("SendFHL", "false"),
				new ("IncludeECSD", "true"),
				new ("SendFWBOrFHLToAirlineBasedOnMAWBPrefix", "False"),
				new ("SendFWBNatureAndQuantityOfGoodsType", "TRUE"),
				new ("DefaultIdentifierForCneNfyName", "AB"),
				new ("DefaultIdentifierForCneNfyPhone", "CD"),
				new ("AnyOtherAddInfoKey", "TRUE"),
			};
			consolObject.IncludeAddInfo(addInfoCollection);

			AssertEquals(true, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "SendFWB")).Value));
			AssertEquals(false, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "SendFHL")).Value));
			AssertEquals(true, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "IncludeECSD")).Value));
			AssertEquals(false, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "SendFWBOrFHLToAirlineBasedOnMAWBPrefix")).Value));
			AssertEquals(true, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "SendFWBNatureAndQuantityOfGoodsType")).Value));
			AssertEquals("AB", consolObject.AddInfoCollection.First(x => Equals(x.Key, "DefaultIdentifierForCneNfyName")).Value);
			AssertEquals("CD", consolObject.AddInfoCollection.First(x => Equals(x.Key, "DefaultIdentifierForCneNfyPhone")).Value);
			AssertEquals(true, bool.Parse(consolObject.AddInfoCollection.First(x => Equals(x.Key, "AnyOtherAddInfoKey")).Value));
		}

		public void TestIncludeAddInfoWithEmptyValue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolObject = consol.ToDataObject();
			AssertNull(consolObject.AddInfoCollection);

			var addInfoCollection = new KeyValuePair<string, string>[]
			{
				new ("DefaultIdentifierForCneNfyName", ""),
				new ("DefaultIdentifierForCneNfyPhone", ""),
			};
			consolObject.IncludeAddInfo(addInfoCollection);
			
			AssertEquals("", consolObject.AddInfoCollection.First(x => Equals(x.Key, "DefaultIdentifierForCneNfyName")).Value);
			AssertEquals("", consolObject.AddInfoCollection.First(x => Equals(x.Key, "DefaultIdentifierForCneNfyPhone")).Value);
		}
	}
}

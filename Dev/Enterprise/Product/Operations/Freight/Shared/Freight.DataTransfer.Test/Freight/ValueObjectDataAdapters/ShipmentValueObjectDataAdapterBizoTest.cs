using System;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentValueObjectDataAdapter<CommonShipment>))]
	sealed class ShipmentValueObjectDataAdapterBizoTest : ShipmentValueObjectDataAdapterTest<CommonShipment, CommonConsol>
	{
		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.NoData);
			if (shipment.JS_RX_NKGoodsValueCurr != shipment.JS_RX_NKInsuranceCurrency)
			{
				shipment.JS_RX_NKInsuranceCurrency = shipment.JS_RX_NKGoodsValueCurr;
			}
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyShipment.xml", "EmptyShipment.xml");

			XDocument xmlDoc = XDocument.Load(expectedOutputFilename);
			XNamespace nameSpace = xmlDoc.Root.Name.Namespace;
			XElement dateElement = xmlDoc.Root.Element(nameSpace + "ShipmentDetails")?.Element(nameSpace + "HBLIssueDate");
			if (dateElement != null)
			{
				dateElement.Value = shipment.JS_HouseBillIssueDate.ToString("yyyy-MM-ddTHH:mm:ss");
			}
			xmlDoc.Save(expectedOutputFilename);

			return new BusinessObjectAndExpectedOutputFileName(shipment, expectedOutputFilename, ValidationKind.None, "Empty shipment");
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}

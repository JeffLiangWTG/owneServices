using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class CO2eXmlWriterTest : TestCaseWithFactory
{
	public void TestWriteXML()
	{
		using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
		{
			var shipment = CreateShipmentDO();
			shipment.SubShipmentCollection.Add(CreateShipmentDO());
			shipment.SubShipmentCollection.Add(CreateShipmentDO());

			var transportLegDO = new TransportLeg
			{
				DepartureFrom = new OrganizationAddress(),
				ArrivalAt = new OrganizationAddress()
			};
			shipment.TransportLegCollection.Add(transportLegDO);

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writer = new CO2eXmlWriter();
				writer.WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2012_11);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipment_2012_11.Trim(), result);
				}
			}
		}
	}

	Shipment CreateShipmentDO()
	{
		var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
		result.SetPostCarriageShipmentCollection(() => new List<Shipment>());
		result.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
		result.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
		result.SetPreCarriageShipmentCollection(() => new List<Shipment>());
		return result;
	}

	const string UniversalShipment_2012_11 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PreCarriageShipmentCollection>
    </PreCarriageShipmentCollection>
    <TransportLegCollection>
      <TransportLeg>
        <DepartureFrom>
        </DepartureFrom>
        <ArrivalAt>
        </ArrivalAt>
      </TransportLeg>
    </TransportLegCollection>
    <SubShipmentCollection>
      <SubShipment>
        <PreCarriageShipmentCollection>
        </PreCarriageShipmentCollection>
        <TransportLegCollection>
        </TransportLegCollection>
        <SubShipmentCollection>
        </SubShipmentCollection>
        <PostCarriageShipmentCollection>
        </PostCarriageShipmentCollection>
      </SubShipment>
      <SubShipment>
        <PreCarriageShipmentCollection>
        </PreCarriageShipmentCollection>
        <TransportLegCollection>
        </TransportLegCollection>
        <SubShipmentCollection>
        </SubShipmentCollection>
        <PostCarriageShipmentCollection>
        </PostCarriageShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>
    <PostCarriageShipmentCollection>
    </PostCarriageShipmentCollection>
  </Shipment>
</UniversalShipment>";
}

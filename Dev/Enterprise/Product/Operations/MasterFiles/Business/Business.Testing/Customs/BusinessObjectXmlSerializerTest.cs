using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BusinessObjectXmlSerializerTest : TestCaseWithFactory
	{
		const string CarData = @"
  <Number>BJ-75-FE</Number>
  <SeatsCount>5</SeatsCount>
  <Model>Mazda 626</Model>
  <Kilometers>312054</Kilometers>
  <UID>37c2cf76-7e1b-4601-85b3-89390f81436b</UID>
  <ProductionDate>1998-05-17</ProductionDate>
  <Engine>
    <Data>Q2FyZ29XaXNl</Data>
    <Cylinders>4</Cylinders>
  </Engine>
  <Radio>
    <HasUSB>N</HasUSB>
    <CurrentTime>2013-08-12T09:58:00</CurrentTime>
    <FavouriteStations>
      <Station>
        <Frequency>106.5</Frequency>
      </Station>
      <Station>
        <Frequency>105.9</Frequency>
      </Station>
    </FavouriteStations>
  </Radio>
  <Owners>
    <Human>
      <Name>Anton</Name>
    </Human>
    <Human>
      <Name>Alex</Name>
    </Human>
  </Owners>
";

		public void TestInheritedSchema()
		{
			var xml = @"<Truck>
  <MaxWeight>5000</MaxWeight>" + CarData + "</Truck>";
			var truck = new BigCar(Factory);
			truck.Deserialize(xml);
			AssertMultilineASCIIEquals("XML is the same", xml, truck.Serialize());

			xml = @"<Truck xmlns=""BobNamespace.com"">
  <MaxWeight>5000</MaxWeight>" + CarData + "</Truck>";
			truck = new BigCar(Factory);
			truck.Deserialize(xml, "BobNamespace.com");
			AssertMultilineASCIIEquals("XML is the same", xml, truck.Serialize("BobNamespace.com"));
		}

		public void TestEndToEnd()
		{
			var xml = "<Car>" + CarData + "</Car>";
			var car = new Car(Factory);
			car.Deserialize(xml);
			AssertMultilineASCIIEquals("XML is the same", xml, car.Serialize());

			xml = @"<Car xmlns=""BobNamespace.com"">" + CarData + "</Car>";
			car = new Car(Factory);
			car.Deserialize(xml, "BobNamespace.com");
			AssertMultilineASCIIEquals("XML is the same", xml, car.Serialize("BobNamespace.com"));
		}

		public void TestDoNotSerialiseEmptyZValues()
		{
			var car = new Car(Factory);
			car.Number = "ABC";
			car.SeatsCount = 0;//should not serialise

			var xml = car.Serialize();
			AssertNotContains("<SeatsCount>", xml);
		}

		public void TestSerialiseMultipleBusinessObjects()
		{
			var car1 = new Car(Factory);
			car1.Number = "ABC";

			var car2 = new Car(Factory);
			car2.Number = "DEF";
			car2.SeatsCount = 4;
			var xml = new XmlSerializableNonPersistentBusinessObject[] { car1, car2 }.Serialize("Cars", "");
			AssertEquals("XML is the same", multipleBizObjXml, xml);
		}

		const string multipleBizObjXml = @"<Cars>
  <Car>
    <Number>ABC</Number>
    <Radio>
      <HasUSB>N</HasUSB>
    </Radio>
  </Car>
  <Car>
    <Number>DEF</Number>
    <SeatsCount>4</SeatsCount>
    <Radio>
      <HasUSB>N</HasUSB>
    </Radio>
  </Car>
</Cars>";

		public void TestDeserialiseMultipleBusinessObjects()
		{
			var cars = BusinessObjectXmlSerializer.Deserialize(multipleBizObjXml, () => new Car(Factory)).Cast<Car>();

			AssertEquals(2, cars.Count());
			AssertEquals("ABC", cars.ElementAt(0).Number);
			AssertEquals("DEF", cars.ElementAt(1).Number);
			AssertEquals((byte)4, cars.ElementAt(1).SeatsCount);
		}
	}
}

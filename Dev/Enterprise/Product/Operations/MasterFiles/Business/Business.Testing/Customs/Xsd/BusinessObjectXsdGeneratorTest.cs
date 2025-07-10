using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Xsd.Testing
{
	sealed class BusinessObjectXsdGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateXsd()
		{
			var car = new Car(Factory);
			PopulateCar(car);
			string xsdMarkup = new BusinessObjectXsdGenerator().GenerateXsd(car, 1, "BobNameSpace");
			var schemas = new XmlSchemaSet();
			schemas.Add("BobNameSpace", XmlReader.Create(new StringReader(xsdMarkup)));

			var xmlData = car.Serialize("");
			var doc = XDocument.Load(new StringReader(xmlData));
			var error = new ZStringBuilder();
			doc.Validate(schemas, (o, e) =>
			{
				error.AppendLine(e.Message);
			});
			AssertMultilineASCIIEquals("Error", "", error.ToStringWithNewLineBetweenAppends());

			var bigCar = new BigCar(Factory);
			PopulateCar(bigCar);
			bigCar.MaxWeight = 1400;
			xsdMarkup = new BusinessObjectXsdGenerator().GenerateXsd(bigCar, 1, "BobNameSpace");
			schemas = new XmlSchemaSet();
			schemas.Add("BobNameSpace", XmlReader.Create(new StringReader(xsdMarkup)));
			xmlData = bigCar.Serialize("");

			doc = XDocument.Load(new StringReader(xmlData));
			error = new ZStringBuilder();
			doc.Validate(schemas, (o, e) =>
			{
				error.AppendLine(e.Message);
			});
			AssertMultilineASCIIEquals("Error", "", error.ToStringWithNewLineBetweenAppends());

			var stations = bigCar.Radio.SavedStations;
			xsdMarkup = new BusinessObjectXsdGenerator().GenerateXsd(stations, "Stations", 1, "BobNameSpace");
			schemas = new XmlSchemaSet();
			schemas.Add("BobNameSpace", XmlReader.Create(new StringReader(xsdMarkup)));
			xmlData = stations.OfType<XmlSerializableNonPersistentBusinessObject>().Serialize("Stations", "");

			doc = XDocument.Load(new StringReader(xmlData));
			error = new ZStringBuilder();
			doc.Validate(schemas, (o, e) =>
			{
				error.AppendLine(e.Message);
			});
			AssertMultilineASCIIEquals("Error", "", error.ToStringWithNewLineBetweenAppends());
		}

		public void TestGenerateXsd_Exception()
		{
			NUnit.Framework.Assert.That(
				delegate
				{
					new BusinessObjectXsdGenerator().GenerateXsd(new StringMissingMaxLength(Factory), 1);
				}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "All ZString properties on must have the MaxLengthAttribute applied.", true), "Missing Maxlength Exception");

			NUnit.Framework.Assert.That(
				delegate
				{
					new BusinessObjectXsdGenerator().GenerateXsd(new DecimalMissingDecimalPlaces(Factory), 1);
				}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "All ZDecimal properties on must have the DecimalPlacesAttribute applied.", true), "Missing DecimalPlaces Exception");

			NUnit.Framework.Assert.That(
				delegate
				{
					new BusinessObjectXsdGenerator().GenerateXsd(new DecimalMissingDecimalPrecision(Factory), 1);
				}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "All ZDecimal properties on must have the DecimalPrecisionAttribute applied.", true), "Missing DecimalPrecision Exception");

			var bizObj = new BizObjNullRelatedData(Factory);
			NUnit.Framework.Assert.That(delegate
			{
				new BusinessObjectXsdGenerator().GenerateXsd(bizObj, 1);
			}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "Enterprise.MasterFiles.Business.Xsd.Testing.BusinessObjectXsdGeneratorTest+BizObjNullRelatedData.Bob property must not be null in order to generate the Xsd properly", true), "Bob Is Null");

			bizObj.Bob = new Human(Factory);
			NUnit.Framework.Assert.That(delegate
			{
				new BusinessObjectXsdGenerator().GenerateXsd(bizObj, 1);
			}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "Enterprise.MasterFiles.Business.Xsd.Testing.BusinessObjectXsdGeneratorTest+BizObjNullRelatedData.Stations collection must not be null in order to generate the Xsd properly", true), "Stations Is Null");

			bizObj.Stations = new StationsCollection(Factory);
			NUnit.Framework.Assert.That(delegate
			{
				new BusinessObjectXsdGenerator().GenerateXsd(bizObj, 1);
			}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), "Enterprise.MasterFiles.Business.Xsd.Testing.BusinessObjectXsdGeneratorTest+BizObjNullRelatedData.Stations collection needs to have at least 1 element to be able to generate the Xsd properly", true), "Stations Is Empty");

			bizObj.Stations.AddNew();
			AssertNoExceptionThrown(() => new BusinessObjectXsdGenerator().GenerateXsd(bizObj, 1));
		}

		void PopulateCar(Car car)
		{
			car.Number = "ABC";
			car.SeatsCount = 1;
			car.Model = "Model";
			car.Kilometers = 12;
			car.Guid = ZGuid.Invalid;
			car.ProductionDate = ZDate.BrettsBirthday;

			car.Engine.Data = ZBlob.Empty;
			car.Engine.CylindersCount = (ZShort)10;
			var radioStation = car.Radio.SavedStations.AddNew();
			radioStation.Frequency = 103.2m;
			var normalStation = car.Radio.NormalStations.AddNew();
			normalStation.Frequency = 106.1m;
			car.Radio.HasUSB = ZBool.True;
			car.Radio.CurrentTime = ZDateTime.BrettsBirthday;
			var owner = car.Owners.AddNew();
			owner.Name = "BOB";
			owner.DOB = ZDate.BrettsBirthday;
		}

		public class BizObjNullRelatedData : XmlSerializableNonPersistentBusinessObject
		{
			public BizObjNullRelatedData(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public Human Bob
			{
				get { return bob; }
				set { bob = value; }
			}
			Human bob;

			public StationsCollection Stations
			{
				get { return stations; }
				set { stations = value; }
			}
			StationsCollection stations;
		}

		public class StringMissingMaxLength : XmlSerializableNonPersistentBusinessObject
		{
			public static class Schema
			{
				public const string Name = "Name";
			}

			public StringMissingMaxLength(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZString Name
			{
				get { return name; }
				set { name = value; }
			}
			ZString name;
		}

		public class DecimalMissingDecimalPlaces : XmlSerializableNonPersistentBusinessObject
		{
			public static class Schema
			{
				public const string Number = "Number";
			}

			public DecimalMissingDecimalPlaces(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			[DecimalPrecision(6)]
			public ZDecimal Number
			{
				get { return name; }
				set { name = value; }
			}
			ZDecimal name;
		}

		public class DecimalMissingDecimalPrecision : XmlSerializableNonPersistentBusinessObject
		{
			public static class Schema
			{
				public const string Number = "Number";
			}

			public DecimalMissingDecimalPrecision(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			[DecimalPlaces(1)]
			public ZDecimal Number
			{
				get { return name; }
				set { name = value; }
			}
			ZDecimal name;
		}
	}
}

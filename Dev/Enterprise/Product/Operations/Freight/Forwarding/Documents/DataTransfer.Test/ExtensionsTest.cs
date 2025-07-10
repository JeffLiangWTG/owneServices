using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUNDG = Enterprise.UniversalDataBuss.DataObjects.Universal.UNDG;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	class ExtensionsTest : DataObjectWriterTest
	{
		public void TestToUXmlUNDG_NetExplosiveWeight()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new AdvancedLogisticsPortOrderDataObjectWriter(manager);

			var universalShipment = new UniversalShipment(manager.WriterStrategy);
			var universalPackingLine = new UniversalPackingLine(manager.WriterStrategy);

			universalShipment.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { universalPackingLine });

			var dangerousGood = new DangerousGood();
			universalPackingLine.SetUNDGCollection(() => new List<UniversalUNDG>() { dangerousGood.ToUXmlUNDG(DefaultDataObjectWriterStrategy.Instance, false) });

			AssertNotInUXml(universalShipment, "NetExplosiveWeight");
			AssertNotInUXml(universalShipment, "NetExplosiveWeightUQ");

			dangerousGood.NetExplosiveWeight = new Measurement
			{
				Value = 1,
				Unit = new DummyCodeDescription()
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = Core.Constants.Weight.GetDescription(Core.Constants.Weight.Kilograms, Core.Constants.PluralState.NonPlural)
				}
			};

			universalPackingLine.SetUNDGCollection(() => new List<UniversalUNDG>() { dangerousGood.ToUXmlUNDG(DefaultDataObjectWriterStrategy.Instance, false) });

			AssertInUXml(universalShipment, "<NetExplosiveWeight>1</NetExplosiveWeight>");
			AssertInUXml(universalShipment, @"<NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>");
		}

		public void Test_GetUnknownAddress_Return_ExpectedOrganizationAddress()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var address = DocAddressType.Manufacturer.GetUnknownAddress(manager.WriterStrategy);

			AssertNotNull(address);
			AssertType<OrganizationAddress>(address);
			AssertEquals("AddressType", address.AddressType, "Manufacturer");
			AssertEquals("CompanyName", address.CompanyName, "UNKNOWN");
		}

		public void TestGetCountriesOfRouting()
		{
			AssertEquals("CA|US|SG|DE|US|DE|NL|CA", Extensions.GetCountriesOfRouting(CreateTransports(), "CA", "CA"));
			AssertEquals("CN|CA|US|SG|DE|US|DE|NL|CA", Extensions.GetCountriesOfRouting(CreateTransports(), "CN", "CA"));
			AssertEquals("CA|US|SG|DE|US|DE|NL|CA|US", Extensions.GetCountriesOfRouting(CreateTransports(), "CA", "US"));
			AssertEquals("CN|CA|US|SG|DE|US|DE|NL|CA|US", Extensions.GetCountriesOfRouting(CreateTransports(), "CN", "US"));
		}

		#region implementation
		ITransports CreateTransports()
		{
			var transports = new List<Transport>();

			var transport1 = new TransportLeg()
			{
				LegOrder = 1,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "CAACT", Name = "Acton" },
				PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles"	}
			};
			transports.Add(Transport.Create(Context, transport1));

			var transport2 = new TransportLeg()
			{
				LegOrder = 2,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "USLAX", Name = "Los Angeles"	},
				PortOfDischarge = new UNLOCO() { Code = "SGCLE", Name = "Clementi" }
			};
			transports.Add(Transport.Create(Context, transport2));

			var transport3 = new TransportLeg()
			{
				LegOrder = 3,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "SGCLE", Name = "Clementi" },
				PortOfDischarge = new UNLOCO() { Code = "DEHAM", Name = "Hamburg" }
			};
			transports.Add(Transport.Create(Context, transport3));

			var transport4 = new TransportLeg()
			{
				LegOrder = 4,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "DEHAM", Name = "Hamburg" },
				PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" }
			};
			transports.Add(Transport.Create(Context, transport4));

			var transport5 = new TransportLeg()
			{
				LegOrder = 5,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" },
				PortOfDischarge = new UNLOCO() { Code = "DEHAM", Name = "Hamburg" }
			};
			transports.Add(Transport.Create(Context, transport5));

			var transport6 = new TransportLeg()
			{
				LegOrder = 6,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "DEHAM", Name = "Hamburg" },
				PortOfDischarge = new UNLOCO() { Code = "NLRTM", Name = "Rotterdam" }
			};
			transports.Add(Transport.Create(Context, transport6));

			var transport7 = new TransportLeg()
			{
				LegOrder = 7,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "NLRTM", Name = "Rotterdam"	},
				PortOfDischarge = new UNLOCO() { Code = "NLAMS", Name = "Amsterdam"	}
			};
			transports.Add(Transport.Create(Context, transport7));

			var transport8 = new TransportLeg()
			{
				LegOrder = 8,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "NLAMS", Name = "Amsterdam" },
			};
			transports.Add(Transport.Create(Context, transport8));

			var transport9 = new TransportLeg()
			{
				LegOrder = 9,
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "NLAMS", Name = "Amsterdam" },
				PortOfDischarge = new UNLOCO() { Code = "CAACT", Name = "Acton" }
			};
			transports.Add(Transport.Create(Context, transport9));

			return Transports.Create(transports);
		}

		#endregion
	}
}

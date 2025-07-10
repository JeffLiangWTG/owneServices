using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CO2eLegBasedResponseImporterTest : TestCaseWithFactory
	{
		public void TestImportGreenHouseGasEmission_ForwardingShipment()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(8000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(8000m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(2000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());

			AssertGHGEvent(shipment.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Sailing.Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertNull(shipment.TransportsIncludingRelated[1].Sailing);

			AssertEquals(CO2eStatusList.Codes.Current, ((ICO2eProvider)shipment).GetCO2eStatus());
			foreach (Transport leg in shipment.TransportsIncludingRelated)
			{
				AssertEquals(CO2eStatusList.Codes.Current, leg.GetCO2eStatus());
				if (leg.JW_IsLinked)
				{
					AssertEquals(CO2eStatusList.Codes.Current, leg.Sailing.GetCO2eStatus());
				}
			}
		}

		public void TestImportGreenHouseGasEmission_WhenPreviousCO2eValue()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			var previousCO2eValue = (TotalCO2e: 0m, Transports: Enumerable.Empty<BusinessObject>());

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName, previousCO2eValue);

			AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(8000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(8000m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(2000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());

			AssertGHGEvent(shipment.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Sailing.Logs, "|NEW=8000|OLD=NA|TYP=Updated");

			previousCO2eValue = (10000m, shipment.TransportsIncludingRelated);
			dataObject.GreenhouseGasEmission.CO2e = 5000;
			dataObject.TransportLegCollection[0].GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 2000,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};
			dataObject.TransportLegCollection[1].GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 3000,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName, previousCO2eValue);

			AssertGHGEvent(shipment.Logs, "|NEW=5000|OLD=10000|TYP=Updated", 2);
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=2000|OLD=8000|TYP=Updated", 2);
			AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=3000|OLD=2000|TYP=Updated", 2);
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipmentWhenCO2EmissionIsZero()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectWithZeroGreenhouseGasEmissionForLegs();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());

			AssertGHGEvent(shipment.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=ERR|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=ERR|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Sailing.Logs, "|NEW=ERR|OLD=NA|TYP=Updated");
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipmentWhenTheOnlyLegDoesNotHaveLoadAndDischarge()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			((ICO2eProvider)shipment).SetCO2eStatus(CO2eStatusList.Codes.Pending);
			shipment.Transports.RemoveAll();
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = "";
			transport.SetCO2eStatus(CO2eStatusList.Codes.Pending);

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.SetTransportLegCollection(() => null);

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertGHGEvent(shipment.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertEquals(CO2eStatusList.Codes.Current, ((ICO2eProvider)shipment).GetCO2eStatus());

			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0, shipment.TransportsIncludingRelated[0].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_WithVirtualLegs()
		{
			var shipmentWithIncompleteLegs = CO2eTestHelper.CreateForwardingShipmentWithIncompleteLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectWithVirtualLegs();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipmentWithIncompleteLegs, shipmentWithIncompleteLegs.HumanReadableName);

			AssertEquals("Shipment CO2e includes virtual legs CO2e", 12500m, ((ICO2eProvider)shipmentWithIncompleteLegs).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipmentWithIncompleteLegs).GetCO2eDistanceInKM());
			AssertGHGEvent(shipmentWithIncompleteLegs.Logs, "|NEW=12500|OLD=NA|TYP=Updated");

			AssertEquals(3, shipmentWithIncompleteLegs.TransportsIncludingRelated.Count);
			AssertCO2eShipmentLeg(shipmentWithIncompleteLegs.TransportsIncludingRelated[0], true, 2000);
			AssertCO2eShipmentLeg(shipmentWithIncompleteLegs.TransportsIncludingRelated[1]);
			AssertCO2eShipmentLeg(shipmentWithIncompleteLegs.TransportsIncludingRelated[2]);
		}

		[ExpectNoExceptions]
		public void TestImportGreenHouseGasEmission_ForwardingShipment_TotalCO2eValueExceedsLimit()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTonne = 98000m;
			dataObject.GreenhouseGasEmission.CO2e = 1000000000000000m;

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertStatusRejected(shipment);
			AssertGHGEvent(shipment.Logs, $"|RES=Total CO2e 1000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded|TYP=Rejected");
			AssertEquals($"Warning - Total CO2e 1000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded", logger.Logs);
		}

		[ExpectNoExceptions]
		public void TestImportGreenHouseGasEmission_ForwardingShipment_CO2ePerTonneValueExceedsLimit()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.GreenhouseGasEmission.CO2ePerTonne = 100000000m;
			dataObject.GreenhouseGasEmission.CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" };

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertStatusRejected(shipment);
			AssertGHGEvent(shipment.Logs, "|RES=Total CO2e/tonne 100000000 is greater than maximum limit 99999.9999999, it will be discarded|TYP=Rejected");
			AssertEquals("Warning - Total CO2e/tonne 100000000 is greater than maximum limit 99999.9999999, it will be discarded", logger.Logs);
		}

		[ExpectNoExceptions]
		public void TestImportGreenHouseGasEmission_ForwardingShipment_CO2ePerTEUValueExceedsLimit()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.GreenhouseGasEmission.CO2ePerTEU = 100000000m;
			dataObject.GreenhouseGasEmission.CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" };

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertStatusRejected(shipment);
			AssertGHGEvent(shipment.Logs, "|RES=Total CO2e/teu 100000000 is greater than maximum limit 99999.9999999, it will be discarded|TYP=Rejected");
			AssertEquals("Warning - Total CO2e/teu 100000000 is greater than maximum limit 99999.9999999, it will be discarded", logger.Logs);
		}

		[ExpectNoExceptions]
		public void TestImportGreenHouseGasEmission_ForwardingShipment_TransportLegCO2eValueExceedsLimit()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTonne = 100000m;

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertStatusRejected(shipment);
			AssertGHGEvent(shipment.Logs, "|RES=Transport Leg CO2e/tonne 100000000 is greater than maximum limit 99999.9999999, it will be discarded|TYP=Rejected");
			AssertEquals("Warning - Transport Leg CO2e/tonne 100000000 is greater than maximum limit 99999.9999999, it will be discarded", logger.Logs);
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_WithCO2eDistanceInKM()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject_WithCO2eDistanceInKM();

			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			AssertEquals(10_000m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
			AssertEquals(6_000m, shipment.TransportsIncludingRelated[0].GetCO2eDistanceInKM());
			AssertEquals(4_000m, shipment.TransportsIncludingRelated[1].GetCO2eDistanceInKM());
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_RequireTEU()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Arrange
				var shipment = CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject_WithTEU();

				// Act
				var importer = new CO2eLegBasedResponseImporter(Factory, new TestErrorLogger());
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertEquals(0m, ((ICO2eProvider)shipment).GetCO2eDistanceInKM());
				AssertEquals(8000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
				AssertEquals(8000m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
				AssertEquals(2000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
				AssertEquals(150m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
				AssertEquals(150m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTEUInKg());
				AssertEquals(50m, shipment.TransportsIncludingRelated[1].GetCO2ePerTEUInKg());

				AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
				AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[0].Sailing.GetCO2eStatus());
				AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
				AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[1].GetCO2eStatus());
				AssertEquals(CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[1].GetCO2eStatus());
			}
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_RequireTEU_TotalCO2eValueExceedsLimit()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject_WithTEU();
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEU = 98000m;
			dataObject.GreenhouseGasEmission.CO2e = 1000000000000000m;

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTEUInKg());
			AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertStatusRejected(shipment);
			AssertGHGEvent(shipment.Logs, $"|RES=Total CO2e 1000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded|TYP=Rejected");
			AssertEquals("Warning - Total CO2e 1000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded", logger.Logs);
		}

		[ExpectNoExceptions]
		public void TestImportGreenHouseGasEmission_ForwardingShipment_RequireTEU_TransportLegCO2eTEUValueExceedsLimit()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Arrange
				var shipment = CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject_WithTEU();
				dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEU = 100000m;

				// Act
				var logger = new TestErrorLogger();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger);
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals(0m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
				AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTEUInKg());
				AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
				AssertStatusRejected(shipment);
				AssertGHGEvent(shipment.Logs, "|RES=Transport Leg CO2e/teu 100000 is greater than maximum limit 99999.9999999, it will be discarded|TYP=Rejected");
				AssertEquals("Warning - Transport Leg CO2e/teu 100000 is greater than maximum limit 99999.9999999, it will be discarded", logger.Logs);
			}
		}

		void AssertStatusRejected(CommonShipment shipment)
		{
			var parent = shipment as ICO2eLegBasedSupporter;
			AssertEquals(CO2eStatusList.Codes.Rejected, parent.GetCO2eStatus());
			foreach (Transport leg in parent.Legs)
			{
				AssertEquals(CO2eStatusList.Codes.Rejected, leg.GetCO2eStatus());
				if (leg.JW_IsLinked && leg.Sailing != null)
				{
					AssertEquals(CO2eStatusList.Codes.Rejected, leg.Sailing.GetCO2eStatus());
				}
			}
		}

		public void TestIsGreenhouseGasEmissionValid()
		{
			// Arrange
			var ghg1 = new GreenhouseGasEmission
			{
				CO2e = 1000000000000000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			var ghg2 = new GreenhouseGasEmission
			{
				CO2e = 10m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
				CO2ePerTonne = 100000000m,
				CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
			};

			var ghg3 = new GreenhouseGasEmission
			{
				CO2e = 10m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
				CO2ePerTEU = 100000000m,
				CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
			};

			var reason = string.Empty;
			var importer = new CO2eLegBasedResponseImporter(Factory, new TestErrorLogger());

			// Act & Assert
			AssertGHGInvalid(ghg1, "Total CO2e 1000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded");
			AssertGHGInvalid(ghg2, "name CO2e/tonne 100000000 is greater than maximum limit 99999.9999999, it will be discarded");
			AssertGHGInvalid(ghg3, "name CO2e/teu 100000000 is greater than maximum limit 99999.9999999, it will be discarded");

			void AssertGHGInvalid(GreenhouseGasEmission ghg, string expectedReason)
			{
				Assert(!importer.IsGreenhouseGasEmissionValid(ghg, "name", out reason));
				AssertEquals(expectedReason, reason);
			}
		}

		public void TestImportGreenHouseGasEmission_DoNotOverrideExistingCO2ePerTEU()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			shipment.TransportsIncludingRelated[0].SetCO2ePerTEUInKg(100m);
			shipment.TransportsIncludingRelated[1].SetCO2ePerTEUInKg(200m);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			AssertEquals(10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(8000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			AssertEquals(2000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			AssertEquals("CO2ePerTEU remains", 100m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
			AssertEquals("CO2ePerTEU remains", 200m, shipment.TransportsIncludingRelated[1].GetCO2ePerTEUInKg());
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_EmptyContainer()
		{
			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
				Factory.Save();
				var consol = shipment.Consols[0];
				var container = consol.Containers[0];
				var shipmentDataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject_WithTEU();

				var consolDataObject = CreateSubShipmentForConsol(consol);
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consolDataObject });

				var logger = new TestErrorLogger();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger);
				importer.ImportGreenHouseGasEmission(shipmentDataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				var containerProvider = container as ICO2eEmptyContainerProvider;
				AssertNotNull(containerProvider);
				AssertEquals(2000m, containerProvider.GetCO2ePerTEUInKg("PIC"));
				AssertEquals(CO2eStatusList.Codes.Current, containerProvider.GetCO2eStatus("PIC"));
				AssertEquals(container.JC_Calc_TEUCount * 2000, containerProvider.GetTotalCO2e("PIC"));

				var note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).SingleOrDefault();
				AssertNotNull("Emissions log note exists", note);
				AssertContains("Empty Container Pickup/Return:", note.ST_NoteText);
				AssertContains($"Consol number: {consol.JK_UniqueConsignRef}", note.ST_NoteText);
				AssertContains($"Container number: {container.JC_ContainerNum}", note.ST_NoteText);
				AssertContains("Pickup Transport Mode:", note.ST_NoteText);
			}
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_SubShipments()
		{
			// Arrange
			var rc_20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var rc_40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var consol1 = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory, "AUSYD", "VNVNH", "SGSIN", "SEA");
			var consol1Container = consol1.Containers.AddNew();
			consol1Container.JC_RC = rc_20GP_PK;
			var consol2 = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory, "VNVNH", "JPTYO", "CNSHA", "SEA");
			var consol2Container = consol2.Containers.AddNew();
			consol2Container.JC_RC = rc_40GP_PK;

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "JPTYO";
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			var packLine = shipment.OuterPackLines[0];
			packLine.JL_ActualWeight = 1m;
			packLine.JL_ActualWeightUQ = "T";
			packLine.SetContainer(consol1, consol1Container);
			packLine.SetContainer(consol2, consol2Container);
			AssertEquals(1m, consol1Container.JC_Calc_TEUCount);
			AssertEquals(2m, consol2Container.JC_Calc_TEUCount);

			Factory.Save();

			var shipmentDataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			shipmentDataObject.PortOfDischarge = new UNLOCO { Code = "JPTYO" };
			shipmentDataObject.SetTransportLegCollection(() => null);
			shipmentDataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 6000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
			};

			var consol1DataObject = CreateSubShipmentForConsol(consol1);
			var consol2DataObject = CreateSubShipmentForConsol(consol2);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consol1DataObject, consol2DataObject });

			// Act
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new TestErrorLogger();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger);
				importer.ImportGreenHouseGasEmission(shipmentDataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals("Shipment TotalCO2e", 18000m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertEquals("Shipment CO2e Status", CO2eStatusList.Codes.Current, ((ICO2eProvider)shipment).GetCO2eStatus());

				AssertEquals("Leg 1", 1000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
				AssertEquals("Leg 2", 1000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTEUInKg());
				AssertEquals("Leg 3", 1000m, shipment.TransportsIncludingRelated[2].GetCO2ePerTEUInKg());
				AssertEquals("Leg 4", 1000m, shipment.TransportsIncludingRelated[3].GetCO2ePerTEUInKg());

				AssertEquals("Container 1 Pickup", 2000m, ((ICO2eProvider)consol1Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyPickup));
				AssertEquals("Container 1 Return", 2000m, ((ICO2eProvider)consol1Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyReturn));
				AssertEquals("Container 2 Pickup", 2000m, ((ICO2eProvider)consol2Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyPickup));
				AssertEquals("Container 2 Return", 2000m, ((ICO2eProvider)consol2Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyReturn));

				AssertGHGEvent(shipment.Logs, "|NEW=18000.00|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=1000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=1000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[2].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[3].Logs, "|NEW=2000|OLD=NA|TYP=Updated");

				var note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).SingleOrDefault();
				AssertNotNull("Emissions log note exists", note);
				AssertContains("Empty Container Pickup/Return:", note.ST_NoteText);

				AssertEquals(1, Regex.Matches(note.ST_NoteText, consol1.JK_UniqueConsignRef).Count);
				AssertEquals(1, Regex.Matches(note.ST_NoteText, consol2.JK_UniqueConsignRef).Count);

				AssertContains($"Container number: {consol1Container.JC_ContainerNum}", note.ST_NoteText);
				AssertContains($"Container number: {consol2Container.JC_ContainerNum}", note.ST_NoteText);
			}
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_SubShipments_UseTotal()
		{
			// Arrange
			var rc_20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var rc_40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var consol1 = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory, "AUSYD", "VNVNH", "SGSIN", "SEA");
			var consol1Container = consol1.Containers.AddNew();
			consol1Container.JC_RC = rc_20GP_PK;
			var consol2 = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory, "VNVNH", "JPTYO", "CNSHA", "SEA");
			consol2.JK_ConsolMode = "LCL";
			var consol2Container = consol2.Containers.AddNew();
			consol2Container.JC_RC = rc_40GP_PK;

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "JPTYO";
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			var packLine = shipment.OuterPackLines[0];
			packLine.JL_ActualWeight = 1m;
			packLine.JL_ActualWeightUQ = "T";
			packLine.SetContainer(consol1, consol1Container);
			packLine.SetContainer(consol2, consol2Container);
			AssertEquals(1m, consol1Container.JC_Calc_TEUCount);
			AssertEquals(2m, consol2Container.JC_Calc_TEUCount);

			Factory.Save();

			var shipmentDataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			shipmentDataObject.PortOfDischarge = new UNLOCO { Code = "JPTYO" };
			shipmentDataObject.SetTransportLegCollection(() => null);
			shipmentDataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 20000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
			};

			var consol1DataObject = CreateSubShipmentForConsol(consol1);
			var consol2DataObject = CreateSubShipmentForConsol(consol2);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consol1DataObject, consol2DataObject });

			// Act
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new TestErrorLogger();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger);
				importer.ImportGreenHouseGasEmission(shipmentDataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals("Shipment TotalCO2e", 32000m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertEquals("Shipment CO2e Status", CO2eStatusList.Codes.Current, ((ICO2eProvider)shipment).GetCO2eStatus());

				AssertEquals("Leg 1", 1000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTEUInKg());
				AssertEquals("Leg 2", 1000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTEUInKg());
				AssertEquals("Leg 3", 1000m, shipment.TransportsIncludingRelated[2].GetCO2ePerTEUInKg());
				AssertEquals("Leg 4", 1000m, shipment.TransportsIncludingRelated[3].GetCO2ePerTEUInKg());

				AssertEquals("Container 1 Pickup", 2000m, ((ICO2eProvider)consol1Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyPickup));
				AssertEquals("Container 1 Return", 2000m, ((ICO2eProvider)consol1Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyReturn));
				AssertEquals("Container 2 Pickup", 2000m, ((ICO2eProvider)consol2Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyPickup));
				AssertEquals("Container 2 Return", 2000m, ((ICO2eProvider)consol2Container).GetCO2ePerTEUInKg(CO2eTypes.EmptyReturn));

				AssertGHGEvent(shipment.Logs, "|NEW=32000.00|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=1000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=1000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[2].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[3].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			}
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_TransportBookingsHaveCO2e()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				var dtbBookingPIC = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking((IDtbBookingParent)shipment, "PIC", Factory);
				dtbBookingPIC.SetTotalCO2e(100m);
				dtbBookingPIC.SetCO2eStatus(CO2eStatusList.Codes.Current);
				var dtbBookingDLV = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking((IDtbBookingParent)shipment, "DLV", Factory);
				dtbBookingDLV.SetTotalCO2e(200m);
				dtbBookingDLV.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				var logger = new Mock<IXmlImportLogger>();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);

				// Act
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals("Shipment TotalCO2e from the response plus TB emissions", 10300m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertGHGEvent(shipment.Logs, "|NEW=10300|OLD=NA|TYP=Updated");
				AssertEquals(CO2eStatusList.Codes.Current, ((ICO2eProvider)shipment).GetCO2eStatus());
			}
		}

		public void TestImportGreenHouseGasEmission_ForwardingShipment_TransportBookingsHaveNoCO2e()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				var dtbBookingPIC = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking((IDtbBookingParent)shipment, "PIC", Factory);
				dtbBookingPIC.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				var dtbBookingDLV = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking((IDtbBookingParent)shipment, "DLV", Factory);
				dtbBookingDLV.SetTotalCO2e(200m);
				dtbBookingDLV.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				var logger = new Mock<IXmlImportLogger>();
				var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);

				// Act
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

				// Assert
				AssertEquals("Shipment TotalCO2e is set from the response", 10000m, ((ICO2eProvider)shipment).GetTotalCO2e());
				AssertEquals("No GHG log yet", 0, shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
				AssertEquals("Shipment CO2eStatus remains Pending", CO2eStatusList.Codes.Pending, ((ICO2eProvider)shipment).GetCO2eStatus());
			}
		}

		public void TestImportGreenHouseGasEmission_RoundsToCorrectFormatInLogs()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.GreenhouseGasEmission.CO2e = 10000.123456789m;
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2e = 10000.123456789m;

			// Act
			var logger = new TestErrorLogger();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger);
			importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName);

			// Assert
			AssertEquals(10000.1234568m, ((ICO2eProvider)shipment).GetTotalCO2e());
			AssertEquals(10000.1234568m, shipment.TransportsIncludingRelated[0].GetTotalCO2e());
			AssertGHGEvent(shipment.Logs, "|NEW=10000.1234568|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=10000.1234568|OLD=NA|TYP=Updated");
		}

		Shipment CreateSubShipmentForConsol(CommonConsol consol)
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = new DataContext();
			dataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_UniqueConsignRef);
			dataObject.SetEmptyContainerCollection(() => new DataObjectList<WeightData>()
			{
				new ()
				{
					ContainerJobID = consol.Containers[0].JC_ContainerJobID,
					EmptyPickup = new EmptyContainerAddress
					{
						GreenhouseGasEmission = new GreenhouseGasEmission
						{
							CO2ePerTEU = 2000,
							CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
						}
					},
					EmptyReturn = new EmptyContainerAddress()
					{
						GreenhouseGasEmission = new GreenhouseGasEmission
						{
							CO2ePerTEU = 2000,
							CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
						}
					}
				}
			});
			var legs = new DataObjectList<TransportLeg>();
			foreach (Transport transport in consol.Transports)
			{
				var totalCO2e = consol.JK_ConsolMode == "LCL" ? 8000 * consol.JK_Calc_ActualVolumeWeight : 1000 * consol.JK_Calc_TEUCount;
				legs.Add(new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = transport.JW_RL_NKLoadPort },
					PortOfDischarge = new UNLOCO { Code = transport.JW_RL_NKDiscPort },
					TransportMode = new TransportModeConverter().ToEnumValue(transport.JW_TransportMode),
					VoyageFlightNo = transport.JW_VoyageFlight,
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = totalCO2e,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 8,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "T" },
						CO2ePerTEU = 1000,
						CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
					},
					LegOrder = 0
				});
			}
			dataObject.SetTransportLegCollection(() => legs);

			return dataObject;
		}

		void AssertCO2eShipmentLeg(Transport leg, bool expectedPopulated = false, decimal expectedCO2eInKg = 0m)
		{
			if (expectedPopulated)
			{
				AssertEquals(expectedCO2eInKg, leg.GetCO2ePerTonneInKg());
				AssertGHGEvent(leg.Logs, $@"|NEW={expectedCO2eInKg.ToString(CultureInfo.InvariantCulture)}|OLD=NA|TYP=Updated");
			}
			else
			{
				AssertEquals(0m, leg.GetCO2ePerTonneInKg());
				AssertEquals(0, leg.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			}

			AssertEquals(CO2eStatusList.Codes.Current, leg.GetCO2eStatus());
		}

		void AssertGHGEvent(Logs logs, string reference, int logCount = 1)
		{
			AssertEquals("New GHG event created", logCount, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		public void TestImportGreenHouseGasEmission_AddEmissionsCalculationNoteForShipment()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			ITopLevelDataObject dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);

			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)shipment, shipment.HumanReadableName, (100m, []));
				var note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNotNull("Emissions Calculation Log note exists", note);
				AssertContains("Previous CO2e value: 100 kg", note.ST_NoteText);
				AssertContains("New CO2e value: 10000 kg", note.ST_NoteText);
			}
		}

		public void TestImportGreenHouseGasEmission_DoNotAddEmissionsCalculationNoteForConsol()
		{
			var consol = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);
			ITopLevelDataObject dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new CO2eLegBasedResponseImporter(Factory, logger.Object);

			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportGreenHouseGasEmission(dataObject, (ICO2eLegBasedSupporter)consol, consol.HumanReadableName);
				var note = consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNull("Emissions Calculation Log note does not exist", note);
			}
		}
	}
}

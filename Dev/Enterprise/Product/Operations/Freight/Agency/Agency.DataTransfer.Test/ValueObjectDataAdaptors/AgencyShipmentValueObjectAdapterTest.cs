using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.SDF;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyShipmentValueObjectDataAdapter<AgencyShipment>))]
	internal class AgencyShipmentValueObjectAdapterTest : ValueObjectDataAdapterResourceAgnosticTest<AgencyShipment, Xsd.AgencyBillOfLading>
	{
		public void TestDefaultWeightVolumeUnit_FCL()
		{
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				CargoType = Xsd.ContainerMode.FCL,
				PackageSummary = new Xsd.Package()
				{
					Weight = new Xsd.DimensionValue()
					{ DimensionType = "XX", Value = 15, },
					Volume = new Xsd.DimensionValue()
					{ DimensionType = "XX", Value = 15, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment;
			shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals(Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(Constants.Weight.Tonnes, shipment.JS_UnitOfWeight);
			bill.BillNumber += "X";
			bill.Packages.Add(new Xsd.Package()
			{
				Weight = new Xsd.DimensionValue()
				{ DimensionType = Constants.Weight.Pounds, Value = 10 },
				Volume = new Xsd.DimensionValue()
				{ DimensionType = Constants.Volume.CubicFeet, Value = 10 },
			});
			shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals(Constants.Volume.CubicFeet, shipment.JS_UnitOfVolume);
			AssertEquals(Constants.Weight.Pounds, shipment.JS_UnitOfWeight);
		}

		public void TestDefaultWeightVolumeUnit_NonFCL()
		{
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				CargoType = Xsd.ContainerMode.BBK,
				PackageSummary = new Xsd.Package()
				{
					Weight = new Xsd.DimensionValue()
					{ DimensionType = "XX", Value = 15, },
					Volume = new Xsd.DimensionValue()
					{ DimensionType = "XX", Value = 15, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment;
			shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals("", shipment.JS_UnitOfVolume);
			AssertEquals("", shipment.JS_UnitOfWeight);
			bill.BillNumber += "X";
			bill.Packages.Add(new Xsd.Package()
			{
				Weight = new Xsd.DimensionValue()
				{ DimensionType = Constants.Weight.Pounds, Value = 10 },
				Volume = new Xsd.DimensionValue()
				{ DimensionType = Constants.Volume.CubicFeet, Value = 10 },
			});
			shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals("", shipment.JS_UnitOfVolume);
			AssertEquals("", shipment.JS_UnitOfWeight);
		}

		public void TestExportTopLevelPacks()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;
				var container1 = shipment.ShippingContainers.AddNew();
				container1.JC_ContainerNum = "HONDA";
				container1.JC_ContainerCount = 10;
				container1.JC_Description = "HONDA CRV";
				container1.JC_MarksAndNumbers = "*LE 4WD";
				container1.JC_RH_NKContainerCommodityCode = "AAA";
				container1.JC_TotalUnitOfMeasure = Constants.Length.Metres;
				container1.JC_TotalLength = 1.1;
				container1.JC_TotalHeight = 1.2;
				container1.JC_TotalWidth = 1.3;
				container1.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
				container1.JC_GrossVolume = 10m;
				container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				container1.JC_GrossWeight = 1000m;
				var dangerousGoods = container1.UNDGs.AddNew();
				dangerousGoods.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				dangerousGoods.DI_DGWeight = 10m;
				dangerousGoods.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
				dangerousGoods.DI_DGVolume = 3m;
				dangerousGoods.DI_UnitOfVolume = Core.Constants.Volume.CubicFeet;
				var container2 = shipment.ShippingContainers.AddNew();
				AssertEquals("Precondition", 2, shipment.ShippingContainers.Count);
				var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
				var context = new ValueObjectExportContext(new NotificationBuffer());
				Xsd.AgencyBillOfLading bill = adapter.ExportToValueObject(shipment, context);
				AssertEquals("No containers should be exported", 0, bill.Containers.Count);
				AssertEquals("Top level packs should be exported as packages", 2, bill.Packages.Count);
				CombineAssertions(() =>
				{
					var valuePackage1 = bill.Packages[0];
					AssertEquals("HONDA", valuePackage1.RefNumber);
					AssertEquals(10, valuePackage1.NumberOfPacks);
					AssertEquals("HONDA CRV", valuePackage1.GoodsDescription);
					AssertEquals("*LE 4WD", valuePackage1.MarksAndNumbers);
					AssertEquals("AAA", valuePackage1.CommodityCode);
					AssertEquals(Constants.Volume.CubicMetres, valuePackage1.Volume.DimensionType);
					AssertEquals(10m, valuePackage1.Volume.Value);
					AssertEquals(Constants.Weight.Kilograms, valuePackage1.Weight.DimensionType);
					AssertEquals(1000m, valuePackage1.Weight.Value);
					AssertEquals(Constants.Length.Metres, valuePackage1.Length.DimensionType);
					AssertEquals(1.1m, valuePackage1.Length.Value);
					AssertEquals(Constants.Length.Metres, valuePackage1.Height.DimensionType);
					AssertEquals(1.2m, valuePackage1.Height.Value);
					AssertEquals(Constants.Length.Metres, valuePackage1.Width.DimensionType);
					AssertEquals(1.3m, valuePackage1.Width.Value);
					AssertEquals("dangerous goods exported", 1, valuePackage1.DangerousGoods.Count);
					AssertEquals(10m, valuePackage1.DangerousGoods[0].Weight.Value);
					AssertEquals(Core.Constants.Weight.Kilograms, valuePackage1.DangerousGoods[0].Weight.DimensionType);
					AssertEquals(3m, valuePackage1.DangerousGoods[0].Volume.Value);
					AssertEquals(Core.Constants.Volume.CubicFeet, valuePackage1.DangerousGoods[0].Volume.DimensionType);
				});
			}
		}

		public void TestImportDecimals_OutOfRange()
		{
			var outOfSqlRangeDecimal = 9876543210.1M;
			var netWeight = 1000M;
			var expectedValue = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").RC_TareWeight;
			var container = new Xsd.Container();
			container.ContainerType = new Xsd.ContainerType()
			{ ContainerCode = "20GP", ISOCode = "22G0" };
			container.AirVentFlow = outOfSqlRangeDecimal;
			container.SetPointTemperature = outOfSqlRangeDecimal;
			container.GrossWeight.Value = outOfSqlRangeDecimal;
			container.Weight = outOfSqlRangeDecimal;
			container.NetWeight.Value = netWeight;
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{ BillNumber = "blaticus", Containers = { container }, };
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			var shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertContains("Value overflow error", buffer.AsString);
			var containers = shipment.BookedContainers.ToArray<AgencyShipmentContainer>();
			AssertEquals(1, containers.Length);
			AssertEquals("JC_AirVentFlow", 0M, containers[0].JC_AirVentFlow);
			AssertEquals("JC_SetPointTemp", 0M, containers[0].JC_SetPointTemp);
			AssertEquals("JC_GrossWeight", expectedValue, containers[0].JC_GrossWeight);
			AssertEquals("JC_TareWeight", expectedValue, containers[0].JC_TareWeight);
			AssertEquals("JC_Calc_NetWeight", 0M, containers[0].JC_Calc_NetWeight);
			var grossWeight = 2000M;
			container.GrossWeight.Value = grossWeight;
			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			containers = shipment.BookedContainers.ToArray<AgencyShipmentContainer>();
			AssertEquals("JC_GrossWeight", grossWeight, containers[0].JC_GrossWeight);
			AssertEquals("JC_TareWeight", grossWeight - netWeight, containers[0].JC_TareWeight);
			AssertEquals("JC_Calc_NetWeight", netWeight, containers[0].JC_Calc_NetWeight);
		}

		public void TestImportTopLevelPacks()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading { BillNumber = "MEH", CargoType = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(mode, string.Empty, context), };
				bill.Packages.Add(new Xsd.Package { RefNumber = "HONDA", NumberOfPacks = 10, GoodsDescription = "HONDA CRV", MarksAndNumbers = "*LE 4WD", CommodityCode = "AAA", Volume = new Xsd.DimensionValue { DimensionType = Constants.Volume.CubicMetres, Value = 10, }, Weight = new Xsd.DimensionValue { DimensionType = Constants.Weight.Kilograms, Value = 1000, }, Length = new Xsd.DimensionValue { DimensionType = Constants.Length.Metres, Value = 1.1, }, Height = new Xsd.DimensionValue { DimensionType = Constants.Length.Metres, Value = 1.2, }, Width = new Xsd.DimensionValue { DimensionType = Constants.Length.Metres, Value = 1.3, }, });
				bill.Packages[0].DangerousGoods.Add(new Xsd.HazardousGoods { UNDGCode = "0004A", Weight = new Xsd.DimensionValue { DimensionType = Constants.Weight.Kilograms, Value = 10m, }, Volume = new Xsd.DimensionValue { DimensionType = Constants.Volume.CubicFeet, Value = 3m, } });
				bill.Packages.Add(new Xsd.Package());
				var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
				AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
				AssertEquals("Packlines should not be created", 0, shipment.OuterPackLines.Count);
				AssertEquals("Packages should be imported as top level packs", 2, shipment.ShippingContainers.Count);
				CombineAssertions(() =>
				{
					var container = shipment.ShippingContainers[0];
					AssertEquals("HONDA", container.JC_ContainerNum);
					AssertEquals((short)10, container.JC_ContainerCount);
					AssertEquals("HONDA CRV", container.JC_Description);
					AssertEquals("*LE 4WD", container.JC_MarksAndNumbers);
					AssertEquals("AAA", container.JC_RH_NKContainerCommodityCode);
					AssertEquals(Constants.Volume.CubicMetres, container.JC_GrossVolumeUQ);
					AssertEquals(10m, container.JC_GrossVolume);
					AssertEquals(Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
					AssertEquals(1000m, container.JC_GrossWeight);
					AssertEquals(Constants.Length.Metres, container.JC_TotalUnitOfMeasure);
					AssertEquals(1.1m, container.JC_TotalLength);
					AssertEquals(1.2m, container.JC_TotalHeight);
					AssertEquals(1.3m, container.JC_TotalWidth);
					AssertEquals("dangerous goods imported", 1, container.UNDGs.Count);
					AssertEquals(container.PK, container.UNDGs[0].DI_ParentID);
					AssertEquals(JobContainerSchema.Constants.Prefix, container.UNDGs[0].DI_ParentTableCode);
					AssertEquals("0004a", container.UNDGs[0].Substance.DG_Code);
					AssertEquals(10m, container.UNDGs[0].DI_DGWeight);
					AssertEquals(Core.Constants.Weight.Kilograms, container.UNDGs[0].DI_UnitOfWeight);
					AssertEquals(3m, container.UNDGs[0].DI_DGVolume);
					AssertEquals(Core.Constants.Volume.CubicFeet, container.UNDGs[0].DI_UnitOfVolume);
				});
				var topLevelPacks = shipment.ShippingContainers.ToArray();
				adapter.ImportFromValueObject(shipment, bill, context);
				AssertEquals("Existing top level packs has been deleted", true, topLevelPacks.All(pack => pack.IsDeleted));
				AssertEquals("New top level packs has been imported", 2, shipment.ShippingContainers.Count);
			}
		}

		public void TestImportLoadDischargeDefaultsOriginDestination()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "MAGIC1";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			AgencyShipment bill = Factory.New<AgencyShipment>();
			bill.JS_HouseBill = "SuperMegaAwesome";
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_RL_NKOrigin = "";
			bill.JS_RL_NKDestination = "";
			Factory.Save();
			AssertEquals("precondition: origin", "", bill.JS_RL_NKOrigin);
			AssertEquals("precondition: destination", "", bill.JS_RL_NKDestination);
			Xsd.AgencyBillOfLading value = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				Load = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "BANOWATI", VoyageNo = "MAGIC1", }
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment;
			shipment = adapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("should update existing bill", bill.PK, shipment.PK);
			AssertEquals("load should default origin", "AUBNE", shipment.JS_RL_NKOrigin);
			AssertEquals("disc should default destination", "NLAMS", shipment.JS_RL_NKDestination);
			bill.JS_RL_NKOrigin = "AUSYD";
			bill.JS_RL_NKDestination = "GBLON";
			shipment = adapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("should update existing bill", bill.PK, shipment.PK);
			AssertEquals("load should not overwrite origin", "AUSYD", shipment.JS_RL_NKOrigin);
			AssertEquals("disc should not overwrite destination", "GBLON", shipment.JS_RL_NKDestination);
			value.Origin.Value = "AUMEL";
			value.Destination.Value = "NLMUD";
			shipment = adapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("should update existing bill", bill.PK, shipment.PK);
			AssertEquals("origin should overwrite origin", "AUMEL", shipment.JS_RL_NKOrigin);
			AssertEquals("destination should overwrite destination", "NLMUD", shipment.JS_RL_NKDestination);
		}

		public void TestImportBilling_Never()
		{
			SystemDataRegistry.Instance.ImportBillingInfoFromAgencyXmlFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ImportBillingInfoFromXMLMethod.Codes.Never);
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			principal.OH_Code = "Principal";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";
			Factory.Save();
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				Load = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "SGSIN" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "234", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
				Principal = new Xsd.Organisation()
				{ EDICode = principal.OH_Code },
				Billing = new Xsd.BillingWithExchangeRates()
				{
					LocalClient = new Xsd.Organisation()
					{ EDICode = client.OH_Code },
					ChargeLines = { new Xsd.ChargeLine()
			{ ChargeCode = "FRT", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 500m }, }, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment;
			shipment = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertContainsExactElementsInAnyOrder("Not Imported", Array.Empty<string>(), ChargesAsStrings(shipment.Job));
		}

		public void TestImportBilling_NewOnly()
		{
			SystemDataRegistry.Instance.ImportBillingInfoFromAgencyXmlFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ImportBillingInfoFromXMLMethod.Codes.NewOnly);
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			principal.OH_Code = "Principal";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";
			Factory.Save();
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				Load = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "SGSIN" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "234", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
				Principal = new Xsd.Organisation()
				{ EDICode = principal.OH_Code },
				Billing = new Xsd.BillingWithExchangeRates()
				{
					LocalClient = new Xsd.Organisation()
					{ EDICode = client.OH_Code },
					ChargeLines = { new Xsd.ChargeLine()
			{ ChargeCode = "FRT", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 500m }, }, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			const string expected = "Code: FRT\r\n" + "Description: International Freight\r\n" + "Sell Amt: 500.000 AUD\r\n" + "";
			AgencyShipment shipment1 = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertContainsExactElementsInAnyOrder("Initial Import", new string[] { expected }, ChargesAsStrings(shipment1.Job));
			Factory.Save();
			bill.Billing.ChargeLines[0].OSSellAmount.Value = 400;
			AgencyShipment shipment2 = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals("Should match same shipment.", shipment1, shipment2);
			AssertContainsExactElementsInAnyOrder("Not Updated", new string[] { expected }, ChargesAsStrings(shipment2.Job));
		}

		public void TestImportBilling_Always()
		{
			SystemDataRegistry.Instance.ImportBillingInfoFromAgencyXmlFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ImportBillingInfoFromXMLMethod.Codes.Always);
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			principal.OH_Code = "Principal";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";
			Factory.Save();
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwesome",
				Load = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "SGSIN" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "234", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
				Principal = new Xsd.Organisation()
				{ EDICode = principal.OH_Code },
				Billing = new Xsd.BillingWithExchangeRates()
				{
					LocalClient = new Xsd.Organisation()
					{ EDICode = client.OH_Code },
					ChargeLines = { new Xsd.ChargeLine()
			{ ChargeCode = "FRT", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 500m }, }, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			const string expected1 = "Code: FRT\r\n" + "Description: International Freight\r\n" + "Sell Amt: 500.000 AUD\r\n" + "";
			const string expected2 = "Code: FRT\r\n" + "Description: International Freight\r\n" + "Sell Amt: 400.000 AUD\r\n" + "";
			AgencyShipment shipment1 = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertContainsExactElementsInAnyOrder("Initial Import", new string[] { expected1 }, ChargesAsStrings(shipment1.Job));
			Factory.Save();
			bill.Billing.ChargeLines[0].OSSellAmount.Value = 400;
			AgencyShipment shipment2 = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals("Should match same shipment.", shipment1, shipment2);
			AssertContainsExactElementsInAnyOrder("Not Updated", new string[] { expected2 }, ChargesAsStrings(shipment2.Job));
		}

		public void TestExceptionNotThrownWhenNumberOfPacksIsBig()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_OuterPacks = 136443;
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			var xsdAgencyBillOfLading = new Xsd.AgencyBillOfLading();
			AssertNoExceptionThrown(() =>
			{
				adapter.ExportToValueObject(agencyShipment, xsdAgencyBillOfLading, new ValueObjectExportContext(new NotificationBuffer()));
			});
			AssertEquals(136443, xsdAgencyBillOfLading.PackageSummary.NumberOfPacks);
		}

		public void TestImportBillingShouldDefaultCreditor()
		{
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			principal.OH_Code = "Principal";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Load = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				INCOTerm = Constants.DomesticPaymentTerms.Collect,
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "665", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
				Principal = new Xsd.Organisation()
				{ EDICode = principal.OH_Code },
				Billing = new Xsd.BillingWithExchangeRates()
				{
					LocalClient = new Xsd.Organisation()
					{ EDICode = client.OH_Code },
					ChargeLines = { new Xsd.ChargeLine()
			{ ChargeCode = "FRT", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 500m }, }, new Xsd.ChargeLine()
			{ ChargeCode = "BAF", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 100m }, }, new Xsd.ChargeLine()
			{ ChargeCode = "DDOC", Collect = true, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 100m }, }, new Xsd.ChargeLine()
			{ ChargeCode = "ODOC", Collect = false, OSSellAmount = new Xsd.FinancialValue()
			{ CurrencyCode = "AUD", Value = 100m }, }, },
				},
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			ZQuery filter = new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK);
			JobCharge[] charges = Factory.Load<JobCharge>(filter);
			string[] lines = Array.ConvertAll(charges, (c) =>
			{
				return string.Format("{0}|{1}|{2}", c.ChargeCode == null ? "<null>" : c.ChargeCode.AC_Code.ToString(), c.CostAccount == null ? "<null>" : c.CostAccount.OH_Code.ToString(), c.SellAccount == null ? "<null>" : c.SellAccount.OH_Code.ToString());
			});
			Array.Sort(lines);
			const string expected = @"
BAF|Principal|Client
DDOC|Principal|Client
FRT|Principal|Client
ODOC|Principal|Client
";
			AssertMultilineASCIIEquals("", expected.Trim(), string.Join("\r\n", lines));
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var jobs = Factory.Load<JobHeader>(query);
			foreach (var job in jobs)
			{
				job.Dispose();
			}
		}

		public void TestImportWithContainerTypeNotSpecified()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Load = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "665", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
			};
			AgencyShipment shipment;
			{
				AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
				NotificationBuffer buffer = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
				shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
				AssertEquals("New Shipment: Use system default if not specified in xml.", Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			}

			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			Factory.Save();
			{
				AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
				NotificationBuffer buffer = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
				shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
				AssertEquals("Existing Shipment: Dont overwrite if not specified in xml.", Constants.ContainerModes.Bulk, shipment.JS_PackingMode);
			}

			shipmentValueObject.CargoType = Xsd.ContainerMode.BBK;
			{
				AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
				NotificationBuffer buffer = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
				shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
				AssertEquals("Existing Shipment: Do overwrite if specified in xml.", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
			}
		}

		public void TestImportContainerWeight()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "blaticus",
				Containers = { new Xsd.Container()
			{ ContainerNumber = "TEST4100001", ContainerType = new Xsd.ContainerType()
			{ ContainerCode = "20GP", ISOCode = "22G0" }, GrossWeight = new Xsd.DimensionValue()
			{ DimensionType = Constants.Weight.Tonnes, Value = 30 }, NetWeight = new Xsd.DimensionValue()
			{ DimensionType = Constants.Weight.Kilograms, Value = 27000 }, Weight = 4000, }, new Xsd.Container()
			{ ContainerNumber = "TEST4100002", ContainerType = new Xsd.ContainerType()
			{ ContainerCode = "20GP", ISOCode = "22G0" }, GrossWeight = new Xsd.DimensionValue()
			{ DimensionType = Constants.Weight.Tonnes, Value = 30 }, Weight = 4000, }, new Xsd.Container()
			{ ContainerNumber = "TEST4100003", ContainerType = new Xsd.ContainerType()
			{ ContainerCode = "20GP", ISOCode = "22G0" }, NetWeight = new Xsd.DimensionValue()
			{ DimensionType = Constants.Weight.Kilograms, Value = 27000 }, Weight = 4000, }, new Xsd.Container()
			{ ContainerNumber = "TEST4100004", ContainerType = new Xsd.ContainerType()
			{ ContainerCode = "20GP", ISOCode = "22G0" }, Weight = 4000, }, },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AgencyShipmentContainer[] containers = shipment.BookedContainers.ToArray<AgencyShipmentContainer>();
			Array.Sort(containers, (c1, c2) => StringComparer.OrdinalIgnoreCase.Compare(c1.JC_ContainerNum, c2.JC_ContainerNum));
			StringBuilder builder = new StringBuilder();
			foreach (AgencyShipmentContainer container in containers)
			{
				builder.AppendLine();
				builder.Append("Container Number : ");
				builder.AppendLine(container.JC_ContainerNum);
				builder.Append("Unit of Weight   : ");
				builder.AppendLine(container.JC_GrossWeightUQ);
				builder.Append("Gross Weight     : ");
				builder.AppendLine(container.JC_GrossWeight.ToString("0.000"));
				builder.Append("Net Weight       : ");
				builder.AppendLine(container.JC_Calc_NetWeight.ToString("0.000"));
				builder.Append("Tare Weight      : ");
				builder.AppendLine(container.JC_TareWeight.ToString("0.000"));
			}

			const string expected = @"
Container Number : TEST4100001
Unit of Weight   : T
Gross Weight     : 30.000
Net Weight       : 27.000
Tare Weight      : 3.000

Container Number : TEST4100002
Unit of Weight   : T
Gross Weight     : 30.000
Net Weight       : 27.720
Tare Weight      : 2.280

Container Number : TEST4100003
Unit of Weight   : KG
Gross Weight     : 29280.000
Net Weight       : 27000.000
Tare Weight      : 2280.000

Container Number : TEST4100004
Unit of Weight   : KG
Gross Weight     : 4000.000
Net Weight       : 0.000
Tare Weight      : 4000.000
";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestImportETDETA()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Load = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "665", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertNotNull("should have created the shipment", shipment);
			AssertNotNull("generated shipment should have a sailing", shipment.Sailing);
			AssertEquals("NLAMS", shipment.Sailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("AUBNE", shipment.Sailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("ETD", new ZDateTime(2010, 06, 07), shipment.Sailing.JX_JA_E_DEP);
			AssertEquals("ETA", new ZDateTime(2010, 06, 08), shipment.Sailing.JX_JB_E_ARV);
		}

		public void TestImportETDETA_Update()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "665";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2010, 06, 05);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = new ZDateTime(2010, 06, 06);
			voyage.GenerateSailings();
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Load = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "665", ETD = new ZDateTime(2010, 06, 07), ETA = new ZDateTime(2010, 06, 08), },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertNotNull("should have created the shipment", shipment);
			AssertNotNull("generated shipment should have a sailing", shipment.Sailing);
			AssertEquals("should have matched the existing voyage", voyage, shipment.Sailing.Voyage);
			AssertEquals("NLAMS", shipment.Sailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("AUBNE", shipment.Sailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("ETD", new ZDateTime(2010, 06, 07), shipment.Sailing.JX_JA_E_DEP);
			AssertEquals("ETA", new ZDateTime(2010, 06, 08), shipment.Sailing.JX_JB_E_ARV);
		}

		public void TestImportETDETA_DontClear()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "665";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2010, 06, 05);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = new ZDateTime(2010, 06, 06);
			voyage.GenerateSailings();
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Load = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{ VesselName = "MAJAPAHIT", VoyageNo = "665", ETD = ZDateTime.Empty, ETA = ZDateTime.Empty, },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertNotNull("should have created the shipment", shipment);
			AssertNotNull("generated shipment should have a sailing", shipment.Sailing);
			AssertEquals("should have matched the existing voyage", voyage, shipment.Sailing.Voyage);
			AssertEquals("NLAMS", shipment.Sailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("AUBNE", shipment.Sailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("ETD", new ZDateTime(2010, 06, 05), shipment.Sailing.JX_JA_E_DEP);
			AssertEquals("ETA", new ZDateTime(2010, 06, 06), shipment.Sailing.JX_JB_E_ARV);
		}

		public void TestFilterExportContainers()
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			BillOfLadingContainer container1 = bill.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			BillOfLadingContainer container2 = bill.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			BillOfLadingPackLine packline1 = bill.OuterPackLines.AddNew();
			packline1.JL_Description = "packline1";
			packline1.JL_DetailedDescription = "DetailedDescription1";
			packline1.JL_JC = container1.PK;
			BillOfLadingPackLine packline2 = bill.OuterPackLines.AddNew();
			packline2.JL_Description = "packline2";
			packline2.JL_JC = container2.PK;
			BillOfLadingPackLine packline3 = bill.OuterPackLines.AddNew();
			packline3.JL_Description = "packline3";
			packline3.JL_DetailedDescription = "DetailedDescription3";
			AgencyShipmentValueObjectDataAdapter<BillOfLading> adapter = new AgencyShipmentValueObjectDataAdapter<BillOfLading>();
			IValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			Xsd.AgencyBillOfLading value;
			value = adapter.ExportToValueObject(bill, context);
			AssertContainsExactElementsInAnyOrder("No Filter - Containers", new ZString[] { "TEST4100013", "TEST4100029" }, Array.ConvertAll(GetContainersArray(value.Containers), (c) => c.ContainerNumber));
			AssertContainsExactElementsInAnyOrder("No Filter - Packlines", new ZString[] { "DetailedDescription1", "", "DetailedDescription3" }, Array.ConvertAll(GetPackagesArray(value.Packages), (p) => p.GoodsDescription));
			adapter.ContainerFilter = (c) => c.JC_ContainerNum == "TEST4100013";
			value = adapter.ExportToValueObject(bill, context);
			AssertContainsExactElementsInAnyOrder("Filtered", new ZString[] { "TEST4100013" }, Array.ConvertAll(GetContainersArray(value.Containers), (c) => c.ContainerNumber));
			AssertContainsExactElementsInAnyOrder("Filtered - Packlines", new ZString[] { "DetailedDescription1", }, Array.ConvertAll(GetPackagesArray(value.Packages), (p) => p.GoodsDescription));
		}

		public void TestImportPackTypeMappings_Valid()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				PackageSummary = new Xsd.Package()
				{ PackType = Constants.PkgUnit.Pallet }
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals(Constants.PkgUnit.Pallet, shipment.JS_F3_NKPackType);
		}

		public void TestImportPackTypeMappings_Mapped()
		{
			OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			OrgPatternMatchOverride map1 = proxy.CreatePatternMatchOverrideForTest();
			map1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.PackageType;
			map1.OO_ForeignCode = "XXX";
			map1.OO_LocalCode = Constants.PkgUnit.Piece;
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				PackageSummary = new Xsd.Package()
				{ PackType = "XXX" }
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals(Constants.PkgUnit.Piece, shipment.JS_F3_NKPackType);
		}

		public void TestImportPackTypeMappings_Invalid()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				PackageSummary = new Xsd.Package()
				{ PackType = "XXX" }
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals(Constants.PkgUnit.Package, shipment.JS_F3_NKPackType);
		}

		public void TestIncoTermsAreCorrectlyImportedForImportShipmentsWhenLoggedIntoAnExportDepartment()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "XXX";
			department.GE_Import = false;
			department.GE_Domestic = false;
			department.GE_Export = true;
			department.GE_NonDirectional = false;
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				INCOTerm = Constants.DomesticPaymentTerms.Collect,
				Origin = new Xsd.UNLOCO()
				{ Value = "DEMEE" },
				Load = new Xsd.UNLOCO()
				{ Value = "DEMEE" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Destination = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment;
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.CurrentBranch.PK, department.PK.ToGuid()))
			{
				shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			}

			AssertEquals(Constants.DomesticPaymentTerms.Collect, shipment.JS_INCO);
		}

		public void TestImportPortMappings()
		{
			RefUNLOCO ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO nlams = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NLAMS");
			RefUNLOCO gblon = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			OrgPatternMatchOverride map1 = proxy.CreatePatternMatchOverrideForTest();
			map1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			map1.OO_ForeignCode = "WWW";
			map1.OO_LocalGuid = ausyd.PK;
			OrgPatternMatchOverride map2 = proxy.CreatePatternMatchOverrideForTest();
			map2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			map2.OO_ForeignCode = "XXX";
			map2.OO_LocalGuid = aubne.PK;
			OrgPatternMatchOverride map3 = proxy.CreatePatternMatchOverrideForTest();
			map3.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			map3.OO_ForeignCode = "YYY";
			map3.OO_LocalGuid = nlams.PK;
			OrgPatternMatchOverride map4 = proxy.CreatePatternMatchOverrideForTest();
			map4.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			map4.OO_ForeignCode = "ZZZ";
			map4.OO_LocalGuid = gblon.PK;
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Origin = new Xsd.UNLOCO()
				{ Value = "WWW" },
				Load = new Xsd.UNLOCO()
				{ Value = "XXX" },
				Discharge = new Xsd.UNLOCO()
				{ Value = "YYY" },
				Destination = new Xsd.UNLOCO()
				{ Value = "ZZZ" },
			};
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals("Origin", ausyd.RL_Code, shipment.JS_RL_NKOrigin);
			AssertEquals("Load", aubne.RL_Code, shipment.JS_NKLoadPort);
			AssertEquals("Discharge", nlams.RL_Code, shipment.JS_NKDischargePort);
			AssertEquals("Destination", gblon.RL_Code, shipment.JS_RL_NKDestination);
		}

		public void TestImportContainerTypeMappings()
		{
			RefContainer container1 = Factory.New<RefContainer>();
			container1.RC_Code = "CT1";
			container1.RC_ISOType = "XXXX";
			RefContainer container2 = Factory.New<RefContainer>();
			container2.RC_Code = "CT2";
			container2.RC_ISOType = "ISO2";
			OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			OrgPatternMatchOverride map1 = proxy.CreatePatternMatchOverrideForTest();
			map1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			map1.OO_ForeignCode = "CODE1";
			map1.OO_LocalGuid = container1.PK;
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading()
			{ BillNumber = "Blaticus", };
			shipmentValueObject.Containers.Add(new Xsd.Container()
			{
				ContainerNumber = "FAKE4100011",
				ContainerType = new Xsd.ContainerType()
				{ ContainerCode = "CODE1", ISOCode = "ISO1" }
			});
			shipmentValueObject.Containers.Add(new Xsd.Container()
			{
				ContainerNumber = "FAKE4100027",
				ContainerType = new Xsd.ContainerType()
				{ ContainerCode = "CODE2", ISOCode = "ISO2" }
			});
			shipmentValueObject.Containers.Add(new Xsd.Container()
			{
				ContainerNumber = "FAKE4100032",
				ContainerType = new Xsd.ContainerType()
				{ ContainerCode = "CODE3", ISOCode = "ISO3" }
			});
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			shipment.RealContainers.Sort((BillOfLadingContainer c1, BillOfLadingContainer c2) =>
			{
				return StringComparer.OrdinalIgnoreCase.Compare(c1.JC_ContainerNum, c2.JC_ContainerNum);
			});
			AssertEquals(3, shipment.BookedContainers.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { container1.PK, container2.PK, ZGuid.Empty }, Array.ConvertAll(shipment.BookedContainers.ToArray<AgencyShipmentContainer>(), c => c.JC_RC));
		}

		public void TestImportGoodsDescription_ShortOnly()
		{
			GenericImportGoodsDescriptionTest("Short Description 01234567890123456", "Short Description 0123456789012345678901", "Short Description 0123456789012345678901", "");
		}

		public void TestImportGoodsDescription_LongOnly()
		{
			GenericImportGoodsDescriptionTest("Long Description 012345678901234567", "Long Description 0123456789012345678901", "", "Long Description 0123456789012345678901");
		}

		public void TestImportGoodsDescription_Both()
		{
			GenericImportGoodsDescriptionTest("Short Description 01234567890123456", "Long Description 0123456789012345678901", "Short Description 0123456789012345678901", "Long Description 0123456789012345678901");
		}

		public void TestImportGoodsDescription_Neither()
		{
			GenericImportGoodsDescriptionTest("", "", "", "");
		}

		public void TestImportVessel_MatchByLloydsNumber()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			var shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.Load = new Xsd.UNLOCO()
			{ Value = "NLAMS" };
			shipmentValueObject.Discharge = new Xsd.UNLOCO()
			{ Value = "AUBNE" };
			shipmentValueObject.Sailing = new Xsd.SailingWithVesselVoyage();
			shipmentValueObject.Sailing.LloydsNo = vessel.RV_LloydsNumber;
			shipmentValueObject.Sailing.VesselName = "BLAH";
			shipmentValueObject.Sailing.ETD = new ZDateTime(2010, 06, 07);
			shipmentValueObject.Sailing.ETA = new ZDateTime(2010, 06, 08);
			var shipment = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>().CreateOrUpdateFromValueObject(shipmentValueObject, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(vessel.RV_Name, shipment.Sailing.Vessel.RV_Name);
		}

		public void TestImportVessel_MatchByVesselNameWhenLloydsNumberInvalidOrEmpty()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.Load = new Xsd.UNLOCO()
			{ Value = "NLAMS" };
			shipmentValueObject.Discharge = new Xsd.UNLOCO()
			{ Value = "AUBNE" };
			shipmentValueObject.Sailing = new Xsd.SailingWithVesselVoyage();
			shipmentValueObject.Sailing.LloydsNo = "9999999";
			shipmentValueObject.Sailing.VesselName = existingVessel.RV_Name;
			shipmentValueObject.Sailing.ETD = new ZDateTime(2010, 06, 07);
			shipmentValueObject.Sailing.ETA = new ZDateTime(2010, 06, 08);
			var shipment = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>().CreateOrUpdateFromValueObject(shipmentValueObject, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(existingVessel.RV_Name, shipment.Sailing.Vessel.RV_Name);
		}

		void GenericImportGoodsDescriptionTest(ZString expectedShortDescription, ZString expectedLongDescription, ZString providedShortDescription, ZString providedLongDescription)
		{
			Xsd.AgencyBillOfLading xsdAgencyBillOfLading = new Xsd.AgencyBillOfLading();
			xsdAgencyBillOfLading.Description = providedShortDescription;
			if (!providedLongDescription.IsEmpty)
			{
				Xsd.NotesNote note = xsdAgencyBillOfLading.Notes.AddNew();
				note.NoteType = Xsd.NotesNoteNoteType.DetailedGoodsDescription;
				note.NoteData = providedLongDescription;
			}

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			adapter.ImportFromValueObject(agencyShipment, xsdAgencyBillOfLading, context);
			AssertEquals("Short", expectedShortDescription, agencyShipment.JS_GoodsDescription);
			AssertEquals("Long", expectedLongDescription, agencyShipment.DetailedGoodsDescriptionNoteText);
		}

		public void TestExportGoodsDescription()
		{
			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.DetailedGoodsDescriptionNoteText = "Detailed Goods Description";
			agencyShipment.JS_GoodsDescription = "Short Goods Description";
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			Xsd.AgencyBillOfLading xsdAgencyBillOfLading = new Xsd.AgencyBillOfLading();
			adapter.ExportToValueObject(agencyShipment, xsdAgencyBillOfLading, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Short Goods Description", xsdAgencyBillOfLading.Description);
			AssertEquals(1, xsdAgencyBillOfLading.Notes.Count);
			AssertEquals("Detailed Goods Description", xsdAgencyBillOfLading.Notes[0].NoteData);
		}

		public void TestExportCustomAttributes()
		{
			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_HouseBill = "HOUSEBILL";
			PackLine packline = agencyShipment.OuterPackLines.AddNew();
			packline.JL_CustomDate1 = ZDateTime.Now;
			packline.JL_CustomDate2 = ZDateTime.Empty;
			packline.JL_CustomAttrib1 = "Test Custom Text 1";
			packline.JL_CustomAttrib2 = "Test Custom Text 2";
			packline.JL_CustomAttrib3 = "Test Custom Text 3";
			packline.JL_CustomAttrib4 = "Test Custom Text 4";
			packline.JL_CustomDecimal1 = 7;
			packline.JL_CustomDecimal2 = 2.5;
			packline.JL_CustomFlag1 = true;
			packline.JL_CustomFlag2 = false;
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			Xsd.AgencyBillOfLading xsdAgencyBillOfLading = new Xsd.AgencyBillOfLading();
			adapter.ExportToValueObject(agencyShipment, xsdAgencyBillOfLading, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.Package valuePackage = xsdAgencyBillOfLading.Packages[0];
			AssertEquals("packline custom attribute date 1", packline.JL_CustomDate1, valuePackage.Custom.Date1);
			AssertEquals("packline custom attribute date 2", packline.JL_CustomDate2, valuePackage.Custom.Date2);
			AssertEquals("packline custom attribute text 1", packline.JL_CustomAttrib1, valuePackage.Custom.Text1);
			AssertEquals("packline custom attribute text 2", packline.JL_CustomAttrib2, valuePackage.Custom.Text2);
			AssertEquals("packline custom attribute text 3", packline.JL_CustomAttrib3, valuePackage.Custom.Text3);
			AssertEquals("packline custom attribute text 4", packline.JL_CustomAttrib4, valuePackage.Custom.Text4);
			AssertEquals("packline custom attribute decimal 1", packline.JL_CustomDecimal1, valuePackage.Custom.Decimal1);
			AssertEquals("packline custom attribute decimal 2", packline.JL_CustomDecimal2, valuePackage.Custom.Decimal2);
			AssertEquals("packline custom attribute flag 1", packline.JL_CustomFlag1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false, valuePackage.Custom.Flag1);
			AssertEquals("packline custom attribute flag 2", packline.JL_CustomFlag2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false, valuePackage.Custom.Flag2);
		}

		public void TestAddExportEvent()
		{
			NotificationBuffer notifications = new NotificationBuffer();
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			AgencyShipment shipmentBizObj = Factory.NewWithValidTestData<AgencyShipment>();
			StmALog[] dataExportEvents = shipmentBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to the shipment", 0, dataExportEvents.Length);
			adapter.ExportToValueObject(shipmentBizObj, new ValueObjectExportContext(notifications));
			dataExportEvents = shipmentBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to the shipment", 1, dataExportEvents.Length);
			adapter.ExportToValueObject(shipmentBizObj, new ValueObjectExportContext(notifications));
			dataExportEvents = shipmentBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to the shipment", 2, dataExportEvents.Length);
		}

		public void TestAddImportEvent()
		{
			Xsd.AgencyBillOfLading bill = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "SuperMegaAwsome",
				Origin = new Xsd.UNLOCO()
				{ Value = "AUBNE" },
				Destination = new Xsd.UNLOCO()
				{ Value = "NLAMS" },
			};
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			StmALog[] logs;
			AgencyShipment shipment1 = adapter.CreateOrUpdateFromValueObject(bill, context);
			logs = shipment1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("Should have added a data import event", 1, logs.Length);
			Factory.Save();
			AgencyShipment shipment2 = adapter.CreateOrUpdateFromValueObject(bill, context);
			AssertEquals("update the existing shipment rather than creating a new one", shipment1.PK, shipment2.PK);
			logs = shipment2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("Should have added another data import event", 2, logs.Length);
		}

		public void TestExportIncludeCharges()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_HouseBill = "HouseBill";
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 2;
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = header.PK;
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			Xsd.AgencyBillOfLading valueObject = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			Assert(valueObject.Billing.IsSpecified);
			SystemDataRegistry.Instance.IncludeBillingInfoInAgencyXMLFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude);
			valueObject = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			Assert(!valueObject.Billing.IsSpecified);
		}

		public void TestMergeChargesWithDepartmentCharges_Booking()
		{
			var sEC = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "SEC");
			sEC.DeptCharges.DeleteAll();
			AddCharge(sEC, "ODOC", 1);
			Factory.Save();
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyBooking>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			AgencyBooking booking = adapter.CreateOrUpdateFromValueObject(GetBOLFromXml("Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.PopulatedAgencyBillOfLadingForChargeMergeTest.xml"), context);
			AssertMergeChargesWithDepartmentCharges(booking);
		}

		public void TestMergeChargesWithDepartmentCharges_BillOfLading()
		{
			var sEC = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "SEC");
			sEC.DeptCharges.DeleteAll();
			AddCharge(sEC, "ODOC", 1);
			Factory.Save();
			var adapter = new AgencyShipmentValueObjectDataAdapter<BillOfLading>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BillOfLading billOfLading = adapter.CreateOrUpdateFromValueObject(GetBOLFromXml("Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.PopulatedAgencyBillOfLadingForChargeMergeTest.xml"), context);
			AssertMergeChargesWithDepartmentCharges(billOfLading);
		}

		void AssertMergeChargesWithDepartmentCharges(AgencyShipment shipment)
		{
			var header = new JobHeader.Loader(shipment).Load();
			Factory.Save();
			AssertNotNull("Should have a job header.", header);
			AssertEquals("Should have the correct department.", "SEC", header.Department.GE_Code);
			AssertContainsExactElementsInAnyOrder("Should contain 2 charges", new string[] {// from the department charges
			"Code: ODOC\r\n" + "Description: Origin Documentation Fee\r\n" + "Sell Amt: 0.000 AUD\r\n" + "", // from the Xml
			"Code: FRT\r\n" + "Description: Random Crap\r\n" + "Sell Amt: 1500.000 USD\r\n" + "", }, ChargesAsStrings(header));
		}

		public void TestImportOversizeWeightsAndVolumes()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.BillNumber = "Blaticus";
			shipmentValueObject.PackageSummary.Weight.Value = GetOversizeValue(JobShipmentSchema.JS_ActualWeight);
			shipmentValueObject.PackageSummary.Weight.DimensionType = "KG";
			shipmentValueObject.PackageSummary.Volume.Value = GetOversizeValue(JobShipmentSchema.JS_ActualVolume);
			shipmentValueObject.PackageSummary.Volume.DimensionType = "L";
			Xsd.Package package = shipmentValueObject.Packages.AddNew();
			package.Weight.Value = GetOversizeValue(JobPackLinesSchema.JL_ActualWeight);
			package.Weight.DimensionType = "KG";
			package.Volume.Value = GetOversizeValue(JobPackLinesSchema.JL_ActualVolume);
			package.Volume.DimensionType = "L";
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adaptor = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adaptor.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			const string expectedLog = "Importing shipment with bill 'Blaticus'.\r\n" + "Shipping Booking V00001000 created\r\n";
			AssertNoExceptionThrown("should not set any values that cause an exception on saving", shipment.Factory.Save);
			AssertMultilineASCIIEquals("should be no errors", expectedLog, buffer.AsString);
			AssertEquals("should have no errors", false, buffer.HasErrors);
			AssertEquals(1000m, shipment.JS_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(1000m, shipment.JS_ActualWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment.JS_UnitOfWeight);
		}

		public void TestImportOversizeWeightsAndVolumes_Maxed()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.BillNumber = "Blaticus";
			shipmentValueObject.PackageSummary.Weight.Value = GetOversizeValue(JobShipmentSchema.JS_ActualWeight);
			shipmentValueObject.PackageSummary.Weight.DimensionType = "KT";
			shipmentValueObject.PackageSummary.Volume.Value = GetOversizeValue(JobShipmentSchema.JS_ActualVolume);
			shipmentValueObject.PackageSummary.Volume.DimensionType = "ML";
			Xsd.Container container = shipmentValueObject.Containers.AddNew();
			container.Weight = GetOversizeValue(JobContainerSchema.JC_TareWeight);
			Xsd.Package package = shipmentValueObject.Packages.AddNew();
			package.Weight.Value = GetOversizeValue(JobPackLinesSchema.JL_ActualWeight);
			package.Weight.DimensionType = "KT";
			package.Volume.Value = GetOversizeValue(JobPackLinesSchema.JL_ActualVolume);
			package.Volume.DimensionType = "ML";
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adaptor = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adaptor.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			const string expectedLog = "Importing shipment with bill 'Blaticus'.\r\n" + "Error: Value overflow error (Package summary volume; value = 1000000ML, max = 999999.999ML)\r\n" + "Error: Value overflow error (Package summary weight; value = 1000000KT, max = 999999.999KT)\r\n" + "Error: Value overflow error (Container tare weight; value = 1000000, max = 999999.999)\r\n" + "Error: Value overflow error (Pack line actual volume; value = 1000000ML, max = 999999.999ML)\r\n" + "Error: Value overflow error (Pack line actual weight; value = 1000000KT, max = 999999.999KT)\r\n" + "Shipping Booking V00001000 created\r\n";
			AssertNoExceptionThrown("should not set any values that cause an exception on saving", shipment.Factory.Save);
			AssertMultilineASCIIEquals("", expectedLog, buffer.AsString);
			Assert("should have errors", buffer.HasErrors);
			Assert("should have errors", !buffer.ContainsNotificationType(ErrorType.DataErrorPreventSave));
		}

		public void TestImportCustomAttributes()
		{
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.IsSpecified = true;
			shipmentValueObject.BillNumber = "HOUSEBILL";
			Xsd.Package package = shipmentValueObject.Packages.AddNew();
			package.Custom.Date1 = new ZDateTime(2012, 07, 13, 10, 0, 0);
			package.Custom.Date2 = ZDateTime.Empty;
			package.Custom.Text1 = "Test Text 1";
			package.Custom.Text2 = "Test Text 2";
			package.Custom.Text3 = "Test Text 3";
			package.Custom.Text4 = "Test Text 4";
			package.Custom.Decimal1 = 21m;
			package.Custom.Decimal2 = 86.45m;
			package.Custom.Flag1 = Xsd.TrueFalse.@true;
			package.Custom.Flag2 = Xsd.TrueFalse.@false;
			package.Custom.Decimal1Specified = true;
			package.Custom.Decimal2Specified = true;
			package.Custom.Flag1Specified = true;
			package.Custom.Flag2Specified = true;
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AgencyShipmentPackLine packline = shipment.OuterPackLines[0];
			AssertEquals("Date 1", new ZDateTime(2012, 07, 13, 10, 0, 0), packline.JL_CustomDate1);
			AssertEquals("Date 2", ZDateTime.Empty, packline.JL_CustomDate2);
			AssertEquals("Custom Text 1", "Test Text 1", packline.JL_CustomAttrib1);
			AssertEquals("Custom Text 2", "Test Text 2", packline.JL_CustomAttrib2);
			AssertEquals("Custom Text 3", "Test Text 3", packline.JL_CustomAttrib3);
			AssertEquals("Custom Text 4", "Test Text 4", packline.JL_CustomAttrib4);
			AssertEquals("Decimal 1", 21m, packline.JL_CustomDecimal1);
			AssertEquals("Decimal 2", 86.45m, packline.JL_CustomDecimal2);
			AssertEquals("Flag 1", true, packline.JL_CustomFlag1);
			AssertEquals("Flag 2", false, packline.JL_CustomFlag2);
			package.Custom.Decimal1Specified = false;
			package.Custom.Decimal2Specified = false;
			package.Custom.Flag1Specified = false;
			package.Custom.Flag2Specified = false;
			adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			packline = shipment.OuterPackLines[0];
			AssertEquals("Decimal 1", 0m, packline.JL_CustomDecimal1);
			AssertEquals("Decimal 2", 0m, packline.JL_CustomDecimal2);
			AssertEquals("Flag 1", false, packline.JL_CustomFlag1);
			AssertEquals("Flag 1", false, packline.JL_CustomFlag2);
		}

		public void TestImportOriginalsAndCoppies()
		{
			AgencyShipment existingShipment = Factory.New<AgencyShipment>();
			existingShipment.JS_HouseBill = "Blaticus";
			existingShipment.JS_NoOriginalBills = 2;
			existingShipment.JS_NoCopyBills = 2;
			Factory.Save();
			Xsd.AgencyBillOfLading shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.BillNumber = "Blaticus";
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			AgencyShipment shipment1 = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals("precondition:", existingShipment.PK, shipment1.PK);
			AssertEquals("Original Bills not set", (byte)2, shipment1.JS_NoOriginalBills);
			AssertEquals("Copy Bills not set", (byte)2, shipment1.JS_NoCopyBills);
			shipmentValueObject.OriginalBills = 3;
			shipmentValueObject.OriginalBillsSpecified = true;
			shipmentValueObject.CopyBills = 4;
			shipmentValueObject.CopyBillsSpecified = true;
			AgencyShipment shipment2 = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals("precondition:", existingShipment.PK, shipment2.PK);
			AssertEquals("Original Bills set", (byte)3, shipment2.JS_NoOriginalBills);
			AssertEquals("Copy Bills set", (byte)4, shipment2.JS_NoCopyBills);
		}

		public void TestFindBusinessObject()
		{
			AgencyShipment shipment = ImportAgencyShipment();
			int numberOfQueries = 0;
			AgencyShipment shipment2 = ImportAgencyShipment((IQueryUserEventArgs e) =>
			{
				numberOfQueries++;
				QueryUserYesNoYesAllNoAllEventArgs yesNoArgs = (QueryUserYesNoYesAllNoAllEventArgs)e;
				AssertEquals("An existing Bill of Lading (V00001000, SHIPMENT123) has been found, do you wish to update it?", yesNoArgs.Message);
				AssertEquals(true, yesNoArgs.Response);
			});
			AssertEquals(1, numberOfQueries);
			AssertEquals(shipment, shipment2);
		}

		public void TestFindBusinessObjectWithExternalPortCode()
		{
			SetupPortCodeMapping();
			AgencyShipment shipment = ImportAgencyShipment();
			int numberOfQueries = 0;
			AgencyShipment shipment2 = ImportAgencyShipment((IQueryUserEventArgs e) =>
			{
				numberOfQueries++;
				QueryUserYesNoYesAllNoAllEventArgs yesNoArgs = (QueryUserYesNoYesAllNoAllEventArgs)e;
				AssertEquals("An existing Bill of Lading (V00001000, SHIPMENT123) has been found, do you wish to update it?", yesNoArgs.Message);
				AssertEquals(true, yesNoArgs.Response);
			}, "ABC", "EFG", "ADMIRALENGRACHT");
			AssertEquals(1, numberOfQueries);
			AssertEquals(shipment, shipment2);
		}

		public void TestFindBusinessObject_SailingCarrierIsUsedForMatching()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Visund";
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_FullName = "CarrierONE";
			carrier1.OrganisationTypes = OrganisationTypes.Carrier;
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CARRIER2";
			carrier2.OH_FullName = "CarrierTWO";
			carrier2.OrganisationTypes = OrganisationTypes.Carrier;
			Factory.Save();
			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "500", carrier1.PK, "AUSYD", "NZAKL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "500", carrier2.PK, "AUSYD", "NZAKL");
			var shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_HouseBill = "MATCHME";
			shipment1.JS_JX = voyage1.Sailings[0].PK;
			var shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_HouseBill = "MATCHME";
			shipment2.JS_JX = voyage2.Sailings[0].PK;
			var xmlShipment = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "MATCHME",
				Origin = new Xsd.UNLOCO()
				{ Value = "AUSYD" },
				Destination = new Xsd.UNLOCO()
				{ Value = "NZAKL" },
				Sailing = new Xsd.SailingWithVesselVoyage()
				{
					VoyageNo = "500",
					VesselName = "Visund",
					Carrier = new Xsd.Organisation()
					{
						EDICode = "CARRIER2",
						OrganisationDetails = new Xsd.OrganisationDetail()
						{ Name = "CarrierTWO" }
					}
				}
			};
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var importedShipment = adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("Carrier used in matching", shipment2, importedShipment);
		}

		public void TestCannotFindBusinessObjectWhenBillNumberIsDifferent()
		{
			CheckDifferent((AgencyShipment shipment) =>
			{
				shipment.JS_HouseBill = "AWESOME";
			});
		}

		public void TestCannotFindBusinessObjectWhenVesselIsDifferent()
		{
			CheckDifferent((AgencyShipment shipment) =>
			{
				RefVessel vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "FOO";
				vessel.RV_LloydsNumber = "1111111";
				shipment.Sailing.Voyage.JV_RV_NKVessel = vessel.RV_FK;
			});
		}

		public void TestCannotFindBusinessObjectWhenVoyageIsDifferent()
		{
			CheckDifferent((AgencyShipment shipment) =>
			{
				shipment.Sailing.Voyage.JV_VoyageFlight = "666";
			});
		}

		public void TestCannotFindBusinessObjectWhenLoadPortIsDifferent()
		{
			CheckDifferent((AgencyShipment shipment) =>
			{
				JobSailing sailing = GetSailing("AQMCM", shipment.JS_NKDischargePort, ZDateTime.Now);
				shipment.JS_JX = sailing.PK;
			});
		}

		public void TestCannotFindBusinessObjectWhenDischargePortIsDifferent()
		{
			CheckDifferent((AgencyShipment shipment) =>
			{
				JobSailing sailing = GetSailing(shipment.JS_NKLoadPort, "AQMCM", ZDateTime.Now);
				shipment.JS_JX = sailing.PK;
			});
		}

		public void TestSendingReceivingAgents()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_Code = "SND_Agent";
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.MainAddress.OA_Code = "SND Address";
			var wrongAddress = sendingAgent.Addresses.AddNew();
			wrongAddress.OA_Code = "WRG Address";
			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_Code = "RCV_Agent";
			receivingAgent.OH_FullName = "Receiving Agent";
			receivingAgent.MainAddress.OA_Code = "RCV Main Address";
			var receivingAddress = receivingAgent.Addresses.AddNew();
			receivingAddress.OA_Code = "RCV Address";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var appointedPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedPort.O5_PortOrCountry = "AU";
			appointedPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			appointedPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedPort.O5_PortOrCountry = "USLAX";
			appointedPort.O5_OA_AgentOfficeAddress = receivingAddress.PK;
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var adapter = new AgencyShipmentValueObjectDataAdapter<BillOfLading>();
			var value = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("SND_Agent", value.SendingAgent.Organisation.EDICode);
			AssertEquals(1, value.SendingAgent.Organisation.OrganisationDetails.Addresses.Count);
			AssertEquals("SND Address", value.SendingAgent.Organisation.OrganisationDetails.Addresses[0].AddressCode);
			AssertEquals("RCV_Agent", value.ReceivingAgent.Organisation.EDICode);
			AssertEquals(1, value.ReceivingAgent.Organisation.OrganisationDetails.Addresses.Count);
			AssertEquals("RCV Address", value.ReceivingAgent.Organisation.OrganisationDetails.Addresses[0].AddressCode);
		}

		public void TestOriginDestination()
		{
			var eTD = new ZDateTime(2010, 10, 1);
			var eTA = new ZDateTime(2010, 11, 1);
			var aUSYD = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nLAMS = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NLAMS");
			var aUBNE = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			var gBLON = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			var xsdShipmentOld = new Xsd.AgencyBillOfLading()
			{ BillNumber = "Blaticus", Origin = Xsd.UNLOCO.FromPort(aUSYD), Destination = Xsd.UNLOCO.FromPort(nLAMS), };
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var shipment = adapter.CreateOrUpdateFromValueObject(xsdShipmentOld, context);
			AssertEquals("Origin", aUSYD.RL_Code, shipment.JS_RL_NKOrigin);
			AssertEquals("Origin ETD", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("Destination", nLAMS.RL_Code, shipment.JS_RL_NKDestination);
			AssertEquals("Destination ETA", ZDateTime.Empty, shipment.JS_E_ARV);
			var xsdShipmentNew = new Xsd.AgencyBillOfLading()
			{
				BillNumber = "Blaticus",
				Origin = Xsd.UNLOCO.FromPort(aUSYD),
				PortOfOrigin = new Xsd.Movement()
				{ Port = Xsd.UNLOCO.FromPort(aUBNE), EstimatedDateTime = eTD },
				Destination = Xsd.UNLOCO.FromPort(nLAMS),
				PortOfDestination = new Xsd.Movement()
				{ Port = Xsd.UNLOCO.FromPort(gBLON), EstimatedDateTime = eTA },
			};
			adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			shipment = adapter.CreateOrUpdateFromValueObject(xsdShipmentNew, context);
			AssertEquals("Origin", aUBNE.RL_Code, shipment.JS_RL_NKOrigin);
			AssertEquals("Origin ETD", eTD, shipment.JS_E_DEP);
			AssertEquals("Destination", gBLON.RL_Code, shipment.JS_RL_NKDestination);
			AssertEquals("Destination ETA", eTA, shipment.JS_E_ARV);
		}

		#region Test Matching Is Not Case Sensitive
		public void TestMatchingIsNotCaseSensitive()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "BANO200";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "GBLON";
			var bill = Factory.New<AgencyShipment>();
			bill.JS_HouseBill = "HOUSEBILL";
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_RL_NKOrigin = "AUBNE";
			bill.JS_RL_NKDestination = "GBLON";
			Factory.Save();
			var shipmentValueObject = new Xsd.AgencyBillOfLading();
			shipmentValueObject.BillNumber = "housebill";
			shipmentValueObject.Load = new Xsd.UNLOCO()
			{ Value = "auBNE" };
			shipmentValueObject.Discharge = new Xsd.UNLOCO()
			{ Value = "gblon" };
			shipmentValueObject.Sailing = new Xsd.SailingWithVesselVoyage();
			shipmentValueObject.Sailing.VesselName = "BanoWati";
			shipmentValueObject.Sailing.VoyageNo = "bano200";
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var shipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertEquals("Should update the bill to become a shipment, and not create a new shipment despite being different cases", bill.PK, shipment.PK);
			shipmentValueObject.Sailing.VesselName = "New Vessel";
			var newShipment = adapter.CreateOrUpdateFromValueObject(shipmentValueObject, context);
			AssertNotEquals("Should create a new shipment from the bill as the vessel name does not match", bill.PK, newShipment.PK);
		}

		#endregion
		#region DocDataValueObjectDataAdapter
		public void TestExportAgencyShipmentWithDocData()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocumentNote docNote = DocumentNote.LoadNote(shipment);
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			field.S1_Name = "Notify Party";
			StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);
			docNote.SystemDefinedFieldWrappers.Add(wrapper);
			docNote.SetSystemDefinedFieldValue("Notify Party", "Test");
			ValueObjectExportContext expContext = new ValueObjectExportContext(new NotificationBuffer());
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			Xsd.AgencyBillOfLading xsdShipment = adapter.ExportToValueObject(shipment, expContext);
			AssertEquals(1, xsdShipment.DocData.SystemDefinedData.Count);
			AssertEquals("Notify Party", xsdShipment.DocData.SystemDefinedData[0].Name);
			AssertEquals("Test", xsdShipment.DocData.SystemDefinedData[0].Value);
		}

		public void TestImportAgencyShipmentWithDocData()
		{
			Xsd.AgencyBillOfLading xsdShipment = new Xsd.AgencyBillOfLading();
			var definedData = xsdShipment.DocData.SystemDefinedData.AddNew();
			definedData.Name = "Notify Party";
			definedData.Value = "Test";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocumentNote docNote = DocumentNote.LoadNote(shipment);
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			field.S1_Name = "Notify Party";
			StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);
			docNote.SystemDefinedFieldWrappers.Add(wrapper);
			docNote.SetSystemDefinedFieldValue("Notify Party", "Before Test");
			ValueObjectImportContext impContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			adapter.ImportFromValueObject(shipment, xsdShipment, impContext);
			AssertEquals(1, docNote.GetSystemDefinedFieldList().Count);
			AssertEquals("Test", docNote.GetSystemDefinedFieldValue("Notify Party"));
		}

		#endregion
		#region Implementation
		delegate void MakeDifferentDelegate(AgencyShipment shipment);
		void CheckDifferent(MakeDifferentDelegate makeDifferent)
		{
			AgencyShipment shipment = ImportAgencyShipment();
			makeDifferent(shipment);
			Factory.Save();
			AgencyShipment shipment2 = ImportAgencyShipment(delegate
			{
				Fail("Shouldn't have asked the user anything, because we shouldn't have found an existing business object.");
			});
			AssertNotEquals(shipment, shipment2);
		}

		ZDecimal GetOversizeValue(SchemaDecimalColumn column)
		{
			return (decimal)Math.Pow(10, column.Precision - column.Scale);
		}

		AgencyShipment ImportAgencyShipment(TestNotificationBuffer.QueryUserDelegate queryUser)
		{
			return ImportAgencyShipment(new TestNotificationBuffer(queryUser));
		}

		AgencyShipment ImportAgencyShipment(TestNotificationBuffer.QueryUserDelegate queryUser, ZString loadPort, ZString dischargePort, ZString vesselName)
		{
			return ImportAgencyShipment(new TestNotificationBuffer(queryUser), loadPort, dischargePort, vesselName);
		}

		AgencyShipment ImportAgencyShipment()
		{
			return ImportAgencyShipment(new NotificationBuffer(), "NZAKL", "AUSYD", "ADMIRALENGRACHT");
		}

		AgencyShipment ImportAgencyShipment(INotifications notifications)
		{
			return ImportAgencyShipment(notifications, "NZAKL", "AUSYD", "ADMIRALENGRACHT");
		}

		AgencyShipment ImportAgencyShipment(INotifications notifications, ZString loadPort, ZString dischargePort, ZString vesselName)
		{
			Xsd.AgencyBillOfLading valueShipment = new Xsd.AgencyBillOfLading();
			valueShipment.BillNumber = "SHIPMENT123";
			valueShipment.Load.Value = loadPort;
			valueShipment.Load.IsSpecified = true;
			valueShipment.Discharge.Value = dischargePort;
			valueShipment.Discharge.IsSpecified = true;
			valueShipment.Sailing.VesselName = vesselName;
			valueShipment.Sailing.VoyageNo = "123S";
			valueShipment.Sailing.IsSpecified = true;
			Xsd.Package valuePackage = valueShipment.Packages.AddNew();
			valuePackage.NumberOfPacks = 7;
			AgencyShipmentValueObjectDataAdapter<AgencyShipment> adapter = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
			AgencyShipment shipment = adapter.CreateOrUpdateFromValueObject(valueShipment, new ValueObjectImportContext(Factory, notifications));
			Factory.Save();
			return shipment;
		}

		JobSailing GetSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		protected override string ExpectedRootCollectionElementName
		{
			get
			{
				return "AgencyBillsOfLading";
			}
		}

		protected override string ExpectedRootElementName
		{
			get
			{
				return "AgencyBillOfLading";
			}
		}

		protected override BusinessObjectSampleAndExpectedOutput GetEmptyBusinessObjectSampleAndExpectedOutput()
		{
			return new BusinessObjectAndExpectedOutputResource(Factory.New<AgencyShipment>(), "Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.EmptyAgencyBillOfLading.xml", ValidationKind.None, "Empty BillOfLading");
		}

		protected override BusinessObjectSampleAndExpectedOutput GetFullyPopulatedBusinessObjectSampleAndExpectedOutput()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_IsForwarder = true;
			principal.OH_Code = "PRINCIPAL";
			principal.OH_FullName = "Principal";
			principal.MainAddress.OA_Address1 = "Principal Address";
			principal.MainAddress.OA_Code = "PRADDR";
			OrgHeader sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_Code = "SND_Agent";
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.MainAddress.OA_Address1 = "Sending Main Address";
			sendingAgent.MainAddress.OA_Code = "SND Address";
			OrgHeader receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_Code = "RCV_Agent";
			receivingAgent.OH_FullName = "Receiving Agent";
			receivingAgent.MainAddress.OA_Address1 = "Receiving Address";
			receivingAgent.MainAddress.OA_Code = "RCV Address";
			OrgCarrierAppointedAgentPorts appointedPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedPort.O5_PortOrCountry = "AU";
			appointedPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			appointedPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedPort.O5_PortOrCountry = "AQMCM";
			appointedPort.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			var voyageExRates = Factory.Load<VoyageExRate>(new ZQuery());
			voyageExRates.DeleteAll();
			var exRates = (BusinessObject[])Factory.Load<IExchangeRate>(new ZQuery());
			exRates.DeleteAll();
			JobHeader header = new JobHeader.Loader(shipment).TryLoadOrCreate();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header, "FRT", AgencyInvoiceTypesList.Codes.ForeignCollect, "USD", 1500, "USD", 1000, "DEBT1", "CRED1");
			JobCharge charge = AddCharge(header, "ODOC", AgencyInvoiceTypesList.Codes.LocalPrePaid, "AUD", 500, "AUD", 200, "DEBT2", "CRED2");
			header.JH_OA_LocalChargesAddr = charge.SellAccount.MainAddress.PK;
			shipment.JS_HouseBill = "OBL123";
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUADL";
			shipment.JS_RL_NKDestination = "AQMCM";
			RefVessel vessel = RefVessel.LookupVesselByCode("FOOMONKEY", Factory);
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "FOOMONKEY";
				vessel.RV_LloydsNumber = "1234567";
				vessel.RV_CarrierCode = "FKEY";
			}

			OrgHeader carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CARRIER"));
			if (carrier == null)
			{
				var creationFactory = new BusinessObjectFactory();
				carrier = creationFactory.New<OrgHeader>();
				carrier.MainAddress.OA_Address1 = "Carrier Address";
				carrier.MainAddress.OA_Code = "CRADDR";
				carrier.OH_Code = "CARRIER";
				carrier.OH_FullName = "Carrier";
				carrier.OH_IsShippingLine = true;
				carrier.OH_IsShippingProvider = true;
				// Carrier would be used for voyage matching, should be in database, overwise org matching would not find it
				creationFactory.Save();
			}

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voy123";
			voyage.JV_OH_Line = carrier.PK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2007, 06, 23, 11, 30, 00);
			origin.JA_A_DEP = new ZDateTime(2007, 06, 23, 11, 35, 00);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2007, 06, 28, 11, 00, 00);
			destination.JB_A_ARV = new ZDateTime(2007, 06, 28, 11, 05, 00);
			voyage.GenerateSailings();
			shipment.Sailings.Add(voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL"));
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_CFSReference = "booking"; // NB: This is actually the booking reference
			shipment.JS_BookingReference = "shippers"; // NB: This is actually the shipper's reference
			shipment.JS_InterimReceipt = "interim";
			shipment.JS_GoodsDescription = "goodsdesc";
			shipment.JS_MarksAndNumbers = "marks";
			shipment.JS_RS_NKServiceLevel = "PER";
			shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;
			shipment.CustomsEntryNumber = "FEFEFE";
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Bag;
			shipment.JS_OuterPacks = 17;
			shipment.JS_ActualWeight = 15.5;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 3.2;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2007, 06, 26, 10, 0, 0);
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2007, 06, 24, 12, 0, 0);
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Clean;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_E_DEP = new ZDateTime(2007, 06, 23, 11, 30, 00);
			shipment.JS_E_ARV = new ZDateTime(2007, 06, 28, 11, 00, 00);
			CusEntryNumber number = shipment.Numbers.AddNew();
			number.CE_RN_NKCountryCode = "AU";
			number.CE_EntryType = "COC";
			number.CE_EntryNum = "MAGIC";
			shipment.OuterPackLines.RemoveAndDeleteAll();
			{
				AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
				container.JC_ContainerNum = "AAAA1111113";
				container.JC_ContainerMode = Constants.ContainerModes.FCL;
				container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20FR").PK;
				container.JC_SealNum = "SEALSEAL";
				container.JC_TareWeight = 2543.1;
				container.JC_IsEmptyContainer = false;
				container.JC_IsDamaged = false;
				container.JC_IsShipperOwned = false;
				container.JC_SetPointTemp = 11;
				container.JC_SetPointTempUnit = "C";
				container.JC_HumidityPercent = 22;
				container.JC_AirVentFlow = 33;
				container.JC_AirVentFlowRateUnit = "P1";
				container.JC_ReleaseNum = "Release";
				{
					AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 19;
					packLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
					packLine.JL_MarksAndNumbers = "packmarks";
					packLine.JL_Description = "packdescription";
					packLine.JL_DetailedDescription = "packDetailedDescription1";
					packLine.JL_ActualVolume = 1.868;
					packLine.JL_ActualVolumeUQ = "M3";
					packLine.JL_ActualWeight = 505.1;
					packLine.JL_ActualWeightUQ = "KG";
					packLine.JL_Length = 10;
					packLine.JL_Width = 20;
					packLine.JL_Height = 30;
					packLine.JL_UnitOfDimension = "IN";
					packLine.JL_RefNumber = "ABCD123467890";
					RefCommodityCode commodityCode = Factory.New<RefCommodityCode>();
					commodityCode.RH_Code = "CMM";
					packLine.JL_RH_NKCommodityCode = commodityCode.RH_Code;
					packLine.JL_JC = container.PK;
				}

				{
					AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 2;
					packLine.JL_F3_NKPackType = Constants.PkgUnit.Box;
					packLine.JL_MarksAndNumbers = "packmarks2";
					packLine.JL_Description = "packdescription2";
					packLine.JL_DetailedDescription = "packDetailedDescription2";
					packLine.JL_ActualVolume = 2.0;
					packLine.JL_ActualVolumeUQ = "M3";
					packLine.JL_ActualWeight = 10.0;
					packLine.JL_ActualWeightUQ = "KG";
					packLine.JL_JC = container.PK;
				}
			}

			{
				AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
				container.JC_ContainerNum = "BBBB2222222";
				container.JC_ContainerMode = Constants.ContainerModes.LCL;
				container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("40FR").PK;
				container.JC_SealNum = "SEALHA";
				container.JC_TareWeight = 2500.0;
				container.JC_IsEmptyContainer = true;
				container.JC_IsDamaged = true;
				container.JC_IsShipperOwned = true;
				container.JC_SetPointTempUnit = "C";
			}

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "consignor";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "cnora1";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "cnora2";
			shipment.ConsignorDocumentaryAddress.E2_State = "NSW";
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "consignee";
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "cneea1";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "cneea2";
			shipment.ConsigneeDocumentaryAddress.E2_State = "NSW";
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "notify";
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ntfya1";
			shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "ntfya2";
			shipment.NotifyPartyDocumentaryAddress.E2_State = "NSW";
			shipment.NotifyPartyDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "consigneepad";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "cnepa1";
			shipment.ConsigneeDeliveryAddress.E2_Address2 = "cnepa2";
			shipment.ConsigneeDeliveryAddress.E2_State = "NSW";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";
			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.BookingPartyDocumentaryAddress.E2_CompanyName = "booking";
			shipment.BookingPartyDocumentaryAddress.E2_Address1 = "booka1";
			shipment.BookingPartyDocumentaryAddress.E2_Address2 = "booka2";
			shipment.BookingPartyDocumentaryAddress.E2_State = "NSW";
			shipment.BookingPartyDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.JS_NoOriginalBills = 3;
			shipment.JS_NoCopyBills = 2;
			ZQuery dgFilter = new ZQuery();
			dgFilter.AddToFilter(UNDGSubstanceSchema.DG_UNNO, "2478");
			dgFilter.AddToFilter(UNDGSubstanceSchema.DG_Variant, "c");
			dgFilter.AddToFilter(UNDGSubstanceSchema.DG_Class, "3");
			shipment.OuterPackLines[0].UNDGs.AddNew().DI_DG = Factory.LoadTop1<UNDGSubstance>(dgFilter).PK;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very long detailed goods description");
			return new BusinessObjectAndExpectedOutputResource(shipment, "Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.PopulatedAgencyBillOfLading.xml", ValidationKind.None, "Populated BillOfLading");
		}

		protected override ValueObjectDataAdapter<AgencyShipment, Xsd.AgencyBillOfLading> GetNewBizObjXmlDataAdapter()
		{
			return new AgencyShipmentValueObjectDataAdapter<AgencyShipment>();
		}

		protected override BusinessObjectSampleAndExpectedOutput GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput()
		{
			return GetEmptyBusinessObjectSampleAndExpectedOutput();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[] { "Containers/Seal3", "Containers/Seal2", "Load/Country", "Load/City", "Addresses/DocAddress/StateOrProvince", "Addresses/DocAddress/AddressType", "Addresses/DocAddress/AddressCode", "Addresses/DocAddress/Email", "Addresses/DocAddress/CountryCode", "Addresses/DocAddress/CompanyName", "Addresses/DocAddress/ContactName", "Addresses/DocAddress/IsResidential", "Addresses/DocAddress/PostCode", "Addresses/DocAddress/TelephoneNumbers/Value", "Addresses/DocAddress/CityOrSuburb", "Addresses/DocAddress/AddressReference/Organisation", "Addresses/DocAddress/AddressLine2", "Addresses/DocAddress/AddressLine1", "Addresses/DocAddress/RegistrationNumber", "Billing/ExchangeRates", "Sailing/DocCutOffDate", "Sailing/IsTranshipment", "Sailing/ArrivalBerth", "Sailing/IsPublished", "Sailing/ATA", "Sailing/LCLDates/StorageDate", "Sailing/LCLDates/AvailableDate", "Sailing/LCLDates/ReceivalCommencesDate", "Sailing/LCLDates/CutOffDate", "Sailing/ATD", "Sailing/ETA", "Sailing/ETD", "Sailing/DepartureBerth", "Sailing/DepartureCTO/Organisation", "Sailing/ArrivalCTO/Organisation", "Sailing/ArrivalCTO/Organisation/EDICode", "Sailing/FCLDates/StorageDate", "Sailing/FCLDates/AvailableDate", "Sailing/FCLDates/ReceivalCommencesDate", "Sailing/FCLDates/CutOffDate", "Sailing/DepartureCTO/Organisation/OrganisationDetails/Addresses/Language", "Sailing/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Language", "Sailing/LoadPortETA", "Sailing/LoadPortATA", "Sailing/Carrier", "PackageSummary/DGContact/Organisation/OrganisationDetails/Addresses/Language", "Packages/DGContact/Organisation/OrganisationDetails/Addresses/Language", "Addresses/DocAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "Addresses/DocAddress/Language", "PackageSummary/HazardousGoods/FlashPoint", "PackageSummary/HazardousGoods/IMOClass", "PackageSummary/HazardousGoods/UNDGCode", "PackageSummary/HazardousGoods/ProperShippingName", "PackageSummary/GoodsDescription", "PackageSummary/DGContact/Organisation", "PackageSummary/Origin", "PackageSummary/Width/Description", "PackageSummary/Width/DimensionType", "PackageSummary/Height/Description", "PackageSummary/Height/DimensionType", "PackageSummary/MarksAndNumbers", "PackageSummary/Weight/Description", "PackageSummary/Length/Description", "PackageSummary/Length/DimensionType", "PackageSummary/ContainerNumber", "PackageSummary/Volume/Description", "PackageSummary/CommodityCode", "PackageSummary/RefNumber", "PackageSummary/Custom/Date1", "PackageSummary/Custom/Date2", "PackageSummary/Custom/Text1", "PackageSummary/Custom/Text2", "PackageSummary/Custom/Text3", "PackageSummary/Custom/Text4", "PackageSummary/Custom/Decimal1", "PackageSummary/Custom/Decimal2", "PackageSummary/Custom/Flag1", "PackageSummary/Custom/Flag2", "Packages/Custom/Date2", "Packages/Custom/Text1", "Packages/Custom/Text2", "Packages/Custom/Text3", "Packages/Custom/Text4", "Packages/Custom/Decimal1", "Packages/Custom/Decimal2", "Packages/Custom/Flag1", "Packages/Custom/Flag2", "Containers/IsArrivingAtCTOByRail", "Containers/CommodityCode", "Containers/FCLAvailable", "Containers/EstimatedDelivery", "Containers/DeliveryMode", "Containers/LCLAvailable", "Containers/ContainerType/Length", "Containers/ContainerType/Height", "Containers/ContainerType/Width", "Containers/ContainerType/ISOCode", "Containers/ContainerType/USContainerCode", "Containers/BookingReference", "Containers/ImportProcess", "Containers/ExportProcess", "Containers/Custom", "Discharge/Country", "Discharge/City", "Origin/Country", "Origin/City", "Destination/Country", "Destination/City", "Packages/HazardousGoods/FlashPoint", "Packages/HazardousGoods/IMOClass", "Packages/HazardousGoods/UNDGCode", "Packages/HazardousGoods/ProperShippingName", "Packages/HazardousGoods/MarinePollutant", "Packages/HazardousGoods/PackingGroup", "Packages/DGContact/Organisation", "Packages/Origin", "Packages/Width/Description", "Packages/Height/Description", "Packages/Weight/Description", "Packages/Length/Description", "Packages/Volume/Description", "PackageSummary/PackageID", "PackageSummary/TransportRef", "PackageSummary/LoadingMeters", "PackageSummary/HarmonisedCode", "PackageSummary/HazardousGoods/MarinePollutant", "PackageSummary/HazardousGoods/PackingGroup", "PackageSummary/DangerousGoods", "Packages/PackageID", "Packages/TransportRef", "Packages/LoadingMeters", "Packages/HarmonisedCode", "Packages/Custom/Date1", "Carrier", "Principal", "Billing/ChargeLines/Debtor", "Billing/ChargeLines/Creditor", "Billing/LocalClient", "Billing/ChargeLines/LocalSellAmount/CurrencyCode", "Billing/ChargeLines/LocalCostAmount/CurrencyCode", "Billing/ChargeLines/SellExchangeRate", "Billing/ChargeLines/CostExchangeRate", "Billing/ChargeLines/InvoiceNumber", "Billing/ChargeLines/InvoiceDate", "Billing/ChargeLines/SellRatingOverride", "Billing/ChargeLines/RevenueIsPosted", "Billing/ChargeLines/CostIsPosted", "Billing/ChargeLines/ARInvoiceNumber", "Billing/ChargeLines/SupplierReference", "Notes/CustomNoteTypeName", "Notes/NoteCreatedDateTime", "PackageSummary/HazardousGoods", "Packages/DangerousGoods", "Packages/HazardousGoods", "PackageSummary/PackageProducts", "Packages/PackageProducts", "SendingAgent/Organisation/OrganisationDetails", "SendingAgent/Organisation/Notes", "ReceivingAgent/Organisation/OrganisationDetails", "ReceivingAgent/Organisation/Notes", "PortOfOrigin/ActualDateTime", "PortOfDestination/ActualDateTime", "DocData/SystemDefinedData/Category", "DocData/SystemDefinedData/Name", "DocData/SystemDefinedData/Value", "DocData/UserDefinedData/Name", "DocData/UserDefinedData/Value", };
			}
		}

		static string[] ChargesAsStrings(JobHeader header)
		{
			List<string> result = new List<string>();
			if (header != null)
			{
				foreach (JobCharge charge in (IBusinessObjectCollection)header["Charges"])
				{
					const string format = "Code: {0}\r\n" + "Description: {1}\r\n" + "Sell Amt: {2:0.000} {3}\r\n" + "";
					result.Add(string.Format(format, charge.ChargeCode == null ? "<null>" : charge.ChargeCode.AC_Code.ToString(), charge.JR_Desc, charge.JR_OSSellAmt, charge.SellCurrency == null ? "<null>" : charge.JR_RX_NKSellCurrency.ToString()));
				}
			}

			return result.ToArray();
		}

		static JobCharge AddCharge(JobHeader header, string chargeCode, string invoiceType, string sellCurrencyCode, decimal sellAmount, string costCurrencyCode, decimal costAmount, string debtorCode, string creditorCode)
		{
			ZQuery chargeCodeFilter = new ZQuery();
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);
			IBusinessObjectCollection charges = (IBusinessObjectCollection)header["Charges"];
			JobCharge charge = (JobCharge)charges.AddNew();
			charge.JR_JH = header.PK;
			charge.JR_AC = header.Factory.LoadTop1<AccChargeCode>(chargeCodeFilter).PK;
			charge.JR_RX_NKSellCurrency = sellCurrencyCode;
			charge.JR_OSSellAmt = sellAmount;
			charge.JR_RX_NKCostCurrency = costCurrencyCode;
			charge.JR_OSCostAmt = costAmount;
			BusinessObjectFactory factory = header.Factory;
			OrgHeader debtor = factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = debtorCode;
			debtor.OH_FullName = debtorCode + " full name";
			debtor.OH_IsDebtor = true;
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			OrgHeader creditor = factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = creditorCode;
			creditor.OH_FullName = creditorCode + " full name";
			creditor.OH_IsCreditor = true;
			creditor.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			if (anotherFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, debtorCode)) == null)
			{
				factory.Save();
			}

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_InvoiceType = invoiceType;
			return charge;
		}

		static void AddCharge(GlbDepartment department, string chargeCode, byte seqNum)
		{
			ZQuery chargeCodeFilter = new ZQuery();
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			GlbDeptCharges charge = department.DeptCharges.AddNew();
			charge.GD_AC = department.Factory.LoadTop1<AccChargeCode>(chargeCodeFilter).PK;
			charge.GD_SequenceNumber = seqNum;
		}

		Xsd.Container[] GetContainersArray(Xsd.ContainerCollection containers)
		{
			Xsd.Container[] result = new Xsd.Container[containers.Count];
			((IList)containers).CopyTo(result, 0);
			return result;
		}

		Xsd.Package[] GetPackagesArray(Xsd.PackageCollection packages)
		{
			Xsd.Package[] result = new Xsd.Package[packages.Count];
			((IList)packages).CopyTo(result, 0);
			return result;
		}

		Xsd.AgencyBillOfLading GetBOLFromXml(string resourceName)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.AgencyBillOfLading));
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				return (Xsd.AgencyBillOfLading)serializer.Deserialize(stream);
			}
		}

		class TestNotificationBuffer : NotificationBuffer
		{
			public TestNotificationBuffer(QueryUserDelegate queryUser) : base()
			{
				this.queryUser = queryUser;
			}

			public delegate void QueryUserDelegate(IQueryUserEventArgs e);
			protected override void QueryUser(IQueryUserEventArgs e)
			{
				queryUser(e);
			}

			readonly QueryUserDelegate queryUser;
		}

		void SetupPortCodeMapping()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy == null)
			{
				orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
			}

			var portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "EFG";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).PK;
			portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "ABC";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL")).PK;
		}
		#endregion
	}
}

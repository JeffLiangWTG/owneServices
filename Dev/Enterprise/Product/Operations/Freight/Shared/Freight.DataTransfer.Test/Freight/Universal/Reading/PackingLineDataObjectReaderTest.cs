using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class PackingLineDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBizObjectFinder()
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsDescription = "iPhones"
			};

			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Description = "frozen ducks";

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => packLine);
			reader.ReadIntoBusinessObject();

			AssertEquals("iPhones", packLine.JL_Description);
		}

		public void TestBizObjectCreator()
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsDescription = "iPhones"
			};

			var shipment = Factory.New<CommonShipment>();

			AssertEquals("prerequisite", 0, shipment.OuterPackLines.Count);

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(
				dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);

			reader.ReadIntoBusinessObject();

			AssertEquals("prerequisite", 1, shipment.OuterPackLines.Count);
			AssertEquals("iPhones", shipment.OuterPackLines[0].JL_Description);
		}

		public void TestImportClassification()
		{
			var shipment = Factory.New<CommonShipment>();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.GoodsDescription = "Line001";

			var classification1 = new Classification();
			classification1.Code = "HS1";
			classification1.Country = new Country { Code = "AU", Name = "Australia" };
			classification1.Type = new CodeDescriptionPair { Code = "HSC", Description = "Harmonized Code" };

			var classification2 = new Classification();
			classification2.Code = "HS2";
			classification2.Country = new Country { Code = "AU", Name = "Australia" };
			classification2.Type = new CodeDescriptionPair { Code = "XXX", Description = "Code" };

			dataObject.SetClassificationCollection(() => new DataObjectList<Classification>());
			dataObject.ClassificationCollection.Content = CollectionContent.Partial;
			dataObject.ClassificationCollection.AddRange(new[] { classification1, classification2 });

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("PackingLine should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("Only one harmonised code should be added", 1, shipment.OuterPackLines[0].HarmonisedCodes.Count);

			var harmoisedCode = shipment.OuterPackLines[0].HarmonisedCodes[0];
			AssertEquals("HS1", harmoisedCode.JLH_Code);
			AssertEquals("AU", harmoisedCode.JLH_RN_NKCountry);
		}

		public void TestImportClassification_Partial()
		{
			var shipment = Factory.New<CommonShipment>();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.GoodsDescription = "Line001";

			var classification1 = UniversalTestHelper.CreateClassificationDataObject("HS1", "HSC", "Harmonized Code", "AU", "Australia");
			var classification2 = UniversalTestHelper.CreateClassificationDataObject("HS2", "XXX", "XXX Code", "AU", "Australia");

			dataObject.SetClassificationCollection(() => new DataObjectList<Classification>());
			dataObject.ClassificationCollection.Content = CollectionContent.Partial;
			dataObject.ClassificationCollection.AddRange(new[] { classification1, classification2 });

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("PackingLine should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("Only one harmonised code should be added", 1, shipment.OuterPackLines[0].HarmonisedCodes.Count);

			var harmoisedCode = shipment.OuterPackLines[0].HarmonisedCodes[0];
			AssertEquals("HS1", harmoisedCode.JLH_Code);
			AssertEquals("AU", harmoisedCode.JLH_RN_NKCountry);
		}

		public void TestRoundingRoundDownDecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			var registryEntryDimension = new DefaultNumberOfDecimals();
			registryEntryDimension.UnitOfMeasure = Core.Constants.Dimension.Metres;
			registryEntryDimension.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryDimension.NumberOfDecimals = 1;
			registryEntryDimension.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryDimension);

			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			dataObject.Commodity = new Commodity() { Code = "GEN" };
			dataObject.DetailedDescription = "SOARE PARTS INV:";
			dataObject.GoodsDescription = "SPARE PARTS INV:";
			dataObject.MarksAndNos = "NGN:D2310789PR, REF:Antonio";
			dataObject.PackQty = 8;
			dataObject.PackType = new PackageType() { Code = "BOX" };
			dataObject.Volume = 999999.999m;
			dataObject.Weight = 999999.999m;
			dataObject.Length = 999999.999m;
			dataObject.Height = 999999.999m;
			dataObject.Width = 999999.999m;
			dataObject.OutturnedLength = 999999.999m;
			dataObject.OutturnedWidth = 999999.999m;
			dataObject.OutturnedHeight = 999999.999m;
			dataObject.OutturnedVolume = 999999.999m;
			dataObject.OutturnedWeight = 999999.999m;
			dataObject.WeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms };
			dataObject.VolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicMetres };
			dataObject.LengthUnit = new UnitOfLength { Code = Core.Constants.Dimension.Metres };

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				readerToTest.ReadIntoBusinessObject();
			}

			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_ActualVolume);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_ActualWeight);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_Length);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_Height);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_Width);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_OutturnedLength);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_OutturnedHeight);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_OutturnedWidth);
			AssertEquals((ZDecimal)999999.9, shipment.OuterPackLines[0].JL_OutturnedWeight);
		}

		public void TestImportCustomValues()
		{
			SetupCustomValuesRegistry();

			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			dataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				new CustomizedField { Key = "CustomDate1", Value = (new ZDateTime(2012, 1, 1)).ToISO8601String(), DataType = DataType.DateTime },
				new CustomizedField { Key = "CustomDate2", Value = (new ZDateTime(2012, 1, 2)).ToISO8601String(), DataType = DataType.DateTime },

				new CustomizedField { Key = "CustomDecimal1", Value = 11m.ToString(), DataType = DataType.Decimal },
				new CustomizedField { Key = "CustomDecimal2", Value = 22m.ToString(), DataType = DataType.Decimal },

				new CustomizedField { Key = "CustomFlag1", Value = false.ToString(), DataType = DataType.Boolean },
				new CustomizedField { Key = "CustomFlag2", Value = true.ToString(), DataType = DataType.Boolean },

				new CustomizedField { Key = "CustomAttrib1", Value = "aaa", DataType = DataType.String },
				new CustomizedField { Key = "CustomAttrib2", Value = "bbb", DataType = DataType.String },
				new CustomizedField { Key = "CustomAttrib3", Value = "ccc", DataType = DataType.String },
				new CustomizedField { Key = "CustomAttrib4", Value = "ddd", DataType = DataType.String }
			});

			var shipment = Factory.New<CommonShipment>();

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(
				dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);

			AssertNoExceptionThrown("bad data didn't cause the import to flip out", () => reader.ReadIntoBusinessObject());
			AssertNoExceptionThrown("bad data didn't cause save exception", Factory.SaveForTesting);

			AssertEquals("prerequisite - packline created", 1, shipment.OuterPackLines.Count);
			AssertEquals("prerequisite - packline saved", true, shipment.OuterPackLines[0].IsInDatabase);

			AssertEquals("JL_CustomDate1", new ZDateTime(2012, 1, 1), shipment.OuterPackLines[0].JL_CustomDate1);
			AssertEquals("JL_CustomDate2", new ZDateTime(2012, 1, 2), shipment.OuterPackLines[0].JL_CustomDate2);

			AssertEquals("JL_CustomDecimal1", 11m, shipment.OuterPackLines[0].JL_CustomDecimal1);
			AssertEquals("JL_CustomDecimal2", 22m, shipment.OuterPackLines[0].JL_CustomDecimal2);

			AssertEquals("JL_CustomFlag1", ZBool.False, shipment.OuterPackLines[0].JL_CustomFlag1);
			AssertEquals("JL_CustomFlag2", ZBool.True, shipment.OuterPackLines[0].JL_CustomFlag2);

			AssertEquals("JL_CustomDecimal1", "aaa", shipment.OuterPackLines[0].JL_CustomAttrib1);
			AssertEquals("JL_CustomDecimal2", "bbb", shipment.OuterPackLines[0].JL_CustomAttrib2);
			AssertEquals("JL_CustomDecimal3", "ccc", shipment.OuterPackLines[0].JL_CustomAttrib3);
			AssertEquals("JL_CustomDecimal4", "ddd", shipment.OuterPackLines[0].JL_CustomAttrib4);
		}

		public void TestVolumeIsImportedAfterDimensionsAndOverridesAutocalculatedValue()
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var shipment = Factory.New<CommonShipment>();

			dataObject.Commodity = new Commodity() { Code = "GEN" };
			dataObject.DetailedDescription = "SOARE PARTS INV:";
			dataObject.GoodsDescription = "SPARE PARTS INV:";
			dataObject.Height = 10.0;
			dataObject.Length = 10.0;
			dataObject.LengthUnit = new UnitOfLength() { Code = "CM" };
			dataObject.MarksAndNos = "NGN:D2310789PR, REF:Antonio";
			dataObject.PackQty = 8;
			dataObject.PackType = new PackageType() { Code = "BOX" };
			dataObject.Volume = 0.8;
			dataObject.VolumeUnit = new UnitOfVolume() { Code = "M3" };
			dataObject.Width = 10.0;

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("PackingLine should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("Volume should NOT be recalculated", 0.8m, shipment.OuterPackLines[0].JL_ActualVolume);
		}

		public void TestErrorMessageForInvalidLastKnownTransitWarehouseStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			var packingLineDataObject = SetupPackingLine(Factory);
			PrepareShipmentForSetupPackingLine(shipment);
			packingLineDataObject.LastKnownCFSStatus.Code = "XXX";
			packingLineDataObject.LastKnownCFSStatus.Description = "XXX";

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);

			AssertExceptionThrown("Exception should be thrown when invalid CFS status provided.", typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());

			packingLineDataObject.LastKnownCFSStatus.Code = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packingLineDataObject.LastKnownCFSStatus.Description = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Received;

			AssertNoExceptionThrown("No exceptions should be thrown when valid CFS status provided.", () => reader.ReadIntoBusinessObject());
		}

		public void TestBasicPackingLineLevelFieldMappings()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = Factory.New<CommonShipment>();

			var packingLineDataObject = SetupPackingLine(Factory);
			PrepareShipmentForSetupPackingLine(shipment);
			Factory.SaveForTesting();
			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);
			var packingLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull(packingLineBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("packline has been allocated to parent shipment", shipment.PK, packingLineBO.JL_JS);
				AssertContents(packingLineBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - Matching 'LastKnownCFSFacility':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no contact found in database with name 'Telepuzik' and phone '' for dangerous goods substance code '3000c' (IMO Class = ''). Make sure that name and phone is not empty. If not create contact first.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no substance with code '3001' found. Please use standard dangerous goods substance code.
".Trim(), logger.Logs);
			});

			#endregion
		}

		void PrepareShipmentForSetupPackingLine(CommonShipment shipment)
		{
			var address = shipment.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "OH1")).Addresses.Cast<OrgAddress>().First(addr => addr.OA_Address1 == "AD1");
			shipment.JS_OA_ImportReleaseDepot = address.PK;
			shipment.JS_OA_ExportReceivingDepot = address.PK;
		}

		public void TestReadIntoBusinessObject_CalculatedVolumeIsToBig_ReplaceWithInvalidValue()
		{
			var packlineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packlineDataObject.Length = 1000;
			packlineDataObject.Width = 1000;
			packlineDataObject.Height = 1000;
			packlineDataObject.PackQty = 1;
			packlineDataObject.LengthUnit = new UnitOfLength() { Code = "M" };

			var shipment = Factory.New<CommonShipment>();
			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(packlineDataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("Packline should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("The value is fixed", 999999m, shipment.OuterPackLines[0].JL_ActualVolume);
		}

		public void TestRemoveAllPreExistedUNDGs()
		{
			var packlineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packlineDataObject.Length = 1000;
			packlineDataObject.Width = 1000;
			packlineDataObject.Height = 1000;
			packlineDataObject.PackQty = 1;
			packlineDataObject.LengthUnit = new UnitOfLength() { Code = "M" };

			var shipment = Factory.New<CommonShipment>();
			var packingLineBO = shipment.OuterPackLines.AddNew();
			var undgDataItem = packingLineBO.UNDGs.AddNew();

			undgDataItem.DI_DGVolume = 5m;
			undgDataItem.DI_DGWeight = 8m;

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(packlineDataObject, logger, Factory, shipment, dataObj => packingLineBO, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("Keep the exist PackingLine", packingLineBO.PK, shipment.OuterPackLines[0].PK);
			AssertEquals("Remove all pre-existed UNDGs before populating.", 0, shipment.OuterPackLines[0].UNDGs.Count);
		}

		public void TestPopulateMatchedUNDG()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "123", "a", "IMO").FirstOrDefault();
			subs.DG_Class = "1.1A";

			var packingLineDO = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLineDO.Length = 1000;
			packingLineDO.Width = 1000;
			packingLineDO.Height = 1000;
			packingLineDO.PackQty = 1;
			packingLineDO.LengthUnit = new UnitOfLength() { Code = "M" };

			var undgDO = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);

			undgDO.UNDGCode = subs.DG_Code;
			undgDO.IMOClass = "";
			undgDO.Volume = 100m;
			undgDO.Weight = 200m;

			packingLineDO.SetUNDGCollection(() => new List<UNDG>());
			packingLineDO.UNDGCollection.Add(undgDO);

			var shipment = Factory.New<CommonShipment>();
			var packingLineBO = shipment.OuterPackLines.AddNew();

			var undgBO = packingLineBO.UNDGs.AddNew();
			undgBO.DI_DG = subs.PK;
			undgBO.DI_IMOClass = "";
			undgBO.DI_DGVolume = 5m;
			undgBO.DI_DGWeight = 8m;

			Factory.SaveForTesting();

			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDO, logger, Factory, shipment, dataObj => packingLineBO, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("Keep the exist PackingLine", packingLineBO.PK, shipment.OuterPackLines[0].PK);
			AssertEquals("match and update the old UNDG", 1, shipment.OuterPackLines[0].UNDGs.Count);
			AssertEquals(undgBO.PK, shipment.OuterPackLines[0].UNDGs[0].PK);
			AssertEquals(100m, shipment.OuterPackLines[0].UNDGs[0].DI_DGVolume);
			AssertEquals(200m, shipment.OuterPackLines[0].UNDGs[0].DI_DGWeight);
		}

		public void TestErrorMessageForInvalidRequiredTemperatures()
		{
			var shipment = Factory.New<CommonShipment>();

			var packingLineDataObject = SetupPackingLine(Factory);
			PrepareShipmentForSetupPackingLine(shipment);
			packingLineDataObject.RequiresTemperatureControl = true;

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);

			packingLineDataObject.RequiredTemperatureUnit = null;
			packingLineDataObject.RequiredTemperatureMinimum = null;
			packingLineDataObject.RequiredTemperatureMaximum = null;
			AssertNoExceptionThrown("Existing value should be used if not specified in XML.", () => reader.ReadIntoBusinessObject());

			packingLineDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Kelvin };
			AssertExceptionThrown("Exception should be thrown when invalid temperature unit provided.",
				typeof(DataObjectReadFailureException),
				"Invalid temperature unit (K). Temperature must be set to C (Celsius) or F (Fahrenheit).",
				() => reader.ReadIntoBusinessObject());

			packingLineDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Centigrade };
			packingLineDataObject.RequiredTemperatureMinimum = -273.2m;
			AssertExceptionThrown("Exception should be thrown when invalid minimum temperature in Celsius provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-273.2°C) is below the minimum possible temperature of absolute zero (-273.15°C).",
				() => reader.ReadIntoBusinessObject());

			packingLineDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Fahrenheit };
			packingLineDataObject.RequiredTemperatureMinimum = -459.7m;
			AssertExceptionThrown("Exception should be thrown when invalid minimum temperature in Fahrenheit provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-459.7°F) is below the minimum possible temperature of absolute zero (-459.67°F).",
				() => reader.ReadIntoBusinessObject());

			packingLineDataObject.RequiredTemperatureMinimum = -459.6m;
			packingLineDataObject.RequiredTemperatureMaximum = -459.7m;
			AssertExceptionThrown("Exception should be thrown when invalid maximum temperature provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-459.6°F) cannot be higher than maximum temperature (-459.7°F).",
				() => reader.ReadIntoBusinessObject());

			packingLineDataObject.RequiredTemperatureMaximum = -459.6m;
			AssertNoExceptionThrown("No exceptions should be thrown when valid temperatures provided.", () => reader.ReadIntoBusinessObject());
		}

		public void TestJL_OA_LastKnownTransitWarehouseAddress()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_RL_NKDestination = "MAAGA";
			shipment.JS_RL_NKDischargePort = "SGSIN";
			var consol = shipment.Consols.AddNew();

			consol.JK_OA_PackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
			consol.PackDepotAddress.OA_Address1 = "Pack";
			consol.PackDepotAddress.Header.OH_Code = "PAC";
			consol.JK_OA_UnpackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
			consol.UnpackDepotAddress.OA_Address1 = "Unpack";
			consol.UnpackDepotAddress.Header.OH_Code = "UNP";
			shipment.JS_OA_ExportReceivingDepot = Factory.New<OrgHeader>().MainAddress.PK;
			shipment.ExportReceivingDepot.OA_Address1 = "ExportReceiving";
			shipment.ExportReceivingDepot.Header.OH_Code = "EXP";
			shipment.JS_OA_ImportReleaseDepot = Factory.New<OrgHeader>().MainAddress.PK;
			shipment.ImportReleaseDepot.OA_Address1 = "ImportRelease";
			shipment.ImportReleaseDepot.Header.OH_Code = "IMP";

			var packingLineDataObject = SetupPackingLine(Factory);
			packingLineDataObject.OrganizationAddressCollection[0].AddressShortCode = null;
			Factory.SaveForTesting();
			AssertExceptionThrown<DataObjectReadFailureException>(() => new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject());

			packingLineDataObject.OrganizationAddressCollection[0].AddressShortCode = "AD1";
			Factory.SaveForTesting();
			AssertExceptionThrown<DataObjectReadFailureException>(() => new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject());

			foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
			{
				packingLineDataObject.LastKnownCFSStatus = new CodeDescriptionPair { Code = status };
				foreach (var address in new OrgAddress[] { consol.PackDepotAddress, consol.UnpackDepotAddress, shipment.ExportReceivingDepot, shipment.ImportReleaseDepot })
				{
					packingLineDataObject.OrganizationAddressCollection[0].OrganizationCode = address.Header.OH_Code;
					packingLineDataObject.OrganizationAddressCollection[0].AddressShortCode = address.OA_Address1;

					var packLine = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject();
					AssertEquals(address.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
				}
			}

			consol.PackDepotAddress.Header.Addresses.AddNew();
			consol.UnpackDepotAddress.Header.Addresses.AddNew();
			shipment.ExportReceivingDepot.Header.Addresses.AddNew();
			shipment.ImportReleaseDepot.Header.Addresses.AddNew();
			foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
			{
				packingLineDataObject.LastKnownCFSStatus = new CodeDescriptionPair { Code = status };
				foreach (var address in new OrgAddress[] { consol.PackDepotAddress, consol.UnpackDepotAddress, shipment.ExportReceivingDepot, shipment.ImportReleaseDepot })
				{
					packingLineDataObject.OrganizationAddressCollection[0].OrganizationCode = address.Header.OH_Code;
					packingLineDataObject.OrganizationAddressCollection[0].AddressShortCode = address.OA_Address1;

					if (status == ZString.Empty
						|| (status == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received && (address == shipment.ExportReceivingDepot || address == consol.PackDepotAddress))
						|| (status == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched && (address == shipment.ImportReleaseDepot || address == consol.UnpackDepotAddress)))
					{
						var packLine = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject();
						AssertEquals(address.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
					else
					{
						AssertExceptionThrown<DataObjectReadFailureException>(() => new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject());
					}
				}
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH9";
			orgHeader.MainAddress.OA_Address1 = "AD9";
			foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
			{
				packingLineDataObject.LastKnownCFSStatus = new CodeDescriptionPair { Code = status };
				packingLineDataObject.OrganizationAddressCollection[0].OrganizationCode = orgHeader.OH_Code;
				packingLineDataObject.OrganizationAddressCollection[0].AddressShortCode = orgHeader.MainAddress.OA_Address1;

				var packLine = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject();
				AssertEquals(orgHeader.MainAddress.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			}
		}

		public void TestVehicleWithoutTransmission()
		{
			var shipment = Factory.New<CommonShipment>();

			var packingLineDataObject = SetupPackingLine(Factory);
			packingLineDataObject.Vehicle = new Vehicle()
			{
				Color = "White",
				Make = "WTG",
				Model = "Richard I",
				NumberOfDoors = 5,
				Year = 2023
			};
			PrepareShipmentForSetupPackingLine(shipment);

			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);

			AssertNoExceptionThrown("No null reference exception should be thrown.", () => reader.ReadIntoBusinessObject());

			var packingLineBO = reader.ReadIntoBusinessObject();
			AssertNotNull(packingLineBO);
			AssertEquals("packingLineBO.JL_VehicleColor", "White", packingLineBO.JL_VehicleColor);
			AssertEquals("packingLineBO.JL_VehicleMake", "WTG", packingLineBO.JL_VehicleMake);
			AssertEquals("packingLineBO.JL_VehicleModel", "Richard I", packingLineBO.JL_VehicleModel);
			AssertEquals("packingLineBO.JL_VehicleNumberOfDoors", (ZByte)5, packingLineBO.JL_VehicleNumberOfDoors);
			AssertEquals("packingLineBO.JL_VehicleTransmission", "", packingLineBO.JL_VehicleTransmission);
			AssertEquals("packingLineBO.JL_VehicleYear", (ZShort)2023, packingLineBO.JL_VehicleYear);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		void SetupCustomValuesRegistry()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "CustomAttrib1";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "CustomAttrib2";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "CustomAttrib3";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption = "CustomAttrib4";

			Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption = "CustomDecimal1";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption = "CustomDecimal2";

			Env.Registry.Freight.PackLine.PackLineCustomDate1Caption = "CustomDate1";
			Env.Registry.Freight.PackLine.PackLineCustomDate2Caption = "CustomDate2";

			Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption = "CustomFlag1";
			Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption = "CustomFlag2";
		}

		public static void SetupCFSAddresses(Shipment dataObject)
		{
			var cfsAddresses = new List<OrganizationAddress>();
			cfsAddresses.Add(new OrganizationAddress { AddressType = nameof(MasterFiles.Integration.DocAddressType.ArrivalCFSAddress), AddressShortCode = "AD1", OrganizationCode = "OH1" });
			cfsAddresses.Add(new OrganizationAddress { AddressType = nameof(MasterFiles.Integration.DocAddressType.DepartureCFSAddress), AddressShortCode = "AD1", OrganizationCode = "OH1" });

			if (dataObject.OrganizationAddressCollection == null)
			{
				dataObject.SetOrganizationAddressCollection(() => cfsAddresses);
			}
			else
			{
				dataObject.OrganizationAddressCollection.AddRange(cfsAddresses);
			}
		}

		public static PackingLine SetupPackingLine(UniversalObjectFactory factory)
		{
			var packingLineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			packingLineDataObject.ContainerNumber = "OOCL0000027";
			packingLineDataObject.Commodity = new Commodity() { Code = "FGFG", Description = "Fudge Guts Fingers Gone" };
			packingLineDataObject.ContainerPackingOrder = 1;
			packingLineDataObject.HarmonisedCode = "2103.45.34.12A";
			packingLineDataObject.Height = 88m;
			packingLineDataObject.ItemNo = 1;
			packingLineDataObject.Length = 66m;
			packingLineDataObject.MarksAndNos = "MORE ART RIPPING KNOT";
			packingLineDataObject.CountryOfOrigin = new Country() { Code = "CD", Name = "Congo, The Democratic Republic of t" };
			packingLineDataObject.OutturnQty = 50;
			packingLineDataObject.OutturnDamagedQty = 2;
			packingLineDataObject.OutturnPillagedQty = 48;
			packingLineDataObject.OutturnComment = "AHARRRRR!!!";
			packingLineDataObject.OutturnedHeight = 23.34m;
			packingLineDataObject.OutturnedLength = 34.45m;
			packingLineDataObject.OutturnedVolume = 37.539m;
			packingLineDataObject.OutturnedWeight = 6.78m;
			packingLineDataObject.OutturnedWidth = 56.98m;
			packingLineDataObject.PackQty = 12;
			packingLineDataObject.PackType = new PackageType() { Code = "PAI", Description = "Pail" };
			packingLineDataObject.ReferenceNumber = "FORYOURREFERENCE";
			packingLineDataObject.ExportReferenceNumber = "ExportRefNumber";
			packingLineDataObject.ImportReferenceNumber = "ImportRefNumber";
			packingLineDataObject.LengthUnit = new UnitOfLength() { Code = "IN", Description = "Inches" };
			packingLineDataObject.Volume = 87.943m;
			packingLineDataObject.VolumeUnit = new UnitOfVolume() { Code = "M3", Description = "Cubic Meters" };
			packingLineDataObject.Weight = 2.34m;
			packingLineDataObject.WeightUnit = new UnitOfWeight() { Code = "T", Description = "Tonnes" };
			packingLineDataObject.Width = 77m;
			packingLineDataObject.GoodsDescription = "Detailed Goods Description 01;";
			packingLineDataObject.LoadingMeters = 4.32m;
			packingLineDataObject.EndItemNo = 1;
			packingLineDataObject.LinePrice = 5.43m;
			packingLineDataObject.DetailedDescription = "This is a detailed description to go in the detailed description field.";
			packingLineDataObject.IsHighRisk = true;

			packingLineDataObject.RequiresTemperatureControl = true;
			packingLineDataObject.RequiredTemperatureMinimum = 1.00;
			packingLineDataObject.RequiredTemperatureMaximum = 100.00;
			packingLineDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Core.Constants.Temperature.Centigrade };

			packingLineDataObject.Vehicle = new Vehicle()
			{
				Color = "White",
				Make = "Make 1",
				Model = "Model 1",
				NumberOfDoors = 4,
				Transmission = new CodeDescriptionPair() { Code = "MAN", Description = "Manual" },
				Year = 2015,
			};

			if (UNDGSubstanceLoader.LoadSubstances(factory.BOFactory, "3000", "c", "IMO").FirstOrDefault() == null)
			{
				var subs = factory.New<UNDGSubstance>();
				subs.DG_UNNO = "3000";
				subs.DG_Variant = "c";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var substance = UNDGSubstanceLoader.LoadSubstances(factory.BOFactory, "3000", "c", "IMO").FirstOrDefault();
				substance.DG_FlashPoint = "100 C";
				substance.DG_Class = "Clas";
				substance.DG_PG = "Gr1";
				substance.DG_PSN = "Name1";
				substance.DG_TechName = "T";
				substance.DG_MP = "Y";
			}

			var header = factory.New<OrgHeader>();
			header.OH_Code = "BLATESBLA";

			var contact = factory.New<OrgContact>();
			contact.OC_ContactName = "Contact1";
			contact.OC_Phone = "123456";
			contact.OC_OH = header.PK;

			packingLineDataObject.SetUNDGCollection(() => new List<UNDG>());
			var item1 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item1.Contact = new OrganizationContact();
			item1.Contact.FullName = "Contact1";
			item1.Contact.Phone = "123456";
			item1.FlashPoint = "0.1";
			item1.IMOClass = "Clas";
			item1.MarinePollutant = new UNDGMarinePollutant();
			item1.MarinePollutant.Code = "Y";
			item1.MarinePollutant.Description = "Marine Pollute";
			item1.PackedInLimitedQuantity = false;
			item1.PackingGroup = "Gr1";
			item1.ProperShippingName = "Name1";
			item1.TechicalName = "Tech1";
			item1.UNDGCode = "3000c";
			item1.Volume = 1m;
			item1.Weight = 2m;
			item1.WeightUQ = new UnitOfWeight();
			item1.WeightUQ.Code = "kg";
			item1.WeightUQ.Description = "kilo";
			item1.VolumeUQ = new UnitOfVolume();
			item1.VolumeUQ.Code = "m3";
			item1.VolumeUQ.Description = "cubic";
			packingLineDataObject.UNDGCollection.Add(item1);

			var item2 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item2.UNDGCode = "3000c";
			item2.PackedInLimitedQuantity = true;
			item2.Contact = new OrganizationContact();
			item2.Contact.FullName = "Telepuzik";
			packingLineDataObject.UNDGCollection.Add(item2);

			var item3 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item3.UNDGCode = "3001";
			packingLineDataObject.UNDGCollection.Add(item3);

			packingLineDataObject.LastKnownCFSStatusDate = new ZDateTime(2013, 7, 6);
			packingLineDataObject.LastKnownCFSStatus = new CodeDescriptionPair()
			{
				Code = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				Description = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Received
			};

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH1";

			var orgAddress = orgHeader.Addresses.AddNew(OrgAddressType.Office, false);
			orgAddress.OA_Address1 = "AD1";

			var lastKnownWarehouseAddress = new OrganizationAddress()
			{
				AddressType = AddressTypes.LastKnownCFSFacility,
				OrganizationCode = orgHeader.OH_Code,
				AddressShortCode = orgAddress.OA_Address1
			};

			packingLineDataObject.AddOrgAddress(lastKnownWarehouseAddress);

			return packingLineDataObject;
		}

		public static void AssertContents(PackLine packingLineBO)
		{
			AssertEquals("packingLineBO.JL_F3_NKPackType", "PAI", packingLineBO.JL_F3_NKPackType);
			AssertEquals("packingLineBO.JL_PackageCount", 12, packingLineBO.JL_PackageCount);
			AssertEquals("packingLineBO.JL_ActualVolumeUQ", Constants.Volume.CubicMetres, packingLineBO.JL_ActualVolumeUQ);
			AssertEquals("packingLineBO.JL_ActualWeight", 2.34m, packingLineBO.JL_ActualWeight);
			AssertEquals("packingLineBO.JL_ActualWeightUQ", Constants.Weight.Tonnes, packingLineBO.JL_ActualWeightUQ);
			AssertEquals("packingLineBO.JL_ContainerPackingOrder", 1, packingLineBO.JL_ContainerPackingOrder);
			AssertEquals("packingLineBO.JL_UnitOfDimension", Constants.Length.Inches, packingLineBO.JL_UnitOfDimension);
			AssertEquals("packingLineBO.JL_Length", 66m, packingLineBO.JL_Length);
			AssertEquals("packingLineBO.JL_Width", 77m, packingLineBO.JL_Width);
			AssertEquals("packingLineBO.JL_Height", 88m, packingLineBO.JL_Height);
			AssertEquals("packingLineBO.JL_RN_NKOrigin", Constants.CountryCodes.DemocraticRepublicOfCongo, packingLineBO.JL_RN_NKOrigin);
			AssertEquals("packingLineBO.JL_RH_NKCommodityCode", "FGFG", packingLineBO.JL_RH_NKCommodityCode);
			AssertEquals("packingLineBO.JL_HarmonisedCode", "2103.45.34.12A", packingLineBO.JL_HarmonisedCode);
			AssertEquals("packingLineBO.JL_ItemNo", new ZShort(1), packingLineBO.JL_ItemNo);
			AssertEquals("packingLineBO.JL_MarksAndNumbers", "MORE ART RIPPING KNOT", packingLineBO.JL_MarksAndNumbers);
			AssertEquals("packingLineBO.JL_RefNumber", "FORYOURREFERENCE", packingLineBO.JL_RefNumber);
			AssertEquals("packingLineBO.JL_ExportRefNumber", "ExportRefNumber", packingLineBO.JL_ExportRefNumber);
			AssertEquals("packingLineBO.JL_ImportRefNumber", "ImportRefNumber", packingLineBO.JL_ImportRefNumber);
			AssertEquals("packingLineBO.JL_Outturn", 50, packingLineBO.JL_Outturn);
			AssertEquals("packingLineBO.JL_Damaged", 2, packingLineBO.JL_Damaged);
			AssertEquals("packingLineBO.JL_Pillaged", 48, packingLineBO.JL_Pillaged);
			AssertEquals("packingLineBO.JL_OutturnComment", "AHARRRRR!!!", packingLineBO.JL_OutturnComment);
			AssertEquals("packingLineBO.JL_OutturnedHeight", 23.34m, packingLineBO.JL_OutturnedHeight);
			AssertEquals("packingLineBO.JL_OutturnedLength", 34.45m, packingLineBO.JL_OutturnedLength);
			AssertEquals("packingLineBO.JL_OutturnedWeight", 6.78m, packingLineBO.JL_OutturnedWeight);
			AssertEquals("packingLineBO.JL_OutturnedWidth", 56.98m, packingLineBO.JL_OutturnedWidth);
			AssertEquals("packingLineBO.JL_Description", "Detailed Goods Description 01;", packingLineBO.JL_Description);
			AssertEquals("packingLineBO.JL_LoadingMeters", 4.32m, packingLineBO.JL_LoadingMeters);
			AssertEquals("packingLineBO.JL_EndItemNo", new ZShort(1), packingLineBO.JL_EndItemNo);
			AssertEquals("packingLineBO.JL_LinePrice", 5.43m, packingLineBO.JL_LinePrice);
			AssertEquals("packingLineBO.JL_DetailedDescription", "This is a detailed description to go in the detailed description field.", packingLineBO.JL_DetailedDescription);
			AssertEquals("packingLineBO.JL_IsHighRisk", true, packingLineBO.JL_IsHighRisk);

			AssertEquals("packingLineBO.JL_RequiresTemperatureControl", true, packingLineBO.JL_RequiresTemperatureControl);
			AssertEquals("packingLineBO.JL_RequiredTemperatureMinimum", (ZDecimal)1.00, packingLineBO.JL_RequiredTemperatureMinimum);
			AssertEquals("packingLineBO.JL_RequiredTemperatureMaximum", (ZDecimal)100.0, packingLineBO.JL_RequiredTemperatureMaximum);
			AssertEquals("packingLineBO.JL_RequiredTemperatureUnit", Core.Constants.Temperature.Centigrade, packingLineBO.JL_RequiredTemperatureUnit);

			AssertEquals("packingLineBO.JL_VehicleColor", "White", packingLineBO.JL_VehicleColor);
			AssertEquals("packingLineBO.JL_VehicleMake", "Make 1", packingLineBO.JL_VehicleMake);
			AssertEquals("packingLineBO.JL_VehicleModel", "Model 1", packingLineBO.JL_VehicleModel);
			AssertEquals("packingLineBO.JL_VehicleNumberOfDoors", (ZByte)4, packingLineBO.JL_VehicleNumberOfDoors);
			AssertEquals("packingLineBO.JL_VehicleTransmission", "MAN", packingLineBO.JL_VehicleTransmission);
			AssertEquals("packingLineBO.JL_VehicleYear", (ZShort)2015, packingLineBO.JL_VehicleYear);

			AssertEquals("UNDGs.Count", 3, packingLineBO.UNDGs.Count);

			AssertEquals("DGContact.OC_ContactName", "Contact1", packingLineBO.UNDGs[0].DGContact.OC_ContactName);
			AssertEquals("DGContact.OC_Phone", "123456", packingLineBO.UNDGs[0].DGContact.OC_Phone);
			AssertEquals("Subs.DG_Code", "3000c", packingLineBO.UNDGs[0].Substance.DG_Code);
			AssertEquals("DI_DGFlashPoint", 0.1m, packingLineBO.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("DI_DGVolume", 1m, packingLineBO.UNDGs[0].DI_DGVolume);
			AssertEquals("DI_DGWeight", 2m, packingLineBO.UNDGs[0].DI_DGWeight);
			AssertEquals("DI_MPMarinePollutant", "Y", packingLineBO.UNDGs[0].DI_MPMarinePollutant);
			AssertEquals("DI_UnitOfWeight", "KG", packingLineBO.UNDGs[0].DI_UnitOfWeight);
			AssertEquals("DI_UnitOfVolume", "M3", packingLineBO.UNDGs[0].DI_UnitOfVolume);
			AssertEquals("DI_TechnicalName", "Tech1", packingLineBO.UNDGs[0].DI_TechnicalName);
			AssertEquals("DI_IsLimitedQuantity", false, packingLineBO.UNDGs[0].DI_IsLimitedQuantity);

			AssertEquals("Subs.DG_Code", "3000c", packingLineBO.UNDGs[1].Substance.DG_Code);
			AssertEquals("DGContact.OC_ContactName", null, packingLineBO.UNDGs[1].DGContact);
			AssertEquals("DI_DGFlashPoint", 0m, packingLineBO.UNDGs[1].DI_DGFlashPoint);
			AssertEquals("DI_DGVolume", 0m, packingLineBO.UNDGs[1].DI_DGVolume);
			AssertEquals("DI_DGWeight", 0m, packingLineBO.UNDGs[1].DI_DGWeight);
			AssertEquals("DI_MPMarinePollutant", "", packingLineBO.UNDGs[1].DI_MPMarinePollutant);
			AssertEquals("DI_UnitOfWeight", "", packingLineBO.UNDGs[1].DI_UnitOfWeight);
			AssertEquals("DI_UnitOfVolume", "", packingLineBO.UNDGs[1].DI_UnitOfVolume);
			AssertEquals("DI_TechnicalName", "", packingLineBO.UNDGs[1].DI_TechnicalName);
			AssertEquals("DI_IsLimitedQuantity", true, packingLineBO.UNDGs[1].DI_IsLimitedQuantity);

			AssertEquals("packingLineBO.JL_LastKnownTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, packingLineBO.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("packingLineBO.JL_LastKnownTransitWarehouseStatusDateTime", new ZDateTime(2013, 7, 6), packingLineBO.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertEquals("packingLineBO.LastKnownTransitWarehouseAddress", "AD1", packingLineBO.LastKnownTransitWarehouseAddress?.OA_Address1);
		}

		#endregion

		#region Aviation Security Additional Inspection Type

		public void TestReadIntoBusinessObject_AviationSecurityAdditionalInspectionType()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = Factory.New<CommonShipment>();

			var packingLineDataObject = SetupPackingLine(Factory);
			PrepareShipmentForSetupPackingLine(shipment);
			Factory.SaveForTesting();
			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);
			var packingLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull(packingLineBO);
			packingLineDataObject.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair { Code = "PHS" };
			packingLineDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "XRY" };
			packingLineBO = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject();

			AssertEquals("AdditionalInspectionType", "PHS", packingLineBO.JL_AdditionalInspectionTypeCode);
			AssertEquals("InspectionType", "XRY", packingLineBO.JL_InspectionTypeCode);

			packingLineDataObject.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair { Code = "UNK" };
			packingLineDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "UNK" };
			packingLineBO = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment).ReadIntoBusinessObject();
			AssertEquals("AdditionalInspectionType", "UNK", packingLineBO.JL_AdditionalInspectionTypeCode);
			AssertEquals("InspectionType", "UNK", packingLineBO.JL_InspectionTypeCode);
		}

		#endregion

		#region IsHighRisk

		public void TestReadIntoBusinessObject_IsHighRisk()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = Factory.New<CommonShipment>();

			var packingLineDataObject = SetupPackingLine(Factory);
			PrepareShipmentForSetupPackingLine(shipment);
			packingLineDataObject.IsHighRisk = true;
			Factory.SaveForTesting();

			packingLineDataObject.IsHighRisk = true;
			var reader = new PackingLineDataObjectReader<PackLine, CommonShipment>(packingLineDataObject, logger, Factory, shipment);
			var packingLineBO = reader.ReadIntoBusinessObject();

			AssertEquals(true, packingLineBO.JL_IsHighRisk);
		}

		#endregion
	}
}

using System;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class TopLevelPackPackingLineDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBasicContainerLevelFieldMappings()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var dataObject = SetupPackingLine(Factory);
			Factory.SaveForTesting();
			var reader = new TopLevelPackPackingLineDataObjectReader<AgencyShipmentContainer>(dataObject, logger, Factory, (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(containerBO);
			#region Check Contents of Business Object
			CombineAssertions(delegate
			{
				AssertContents(containerBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching AgencyShipmentContainer found, creating new AgencyShipmentContainer.
Information - Populating AgencyShipmentContainer...
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

		public void TestReadIntoBusinessObject_CalculatedVolumeIsToBig_ReplaceWithInvalidValue()
		{
			var packlineDataObject = SetupPackingLine(Factory);
			packlineDataObject.Length = 1000;
			packlineDataObject.Width = 1000;
			packlineDataObject.Height = 1000;
			packlineDataObject.LengthUnit = new UnitOfLength()
			{ Code = "M" };
			var readerToTest = new TopLevelPackPackingLineDataObjectReader<AgencyShipmentContainer>(packlineDataObject, new TestErrorLogger(), Factory, c => null);
			var readContainer = readerToTest.ReadIntoBusinessObject();
			AssertNotNull(readContainer);
			AssertEquals("The value is fixed", 999999m, readContainer.JC_GrossVolume);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;
		public static PackingLine SetupPackingLine(UniversalObjectFactory factory)
		{
			var packingLineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLineDataObject.Commodity = new Commodity()
			{ Code = "FGFG", Description = "Fudge Guts Fingers Gone" };
			packingLineDataObject.HarmonisedCode = "2103.45.34.12A";
			packingLineDataObject.Height = 88m;
			packingLineDataObject.Length = 66m;
			packingLineDataObject.MarksAndNos = "MORE ART RIPPING KNOT";
			packingLineDataObject.PackQty = 12;
			packingLineDataObject.ReferenceNumber = "COOLCAR111";
			packingLineDataObject.LengthUnit = new UnitOfLength()
			{ Code = "IN", Description = "Inches" };
			packingLineDataObject.Volume = 87.943m;
			packingLineDataObject.VolumeUnit = new UnitOfVolume()
			{ Code = "M3", Description = "Cubic Meters" };
			packingLineDataObject.Weight = 2.34m;
			packingLineDataObject.WeightUnit = new UnitOfWeight()
			{ Code = "T", Description = "Tonnes" };
			packingLineDataObject.Width = 77m;
			packingLineDataObject.GoodsDescription = "Detailed Goods Description 01;";
			packingLineDataObject.LinePrice = 111.22m;
			packingLineDataObject.LinePriceCurrency = new Currency()
			{ Code = "UAH", Description = "Ukrainian Hryvnia" };
			var vehicle = new Vehicle();
			vehicle.Color = "RED";
			vehicle.Make = "IGOR";
			vehicle.Model = "GORICAR";
			vehicle.NumberOfDoors = 1;
			vehicle.Transmission = new CodeDescriptionPair()
			{ Code = "AUT", Description = "Automatic" };
			vehicle.Year = 2012;
			packingLineDataObject.Vehicle = vehicle;
			var substance = factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var header = factory.New<OrgHeader>();
			header.OH_Code = "BLATESBLA";
			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Phone = "123456";
			packingLineDataObject.SetUNDGCollection(() => new System.Collections.Generic.List<UNDG>());
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
			item1.Standard = "IMO";
			packingLineDataObject.UNDGCollection.Add(item1);
			var item2 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item2.UNDGCode = "3000c";
			item2.PackedInLimitedQuantity = true;
			item2.Contact = new OrganizationContact();
			item2.Contact.FullName = "Telepuzik";
			item2.Standard = "IMO";
			packingLineDataObject.UNDGCollection.Add(item2);
			var item3 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item3.UNDGCode = "3001";
			item3.Standard = "IMO";
			packingLineDataObject.UNDGCollection.Add(item3);
			return packingLineDataObject;
		}

		public static void AssertContents(AgencyShipmentContainer containerBO)
		{
			AssertEquals("containerBO.JC_RH_NKContainerCommodityCode", "FGFG", containerBO.JC_RH_NKContainerCommodityCode);
			AssertEquals("containerBO.JC_ContainerNum", "COOLCAR111", containerBO.JC_ContainerNum);
			AssertEquals("containerBO.JC_GrossWeight", 2.34m, containerBO.JC_GrossWeight);
			AssertEquals("containerBO.JC_GrossWeightUQ", Constants.Weight.Tonnes, containerBO.JC_GrossWeightUQ);
			AssertEquals("containerBO.JC_TotalHeight", 88m, containerBO.JC_TotalHeight);
			AssertEquals("containerBO.JC_TotalLength", 66m, containerBO.JC_TotalLength);
			AssertEquals("containerBO.JC_TotalUnitOfMeasure", Constants.Length.Inches, containerBO.JC_TotalUnitOfMeasure);
			AssertEquals("containerBO.JC_TotalWidth", 77m, containerBO.JC_TotalWidth);
			AssertEquals("containerBO.JC_HarmonisedCode", "2103.45.34.12A", containerBO.JC_HarmonisedCode);
			AssertEquals("containerBO.JC_MarksAndNumbers", "MORE ART RIPPING KNOT", containerBO.JC_MarksAndNumbers);
			AssertEquals("containerBO.JC_ContainerCount", (short)12, containerBO.JC_ContainerCount);
			AssertEquals("containerBO.JC_GrossVolume", 87.943m, containerBO.JC_GrossVolume);
			AssertEquals("containerBO.JC_GrossVolumeUQ", Constants.Volume.CubicMetres, containerBO.JC_GrossVolumeUQ);
			AssertEquals("containerBO.JC_Description", "Detailed Goods Description 01;", containerBO.JC_Description);
			AssertEquals("containerBO.JC_GoodsValue", 111.22m, containerBO.JC_GoodsValue);
			AssertEquals("containerBO.JC_RX_NKGoodsCurrency", "UAH", containerBO.JC_RX_NKGoodsCurrency);
			AssertEquals("containerBO.JC_VehicleColor", "RED", containerBO.JC_VehicleColor);
			AssertEquals("containerBO.JC_VehicleMake", "IGOR", containerBO.JC_VehicleMake);
			AssertEquals("containerBO.JC_VehicleModel", "GORICAR", containerBO.JC_VehicleModel);
			AssertEquals("containerBO.JC_VehicleNumberOfDoors", (byte)1, containerBO.JC_VehicleNumberOfDoors);
			AssertEquals("containerBO.JC_VehicleTransmission.Code", "AUT", containerBO.JC_VehicleTransmission);
			AssertEquals("containerBO.JC_VehicleYear", (short)2012, containerBO.JC_VehicleYear);
			AssertEquals("UNDGs.Count", 3, containerBO.UNDGs.Count);
			AssertEquals("DGContact.OC_ContactName", "Contact1", containerBO.UNDGs[0].DGContact.OC_ContactName);
			AssertEquals("DGContact.OC_Phone", "123456", containerBO.UNDGs[0].DGContact.OC_Phone);
			AssertEquals("Subs.DG_Code", "3000c", containerBO.UNDGs[0].Substance?.DG_Code);
			AssertEquals("DI_DGFlashPoint", 0.1m, containerBO.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("DI_DGVolume", 1m, containerBO.UNDGs[0].DI_DGVolume);
			AssertEquals("DI_DGWeight", 2m, containerBO.UNDGs[0].DI_DGWeight);
			AssertEquals("DI_MPMarinePollutant", "Y", containerBO.UNDGs[0].DI_MPMarinePollutant);
			AssertEquals("DI_UnitOfWeight", "KG", containerBO.UNDGs[0].DI_UnitOfWeight);
			AssertEquals("DI_UnitOfVolume", "M3", containerBO.UNDGs[0].DI_UnitOfVolume);
			AssertEquals("DI_TechnicalName", "Tech1", containerBO.UNDGs[0].DI_TechnicalName);
			AssertEquals("DI_IsLimitedQuantity", false, containerBO.UNDGs[0].DI_IsLimitedQuantity);
			AssertEquals("Subs.DG_Code", "3000c", containerBO.UNDGs[1].Substance?.DG_Code);
			AssertEquals("DGContact.OC_ContactName", null, containerBO.UNDGs[1].DGContact);
			AssertEquals("DI_DGFlashPoint", 0m, containerBO.UNDGs[1].DI_DGFlashPoint);
			AssertEquals("DI_DGVolume", 0m, containerBO.UNDGs[1].DI_DGVolume);
			AssertEquals("DI_DGWeight", 0m, containerBO.UNDGs[1].DI_DGWeight);
			AssertEquals("DI_MPMarinePollutant", "", containerBO.UNDGs[1].DI_MPMarinePollutant);
			AssertEquals("DI_UnitOfWeight", "", containerBO.UNDGs[1].DI_UnitOfWeight);
			AssertEquals("DI_UnitOfVolume", "", containerBO.UNDGs[1].DI_UnitOfVolume);
			AssertEquals("DI_TechnicalName", "", containerBO.UNDGs[1].DI_TechnicalName);
			AssertEquals("DI_IsLimitedQuantity", true, containerBO.UNDGs[1].DI_IsLimitedQuantity);
		}
		#endregion
	}
}

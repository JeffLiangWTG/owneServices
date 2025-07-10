using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class AgencyShipmentContainerDataObjectReaderTest_BillOfLadingContainer : UniversalShipmentDataObjectReaderTest
	{
		protected override string CreateFieldMap(BusinessObject businessObject)
		{
			var container = (BillOfLadingContainer)businessObject;
			var mapper = new BillOfLadingFieldMapper(container.Booking);
			return mapper.Map();
		}

		protected override UniversalShipment GetDataObject()
		{
			var factory = new BusinessObjectFactory();
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(factory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = factory.New<BillOfLading>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			factory.Save();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea" };
			dataObject.PortOfOrigin = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDestination = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.ShipmentIncoTerm = new IncoTerm { Code = "FOB", Description = "Free On Board" };
			dataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };
			dataObject.GoodsDescription = "frozen ducks";
			dataObject.ReleaseType = new CodeDescriptionPair { Code = "EBL", Description = "Release Bill of Lading" };
			dataObject.HBLAWBChargesDisplay = new CodeDescriptionPair { Code = "SHW", Description = "Show Collect Charges" };
			dataObject.ShippedOnBoard = new CodeDescriptionPair { Code = "LDN", Description = "Laden" };
			dataObject.NoCopyBills = 2;
			dataObject.NoOriginalBills = 3;
			dataObject.TotalVolume = 11;
			dataObject.TotalVolumeUnit = new UnitOfVolume { Code = "M3", Description = "Cubic Meters" };
			dataObject.TotalWeight = 22;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG", Description = "Kilograms" };
			dataObject.ActualChargeable = 33;
			dataObject.InterimReceiptNumber = "IR001";
			dataObject.ShipmentType = new CodeDescriptionPair { Code = "STD", Description = "Standard House" };
			dataObject.ShipperCODAmount = 44;
			dataObject.ShipperCODPayMethod = new CodeDescriptionPair { Code = "COC", Description = "Company Check" };
			dataObject.BookingConfirmationReference = "booking ref";
			dataObject.AgentsReference = "agent ref";
			dataObject.TotalNoOfPacks = 55;
			dataObject.TotalNoOfPacksPackageType = new PackageType { Code = "KEG", Description = "Keg" };
			dataObject.ManifestedVolume = 66;
			dataObject.ManifestedWeight = 77;
			dataObject.ManifestedChargeable = 88;
			dataObject.DocumentedVolume = 99;
			dataObject.DocumentedWeight = 111;
			dataObject.DocumentedChargeable = 222;
			dataObject.TranshipToOtherCFS = true;
			dataObject.ServiceLevel = new ServiceLevel { Code = "STD", Description = "Standard" };
			dataObject.FreightRate = 67.89m;
			dataObject.FreightRateCurrency = new Currency { Code = "NGN", Description = "Nigerian Naira" };
			dataObject.GoodsValue = 123.45m;
			dataObject.GoodsValueCurrency = new Currency { Code = "ZMK", Description = "Zambia Kwacha" };
			dataObject.InsuranceValue = 345.67m;
			dataObject.InsuranceValueCurrency = new Currency { Code = "CFA", Description = " Central African Franc" };
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
			dataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.BookingConfirmed, Value = new ZDateTime(2012, 2, 1) }, new Date { Type = DateType.Received, Value = new ZDateTime(2012, 2, 2) }, new Date { Type = DateType.Departure, Value = new ZDateTime(2012, 2, 3) }, new Date { Type = DateType.Arrival, Value = new ZDateTime(2012, 2, 4) }, new Date { Type = DateType.BillIssued, Value = new ZDateTime(2012, 2, 5) }, new Date { Type = DateType.ShippedOnBoard, Value = new ZDateTime(2012, 2, 6) } });
			dataObject.SetNoteCollection(() => new DataObjectList<Note> { new Note { Description = "CAT EATER", Visibility = new CodeDescriptionPair { Code = "PUB", Description = "Public" }, NoteContext = new NoteContext { Code = "BEB", Description = "Baby Eats Banana" } } });
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { new AdditionalReference { Type = new EntryType { Code = "CON", Description = "Contract Number" }, ReferenceNumber = "Additional reference number" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cheese" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, LegType = LegType.PreCarriage, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" }, new TransportLeg { PortOfLoading = new UNLOCO { Code = "NZAKL" }, PortOfDischarge = new UNLOCO { Code = "USMIA" }, TransportMode = TransportMode.Sea, LegType = LegType.Other } });
			return dataObject;
		}

		protected override string GetExpectedShipmentMap()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyShipmentContainer.TestFiles.BillOfLadingContainer_FieldMap.txt");
			}
		}

		protected override ITopLevelDataObjectReader GetReader(UniversalShipment dataObject)
		{
			return new AgencyShipmentContainerDataObjectReader(dataObject, new TestErrorLogger(), Factory);
		}
	}
}

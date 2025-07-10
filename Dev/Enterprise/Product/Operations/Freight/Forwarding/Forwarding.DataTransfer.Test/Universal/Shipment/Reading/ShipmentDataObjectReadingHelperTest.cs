using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public abstract class ShipmentDataObjectReadingHelperTest : OrganizationAddressTestHelper
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			shipmentDataObject = SetupShipment();
			logger = new TestErrorLogger();
		}

		internal static UniversalShipment SetupShipment()
		{
			var result = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataSource = result.DataContext = DataContextFactory.New();
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			result.ActualChargeable = 123.45m;
			result.AWBServiceLevel = new CodeDescriptionPair() { Code = "ELG", Description = "Elegant" };
			result.BookingConfirmationReference = "BOOK ME";
			result.CartageWaybillNumber = "CARTAGE BILL";
			result.CFSReference = "CFS Book Ref";
			result.ContainerCount = 0;
			result.ContainerMode = new ContainerMode() { Code = "LCL", Description = "Less Container Load" };
			result.DocumentedChargeable = 2.34m;
			result.DocumentedVolume = 3.45m;
			result.DocumentedWeight = 4.56m;

			result.FreightRate = 67.89m;
			result.FreightRateCurrency = new Currency() { Code = "CZK" };
			result.GoodsDescription = "RAT HATS";
			result.GoodsValue = 5.67m;
			result.GoodsValueCurrency = new Currency() { Code = "GHS" };
			result.HBLAWBChargesDisplay = new CodeDescriptionPair() { Code = "FOO" };
			result.HBLContainerPackModeOverride = "FAR";

			result.InsuranceValue = 6.78m;
			result.InsuranceValueCurrency = new Currency() { Code = "KES" };
			result.InterimReceiptNumber = "IR Text";

			result.IsBooking = false;
			result.IsCFSRegistered = false;
			result.IsDirectBooking = false;
			result.IsForwardRegistered = true;
			result.IsNeutralMaster = new IsNeutralMaster() { Value = false };
			result.IsShipping = false;
			result.IsSplitShipment = false;

			result.ManifestedChargeable = 7.89m;
			result.ManifestedVolume = 8.90m;
			result.ManifestedWeight = 9.01m;
			result.NoCopyBills = new ZByte(3);
			result.NoOriginalBills = new ZByte(4);
			result.OuterPacks = 44;
			result.OuterPacksPackageType = new PackageType() { Code = "VF" };
			result.PackingOrder = 1;

			result.ReleaseType = new CodeDescriptionPair() { Code = "CAD", Description = "Cash Against Documents" };
			result.ServiceLevel = new ServiceLevel() { Code = "PFT", Description = "Perfect" };
			result.ShipmentIncoTerm = new UniversalIncoTerm() { Code = "CIF", Description = "Cost, Insurance And Freight" };
			result.ShipmentType = new CodeDescriptionPair() { Code = "STD", Description = "Standard House" };
			result.ShipmentStatus = new CodeDescriptionPair() { Code = "BLA", Description = "BLACK" };
			result.ShippedOnBoard = new CodeDescriptionPair() { Code = "LDN", Description = "Laden" };
			result.ShipperCODAmount = 12.34m;
			result.ShipperCODPayMethod = new CodeDescriptionPair() { Code = "COC", Description = "Company Check" };

			result.TotalNoOfPacks = 45;
			result.TotalNoOfPacksPackageType = new PackageType() { Code = "KEG", Description = "Keg" };
			result.TotalVolume = 23.45m;
			result.TotalVolumeUnit = new UnitOfVolume() { Code = "CF", Description = "Cubic Feet" };
			result.TotalWeight = 34.56m;
			result.TotalWeightUnit = new UnitOfWeight() { Code = "KT", Description = "Kilotons" };
			result.TranshipToOtherCFS = true;

			result.TransportMode = new CodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" };
			result.PortOfOrigin = new UNLOCO() { Code = "NZDUD", Name = "Dunedin" };
			result.PortOfLoading = new UNLOCO() { Code = "NZCHC", Name = "Christchurch" };
			result.PortOfFirstArrival = new UNLOCO() { Code = "AUNTL", Name = "Newcastle" };
			result.PortOfDischarge = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			result.PortOfDestination = new UNLOCO() { Code = "AUBDG", Name = "Bendigo" };

			result.VesselName = "BUNGA DELIMA";
			result.VoyageFlightNo = "343L";
			result.WarehouseLocation = "HOME";

			return result;
		}

		internal UniversalShipment SetupShipment(string resourcePath)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var resourceStream = resourceRetriever.GetStream(resourcePath))
			using (var subStreamableStream = new SubStreamableStream(resourceStream, DisposableLeakListener.Instance))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, subStreamableStream, logger);
			}

			return shipmentDataObject;
		}

		protected TestErrorLogger logger;

		protected UniversalShipment shipmentDataObject;

		protected static PackingLine SetupPackingLine(UniversalObjectFactory factory)
		{
			return PackingLineDataObjectReaderTest.SetupPackingLine(factory);
		}

		protected static void SetupCFSAddresses(UniversalShipment dataObject)
		{
			PackingLineDataObjectReaderTest.SetupCFSAddresses(dataObject);
		}

		protected static void AssertContents(ForwardingPackLine packingLineBO)
		{
			PackingLineDataObjectReaderTest.AssertContents(packingLineBO);
		}

		protected static EntryNumber SetupEntryNumber()
		{
			return EntryNumberDataObjectReaderTest.SetupEntryNumberDataObject();
		}

		protected static void AssertContents(CusEntryNumber entryNumberBO, BusinessObject entryNumberParent)
		{
			EntryNumberDataObjectReaderTest.AssertContents(entryNumberBO, entryNumberParent);
		}

		protected static Note SetupNote()
		{
			var noteDataObject = new Note();

			noteDataObject.Description = "DOG FLOGGER!!";
			noteDataObject.Visibility = new CodeDescriptionPair() { Code = nameof(CargoWise.Definitions.StmNoteVisibility.PUB), Description = "Public" };
			noteDataObject.NoteContext = new NoteContext() { Code = "BEB", Description = "Baby Eats Banana" };

			return noteDataObject;
		}

		protected internal static void AssertNoteContents(StmNote noteBO)
		{
			AssertEquals("noteBO.ST_Description", "DOG FLOGGER!!", noteBO.ST_Description);
			AssertEquals("noteBO.ST_NoteContext", "BEB", noteBO.ST_NoteContext);
			AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
		}

		protected UniversalShipment GetNewOrderDataObject(string orderNum = "")
		{
			Data.CreateConsigneeAddressCRAHOLSYDInDB();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.DataContext = DataContextFactory.New();
			orderDataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(Data.ConsigneeAddressCRAHOLSYDDataObject);

			if (!string.IsNullOrEmpty(orderNum))
			{
				orderDataObject.Order = new Order() { OrderNumber = orderNum, OrderNumberSplit = new ZByte(2) };
			}

			return orderDataObject;
		}

		protected static UniversalShipment CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType contextType, ZString sourceNumber, DateType dateType, ZBool isEstimate, ZDateTime value)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetDateCollection(() => new List<Date>());
			shipmentDataObject.DateCollection.Add(Date.New(dateType, isEstimate, value));
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(contextType, sourceNumber);
			return shipmentDataObject;
		}

		protected UniversalTestData Data
		{
			get { return data ?? (data = new UniversalTestData(Factory)); }
		}
		UniversalTestData data;

		#endregion
	}
}

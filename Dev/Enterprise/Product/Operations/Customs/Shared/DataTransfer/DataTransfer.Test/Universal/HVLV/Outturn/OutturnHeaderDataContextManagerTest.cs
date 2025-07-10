using System.Reflection;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	[TestedType(typeof(OutturnHeaderDataContextManager))]
	sealed class OutturnHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<OutturnHeaderDataContextManager, CusOutturnHeader>
	{
		public void TestGetSpecificCountryShipmentDataObjectReader()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var manager = new OutturnHeaderDataContextManager();

			var getShipmentDataObjectReader = typeof(OutturnHeaderDataContextManager).GetMethod("GetShipmentDataObjectReader", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			var reader = getShipmentDataObjectReader.Invoke(manager, new object[] { new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new DummyLogger(), Factory });
			AssertEquals("Specific Country CusOutturnHeaderDataObjectReader Type", "Enterprise.Customs.AU.Declaration.Business.CusOutturnHeaderDataObjectReader", reader.GetType().ToString());
		}

		public void TestETailReaderIsCalled()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C00029310");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S900053111");

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var logger = new DummyLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>()
				{
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ShipmentType = new CodeDescriptionPair()
						{
							Code = Core.Constants.ShipmentTypes.HighVolumeLowValue
						}
					}
				});

			var reader = OutturnHeaderDataContextManager.GetDataObjectReader(shipment, logger, Factory);
			AssertEquals("Specific Country CusOutturnHeaderDataObjectReader Type", "Enterprise.Customs.AU.Declaration.Business.ETailCusOutturnHeaderDataObjectReader", reader.GetType().ToString());
		}

		public void TestTransitWarehouseReaderIsCalled()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransitReceiveHeader, "Receival Warehouse Key");

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var logger = new DummyLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>()
				{
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ShipmentType = new CodeDescriptionPair()
						{
							Code = nameof(DataContextType.TransitReceive)
						}
					}
				});

			var reader = OutturnHeaderDataContextManager.GetDataObjectReader(shipment, logger, Factory);
			AssertEquals("Specific Country CusOutturnHeaderDataObjectReader Type", "Enterprise.Customs.DataTransfer.Universal.Outturn.CusOutturnHeaderDataObjectReaderForTransitWarehouse", reader.GetType().ToString());
		}

		public void TestOnlyProcessSeaShipment()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			IShipmentDataContextManager manager = new OutturnHeaderDataContextManager();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.SeaCargoOutturn, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB1234",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Air }
			};
			var logger = new TestErrorLogger();
			AssertEquals("Should not be used as it's not Sea", false, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
			shipment.TransportMode.Code = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be used as it's Sea", true, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
		}

		public void TestGetSpecificCountryShipmentDataObjectWriter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var outturnHeader = Factory.New<CusOutturnHeader>();

			IShipmentDataContextManager manager = new OutturnHeaderDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, outturnHeader)));
			AssertEquals("Specific Country CusOutturnHeaderDataObjectWriter Type", "Enterprise.Customs.AU.Declaration.Business.CusOutturnHeaderDataObjectWriter", writer.GetType().ToString());
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		protected override CusOutturnHeader GetNewBusinessObjectForTesting()
		{
			// For now only AU use CusOutturnHeader
			var result = (CusOutturnHeader)Factory.BOFactory.New<Integration.Customs.AU.ICusOutturnHeader>();
			result.FillWithValidTestData();
			return result;
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.COA };

		protected override bool ManagerChecksDataTargetToImport => false;

		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>OutturnHeader</Type>
		</DataSource>
	  </DataSourceCollection>
	</DataContext>
  </Shipment>
</UniversalShipment>";
	}
}

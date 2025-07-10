using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobAddressAdditionalInfoDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestInvalidAddressType()
		{
			var dataObject = SetupDataObject("InvalidAddressType", "RAI");
			var shipment = Factory.New<ForwardingShipment>();
			var logger = new TestErrorLogger();

			var reader = new DummyJobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, shipment);
			var reason = reader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(null);
			AssertEquals($"Invalid AddressType '{dataObject.AddressType}' provided in AddressAdditionalInfo.", reason);
		}

		public void TestEmptyAddressType()
		{
			var dataObject = SetupDataObject(string.Empty, "RAI");
			var shipment = Factory.New<ForwardingShipment>();
			var logger = new TestErrorLogger();

			var reader = new DummyJobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, shipment);
			var reason = reader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(null);
			AssertEquals("Empty or Null AddressType provided in AddressAdditionalInfo.", reason);
		}

		public void TestPopulateNewBusinessObject()
		{
			var dataObject = SetupDataObject(nameof(DocAddressType.ConsignorPickupDeliveryAddress), "ROA");
			var shipment = Factory.New<ForwardingShipment>();
			var reader = new JobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, shipment);
			reader.ReadIntoBusinessObject();

			var expectedAddressTypeCode = DocAddressTypes.GetCode(Factory.BOFactory, DocAddressType.ConsignorPickupDeliveryAddress);

			AssertEquals(1, shipment.JobAddressAdditionalInfoCollection.Count);
			AssertResult(shipment.JobAddressAdditionalInfoCollection.First(), expectedAddressTypeCode, "ROA");
		}

		public void TestPopulateExistedBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var addressTypeCode = DocAddressTypes.GetCode(Factory.BOFactory, DocAddressType.ConsigneePickupDeliveryAddress);
			var existingInfo = shipment.JobAddressAdditionalInfoCollection.GetOrCreate(addressTypeCode);
			existingInfo.TransportMode = "RAI";

			var dataObject = SetupDataObject(nameof(DocAddressType.ConsigneePickupDeliveryAddress), "IWT");

			var reader = new JobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, shipment);
			reader.ReadIntoBusinessObject();

			AssertEquals(1, shipment.JobAddressAdditionalInfoCollection.Count);
			AssertResult(shipment.JobAddressAdditionalInfoCollection.First(), addressTypeCode, "IWT");
		}

		public void TestPopulateAllAddressTypesWithTransportModes()
		{
			var shipment = Factory.New<ForwardingShipment>();

			foreach (var addressType in shipmentAddressTypes)
			{
				foreach (var transportMode in transportModes)
				{
					var dataObject = SetupDataObject(addressType.ToString(), transportMode);
					var reader = new JobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, shipment);
					reader.ReadIntoBusinessObject();

					var addressTypeCode = DocAddressTypes.GetCode(Factory.BOFactory, addressType);
					AssertResult(shipment.JobAddressAdditionalInfoCollection.Get(addressTypeCode), addressTypeCode, transportMode);
				}
			}

			AssertEquals(shipmentAddressTypes.Length, shipment.JobAddressAdditionalInfoCollection.Count);

			var container = Factory.New<ForwardingContainer>();

			foreach (var addressType in containerAddressTypes)
			{
				foreach (var transportMode in transportModes)
				{
					var dataObject = SetupDataObject(addressType.ToString(), transportMode);
					var reader = new JobAddressAdditionalInfoDataObjectReader(dataObject, logger, Factory, container);
					reader.ReadIntoBusinessObject();

					var addressTypeCode = DocAddressTypes.GetCode(Factory.BOFactory, addressType);
					AssertResult(container.JobAddressAdditionalInfoCollection.Get(addressTypeCode), addressTypeCode, transportMode);
				}
			}

			AssertEquals(containerAddressTypes.Length, container.JobAddressAdditionalInfoCollection.Count);
		}

		#region Helper Methods

		AdditionalAddressInfo SetupDataObject(ZString addressType, ZString transportMode)
		{
			return new AdditionalAddressInfo
			{
				AddressType = addressType,
				TransportMode = new CodeDescriptionPair { Code = transportMode, Description = ZString.Empty }
			};
		}

		void AssertResult(IJobAddressAdditionalInfo info, ZString expectedAddressType, ZString expectedTransportMode)
		{
			AssertNotNull($"Expected JobAddressAdditionalInfo for {expectedAddressType}", info);
			AssertEquals(expectedAddressType, info.AddressType);
			AssertEquals(expectedTransportMode, info.TransportMode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion

		#region Constants

		readonly DocAddressType[] shipmentAddressTypes = new[]
		{
			DocAddressType.ConsignorPickupDeliveryAddress,
			DocAddressType.DepartureCFSAddress,
			DocAddressType.ArrivalCFSAddress,
			DocAddressType.ConsigneePickupDeliveryAddress
		};

		readonly DocAddressType[] containerAddressTypes = new[]
		{
			DocAddressType.DepartureCYDAddress,
			DocAddressType.ArrivalCYDAddress
		};

		readonly string[] transportModes = new[]
		{
			"ROA",
			"RAI",
			"IWT"
		};

		#endregion

		class DummyJobAddressAdditionalInfoDataObjectReader : JobAddressAdditionalInfoDataObjectReader
		{
			public DummyJobAddressAdditionalInfoDataObjectReader(AdditionalAddressInfo dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IJobAddressAdditionalInfoSupport parent)
				: base(dataObject, logger, factory, parent)
			{
			}

			public new ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobAddressAdditionalInfo targetBO)
				=> base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}
	}
}

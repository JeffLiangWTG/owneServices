using System;
using System.Linq;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CarrierShipmentDataContextManager))]
	sealed class CarrierShipmentDataContextManagerTest : ShipmentDataContextManagerTestCase<CarrierShipmentDataContextManager, CarrierShipmentHeader>
	{
		#region Context
		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new CarrierShipmentDataContextManager().DataContextType);
		}

		DataContextType ExpectedDataContextType => DataContextType.CarrierShipment;

		public void TestDataContextKey()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";
			AssertEquals("CSH001", shipment.GetUniversalDataContextManager().DataContextKey);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(new CarrierShipmentDataContextManager().DefaultOutputDirectory);
		}

		public void TestManagesShipments()
		{
			AssertEquals(true, new CarrierShipmentDataContextManager().ManagesShipments);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";
			var manager = new CarrierShipmentDataContextManager();
			var dataObject = new UniversalDataBuss.DataObjects.Universal.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.CodesMappedToTarget = true;
			dataObject.DataContext.AddDataTarget(DataContextType.DummyBusinessObject, "CSH001");
			dataObject.BookingConfirmationReference = "CSH001";
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();
			AssertEquals(shipment, manager.LoadBusinessObjectFromDataTarget(dataObject, dataTarget, Factory.BOFactory, new DummyLogger()));
		}
		#endregion

		#region Implementation
		protected sealed override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<DataProvider Type=""EnterpriseID""></DataProvider>
				<Key>CSH001</Key>
				<Type>CarrierShipment</Type>
			</DataTarget>
		</DataTargetCollection>
      </DataContext>
    <BookingConfirmationReference>CSH001</BookingConfirmationReference>
  </Shipment>
</UniversalShipment>";
			}
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";
			Factory.SaveForTesting();
		}
		#endregion
	}
}

using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbCarrierBookingConsignmentDataContextManager))]
	class DtbCarrierBookingConsignmentDataContextManagerTest : ShipmentDataContextManagerTestCase<DtbCarrierBookingConsignmentDataContextManager, DtbCarrierBookingConsignment>
	{
		#region Context
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.CarrierBookingConsignment, new DtbCarrierBookingConsignmentDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			var consignment = Helper.CreateConsignment("LT0001");
			AssertEquals("LT0001", consignment.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion
		#region Shipments
		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbCarrierBookingConsignmentDataContextManager();
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			AssertEquals(typeof(DtbCarrierBookingConsignmentDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		public void TestManagesShipments()
		{
			AssertEquals(true, new DtbCarrierBookingConsignmentDataContextManager().ManagesShipments);
		}

		#endregion
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var manager = consignment.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("", manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion

		#region TestDefaultOutputDirectory
		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbCarrierBookingConsignmentDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#region Helper
		CarrierBookingTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new CarrierBookingTestHelper(Factory.BOFactory));
			}
		}

		CarrierBookingTestHelper helper;
		#endregion

		#region Implementation
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return System.Array.Empty<RecipientRoleType>();
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return "<UniversalShipment></UniversalShipment>";
			}
		}
		#endregion
	}
}

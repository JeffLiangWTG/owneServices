using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsTransitReceiveASNDataContextManager))]
	class WhsTransitReceiveASNDataContextManagerTest : ShipmentDataContextManagerTestCase<WhsTransitReceiveASNDataContextManager, WhsItemReceiveASN>
	{
		#region TestDataContextKey

		protected void TestDataContextKey()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			asn.WRP_ReferenceNumber = "REFNUM";
			AssertEquals("DataContextKey should be return WRP_ReferenceNumber.", "REFNUM", asn.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals(DataContextType.TransitReceiveASN, new WhsTransitReceiveASNDataContextManager().DataContextType);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new WhsTransitReceiveASNDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new WhsTransitReceiveASNDataContextManager().ManagesShipments);
		}

		#endregion

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => "";

		protected override bool ManagerChecksDataTargetToImport => false;
	}
}

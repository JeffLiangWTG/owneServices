using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.PackageState
{
	[TestedType(typeof(WhsTransitPackageStateDataContextManager))]
	public class WhsTransitPackageStateDataContextManagerTest : ShipmentDataContextManagerTestCase<WhsTransitPackageStateDataContextManager, WhsItemPackageState>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => "";

		#region TestDataContextKey

		protected void TestDataContextKey()
		{
			var packageState = Factory.BOFactory.New<WhsItemPackageState>();
			var pkg = Factory.BOFactory.New<PkgPackage>();
			var packageHeader = Factory.BOFactory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "PKG-1";
			pkg.KP_KPH_PackageHeader = packageHeader.PK;
			packageState.WPS_KP_Package = pkg.PK;

			AssertEquals("DataContextKey should be return WRP_ReferenceNumber.", "PKG-1", packageState.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals(DataContextType.TransitPackage, new WhsTransitPackageStateDataContextManager().DataContextType);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new WhsTransitPackageStateDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new WhsTransitPackageStateDataContextManager().ManagesShipments);
		}

		#endregion

		protected override bool ManagerChecksDataTargetToImport => false;

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("PackageState does not implement IJobNumber", true);
		}
	}
}

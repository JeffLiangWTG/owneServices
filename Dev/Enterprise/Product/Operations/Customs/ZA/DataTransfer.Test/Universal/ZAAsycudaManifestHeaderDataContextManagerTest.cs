using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ZAAsycudaManifestHeaderDataContextManager))]
	sealed class ZAAsycudaManifestHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<ZAAsycudaManifestHeaderDataContextManager, AsycudaManifestHeader>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();
		protected override bool ManagerChecksDataTargetToImport => true;
		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ZAOutTurn</Type>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";
	}
}

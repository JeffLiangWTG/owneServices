using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	[TestedType(typeof(USLVConsignmentDataContextManager))]
	public class USLVConsignmentDataContextManagerTest : ShipmentDataContextManagerTestCase<USLVConsignmentDataContextManager, CusUSLVConsignment>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override bool ManagerChecksDataTargetToImport
		{
			get { return true; }
		}

		protected override bool SaveAfterUseIncomingShipment => false;
	}
}


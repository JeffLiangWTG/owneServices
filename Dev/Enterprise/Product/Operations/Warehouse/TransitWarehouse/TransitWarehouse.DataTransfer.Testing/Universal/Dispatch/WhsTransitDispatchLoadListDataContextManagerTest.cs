using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsTransitDispatchLoadListDataContextManager))]
	class WhsTransitDispatchLoadListDataContextManagerTest : ShipmentDataContextManagerTestCase<WhsTransitDispatchLoadListDataContextManager, WhsItemDispatchLoadList>
	{
		#region ValidPopulatedUniversalShipmentXML

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => "";

		#endregion
	}
}

using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(StatementDataContextManager))]
	sealed class StatementDataContextManagerTestCase : UniversalDataBuss.Management.Testing.ShipmentDataContextManagerTestCase<StatementDataContextManager, CusStatementHeader>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => throw new System.NotImplementedException("ManagesShipments is false, this is not needed");
	}
}

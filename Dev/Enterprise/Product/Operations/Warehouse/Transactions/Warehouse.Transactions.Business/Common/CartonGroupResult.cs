using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CartonGroupResult
	{
		internal CartonGroupResult(ZGuid orgCartonGroupPK, bool productCartonGroupTakesPrecedence)
		{
			OrgCartonGroupPK = Argument.NotNull(orgCartonGroupPK, nameof(orgCartonGroupPK));
			ProductCartonGroupTakesPrecedence = productCartonGroupTakesPrecedence;
		}

		public void Deconstruct(out ZGuid orgCartonGroupPK, out bool productCartonGroupTakesPrecedence)
		{
			orgCartonGroupPK = OrgCartonGroupPK;
			productCartonGroupTakesPrecedence = ProductCartonGroupTakesPrecedence;
		}

		public ZGuid OrgCartonGroupPK { get; }
		public bool ProductCartonGroupTakesPrecedence { get; }
	}
}

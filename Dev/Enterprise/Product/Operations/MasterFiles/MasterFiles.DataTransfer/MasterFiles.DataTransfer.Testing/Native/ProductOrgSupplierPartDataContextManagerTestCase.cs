using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Native.Testing
{
	[TestedType(typeof(ProductDataContextManager))]
	internal class ProductOrgSupplierPartDataContextManagerTestCase : UniversalDataBuss.Management.Testing.ShipmentDataContextManagerTestCase<ProductDataContextManager, OrgSupplierPart>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { throw new System.NotImplementedException("ManagesShipments is false, this is not needed"); }
		}
	}
}

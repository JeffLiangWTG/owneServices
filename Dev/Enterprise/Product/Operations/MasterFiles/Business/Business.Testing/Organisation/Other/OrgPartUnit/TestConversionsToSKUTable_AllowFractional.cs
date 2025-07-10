namespace Enterprise.MasterFiles.Business.Testing
{
	public class TestConversionsToSKUTable_AllowFractional : TestConversionsToSKUTable
	{
		protected override bool AllowFractionalConversion => true;

		protected override ConversionsToSKUTable GetTable(OrgSupplierPart product) => new ConversionsToSKUTable(product, allowFractionalConversions: true);
	}
}

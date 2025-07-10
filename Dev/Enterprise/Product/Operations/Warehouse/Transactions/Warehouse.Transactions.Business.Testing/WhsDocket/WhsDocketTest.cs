using CargoWise.EntityFramework;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using WTG.RTUS.Interface;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketTest : WhsTestCaseWithFactory
	{
		#region TestUniversalCopyDocketDoesNotCopyWarehouse

		public void TestUniversalCopyDocketDoesNotCopyWarehouse()
		{
			var docketType = typeof(WhsDocket);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(docketType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, docketType,
				BusinessObjectCopyManager.CopyTreeConfiguration);
			var warehouseProperty =
				(RelatedEntityCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node =>
					node is RelatedEntityCopyTemplateNode &&
					((RelatedEntityCopyTemplateNode)node).RelatedEntityTableName == "WhsWarehouse");
			AssertNull(
				"Should not have warehouse type to copy (but still user can copy WD_WW_whs link to existing warehouse).",
				warehouseProperty);
		}

		#endregion

		#region TestWD_TransportReferenceMaxLengthIsMatchWithMaxTransportReferenceLength

		public void TestWD_TransportReferenceMaxLengthIsMatchWithMaxTransportReferenceLength()
		{
			AssertEquals("Please check if WD_TransportReference length has changed in the database and update MaxTrackingNumberLength accordingly.", RTUSConstants.MaxTransportReferenceLength, WhsDocketSchema.WD_TransportReference.MaxLength);
		}

		#endregion
	}
}

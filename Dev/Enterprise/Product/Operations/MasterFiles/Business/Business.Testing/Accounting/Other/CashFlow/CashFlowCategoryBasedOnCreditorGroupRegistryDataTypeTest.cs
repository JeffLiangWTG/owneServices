using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroupRegistryDataType))]
	sealed class CashFlowCategoryBasedOnCreditorGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CashFlowCategoryBasedOnCreditorGroupRegistryDataType>
	{
		#region Implementation

		protected override CashFlowCategoryBasedOnCreditorGroupRegistryDataType GetNewDataType()
		{
			return new CashFlowCategoryBasedOnCreditorGroupRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CashFlowCategoryBasedOnCreditorGroupRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CashFlowCategoryBasedOnCreditorGroupCollection collection = new CashFlowCategoryBasedOnCreditorGroupCollection();
			CashFlowCategoryBasedOnCreditorGroup configuration = collection.AddNew();
			var factory = new BusinessObjectFactory();
			var creditorGroup = factory.Load<OrgCreditorGroup>(new ZGuid("dfe48c5a-d0d3-4065-b352-466fff4b3959"));
			configuration.OrgGroupPK = creditorGroup.PK;
			configuration.CashFlowCategory = "O01";

			byte[] byteArrayValue = new byte[]
			{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,66,0,97,0,115,
0,101,0,100,0,79,0,110,0,67,0,114,0,101,0,100,0,105,0,116,0,111,0,114,0,71,0,114,0,111,0,117,0,112,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,
0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,
0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,
0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,
0,114,0,121,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,114,0,101,0,100,0,105,0,116,0,111,0,114,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,79,0,114,0,103,0,71,0,114,0,111,0,117,0,112,0,80,
0,75,0,62,0,100,0,102,0,101,0,52,0,56,0,99,0,53,0,97,0,45,0,100,0,48,0,100,0,51,0,45,0,52,0,48,0,54,0,53,0,45,0,98,0,51,0,53,0,50,0,45,0,52,0,54,0,54,0,102,0,102,0,102,0,52,
0,98,0,51,0,57,0,53,0,57,0,60,0,47,0,79,0,114,0,103,0,71,0,114,0,111,0,117,0,112,0,80,0,75,0,62,0,60,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,
0,114,0,121,0,62,0,79,0,48,0,49,0,60,0,47,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,62,0,60,0,47,0,67,0,97,0,115,0,104,0,70,0,108,
0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,114,0,101,0,100,0,105,0,116,0,111,0,114,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,47,
0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,114,0,101,
0,100,0,105,0,116,0,111,0,114,0,71,0,114,0,111,0,117,0,112,0,62,0
};
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}

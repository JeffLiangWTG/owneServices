using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerageBoxNumberRegistryDataType))]
	sealed class CusBrokerageBoxNumberRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CusBrokerageBoxNumberRegistryDataType>
	{
		protected override CusBrokerageBoxNumberRegistryDataType GetNewDataType() => new CusBrokerageBoxNumberRegistryDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CusBrokerageBoxNumberCollection();
			var element1 = collection1.AddNew();
			element1.BoxNumber = "600";
			element1.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			element1.IsDefaultBoxNumber = ZBool.True;
			var collection2 = new CusBrokerageBoxNumberCollection();
			var element2 = collection1.AddNew();
			element2.BoxNumber = "100";
			element2.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.B;
			element2.IsDefaultBoxNumber = ZBool.True;
			return new[] { new ValidSampleAndBinaryValueInDB(collection1, new CusBrokerageBoxNumberRegistryDataType().Serialise(collection1)), new ValidSampleAndBinaryValueInDB(collection2, new CusBrokerageBoxNumberRegistryDataType().Serialise(collection2)) };
		}

		protected override string ExpectedEditorName => "CusBrokerageBoxNumberRegistryItemEditor";
	}
}

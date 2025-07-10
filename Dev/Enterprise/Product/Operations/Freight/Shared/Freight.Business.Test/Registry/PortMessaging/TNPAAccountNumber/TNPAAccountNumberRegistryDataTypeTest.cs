using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TNPAAccountNumberRegistryDataType))]
	sealed class TNPAAccountNumberRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TNPAAccountNumberRegistryDataType>
	{
		protected override TNPAAccountNumberRegistryDataType GetNewDataType()
		{
			return new TNPAAccountNumberRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TNPAAccountNumberRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new TNPAAccountNumberCollection();
			var item = collection.AddNew();
			item.Port = "ZACPT";
			item.ImportNumber = "I001";
			item.ExportNumber = "E001";
			item.CoastwiseNumber = "C001";

			var collection2 = new TNPAAccountNumberCollection();
			var item2 = collection2.AddNew();
			item2.Port = "ZACPT";
			item2.ImportNumber = "I002";
			item2.ExportNumber = "E002";
			item2.CoastwiseNumber = "C002";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new TNPAAccountNumberRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new TNPAAccountNumberRegistryDataType().Serialise(collection2))
			};
		}
	}
}

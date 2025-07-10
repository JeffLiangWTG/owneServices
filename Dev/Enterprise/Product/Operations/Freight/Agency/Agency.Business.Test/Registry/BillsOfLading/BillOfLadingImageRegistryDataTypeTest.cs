using System.Drawing;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Agency.Business.BillOfLadingImageCollectionRegistryItem;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingImageRegistryDataType))]
	public class BillOfLadingImageRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillOfLadingImageRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingImageRegistryItemEditor";

		protected override BillOfLadingImageRegistryDataType GetNewDataType()
		{
			return new BillOfLadingImageRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new BillOfLadingImageCollection();
			var bi1 = collection.AddNew();
			bi1.PrincipalPK = principalPK1;
			bi1.Description = "Test1";
			bi1.Enabled = true;
			bi1.Image = new Bitmap(1, 1);

			var collection2 = new BillOfLadingImageCollection();
			var bi2 = collection2.AddNew();
			bi2.PrincipalPK = principalPK2;
			bi2.Description = "Test2";
			bi2.Enabled = true;
			bi2.Image = new Bitmap(1, 1);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new BillOfLadingImageRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new BillOfLadingImageRegistryDataType().Serialise(collection2))
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			principalPK1 = BillOfLadingImageCollectionRegistryItemTest.CreatePrincipal("Test1");
			principalPK2 = BillOfLadingImageCollectionRegistryItemTest.CreatePrincipal("Test2");
		}

		ZGuid principalPK1;
		ZGuid principalPK2;
	}
}

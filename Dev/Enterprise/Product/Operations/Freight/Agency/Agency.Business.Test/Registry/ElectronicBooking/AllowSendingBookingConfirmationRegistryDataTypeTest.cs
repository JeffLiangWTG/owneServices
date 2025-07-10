using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Agency.Business.AllowSendingBookingConfirmationCollectionRegistryItem;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllowSendingBookingConfirmationRegistryDataType))]
	public class AllowSendingBookingConfirmationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AllowSendingBookingConfirmationRegistryDataType>
	{
		protected override string ExpectedEditorName => "AllowSendingBookingConfirmationRegistryItemEditor";

		protected override AllowSendingBookingConfirmationRegistryDataType GetNewDataType()
		{
			return new AllowSendingBookingConfirmationRegistryDataType(AllowSendingBookingConfirmationCollection.NewWithDefaultValues());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new AllowSendingBookingConfirmationCollection();
			var item1 = collection.AddNew();
			item1.PrincipalPK = principalPK1;
			item1.Enabled = true;

			var collection2 = new AllowSendingBookingConfirmationCollection();
			var item2 = collection2.AddNew();
			item2.PrincipalPK = principalPK2;
			item2.Enabled = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new AllowSendingBookingConfirmationRegistryDataType(AllowSendingBookingConfirmationCollection.NewWithDefaultValues()).Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new AllowSendingBookingConfirmationRegistryDataType(AllowSendingBookingConfirmationCollection.NewWithDefaultValues()).Serialise(collection2))
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			principalPK1 = AllowSendingBookingConfirmationCollectionRegistryItemTest.CreatePrincipal("Test1");
			principalPK2 = AllowSendingBookingConfirmationCollectionRegistryItemTest.CreatePrincipal("Test2");
		}

		ZGuid principalPK1;
		ZGuid principalPK2;
	}
}

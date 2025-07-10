using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImporterAddressTypesListTest : TestCase
	{
		public void TestIsPhysicalRelatedMailingAddressType()
		{
			AssertEquals(true, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._06));
			AssertEquals(true, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._07));
			AssertEquals(true, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._08));
			AssertEquals(false, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._01));
			AssertEquals(false, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._02));
			AssertEquals(false, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._03));
			AssertEquals(false, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._04));
			AssertEquals(false, ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(ImporterAddressTypesList.Codes._05));
		}
	}
}

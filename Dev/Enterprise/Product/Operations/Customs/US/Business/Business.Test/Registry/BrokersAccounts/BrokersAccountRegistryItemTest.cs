using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BrokersAccountRegistryItem))]
	sealed class BrokersAccountRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<BrokersAccountCollection>
	{
		protected override StronglyTypedRegistryItem<BrokersAccountCollection, BrokersAccountCollection> GetNewRegistryItem() => new BrokersAccountRegistryItem("", null, null, null);

		protected override BrokersAccountCollection ValidValue
		{
			get
			{
				var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

				var collection = new BrokersAccountCollection();
				var account = collection.AddNew();
				account.BankAccount = bankAccount.PK;
				account.PayerUnitNumber = "123456";
				return collection;
			}
		}
	}

	[TestedType(typeof(BrokersAccountCollectionRegistryDataType))]
	sealed class BrokersAccountCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BrokersAccountCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName => "BrokerAccountsRegistryItemEditor";

		protected override BrokersAccountCollectionRegistryDataType GetNewDataType() => new BrokersAccountCollectionRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var first = GetValidSample(factory, new Guid("34F8AA4B-0926-4952-A9FE-932949B23BF4"), "QFK7JIVCRB", "123456", new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110,
				0, 115, 0, 58, 0, 120, 0, 115, 0, 105, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76,
				0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 45, 0, 105, 0, 110, 0, 115, 0, 116, 0, 97, 0, 110, 0, 99, 0, 101, 0, 34, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 100, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58,
				0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 34, 0, 62, 0, 60, 0, 77, 0, 97, 0, 110,
				0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 66, 0, 97, 0, 110, 0, 107, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 51, 0, 52, 0, 102, 0, 56, 0, 97, 0, 97, 0, 52, 0, 98,
				0, 45, 0, 48, 0, 57, 0, 50, 0, 54, 0, 45, 0, 52, 0, 57, 0, 53, 0, 50, 0, 45, 0, 97, 0, 57, 0, 102, 0, 101, 0, 45, 0, 57, 0, 51, 0, 50, 0, 57, 0, 52, 0, 57, 0, 98, 0, 50, 0, 51, 0, 98, 0, 102, 0, 52, 0, 60, 0, 47, 0, 66, 0, 97, 0, 110,
				0, 107, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 97, 0, 121, 0, 101, 0, 114, 0, 85, 0, 110, 0, 105, 0, 116, 0, 78, 0, 117, 0, 109, 0, 98, 0, 101, 0, 114, 0, 62, 0, 49, 0, 50, 0, 51, 0, 52, 0, 53, 0, 54, 0, 60,
				0, 47, 0, 80, 0, 97, 0, 121, 0, 101, 0, 114, 0, 85, 0, 110, 0, 105, 0, 116, 0, 78, 0, 117, 0, 109, 0, 98, 0, 101, 0, 114, 0, 62, 0, 60, 0, 67, 0, 108, 0, 105, 0, 101, 0, 110, 0, 116, 0, 66, 0, 114, 0, 97, 0, 110, 0, 99, 0, 104, 0, 68, 0, 101, 0, 115,
				0, 105, 0, 103, 0, 110, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 32, 0, 47, 0, 62, 0, 60, 0, 47, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 47, 0, 65, 0, 114, 0, 114,
				0, 97, 0, 121, 0, 79, 0, 102, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0
			});

			var second = GetValidSample(factory, new Guid("BA59D9E8-9A43-4271-9C2F-29FCD5DF88AB"), "QFK8JIVCRB", "654321", new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110,
				0, 115, 0, 58, 0, 120, 0, 115, 0, 105, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76,
				0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 45, 0, 105, 0, 110, 0, 115, 0, 116, 0, 97, 0, 110, 0, 99, 0, 101, 0, 34, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 100, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58,
				0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 34, 0, 62, 0, 60, 0, 77, 0, 97, 0, 110,
				0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 66, 0, 97, 0, 110, 0, 107, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 98, 0, 97, 0, 53, 0, 57, 0, 100, 0, 57, 0, 101, 0, 56,
				0, 45, 0, 57, 0, 97, 0, 52, 0, 51, 0, 45, 0, 52, 0, 50, 0, 55, 0, 49, 0, 45, 0, 57, 0, 99, 0, 50, 0, 102, 0, 45, 0, 50, 0, 57, 0, 102, 0, 99, 0, 100, 0, 53, 0, 100, 0, 102, 0, 56, 0, 56, 0, 97, 0, 98, 0, 60, 0, 47, 0, 66, 0, 97, 0, 110,
				0, 107, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 97, 0, 121, 0, 101, 0, 114, 0, 85, 0, 110, 0, 105, 0, 116, 0, 78, 0, 117, 0, 109, 0, 98, 0, 101, 0, 114, 0, 62, 0, 54, 0, 53, 0, 52, 0, 51, 0, 50, 0, 49, 0, 60,
				0, 47, 0, 80, 0, 97, 0, 121, 0, 101, 0, 114, 0, 85, 0, 110, 0, 105, 0, 116, 0, 78, 0, 117, 0, 109, 0, 98, 0, 101, 0, 114, 0, 62, 0, 60, 0, 67, 0, 108, 0, 105, 0, 101, 0, 110, 0, 116, 0, 66, 0, 114, 0, 97, 0, 110, 0, 99, 0, 104, 0, 68, 0, 101, 0, 115,
				0, 105, 0, 103, 0, 110, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 32, 0, 47, 0, 62, 0, 60, 0, 47, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0, 60, 0, 47, 0, 65, 0, 114, 0, 114,
				0, 97, 0, 121, 0, 79, 0, 102, 0, 77, 0, 97, 0, 110, 0, 97, 0, 103, 0, 101, 0, 100, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 62, 0
			});

			return new[] { first, second };
		}

		ValidSampleAndBinaryValueInDB GetValidSample(BusinessObjectFactory factory, Guid bankAccountPk, string bankAccountCode, string payerUnitNumber, byte[] binaryValue)
		{
			var acc = GetOrNew<AccBankAccount>(factory, bankAccountPk, bankAccount => bankAccount.AB_Code = bankAccountCode);
			factory.Save();
			var collection = new BrokersAccountCollection();

			var account = collection.AddNew();
			account.BankAccount = acc.PK.ToGuid();
			account.PayerUnitNumber = payerUnitNumber;

			return new ValidSampleAndBinaryValueInDB(collection, binaryValue);
		}
	}
}

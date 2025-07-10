using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class AccountingCodeDescriptionWithGroupRegistryItemTest<T, T2> : CodeDescriptionWithGroupRegistryItemTest
		where T : CodeDescriptionWithGroupCollection
		where T2 : CodeDescriptionWithGroup
	{
		protected abstract T GetDefaultValue();

		protected abstract Func<CodeDescriptionWithGroupCollection, T> ValueConveter { get; }

		public new void TestConstructor()
		{
			var dummyConveterCounter = 0;
			var defualtValue = GetDefaultValue();

			var item = new AccountingCodeDescriptionWithGroupRegistryItem<T, T2>(
				"DummyRegistryName",
				(NoResString)"DummyCategory",
				(NoResString)"DummyCaption",
				(NoResString)"DummyHint",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"DummyGroupName"),
				defualtValue,
				DummyConveter
			);
			AssertEquals(0, dummyConveterCounter);

			var registryValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) as T;
			AssertNotNull(registryValue);
			AssertEquals(defualtValue.Count, registryValue.Count);
			AssertEquals(1, dummyConveterCounter);

			foreach (T2 itemInDefaultValue in defualtValue)
			{
				var itemInRegistry = registryValue.FindByCode(itemInDefaultValue.Code) as CodeDescriptionWithGroup;
				AssertNotNull(itemInRegistry);
				AssertEquals(itemInDefaultValue.Code, itemInRegistry.Code);
				AssertEquals(itemInDefaultValue.Description, itemInRegistry.Description);
				AssertEquals(itemInDefaultValue.Group, itemInRegistry.Group);
			}

			T DummyConveter(CodeDescriptionWithGroupCollection inputValue)
			{
				dummyConveterCounter++;
				return ValueConveter(inputValue);
			}
		}

		public void TestConstructor_NullConverter()
		{
			var exp = AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new AccountingCodeDescriptionWithGroupRegistryItem<T, T2>(
					"DummyRegistryName",
					(NoResString)"DummyCategory",
					(NoResString)"DummyCaption",
					(NoResString)"DummyHint",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"DummyGroupName"),
					GetDefaultValue(),
					null
				);
			});
#if NETFRAMEWORK
			AssertEquals("Value cannot be null.\r\nParameter name: valueConverter", exp.Message);
#else
			AssertEquals("Value cannot be null. (Parameter 'valueConverter')", exp.Message);
#endif
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionWithGroupCollection, CodeDescriptionWithGroupCollection> GetNewRegistryItem()
		{
			return new AccountingCodeDescriptionWithGroupRegistryItem<T, T2>(
				"DummyRegistryName",
				(NoResString)"DummyCategory",
				(NoResString)"DummyCaption",
				(NoResString)"DummyHint",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"DummyGroupName"),
				GetDefaultValue(),
				ValueConveter
			);
		}
	}

	abstract class AccountingCodeDescriptionWithGroupRegistryDataTypeTest<T> : NonPersistentBusinessObjectRegistryDataTypeTestCase<AccountingCodeDescriptionWithGroupRegistryDataType<T>>
		where T : CodeDescriptionWithGroupCollection
	{
		protected abstract IEnumerable<(T Collection, byte[] ByteArray)> CreateValidSample();

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo.
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override AccountingCodeDescriptionWithGroupRegistryDataType<T> GetNewDataType()
		{
			return new AccountingCodeDescriptionWithGroupRegistryDataType<T>();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return CreateValidSample()
				.Select(x => new ValidSampleAndBinaryValueInDB(x.Collection, x.ByteArray))
				.ToArray();
		}
	}
}

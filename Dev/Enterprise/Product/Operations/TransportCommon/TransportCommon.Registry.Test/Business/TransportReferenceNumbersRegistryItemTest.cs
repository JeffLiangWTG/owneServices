using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(TransportReferenceNumberTypesRegistryItem))]
	class TransportReferenceNumbersRegistryItemTest : StronglyTypedRegistryItemTestCase<TransportReferenceNumberTypeCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<TransportReferenceNumberTypeCollection, TransportReferenceNumberTypeCollection> GetNewRegistryItem()
		{
			return new TransportReferenceNumberTypesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new TransportReferenceNumberTypeCollection());
		}

		#endregion

		#region TestRegistryOptionsConstructor

		public void TestRegistryOptionsConstructor()
		{
			var registryItem = new TransportReferenceNumberTypesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new TransportReferenceNumberTypeCollection(), RegistryOptions.IsHidden);
			Assert(registryItem.HasOption(RegistryOptions.IsHidden));
		}

		#endregion

		#region TestAddsMissingDefaults

		public void TestAddsMissingDefaults()
		{
			var list = new TransportReferenceNumberTypeCollection();
			list.Add("AAA", (NoResString)"A Desc");
			list.Add("BBB", (NoResString)"B Desc");

			var defaults = new TransportReferenceNumberTypeCollection();
			defaults.AddSystemDefined("AAA", (NoResString)"A Desc", isUnique: true);
			defaults.AddSystemDefined("CCC", (NoResString)"C Desc", isUnique: true);

			var item = new TransportReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertContainsExactElementsInAnyOrder("Should have added the missing default 'CCC' type when calling GetValueWithoutFallback",
				new[] { "AAA", "BBB", "CCC" },
				item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Cast<TransportReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));

			AssertContainsExactElementsInAnyOrder("Should have added the missing default 'CCC' type when accessing Value",
				new[] { "AAA", "BBB", "CCC" },
				item.Value
				.Cast<TransportReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));
		}

		public void TestAddsMissingDefaults_OnlyAddsIfSystemDefined()
		{
			// Clients are free to edit/modify non system defined types as per old functionality
			var list = new TransportReferenceNumberTypeCollection();
			list.Add("AAA", (NoResString)"A Desc");
			list.Add("BBB", (NoResString)"B Desc");

			var defaults = new TransportReferenceNumberTypeCollection();
			defaults.Add("AAA", (NoResString)"A Desc", isUnique: true);
			defaults.Add("CCC", (NoResString)"C Desc", isUnique: true);

			var item = new TransportReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertContainsExactElementsInAnyOrder("Should not have added the missing default 'CCC' type when calling GetValueWithoutFallback",
				new[] { "AAA", "BBB" },
				item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Cast<TransportReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));

			AssertContainsExactElementsInAnyOrder("Should not have added the missing default 'CCC' type when accessing Value",
				new[] { "AAA", "BBB" },
				item.Value
				.Cast<TransportReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));
		}

		#endregion

		#region TestAddsMissingDefaults_SetsSystemDefined

		public void TestAddsMissingDefaults_SetsSystemDefined()
		{
			var defaults = new TransportReferenceNumberTypeCollection();
			defaults.AddSystemDefined("AAA", (NoResString)"A Desc");
			defaults.AddSystemDefined("BBB", (NoResString)"B Desc");

			var deserialisedList = new TransportReferenceNumberTypeCollection();
			deserialisedList.Add("AAA", (NoResString)"A Desc22", isUnique: true); // SystemDefined is not serialised
			deserialisedList.Add("BBB", (NoResString)"B Desc22", isUnique: false); // SystemDefined is not serialised

			var item = new TransportReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deserialisedList);

			var aaa = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<TransportReferenceNumberType>().Single(c => c.Code == "AAA");
			var bbb = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<TransportReferenceNumberType>().Single(c => c.Code == "BBB");
			CombineAssertions("When using GetValueWithoutFallback", () =>
			{
				AssertEquals("Description should not be same as system defined.", "A Desc22", aaa.Description);
				AssertEquals("Description should not be same as system defined.", "B Desc22", bbb.Description);
				AssertEquals("IsUnique should be same as deserialisedList.", true, aaa.IsUnique);
				AssertEquals("IsUnique should not be same as system defined.", false, bbb.IsUnique);
				AssertEquals("Should set system defined.", true, aaa.SystemDefined);
				AssertEquals("Should set system defined.", true, bbb.SystemDefined);
			});

			aaa = item.Value.Cast<TransportReferenceNumberType>().Single(c => c.Code == "AAA");
			bbb = item.Value.Cast<TransportReferenceNumberType>().Single(c => c.Code == "BBB");
			CombineAssertions("When using Value", () =>
			{
				AssertEquals("Description should not be same as system defined.", "A Desc22", aaa.Description);
				AssertEquals("Description should not be same as system defined.", "B Desc22", bbb.Description);
				AssertEquals("IsUnique should be same as deserialisedList.", true, aaa.IsUnique);
				AssertEquals("IsUnique should not be same as system defined.", false, bbb.IsUnique);
				AssertEquals("Should set system defined.", true, aaa.SystemDefined);
				AssertEquals("Should set system defined.", true, bbb.SystemDefined);
			});
		}

		#endregion
	}
}
